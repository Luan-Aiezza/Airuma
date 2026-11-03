using System;
using System.Collections.Generic;
using Godot;
using P = PixelColor;

/// <summary>
/// The picture of the end of the story: a sunrise, all in gold and in shadow. The sun rises over the Andes, the river holds its light; on the bank, at the right and far off, two small silhouettes sit side by side
/// and watch it, a boy who holds up his lantern and the priestess with her long hair; at the left, against the bank, the shadow of his boat with its sail, moored. The top of the sky is in shadow, for the writing; the foot is in shadow too, and there goes the word FIM. The
/// picture is 222 x 155 art pixels (shown at 5 points each).
/// </summary>
public static class EndArt
{
    public const int Width = 222, Height = 155;
    private const int Horizon = 104, SunX = 104;

    private static readonly P Shadow = P.Hex(0x110904), ShadowSoft = P.Hex(0x2A1807);
    private static readonly P[] Sky = { P.Hex(0x1A0E06), P.Hex(0x3C1F08), P.Hex(0x7C4410), P.Hex(0xC27A18), P.Hex(0xF2B232), P.Hex(0xFFD978), P.Hex(0xFFF1C4) };
    private static readonly P Rim = P.Hex(0xE09A24), RimBright = P.Hex(0xFFD060);

    private static int Hash(int a, int b = 0, int c = 0)
    {
        unchecked { long v = ((long)a * 73_856_093L) ^ ((long)b * 19_349_663L) ^ ((long)c * 83_492_791L); return (int)(Math.Abs(v) % 1000); }
    }

    private static P Pick(P[] ramp, float t, int x, int y)
    {
        t = Math.Clamp(t, 0, 1) * (ramp.Length - 1);
        int i = (int)MathF.Floor(t);
        float f = t - i;
        return i >= ramp.Length - 1 ? ramp[^1] : (f > Bayer.Threshold(x, y) ? ramp[i + 1] : ramp[i]);
    }

    public static PixelCanvas MakeScene()
    {
        var c = new PixelCanvas(Width, Height);
        DrawSky(c);
        DrawMountains(c);
        DrawWater(c);
        var sil = new PixelCanvas(Width, Height);
        DrawBanks(sil);
        DrawBoat(sil);
        DrawSitters(sil);
        // the silhouettes over the picture, lit with gold on the edges that face the sun
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                if (sil[x, y].A == 0) continue;
                c[x, y] = sil[x, y];
            }
        for (int y = 1; y < Height; y++)
            for (int x = 1; x < Width - 1; x++)
            {
                if (sil[x, y].A == 0) continue;
                int toward = SunX > x ? 1 : -1;
                bool sunSide = sil[x + toward, y].A == 0, top = sil[x, y - 1].A == 0;
                if (y > Horizon + 30) continue;
                if (sunSide && top) c[x, y] = RimBright;
                else if (sunSide || top) c[x, y] = y < Horizon ? Rim : Rim.Scaled(0.75f);
            }
        DrawLantern(c);
        // the shadow at the top, for the writing (the sky there is already the darkest), and a lighter one at the foot
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                float top = y < 70 ? 0.45f * (1 - y / 70f) : 0;
                float foot = y > 124 ? 0.62f * Math.Clamp((y - 124) / 24f, 0, 1) : 0;
                if (top + foot > 0) c[x, y] = c[x, y].Scaled(1 - top - foot);
            }
        return c;
    }

    private static void DrawSky(PixelCanvas c)
    {
        for (int y = 0; y < Horizon; y++)
            for (int x = 0; x < Width; x++)
            {
                float t = MathF.Pow(y / (float)Horizon, 1.7f);
                float dx = x - SunX, dy = y - Horizon;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                // the glow of the sun, and long slanting rays of light, in flat steps
                float glow = MathF.Pow(MathF.Max(0, 1 - d / 70f), 1.5f);
                float ang = MathF.Atan2(dy, dx);
                float rays = d > 16 ? 0.10f * MathF.Max(0, MathF.Sin(ang * 15f)) * MathF.Max(0, 1 - d / 95f) : 0;
                c[x, y] = Pick(Sky, t + glow * 0.9f + rays, x, y);
            }
        // the sun: a bright disc half risen
        for (int y = Horizon - 20; y <= Horizon; y++)
            for (int x = SunX - 20; x <= SunX + 20; x++)
            {
                float d = MathF.Sqrt((x - SunX) * (x - SunX) + (y - Horizon) * (y - Horizon));
                if (d <= 15) c[x, y] = d <= 11 ? P.Hex(0xFFFAE0) : (d <= 13 ? P.Hex(0xFFF1C4) : P.Hex(0xFFDF86));
            }
    }

    private static void DrawMountains(PixelCanvas c)
    {
        for (int x = 0; x < Width; x++)
        {
            // the far range, hazy gold; the near one, dark
            int far = Horizon - (int)((7 + 8 * MathF.Abs(MathF.Sin(x * 0.045f + 1f)) + 3 * MathF.Sin(x * 0.19f)) * (x > 130 ? Math.Clamp((215 - x) / 85f, 0.3f, 1f) : 1f));
            for (int y = far; y < Horizon; y++) c[x, y] = c[x, y].Mixed(P.Hex(0x7A4A14), 0.78f);
            // (the near range is low where the two watchers sit, so that they stand against the light of the sky)
            float low = x > 130 ? Math.Clamp((215 - x) / 85f, 0.12f, 1f) : 1f;
            int near = Horizon - (int)((3 + 11 * MathF.Pow(MathF.Abs(MathF.Sin(x * 0.028f + 2.3f)), 0.9f) + 2 * MathF.Sin(x * 0.27f)) * low);
            for (int y = near; y < Horizon; y++) c[x, y] = (x + y) % 5 == 0 ? ShadowSoft : P.Hex(0x1E1006);
        }
    }

    private static void DrawWater(PixelCanvas c)
    {
        for (int y = Horizon; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                float t = (y - Horizon) / (float)(Height - Horizon);
                // the river holds the gold of the sky, darker as it comes near; the sun's road under the sun, in broken strokes
                var water = new[] { P.Hex(0xFFD978), P.Hex(0xE8A030), P.Hex(0xA8681A), P.Hex(0x5A3410), P.Hex(0x2A1807) };
                float v = MathF.Pow(t, 0.75f);
                var col = Pick(water, v, x, y);
                float road = 1 - MathF.Abs(x - SunX) / (7 + (y - Horizon) * 0.45f);
                if (road > 0 && Hash(x / 2, y, 3) % 5 < 3 && (y + x / 3) % 2 == 0) col = Pick(water, v - 0.45f * road, x, y);
                // ripples: short dark and light strokes
                int h = Hash(x / 4, y, 9) % 11;
                if (h == 0) col = col.Scaled(0.72f);
                else if (h == 1) col = col.Mixed(P.Hex(0xFFE9A8), 0.35f);
                c[x, y] = col;
            }
    }

    // MARK: Silhouettes

    private static void Poly(PixelCanvas c, P colour, params (float x, float y)[] pts)
    {
        var list = new List<Vector2>(); foreach (var (x, y) in pts) list.Add(new Vector2(x, y));
        PixelPaint.Poly(c, list, colour);
    }

    private static void Line(PixelCanvas c, int x0, int y0, int x1, int y1, P colour)
    {
        int dx = Math.Abs(x1 - x0), dy = -Math.Abs(y1 - y0), sx = x0 < x1 ? 1 : -1, sy = y0 < y1 ? 1 : -1, err = dx + dy;
        while (true)
        {
            if (c.Contains(x0, y0)) c[x0, y0] = colour;
            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 >= dy) { err += dy; x0 += sx; }
            if (e2 <= dx) { err += dx; y0 += sy; }
        }
    }

    /// <summary>The two banks of the river: at the right, a bank that rises towards the corner, with tufts of grass and reeds on its edge; at the left, a low one.</summary>
    private static void DrawBanks(PixelCanvas sil)
    {
        Poly(sil, Shadow, (112, 156), (126, 130), (146, 122), (168, 118), (188, 115), (210, 112), (223, 110), (223, 156));
        Poly(sil, Shadow, (-1, 156), (-1, 117), (12, 118), (28, 123), (40, 133), (58, 156));
        // tufts of grass and reeds along the top of the right bank
        for (int x = 128; x < 222; x += 3)
        {
            int top = BankTop(x);
            int h = 2 + Hash(x, 1) % 4;
            for (int k = 1; k <= h; k++) { if (sil.Contains(x + k / 3, top - k)) sil[x + k / 3, top - k] = Shadow; }
        }
        foreach (int x in new[] { 134, 141, 150, 214 }) Line(sil, x, BankTop(x), x + 2, BankTop(x) - 12, Shadow);
        for (int x = 2; x < 36; x += 3) { int top = LeftTop(x); int h = 2 + Hash(x, 2) % 3; for (int k = 1; k <= h; k++) if (sil.Contains(x, top - k)) sil[x, top - k] = Shadow; }
    }

    private static int BankTop(int x)
    {
        // the top edge of the right bank, as drawn above
        var pts = new[] { (112, 156), (126, 130), (146, 122), (168, 118), (188, 115), (210, 112), (223, 110) };
        for (int i = 0; i + 1 < pts.Length; i++)
            if (x >= pts[i].Item1 && x <= pts[i + 1].Item1) return pts[i].Item2 + (pts[i + 1].Item2 - pts[i].Item2) * (x - pts[i].Item1) / (pts[i + 1].Item1 - pts[i].Item1);
        return 112;
    }

    private static int LeftTop(int x) => x < 12 ? 117 + x / 12 : (x < 28 ? 118 + (x - 12) * 5 / 16 : 123 + (x - 28) * 10 / 12);

    /// <summary>His boat, moored to the bank at the left: the hull low on the water, the mast, the sail raised and a small one in front of it, and the rope to the shore.</summary>
    private static void DrawBoat(PixelCanvas sil)
    {
        Poly(sil, Shadow, (16, 121), (22, 127), (50, 127), (58, 120), (53, 123), (21, 123));
        Line(sil, 38, 124, 38, 90, Shadow); Line(sil, 39, 124, 39, 90, Shadow);
        Poly(sil, Shadow, (40, 93), (40, 121), (55, 121));                  // the sail
        Poly(sil, Shadow, (36, 99), (36, 121), (24, 121));                  // the little one
        Line(sil, 38, 92, 18, 121, Shadow);                                   // the stay to the bow
        Line(sil, 17, 121, 6, 119, Shadow);                                   // the mooring rope to the shore
    }

    private const int SittersBank = 182;                  // where on the right bank the two sit (the column of the boy's seat)

    /// <summary>The boy and the priestess, small and far, sitting side by side on the right bank, watching the sun: the boy a little hunched, holding up a lantern; she with her long hair down her back.</summary>
    private static void DrawSitters(PixelCanvas sil)
    {
        int b = BankTop(SittersBank) + 1;
        // Atli: the body, the head, the legs stretched forward (towards the sun), the arm that holds up the lantern
        int ax = SittersBank - 8;
        Poly(sil, Shadow, (ax - 3, b - 12), (ax + 4, b - 12), (ax + 7, b), (ax - 4, b));
        PixelPaint.Ellipse(sil, ax, b - 15.5f, 3.3f, 3.6f, Shadow);
        Poly(sil, Shadow, (ax - 4, b - 3), (ax - 13, b - 3), (ax - 13, b), (ax - 4, b));
        Line(sil, ax - 2, b - 9, ax - 10, b - 13, Shadow); Line(sil, ax - 3, b - 9, ax - 10, b - 12, Shadow);
        Line(sil, ax - 10, b - 13, ax - 12, b - 17, Shadow); Line(sil, ax - 11, b - 13, ax - 13, b - 17, Shadow);
        // she, close to him: the head a little towards his shoulder, the long hair
        int bx = SittersBank + 4;
        Poly(sil, Shadow, (bx - 3, b - 12), (bx + 4, b - 12), (bx + 7, b), (bx - 4, b));
        PixelPaint.Ellipse(sil, bx - 0.5f, b - 15.5f, 3.2f, 3.5f, Shadow);
        Poly(sil, Shadow, (bx, b - 19), (bx + 5, b - 17), (bx + 9, b - 3), (bx + 3, b - 4));
        Poly(sil, Shadow, (bx - 4, b - 3), (bx - 11, b - 2), (bx - 11, b), (bx - 4, b));
    }

    /// <summary>The lantern: a small flame of gold in his raised hand, with a halo.</summary>
    private static void DrawLantern(PixelCanvas c)
    {
        int b = BankTop(SittersBank) + 1;
        int lx = SittersBank - 8 - 13, ly = b - 20;
        for (int y = ly - 7; y <= ly + 7; y++)
            for (int x = lx - 7; x <= lx + 7; x++)
            {
                float d = MathF.Sqrt((x - lx) * (x - lx) + (y - ly) * (y - ly));
                if (d < 7 && (x + y) % 2 == 0 && c.Contains(x, y)) c[x, y] = c[x, y].Mixed(P.Hex(0xFFD060), 0.6f * (1 - d / 7));
            }
        for (int dy = -1; dy <= 2; dy++) for (int dx = -1; dx <= 1; dx++) if (c.Contains(lx + dx, ly + dy)) c[lx + dx, ly + dy] = dy == 0 || dy == 1 ? P.Hex(0xFFF6D0) : P.Hex(0xFFC840);
    }
}
