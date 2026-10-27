using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A conversation in the style of Touhou's pre-stage chatter: a wooden text box along the bottom of the river with the speaker's
/// name on a plaque (at the left for Atli, at the right for Airu). There are no portraits for now. Text is typed out;
/// Advance finishes the line, or moves to the next one; Skip ends the whole conversation.
/// Place at the scene origin (like the HUD); it covers the river and leaves the side panel visible. ZIndex 140 (between the HUD
/// and the card picker).
/// </summary>
public partial class DialogueBox : Node2D
{
    public event Action Finished;

    private const float Art = HudArt.Scale;
    private const int LineWidth = 36;       // letters per line
    private const int MaxLines = 3;
    private const float TypeSpeed = 48;     // letters per second
    private const float InputLock = 0.45f;  // seconds after starting during which input is ignored

    private static readonly float BoxX = 19, BoxY = 592, BoxW = 720, BoxH = 168;

    private readonly Sprite2D _plaque = new() { Centered = false };
    private readonly PixelLabel _name = new(PixelLabel.Style.Gold, 3, PixelLabel.Align.Center);
    private readonly PixelLabel[] _lines = new PixelLabel[MaxLines];
    private readonly PixelLabel _hint = new(PixelLabel.Style.Dim, 2, PixelLabel.Align.Right);
    private readonly Sprite2D _next = new() { Centered = false };

    private IReadOnlyList<DialogueLine> _script;
    private int _index;
    private List<string> _wrapped = new();
    private float _typed;
    private int _total;
    private float _lock;
    private float _clock;
    private float _shake;
    private const float ShakeTime = 0.6f, ShakeAmount = 7;

    private enum Phase { Hidden, In, Open, Out }
    private Phase _phase = Phase.Hidden;
    private float _fade;
    private const float FadeInTime = 0.4f, FadeOutTime = 0.3f;

    /// <summary>A conversation is going on (the box is arriving or open), so keys and clicks move it along.</summary>
    public bool IsActive => _phase == Phase.In || _phase == Phase.Open;
    /// <summary>The box is on the screen, even if it is only fading away.</summary>
    public bool IsOnScreen => Visible;
    public int Index => _index;

    public DialogueBox()
    {
        ZIndex = 140;
        Visible = false;
    }

    public override void _Ready()
    {
        AddChild(new ColorRect
        {
            Color = new Color(0.02f, 0.03f, 0.06f, 0.35f), Size = new Vector2(Layout.PanelX, Layout.Height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        });
        AddChild(new Sprite2D
        {
            Texture = HudArt.Box((int)(BoxW / Art), (int)(BoxH / Art)).ToTexture(), Centered = false,
            Scale = new Vector2(Art, Art), Position = new Vector2(BoxX, BoxY), ZIndex = 5,
        });
        _plaque.Texture = HudArt.Plaque(34, 9).ToTexture();
        _plaque.Scale = new Vector2(Art, Art);
        _plaque.ZIndex = 6;
        AddChild(_plaque);
        _name.ZIndex = 7;
        AddChild(_name);
        for (int i = 0; i < MaxLines; i++)
        {
            _lines[i] = new PixelLabel(PixelLabel.Style.Value, 3, PixelLabel.Align.Left) { ZIndex = 7 };
            _lines[i].Position = new Vector2(BoxX + 30, BoxY + 30 + i * 30);
            AddChild(_lines[i]);
        }
        _hint.Position = new Vector2(BoxX + BoxW - 20, BoxY + BoxH - 28);
        _hint.ZIndex = 7;
        _hint.SetText(UIText.DialogueHint);
        AddChild(_hint);
    }

    public void Start(IReadOnlyList<DialogueLine> script)
    {
        _script = script;
        _index = 0;
        _lock = InputLock;
        _clock = 0;
        Visible = true;
        _phase = Phase.In;
        _fade = 0;
        ApplyFade();
        Show(_index);
    }

    private void Show(int index)
    {
        var line = _script[index];
        bool left = line.Speaker == DialogueSpeaker.Atli;

        string name = Dialogues.NameOf(line.Speaker);
        _name.SetText(name);
        float plaqueW = 34 * Art;
        // Atli's name at the left, Airu's at the right, the boss's in the middle.
        float px = line.Speaker == DialogueSpeaker.Atli ? BoxX + 28 : (line.Speaker == DialogueSpeaker.Airu ? BoxX + BoxW - 28 - plaqueW : BoxX + (BoxW - plaqueW) / 2);
        _plaque.Position = new Vector2(px, BoxY - 24);
        _name.Position = new Vector2(px + plaqueW / 2, BoxY - 24 + 8);

        _shake = line.Shout ? ShakeTime : 0;
        if (line.Shout) SfxPlayer.Instance?.Play(SfxBank.Sound.HitBoss);
        _wrapped = GameHud.Wrap(line.Text, LineWidth, MaxLines);
        _total = _wrapped.Sum(l => l.Length);
        _typed = 0;
        ShowTyped();
    }

    private void ShowTyped()
    {
        int left = (int)_typed;
        for (int i = 0; i < MaxLines; i++)
        {
            string text = i < _wrapped.Count ? _wrapped[i] : "";
            int take = Math.Min(text.Length, Math.Max(0, left));
            left -= text.Length;
            _lines[i].SetText(text.Substring(0, take));
        }
    }

    public override void _Process(double delta)
    {
        if (!Visible) return;
        float dt = (float)Math.Min(delta, 0.1);
        if (_phase == Phase.In)
        {
            _fade = Math.Min(1, _fade + dt / FadeInTime);
            ApplyFade();
            if (_fade >= 1) _phase = Phase.Open;
        }
        else if (_phase == Phase.Out)
        {
            _fade = Math.Max(0, _fade - dt / FadeOutTime);
            ApplyFade();
            if (_fade <= 0)
            {
                Visible = false;
                _phase = Phase.Hidden;
                Finished?.Invoke();
            }
            return;
        }
        _clock += dt;
        _lock = Math.Max(0, _lock - dt);
        if (_shake > 0) { _shake = Math.Max(0, _shake - dt); ApplyFade(); }
        if (_typed < _total)
        {
            _typed = Math.Min(_total, _typed + TypeSpeed * dt);
            ShowTyped();
        }
    }

    /// <summary>Finishes typing the line; if it is already complete, moves on to the next (and ends after the last).</summary>
    public void Advance(bool force = false)
    {
        if (!IsActive || (_lock > 0 && !force)) return;
        SfxPlayer.Instance?.Play(SfxBank.Sound.DialogueNext);
        if (_typed < _total)
        {
            _typed = _total;
            ShowTyped();
            return;
        }
        _index++;
        if (_index >= _script.Count) End();
        else Show(_index);
    }

    /// <summary>Ends the whole conversation.</summary>
    public void Skip(bool force = false)
    {
        if (!IsActive || (_lock > 0 && !force)) return;
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuBack);
        End();
    }

    /// <summary>The box glides up as it fades in and back down as it fades out.</summary>
    private void ApplyFade()
    {
        float eased = 1 - (1 - _fade) * (1 - _fade);
        Modulate = new Color(1, 1, 1, eased);
        // A shouted line shakes the box, less and less.
        var jolt = _shake > 0
            ? new Vector2(((_clock * 97) % 2 < 1 ? -1 : 1) * ShakeAmount * _shake / ShakeTime, ((_clock * 61) % 2 < 1 ? -1 : 1) * ShakeAmount * 0.5f * _shake / ShakeTime)
            : Vector2.Zero;
        Position = new Vector2(0, (1 - eased) * 36) + jolt;
    }

    private void End()
    {
        // The box fades away first; the conversation is over (Finished) when it has gone.
        _phase = Phase.Out;
    }

    /// <summary>Drops the conversation at once (the run is restarted or left).</summary>
    public void Cancel()
    {
        Visible = false;
        _phase = Phase.Hidden;
        _fade = 0;
    }
}
