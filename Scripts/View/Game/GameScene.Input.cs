using System;
using System.Linq;
using Godot;

/// <summary>Keys, touch and mouse, the test pilot, and the card picker and the dialogue they drive.</summary>
public partial class GameScene : Node2D
{
    // MARK: Autoplay (test pilot)

    private float _autoNextBomb = 12;
    private PlayerInputComponent AutoInput()
    {
        float t = (float)_clock;
        var p = new PlayerInputComponent { IsShooting = true };
        float x = World.Player.Transform.Position.X;
        float target = 300 + 220 * MathF.Sin(t * 0.7f);
        p.Direction = new Vector2(Math.Clamp((target - x) / 40, -1, 1), Math.Clamp((620 - World.Player.Transform.Position.Y) / 60, -1, 1));
        if (t >= _autoNextBomb) { p.IsBombing = true; if (t >= _autoNextBomb + 0.1f) _autoNextBomb += 25; }
        return p;
    }

    // MARK: Cards

    private static bool KeyDown(Key key) => Godot.Input.IsKeyPressed(key);

    private bool JustPressed(params Key[] keys) => keys.Any(k => KeyDown(k) && !_heldKeys.Contains(k));

    private static readonly Key[] PickerKeys = { Key.Left, Key.Right, Key.A, Key.D, Key.Z, Key.Enter, Key.KpEnter, Key.Space, Key.Key1, Key.Key2, Key.Key3 };

    /// <summary>Shows the card choice while the world waits for one, and reads the keys that pick.</summary>
    private void UpdatePicker(float dt)
    {
        var draft = World.Draft;
        if (draft != null)
        {
            if (!_picker.IsShown)
            {
                _input.EndAll();
                _picker.Show(draft, World.Run.Owned);
                _pickerLock = 0.8f;
            }
            _pickerLock = Math.Max(0, _pickerLock - dt);
            if (_pickerLock == 0)
            {
                if (Autoplay) { Take(0); return; }
                if (JustPressed(Key.Left, Key.A)) _picker.Move(-1);
                if (JustPressed(Key.Right, Key.D)) _picker.Move(1);
                if (JustPressed(Key.Z, Key.Enter, Key.KpEnter, Key.Space)) Take(_picker.Selected);
                else
                {
                    var numbers = new[] { Key.Key1, Key.Key2, Key.Key3 };
                    for (int i = 0; i < 3; i++) if (JustPressed(numbers[i])) { Take(i); break; }
                }
            }
        }
        else if (_picker.IsShown)
        {
            _picker.Hide();
        }
        _heldKeys = PickerKeys.Where(KeyDown).ToHashSet();
    }

    private static readonly Key[] DialogueKeys = { Key.Z, Key.Enter, Key.KpEnter, Key.Space, Key.X };

    /// <summary>Z / Enter / Space move the conversation on; X skips it. (The autoplay test pilot skips it at once.)</summary>
    private void UpdateDialogueKeys()
    {
        if (Autoplay) { _dialogue.Skip(true); return; }
        bool Fresh(params Key[] keys) => keys.Any(k => KeyDown(k) && !_dialogueHeld.Contains(k));
        if (Fresh(Key.X)) _dialogue.Skip();
        else if (Fresh(Key.Z, Key.Enter, Key.KpEnter, Key.Space)) _dialogue.Advance();
        _dialogueHeld = DialogueKeys.Where(KeyDown).ToHashSet();
    }

    private void Take(int index)
    {
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuSelect);
        World.Choose(index);
        _picker.Hide();
    }

    // MARK: Touch / mouse

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
        {
            if (key.Keycode == Key.Escape && World.Result == null && !_exited)
            {
                SfxPlayer.Instance?.Play(_pause.IsShown ? SfxBank.Sound.MenuBack : SfxBank.Sound.MenuSelect);
                if (_pause.IsShown) Resume(); else Pause();
                GetViewport().SetInputAsHandled();
                return;
            }
            if (_pause.IsShown)
            {
                _pause.HandleKey(key.Keycode);
                GetViewport().SetInputAsHandled();
                return;
            }
        }
        if (_pause.IsShown)
        {
            if (@event is InputEventMouseButton click && click.ButtonIndex == MouseButton.Left && click.Pressed) _pause.Click((click.Position - Main.StageOffset));
            else if (@event is InputEventScreenTouch tap && tap.Pressed) _pause.Click((tap.Position - Main.StageOffset));
            return;
        }
        switch (@event)
        {
            case InputEventScreenTouch t:
                if (t.Pressed) TouchBegan(t.Index, (t.Position - Main.StageOffset)); else _input.End(t.Index);
                break;
            case InputEventScreenDrag d:
                _input.MoveTouch(d.Index, (d.Position - Main.StageOffset));
                break;
            case InputEventMouseButton m when m.ButtonIndex == MouseButton.Left:
                if (m.Pressed) TouchBegan(-1, (m.Position - Main.StageOffset)); else _input.End(-1);
                break;
            case InputEventMouseMotion mm:
                _input.MoveTouch(-1, (mm.Position - Main.StageOffset));
                break;
        }
    }

    private void TouchBegan(int id, Vector2 p)
    {
        if (_dialogue.IsActive)
        {
            _dialogue.Advance();
            return;
        }
        if (_picker.IsShown)
        {
            if (_pickerLock == 0 && _picker.CardIndexAt(p) is int index) Take(index);
            return;
        }
        var control = _hud.ControlAt(p);
        if (_hud.IsEndVisible)
        {
            if (control == GameHud.Control.Restart) Restart();
            else if (control == GameHud.Control.Menu) { SfxPlayer.Instance?.Play(SfxBank.Sound.MenuBack); DoExit(); }
            return;
        }
        switch (control)
        {
            default: if (p.X < GameHud.PanelX) _input.BeginMovement(id, p); break;
        }
    }
}
