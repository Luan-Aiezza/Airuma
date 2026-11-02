using System;
using P = PixelColor;

/// <summary>
/// The sky of the river of stars (stage 6), as the water reflects it, in the manner of the sky of stage 1 but cosmic: a deep violet gradient with thin clouds of nebula, a band of the
/// Milky Way, a few small planets, and few stars. Nothing is drawn directly: it lives in the reflection (and is distorted by the water). Its bed (<see cref="MakeBed"/>), seen faintly
/// under the water, is dark space with dust and dim stars.
/// </summary>
public static class CosmosArt
{
    private static float Hypot(float a, float b) => MathF.Sqrt(a * a + b * b);

    public static PixelCanvas MakeSky()
    {
        int w = SkyArt.Width, h = SkyArt.Height;
        var rng = new SeededGenerator(0xC05A);
        var clouds = new PeriodicNoise(5, 6, rng);
        var dust = new PeriodicNoise(9, 11, rng);
        var c = new PixelCanvas(w, h);
        P top = P.Hex(0x040616), bottom = P.Hex(0x16103C);
        P[] milk = { P.Hex(0x1E1850), P.Hex(0x3A2A7A), P.Hex(0x5A3A94), P.Hex(0x4A6AB0), P.Hex(0xA8C0E8) };
        // the band of the Milky Way runs from the lower left to the upper right
        float x0 = w * 0.05f, y0 = h * 0.92f, x1 = w * 0.95f, y1 = h * 0.12f;
        float lx = x1 - x0, ly = y1 - y0, len = Hypot(lx, ly);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float t = (float)y / h;
                P colour = top.Mixed(bottom, t);
                float n = clouds.Sample((float)x / w, (float)y / h);
                // thin clouds of nebula over all the sky
                if (n > 0.62f)
                {
                    float density = MathF.Min(1, (n - 0.62f) * 5);
                    float level = MathF.Floor(density * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.7f) / 3;
                    if (level > 0) colour = colour.Mixed(P.Hex(0x2A2468), level * 0.55f);
                }
                // the Milky Way: a soft band, with a lane of dust across it
                float across = ((x - x0) * ly - (y - y0) * lx) / len;       // signed distance to the band's axis
                float core = MathF.Exp(-(across * across) / (2 * 15f * 15f));
                float grain = dust.Sample((float)x / w, (float)y / h);
                float density2 = core * (0.35f + 0.9f * grain);
                float lane = MathF.Max(0, 1 - MathF.Abs(across + 5f) / 3.2f) * (grain > 0.45f ? 0.6f : 0.2f);
                density2 *= 1 - lane;
                float f = density2 * 4.4f + (Bayer.Threshold(x, y) - 0.5f) * 0.9f;
                int band = (int)MathF.Floor(f);
                if (band >= 0) colour = colour.Mixed(milk[Math.Clamp(band, 0, milk.Length - 1)], band == 0 ? 0.35f : 0.8f);
                c[x, y] = colour;
            }

        // fine stars in the band, and a few in the rest of the sky
        for (int i = 0; i < 260; i++)
        {
            float u = rng.Range(0f, 1f);
            float acr = (rng.Range(-1f, 1f) + rng.Range(-1f, 1f)) * 13f;
            int x = (int)(x0 + lx * u - ly / len * acr), y = (int)(y0 + ly * u + lx / len * acr);
            if (c.Contains(x, y)) c[x, y] = c[x, y].Mixed(P.Hex(0xB8CCF0), rng.Range(0.3f, 0.6f));
        }
        P[] starColours = { P.Hex(0xFFFFFF), P.Hex(0xAFC8FF), P.Hex(0xFFE9B0) };
        for (int i = 0; i < 40; i++)
        {
            int x = rng.Range(1, w - 2), y = rng.Range(1, h - 2);
            var colour = starColours[rng.Range(0, starColours.Length - 1)];
            c[x, y] = colour;
            if (i % 5 == 0) foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) c[x + dx, y + dy] = colour.Mixed(P.Hex(0x16103C), 0.55f);
        }

        // a few small planets, far off (they are reflected, so the water makes them tremble): a blue giant with bands, a red one, a ringed gold one, a small pale one
        Planet(c, w * 0.2f, h * 0.22f, 6.5f, P.Hex(0x58A0C8), P.Hex(0x0A1838), 1f, 0f, 0f);
        Planet(c, w * 0.8f, h * 0.4f, 4f, P.Hex(0xC8607A), P.Hex(0x2A0C28), 0f, 0f, 0f);
        // Saturn: only a part of it is in the picture, in the lower right corner, with its ring pointing at the middle of the scene
        Planet(c, w * 0.965f, h * 0.93f, 9f, P.Hex(0xE0B868), P.Hex(0x34180C), 0.6f, 1f, 0.85f);
        Planet(c, w * 0.72f, h * 0.83f, 2.8f, P.Hex(0xC8D8F0), P.Hex(0x303858), 0f, 0f, 0f);
        return c;
    }

    /// <summary>A lit sphere in flat dithered bands (the light from the upper left), with bands of cloud if it is a giant and a ring if it has one.</summary>
    private static void Planet(PixelCanvas c, float cx, float cy, float r, P baseColour, P dark, float clouds, float ring, float tilt)
    {
        float cosT = MathF.Cos(tilt), sinT = MathF.Sin(tilt);
        for (int y = (int)(cy - r * 2.6f); y <= (int)(cy + r * 2.6f); y++)
            for (int x = (int)(cx - r * 2.6f); x <= (int)(cx + r * 2.6f); x++)
            {
                if (!c.Contains(x, y)) continue;
                float qx = x + 0.5f - cx, qy = y + 0.5f - cy;
                float dx = qx / r, dy = qy / r, d2 = dx * dx + dy * dy;
                float rx = qx * cosT + qy * sinT, ry = -qx * sinT + qy * cosT;
                float rr = ring > 0 ? Hypot(rx / (r * 1.9f), ry / (r * 0.45f)) : 0;
                bool onRing = ring > 0 && MathF.Abs(rr - 1f) < 0.14f;
                bool front = ry >= 0;
                if (onRing && !front && d2 >= 1) { c[x, y] = baseColour.Mixed(P.Hex(0xDCE6FF), 0.4f).Scaled(0.6f); continue; }
                if (d2 < 1)
                {
                    float nz = MathF.Sqrt(1 - d2);
                    float lit = MathF.Max(0, (-0.55f * dx - 0.6f * dy + 0.55f * nz) / 0.98f);
                    float stripe = clouds * (0.5f + 0.5f * MathF.Sin(dy * 7f + MathF.Sin(dx * 3f) * 0.8f));
                    float level = MathF.Floor(lit * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f + stripe * 0.5f);
                    level = Math.Clamp(level, 0, 3);
                    var col = dark.Mixed(baseColour, level / 3f);
                    if (d2 > 0.82f && lit > 0.35f) col = col.Mixed(P.Hex(0xB0D8FF), 0.35f);
                    if (onRing && front) col = baseColour.Mixed(P.Hex(0xDCE6FF), 0.55f);
                    c[x, y] = col;
                }
                else if (onRing && front) c[x, y] = baseColour.Mixed(P.Hex(0xDCE6FF), 0.55f).Scaled(0.65f + 0.35f * (MathF.Floor(rr * 9f) % 2 == 0 ? 1 : 0));
            }
    }

    /// <summary>The bed under the water: dark space with dust and dim stars, as wide as the river and tall enough to scroll (it repeats).</summary>
    public static PixelCanvas MakeBed()
    {
        int w = BedArt.Width, h = BedArt.Height;
        var rng = new SeededGenerator(0xBED6);
        var dust = new PeriodicNoise(12, 10, rng);
        P[] ramp = { P.Hex(0x07061A), P.Hex(0x0E0B2C), P.Hex(0x181246), P.Hex(0x2A1E66) };
        var c = new PixelCanvas(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float n = dust.Sample((float)x / w, (float)y / h);
                float f = n * 3.4f + (Bayer.Threshold(x, y) - 0.5f) * 0.9f;
                c[x, y] = ramp[Math.Clamp((int)MathF.Floor(f), 0, ramp.Length - 1)];
            }
        for (int i = 0; i < 70; i++) c[rng.Range(0, w - 1), rng.Range(0, h - 1)] = P.Hex(0x7A8AC8);
        return c;
    }

    // MARK: The sky of the fight

    /// <summary>
    /// The sky of the boss fight over the open water, like the one of stage 1 (see <see cref="HorizonSkyArt"/>: the strip of night that reaches up from the horizon, its bottom row) but of the
    /// sky of this stage: a violet haze along the horizon, a band of the Milky Way, a few stars, and a GREAT Moon in the middle, sitting on the horizon with only its upper half showing; the
    /// water below shows this same picture flipped, so its lower half is there, in the reflection.
    /// </summary>
    public static PixelCanvas MakeHorizon() => MakeHorizon(false);

    /// <summary>The same sky with the Moon (and its glow and the haze) gone red: it is faded in over the other when the boss enters her second form.</summary>
    public static PixelCanvas MakeHorizonRed() => MakeHorizon(true);

    private static PixelCanvas MakeHorizon(bool red)
    {
        int w = HorizonSkyArt.Width, h = HorizonSkyArt.Height;
        var rng = new SeededGenerator(0xF1A9);
        var clouds = new PeriodicNoise(9, 3, rng);
        var c = new PixelCanvas(w, h);
        P top = P.Hex(0x040616), mid = P.Hex(0x1A1250), haze = P.Hex(red ? 0xB0506Au : 0x8A68B8u), glowCol = P.Hex(red ? 0xE8604Au : 0xB8C8F0u);
        float mx = w / 2f, my = h;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float t = (float)y / (h - 1);
                P colour = top.Mixed(mid, MathF.Min(1f, t * 1.3f));
                float hz = MathF.Pow(t, 3.2f);
                float band = MathF.Floor(hz * 6 + (Bayer.Threshold(x, y) - 0.5f) * 0.9f) / 6;
                if (band > 0) colour = colour.Mixed(haze, MathF.Min(1f, band * 1.2f));
                // the great glow of the Moon
                float d = Hypot(x - mx, y - my);
                float glow = MathF.Pow(MathF.Max(0, 1 - d / (MoonRadius + 30)), 1.6f);
                float gband = MathF.Floor(glow * 5 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / 5;
                if (gband > 0) colour = colour.Mixed(glowCol, MathF.Min(1f, gband * 0.8f));
                float n = clouds.Sample((float)x / w, (float)y / h);
                if (n > 0.62f && t > 0.35f)
                {
                    float density = MathF.Min(1, (n - 0.62f) * 6);
                    float level = MathF.Floor(density * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.7f) / 3;
                    if (level > 0) colour = colour.Mixed(P.Hex(0x2E2468), level * 0.7f);
                }
                c[x, y] = colour;
            }
        // a band of the Milky Way across the upper part, and stars (fewer in the haze, none near the Moon)
        for (int i = 0; i < 200; i++)
        {
            float u = rng.Range(0f, 1f);
            float across = (rng.Range(-1f, 1f) + rng.Range(-1f, 1f)) * 8;
            int x = (int)(w * (0.02f + 0.96f * u) + across * 0.6f), y = (int)(h * (0.55f - 0.4f * u) + across * 0.45f);
            if (!c.Contains(x, y) || y > h - 8 || Hypot(x - mx, y - my) < MoonRadius + 6) continue;
            c[x, y] = c[x, y].Mixed(P.Hex(0xB8CCF0), rng.Range(0.25f, 0.5f));
        }
        P[] starColours = { P.Hex(0xFFFFFF), P.Hex(0xAFC8FF), P.Hex(0xFFE9B0) };
        for (int i = 0; i < 46; i++)
        {
            int x = rng.Range(1, w - 2), y = rng.Range(0, h - 10);
            if (Hypot(x - mx, y - my) < MoonRadius + 8) continue;
            if (rng.NextFloat() > 1.15f - (float)y / h) continue;
            var colour = starColours[rng.Range(0, starColours.Length - 1)];
            c[x, y] = colour;
        }
        DrawMoon(c, mx, my, red);
        return c;
    }

    /// <summary>The Moon is as high as this many cells from its centre: a great disc, much bigger than the moon of stage 1 (13).</summary>
    public const float MoonRadius = 38;

    private static void DrawMoon(PixelCanvas c, float mx, float my, bool red)
    {
        P bright = P.Hex(red ? 0xFFC8B0u : 0xFAFBFFu), silver = P.Hex(red ? 0xE8826Au : 0xD6DCEEu), shade = P.Hex(red ? 0xA8403Cu : 0xA4AECCu), mare = P.Hex(red ? 0x7C2A36u : 0x8892B4u), crater = P.Hex(red ? 0x903438u : 0x9AA4C4u);
        // the dark seas and the craters, in units of the radius from the centre (the lower half is below the picture and shows in the water)
        (float x, float y, float r)[] seas = { (-0.35f, -0.45f, 0.3f), (0.18f, -0.6f, 0.24f), (0.42f, -0.22f, 0.2f), (-0.08f, -0.2f, 0.2f), (-0.58f, -0.12f, 0.16f) };
        (float x, float y, float r)[] craters = { (0.55f, -0.62f, 0.08f), (-0.62f, -0.55f, 0.07f), (0.05f, -0.82f, 0.06f), (0.3f, -0.4f, 0.06f), (-0.25f, -0.72f, 0.05f), (0.7f, -0.3f, 0.05f), (-0.45f, -0.3f, 0.05f) };
        float r = MoonRadius;
        for (int dy = (int)-r - 1; dy <= 0; dy++)
            for (int dx = (int)-r - 1; dx <= (int)r + 1; dx++)
            {
                float fx = dx + 0.5f, fy = dy + 0.5f;
                float d = Hypot(fx, fy);
                if (d > r) continue;
                float ux = fx / r, uy = fy / r;
                // lit from the upper left: bright there, shaded towards the lower right, in dithered steps
                float lit = 1 - (ux * 0.55f - uy * 0.45f + 1) / 2;
                P colour = lit > 0.62f + (Bayer.Threshold(dx + 64, dy + 64) - 0.5f) * 0.2f ? bright : (lit > 0.3f + (Bayer.Threshold(dx + 64, dy + 64) - 0.5f) * 0.2f ? silver : shade);
                foreach (var s in seas) if (Hypot(ux - s.x, uy - s.y) < s.r * (0.85f + 0.15f * Bayer.Threshold(dx + 9, dy + 9))) colour = colour.Mixed(mare, 0.7f);
                foreach (var k in craters)
                {
                    float kd = Hypot(ux - k.x, uy - k.y);
                    if (kd < k.r) colour = kd > k.r * 0.7f ? bright : crater;
                }
                if (d > r - 1.2f) colour = colour.Mixed(shade, 0.35f);
                int px = (int)(mx + dx), py = (int)(my + dy);
                if (c.Contains(px, py)) c[px, py] = colour;
            }
    }

    /// <summary>The high sky under the reflection of the horizon: the same deep violet-black as the top of the horizon sky, with the same kind of stars and a faint Milky Way.</summary>
    public static PixelCanvas MakeDeep()
    {
        int w = HorizonSkyArt.Width, h = 110;
        var rng = new SeededGenerator(0xDEE6);
        var c = new PixelCanvas(w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++) c[x, y] = P.Hex(0x040616).Mixed(P.Hex(0x0A0A28), (float)y / h * 0.6f);
        for (int i = 0; i < 260; i++)
        {
            float u = rng.Range(0f, 1f);
            float across = (rng.Range(-1f, 1f) + rng.Range(-1f, 1f)) * 9;
            int x = (int)(w * u + across * 0.6f), y = (int)(h * (0.15f + 0.6f * u) + across * 0.5f);
            if (c.Contains(x, y)) c[x, y] = c[x, y].Mixed(P.Hex(0xB8CCF0), rng.Range(0.22f, 0.45f));
        }
        P[] starColours = { P.Hex(0xFFFFFF), P.Hex(0xAFC8FF), P.Hex(0xFFE9B0) };
        for (int i = 0; i < 70; i++) c[rng.Range(1, w - 2), rng.Range(0, h - 2)] = starColours[rng.Range(0, starColours.Length - 1)];
        return c;
    }
}
