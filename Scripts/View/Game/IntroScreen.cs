using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// The opening of the game: the night without end (<see cref="NightArt"/>): a sky of moon and stars, the boy far off on the black river with his lantern, the only gold, and, written in the darkness at the lower left (no box; it goes no
/// further than the middle of the screen), the whole story at once. It fades in from black, shows for <see cref="Seconds"/> seconds and fades out by itself; any key, click or touch makes it fade out at once. Then <see cref="Finished"/> is raised.
/// </summary>
public partial class IntroScreen : Node2D
{
    public event Action Finished;

    /// <summary>How long the picture and the writing show (from the start of the fade-in) before it closes by itself.</summary>
    public const float Seconds = 10;
    private const int MaxLines = 22;
    private readonly int LineWidth = 44;
    private readonly float Duration = Seconds;
    private readonly bool _bad;
    /// <summary>The writing starts 20 points from the left; 44 letters of 12 points end before the middle of the screen (555).</summary>
    private const float FadeTime = 0.9f, InputLock = 0.35f, TextX = 20;
    private readonly float TextY = 536, LineGap = 22, ParagraphGap = 10;

    private readonly string[] _pageKeys;
    private readonly List<PixelLabel> _lines = new();
    private readonly ColorRect _cover = new() { Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Ignore };
    private float _clock, _cover01 = 1, _coverTarget;
    private bool _leaving;

    /// <summary>`bad`: the picture of the bad ending (the boy fishing alone in the night, <see cref="NightArt.MakeScene"/> with `fishing`), with a longer writing, in lines across the foot of the picture.</summary>
    public IntroScreen(IEnumerable<string> pageKeys, bool bad = false)
    {
        _pageKeys = pageKeys.ToArray();
        _bad = bad;
        if (bad) { LineWidth = 80; TextY = 556; LineGap = 20; ParagraphGap = 4; Duration = 30; }
        ZIndex = 148;
    }

    public override void _Ready()
    {
        AddChild(new Sprite2D { Texture = NightArt.MakeScene(_bad).ToTexture(), Centered = false, Scale = new Vector2(Layout.SceneSize.X / NightArt.Width, Layout.SceneSize.Y / NightArt.Height) });

        // the whole story, as paragraphs, written at once
        float y = TextY;
        foreach (var key in _pageKeys)
            foreach (var line in GameHud.Wrap(Loc.Get(key), LineWidth, MaxLines))
            {
                var label = new PixelLabel(PixelLabel.Style.Value, 2, PixelLabel.Align.Left) { Position = new Vector2(TextX, y) };
                label.SetText(line);
                AddChild(label);
                _lines.Add(label);
                y += LineGap;
            }
            // (a little more room between paragraphs)
        RespaceParagraphs();

        var hint = new PixelLabel(PixelLabel.Style.Dim, 2, PixelLabel.Align.Right) { Position = new Vector2(Layout.SceneSize.X - 24, 736) };
        hint.SetText(UIText.IntroHint);
        AddChild(hint);
        _cover.Size = Layout.SceneSize;
        _cover.ZIndex = 10;
        AddChild(_cover);
        _coverTarget = 0;
    }

    /// <summary>Moves the lines down a little at the start of every paragraph after the first.</summary>
    private void RespaceParagraphs()
    {
        int index = 0;
        float extra = 0;
        for (int p = 0; p < _pageKeys.Length; p++)
        {
            int count = GameHud.Wrap(Loc.Get(_pageKeys[p]), LineWidth, MaxLines).Count;
            if (p > 0) extra += ParagraphGap;
            for (int i = 0; i < count; i++) _lines[index++].Position += new Vector2(0, extra);
        }
    }

    public override void _Input(InputEvent @event)
    {
        bool press = @event is InputEventKey { Pressed: true, Echo: false } || @event is InputEventMouseButton { Pressed: true } || @event is InputEventScreenTouch { Pressed: true }
                     || @event is InputEventJoypadButton { Pressed: true };
        if (!press) return;
        if (@event is InputEventKey { Keycode: Key.F11 }) return;       // the window's own key
        GetViewport().SetInputAsHandled();                               // (the game under it must not take the key, not even Escape)
        if (!_leaving && _clock >= InputLock) Close();
    }

    private void Close()
    {
        if (_leaving) return;
        _leaving = true;
        _coverTarget = 1;
    }

    public override void _Process(double delta)
    {
        float dt = (float)Math.Min(delta, 0.1);
        _clock += dt;
        if (!_leaving && _clock >= Duration) Close();
        _cover01 = Mathf.MoveToward(_cover01, _coverTarget, dt / FadeTime);
        _cover.Color = new Color(0, 0, 0, _cover01);
        if (_leaving && _cover01 >= 1) { SetProcess(false); Finished?.Invoke(); }
    }
}
