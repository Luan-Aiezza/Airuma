using System;
using System.Collections.Generic;
using Godot;
using P = PixelColor;

/// <summary>
/// The scenery of the river of the parties (stage 2), in the place of the jungle. The sky that the water mirrors is a night with no moon
/// and no stars. On the banks there is a festival: boardwalks with
/// lamp posts, striped tents seen from above, the shadows of the crowd and of the dancers, and, over the water, strings of party lights
/// with bunting and paper garlands. What the water mirrors of the banks is the same festival, as dark roofs and a scatter of lights.
/// The layers have the same names and sizes as those of <see cref="BankArt"/>, so the river builds either with the same code.
/// </summary>
public static class FestivalArt
{
    public const int Width = BankArt.Width, Height = BankArt.Height;

    // MARK: Colours

    /// <summary>Paper lanterns: the bright centre of the glowing paper, its middle, its dark edge, and the colour of its glow.</summary>
    private static readonly P[][] Lanterns =
    {
        new[] { P.Hex(0xFF8A5A), P.Hex(0xE23C34), P.Hex(0x8E1F2A), P.Hex(0xFF5A3A) },   // red
        new[] { P.Hex(0xFFD070), P.Hex(0xF08A2A), P.Hex(0xA84E1C), P.Hex(0xFFA03A) },   // orange
        new[] { P.Hex(0xFFF4B0), P.Hex(0xFFD060), P.Hex(0xC08A2A), P.Hex(0xFFD870) },   // gold
        new[] { P.Hex(0xFFB0D0), P.Hex(0xE0508A), P.Hex(0x8E2A5E), P.Hex(0xFF70A8) },   // pink
        new[] { P.Hex(0xA8F4E4), P.Hex(0x2FBFB0), P.Hex(0x1A6E72), P.Hex(0x50E0D0) },   // teal
    };
    public const int LanternTones = 5;
    private static readonly P[] Bulbs = { P.Hex(0xFFE27A), P.Hex(0xFF6A5A), P.Hex(0x7AE0FF), P.Hex(0xFF8AD0), P.Hex(0x9AF08A) };
    private static readonly P[] Earth = { P.Hex(0x100812), P.Hex(0x1B0E1E), P.Hex(0x2A1630), P.Hex(0x3A2142) };
    private static readonly P[] Wood = { P.Hex(0x1E1210), P.Hex(0x3A2218), P.Hex(0x5C3A26), P.Hex(0x7E5236) };
    private static readonly P Warm = P.Hex(0xFFA85A);

    private static float Hypot(float a, float b) => MathF.Sqrt(a * a + b * b);

    // MARK: Helpers

    /// <summary>A canvas turned left to right (the right bank is the left one's mirror image).</summary>
    private static PixelCanvas Flip(PixelCanvas src)
    {
        var c = new PixelCanvas(src.Width, src.Height);
        for (int y = 0; y < src.Height; y++) for (int x = 0; x < src.Width; x++) c[src.Width - 1 - x, y] = src[x, y];
        return c;
    }

    /// <summary>Warm light spread over an opaque picture: flat dithered bands that fade with the distance.</summary>
    private static void Glow(PixelCanvas c, float cx, float cy, float radius, P colour, float strength, bool wrapY = false)
    {
        for (int y = (int)(cy - radius); y <= (int)(cy + radius); y++)
            for (int x = (int)(cx - radius); x <= (int)(cx + radius); x++)
            {
                int yy = wrapY ? ((y % c.Height) + c.Height) % c.Height : y;
                if (!c.Contains(x, yy)) continue;
                float d = Hypot(x - cx, y - cy) / radius;
                if (d >= 1) continue;
                float glow = MathF.Pow(1 - d, 1.7f) * strength;
                float band = MathF.Floor(glow * 5 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / 5;
                if (band > 0) c[x, yy] = c[x, yy].Mixed(colour, MathF.Min(1, band));
            }
    }

    /// <summary>The same light for a picture with transparency: a faint halo of the colour, in bands.</summary>
    private static void Halo(PixelCanvas c, float cx, float cy, float radius, P colour, int alpha, bool wrapY = true)
    {
        for (int y = (int)(cy - radius); y <= (int)(cy + radius); y++)
            for (int x = (int)(cx - radius); x <= (int)(cx + radius); x++)
            {
                int yy = wrapY ? ((y % c.Height) + c.Height) % c.Height : y;
                if (!c.Contains(x, yy)) continue;
                float d = Hypot(x - cx, y - cy) / radius;
                if (d >= 1) continue;
                int level = (int)MathF.Floor((1 - d) * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.6f);
                if (level > 0 && c[x, yy].A < alpha) c[x, yy] = colour.WithAlpha((byte)(alpha * level / 3));
            }
    }

    private static void Put(PixelCanvas c, int x, int y, P colour, bool wrapY = true)
    {
        if (colour.A == 0) return;
        if (wrapY) y = ((y % c.Height) + c.Height) % c.Height;
        if (c.Contains(x, y)) c[x, y] = colour;
    }

    /// <summary>A cylindrical paper lantern, `w` by `h` px with its top at `top`, hanging from a cord; glowing paper with ribs, caps and a tassel.</summary>
    public static void DrawLantern(PixelCanvas c, int cx, int top, int w, int h, int tone, bool cord = true, bool tassel = true, bool wrapY = false)
    {
        var pal = Lanterns[((tone % LanternTones) + LanternTones) % LanternTones];
        int left = cx - w / 2;
        if (cord) { Put(c, cx, top - 2, pal[2], wrapY); Put(c, cx, top - 1, pal[2], wrapY); }
        for (int x = 1; x < w - 1; x++) { Put(c, left + x, top, pal[2], wrapY); Put(c, left + x, top + h - 1, pal[2], wrapY); }   // the caps
        for (int y = 1; y < h - 1; y++)
            for (int x = 0; x < w; x++)
            {
                float t = (x + 0.5f) / w, shade = 1 - MathF.Abs(2 * t - 1);
                P colour = shade > 0.66f ? pal[0] : (shade > 0.3f ? pal[1] : pal[2]);
                if ((y == h / 3 || y == 2 * h / 3) && shade > 0.1f) colour = pal[2].Mixed(pal[1], 0.4f);   // the bands of the frame
                Put(c, left + x, top + y, colour, wrapY);
            }
        if (tassel && w >= 5)
        {
            Put(c, cx, top + h, pal[2], wrapY);
            Put(c, cx, top + h + 1, pal[3], wrapY);
        }
    }

    // MARK: The sky (reflection only)

    /// <summary>The night that the water mirrors: dark, with a few thin clouds. No moon, no stars, no lanterns (the lanterns are on the banks and on the water).</summary>
    public static PixelCanvas MakeSky()
    {
        var rng = new SeededGenerator(0xFE57);
        var clouds = new PeriodicNoise(5, 6, rng);
        int w = SkyArt.Width, h = SkyArt.Height;
        var c = new PixelCanvas(w, h);
        P top = P.Hex(0x0A0614), bottom = P.Hex(0x261238);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                P colour = top.Mixed(bottom, (float)y / h);
                float n = clouds.Sample((float)x / w, (float)y / h);
                if (n > 0.62f)
                {
                    float level = MathF.Floor(MathF.Min(1, (n - 0.62f) * 6) * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.7f) / 3;
                    if (level > 0) colour = colour.Mixed(P.Hex(0x4A2A58), level * 0.4f);
                }
                c[x, y] = colour;
            }
        return c;
    }

    // MARK: The banks

    /// <summary>The posts that carry the lamps and the strings of lights: where they stand (rows) along the bank.</summary>
    private static readonly int[] PostRows = { 12, 76, 140, 204 };

    private static int EdgeOf(int y, BankArt.Side side) => BankArt.Edge(y, side == BankArt.Side.Left ? 0.4f : 2.2f);


    private static PixelCanvas Oriented(PixelCanvas left, BankArt.Side side) => side == BankArt.Side.Left ? left : Flip(left);

    /// <summary>The ground of the festival, seen from above and drawn over the boat: the dark earth lit warm by the lamps, the boardwalk with its piles, the tents, and the shadows of the crowd.</summary>
    public static PixelCanvas MakeGround(BankArt.Side side)
    {
        var rng = new SeededGenerator(side == BankArt.Side.Left ? 0xF00DUL : 0xF00EUL);
        var c = new PixelCanvas(Width, Height);
        var lumps = new PeriodicNoise(6, 12, rng);

        // Earth.
        for (int y = 0; y < Height; y++)
        {
            int e = EdgeOf(y, side);
            for (int x = 0; x < e; x++)
            {
                float v = lumps.Sample((float)x / Width, (float)y / Height) * 0.9f + 0.15f * (float)x / e;
                c[x, y] = Earth[Math.Clamp((int)(v * 4), 0, 3)];
            }
        }
        // Lamplight on the ground.
        foreach (int row in PostRows) Glow(c, EdgeOf(row, side) - 4, row, 26, Warm, 0.5f, wrapY: true);

        // Boardwalk: planks across the last few columns before the water, a lamp post at every light.
        for (int y = 0; y < Height; y++)
        {
            int e = EdgeOf(y, side);
            for (int x = Math.Max(0, e - 6); x < e; x++)
            {
                int plank = (y / 4) % 2;
                P colour = Wood[x >= e - 1 ? 0 : (y % 4 == 0 ? 1 : 2 + plank)];
                foreach (int row in PostRows)
                {
                    float d = Hypot(x - (e - 4), y - row);
                    if (d < 22) colour = colour.Mixed(Warm, 0.5f * (1 - d / 22));
                }
                c[x, y] = colour;
            }
        }

        // Tents seen from above: round, with radial stripes, a pole in the middle.
        foreach (int cy in new[] { 44, 108, 172, 236 })
        {
            int e = EdgeOf(cy, side);
            int r = 8 + (cy / 64) % 2 * 2, cx = Math.Max(r + 1, e - 16 - r);
            P a = (cy / 64) % 2 == 0 ? P.Hex(0xA8323A) : P.Hex(0x2A8C94), b = P.Hex(0xD8C8A0);
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    float d = Hypot(dx + 0.5f, dy + 0.5f);
                    if (d > r) continue;
                    float angle = MathF.Atan2(dy + 0.5f, dx + 0.5f);
                    bool stripe = (int)MathF.Floor((angle + MathF.PI) / (2 * MathF.PI) * 10) % 2 == 0;
                    P colour = (stripe ? a : b).Scaled(0.55f);
                    // lit from the side of the lamps
                    float toLamp = ((cx + dx) - (e - 4)) / (float)r;
                    colour = colour.Mixed(Warm, MathF.Max(0, 0.32f * toLamp) * (1 - d / r * 0.4f));
                    if (d > r - 1.2f) colour = colour.Scaled(0.6f);
                    Put(c, cx + dx, cy + dy, colour);
                }
            Put(c, cx, cy, P.Hex(0xFFE0A0)); Put(c, cx - 1, cy, P.Hex(0x8A6A4A)); Put(c, cx, cy - 1, P.Hex(0x8A6A4A));
        }

        // The crowd on the boardwalk and the ground by it: dark figures with a rim of lamplight and a long shadow away from the lamp.
        for (int i = 0; i < 38; i++)
        {
            int y = rng.Range(2, Height - 3);
            int e = EdgeOf(y, side);
            int x = rng.Range(Math.Max(2, e - 14), e - 2);
            bool dancing = rng.Range(0, 3) == 0;
            for (int k = 1; k <= 3; k++) Put(c, x - k, y + 1, c[Math.Clamp(x - k, 0, Width - 1), ((y + 1) % Height)].Scaled(0.55f));   // shadow
            P body = rng.Range(0, 1) == 0 ? P.Hex(0x1A1020) : P.Hex(0x241A2E);
            Put(c, x, y, body); Put(c, x, y - 1, body); Put(c, x, y + 1, body);               // shoulders
            if (dancing) { Put(c, x, y - 2, body); Put(c, x, y + 2, body); }               // arms out
            Put(c, x + 1, y, P.Hex(0x2A1C26));                                                 // the head
            Put(c, x + 1, y - 1, Warm.Scaled(0.7f));                                           // the rim of light, toward the water
        }

        // Piles in the water at the posts, and the posts with their lamps (red paper lanterns seen from above).
        foreach (int row in PostRows)
        {
            int e = EdgeOf(row, side);
            for (int dy = -1; dy <= 1; dy++) for (int dx = 0; dx <= 2; dx++) Put(c, e + dx, row + dy, Wood[dx == 2 || dy == 1 ? 0 : 1]);
            for (int dy = -2; dy <= 2; dy++) for (int dx = -2; dx <= 2; dx++)
                if (Hypot(dx, dy) <= 2.3f) Put(c, e - 4 + dx, row + dy, Lanterns[row / 64 % 2 == 0 ? 0 : 1][Hypot(dx, dy) < 1.2f ? 0 : 1]);
        }
        return Oriented(c, side);
    }

    /// <summary>Foam where the piles meet the water, in two sizes that the scene alternates between.</summary>
    public static PixelCanvas[] MakeRipples(BankArt.Side side)
    {
        var foam = P.Hex(0xE8D0E8);
        var result = new PixelCanvas[2];
        for (int variant = 0; variant < 2; variant++)
        {
            var c = new PixelCanvas(Width, Height);
            foreach (int row in PostRows)
            {
                int tx = EdgeOf(row, side) + 3;
                float rx = 2.5f + variant * 1.5f, ry = 2.2f + variant * 0.8f;
                for (int i = 0; i < 28; i++)
                {
                    if (!(i % 3 != 0 && i < 20)) continue;
                    float a = (float)i / 28 * 2 * MathF.PI;
                    c.SetWrappedY(tx + (int)BossArt.Rnd(MathF.Cos(a) * rx), row + (int)BossArt.Rnd(MathF.Sin(a) * ry), foam.WithAlpha((byte)(variant == 0 ? 120 : 70)));
                }
            }
            result[variant] = Oriented(c, side);
        }
        return result;
    }

    /// <summary>Over the water, near the camera, and drawn over the boat: strings of party lights from post to post with bunting and a few paper lanterns.</summary>
    public static PixelCanvas MakeStrings(BankArt.Side side)
    {
        var rng = new SeededGenerator(side == BankArt.Side.Left ? 0xBA11UL : 0xBA12UL);
        var c = new PixelCanvas(Width, Height);
        P cord = P.Hex(0x1A1018);
        for (int i = 0; i < PostRows.Length; i++)
        {
            int y0 = PostRows[i], y1 = PostRows[i] + 64;
            int e0 = EdgeOf(y0, side), e1 = EdgeOf(y1 % Height, side);
            float reach = 14 + 8 * ((i + (side == BankArt.Side.Left ? 0 : 1)) % 2);
            // The string leaves the first post, swings out over the water and comes back to the next post.
            var points = new List<(int x, int y)>();
            const int steps = 64;
            for (int s = 0; s <= steps; s++)
            {
                float t = (float)s / steps;
                float x = Mathf.Lerp(e0 - 4, e1 - 4, t) + MathF.Sin(t * MathF.PI) * reach;
                points.Add(((int)MathF.Round(x), y0 + s));
            }
            int n = 0;
            foreach (var (x, y) in points)
            {
                Put(c, x, y, cord);
                if (n % 6 == 3)
                {
                    // A bulb with its halo.
                    P bulb = Bulbs[(n / 6 + i) % Bulbs.Length];
                    Halo(c, x, y + 1, 3.6f, bulb, 110);
                    Put(c, x, y + 1, bulb); Put(c, x + 1, y + 1, bulb); Put(c, x, y + 2, bulb.Mixed(Warm, 0.5f)); Put(c, x + 1, y + 2, bulb.Mixed(Warm, 0.5f));
                }
                else if (n % 6 == 0 && n % 12 != 0)
                {
                    // Bunting: a little paper flag hanging from the string.
                    P flag = Bulbs[(n / 6 + i + 2) % Bulbs.Length].Scaled(0.8f);
                    Put(c, x - 1, y + 1, flag); Put(c, x, y + 1, flag); Put(c, x + 1, y + 1, flag);
                    Put(c, x, y + 2, flag); Put(c, x, y + 3, flag.Scaled(0.8f));
                }
                n++;
            }
            // A paper lantern hanging from the middle of every other string.
            if (i % 2 == (side == BankArt.Side.Left ? 0 : 1))
            {
                var mid = points[steps / 2];
                Halo(c, mid.x, mid.y + 8, 8, Lanterns[i % LanternTones][3], 90);
                DrawLantern(c, mid.x, mid.y + 3, 6, 9, i + rng.Range(0, 3), cord: true, tassel: true, wrapY: true);
            }
        }
        return Oriented(c, side);
    }

    /// <summary>What the water mirrors of the banks: the roofs of the festival, dark against the lights, and a scatter of lamps, windows and bulbs.</summary>
    public static PixelCanvas MakeReflection(BankArt.Side side)
    {
        var rng = new SeededGenerator(side == BankArt.Side.Left ? 0xC0DAUL : 0xC0DBUL);
        var c = new PixelCanvas(Width + 8, Height);
        P[] roof = { P.Hex(0x120A1C), P.Hex(0x1E1030), P.Hex(0x2C1A44) };
        // The mass of the festival close to the bank, solid, and roofs of tents and little pagodas going out further.
        for (int y = 0; y < Height; y++) for (int x = 0; x < 12; x++) c[x, y] = roof[(x + y / 3) % 7 == 0 ? 1 : 0];
        for (int n = 0; n < 34; n++)
        {
            int cx = rng.Range(0, 20), cy = rng.Range(6, Height - 6), rw = rng.Range(5, 11), rh = rng.Range(3, 7);
            for (int dy = -rh; dy <= rh; dy++)
                for (int dx = -rw; dx <= rw; dx++)
                {
                    // a roof with upturned corners: wide at the middle, narrowing towards the top and bottom
                    float f = 1 - MathF.Abs(dy) / (rh + 0.5f) * 0.55f;
                    if (MathF.Abs(dx) > rw * f) continue;
                    Put(c, cx + dx, cy + dy, roof[dy < 0 ? 2 : (dy == 0 ? 1 : 0)]);
                }
            // the roof edge catches the light of the lamps
            for (int dx = -rw; dx <= rw; dx++) if (dx % 2 == 0) Put(c, cx + dx, cy - rh, Warm.Scaled(0.55f));
        }
        // Lamps, windows and bulbs.
        for (int n = 0; n < 150; n++)
        {
            int x = rng.Range(1, 26), y = rng.Range(0, Height - 1);
            P bulb = Bulbs[rng.Range(0, Bulbs.Length - 1)];
            if (n % 5 == 0) { Halo(c, x, y, 4, bulb, 90); Put(c, x, y, bulb); Put(c, x + 1, y, bulb); }
            else Put(c, x, y, bulb.Mixed(Warm, 0.35f));
        }
        foreach (int row in PostRows) { Halo(c, EdgeOf(row, side) + 2, row, 7, Lanterns[0][3], 110); DrawLantern(c, EdgeOf(row, side) + 2, row - 3, 5, 7, 0, cord: false, wrapY: true); }
        return Oriented(c, side);
    }

    /// <summary>A paper lantern floating on the water, seen from above: a glowing disc in a dark rim, with a halo on the water.</summary>
    public static PixelCanvas MakeFloatingLantern(int tone)
    {
        var pal = Lanterns[((tone % LanternTones) + LanternTones) % LanternTones];
        var c = new PixelCanvas(17, 17);
        Halo(c, 8, 8, 8.5f, pal[3], 120, wrapY: false);
        for (int y = 0; y < 17; y++)
            for (int x = 0; x < 17; x++)
            {
                float d = Hypot(x - 8, y - 8);
                if (d <= 3.0f) c[x, y] = d < 1.6f ? pal[0] : pal[1];
                else if (d <= 4.0f) c[x, y] = pal[2];
            }
        return c;
    }
}
