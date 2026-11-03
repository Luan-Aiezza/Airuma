using System;
using System.Collections.Generic;
using Godot;
using P = PixelColor;

/// <summary>
/// The picture of the opening of the game, the night without end, in the manner of <see cref="EndArt"/>: a sky taken by the moon and by countless stars, and below it only darkness. Far off, small, on the black
/// river, the silhouette of the boy under the sail of his boat, his face turned up to the sky, and in his hands the lantern, the only gold in the picture. The picture is 222 x 155 art pixels (shown at 5 points each).
/// </summary>
public static class NightArt
{
    public const int Width = 222, Height = 155;
    private const int Horizon = 98;
    private const int MoonX = 118, MoonY = 38;
    /// <summary>Where the boat is on the river (the foot of its mast), and where the boy's lantern is.</summary>
    private const int BoatX = 172, BoatY = 111;

    private static readonly P Dark = P.Hex(0x020308);
    private static readonly P[] Sky = { P.Hex(0x02030A), P.Hex(0x050A1C), P.Hex(0x0A1634), P.Hex(0x142A56), P.Hex(0x22427A) };
    private static readonly P Gold = P.Hex(0xFFC840), GoldBright = P.Hex(0xFFF3C0), GoldDeep = P.Hex(0xD08818);

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

    public static PixelCanvas MakeScene()
    {
        var c = new PixelCanvas(Width, Height);
        DrawSky(c);
        DrawMoon(c);
        DrawMountains(c);
        DrawRiver(c);
        // the boy and his boat: silhouettes, lit on one side by the moon (cold) and, near the lantern, by the lantern (gold)
        var sil = new PixelCanvas(Width, Height);
        DrawBoat(sil);
        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) if (sil[x, y].A > 0) c[x, y] = Dark;
        for (int y = 1; y < Height; y++)
            for (int x = 1; x < Width - 1; x++)
            {
                if (sil[x, y].A == 0) continue;
                bool moonSide = sil[x - 1, y].A == 0 && x > MoonX, top = sil[x, y - 1].A == 0;
                if (moonSide || top) c[x, y] = P.Hex(0x3A5890);
            }
        DrawLantern(c, sil);
        // the dark at the foot, for the writing
        for (int y = 118; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                float t = Math.Clamp((y - 118) / 24f, 0, 1);
                c[x, y] = c[x, y].Scaled(1 - 0.7f * t * t * (3 - 2 * t));
            }
        return c;
    }

    private static void DrawSky(PixelCanvas c)
    {
        for (int y = 0; y < Horizon + 4; y++)
            for (int x = 0; x < Width; x++)
            {
                float t = MathF.Pow(y / (float)Horizon, 2.1f);
                // a pale haze of the Milky Way across the sky, from the upper right to the lower left, and the glow of the moon
                float across = ((x - 20) * 0.62f + (y - 90) * 0.78f) / 1f;
                float band = MathF.Exp(-(across - 60) * (across - 60) / 2200f);
                float haze = MathF.Max(0, band) * (0.18f + 0.1f * MathF.Sin(x * 0.11f + y * 0.07f));
                float dm = MathF.Sqrt((x - MoonX) * (x - MoonX) + (y - MoonY) * (y - MoonY));
                float glow = MathF.Pow(MathF.Max(0, 1 - dm / 60f), 1.8f);
                float horizonGlow = MathF.Pow(MathF.Max(0, 1 - (Horizon - y) / 22f), 2f) * 0.55f;
                c[x, y] = Pick(Sky, t * 0.6f + haze + glow * 0.75f + horizonGlow, x, y);
            }
        // the stars: very many, of three brightnesses, thicker in the Milky Way; a few have little arms of light
        for (int i = 0; i < 520; i++)
        {
            int x = Hash(i, 1) * Width / 1000, y = Hash(i, 2) * (Horizon - 6) / 1000;
            if (Math.Sqrt((x - MoonX) * (x - MoonX) + (y - MoonY) * (y - MoonY)) < 25) continue;
            int kind = Hash(i, 3) % 10;
            P colour = kind < 6 ? P.Hex(0x7C8CB4) : (kind < 9 ? P.Hex(0xC8D4F4) : P.Hex(0xFFFFFF));
            c[x, y] = colour;
            if (kind == 9) foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) if (c.Contains(x + dx, y + dy)) c[x + dx, y + dy] = colour.Mixed(c[x + dx, y + dy], 0.6f);
        }
        for (int i = 0; i < 380; i++)
        {
            float u = Hash(i, 11) / 1000f, v = (Hash(i, 12) + Hash(i, 13)) / 2000f - 0.5f;
            int x = (int)(20 + u * 190 - v * 42), y = (int)(Horizon - 6 - u * 86 - v * 30);
            if (c.Contains(x, y) && y < Horizon - 2) c[x, y] = P.Hex(0x9AAAD0);
        }
    }

    private static void DrawMoon(PixelCanvas c)
    {
        const float r = 19;
        P[] tone = { P.Hex(0xFFFFFF), P.Hex(0xE8EEFF), P.Hex(0xB4C2E4), P.Hex(0x8898C4) };
        (float x, float y, float r)[] craters = { (-6, -5, 4), (5, -8, 3.2f), (7, 3, 4.4f), (-8, 5, 3), (0, 8, 2.4f), (-1, -11, 2), (11, -3, 1.8f), (-12, -1, 1.6f) };
        for (int y = MoonY - 20; y <= MoonY + 20; y++)
            for (int x = MoonX - 20; x <= MoonX + 20; x++)
            {
                float dx = x - MoonX, dy = y - MoonY, d = MathF.Sqrt(dx * dx + dy * dy);
                if (d > r) continue;
                float lit = 1 - (dx * 0.5f + dy * 0.6f) / (2 * r) - 0.1f;                       // the light comes from the upper left
                int t = lit > 0.7f ? 0 : (lit > 0.45f ? 1 : (lit > 0.22f ? 2 : 3));
                foreach (var cr in craters) if (MathF.Sqrt((dx - cr.x) * (dx - cr.x) + (dy - cr.y) * (dy - cr.y)) < cr.r) t = Math.Min(3, t + 1);
                if (d > r - 1.2f) t = Math.Min(3, t + 1);
                c[x, y] = tone[t];
            }
    }

    private static void DrawMountains(PixelCanvas c)
    {
        for (int x = 0; x < Width; x++)
        {
            // (the ranges are low where the boat is, so that the boy stands against the sky)
            float low = x > 130 ? Math.Clamp((205 - x) / 75f, 0.1f, 1f) : 1f;
            int far = Horizon - (int)((4 + 7 * MathF.Abs(MathF.Sin(x * 0.04f + 0.7f)) + 2 * MathF.Sin(x * 0.2f)) * low);
            for (int y = far; y < Horizon + 2; y++) c[x, y] = P.Hex(0x040716);
            int near = Horizon - (int)((1 + 6 * MathF.Pow(MathF.Abs(MathF.Sin(x * 0.03f + 2f)), 0.9f) + MathF.Sin(x * 0.3f)) * low);
            for (int y = near; y < Horizon + 2; y++) c[x, y] = Dark;
            // a cold edge of moonlight on the tops
            if (x > 0 && far < Horizon - 1) c[x, far] = P.Hex(0x1A2A52);
        }
    }

    private static void DrawRiver(PixelCanvas c)
    {
        for (int y = Horizon + 2; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                float t = (y - Horizon) / (float)(Height - Horizon);
                // black, with the faintest sheen of the sky in it near the horizon and a column of the moon's light, broken, very dim
                P col = Pick(new[] { P.Hex(0x0A1228), P.Hex(0x04070F), Dark }, MathF.Pow(t, 0.5f) * 1.2f, x, y);
                float road = 1 - MathF.Abs(x - MoonX) / (3 + (y - Horizon) * 0.3f);
                if (road > 0 && Hash(x / 2, y, 5) % 6 < 2 && (y + x / 3) % 2 == 0) col = col.Mixed(P.Hex(0x30487C), 0.5f * road * (1 - t));
                c[x, y] = col;
            }
    }

    /// <summary>His boat, far and small on the dark river: the low hull, the mast, the sail raised, and the boy standing in front of the mast with his face turned up and his hands before him holding the lantern.</summary>
    private static void DrawBoat(PixelCanvas sil)
    {
        int bx = BoatX, by = BoatY;
        P k = Dark;
        Poly(sil, k, (bx - 18, by - 2), (bx - 12, by + 3), (bx + 12, by + 3), (bx + 19, by - 3), (bx + 15, by), (bx - 14, by));          // the hull
        Line(sil, bx + 4, by, bx + 4, by - 38, k); Line(sil, bx + 5, by, bx + 5, by - 38, k);                                              // the mast
        Poly(sil, k, (bx + 6, by - 36), (bx + 6, by - 3), (bx + 20, by - 3));                                                            // the sail
        Line(sil, bx + 4, by - 38, bx - 17, by - 2, k);                                                                                   // the stay
        // the boy: the body, the head tilted back, the arms bent with the hands together in front of the chest
        Poly(sil, k, (bx - 6, by - 18), (bx - 1, by - 18), (bx + 1, by - 1), (bx - 8, by - 1));
        PixelPaint.Ellipse(sil, bx - 3, by - 22, 2.9f, 3.1f, k);
        Poly(sil, k, (bx - 6, by - 24), (bx - 3, by - 26), (bx - 1, by - 24));                                                            // the face lifted: the chin up, the brow back
        Line(sil, bx - 6, by - 16, bx - 10, by - 12, k); Line(sil, bx - 5, by - 16, bx - 9, by - 12, k);                                   // the arms to the lantern
    }

    private static void DrawLantern(PixelCanvas c, PixelCanvas sil)
    {
        int lx = BoatX - 11, ly = BoatY - 11;
        // the glow of the lantern on the boy, the sail, the boat and the water: gold, in rings that fade
        for (int y = ly - 14; y <= ly + 18; y++)
            for (int x = lx - 14; x <= lx + 14; x++)
            {
                if (!c.Contains(x, y)) continue;
                float d = MathF.Sqrt((x - lx) * (x - lx) + (y - ly) * (y - ly));
                if (d >= 13) continue;
                float f = 1 - d / 13;
                if (sil[x, y].A > 0)
                {
                    bool edge = (x > 0 && sil[x - 1, y].A == 0) || (y > 0 && sil[x, y - 1].A == 0) || (x < Width - 1 && sil[x + 1, y].A == 0) || (y < Height - 1 && sil[x, y + 1].A == 0);
                    if (edge && f > 0.12f) c[x, y] = GoldDeep.Scaled(0.5f + f);
                }
                else if ((x + y) % 2 == 0 || f > 0.6f) c[x, y] = c[x, y].Mixed(Gold, 0.5f * f * f + (f > 0.6f ? 0.2f : 0));
            }
        // the lantern itself: a small frame, a bright flame
        for (int dy = -2; dy <= 2; dy++) for (int dx = -1; dx <= 1; dx++) if (c.Contains(lx + dx, ly + dy)) c[lx + dx, ly + dy] = (dy >= -1 && dy <= 1 && dx == 0) ? GoldBright : Gold;
        c[lx, ly - 3] = GoldDeep; c[lx, ly + 3] = GoldDeep;
        // its light on the black water below, in short strokes
        for (int k = 0; k < 7; k++) { int y = BoatY + 5 + k * 2; int w = 3 - k / 3; for (int x = lx - w; x <= lx + w; x++) if (c.Contains(x, y) && Hash(x, y, 7) % 3 != 0) c[x, y] = c[x, y].Mixed(GoldDeep, 0.55f * (1 - k / 8f)); }
    }
}
