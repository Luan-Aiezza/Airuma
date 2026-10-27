using System;

/// <summary>
/// The art of the lantern's bomb, all drawn additively and centred on the lantern: a flash of light that blooms out
/// to the edge of its reach, a shockwave ring, and rays that turn around the light. Every piece is a pre-drawn
/// frame on the same grid, so nothing is scaled and the pixels stay crisp.
/// </summary>
public static class BombArt
{
    public const float Scale = 4;
    /// <summary>Reach of the light, in art pixels (about the bomb's 250 points).</summary>
    public const int Reach = 62;
    public const int Size = 2 * Reach + 1;

    private static readonly PixelColor Core = PixelColor.Hex(0xFFF3B8);
    private static readonly PixelColor Warm = PixelColor.Hex(0xFFB43A);
    private static readonly PixelColor White = PixelColor.Hex(0xFFFFFF);

    // The bloom

    /// <summary>How far (as a fraction of the reach) the bloom has grown, frame by frame while it swells, then twice for the steady pulse.</summary>
    public static readonly float[] Growth = { 0.25f, 0.5f, 0.75f, 1.0f };
    public static readonly float[] Steady = { 1.0f, 0.94f };

    /// <summary>A soft, bright disc of light, strongest in the middle, dithered into a few steps.</summary>
    public static PixelCanvas MakeBloom(float fraction) =>
        GlowArt.MakeCanvas(Size, Reach * fraction, Core, Warm, new GlowArt.Tiers(190, 120, 55));

    // Rays

    public const int Spokes = 12;
    public const int RayFrames = 8;

    /// <summary>
    /// Twelve rays of light that taper and fade towards the edge. Frame n is turned n/8 of the angle between two
    /// rays, so cycling the frames makes the whole fan turn.
    /// </summary>
    public static PixelCanvas MakeRays(int frame)
    {
        var c = new PixelCanvas(Size, Size);
        float centre = Reach + 0.5f;
        float step = 2 * MathF.PI / Spokes;
        float offset = step * frame / RayFrames;
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float dx = x + 0.5f - centre, dy = y + 0.5f - centre;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                if (!(d > 7 && d < Reach)) continue;
                float angle = MathF.Atan2(dy, dx) - offset;
                int k = (int)MathF.Round(angle / step, MidpointRounding.AwayFromZero);
                // Distance (in radians) from the middle of the nearest ray.
                float away = MathF.Abs(angle - k * step);
                // Every other ray is longer, and each narrows as it goes out.
                float length = Reach * (k % 2 == 0 ? 1.0f : 0.72f);
                if (!(d < length)) continue;
                float width = 0.17f * (1 - 0.7f * d / length);
                if (!(away < width)) continue;
                float fade = MathF.Pow(1 - d / length, 1.1f);
                // Brighter along the spine of the ray; dithered so it reads as pixel art.
                float spine = 1 - away / width;
                float level = MathF.Floor((fade * 0.7f + spine * 0.4f) * 4 + (Bayer.Threshold(x, y) - 0.5f) * 0.9f) / 4;
                if (!(level > 0.2f)) continue;
                var color = level > 0.65f ? White : Core;
                c[x, y] = color.WithAlpha((byte)MathF.Min(255, 190 * level));
            }
        }
        return c;
    }

    // Shockwave

    public const int RingFrames = 10;

    /// <summary>A ring of light spreading out from the lantern, thinning and fading as it goes.</summary>
    public static PixelCanvas MakeRing(int frame)
    {
        var c = new PixelCanvas(Size, Size);
        float centre = Reach + 0.5f;
        float t = (float)(frame + 1) / RingFrames;
        float radius = Reach * (1 - MathF.Pow(1 - t, 2)); // fast at first, slowing down
        float thickness = 3.2f - 2 * t;
        var color = White.WithAlpha((byte)(235 * (1 - t * 0.85f)));
        var halo = Warm.WithAlpha((byte)(120 * (1 - t)));
        for (int y = 0; y < Size; y++)
        {
            for (int x = 0; x < Size; x++)
            {
                float ddx = x + 0.5f - centre, ddy = y + 0.5f - centre;
                float d = MathF.Sqrt(ddx * ddx + ddy * ddy);
                if (MathF.Abs(d - radius) <= thickness * 0.5f) c[x, y] = color;
                else if (MathF.Abs(d - radius) <= thickness * 0.5f + 1.6f && d < radius) c[x, y] = halo;
            }
        }
        return c;
    }
}
