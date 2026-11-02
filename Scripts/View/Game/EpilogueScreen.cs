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

    public EpilogueScreen(IEnumerable<string> pageKeys)
    {
        _pages = pageKeys.Select(Loc.Get).ToList();
        ZIndex = 148;
    }

    public override void _Ready()
    {
        AddChild(new Sprite2D { Texture = MakeDawn().ToTexture(), Centered = false, Scale = new Vector2(Layout.SceneSize.X / PictureWidth, Layout.SceneSize.Y / PictureHeight) });
        AddChild(new ColorRect { Color = new Color(0.03f, 0.02f, 0.08f, 0.62f), Position = new Vector2(70, 480), Size = new Vector2(Layout.SceneSize.X - 140, 240), MouseFilter = Control.MouseFilterEnum.Ignore });
        var title = new PixelLabel(PixelLabel.Style.Title, 6, PixelLabel.Align.Center) { Position = new Vector2(Layout.SceneSize.X / 2, 70) };
        title.SetText(UIText.EpilogueTitle);
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
}
