using System;
using Godot;

/// <summary>
/// The moon of a gravity attack (see <see cref="GravitySpec"/>): a disc of pixel art (see <see cref="GravityMoonArt"/>) with a dithered glow, all on the grid of the pixels of art. When it is about to pull it swells and rings of pixels close in on it; while it pulls, rings run
/// inwards and a faint line joins it to the boat.
///
/// API: Show(MoonData, boat position, dt) every frame.
/// </summary>
public partial class MoonEffect : Node2D
{
    private MoonData _moon;
    private Vector2 _boat;
    private float _clock;

    public override void _Ready()
    {
        RenderingServer.CanvasItemSetClip(GetCanvasItem(), true);
        RenderingServer.CanvasItemSetCustomRect(GetCanvasItem(), true, new Rect2(Vector2.Zero, new Vector2(Layout.RiverWidth, Layout.Height)));
    }

    public void Show(MoonData moon, Vector2 boat, float dt)
    {
        _clock += dt;
        if (!moon.Active && !_moon.Active) return;
        _moon = moon; _boat = boat;
        QueueRedraw();
    }

    private const float Grid = 4;                       // the size of a pixel of art on the screen
    private Texture2D[] _textures;

    /// <summary>The scene's point snapped to the grid of the pixels of art, so that nothing the moon draws falls between two pixels.</summary>
    private static Vector2 Snap(Vector2 p) => new Vector2(MathF.Floor(p.X / Grid) * Grid, MathF.Floor(p.Y / Grid) * Grid);

    private void Block(Vector2 p, Color colour) => DrawRect(new Rect2(Snap(p), new Vector2(Grid, Grid)), colour);

    public override void _Draw()
    {
        if (!_moon.Active) return;
        _textures ??= new[] { GravityMoonArt.Make(0).ToTexture(), GravityMoonArt.Make(1).ToTexture() };
        TextureFilter = TextureFilterEnum.Nearest;
        var at = Layout.ScenePoint(_moon.Position);
        bool swollen = _moon.Warning > 0.35f || _moon.Pull > 0;
        float r = _moon.Radius * (swollen ? 1.13f : 1);
        float glow = 0.25f + 0.5f * Math.Max(_moon.Warning, _moon.Pull);
        int cx = (int)MathF.Floor(at.X / Grid), cy = (int)MathF.Floor(at.Y / Grid);
        // the glow: rings of light of dithered blocks, one pixel of art each
        float cells = r / Grid;
        for (int j = -(int)(cells * 2.5f); j <= (int)(cells * 2.5f); j++)
            for (int i = -(int)(cells * 2.5f); i <= (int)(cells * 2.5f); i++)
            {
                float d = MathF.Sqrt(i * i + j * j);
                float level = d < cells * 1.7f ? 0.55f : (d < cells * 2.5f ? 0.28f : 0);
                if (level <= 0 || level * (0.6f + glow) < Bayer.Threshold(cx + i, cy + j) * 0.9f) continue;
                DrawRect(new Rect2(new Vector2((cx + i) * Grid, (cy + j) * Grid), new Vector2(Grid, Grid)), new Color(0.72f, 0.82f, 1f, d < cells * 1.7f ? 0.2f : 0.1f));
            }
        // rings that close in on the moon (a warning, and then the pull itself): circles of one-pixel blocks
        float strength = Math.Max(_moon.Warning * 0.7f, _moon.Pull);
        if (strength > 0)
            for (int k = 0; k < 4; k++)
            {
                float t = (_clock * 0.9f + k / 4f) % 1f;
                float ring = (r * (1.2f + 3.2f * (1 - t))) / Grid;
                int steps = Math.Max(16, (int)(ring * 5));
                var colour = new Color(0.88f, 0.93f, 1f, Math.Clamp(0.75f * strength * t, 0, 1));
                for (int s = 0; s < steps; s++)
                {
                    float a = MathF.Tau * s / steps;
                    DrawRect(new Rect2(new Vector2((cx + MathF.Round(MathF.Cos(a) * ring)) * Grid, (cy + MathF.Round(MathF.Sin(a) * ring)) * Grid), new Vector2(Grid, Grid)), colour);
                }
            }
        // the pull: a line of square dashes from the boat to the moon
        if (_moon.Pull > 0)
        {
            var boat = Layout.ScenePoint(_boat);
            var dir = (at - boat).Normalized();
            float len = boat.DistanceTo(at);
            for (float d = (_clock * 220) % 36; d < len - r; d += 36) Block(boat + dir * d, new Color(0.88f, 0.93f, 1f, 0.6f * _moon.Pull));
        }
        // the disc: a picture of pixels, never smoothed
        var texture = _textures[swollen ? 1 : 0];
        var size = new Vector2(texture.GetWidth(), texture.GetHeight()) * Grid;
        DrawTextureRect(texture, new Rect2(Snap(at - size / 2 + new Vector2(Grid / 2, Grid / 2)), size), false);
    }
}
