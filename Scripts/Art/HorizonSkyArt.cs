using System;
using P = PixelColor;

/// <summary>
/// The sky of the boss fight, seen over open water: the strip of night that reaches from the horizon (the bottom row) upwards, with
/// a pale haze along the horizon, a few thin clouds, stars, a faint Milky Way and the moon sitting half way down below the
/// horizon, so that only its upper half shows. The water below shows this same picture flipped, so the other half of the moon
/// appears there as its reflection.
/// </summary>
public static class HorizonSkyArt
{
    public const int Width = (int)(Layout.RiverWidth / 4);
    public const int Height = 52;
    /// <summary>Moon centre in grid pixels: on the very bottom edge, so that half of it is above the horizon.</summary>
    public static readonly (int x, int y) Moon = (150, Height);
    public const float MoonRadius = 13;

    private static float Hypot(float a, float b) => MathF.Sqrt(a * a + b * b);

    public static PixelCanvas MakeCanvas()
    {
        var rng = new SeededGenerator(0xB055);
        var clouds = new PeriodicNoise(9, 3, rng);
        var c = new PixelCanvas(Width, Height);
        P top = P.Hex(0x04081A), mid = P.Hex(0x0E2442), haze = P.Hex(0x5E8CAA), glowCol = P.Hex(0x9CC4D8);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                float t = (float)y / (Height - 1);                 // 0 at the top, 1 at the horizon
                P color = top.Mixed(mid, MathF.Min(1f, t * 1.3f));
                // the haze along the horizon, in flat dithered bands
                float h = MathF.Pow(t, 3.2f);
                float band = MathF.Floor(h * 6 + (Bayer.Threshold(x, y) - 0.5f) * 0.9f) / 6;
                if (band > 0) color = color.Mixed(haze, MathF.Min(1f, band * 1.2f));
                // the moon's glow
                float d = Hypot(x - Moon.x, y - Moon.y);
                float glow = MathF.Pow(MathF.Max(0, 1 - d / 40), 1.7f);
                float gband = MathF.Floor(glow * 5 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / 5;
                if (gband > 0) color = color.Mixed(glowCol, MathF.Min(1f, gband * 0.85f));
                // long thin clouds lying along the horizon
                float n = clouds.Sample((float)x / Width * 1.0f, (float)y / Height);
                if (n > 0.62f && t > 0.35f)
                {
                    float density = MathF.Min(1, (n - 0.62f) * 6);
                    float level = MathF.Floor(density * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.7f) / 3;
                    if (level > 0) color = color.Mixed(P.Hex(0x1A3558), level * 0.7f);
                }
                c[x, y] = color;
            }

        // a faint Milky Way across the upper part
        for (int i = 0; i < 260; i++)
        {
            float u = rng.Range(0f, 1f);
            float across = (rng.Range(-1f, 1f) + rng.Range(-1f, 1f)) * 9;
            int x = (int)(Width * (0.05f + 0.9f * u) + across * 0.6f), y = (int)(Height * (0.5f - 0.35f * u) + across * 0.45f);
            if (!c.Contains(x, y) || y > Height - 8 || Hypot(x - Moon.x, y - Moon.y) < 20) continue;
            c[x, y] = c[x, y].Mixed(P.Hex(0xA8C0DC), rng.Range(0.25f, 0.5f));
        }
        // stars: more of them high up, fewer in the haze
        P[] starColors = { P.Hex(0xFFFFFF), P.Hex(0xAFC8FF), P.Hex(0xFFE9B0) };
        for (int i = 0; i < 90; i++)
        {
            int x = rng.Range(1, Width - 2), y = rng.Range(0, Height - 10);
            if (Hypot(x - Moon.x, y - Moon.y) < 22) continue;
            if (rng.NextFloat() > 1.15f - (float)y / Height) continue;
            P color = starColors[rng.Range(0, starColors.Length - 1)];
            c[x, y] = color;
            if (i % 9 == 0)
                foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    if (c.Contains(x + dx, y + dy)) c[x + dx, y + dy] = color.Mixed(c[x + dx, y + dy], 0.55f);
        }
        DrawMoon(c);
        return c;
    }

    /// <summary>
    /// The high sky that the far water reflects, below the reflection of the horizon sky: the same deep night colour as the top of
    /// <see cref="MakeCanvas"/> (so the two join without a seam) with the same kind of stars and a faint Milky Way.
    /// </summary>
    public static PixelCanvas MakeDeepCanvas(int height = 110)
    {
        var rng = new SeededGenerator(0xDEE9);
        var c = new PixelCanvas(Width, height, P.Hex(0x04081A));
        for (int y = 0; y < height; y++)
            for (int x = 0; x < Width; x++)
                c[x, y] = P.Hex(0x04081A).Mixed(P.Hex(0x07102A), (float)y / height * 0.6f);
        for (int i = 0; i < 380; i++)
        {
            float u = rng.Range(0f, 1f);
            float across = (rng.Range(-1f, 1f) + rng.Range(-1f, 1f)) * 10;
            int x = (int)(Width * (0.0f + 1.0f * u) + across * 0.6f), y = (int)(height * (0.15f + 0.6f * u) + across * 0.5f);
            if (!c.Contains(x, y)) continue;
            c[x, y] = c[x, y].Mixed(P.Hex(0xA8C0DC), rng.Range(0.22f, 0.45f));
        }
        P[] starColors = { P.Hex(0xFFFFFF), P.Hex(0xAFC8FF), P.Hex(0xFFE9B0) };
        for (int i = 0; i < 190; i++)
        {
            int x = rng.Range(1, Width - 2), y = rng.Range(0, height - 2);
            P color = starColors[rng.Range(0, starColors.Length - 1)];
            c[x, y] = color;
            if (i % 9 == 0)
                foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    if (c.Contains(x + dx, y + dy)) c[x + dx, y + dy] = color.Mixed(c[x + dx, y + dy], 0.55f);
        }
        return c;
    }

    private static void DrawMoon(PixelCanvas c)
    {
        P cream = P.Hex(0xF6F2D2), shade = P.Hex(0xCBC7A2), crater = P.Hex(0xB4B08E);
        (float, float, float)[] craters = { (-4.5f, -3, 2.4f), (3.8f, -5.2f, 2.8f), (-0.5f, -8.5f, 1.8f), (6.5f, -2, 1.6f), (-8, -6.5f, 1.5f), (1.5f, -1.5f, 1.2f), (-3, -10f, 1.2f) };
        float r = MoonRadius;
        for (int dy = -14; dy <= 0; dy++)
            for (int dx = -14; dx <= 14; dx++)
            {
                float fx = dx + 0.5f, fy = dy + 0.5f;
                float d = Hypot(fx, fy);
                if (!(d <= r)) continue;
                P color = cream;
                if ((fx + fy) / (2 * r) > 0.18f + Bayer.Threshold(dx + 20, dy + 20) * 0.16f) color = shade;
                if (d > r - 1.1f) color = color.Mixed(shade, 0.45f);
                foreach (var (cx, cy, cr) in craters) if (Hypot(fx - cx, fy - cy) < cr) color = crater;
                int px = Moon.x + dx, py = Moon.y + dy;
                if (c.Contains(px, py)) c[px, py] = color;
            }
    }
}
