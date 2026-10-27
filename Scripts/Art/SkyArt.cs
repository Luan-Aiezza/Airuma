using System;
using P = PixelColor;

/// <summary>
/// The night sky that the river mirrors: a deep gradient, thin moonlit clouds, a Milky Way smudge, stars and the moon.
/// It is never drawn directly; it lives in the reflection and is distorted by the water.
/// </summary>
public static class SkyArt
{
    /// <summary>The river's width in grid pixels (4 points each).</summary>
    public const int Width = (int)(Layout.RiverWidth / 4);
    public const int Height = 194;
    /// <summary>Moon centre in grid pixels (column, row from the top).</summary>
    public static readonly (int x, int y) Moon = (104 + (int)(Layout.FieldInset / 4), 56);
    public const float MoonRadius = 8;

    private static float Hypot(float a, float b) => MathF.Sqrt(a * a + b * b);

    public static PixelCanvas MakeCanvas()
    {
        var rng = new SeededGenerator(0x5CA7);
        var clouds = new PeriodicNoise(5, 6, rng);
        var c = new PixelCanvas(Width, Height);

        P top = P.Hex(0x050A18), bottom = P.Hex(0x10304A);
        P halo = P.Hex(0x2E6288);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float t = (float)y / Height;
                P color = top.Mixed(bottom, t);
                // Moon halo.
                float d = Hypot(x - Moon.x, y - Moon.y);
                float glow = MathF.Pow(MathF.Max(0, 1 - d / 46), 1.6f);
                // Banded halo: flat steps, dithered only where one step meets the next.
                float steps = 5;
                float band = MathF.Floor(glow * steps + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / steps;
                if (band > 0) color = color.Mixed(halo, MathF.Min(1, band * 1.1f));
                // Streaky clouds, brighter near the moon.
                float n = clouds.Sample((float)x / Width, (float)y / Height);
                if (n > 0.6f)
                {
                    float density = MathF.Min(1, (n - 0.6f) * 6);
                    float level = MathF.Floor(density * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.7f) / 3;
                    if (level > 0) color = color.Mixed(P.Hex(0x4F7F9C), level * (0.35f + glow * 0.35f));
                }
                c[x, y] = color;
            }
        }

        // The Milky Way: a diagonal band of faint stars.
        for (int i = 0; i < 420; i++)
        {
            float u = rng.Range(0f, 1f);
            float along = rng.Range(-1f, 1f);
            float across = (rng.Range(-1f, 1f) + rng.Range(-1f, 1f)) * 12;
            int x = (int)(Width * (0.1f + 0.8f * u) + across * 0.55f), y = (int)(Height * (0.9f - 0.8f * u) + along * 6 + across * 0.8f);
            if (!(c.Contains(x, y) && Hypot(x - Moon.x, y - Moon.y) > 16)) continue;
            c[x, y] = c[x, y].Mixed(P.Hex(0x9DB8D8), rng.Range(0.3f, 0.6f));
        }

        // Stars.
        P[] starColors = { P.Hex(0xFFFFFF), P.Hex(0xAFC8FF), P.Hex(0xFFE9B0) };
        for (int i = 0; i < 95; i++)
        {
            int x = rng.Range(1, Width - 2), y = rng.Range(1, Height - 2);
            if (!(Hypot(x - Moon.x, y - Moon.y) > 15)) continue;
            P color = starColors[rng.Range(0, starColors.Length - 1)];
            c[x, y] = color;
            if (i % 8 == 0) // a few bright ones with a cross
            {
                foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    c[x + dx, y + dy] = color.Mixed(c[x + dx, y + dy], 0.55f);
            }
        }

        DrawMoon(c);
        return c;
    }

    private static void DrawMoon(PixelCanvas c)
    {
        P cream = P.Hex(0xF6F2D2), shade = P.Hex(0xCBC7A2), crater = P.Hex(0xB4B08E);
        (float, float, float)[] craters = { (-2.5f, -2, 1.7f), (2.2f, 1.0f, 2.1f), (-1, 3.2f, 1.3f), (3.5f, -3, 1.2f), (-4.5f, 1, 1.1f) };
        float r = MoonRadius;
        for (int dy = -10; dy <= 10; dy++)
        {
            for (int dx = -10; dx <= 10; dx++)
            {
                float fx = dx + 0.5f, fy = dy + 0.5f;
                float d = Hypot(fx, fy);
                if (!(d <= r)) continue;
                P color = cream;
                // Shadowed lower-right limb.
                if ((fx + fy) / (2 * r) > 0.28f + Bayer.Threshold(dx + 20, dy + 20) * 0.14f) color = shade;
                if (d > r - 1.1f) color = color.Mixed(shade, 0.45f);
                foreach (var (cx, cy, cr) in craters) if (Hypot(fx - cx, fy - cy) < cr) color = crater;
                c[Moon.x + dx, Moon.y + dy] = color;
            }
        }
    }
}
