using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// The end of the story, told as in Touhou: over a picture (a dawn on the river, for now drawn in code), a few pages of text, each typed out; Z (or a click) finishes the page and
/// turns it, X skips the rest. It fades in from black and, after the last page, back to black, and then <see cref="Finished"/> is raised. Covers the whole picture (ZIndex 148).
/// </summary>
public partial class EpilogueScreen : Node2D
{
    public event Action Finished;

    private const int LineWidth = 52, MaxLines = 5;
    private const float TypeSpeed = 40, FadeTime = 1.2f, InputLock = 0.6f;
    private const int PictureWidth = 222, PictureHeight = 155;

    private readonly List<string> _pages;
    private readonly PixelLabel[] _lines = new PixelLabel[MaxLines];
    private readonly ColorRect _cover = new() { Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Ignore };
    private readonly PixelLabel _hint = new(PixelLabel.Style.Dim, 2, PixelLabel.Align.Right);
    private int _page;
    private List<string> _wrapped = new();
    private float _typed, _clock, _cover01 = 1, _coverTarget;
    private int _total;
    private bool _leaving;

    private readonly string _titleKey;
    private readonly Func<PixelCanvas> _picture;
    /// <summary>The screen reads the keys and the clicks itself (the opening of the game, where there is no scene to do it); in the game the scene does.</summary>
    public bool HandlesOwnInput;

    /// <summary>The pages of the story (keys of the texts), its title (a key; null: the end of the story) and its picture (null: the dawn).</summary>
    public EpilogueScreen(IEnumerable<string> pageKeys, string titleKey = null, Func<PixelCanvas> picture = null)
    {
        _pages = pageKeys.Select(Loc.Get).ToList();
        _titleKey = titleKey;
        _picture = picture;
        ZIndex = 148;
    }

    public override void _Input(InputEvent @event)
    {
        if (!HandlesOwnInput) return;
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            if (key.Keycode is Key.Z or Key.Enter or Key.KpEnter or Key.Space) { Advance(); GetViewport().SetInputAsHandled(); }
            else if (key.Keycode is Key.X or Key.Escape) { Skip(); GetViewport().SetInputAsHandled(); }
        }
        else if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } || @event is InputEventScreenTouch { Pressed: true }) Advance();
    }

    public override void _Ready()
    {
        AddChild(new Sprite2D { Texture = (_picture ?? MakeDawn)().ToTexture(), Centered = false, Scale = new Vector2(Layout.SceneSize.X / PictureWidth, Layout.SceneSize.Y / PictureHeight) });
        AddChild(new ColorRect { Color = new Color(0.03f, 0.02f, 0.08f, 0.62f), Position = new Vector2(70, 480), Size = new Vector2(Layout.SceneSize.X - 140, 240), MouseFilter = Control.MouseFilterEnum.Ignore });
        var title = new PixelLabel(PixelLabel.Style.Title, 6, PixelLabel.Align.Center) { Position = new Vector2(Layout.SceneSize.X / 2, 70) };
        title.SetText(_titleKey == null ? UIText.EpilogueTitle : Loc.Get(_titleKey));
        AddChild(title);
        for (int i = 0; i < MaxLines; i++)
        {
            _lines[i] = new PixelLabel(PixelLabel.Style.Value, 3, PixelLabel.Align.Center) { Position = new Vector2(Layout.SceneSize.X / 2, 506 + i * 40) };
            AddChild(_lines[i]);
        }
        _hint.Position = new Vector2(Layout.SceneSize.X - 90, 692);
        _hint.SetText(UIText.EpilogueHint);
        AddChild(_hint);
        _cover.Size = Layout.SceneSize;
        _cover.ZIndex = 10;
        AddChild(_cover);
        _coverTarget = 0;
        if (_pages.Count == 0) { _leaving = true; _coverTarget = 1; } else ShowPage();
    }

    private void ShowPage()
    {
        _wrapped = GameHud.Wrap(_pages[_page], LineWidth, MaxLines);
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
            _lines[i].SetText(text.Substring(0, Math.Min(text.Length, Math.Max(0, left))));
            left -= text.Length;
        }
    }

    /// <summary>Finishes typing the page; if it is complete, turns to the next (after the last, the screen fades out).</summary>
    public void Advance()
    {
        if (_leaving || _clock < InputLock) return;
        SfxPlayer.Instance?.Play(SfxBank.Sound.DialogueNext);
        if (_typed < _total) { _typed = _total; ShowTyped(); return; }
        if (++_page >= _pages.Count) { _leaving = true; _coverTarget = 1; }
        else ShowPage();
    }

    /// <summary>Gives up the rest of the pages.</summary>
    public void Skip()
    {
        if (_leaving || _clock < InputLock) return;
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuBack);
        _leaving = true; _coverTarget = 1;
    }

    public override void _Process(double delta)
    {
        float dt = (float)Math.Min(delta, 0.1);
        _clock += dt;
        if (!_leaving && _typed < _total) { _typed = Math.Min(_total, _typed + TypeSpeed * dt); ShowTyped(); }
        _cover01 = Mathf.MoveToward(_cover01, _coverTarget, dt / FadeTime);
        _cover.Color = new Color(0, 0, 0, _cover01);
        if (_leaving && _cover01 >= 1) { SetProcess(false); Finished?.Invoke(); }
    }

    /// <summary>A dawn on the river: the sky going from violet to gold at the horizon, the sun half risen with its rays, the mountains of the Andes dark against it and its light on the water.</summary>
    private static PixelCanvas MakeDawn()
    {
        var c = new PixelCanvas(PictureWidth, PictureHeight);
        const int horizon = 104;
        PixelColor[] sky = { PixelColor.Hex(0x1C1646), PixelColor.Hex(0x4A2A78), PixelColor.Hex(0xA8407A), PixelColor.Hex(0xF0705A), PixelColor.Hex(0xFFB347), PixelColor.Hex(0xFFE9A0) };
        PixelColor[] water = { PixelColor.Hex(0xFFD070), PixelColor.Hex(0xE08060), PixelColor.Hex(0x7A3A78), PixelColor.Hex(0x2C2056), PixelColor.Hex(0x161240) };
        const float sunX = 111f, sunY = horizon - 2f;
        for (int y = 0; y < PictureHeight; y++)
            for (int x = 0; x < PictureWidth; x++)
            {
                float bayer = Bayer.Threshold(x, y) - 0.5f;
                if (y < horizon)
                {
                    float t = MathF.Pow(y / (float)horizon, 1.5f) * (sky.Length - 1);
                    c[x, y] = sky[Math.Clamp((int)MathF.Floor(t + bayer * 0.9f + 0.5f), 0, sky.Length - 1)];
                }
                else
                {
                    float t = (y - horizon) / (float)(PictureHeight - horizon) * (water.Length - 1) * 1.15f;
                    var col = water[Math.Clamp((int)MathF.Floor(t + bayer * 0.9f + 0.5f), 0, water.Length - 1)];
                    // the sun's road on the water: broken bands of light under the sun
                    float road = 1 - MathF.Abs(x - sunX) / (6 + (y - horizon) * 0.5f);
                    if (road > 0 && (y + (x / 3)) % 3 != 0) col = col.Mixed(PixelColor.Hex(0xFFE9A0), Math.Clamp(road * 0.8f - (y - horizon) * 0.01f, 0, 0.8f));
                    c[x, y] = col;
                }
            }
        // rays of the sun
        for (int k = 0; k < 15; k++)
        {
            float a = -MathF.PI + (k + 0.5f) * MathF.PI / 15f;
            for (float r = 20; r < (k % 2 == 0 ? 80 : 52); r += 0.7f)
            {
                int x = (int)MathF.Round(sunX + MathF.Cos(a) * r), y = (int)MathF.Round(sunY + MathF.Sin(a) * r);
                if (c.Contains(x, y) && y < horizon && (x + y) % 2 == 0) c[x, y] = c[x, y].Mixed(PixelColor.Hex(0xFFE9A0), 0.35f);
            }
        }
        // the sun
        for (int y = 0; y < horizon; y++)
            for (int x = 0; x < PictureWidth; x++)
            {
                float d = MathF.Sqrt((x - sunX) * (x - sunX) + (y - sunY) * (y - sunY));
                if (d <= 17) c[x, y] = d <= 11 ? PixelColor.Hex(0xFFFBE0) : PixelColor.Hex(0xFFD35A);
            }
        // two ranges of mountains, the far one violet and the near one dark
        for (int x = 0; x < PictureWidth; x++)
        {
            int far = horizon - 6 - (int)(14 * MathF.Abs(MathF.Sin(x * 0.045f + 0.4f)) + 5 * MathF.Sin(x * 0.19f));
            for (int y = far; y < horizon; y++) c[x, y] = PixelColor.Hex(0x5A3A7A);
            int near = horizon - 1 - (int)(22 * MathF.Pow(MathF.Abs(MathF.Sin(x * 0.03f + 2.1f)), 0.9f) + 3 * MathF.Sin(x * 0.33f));
            for (int y = near; y < horizon; y++) c[x, y] = PixelColor.Hex(0x241A44);
        }
        return c;
    }

    /// <summary>The picture of the opening: a night without end. The sky is deep blue with stars and a pale moon that does not move, the mountains are black against it, and on the river, small and
    /// alone, a boat with a lantern whose warm light is the only warm thing in the picture.</summary>
    public static PixelCanvas MakeNight()
    {
        var c = new PixelCanvas(PictureWidth, PictureHeight);
        const int horizon = 100;
        PixelColor[] sky = { PixelColor.Hex(0x03040E), PixelColor.Hex(0x0A1030), PixelColor.Hex(0x142050), PixelColor.Hex(0x22346E) };
        PixelColor[] water = { PixelColor.Hex(0x1C2C60), PixelColor.Hex(0x101C46), PixelColor.Hex(0x080F2C), PixelColor.Hex(0x03061A) };
        const float moonX = 150f, moonY = 40f;
        var rng = new SeededGenerator(0x1A57);
        for (int y = 0; y < PictureHeight; y++)
            for (int x = 0; x < PictureWidth; x++)
            {
                float bayer = Bayer.Threshold(x, y) - 0.5f;
                if (y < horizon)
                {
                    float t = MathF.Pow(y / (float)horizon, 1.4f) * (sky.Length - 1);
                    var col = sky[Math.Clamp((int)MathF.Floor(t + bayer * 0.9f + 0.5f), 0, sky.Length - 1)];
                    float d = MathF.Sqrt((x - moonX) * (x - moonX) + (y - moonY) * (y - moonY));
                    float glow = MathF.Pow(MathF.Max(0, 1 - d / 46f), 1.7f);
                    if (MathF.Floor(glow * 5 + bayer * 0.8f) / 5 > 0) col = col.Mixed(PixelColor.Hex(0x6A86C8), MathF.Min(1, MathF.Floor(glow * 5 + bayer * 0.8f) / 5 * 0.9f));
                    c[x, y] = col;
                }
                else
                {
                    float t = (y - horizon) / (float)(PictureHeight - horizon) * (water.Length - 1) * 1.1f;
                    var col = water[Math.Clamp((int)MathF.Floor(t + bayer * 0.9f + 0.5f), 0, water.Length - 1)];
                    float road = 1 - MathF.Abs(x - moonX) / (3 + (y - horizon) * 0.35f);
                    if (road > 0 && (y + x / 3) % 3 != 0) col = col.Mixed(PixelColor.Hex(0xC8D8F8), Math.Clamp(road * 0.55f, 0, 0.55f));
                    c[x, y] = col;
                }
            }
        // stars
        for (int i = 0; i < 150; i++)
        {
            int x = rng.Range(0, PictureWidth - 1), y = rng.Range(0, horizon - 8);
            if (MathF.Sqrt((x - moonX) * (x - moonX) + (y - moonY) * (y - moonY)) < 20) continue;
            c[x, y] = i % 7 == 0 ? PixelColor.Hex(0xFFF2B0) : (i % 3 == 0 ? PixelColor.Hex(0xAFC8FF) : PixelColor.Hex(0xFFFFFF));
        }
        // the moon, pale and full
        for (int y = 0; y < horizon; y++)
            for (int x = 0; x < PictureWidth; x++)
            {
                float d = MathF.Sqrt((x - moonX) * (x - moonX) + (y - moonY) * (y - moonY));
                if (d <= 13) c[x, y] = d > 11.5f ? PixelColor.Hex(0xC8D4F0) : ((x - moonX) * 0.5f + (y - moonY) * 0.5f > 3 + Bayer.Threshold(x, y) * 3 ? PixelColor.Hex(0xD6DCEE) : PixelColor.Hex(0xF2F6FF));
            }
        // the mountains, black against the sky
        for (int x = 0; x < PictureWidth; x++)
        {
            int far = horizon - 4 - (int)(12 * MathF.Abs(MathF.Sin(x * 0.04f + 1f)) + 4 * MathF.Sin(x * 0.17f));
            for (int y = far; y < horizon; y++) c[x, y] = PixelColor.Hex(0x0E1434);
            int near = horizon - 1 - (int)(16 * MathF.Pow(MathF.Abs(MathF.Sin(x * 0.028f + 2.6f)), 0.9f) + 2 * MathF.Sin(x * 0.3f));
            for (int y = near; y < horizon; y++) c[x, y] = PixelColor.Hex(0x060A1C);
        }
        // the boat and its lantern, small and alone in the middle of the river
        const int bx = 80, by = 126;
        for (int dy = -9; dy <= 9; dy++)
            for (int dx = -12; dx <= 12; dx++)
            {
                float d = MathF.Sqrt(dx * dx + dy * dy);
                if (d < 12 && (dx + dy) % 2 == 0 && c.Contains(bx + dx - 5, by + dy - 6)) c[bx + dx - 5, by + dy - 6] = c[bx + dx - 5, by + dy - 6].Mixed(PixelColor.Hex(0xFFB347), 0.5f * (1 - d / 12));
            }
        for (int x = -8; x <= 8; x++) if (c.Contains(bx + x, by)) c[bx + x, by] = PixelColor.Hex(0x04060F);
        for (int x = -6; x <= 6; x++) if (c.Contains(bx + x, by + 1)) c[bx + x, by + 1] = PixelColor.Hex(0x0A0E22);
        for (int y = -14; y < 0; y++) if (c.Contains(bx + 2, by + y)) c[bx + 2, by + y] = PixelColor.Hex(0x04060F);                     // the mast
        for (int y = -13; y < -2; y++) for (int x = 3; x < 3 + (y + 13) / 2 + 1; x++) if (c.Contains(bx + x, by + y)) c[bx + x, by + y] = PixelColor.Hex(0x14183A);   // the sail
        for (int y = -7; y <= -5; y++) for (int x = -6; x <= -4; x++) if (c.Contains(bx + x, by + y)) c[bx + x, by + y] = y == -6 && x == -5 ? PixelColor.Hex(0xFFFAD0) : PixelColor.Hex(0xFFC060);   // the lantern
        for (int y = -9; y < -7; y++) if (c.Contains(bx - 5, by + y)) c[bx - 5, by + y] = PixelColor.Hex(0x04060F);
        // its light on the water
        for (int y = by + 2; y < by + 12; y++) for (int x = bx - 6; x <= bx - 4; x++) if (c.Contains(x, y) && (x + y) % 2 == 0) c[x, y] = c[x, y].Mixed(PixelColor.Hex(0xFFB347), 0.55f);
        return c;
    }
}
