using System.Collections.Generic;
using Godot;

/// <summary>
/// The notice that an achievement was unlocked: a little box at the top right, over everything, that slides in, stays a few seconds and goes out (one at a time: the others wait their turn).
/// Steam shows its own notice too; this one is for the game without Steam, and for the file of the game itself.
/// </summary>
public partial class AchievementToast : Node2D
{
    private const float SlideSeconds = 0.35f, HoldSeconds = 3.2f, Margin = 16, Pad = 12;
    private readonly Queue<Achievements.Info> _waiting = new();
    private Node2D _box;
    private float _clock, _width;
    private bool _showing;

    public AchievementToast() { ZIndex = 900; }

    public override void _Ready() { Achievements.Unlocked += OnUnlocked; }

    public override void _ExitTree() { Achievements.Unlocked -= OnUnlocked; }

    private void OnUnlocked(Achievements.Info info) { _waiting.Enqueue(info); }

    /// <summary>The words of an achievement, in the language of the moment.</summary>
    public static string NameOf(Achievements.Info info) => info.Kind switch
    {
        Achievements.Kind.Boss => UIText.AchBoss(Stages.All[info.Stage].Boss.Name, info.Difficulty.Title()),
        Achievements.Kind.EasyJoke => UIText.AchJoke,
        Achievements.Kind.EndingBad => UIText.AchEndingBad,
        _ => UIText.AchEndingTrue,
    };

    private void Begin(Achievements.Info info)
    {
        _box?.QueueFree();
        _box = new Node2D();
        var top = new PixelLabel(PixelLabel.Style.Gold, 2, PixelLabel.Align.Left); top.SetText(UIText.AchUnlocked);
        var name = new PixelLabel(PixelLabel.Style.Value, 3, PixelLabel.Align.Left); name.SetText(NameOf(info));
        float w = System.Math.Max(PixelFont.Columns(UIText.AchUnlocked) * 2, PixelFont.Columns(NameOf(info)) * 3) + 2 * Pad;
        _width = w;
        const float h = 66;
        _box.AddChild(new ColorRect { Color = new Color(0.94f, 0.76f, 0.31f), Size = new Vector2(w + 4, h + 4), Position = new Vector2(-2, -2), MouseFilter = Control.MouseFilterEnum.Ignore });
        _box.AddChild(new ColorRect { Color = new Color(0.04f, 0.05f, 0.09f, 0.95f), Size = new Vector2(w, h), MouseFilter = Control.MouseFilterEnum.Ignore });
        top.Position = new Vector2(Pad, Pad); name.Position = new Vector2(Pad, Pad + 22);
        _box.AddChild(top); _box.AddChild(name);
        AddChild(_box);
        _clock = 0; _showing = true;
        SfxPlayer.Instance?.Play(SfxBank.Sound.LevelUp);
        Place(0);
    }

    /// <summary>0: out of the screen, 1: in its place.</summary>
    private void Place(float t) => _box.Position = new Vector2(Layout.SceneSize.X - Margin - _width * t + (1 - t) * 0, Margin);

    public override void _Process(double delta)
    {
        if (!_showing)
        {
            if (_waiting.Count > 0) Begin(_waiting.Dequeue());
            return;
        }
        _clock += (float)delta;
        float t = _clock < SlideSeconds ? _clock / SlideSeconds : (_clock < SlideSeconds + HoldSeconds ? 1 : 1 - (_clock - SlideSeconds - HoldSeconds) / SlideSeconds);
        Place(Mathf.Clamp(t, 0, 1));
        if (_clock >= SlideSeconds * 2 + HoldSeconds) { _box.QueueFree(); _box = null; _showing = false; }
    }
}
