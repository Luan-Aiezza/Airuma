using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// The end of the story, in one picture with its writing (<see cref="EndArt"/>): a sunrise all in gold and shadow, the boy far off on the right bank, small, watching it, the shadow of his boat
/// moored at the left; and, written in the dark of the sky at the top, the whole story at once, no box; and at the foot, in the middle, FIM. It fades in from black, shows for <see cref="Seconds"/> seconds and fades out by itself; any key, click or touch closes it
/// at once. Then <see cref="Finished"/> is raised.
/// </summary>
public partial class EpilogueScreen : Node2D
{
    public event Action Finished;

    public const float Seconds = 10;
    private const int LineWidth = 80;
    private const float FadeTime = 1.1f, InputLock = 0.5f, TextX = 50, TextY = 40, LineGap = 22, ParagraphGap = 10;

    private readonly string[] _pageKeys;
    private readonly ColorRect _cover = new() { Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Ignore };
    private float _clock, _cover01 = 1, _coverTarget;
    private bool _leaving;

    public EpilogueScreen(IEnumerable<string> pageKeys)
    {
        _pageKeys = pageKeys.ToArray();
        ZIndex = 148;
    }

    public override void _Ready()
    {
        AddChild(new Sprite2D { Texture = EndArt.MakeScene().ToTexture(), Centered = false, Scale = new Vector2(Layout.SceneSize.X / EndArt.Width, Layout.SceneSize.Y / EndArt.Height) });
        // the whole story, as paragraphs, written at once, in the gold of the picture
        float y = TextY;
        for (int p = 0; p < _pageKeys.Length; p++)
        {
            if (p > 0) y += ParagraphGap;
            foreach (var line in GameHud.Wrap(Loc.Get(_pageKeys[p]), LineWidth, 6))
            {
                var label = new PixelLabel(PixelLabel.Style.Speech, 2, PixelLabel.Align.Left) { Position = new Vector2(TextX, y) };
                label.SetText(line);
                AddChild(label);
                y += LineGap;
            }
        }
        // FIM, big, at the foot of the picture, in the middle (10 points to the art pixel: the letters are 70 points tall), in the gold of the sunrise
        var fim = new PixelLabel(PixelLabel.Style.Gold, 10, PixelLabel.Align.Center) { Position = new Vector2(Layout.SceneSize.X / 2, 662) };
        fim.SetText(UIText.TheEnd);
        AddChild(fim);
        var hint = new PixelLabel(PixelLabel.Style.Dim, 2, PixelLabel.Align.Right) { Position = new Vector2(Layout.SceneSize.X - 24, 742) };
        hint.SetText(UIText.IntroHint);
        AddChild(hint);
        _cover.Size = Layout.SceneSize;
        _cover.ZIndex = 10;
        AddChild(_cover);
        _coverTarget = 0;
    }

    public override void _Input(InputEvent @event)
    {
        bool press = @event is InputEventKey { Pressed: true, Echo: false } || @event is InputEventMouseButton { Pressed: true } || @event is InputEventScreenTouch { Pressed: true }
                     || @event is InputEventJoypadButton { Pressed: true };
        if (!press) return;
        if (Main.IsFullscreenKey(@event)) return;                        // the window's own keys
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
        if (!_leaving && _clock >= Seconds) Close();
        _cover01 = Mathf.MoveToward(_cover01, _coverTarget, dt / FadeTime);
        _cover.Color = new Color(0, 0, 0, _cover01);
        if (_leaving && _cover01 >= 1) { SetProcess(false); Finished?.Invoke(); }
    }
}
