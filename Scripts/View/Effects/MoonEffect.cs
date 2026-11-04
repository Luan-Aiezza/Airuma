using System;
using Godot;

/// <summary>
/// The moon of a gravity attack (see <see cref="GravitySpec"/>): a pale disc with craters and a halo. When it is about to pull it swells and rings of light close in on it; while it pulls, rings run
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

    public override void _Draw()
    {
        if (!_moon.Active) return;
        var at = Layout.ScenePoint(_moon.Position);
        float r = _moon.Radius * (1 + 0.12f * _moon.Warning + 0.06f * MathF.Sin(_clock * 4));
        float glow = 0.25f + 0.5f * Math.Max(_moon.Warning, _moon.Pull);
        DrawCircle(at, r * 2.4f, new Color(0.6f, 0.7f, 1f, 0.07f + 0.06f * glow));
        DrawCircle(at, r * 1.6f, new Color(0.75f, 0.82f, 1f, 0.12f + 0.1f * glow));
        // rings that close in on the moon: a warning, and then the pull itself
        float strength = Math.Max(_moon.Warning * 0.7f, _moon.Pull);
        if (strength > 0)
            for (int k = 0; k < 4; k++)
            {
                float t = (_clock * 0.9f + k / 4f) % 1f;
                float ring = r * (1.2f + 3.2f * (1 - t));
                DrawArc(at, ring, 0, MathF.Tau, 48, new Color(0.85f, 0.9f, 1f, 0.5f * strength * t), 2);
            }
        if (_moon.Pull > 0)
        {
            var boat = Layout.ScenePoint(_boat);
            var dir = (at - boat).Normalized();
            float len = boat.DistanceTo(at);
            for (float d = (_clock * 220) % 36; d < len - r; d += 36) DrawLine(boat + dir * d, boat + dir * Math.Min(d + 14, len - r), new Color(0.85f, 0.9f, 1f, 0.35f * _moon.Pull), 2);
        }
        // the disc
        DrawCircle(at, r, new Color(0.93f, 0.96f, 1f));
        DrawCircle(at + new Vector2(-r * 0.18f, -r * 0.12f), r * 0.82f, new Color(0.84f, 0.89f, 0.98f));
        foreach (var (dx, dy, cr) in new[] { (-0.35f, -0.25f, 0.2f), (0.3f, 0.1f, 0.26f), (-0.1f, 0.4f, 0.14f), (0.38f, -0.38f, 0.1f) })
            DrawCircle(at + new Vector2(dx, dy) * r, cr * r, new Color(0.66f, 0.73f, 0.9f, 0.8f));
        DrawArc(at, r, 0, MathF.Tau, 40, new Color(1, 1, 1, 0.9f), 2);
    }
}
