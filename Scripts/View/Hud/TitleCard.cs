using Godot;
using System;

/// <summary>
/// The title that opens a part of the game, like Touhou's: a big name (STAGE 1, or the boss's name) between two thin golden lines,
/// and the name of the music that is starting underneath. It fades in, stays a moment and fades out. Place at the scene origin;
/// ZIndex 120.
/// </summary>
public partial class TitleCard : Node2D
{
    private const float FadeIn = 0.45f, FadeOut = 0.7f;

    private readonly PixelLabel _gold = new(PixelLabel.Style.Gold, 7, PixelLabel.Align.Center);
    private readonly PixelLabel _fire = new(PixelLabel.Style.Title, 7, PixelLabel.Align.Center);
    private readonly PixelLabel _music = new(PixelLabel.Style.Label, 3, PixelLabel.Align.Center);
    private readonly PixelLabel _easy = new(PixelLabel.Style.Label, 3, PixelLabel.Align.Center);
    private readonly PixelLabel _normal = new(PixelLabel.Style.Gold, 3, PixelLabel.Align.Center);
    private readonly PixelLabel _hard = new(PixelLabel.Style.Defeat, 3, PixelLabel.Align.Center);
    private readonly ColorRect _upper = new() { Color = new Color(0.95f, 0.76f, 0.3f, 0.9f), MouseFilter = Control.MouseFilterEnum.Ignore };
    private readonly ColorRect _lower = new() { Color = new Color(0.95f, 0.76f, 0.3f, 0.9f), MouseFilter = Control.MouseFilterEnum.Ignore };
    private float _clock = -1, _hold;
    private PixelLabel _current;

    public bool IsShowing => _clock >= 0;

    public TitleCard()
    {
        ZIndex = 120;
        Visible = false;
    }

    public override void _Ready()
    {
        float cx = Layout.PanelX / 2;
        foreach (var label in new[] { _gold, _fire })
        {
            label.Position = new Vector2(cx, 310);
            AddChild(label);
        }
        _music.Position = new Vector2(cx, 392);
        AddChild(_music);
        foreach (var tag in new[] { _easy, _normal, _hard })
        {
            tag.Position = new Vector2(cx, 258);
            AddChild(tag);
        }
        foreach (var line in new[] { _upper, _lower })
        {
            line.Size = new Vector2(440, 3);
            AddChild(line);
        }
        _upper.Position = new Vector2(cx - 220, 292);
        _lower.Position = new Vector2(cx - 220, 432);
    }

    /// <summary>Shows a title and the name of the music under it. The boss's name is drawn in fire.</summary>
    public void Show(string title, string music, bool fire = false, float hold = 2.6f, Difficulty? difficulty = null)
    {
        _easy.Visible = _normal.Visible = _hard.Visible = false;
        if (difficulty is Difficulty level)
        {
            var tag = level == Difficulty.Easy ? _easy : (level == Difficulty.Hard ? _hard : _normal);
            tag.SetText(level.Title());
            tag.Visible = true;
        }
        _current = fire ? _fire : _gold;
        _gold.Visible = false; _fire.Visible = false;
        _current.SetText(title);
        _current.Visible = true;
        _music.SetText(music);
        _clock = 0;
        _hold = hold;
        Visible = true;
        Modulate = new Color(1, 1, 1, 0);
    }

    public void Hide2()
    {
        _clock = -1;
        Visible = false;
    }

    public override void _Process(double delta)
    {
        if (_clock < 0) return;
        _clock += (float)Math.Min(delta, 0.1);
        float alpha;
        if (_clock < FadeIn) alpha = _clock / FadeIn;
        else if (_clock < FadeIn + _hold) alpha = 1;
        else alpha = 1 - (_clock - FadeIn - _hold) / FadeOut;
        if (alpha <= 0 && _clock > FadeIn + _hold) { Hide2(); return; }
        Modulate = new Color(1, 1, 1, Mathf.Clamp(alpha, 0, 1));
        // the lines open out from the middle as the title appears
        float open = Mathf.Clamp(_clock / 0.6f, 0, 1);
        float width = 440 * open;
        float cx = Layout.PanelX / 2;
        _upper.Size = new Vector2(width, 3); _lower.Size = new Vector2(width, 3);
        _upper.Position = new Vector2(cx - width / 2, 292); _lower.Position = new Vector2(cx - width / 2, 432);
    }
}
