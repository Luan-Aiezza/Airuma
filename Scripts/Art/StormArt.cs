using System;
using P = PixelColor;

/// <summary>The sky of a stormy night, as the water reflects it: not a star, not a moon, only thick dark clouds, in heavy layers of grey and blue.</summary>
public static class StormArt
{
    public static PixelCanvas MakeSky()
    {
        var rng = new SeededGenerator(0x57A3);
        var big = new PeriodicNoise(4, 5, rng);
        var small = new PeriodicNoise(11, 12, rng);
        int w = SkyArt.Width, h = SkyArt.Height;
        var c = new PixelCanvas(w, h);
        P top = P.Hex(0x04060A), bottom = P.Hex(0x121A26);
        P[] clouds = { P.Hex(0x0A1018), P.Hex(0x131C28), P.Hex(0x1E2A3A), P.Hex(0x2C3C50) };
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n = big.Sample((float)x / w, (float)y / h) * 0.7f + small.Sample((float)x / w, (float)y / h) * 0.3f;
                float t = Math.Clamp(n * 1.1f + (float)y / h * 0.25f, 0, 0.99f);
                var colour = top.Mixed(bottom, (float)y / h);
                float f = t * clouds.Length + (Bayer.Threshold(x, y) - 0.5f) * 0.9f;
                var cloud = clouds[Math.Clamp((int)Math.Floor(f), 0, clouds.Length - 1)];
                c[x, y] = colour.Mixed(cloud, 0.55f + 0.4f * t);
            }
        return c;
    }

    /// <summary>Where the moon is in the clearing: the middle of the water.</summary>
    public static readonly (int x, int y) Moon = (SkyArt.Width / 2, SkyArt.Height / 2);
    public const float MoonRadius = 12;

    /// <summary>
    /// The sky of the boss fight: the same heavy clouds, but they have parted a little round the middle, and the moon shines in the gap, with a halo and a few
    /// stars. Seen in the water, the moon is in the middle of the picture (the water's glitter goes up and down from it).
    /// </summary>
    public static PixelCanvas MakeClearSky()
    {
        var c = MakeSky();
        var rng = new SeededGenerator(0xC1EA);
        int w = c.Width, h = c.Height;
        var wisps = new PeriodicNoise(6, 5, rng);
        P night = P.Hex(0x0A1630), deep = P.Hex(0x142A4E), halo = P.Hex(0x4C78A8);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = (x - Moon.x) / 58f, dy = (y - Moon.y) / 50f;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                // the gap: wide in the middle, ragged at its edge (the clouds are thin there)
                float ragged = (wisps.Sample((float)x / w, (float)y / h) - 0.5f) * 0.5f;
                float open = Math.Clamp((1 - d + ragged) * 2.2f, 0, 1);
                float band = MathF.Floor(open * 4 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / 4;
                if (band <= 0) continue;
                var sky = night.Mixed(deep, Math.Clamp(1 - d, 0, 1));
                // the halo round the moon, in flat bands
                float hd = MathF.Sqrt(MathF.Pow(x - Moon.x, 2) + MathF.Pow(y - Moon.y, 2));
                float glow = MathF.Pow(MathF.Max(0, 1 - hd / 46), 1.6f);
                float hb = MathF.Floor(glow * 5 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / 5;
                if (hb > 0) sky = sky.Mixed(halo, MathF.Min(1, hb * 1.1f));
                c[x, y] = c[x, y].Mixed(sky, Math.Min(1, band * 1.15f));
            }
        // a few faint stars in the gap
        for (int i = 0; i < 26; i++)
        {
            int x = rng.Range(Moon.x - 40, Moon.x + 40), y = rng.Range(Moon.y - 38, Moon.y + 38);
            float dx = (x - Moon.x) / 44f, dy = (y - Moon.y) / 40f;
            if (dx * dx + dy * dy > 1 || MathF.Sqrt(MathF.Pow(x - Moon.x, 2) + MathF.Pow(y - Moon.y, 2)) < 15 || !c.Contains(x, y)) continue;
            c[x, y] = P.Hex(0xBFD2F0);
        }
        DrawMoon(c);
        return c;
    }

    private static void DrawMoon(PixelCanvas c)
    {
        P cream = P.Hex(0xF2F0D8), shade = P.Hex(0xC4C2A6), crater = P.Hex(0xADAB92);
        (float, float, float)[] craters = { (-2.5f, -2, 1.7f), (2.2f, 1.0f, 2.1f), (-1, 3.2f, 1.3f), (3.5f, -3, 1.2f), (-4.5f, 1, 1.1f) };
        for (int dy = -14; dy <= 14; dy++)
            for (int dx = -14; dx <= 14; dx++)
            {
                float fx = dx + 0.5f, fy = dy + 0.5f, d = MathF.Sqrt(fx * fx + fy * fy);
                if (d > MoonRadius) continue;
                P col = cream;
                if ((fx + fy) / (2 * MoonRadius) > 0.28f + Bayer.Threshold(dx + 20, dy + 20) * 0.14f) col = shade;
                if (d > MoonRadius - 1.1f) col = col.Mixed(shade, 0.45f);
                foreach (var (cx, cy, cr) in craters) if (MathF.Sqrt((fx - cx) * (fx - cx) + (fy - cy) * (fy - cy)) < cr) col = crater;
                if (c.Contains(Moon.x + dx, Moon.y + dy)) c[Moon.x + dx, Moon.y + dy] = col;
            }
    }
}
