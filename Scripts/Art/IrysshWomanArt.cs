using System;
using P = PixelColor;

/// <summary>
/// The Iryssh in her first form, drawn on the finer pixel grid of the Minish Cap bosses (a figure about 48 x 58 pixels, each
/// shown 2 x 2 screen pixels): she is built like the sorcerer Vaati of that game (a small head under a crown with two horns, a
/// gold collar, a long robe with a dark centre panel and a gold stripe that flares to a wide hem, flames at both sides),
/// but her hair is Din-style living fire (the big rolled mass with bright bands of the red-haired Dada Nayure), her horns are
/// two little gold serpents, and her bodice is of green scales. She hovers over the river: ripples spread under her hem.
///
/// Drawn in layers, all mirror-symmetric around the line between columns 23 and 24: hair behind, the body on its own layer
/// (so it gets a dark outline), then the fringe, the horns and the flames in front. Four frames: the flames lick, the hair
/// shimmers, she bobs, the ripples spread.
/// </summary>
public static class IrysshWomanArt
{
    public const int Width = 48;
    public const int Height = 61;
    /// <summary>Art pixel that is the boss's position (the middle of her chest).</summary>
    public static readonly (int x, int y) Centre = (24, 28);
    /// <summary>Row of the water surface, where the ripples spread under her hem.</summary>
    public const int WaterRow = 56;

    private const float Axis = 23.5f;

    private static readonly P Outline = P.Hex(0x2A1410);
    private static readonly P[] Skin = { P.Hex(0xF0C090), P.Hex(0xD9A06A), P.Hex(0xB97C4D) };
    private static readonly P[] Dress = { P.Hex(0xE0584A), P.Hex(0xB3262B), P.Hex(0x7E1A20) }; // lit, mid, shade
    private static readonly P PanelDark = P.Hex(0x2A0E14), PanelMid = P.Hex(0x4A1420);
    private static readonly P[] Gold = { P.Hex(0xFFE9A0), P.Hex(0xF2C14E), P.Hex(0xB8862B), P.Hex(0x7A5A1C) };
    private static P[] Fire => FirePalette.Ramp;
    private static P[] Scales => FirePalette.Scales;
    private static readonly P Yellow = P.Hex(0xFFE94D), Ink = P.Hex(0x1A0F0A), Lips = P.Hex(0xC2403A), Blush = P.Hex(0xF08A8A);
    private static readonly P Cream = P.Hex(0xFFF3D6);

    public static PixelCanvas MakeCanvas(int frame)
    {
        float phase = frame * MathF.PI / 2;
        int bob = frame == 2 || frame == 3 ? 1 : 0;

        var back = new PixelCanvas(Width, Height);   // hair behind the body
        var body = new PixelCanvas(Width, Height);   // robe, head: gets an outline
        var front = new PixelCanvas(Width, Height);  // fringe, horns, flames
        DrawHairBack(back, phase);
        DrawRobe(body, phase);
        DrawSleeves(body);
        DrawBodice(body);
        DrawCollar(body);
        DrawHead(body);
        DrawFringe(front);
        DrawHorns(front);
        var flames = new PixelCanvas(Width, Height);
        DrawFlames(flames, phase);

        // Compose the figure (shifted by the bob) with an outline round the body.
        var fig = new PixelCanvas(Width, Height);
        Blit(fig, back);
        (int, int)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                if (body[x, y].A != 0) continue;
                foreach (var (dx, dy) in dirs)
                    if (body.Contains(x + dx, y + dy) && body[x + dx, y + dy].A > 0) { fig[x, y] = back[x, y].A > 0 ? Fire[5] : Outline; break; }
            }
        Blit(fig, body);
        Blit(fig, front);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                if (flames[x, y].A != 0) continue;
                foreach (var (dx, dy) in dirs)
                    if (flames.Contains(x + dx, y + dy) && flames[x + dx, y + dy].A > 0 && fig[x, y].A == 0) { fig[x, y] = Outline; break; }
            }
        Blit(fig, flames);

        var c = new PixelCanvas(Width, Height);
        DrawShadow(c);
        DrawRipples(c, frame, false);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (fig[x, y].A > 0 && y + bob < Height) c[x, y + bob] = fig[x, y];
        DrawRipples(c, frame, true);
        return c;
    }

    // MARK: Helpers

    private static void Blit(PixelCanvas dst, PixelCanvas src)
    {
        for (int y = 0; y < src.Height; y++) for (int x = 0; x < src.Width; x++) if (src[x, y].A > 0) dst[x, y] = src[x, y];
    }

    private static void Put(PixelCanvas c, int x, int y, P color) => BossArt.Put(c, x, y, color);

    /// <summary>Puts a pixel at distance d from the axis (d = 0 is the first column to the right) and its mirror image.</summary>
    private static void Sym(PixelCanvas c, int d, int y, P color)
    {
        Put(c, 24 + d, y, color);
        Put(c, 23 - d, y, color);
    }

    private static float Dx(int x) => MathF.Abs(x - Axis);

    // MARK: Hair of fire

    /// <summary>Rolled bands of fire: the colour follows curved stripes, brightest in the middle, dark red at the rim.</summary>
    private static P HairColour(int x, int y, float d, float phase, int bandOffset = 0)
    {
        if (d > 0.9f) return Fire[5];
        if (d > 0.78f) return Fire[4];
        float curve = Dx(x) * Dx(x) * 0.012f;
        int band = (int)MathF.Floor(y + curve + bandOffset + 0.8f * MathF.Sin(phase));
        switch (((band % 5) + 5) % 5)
        {
            case 0: return Fire[1];
            case 1: return Fire[2];
            case 2: return Fire[3];
            case 3: return Fire[2];
            default: return Fire[3];
        }
    }

    private static void DrawHairBack(PixelCanvas c, float phase)
    {
        // The tall rolled mass above the head, then the cloud of hair round the face, then long locks to each side.
        for (int y = 0; y < 50; y++)
            for (int x = 0; x < Width; x++)
            {
                float dx = Dx(x), best = 9;
                {
                    float ex = dx / 9.5f, ey = (y + 0.5f - 8.5f) / 8.5f;
                    float wob = 1 + 0.04f * MathF.Sin(MathF.Atan2(ey, ex) * 6 + phase);
                    best = MathF.Min(best, MathF.Sqrt(ex * ex + ey * ey) / wob);
                }
                {
                    float ex = dx / 13.5f, ey = (y + 0.5f - 21f) / 10.5f;
                    float wob = 1 + 0.05f * MathF.Sin(MathF.Atan2(ey, ex) * 9 - phase) + 0.03f * MathF.Sin(MathF.Atan2(ey, ex) * 15 + phase);
                    best = MathF.Min(best, MathF.Sqrt(ex * ex + ey * ey) / wob);
                }
                if (y >= 24 && y < 49)
                {
                    float t = (y - 24) / 25f;
                    float lockX = 11.5f - 2.0f * t + 1.0f * MathF.Sin(y * 0.4f + phase) * t;
                    float half = 4.4f * (1 - t * 0.9f) + 0.5f;
                    best = MathF.Min(best, MathF.Abs(dx - lockX) / half);
                }
                if (best >= 1) continue;
                c[x, y] = HairColour(x, y, best, phase);
            }
    }

    private static void DrawFringe(PixelCanvas c)
    {
        // Bangs over the forehead, parted in the middle, and a lock down each side of the face.
        for (int y = 10; y <= 13; y++)
            for (int d = 0; d <= 7; d++)
            {
                float reach = 13.4f - d * 0.5f - (d <= 1 ? 2.4f - d : 0);
                if (y > reach) continue;
                Sym(c, d, y, y >= 11 ? Fire[3] : (y >= 10 ? Fire[2] : Fire[1]));
            }
        for (int y = 12; y <= 22; y++)
        {
            float t = (y - 12) / 10f;
            int d0 = 6, d1 = 7 + (y < 17 ? 1 : 0);
            for (int d = d0; d <= d1; d++) Sym(c, d, y, t > 0.75f ? Fire[4] : (d == d0 ? Fire[2] : Fire[3]));
        }
    }

    // MARK: Head

    private static void DrawHead(PixelCanvas c)
    {
        // face: a rounded oval that narrows to a small chin
        for (int y = 8; y <= 23; y++)
            for (int x = 0; x < Width; x++)
            {
                float dx = Dx(x), ey = (y + 0.5f - 15.2f) / 7.4f, ex = dx / 6.2f;
                if (y > 19) ex = dx / (6.2f - (y - 19) * 1.05f);
                float d = ex * ex + ey * ey;
                if (d >= 1) continue;
                c[x, y] = d > 0.74f || y >= 22 ? Skin[1] : Skin[0];
            }
        // pointed ears poking out of the hair
        foreach (int y in new[] { 16, 17 }) { Sym(c, 6, y, Skin[0]); Sym(c, 7, y, Skin[1]); }
        Sym(c, 8, 15, Skin[1]); Sym(c, 8, 16, Skin[2]);
        // neck
        for (int y = 22; y <= 25; y++) for (int d = 0; d < 2; d++) Sym(c, d, y, Skin[2]);
        // brows
        foreach (int d in new[] { 3, 4, 5 }) Sym(c, d, 14, P.Hex(0x9A4A2A));
        Sym(c, 2, 15, P.Hex(0x9A4A2A));
        // big eyes: dark upper lash with a flick, a round dark pupil in a yellow iris, a bright glint
        foreach (int d in new[] { 2, 3, 4, 5 }) Sym(c, d, 16, Ink);
        Sym(c, 6, 15, Ink);
        Sym(c, 2, 17, Yellow); Sym(c, 3, 17, Ink); Sym(c, 4, 17, Ink); Sym(c, 5, 17, Fire[1]);
        Sym(c, 2, 18, Fire[1]); Sym(c, 3, 18, Ink); Sym(c, 4, 18, Ink); Sym(c, 5, 18, Fire[2]);
        Sym(c, 2, 19, Skin[1]); Sym(c, 3, 19, Fire[2]); Sym(c, 4, 19, Fire[2]); Sym(c, 5, 19, Skin[1]);
        Sym(c, 2, 17, Cream);
        // blush, nose, mouth
        Sym(c, 5, 21, Blush); Sym(c, 4, 21, Blush);
        Sym(c, 0, 20, Skin[2]);
        Sym(c, 0, 22, Lips); Sym(c, 1, 22, Lips);
        // tiara band across the brow with a ruby
        for (int d = 0; d <= 6; d++) Sym(c, d, 8, d % 2 == 0 ? Gold[0] : Gold[1]);
        for (int d = 2; d <= 6; d++) Sym(c, d, 9, Gold[2]);
        Sym(c, 0, 9, P.Hex(0xE0302A)); Sym(c, 1, 9, P.Hex(0xE0302A));
        Sym(c, 0, 8, P.Hex(0xFF7A6A)); Sym(c, 1, 8, P.Hex(0xFF7A6A));
        Sym(c, 0, 7, Gold[1]); Sym(c, 1, 7, Gold[1]);
        Sym(c, 0, 6, Gold[0]); Sym(c, 1, 6, Gold[0]);
    }

    // MARK: Horns: two little gold serpents

    private static void DrawHorns(PixelCanvas c)
    {
        // Each horn rises from the tiara, bends outwards and ends in a small serpent head looking inwards.
        (int d, int y)[] path = { (7, 8), (8, 7), (9, 6), (10, 5), (10, 4), (10, 3), (9, 2), (9, 1) };
        for (int i = 0; i < path.Length; i++)
        {
            var (d, y) = path[i];
            Sym(c, d, y, Gold[1]);
            Sym(c, d - 1, y, i % 2 == 0 ? Gold[0] : Gold[2]);
            if (i > 0 && i < path.Length - 2) Sym(c, d + 1, y, Gold[2]);
        }
        // head of the serpent at the tip
        Sym(c, 8, 0, Gold[1]); Sym(c, 9, 0, Gold[0]); Sym(c, 10, 0, Gold[2]);
        Sym(c, 8, 1, Yellow);                         // eye
        Sym(c, 7, 0, Fire[2]);                        // tongue of flame
    }

    // MARK: Collar, bodice, sleeves

    private static void DrawCollar(PixelCanvas c)
    {
        // Two round gold shoulder pieces, like the armour on Vaati's shoulders, and a gold necklace between them.
        for (int y = 24; y <= 31; y++)
            for (int x = 0; x < Width; x++)
            {
                float dx = Dx(x);
                float ex = (dx - 6.4f) / 3.9f, ey = (y + 0.5f - 27.6f) / 3.6f;
                if (ex * ex + ey * ey >= 1) continue;
                float lit = (y - 24) / 7f;
                c[x, y] = lit < 0.3f ? Gold[0] : (lit < 0.6f ? Gold[1] : (lit < 0.85f ? Gold[2] : Gold[3]));
                if (dx > 8.6f && lit > 0.3f) c[x, y] = Gold[2];
            }
        for (int d = 0; d < 3; d++) Sym(c, d, 26, d % 2 == 0 ? Gold[1] : Gold[2]);
        Sym(c, 0, 27, Gold[1]); Sym(c, 1, 27, Gold[1]);
        Sym(c, 0, 28, P.Hex(0x55B35F)); Sym(c, 1, 28, P.Hex(0x55B35F));  // emerald pendant
        Sym(c, 0, 29, Scales[1]); Sym(c, 1, 29, Scales[1]);
    }

    private static void DrawBodice(PixelCanvas c)
    {
        // A bodice of green dragon scales ending in a V at the waist, framed by gold.
        for (int y = 28; y <= 37; y++)
        {
            float half = y <= 30 ? 4.6f : 4.6f - (y - 30) * 0.55f;
            for (int d = 0; d < half; d++)
            {
                bool seam = (d + (y / 2)) % 3 == 0 && y % 2 == 1;
                P col = Scales[1 + (d + y) % 3];
                if (seam) col = Scales[0];
                if (d > half - 1.2f) col = Gold[2];
                Sym(c, d, y, col);
            }
        }
        for (int y = 31; y <= 37; y++) Sym(c, 0, y, Gold[1]);
    }

    private static void DrawSleeves(PixelCanvas c)
    {
        // Wide bell sleeves hanging from the shoulders, lighter than the robe, with a gold cuff.
        const int top = 27, cuff = 42;
        for (int y = top; y <= cuff + 3; y++)
        {
            float t = (y - top) / (float)(cuff - top);
            float outer = 9.2f + 3.2f * MathF.Pow(t, 0.8f), inner = 4.8f + 2.4f * t;
            for (int x = 0; x < Width; x++)
            {
                float dx = Dx(x);
                if (dx < inner || dx > outer) continue;
                P col = dx > outer - 1.8f ? Dress[1] : (dx < inner + 2.4f ? Dress[0] : P.Hex(0xCC3A36));
                if (y >= cuff) col = y == cuff ? Gold[0] : (y == cuff + 1 ? Gold[1] : Gold[2]);
                if (y == cuff + 3) col = Gold[3];
                c[x, y] = col;
            }
        }
        // a pale hand peeks out of each sleeve
        foreach (int y in new[] { cuff + 4, cuff + 5 }) for (int d = 8; d <= 10; d++) Sym(c, d, y, y == cuff + 4 ? Skin[0] : Skin[1]);
    }

    // MARK: Robe

    private static float RobeHalf(int y) => 5f + 10.5f * MathF.Pow((y - 26) / 26f, 0.92f);

    private static float Hem(float dx, float phase) => 52f + 0.9f * MathF.Sin(dx * 0.9f + phase * 0.5f) - 3.0f * MathF.Pow(dx / 15.5f, 3);

    private static void DrawRobe(PixelCanvas c, float phase)
    {
        for (int y = 26; y <= 54; y++)
        {
            float half = RobeHalf(y);
            for (int x = 0; x < Width; x++)
            {
                float dx = Dx(x);
                if (dx > half || y > Hem(dx, phase)) continue;
                // outer cloth: deep crimson, darker toward the edge
                float edge = half - dx;
                P col = edge < 2 ? PanelMid : Dress[2];
                if (y > 34 && dx > 7 && ((int)(dx * 0.8f + y * 0.35f)) % 5 == 0) col = PanelMid;   // folds
                // red band with diamonds beside the centre panel
                float panel = 2.6f + MathF.Max(0, y - 32) * 0.28f;
                if (dx <= panel + 4.2f) col = Dress[1];
                if (dx <= panel + 4.2f && (y + (int)dx) % 6 == 0 && dx > panel + 1.5f && dx < panel + 3.4f) col = Cream;
                // dark centre panel with a gold stripe
                if (dx <= panel) col = dx < 1 ? Gold[1] : (dx < panel - 1.4f ? PanelDark : PanelMid);
                if (dx > panel && dx <= panel + 1.1f) col = Gold[2];
                if (y < 34 && dx > 1 && dx <= 4.5f) continue; // bodice sits here
                c[x, y] = col;
            }
        }
        // gold trim round the hem
        for (int x = 0; x < Width; x++)
        {
            float dx = Dx(x);
            int yh = (int)MathF.Floor(Hem(dx, phase));
            if (dx > RobeHalf(yh)) continue;
            Put(c, x, yh, (x / 2) % 2 == 0 ? Gold[1] : Gold[2]);
            Put(c, x, yh - 1, (x / 2) % 2 == 0 ? Gold[0] : Gold[1]);
        }
        // the stripe's medallion at the waist
        Sym(c, 0, 38, Gold[0]); Sym(c, 1, 38, Gold[0]);
        Sym(c, 0, 39, P.Hex(0xE0302A)); Sym(c, 1, 39, P.Hex(0xE0302A));
        Sym(c, 0, 40, Gold[1]); Sym(c, 1, 40, Gold[1]);
    }

    // MARK: Flames at the sides of the robe

    private static void DrawFlames(PixelCanvas c, float phase)
    {
        for (int s = 0; s < 2; s++)
        {
            float ph = phase + s * 1.7f;
            DrawTongue(c, s, 14, 51, 6, ph + 2.1f);
            DrawTongue(c, s, 17, 53, 11, ph);
            DrawTongue(c, s, 20, 53, 6, ph + 4.0f);
        }
    }

    private static void DrawTongue(PixelCanvas c, int side, int d, int baseY, int height, float ph)
    {
        for (int i = 0; i < height; i++)
        {
            float t = i / (float)height;
            float width = 2.1f * (1 - t * t) + 0.4f;
            float sway = MathF.Sin(ph + i * 0.5f) * 1.2f * t;
            int y = baseY - i;
            int lo = (int)MathF.Round(d + sway - width), hi = (int)MathF.Round(d + sway + width);
            for (int x = lo; x <= hi; x++)
            {
                float u = MathF.Abs(x - (d + sway)) / MathF.Max(0.5f, width);
                P col = u > 0.8f ? Fire[3] : (u > 0.45f ? Fire[2] : (t < 0.55f ? Fire[1] : Fire[0]));
                if (t > 0.8f) col = Fire[2];
                int sx = side == 0 ? 23 - x : 24 + x;
                if (side == 0) { if (x >= 0 && x < 24) Put(c, 23 - x, y, col); }
                else { if (x >= 0 && x < 24) Put(c, 24 + x, y, col); }
                _ = sx;
            }
        }
    }

    // MARK: Water

    private static void DrawShadow(PixelCanvas c)
    {
        for (int y = 54; y <= 59; y++)
            for (int x = 0; x < Width; x++)
            {
                float ex = Dx(x) / 16f, ey = (y + 0.5f - 56.5f) / 3f;
                if (ex * ex + ey * ey < 1) c[x, y] = P.Hex(0x0B1A24, 130);
            }
        // firelight on the water under her hem
        for (int x = 8; x < 40; x++)
            if ((x * 3) % 5 == 0) Put(c, x, WaterRow - 2, Fire[2].WithAlpha(120));
    }

    private static void DrawRipples(PixelCanvas c, int frame, bool front)
    {
        BossArt.DrawRipples(c, 24, WaterRow, 22, 4.2f, frame, front);
    }
}
