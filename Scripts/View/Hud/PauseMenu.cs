using Godot;
using System;

/// <summary>
/// The little menu that opens with Esc during a run: Continue or Give Up (back to the title menu). Same wooden box as the
/// victory / defeat panel. Lives at the origin of the scene; ZIndex 200 puts it over the HUD and the card picker.
/// Keys (Left/Right/A/D, Enter/Z/Space, Esc) and clicks are forwarded by GameScene through HandleKey and Click.
/// </summary>
public partial class PauseMenu : Node2D
{
    public event Action Continue;
    public event Action GiveUp;

    private const float Art = HudArt.Scale;
    private static readonly Vector2 Centre = new Vector2(Layout.FieldInset + Layout.PlayWidth / 2, 288);
    private const float PlaqueY = 156, PlaqueX = 98;
    private const int PlaqueWidth = 44;

    private readonly Node2D _panel = new();
    private readonly Sprite2D[] _plaques = new Sprite2D[2];
    private readonly PixelLabel[] _captions = new PixelLabel[2];
    private int _selected;

    public bool IsShown => Visible;
    public int Selected => _selected;

    public PauseMenu()
    {
        ZIndex = 200;
        Visible = false;
    }

    public override void _Ready()
    {
        // Dim the river behind the box.
        AddChild(new ColorRect
        {
            Color = new Color(0.02f, 0.03f, 0.06f, 0.55f),
            Size = new Vector2(Layout.PanelX, Layout.Height),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        });

        _panel.Position = Centre;
        AddChild(_panel);
        _panel.AddChild(new Sprite2D
        {
            Texture = HudArt.Box(100, 50).ToTexture(), Centered = false, Scale = new Vector2(Art, Art),
            Position = new Vector2(-50 * Art, 0),
        });

        AddLabel(PixelLabel.Style.Gold, 5, UIText.Paused, 0, 24);
        AddLabel(PixelLabel.Style.Label, 3, UIText.PauseHint, 0, 84);

        string[] names = { UIText.Continue, UIText.GiveUp };
        for (int i = 0; i < 2; i++)
        {
            float x = i == 0 ? -PlaqueX : PlaqueX;
            var plaque = new Sprite2D
            {
                Texture = HudArt.Plaque(PlaqueWidth, 9).ToTexture(), Centered = false, Scale = new Vector2(Art, Art),
                Position = new Vector2(x - PlaqueWidth * Art / 2, PlaqueY), ZIndex = 10,
            };
            _panel.AddChild(plaque);
            _plaques[i] = plaque;
            _captions[i] = AddLabel(PixelLabel.Style.Value, 3, names[i], x, PlaqueY + 8, 20);
        }
        Refresh();
    }

    private PixelLabel AddLabel(PixelLabel.Style style, float scale, string text, float x, float top, int z = 10)
    {
        var l = new PixelLabel(style, scale, PixelLabel.Align.Center) { Position = new Vector2(x, top), ZIndex = z };
        l.SetText(text);
        _panel.AddChild(l);
        return l;
    }

    private void Refresh()
    {
        for (int i = 0; i < 2; i++)
        {
            bool on = i == _selected;
            _plaques[i].Modulate = on ? new Color(1.35f, 1.2f, 1f) : new Color(0.62f, 0.62f, 0.62f);
            _captions[i].Modulate = on ? Colors.White : new Color(0.75f, 0.75f, 0.75f);
        }
    }

    public void Show()
    {
        _selected = 0;
        if (IsInsideTree()) Refresh();
        Visible = true;
    }

    public new void Hide() => Visible = false;

    public void Move(int step)
    {
        _selected = (_selected + step + 2) % 2;
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuMove);
        Refresh();
    }

    public void Activate()
    {
        SfxPlayer.Instance?.Play(_selected == 0 ? SfxBank.Sound.MenuBack : SfxBank.Sound.MenuSelect);
        if (_selected == 0) Continue?.Invoke(); else GiveUp?.Invoke();
    }

    /// <summary>Which plaque (0 = Continue, 1 = Give Up) is under a screen point.</summary>
    public int? ChoiceAt(Vector2 p)
    {
        for (int i = 0; i < 2; i++)
        {
            var size = new Vector2(PlaqueWidth, 9) * Art;
            var rect = new Rect2(Centre + _plaques[i].Position, size);
            if (rect.HasPoint(p)) return i;
        }
        return null;
    }

    public void Click(Vector2 p)
    {
        if (ChoiceAt(p) is int i)
        {
            _selected = i;
            Refresh();
            Activate();
        }
    }

    /// <summary>Keys while the menu is open (Esc is handled by the owner: it closes the menu).</summary>
    public void HandleKey(Key key)
    {
        switch (key)
        {
            case Key.Left: case Key.A: Move(-1); break;
            case Key.Right: case Key.D: Move(1); break;
            case Key.Enter: case Key.KpEnter: case Key.Z: case Key.Space: Activate(); break;
        }
    }
}
