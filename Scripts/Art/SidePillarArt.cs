using System;
using P = PixelColor;

/// <summary>
/// The art for the sides of the picture, which show when the screen is wider than the game: a pillar of ancient Andean stone,
/// the fitted, many-sided blocks of Inca walls with a stepped cross (a chakana) carved in it, wrapped by two vines with leaves and
/// flowers, and a wall of dark jungle to fill whatever is left beyond it. The light comes from the upper left, from the moon.
/// The pillar is drawn for the left side; the right side is its mirror image.
/// </summary>
public static class SidePillarArt
{
    public const float Scale = 4;
    public const int PillarWidth = 40;
    public const int Height = 194;
    public const int WallWidth = 48;

    internal static readonly P Out = P.Hex(0x0D1417);
    internal static readonly P[] Stone = { P.Hex(0x2A3840), P.Hex(0x3E4F57), P.Hex(0x5A6C70), P.Hex(0x7F9290), P.Hex(0xA8B8AE) }; // dark ... lit
    internal static readonly P[] Leaf = { P.Hex(0x0F3A22), P.Hex(0x1B5C32), P.Hex(0x2F8A45), P.Hex(0x58B85C), P.Hex(0x9BDB78) };
    internal static readonly P[] Flower = { P.Hex(0xA3201C), P.Hex(0xE04E1A), P.Hex(0xFF9A1F), P.Hex(0xFFD23F) };
    private static readonly P Moss = P.Hex(0x2F6B3A);

    private static int Hash(int a, int b = 0, int c = 0)
    {
        unchecked
        {
            long v = ((long)a * 73_856_093L) ^ ((long)b * 19_349_663L) ^ ((long)c * 83_492_791L);
            return (int)(Math.Abs(v) % 1000);
        }
    }

    private static void Put(PixelCanvas c, int x, int y, P color)
    {
        if (c.Contains(x, y)) c[x, y] = color;
    }

    // MARK: The pillar

    /// <summary>x of the middle of the shaft.</summary>
    private const int Mid = 20;
    private const int ShaftHalf = 14;

    private static int HalfWidthAt(int y)
    {
        if (y < 6) return 18;                    // the top step of the capital
        if (y < 12) return 16;                   // second step
        if (y < 16) return ShaftHalf + 1;
        if (y > Height - 8) return 19;           // the plinth
        if (y > Height - 16) return 17;
        if (y > Height - 20) return ShaftHalf + 1;
        return ShaftHalf;
    }

    public static PixelCanvas MakePillar()
    {
        var c = new PixelCanvas(PillarWidth, Height);
        DrawStones(c);
        DrawChakanas(c);
        DrawMoss(c);
        DrawVines(c);
        return c;
    }

    /// <summary>The fitted blocks: rows of irregular height, each cut into stones whose joints do not line up from one row to the next.</summary>
    private static void DrawStones(PixelCanvas c)
    {
        int y = 0, row = 0;
        while (y < Height)
        {
            int h = 9 + Hash(row, 3) % 7;
            if (y < 16) h = y < 6 ? 6 : (y < 12 ? 6 : 4);
            if (Height - y < 20) h = Math.Min(h, Height - y);
            int y1 = Math.Min(Height, y + h);
            int half = HalfWidthAt((y + y1) / 2);
            int left = Mid - half, right = Mid + half;
            // cut the row into stones
            int x = left, k = 0;
            while (x < right)
            {
                int w = 7 + Hash(row, k, 9) % 9;
                if (right - (x + w) < 5) w = right - x;
                DrawStone(c, x, y, x + w, y1, Hash(row, k, 1), left, right);
                x += w; k++;
            }
            y = y1; row++;
        }
    }

    private static void DrawStone(PixelCanvas c, int x0, int y0, int x1, int y1, int tone, int left, int right)
    {
        int baseTone = 1 + tone % 3; // 1..3
        for (int y = y0; y < y1; y++)
        {
            for (int x = x0; x < x1; x++)
            {
                bool edgeL = x == x0, edgeR = x == x1 - 1, edgeT = y == y0, edgeB = y == y1 - 1;
                // rounded corners, like worked stone
                if ((edgeL || edgeR) && (edgeT || edgeB)) continue;
                int t = baseTone;
                // light from the upper left; the whole pillar is rounded, darker towards its right side
                if (edgeT || edgeL) t = Math.Min(4, t + 1);
                else if (edgeB || edgeR) t = Math.Max(0, t - 1);
                float round = (x - Mid) / (float)Math.Max(1, right - left) * 2; // -1 ... +1
                if (round > 0.35f) t = Math.Max(0, t - 1);
                if (round < -0.55f) t = Math.Min(4, t + 1);
                // grain
                int g = Hash(x, y, tone) % 23;
                if (g == 0) t = Math.Max(0, t - 1);
                else if (g == 1) t = Math.Min(4, t + 1);
                c[x, y] = Stone[t];
            }
        }
        // the dark joint under and to the right of each stone
        for (int x = x0; x < x1; x++) Put(c, x, y1 - 1, Stone[0]);
        for (int y = y0; y < y1; y++) Put(c, x1 - 1, y, Stone[0]);
    }

    /// <summary>The outline of the whole pillar, then the stepped crosses carved in the shaft at intervals.</summary>
    private static void DrawChakanas(PixelCanvas c)
    {
        for (int cy = 40; cy < Height - 30; cy += 46)
        {
            // a chakana: a cross whose arms are stepped, 11 pixels across
            string[] rows =
            {
                "....#.#.#....",
                "....#...#....",
                "..###.#.###..",
                "..#.......#..",
                "###.......###",
                "#.....#.....#",
                "###.......###",
                "..#.......#..",
                "..###.#.###..",
                "....#...#....",
                "....#.#.#....",
            };
            int ox = Mid - 6, oy = cy;
            for (int j = 0; j < rows.Length; j++)
                for (int i = 0; i < rows[j].Length; i++)
                {
                    if (rows[j][i] != '#') continue;
                    Put(c, ox + i, oy + j, Stone[0]);
                    Put(c, ox + i - 1, oy + j - 1, Stone[4]); // the lit edge of the cut
                }
        }
        // an outline round the pillar (it stands against the dark wall)
        PixelPaint.Outline(c, Out);
    }

    private static void DrawMoss(PixelCanvas c)
    {
        // moss grows on the tops of the stones, thicker towards the foot
        for (int y = 0; y < Height; y++)
        {
            float lower = y / (float)Height;
            for (int x = 0; x < PillarWidth; x++)
            {
                if (c[x, y].A == 0 || c[x, y] == Out) continue;
                bool underJoint = y > 0 && c[x, y - 1] == Stone[0];
                int chance = (int)(30 + 140 * lower * lower);
                if (underJoint && Hash(x, y, 77) < chance) c[x, y] = Hash(x, y, 5) % 3 == 0 ? Leaf[2] : Moss;
                else if (Hash(x, y, 78) < (int)(14 * lower * lower * 10)) c[x, y] = Hash(x, y, 6) % 2 == 0 ? Moss : Leaf[1];
            }
        }
    }

    // MARK: Vines

    private static void DrawVines(PixelCanvas c)
    {
        // Two vines wind round the shaft, one a half-turn behind the other: in front where the sine is positive.
        for (int v = 0; v < 2; v++)
        {
            float phase = v * MathF.PI;
            for (int y = 14; y < Height - 14; y++)
            {
                float a = y * 0.09f + phase;
                int x = Mid + (int)MathF.Round(MathF.Sin(a) * (ShaftHalf + 1));
                bool front = MathF.Cos(a) > 0;
                if (front)
                {
                    Put(c, x, y, Leaf[1]); Put(c, x + 1, y, Leaf[2]); Put(c, x - 1, y, Leaf[0]);
                    if (Hash(x, y, 31) % 5 == 0) Put(c, x, y, Leaf[3]);
                }
                // leaves along the vine, alternating sides
                if (y % 7 == (v * 3) % 7)
                {
                    int dir = (y / 7 + v) % 2 == 0 ? -1 : 1;
                    DrawLeaf(c, x + dir, y, dir, front);
                    if (Hash(x, y, 91) % 4 == 0) DrawFlower(c, x + dir * 4, y + 2, Hash(x, y, 3) % 3);
                }
                // a tendril that hangs where the vine is at the edge
                if (front && Math.Abs(MathF.Sin(a)) > 0.97f && Hash(y, v, 12) % 3 != 0)
                {
                    int len = 10 + Hash(y, v, 13) % 18;
                    int dir = MathF.Sin(a) > 0 ? 1 : -1;
                    for (int k = 1; k <= len; k++)
                    {
                        int tx = x + dir * (1 + (int)(MathF.Sin(k * 0.5f) * 1.5f));
                        Put(c, tx, y + k, k % 5 == 0 ? Leaf[3] : Leaf[1]);
                        if (k % 6 == 3) DrawLeaf(c, tx + dir, y + k, dir, true);
                    }
                }
            }
        }
        // clumps of plants at the foot and at the head
        for (int x = 2; x < PillarWidth - 2; x += 3)
        {
            int h = 4 + Hash(x, 1, 2) % 9;
            for (int k = 0; k < h; k++) Put(c, x + (k % 3 == 2 ? 1 : 0), Height - 12 - k, k > h - 3 ? Leaf[3] : (k % 2 == 0 ? Leaf[2] : Leaf[1]));
            if (Hash(x, 7, 8) % 3 == 0) DrawFlower(c, x, Height - 13 - h, Hash(x, 9, 9) % 3);
        }
    }

    private static void DrawLeaf(PixelCanvas c, int x, int y, int dir, bool front)
    {
        P body = front ? Leaf[2] : Leaf[0];
        P lit = front ? Leaf[3] : Leaf[1];
        Put(c, x, y, body); Put(c, x + dir, y, body); Put(c, x + 2 * dir, y, lit);
        Put(c, x + dir, y - 1, lit); Put(c, x + dir, y + 1, Leaf[0]);
        Put(c, x + 3 * dir, y, front ? Leaf[4] : Leaf[2]);
    }

    private static void DrawFlower(PixelCanvas c, int x, int y, int kind)
    {
        // a cantuta: red and gold, five little petals round a gold heart
        P petal = kind == 0 ? Flower[0] : (kind == 1 ? Flower[1] : Flower[2]);
        foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }) Put(c, x + dx, y + dy, petal);
        Put(c, x, y, Flower[3]);
        Put(c, x - 1, y - 1, Flower[2]);
    }

    // MARK: The wall of jungle behind

    /// <summary>A dark wall of leaves for the margin beyond the pillar. It tiles sideways.</summary>
    public static PixelCanvas MakeWall()
    {
        var c = new PixelCanvas(WallWidth, Height, P.Hex(0x061014));
        // a faint gradient: lighter towards the top, where the sky shows through the leaves
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < WallWidth; x++)
                if (Bayer.Threshold(x, y) < 0.35f * (1 - y / (float)Height)) c[x, y] = P.Hex(0x0E2328);
        var dark = new[] { P.Hex(0x07161A), P.Hex(0x0C2328), P.Hex(0x12342F), P.Hex(0x1A4A3C) };
        // clumps of leaves
        for (int i = 0; i < 150; i++)
        {
            int cx = Hash(i, 1) % WallWidth, cy = Hash(i, 2) % Height;
            int r = 4 + Hash(i, 3) % 7;
            var color = dark[Hash(i, 4) % dark.Length];
            for (int dy = -r; dy <= r; dy++)
                for (int dx = -r; dx <= r; dx++)
                {
                    if (dx * dx * 0.6f + dy * dy > r * r * 0.6f) continue;
                    int x = ((cx + dx) % WallWidth + WallWidth) % WallWidth, y = cy + dy;
                    if (!c.Contains(x, y)) continue;
                    // the lit side of each clump
                    c[x, y] = dx < 0 && dy < 0 && Hash(x, y, 5) % 3 != 0 ? dark[Math.Min(3, Array.IndexOf(dark, color) + 1)] : color;
                }
        }
        // a few fireflies
        for (int i = 0; i < 9; i++)
        {
            int x = Hash(i, 40) % WallWidth, y = 8 + Hash(i, 41) % (Height - 16);
            Put(c, x, y, P.Hex(0xE8F2A0)); Put(c, x + 1, y, P.Hex(0x7FA860, 140)); Put(c, x - 1, y, P.Hex(0x7FA860, 140));
        }
        return c;
    }
}
