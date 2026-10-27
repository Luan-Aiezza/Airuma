using Godot;

/// <summary>
/// What fills the sides of the picture when the screen is wider than the game (a full screen on a 16:9 monitor, for example):
/// on each side, a wall of dark jungle and, against the edge of the game, a pillar of ancient stone wrapped in vines
/// (<see cref="SidePillarArt"/>). Call <see cref="Fit"/> whenever the size of the view changes.
/// </summary>
public partial class SideFrame : Node2D
{
    private readonly Sprite2D _wallLeft = new() { Centered = false, TextureRepeat = TextureRepeatEnum.Enabled };
    private readonly Sprite2D _wallRight = new() { Centered = false, TextureRepeat = TextureRepeatEnum.Enabled };
    private readonly Sprite2D _pillarLeft = new() { Centered = false };
    private readonly Sprite2D _pillarRight = new() { Centered = false, FlipH = true };

    public SideFrame()
    {
        ZIndex = -50;
        var wall = SidePillarArt.MakeWall().ToTexture();
        var pillar = SidePillarArt.MakePillar().ToTexture();
        foreach (var w in new[] { _wallLeft, _wallRight })
        {
            w.Texture = wall;
            w.RegionEnabled = true;
            w.Scale = new Vector2(SidePillarArt.Scale, SidePillarArt.Scale);
            AddChild(w);
        }
        foreach (var p in new[] { _pillarLeft, _pillarRight })
        {
            p.Texture = pillar;
            p.Scale = new Vector2(SidePillarArt.Scale, SidePillarArt.Scale);
            AddChild(p);
        }
    }

    /// <summary>`margin` is the width of the free space at each side of the game, `gameWidth` that of the game, `viewHeight` the view's height.</summary>
    public void Fit(float margin, float gameWidth, float viewHeight)
    {
        Visible = margin > 0.5f;
        if (!Visible) return;
        float s = SidePillarArt.Scale;
        float scaleY = viewHeight / (SidePillarArt.Height * s);
        float pillarW = SidePillarArt.PillarWidth * s;
        // The walls cover the whole margin (and a little under the game, which is drawn over them).
        float wallArt = (margin + 8) / s;
        foreach (var w in new[] { _wallLeft, _wallRight })
        {
            w.RegionRect = new Rect2(0, 0, wallArt, SidePillarArt.Height);
            w.Scale = new Vector2(s, s * scaleY);
        }
        _wallLeft.Position = new Vector2(0, 0);
        _wallRight.Position = new Vector2(margin + gameWidth - 8, 0);
        // The pillars stand against the edges of the game.
        foreach (var p in new[] { _pillarLeft, _pillarRight }) p.Scale = new Vector2(s, s * scaleY);
        _pillarLeft.Position = new Vector2(margin - pillarW + 4, 0);
        _pillarRight.Position = new Vector2(margin + gameWidth - 4, 0);
    }
}
