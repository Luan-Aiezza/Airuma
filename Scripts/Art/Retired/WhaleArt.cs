using System;
using Godot;
using P = PixelColor;

/// <summary>
/// Baylo's second shape: the great dolphin of the river, a sea monster seen from above and a little from the front (on the fine grid of the
/// other bosses, each pixel shown 2 x 2, so the whole figure is about 256 x 272 screen pixels). A huge round body in blues, deep navy on its
/// back and pale on its belly, with curved dark stripes, scars, barnacles with a touch of coral, a ridge of plates along its spine, gill
/// slits and rows of glowing cyan spots; two wide fins with spines, a spiky tail fluke at the top, a pair of angry yellow eyes under heavy
/// brows, a wide mouth full of teeth and four whiskers hanging from the chin. Four frames: the tail sways, the fins flap, the whiskers
/// wave, the spots glow, a puff comes from the blowhole and rings spread on the water.
/// </summary>
public static class WhaleArt
{
    public const int Width = 128, Height = 136;
    /// <summary>Art pixel that is the boss's position (the middle of the body).</summary>
    public static readonly (int x, int y) Centre = (64, 72);
    private const float BodyRx = 44, BodyRy = 50;

    private static readonly P Outline = P.Hex(0x061838);
    private static readonly P[] Blue = { P.Hex(0xDDF4FF), P.Hex(0x9ADBFA), P.Hex(0x4FA9E8), P.Hex(0x2A74C4), P.Hex(0x1A4C92), P.Hex(0x0E2C62) };   // light .. deep
    private static readonly P[] Belly = { P.Hex(0xF2FBFF), P.Hex(0xCDEEFA), P.Hex(0x9AD2EA), P.Hex(0x6FAFD4) };
    private static readonly P Tooth = P.Hex(0xF4FBFF), ToothShade = P.Hex(0xA8CCE0), Mouth = P.Hex(0x2A0E36), Tongue = P.Hex(0x7A2A5A);
    private static readonly P Gold = P.Hex(0xFFE45C), Orange = P.Hex(0xFF9A2A), Slit = P.Hex(0x1A0A10), Glow = P.Hex(0x7CF7FF);
    private static readonly P[] Coral = { P.Hex(0xFF7AA8), P.Hex(0xFFB0C8) };

    private static void Put(PixelCanvas c, int x, int y, P colour) => BossArt.Put(c, x, y, colour);

    private static void Blit(PixelCanvas dst, PixelCanvas src)
    {
        for (int y = 0; y < src.Height; y++) for (int x = 0; x < src.Width; x++) if (src[x, y].A > 0) dst[x, y] = src[x, y];
    }

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    /// <summary>Shades a solid layer like a lit dome (light from the upper left): the middle is light and the rim, lower right, is deep.</summary>
    private static void Dome(PixelCanvas layer, float cx, float cy, float rx, float ry, float bias = 0.2f)
    {
        for (int y = 0; y < layer.Height; y++)
            for (int x = 0; x < layer.Width; x++)
            {
                if (layer[x, y].A == 0) continue;
                float nx = (x + 0.5f - cx) / rx, ny = (y + 0.5f - cy) / ry;
                float d = MathF.Min(1.2f, MathF.Sqrt(nx * nx + ny * ny));
                float lit = (nx + ny) / 2.8f;                                   // -0.5 (lit) .. 0.5 (dark)
                float t = Math.Clamp(bias + 0.5f * d * d + 0.55f * lit * d, 0, 0.99f);
                layer[x, y] = BossArt.Pick(Blue, t, x, y);
            }
    }

    private static void Line(PixelCanvas c, float x0, float y0, float x1, float y1, P colour, float thick = 0.6f)
    {
        int n = (int)(Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0)) * 2) + 1;
        for (int i = 0; i <= n; i++)
        {
            float t = (float)i / n;
            PixelPaint.Ellipse(c, x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, thick, thick, colour);
        }
    }

    public static PixelCanvas MakeCanvas(int frame)
    {
        int bob = frame == 2 || frame == 3 ? 1 : 0;
        float sway = MathF.Sin(frame * MathF.PI / 2);                   // -1..1
        int flap = (int)MathF.Round(sway * 3);
        var rng = new SeededGenerator(0xB1E7);

        var tail = new PixelCanvas(Width, Height);
        var fins = new PixelCanvas(Width, Height);
        var body = new PixelCanvas(Width, Height);
        var details = new PixelCanvas(Width, Height);
        var whiskers = new PixelCanvas(Width, Height);

        // The tail fluke: a thick stalk from the top of the body and two spiky lobes, swaying.
        int fx = (int)MathF.Round(sway * 4);
        PixelPaint.Ellipse(tail, 64 + fx * 0.5f, 26, 9, 15, Blue[3]);
        foreach (int side in new[] { -1, 1 })
        {
            var lobe = new[] { (0f, 14f), (10, 9), (22, 5), (34, 1), (37, 7), (43, 6), (38, 12), (45, 14), (34, 16), (22, 17), (10, 20), (0, 23) };
            var pts = new Vector2[lobe.Length];
            for (int i = 0; i < lobe.Length; i++) pts[i] = V(64 + fx + side * lobe[i].Item1, lobe[i].Item2);
            PixelPaint.Poly(tail, pts, Blue[3]);
        }
        Dome(tail, 64 + fx, 12, 46, 18, 0.1f);
        // the rays of the fluke and the rings of the stalk
        foreach (int side in new[] { -1, 1 })
            for (int k = 0; k < 4; k++) Line(tail, 64 + fx + side * 6, 18, 64 + fx + side * (22 + k * 6), 7 - k * 1.3f + (k > 1 ? 3 : 0), Blue[5], 0.5f);
        for (int y = 24; y < 40; y += 4) for (int x = 56 + fx / 2; x < 73 + fx / 2; x++) if (tail[Math.Clamp(x, 0, Width - 1), y].A > 0 && (x + y) % 3 != 0) tail[x, y] = Blue[4];

        // The fins: wide, with spines on the trailing edge and ribs.
        foreach (int side in new[] { -1, 1 })
        {
            var fin = new[] { (34f, 54f), (50, 46), (62, 50), (58, 60), (63.5f, 67), (55, 72), (62, 82), (52, 84), (57, 95), (44, 92), (34, 82) };
            var pts = new Vector2[fin.Length];
            for (int i = 0; i < fin.Length; i++)
            {
                float outward = (fin[i].Item1 - 34) / 30f;                   // the tips flap more than the root
                pts[i] = V(64 + side * fin[i].Item1, fin[i].Item2 + flap * outward * (side > 0 ? 1 : -1) * 0.0f + flap * outward);
            }
            var layer = new PixelCanvas(Width, Height);
            PixelPaint.Poly(layer, pts, Blue[3]);
            Dome(layer, 64 + side * 50, 70 + flap, 14, 26, 0.15f);
            foreach (var tip in new[] { 3, 5, 7, 9 }) Line(layer, 64 + side * 38, 70, pts[tip].X, pts[tip].Y, Blue[5], 0.5f);
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) if (layer[x, y].A > 0 && (x * 3 + y) % 11 == 0) layer[x, y] = Blue[1];
            Blit(fins, layer);
        }

        // The body: a huge round mass.
        PixelPaint.Ellipse(body, 64, 72, BodyRx, BodyRy, Blue[3]);
        Dome(body, 64, 72, BodyRx, BodyRy, 0.18f);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                if (body[x, y].A == 0) continue;
                float nx = (x + 0.5f - 64) / BodyRx, ny = (y + 0.5f - 72) / BodyRy;
                float d = MathF.Sqrt(nx * nx + ny * ny);
                // The belly, pale, round the lower part of the head, with the grooves of the throat.
                if (d > 0.62f && ny > 0.38f)
                {
                    float belly = (d - 0.62f) / 0.38f;
                    var tone = belly > 0.75f ? Belly[0] : (belly > 0.4f ? Belly[1] : Belly[2]);
                    if (ny > 0.5f && (int)(y + MathF.Abs(nx) * 5) % 4 == 0) tone = Belly[3];
                    body[x, y] = tone;
                    continue;
                }
                // Curved dark stripes across the back.
                if (ny < 0.3f && d < 0.93f)
                {
                    int band = (int)MathF.Floor((y + 0.014f * (x - 64) * (x - 64)) / 7f);
                    if (band % 3 == 0) body[x, y] = Darker(body[x, y]);
                }
            }

        // The ridge of plates along the spine, from the crown towards the tail: bigger towards the head, each with a dark edge.
        for (int y = 27; y <= 63; y += 6)
        {
            float k = 1.4f + (y - 27) / 36f * 1.2f;
            var plate = new[] { V(64, y - 4 * k), V(64 + 3.2f * k, y), V(64, y + 2.6f * k), V(64 - 3.2f * k, y) };
            PixelPaint.Poly(body, new[] { V(64, y - 4 * k - 1), V(64 + 3.2f * k + 1, y), V(64, y + 2.6f * k + 1), V(64 - 3.2f * k - 1, y) }, Blue[5]);
            PixelPaint.Poly(body, plate, Blue[0]);
            Put(body, 64 + (int)(1.5f * k), y, Blue[2]); Put(body, 64 + (int)(1.5f * k), y + 1, Blue[3]);
        }
        // Scars, barnacles with a touch of coral, gill slits.
        foreach (var (sx, sy) in new[] { (36, 52), (80, 46), (88, 66) }) Line(body, sx, sy, sx + 9, sy + 5, Blue[0], 0.4f);
        for (int i = 0; i < 16; i++)
        {
            float a = rng.Range(0f, MathF.Tau), r = rng.Range(0.45f, 0.86f);
            int bx = (int)(64 + MathF.Cos(a) * r * BodyRx), by = (int)(72 + MathF.Sin(a) * r * BodyRy * 0.8f - 6);
            if (by > 84) continue;
            Put(body, bx, by, Belly[0]); Put(body, bx + 1, by, Belly[2]); Put(body, bx, by + 1, Blue[5]);
            if (i % 4 == 0) { Put(body, bx - 1, by - 1, Coral[0]); Put(body, bx, by - 1, Coral[1]); }
        }
        foreach (int side in new[] { -1, 1 })
            for (int k = 0; k < 3; k++)
                for (int j = 0; j < 7; j++) Put(body, 64 + side * (33 - (int)MathF.Round(MathF.Sin(j * 0.45f) * 2)), 62 + k * 4 + j - 1, Blue[5]);

        // The glowing spots along the sides: they glow brighter on alternate frames.
        for (int i = 0; i < 7; i++)
            foreach (int side in new[] { -1, 1 })
            {
                float a = 0.55f + i * 0.2f;
                int gx = (int)(64 + side * MathF.Sin(a) * 41), gy = (int)(72 - MathF.Cos(a) * 41 + 12);
                bool bright = (i + frame + (side > 0 ? 1 : 0)) % 2 == 0;
                Put(details, gx, gy, bright ? Glow : Blue[1]); Put(details, gx + 1, gy, bright ? Glow : Blue[1]);
                Put(details, gx, gy + 1, bright ? Glow : Blue[2]); Put(details, gx + 1, gy + 1, bright ? Blue[0] : Blue[2]);
            }

        // The blowhole on the crown, with a puff.
        foreach (int side in new[] { -1, 1 })
        {
            for (int k = 0; k < 4; k++) Put(details, 64 + side * (4 + k), 44 + k / 2, Blue[5]);
        }
        int puff = frame % 2 == 0 ? 2 : 4;
        for (int i = 0; i < puff; i++)
            foreach (int side in new[] { -1, 1 }) PixelPaint.Ellipse(details, 64 + side * (5 + i * 2), 40 - i * 3, 1.6f + i * 0.4f, 1.6f + i * 0.4f, P.Hex(0xE8FBFF));

        // The face: heavy brows, angry yellow eyes with slit pupils.
        foreach (int side in new[] { -1, 1 })
        {
            PixelPaint.Poly(details, new[] { V(64 + side * 36, 83), V(64 + side * 10, 91), V(64 + side * 10, 96), V(64 + side * 34, 89) }, Blue[5]);
            Line(details, 64 + side * 35, 83, 64 + side * 11, 91, Blue[4], 0.5f);
            float ex = 64 + side * 22, ey = 97;
            PixelPaint.Ellipse(details, ex, ey, 8, 5.2f, Slit);
            PixelPaint.Ellipse(details, ex, ey, 7, 4.2f, Orange);
            PixelPaint.Ellipse(details, ex, ey, 6, 3.4f, Gold);
            for (int y = -3; y <= 3; y++) Put(details, (int)ex, (int)ey + y, Slit);
            Put(details, (int)ex - 3 * side, (int)ey - 2, P.Hex(0xFFFFFF));
        }

        // The mouth: a wide grin full of teeth.
        for (int x = 36; x <= 92; x++)
        {
            float u = (x - 64) / 28f;
            int top = 109 - (int)MathF.Round(u * u * 9);
            for (int y = top; y < top + 7; y++) Put(details, x, y, y > top + 4 ? Tongue : Mouth);
            if (x % 5 == 0)                                                    // upper teeth
            {
                for (int k = 0; k < 5; k++) for (int w = 0; w < 3 - k / 2 - (k == 4 ? 1 : 0); w++) Put(details, x + w, top + k + 1, k > 2 ? ToothShade : Tooth);
            }
            if (x % 7 == 3)                                                    // lower teeth
            {
                for (int k = 0; k < 3; k++) Put(details, x, top + 7 - k, Tooth);
            }
        }
        // the lips
        for (int x = 36; x <= 92; x++) { float u = (x - 64) / 28f; int top = 109 - (int)MathF.Round(u * u * 9); Put(details, x, top - 1, Blue[5]); Put(details, x, top + 7, Blue[4]); }

        // The whiskers hanging from the chin, waving.
        for (int i = 0; i < 4; i++)
        {
            int side = i < 2 ? -1 : 1, k = i % 2;
            float x0 = 64 + side * (12 + k * 15), y0 = 114 + k * 2;
            float wave = MathF.Sin(frame * MathF.PI / 2 + i * 1.3f);
            float px = x0, py = y0;
            for (int s = 0; s < 14; s++)
            {
                px += side * (0.6f + 0.7f * MathF.Sin(s * 0.5f + wave)) * 0.8f;
                py += 1.0f;
                PixelPaint.Ellipse(whiskers, px, py, 1.1f, 1.1f, Belly[1]);
            }
            PixelPaint.Ellipse(whiskers, px, py + 1, 2.0f, 2.0f, Glow);
            Put(whiskers, (int)px, (int)py, P.Hex(0xFFFFFF));
        }

        // Compose: each part with its own outline.
        var fig = new PixelCanvas(Width, Height);
        void Add(PixelCanvas part, bool outline)
        {
            if (outline)
            {
                (int, int)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                    {
                        if (part[x, y].A != 0) continue;
                        foreach (var (dx, dy) in dirs)
                            if (part.Contains(x + dx, y + dy) && part[x + dx, y + dy].A > 0) { fig[x, y] = Outline; break; }
                    }
            }
            Blit(fig, part);
        }
        Add(tail, true); Add(fins, true); Add(whiskers, true); Add(body, true);
        // the details are cut to the body, so that nothing is left floating
        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) if (details[x, y].A > 0 && (body[x, y].A > 0 || x < 0)) fig[x, y] = details[x, y];

        var c = new PixelCanvas(Width, Height);
        PixelPaint.Ellipse(c, 64, 127, 50, 6, new P(4, 12, 40, 100));
        BossArt.DrawRipples(c, 64, 125, 56, 9, frame, false);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (fig[x, y].A > 0 && y + bob < Height) c[x, y + bob] = fig[x, y];
        BossArt.DrawRipples(c, 64, 125, 56, 9, frame, true);
        return c;
    }

    /// <summary>One step darker on the blue ramp.</summary>
    private static P Darker(P colour)
    {
        for (int i = 0; i < Blue.Length - 1; i++) if (Blue[i] == colour) return Blue[i + 1];
        return colour.Scaled(0.8f);
    }
}
