using System;
using P = PixelColor;

/// <summary>
/// The Andes at night, behind the lake at the top of the waterfall (the boss fight of stage 5): the strip of sky that reaches up from the horizon (the bottom row), with
/// a range of high mountains along it, far ones pale and blue and near ones dark, all with snow on their peaks, lit by the moon (which is high above them, and a little to
/// the right), and stars. It has the size of <see cref="HorizonSkyArt"/>, so that the river builds either the same way; the lake mirrors it.
/// </summary>
public static class AndesArt
{
    public static PixelCanvas MakeHorizon()
    {
        var rng = new SeededGenerator(0xA9DE5);
        int w = HorizonSkyArt.Width, h = HorizonSkyArt.Height;
        var c = new PixelCanvas(w, h);
        P top = P.Hex(0x04081A), mid = P.Hex(0x10284A), haze = P.Hex(0x6A98B6);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float t = (float)y / (h - 1);
                var colour = top.Mixed(mid, MathF.Min(1, t * 1.3f));
                float band = MathF.Floor(MathF.Pow(t, 3.2f) * 6 + (Bayer.Threshold(x, y) - 0.5f) * 0.9f) / 6;
                if (band > 0) colour = colour.Mixed(haze, MathF.Min(1, band * 1.2f));
                c[x, y] = colour;
            }
        // stars
        for (int i = 0; i < 110; i++)
        {
            int x = rng.Range(1, w - 2), y = rng.Range(0, h - 18);
            if (rng.NextFloat() > 1.1f - (float)y / h) continue;
            c[x, y] = i % 4 == 0 ? P.Hex(0xFFE9B0) : (i % 3 == 0 ? P.Hex(0xAFC8FF) : P.Hex(0xFFFFFF));
        }
        // the moon, high, with a halo
        const int mx = 150, my = 11;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float d = MathF.Sqrt((x - mx) * (x - mx) + (y - my) * (y - my));
                float glow = MathF.Pow(MathF.Max(0, 1 - d / 26), 1.7f);
                float gb = MathF.Floor(glow * 5 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / 5;
                if (gb > 0) c[x, y] = c[x, y].Mixed(P.Hex(0x9CC4D8), MathF.Min(1, gb * 0.8f));
            }
        for (int y = -8; y <= 8; y++)
            for (int x = -8; x <= 8; x++)
            {
                float d = MathF.Sqrt((x + 0.5f) * (x + 0.5f) + (y + 0.5f) * (y + 0.5f));
                if (d > 7.5f || !c.Contains(mx + x, my + y)) continue;
                var col = P.Hex(0xF2F0D8);
                if ((x + y) / 15f > 0.28f + Bayer.Threshold(x + 20, y + 20) * 0.14f) col = P.Hex(0xC4C2A6);
                if ((x == -2 && y == -2) || (x == 2 && y == 1) || (x == -1 && y == 3)) col = P.Hex(0xADAB92);
                c[mx + x, my + y] = col;
            }
        // The mountains: two ranges of peaks, the far one pale and blue, the near one dark. A range is the highest of a number of tents (a peak and its two slopes); snow lies
        // on the top part of each peak, ragged at its edge, and the slope that faces the moon (the right one) is lit.
        void Range(float seed, int peaks, float lowest, float highest, float slopeLo, float slopeHi, P rock, P rockLit, P snow, P snowShade, float snowShare)
        {
            var r = new SeededGenerator((ulong)(seed * 1000));
            var px = new float[peaks]; var ph = new float[peaks]; var sl = new float[peaks];
            for (int i = 0; i < peaks; i++) { px[i] = (i + r.Range(0.1f, 0.9f)) * w / peaks; ph[i] = r.Range(lowest, highest); sl[i] = r.Range(slopeLo, slopeHi); }
            for (int x = 0; x < w; x++)
            {
                float best = -1000; int who = 0;
                for (int i = 0; i < peaks; i++) { float v = ph[i] - sl[i] * MathF.Abs(x - px[i]) + 1.2f * MathF.Sin(x * 0.9f + i); if (v > best) { best = v; who = i; } }
                int top = h - 1 - (int)MathF.Max(0, best);
                for (int y = Math.Max(0, top); y < h; y++)
                {
                    float fromTop = y - top;
                    bool snowy = fromTop < ph[who] * snowShare + 2.5f * MathF.Sin(x * 0.8f + y * 0.5f + seed) && best > 6;
                    bool lit = x > px[who] ? true : (Bayer.Threshold(x, y) < 0.3f);
                    c[x, y] = snowy ? (lit ? snow : snowShade) : (lit ? rockLit : rock);
                }
            }
        }
        Range(1.3f, 7, 22, 40, 0.9f, 1.5f, P.Hex(0x2C4468), P.Hex(0x446490), P.Hex(0xE6F2FF), P.Hex(0xA8C4E4), 0.5f);
        Range(4.7f, 9, 9, 22, 0.8f, 1.3f, P.Hex(0x101C30), P.Hex(0x1C2E4C), P.Hex(0xBFD4F0), P.Hex(0x7C98BE), 0.28f);
        // the haze on the foot of the mountains
        for (int y = h - 6; y < h; y++)
            for (int x = 0; x < w; x++)
                if (Bayer.Threshold(x, y) < (y - (h - 7)) / 6f * 0.8f) c[x, y] = c[x, y].Mixed(haze, 0.55f);
        return c;
    }
}
