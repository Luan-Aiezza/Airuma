using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// The claws of the boss, as the scene shows them (see <see cref="SlashData"/>): first a thin, blinking red line with small marks at its head, to warn where it will
/// cut; then, when it strikes, a jagged band, red with a white-hot core and teeth along both edges, that flashes across the field; then a scar that fades.
///
/// API: Show(IReadOnlyList&lt;SlashData&gt;) every frame. The node sits at the scene origin; the slashes are in game coordinates and are moved to the scene by the inset
/// of the river.
/// </summary>
public partial class SlashEffect : Node2D
{
    private readonly List<SlashData> _slashes = new();
    private float _clock;

    public override void _Ready()
    {
        // The cuts are cut at the edges of the river, so that they do not show over the pillars and the panel.
        RenderingServer.CanvasItemSetClip(GetCanvasItem(), true);
        RenderingServer.CanvasItemSetCustomRect(GetCanvasItem(), true, new Rect2(Vector2.Zero, new Vector2(Layout.RiverWidth, Layout.Height)));
    }

    public void Show(IReadOnlyList<SlashData> slashes, float dt)
    {
        _clock += dt;
        if (slashes.Count == 0 && _slashes.Count == 0) return;
        _slashes.Clear();
        _slashes.AddRange(slashes);
        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach (var s in _slashes)
        {
            var top = Layout.ScenePoint(s.Top);
            var bottom = Layout.ScenePoint(s.Bottom);
            var dir = (bottom - top).Normalized();
            var normal = new Vector2(-dir.Y, dir.X);
            if (s.Warning)
            {
                // A thin red line that blinks faster as the cut comes near, and three little marks at its head, like the points of the claws.
                float u = s.Age / s.Telegraph;
                float blink = 0.45f + 0.4f * MathF.Sin(_clock * (12 + 22 * u));
                var warn = new Color(1f, 0.16f, 0.1f, Math.Clamp(blink * (0.5f + u), 0, 1));
                for (float d = 0; d < s.TotalLength; d += 34) DrawLine(top + dir * d, top + dir * (d + 20), warn, 3 + 2 * u);
                foreach (int k in new[] { -1, 0, 1 }) DrawLine(top + dir * 40 + normal * (k * 9), top + dir * 62 + normal * (k * 9), new Color(1, 0.6f, 0.5f, warn.A), 3);
                continue;
            }
            float t = s.Age - s.Telegraph;
            float fade = t < s.Strike ? 1 : Math.Clamp(1 - (t - s.Strike) / SlashData.Fade, 0, 1);
            float grow = Math.Clamp(t / 0.06f, 0, 1);                          // the cut opens in a flash
            float half = s.Width / 2 * (0.4f + 0.6f * grow) * (t < s.Strike ? 1 : 0.35f + 0.65f * fade);
            // the jagged band: teeth on both edges
            var left = new List<Vector2>(); var right = new List<Vector2>();
            int steps = (int)(s.TotalLength / 16);
            for (int i = 0; i <= steps; i++)
            {
                var p = top + dir * (i * 16f);
                float tooth = (i % 2 == 0) ? 0 : 7;
                left.Add(p - normal * (half - tooth * 0.8f + (i % 4 == 1 ? 3 : 0)));
                right.Add(p + normal * (half - (7 - tooth) * 0.8f + (i % 4 == 3 ? 3 : 0)));
            }
            var poly = new List<Vector2>(left);
            for (int i = right.Count - 1; i >= 0; i--) poly.Add(right[i]);
            DrawColoredPolygon(poly.ToArray(), new Color(0.78f, 0.06f, 0.08f, 0.92f * fade));
            DrawLine(top, bottom, new Color(1f, 0.9f, 0.85f, fade), Math.Max(2, half * 0.35f));
            DrawLine(top, bottom, new Color(1f, 0.45f, 0.4f, 0.6f * fade), Math.Max(3, half * 0.7f));
        }
    }
}
