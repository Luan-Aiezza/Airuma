using System;
using P = PixelColor;

/// <summary>
/// The look of the HUD's side panel. Every stage has its own, made after its boss: the carved bark and urucum red of the original
/// panel, or, for stage 1 and its great fire serpent, a panel of dark scales framed in embers with a band of fiery scales at its edge
/// and a serpent coiled at the bottom. A theme only changes colours and ornaments, never the layout.
/// </summary>
public sealed class HudTheme
{
    public string Name = "";
    /// <summary>The panel's background: the colour of a pixel.</summary>
    public Func<int, int, P> Background;
    /// <summary>The 8 pixel wide band on the panel's left edge (x 0..7).</summary>
    public Func<int, int, P> Band;
    public P Ink, RimInner, BeadLight, BeadDark;
    public P PlaqueEdgeA, PlaqueEdgeB, PlaqueFill;
    public P RuleA, RuleB;
    public P FrameDot, FrameBase, FrameLine;
    /// <summary>An ornament for the free space at the bottom of the panel, or null.</summary>
    public Func<PixelCanvas> Ornament;

    /// <summary>The panel as it was before there were themes.</summary>
    public static HudTheme Original()
    {
        var diamond = new[] { "...##...", "..#..#..", ".#.##.#.", "#.#..#.#", "#.#..#.#", ".#.##.#.", "..#..#..", "...##..." };
        return new HudTheme
        {
            Name = "original",
            Background = (x, y) => ((x + y) % 4 == 0 || (x - y + 400) % 8 == 0) ? HudArt.Colors.BarkLight : HudArt.Colors.Bark,
            Band = (x, y) => diamond[y % 8][x] == '#' ? HudArt.Colors.Cream : HudArt.Colors.Urucum,
            Ink = HudArt.Colors.Ink, RimInner = HudArt.Colors.UrucumDark, BeadLight = HudArt.Colors.WoodLight, BeadDark = HudArt.Colors.Wood,
            PlaqueEdgeA = HudArt.Colors.Ochre, PlaqueEdgeB = HudArt.Colors.WoodLight, PlaqueFill = HudArt.Colors.DeepInk,
            RuleA = HudArt.Colors.Ochre, RuleB = HudArt.Colors.Urucum,
            FrameDot = HudArt.Colors.Cream, FrameBase = HudArt.Colors.Urucum, FrameLine = HudArt.Colors.Ink,
        };
    }

    /// <summary>Stage 1, the great serpent of fire under the river.</summary>
    public static HudTheme FireSerpent()
    {
        P scaleEdge = P.Hex(0x120808), scaleDark = P.Hex(0x241214), scaleLight = P.Hex(0x3C1A1A);
        return new HudTheme
        {
            Name = "fire serpent",
            // Dark scales laid in offset rows, like fish scales, each with a lit top edge.
            Background = (x, y) =>
            {
                int row = y / 6;
                int u = (x + (row % 2) * 3) % 6, v = y % 6;
                if (u == 5 || v == 5) return scaleEdge;
                return v == 0 || (v == 1 && u < 2) ? scaleLight : scaleDark;
            },
            // A column of glowing scales: embers at the top of each, shading to dark red.
            Band = (x, y) =>
            {
                int row = y / 4;
                int u = (x + (row % 2) * 2) % 4, v = y % 4;
                if (u == 3 || v == 3) return P.Hex(0x1A0808);
                // a few scales glow like embers; the rest are dark red
                bool ember = (row * 3 + (x + (row % 2) * 2) / 4 * 5) % 7 == 0;
                if (ember) return v switch { 0 => P.Hex(0xFFB347), 1 => P.Hex(0xF25C12), _ => P.Hex(0xC42A12) };
                return v switch { 0 => P.Hex(0xA32414), 1 => P.Hex(0x7C1A12), _ => P.Hex(0x4E100E) };
            },
            Ink = P.Hex(0x120808), RimInner = P.Hex(0xC42A12), BeadLight = P.Hex(0xFFB347), BeadDark = P.Hex(0xC42A12),
            PlaqueEdgeA = P.Hex(0xF25C12), PlaqueEdgeB = P.Hex(0x7A1410), PlaqueFill = P.Hex(0x120D11),
            RuleA = P.Hex(0xFF9A1F), RuleB = P.Hex(0xC42A12),
            FrameDot = P.Hex(0xFFB347), FrameBase = P.Hex(0xC42A12), FrameLine = P.Hex(0x120808),
            Ornament = SerpentOrnament,
        };
    }

    /// <summary>Stage 2, the river of the parties: a plum panel strewn with confetti, a column of bunting along its edge, and a string of paper lanterns at the bottom.</summary>
    public static HudTheme Festival()
    {
        P[] flags = { P.Hex(0xE23C34), P.Hex(0xFFD060), P.Hex(0x2FBFB0), P.Hex(0xE0508A) };
        P[] confetti = { P.Hex(0x7A2A5E), P.Hex(0x8A6A2A), P.Hex(0x1F6E72), P.Hex(0x8E2A3A) };
        return new HudTheme
        {
            Name = "festival",
            Background = (x, y) =>
            {
                int hash = (x * 73 + y * 151 + (x / 5) * 31) % 97;
                if (hash == 0) return confetti[(x + y) % confetti.Length];
                return ((x + y) % 6 == 0 || (x - y + 600) % 12 == 0) ? P.Hex(0x2A1840) : P.Hex(0x1E1030);
            },
            // Bunting: little flags hanging in a column, in turns of red, gold, teal and pink, each with a lit top.
            Band = (x, y) =>
            {
                int v = y % 10, index = (y / 10) % flags.Length;
                float half = 3.9f - v * 0.42f;
                if (v < 9 && MathF.Abs(x - 3.5f) <= half) return v == 0 ? flags[index].Mixed(P.Hex(0xFFFFFF), 0.35f) : (v > 6 ? flags[index].Scaled(0.75f) : flags[index]);
                return P.Hex(0x140A1E);
            },
            Ink = P.Hex(0x0E0714), RimInner = P.Hex(0xE0508A), BeadLight = P.Hex(0xFFD870), BeadDark = P.Hex(0xE23C34),
            PlaqueEdgeA = P.Hex(0xFFD060), PlaqueEdgeB = P.Hex(0x8E2A5E), PlaqueFill = P.Hex(0x120A1C),
            RuleA = P.Hex(0xFFA03A), RuleB = P.Hex(0xE0508A),
            FrameDot = P.Hex(0xFFE27A), FrameBase = P.Hex(0xE0508A), FrameLine = P.Hex(0x0E0714),
            Ornament = LanternOrnament,
        };
    }

    /// <summary>
    /// Stage 3, the storm of the cat of the stars: a night-blue panel with stars scattered over it and faint streaks of rain, a column of lightning
    /// bolts and little stars along its edge, and a constellation, struck through by a bolt, at the bottom.
    /// </summary>
    public static HudTheme Storm()
    {
        P night = P.Hex(0x0B1224), nightLight = P.Hex(0x131D38), streak = P.Hex(0x1C2A4C), cloud = P.Hex(0x080D1A);
        return new HudTheme
        {
            Name = "storm",
            // Dark clouds, rain slanting through them, and here and there a star.
            Background = (x, y) =>
            {
                int hash = (x * 73 + y * 151 + (x / 7) * 29) % 113;
                if (hash == 0) return P.Hex(0xFFF6C8);
                if (hash == 1 && (x + y) % 2 == 0) return P.Hex(0x9AB4E8);
                if ((x * 3 + y) % 23 == 0 || (x * 3 + y) % 23 == 1 && y % 3 != 0) return streak;
                return (x / 6 + y / 5) % 5 == 0 ? nightLight : ((x + y * 2) % 9 == 0 ? cloud : night);
            },
            // Lightning along the edge: a bolt of white zigzagging down a dark column, with a star between the bolts.
            Band = (x, y) =>
            {
                int v = y % 32;
                float centre = v < 6 ? 2 + v * 0.55f : (v < 12 ? 5.3f - (v - 6) * 0.9f : (v < 18 ? 0 + (v - 12) * 0.7f : 4.2f));
                if (v < 20 && MathF.Abs(x - 3.5f - (centre - 3)) < 1.1f) return v % 2 == 0 ? P.Hex(0xFFFFFF) : P.Hex(0xDCE8FF);
                if (v < 20 && MathF.Abs(x - 3.5f - (centre - 3)) < 2.1f) return P.Hex(0x3A4E88);
                if (v == 26 && (x == 3 || x == 4)) return P.Hex(0xFFF2B0);
                if ((v == 25 || v == 27) && (x == 3 || x == 4)) return P.Hex(0xB8A868);
                return P.Hex(0x0A1020);
            },
            Ink = P.Hex(0x070A14), RimInner = P.Hex(0x6A8ADC), BeadLight = P.Hex(0xDCE8FF), BeadDark = P.Hex(0x5A6EA8),
            PlaqueEdgeA = P.Hex(0xA8C0F0), PlaqueEdgeB = P.Hex(0x2A3A6A), PlaqueFill = P.Hex(0x080C18),
            RuleA = P.Hex(0xDCE8FF), RuleB = P.Hex(0x6A8ADC),
            FrameDot = P.Hex(0xFFF2B0), FrameBase = P.Hex(0x5A6EA8), FrameLine = P.Hex(0x070A14),
            Ornament = ConstellationOrnament,
        };
    }

    /// <summary>
    /// Stage 4, the hunt and the beast: the panel is the hide of a bear, in short strokes of fur, with the scratches of claws torn across it (three lines, with a red edge);
    /// the bark of a tree along its edge, raked by the claw marks of a bear; plaques framed in leather and bone; and, at the bottom, a bear-skin rug, its head pointing down.
    /// </summary>
    public static HudTheme Cave()
    {
        P fur = P.Hex(0x2A1A10), furLight = P.Hex(0x3C2616), furDark = P.Hex(0x1C110A);
        return new HudTheme
        {
            Name = "hunt",
            // Short strokes of fur, and here and there the scratches of a claw: three parallel lines torn across the hide, dark with a raw edge.
            Background = (x, y) =>
            {
                int cell = 56, cx = x / cell, cy = y / cell, lx = x % cell, ly = y % cell;
                if (((cx * 7 + cy * 13) % 5) == 0)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        float line = 12 + k * 9 + ly * 0.55f;
                        if (ly >= 6 && ly <= 46 && MathF.Abs(lx - line) < 0.9f) return P.Hex(0x0E0804);
                        if (ly >= 6 && ly <= 46 && MathF.Abs(lx - line - 1.6f) < 0.8f) return P.Hex(0x8A2E1E);
                    }
                }
                int stroke = (x * 5 + (y / 2) * 11 + (x / 3) * 7) % 17;
                if (stroke == 0 && y % 2 == 0) return furLight;
                if (stroke == 1 && y % 2 == 1) return furLight;
                if ((x * 3 + y * 5) % 19 == 0) return furDark;
                return fur;
            },
            // The bark of a tree torn by a bear: dark bark in vertical streaks, and long claw marks, three or four gouges side by side that show the pale wood and have
            // splinters of bark at their ends.
            Band = (x, y) =>
            {
                int period = 52, block = y / period, v = y % period;
                int shift = new[] { 0, 1, 0, 2, 1 }[block % 5];              // the marks do not repeat exactly
                int lean = v * 3 / 40;                                         // the gouges lean a little as they go down
                for (int k = 0; k < 3 + (block % 2); k++)
                {
                    int gx = 1 + k * 2 + shift - lean / 2;
                    int top = 5 + k * 2 + (k % 2), len = 30 - k * 2 - (block % 3);
                    if (v >= top && v < top + len && x == gx + (v - top) / 14)
                    {
                        if (v < top + 2 || v >= top + len - 3) return P.Hex(0xA8764A);                      // the ends, where the bark is lifted
                        return (v + k) % 3 == 0 ? P.Hex(0xF0D098) : P.Hex(0xD9AC70);                       // the wood, pale and raw
                    }
                    if (v >= top && v < top + len && (x == gx + (v - top) / 14 + 1)) return P.Hex(0x4A2A14);     // the lip of the gouge, in shadow
                }
                // splinters of bark curling out beside the marks
                if ((v == 4 || v == 38) && x == 6 - shift) return P.Hex(0x7A5230);
                int streak = (x * 7 + (y / 6) * 5) % 9;
                if (streak == 0) return P.Hex(0x1C100A);
                if (streak == 4 || (x == 7 && y % 3 == 0)) return P.Hex(0x4A3220);
                return (x + y / 2) % 7 == 0 ? P.Hex(0x3E281A) : P.Hex(0x2E1D12);
            },
            Ink = P.Hex(0x0E0805), RimInner = P.Hex(0x9A3A22), BeadLight = P.Hex(0xE8DCC0), BeadDark = P.Hex(0x9A8A6A),
            PlaqueEdgeA = P.Hex(0xB88A58), PlaqueEdgeB = P.Hex(0x4A2E1A), PlaqueFill = P.Hex(0x120C08),
            RuleA = P.Hex(0xD8CAA8), RuleB = P.Hex(0x9A2A1E),
            FrameDot = P.Hex(0xE8DCC0), FrameBase = P.Hex(0x8A5A32), FrameLine = P.Hex(0x0E0805),
            Ornament = BearRugOrnament,
        };
    }

    /// <summary>
    /// Stage 5, the divinity under the mountains of the Andes: the panel is Inca stonework, blocks of dark blue-grey laid in courses with a lit top edge; along its edge runs a column of
    /// tocapu (stepped squares and crosses in crimson, gold and indigo); the plaques are framed in gold; and, at the bottom, a range of snowy peaks under a golden sun with its rays.
    /// </summary>
    public static HudTheme Andes()
    {
        P joint = P.Hex(0x0C0E1C), stone = P.Hex(0x1C2240), stoneLight = P.Hex(0x2A3258), stoneDark = P.Hex(0x161B34), lit = P.Hex(0x3A4474);
        P gold = P.Hex(0xF2C14E), goldDark = P.Hex(0xB8862B), crimson = P.Hex(0xB62A30), indigo = P.Hex(0x3C34A4), ice = P.Hex(0xA8D0F0);
        // a chakana (the stepped Andean cross) and a stepped square, 8 x 8 each
        string[] cross = { "...##...", "..####..", ".######.", "########", "########", ".######.", "..####..", "...##..." };
        string[] square = { "########", "#......#", "#.####.#", "#.#..#.#", "#.#..#.#", "#.####.#", "#......#", "########" };
        return new HudTheme
        {
            Name = "andes",
            // Ashlar: courses of 9 rows, the blocks of each course offset from the last; every block lit on its top edge and its left, and worn in places.
            Background = (x, y) =>
            {
                int course = y / 9, v = y % 9;
                int width = 14 + (course % 3) * 2, u = (x + (course * 7) % width) % width;
                if (v == 8 || u == width - 1) return joint;
                if (v == 0 || u == 0) return lit;
                int wear = (x * 31 + y * 17 + course * 13) % 23;
                if (wear == 0) return stoneLight;
                if (wear == 1 || (x + y * 3) % 29 == 0) return stoneDark;
                return ((course * 5 + (x + (course * 7) % width) / width) % 4 == 0) ? stoneLight : stone;
            },
            // Tocapu: a chakana in gold on crimson, then a stepped square in ice on indigo, each in its 8 x 8 cell, with a dark line between.
            Band = (x, y) =>
            {
                int cell = (y / 10) % 2, v = y % 10;
                if (v >= 8) return joint;
                char mark = (cell == 0 ? cross : square)[v][x];
                if (cell == 0) return mark == '#' ? ((x + v) % 4 == 0 ? P.Hex(0xFFF0A8) : gold) : crimson;
                return mark == '#' ? ice : indigo;
            },
            Ink = P.Hex(0x080A16), RimInner = goldDark, BeadLight = P.Hex(0xFFF0A8), BeadDark = goldDark,
            PlaqueEdgeA = gold, PlaqueEdgeB = P.Hex(0x4A3A8A), PlaqueFill = P.Hex(0x0A0C1A),
            RuleA = gold, RuleB = crimson,
            FrameDot = P.Hex(0xFFF0A8), FrameBase = indigo, FrameLine = P.Hex(0x080A16),
            Ornament = MountainOrnament,
        };
    }

    /// <summary>A range of snowy peaks across the bottom of the panel, with the sun of the Andes (a gold disc and its rays) rising between them, and a few stars.</summary>
    public static PixelCanvas MountainOrnament()
    {
        var c = new PixelCanvas(OrnamentWidth, OrnamentHeight);
        P gold = P.Hex(0xF2C14E), goldLight = P.Hex(0xFFF0A8), ray = P.Hex(0x8A6A2A);
        const float sunX = 37f, sunY = 11f;
        // rays
        for (int k = 0; k < 14; k++)
        {
            float a = -MathF.PI + k * MathF.PI / 13f;
            for (float r = 6.5f; r < (k % 2 == 0 ? 15f : 11f); r += 0.5f)
            {
                int x = (int)MathF.Round(sunX + MathF.Cos(a) * r * 1.5f), y = (int)MathF.Round(sunY + MathF.Sin(a) * r * 0.9f);
                if (c.Contains(x, y)) c[x, y] = r < 10f ? gold : ray;
            }
        }
        PixelPaint.Ellipse(c, sunX, sunY, 6f, 5f, gold);
        PixelPaint.Ellipse(c, sunX - 1f, sunY - 1f, 3.6f, 3f, goldLight);
        foreach (var (x, y) in new[] { (5, 3), (14, 6), (60, 4), (68, 8), (26, 2), (50, 2) }) if (c.Contains(x, y)) c[x, y] = P.Hex(0xE8F6FF);
        // two ranges: a far one in indigo, a near one in dark blue with snow on the peaks
        float Far(int x) => 17f - 4.5f * MathF.Abs(MathF.Sin(x * 0.11f + 0.6f)) - 2f * MathF.Sin(x * 0.31f);
        float Near(int x) => 20f - 9f * MathF.Pow(MathF.Abs(MathF.Sin(x * 0.085f + 1.1f)), 0.8f) - 1.5f * MathF.Sin(x * 0.4f);
        for (int x = 0; x < OrnamentWidth; x++)
            for (int y = (int)Far(x); y < OrnamentHeight; y++) if (c.Contains(x, y)) c[x, y] = (x + y) % 5 == 0 ? P.Hex(0x4A42B8) : P.Hex(0x3C34A4);
        for (int x = 0; x < OrnamentWidth; x++)
        {
            int top = (int)Near(x);
            for (int y = top; y < OrnamentHeight; y++)
            {
                if (!c.Contains(x, y)) continue;
                int depth = y - top;
                bool snow = Near(x) < 14.5f && depth < 2 + (int)((14.5f - Near(x)) * 0.6f) && (depth < 1 || (x + y) % 3 != 0);
                c[x, y] = snow ? (depth == 0 ? P.Hex(0xFFFFFF) : P.Hex(0xDCE8FF)) : (x % 7 < 3 ? P.Hex(0x1C2240) : P.Hex(0x141A34));
            }
            if (c.Contains(x, top - 1) && c[x, top - 1].A == 0) c[x, top - 1] = P.Hex(0x080A16);
        }
        return c;
    }

    /// <summary>
    /// At the bottom of the panel, a bear-skin rug laid flat, seen from above, with its head pointing down: only the head (the ears, the shut eyes, the snout and the
    /// nose) and the two front paws, with their claws, on either side of it, are in the picture; the body goes on above, out of it. Brown fur on a border of red felt.
    /// </summary>
    public static PixelCanvas BearRugOrnament()
    {
        var c = new PixelCanvas(OrnamentWidth, OrnamentHeight);
        P[] fur = { P.Hex(0x9A6A3A), P.Hex(0x7A4E28), P.Hex(0x57361C), P.Hex(0x3A2412) };
        P felt = P.Hex(0x8A2218), feltDark = P.Hex(0x5A140E), claw = P.Hex(0xEFE4C8), clawDark = P.Hex(0xB8A888), dark = P.Hex(0x120A06);
        P snout = P.Hex(0xC8A070), snoutDark = P.Hex(0x9A7448), earInside = P.Hex(0x3E2616);
        const float mid = OrnamentWidth / 2f;
        var shape = new PixelCanvas(OrnamentWidth, OrnamentHeight);
        void Blob(float cx, float cy, float rx, float ry)
        {
            for (int y = 0; y < shape.Height; y++)
                for (int x = 0; x < shape.Width; x++)
                {
                    float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry;
                    if (dx * dx + dy * dy <= 1) shape[x, y] = fur[1];
                }
        }
        // the skin: the shoulders (going up out of the picture), the front legs, the head, the ears and the snout
        Blob(mid, -5f, 27f, 12f);
        Blob(mid - 21f, 6.5f, 8f, 5.4f); Blob(mid + 21f, 6.5f, 8f, 5.4f);
        Blob(mid, 8.2f, 7f, 5.6f);
        Blob(mid - 6.4f, 3.6f, 2.9f, 2.9f); Blob(mid + 6.4f, 3.6f, 2.9f, 2.9f);
        Blob(mid, 12.8f, 3.6f, 3.0f);
        // the felt border, all round the skin (the skin goes on above, so there is none on top)
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                if (shape[x, y].A > 0) continue;
                for (int dy = -1; dy <= 1; dy++)
                    for (int dx = -1; dx <= 1; dx++)
                        if (shape.Contains(x + dx, y + dy) && shape[x + dx, y + dy].A > 0) { c[x, y] = (x + y) % 2 == 0 ? felt : feltDark; goto next; }
                next:;
            }
        // the fur: lit from the upper left, in strokes
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                if (shape[x, y].A == 0) continue;
                float light = 0.6f - (x / 74f) * 0.25f - y / 40f + ((x * 3 + y * 5) % 7 == 0 ? 0.25f : 0) - ((x + y * 2) % 11 == 0 ? 0.25f : 0);
                c[x, y] = Heightfield.Pick(fur, light, x, y);
            }
        // the face: the ears (inside darker), the shut eyes, the lighter snout and the nose
        foreach (int side in new[] { -1, 1 })
        {
            int ex = (int)(mid + side * 6.4f);
            for (int dy = 0; dy < 2; dy++) for (int dx = 0; dx < 2; dx++) if (c.Contains(ex + dx - 1, 3 + dy)) c[ex + dx - 1, 3 + dy] = earInside;
            int eyeX = (int)(mid + side * 3.2f);
            c[eyeX, 8] = dark; c[eyeX + (side > 0 ? 1 : -1), 8] = dark; c[eyeX, 7] = fur[3];
        }
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                float dx = (x + 0.5f - mid) / 3.3f, dy = (y + 0.5f - 12.4f) / 2.7f;
                if (dx * dx + dy * dy <= 1) c[x, y] = dy < -0.25f ? snoutDark : snout;
            }
        c[(int)mid - 1, 13] = dark; c[(int)mid, 13] = dark; c[(int)mid - 1, 14] = dark; c[(int)mid, 14] = dark;
        c[(int)mid, 15] = dark;
        // the claws of the front paws, pointing down
        foreach (int side in new[] { -1, 1 })
            for (int i = -1; i <= 1; i++)
            {
                int x = (int)(mid + side * 21 + i * 3);
                if (c.Contains(x, 11)) c[x, 11] = claw;
                if (c.Contains(x, 12)) c[x, 12] = clawDark;
            }
        return c;
    }

    /// <summary>A constellation across the bottom of the panel: stars joined by thin lines, and a bolt of lightning striking through it.</summary>
    public static PixelCanvas ConstellationOrnament()
    {
        var c = new PixelCanvas(OrnamentWidth, OrnamentHeight);
        var stars = new[] { (6, 15), (16, 7), (29, 13), (41, 5), (53, 12), (66, 8) };
        void Line(int x0, int y0, int x1, int y1, P colour)
        {
            int n = Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0));
            for (int i = 0; i <= n; i++) { int x = x0 + (x1 - x0) * i / Math.Max(1, n), y = y0 + (y1 - y0) * i / Math.Max(1, n); if (c.Contains(x, y)) c[x, y] = colour; }
        }
        for (int i = 0; i + 1 < stars.Length; i++) Line(stars[i].Item1, stars[i].Item2, stars[i + 1].Item1, stars[i + 1].Item2, P.Hex(0x5A74B8));
        // a bolt of lightning from the top, zigzagging through the constellation
        int bx = 35;
        for (int y = 0; y < OrnamentHeight; y++)
        {
            int leg = (y / 4) % 2 == 0 ? 1 : -1;
            bx += leg;
            if (c.Contains(bx, y)) { c[bx, y] = P.Hex(0xFFFFFF); if (c.Contains(bx + 1, y)) c[bx + 1, y] = P.Hex(0x9AB4E8); }
        }
        foreach (var (sx, sy) in stars)
        {
            c[sx, sy] = P.Hex(0xFFFFFF);
            foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) if (c.Contains(sx + dx, sy + dy)) c[sx + dx, sy + dy] = P.Hex(0xFFF2B0);
            foreach (var (dx, dy) in new[] { (2, 0), (-2, 0), (0, 2), (0, -2) }) if (c.Contains(sx + dx, sy + dy)) c[sx + dx, sy + dy] = P.Hex(0xB8A868);
        }
        return c;
    }

    /// <summary>A string of paper lanterns across the bottom of the panel, in the colours of the festival.</summary>
    public static PixelCanvas LanternOrnament()
    {
        var c = new PixelCanvas(OrnamentWidth, OrnamentHeight);
        P cord = P.Hex(0x0E0714);
        var xs = new[] { 9, 23, 37, 51, 65 };
        for (int x = 0; x < OrnamentWidth; x++) c[x, 2 + (int)MathF.Round(2.2f * MathF.Sin(x * MathF.PI / 14f))] = cord;
        for (int i = 0; i < xs.Length; i++)
        {
            int sag = 2 + (int)MathF.Round(2.2f * MathF.Sin(xs[i] * MathF.PI / 14f));
            FestivalArt.DrawLantern(c, xs[i], sag + 3, 7, 9, i, cord: true, tassel: true);
        }
        return c;
    }

    // MARK: The serpent coiled at the bottom of the panel

    public const int OrnamentWidth = 74, OrnamentHeight = 22;

    /// <summary>A fire serpent swimming across the panel from left to right: dark red at the tail, bright at the head, which has a
    /// yellow slit eye, a forked tongue and a crest of flame.</summary>
    public static PixelCanvas SerpentOrnament()
    {
        var c = new PixelCanvas(OrnamentWidth, OrnamentHeight);
        var ramp = FirePalette.Ramp;   // white-hot .. dark red
        const int bodyEnd = 62;
        for (int x = 0; x < bodyEnd; x++)
        {
            float t = x / (float)bodyEnd;                                   // 0 at the tail
            float yc = 12 + 4.4f * MathF.Sin(x * 0.27f + 0.5f);
            float half = 1.0f + 2.6f * MathF.Min(1, t * 1.6f);              // the body thickens towards the head
            for (int y = (int)MathF.Floor(yc - half - 1); y <= (int)MathF.Ceiling(yc + half + 1); y++)
            {
                if (!c.Contains(x, y)) continue;
                float d = (y + 0.5f - yc) / half;
                if (MathF.Abs(d) > 1) continue;
                // the back is dark and fiery, the belly pale gold
                int index = d < -0.5f ? 4 : (d < 0.15f ? (t > 0.55f ? 3 : 4) : (d < 0.6f ? 2 : 1));
                if (t < 0.25f) index = Math.Min(5, index + 1);
                var color = ramp[index];
                // scales: darker edges every third column, offset by row
                if ((x + (y % 2)) % 3 == 0) color = color.Scaled(0.72f);
                c[x, y] = color;
            }
        }
        // the dark outline
        var copy = c.Clone();
        (int, int)[] n = { (1, 0), (-1, 0), (0, 1), (0, -1) };
        for (int y = 0; y < OrnamentHeight; y++)
            for (int x = 0; x < OrnamentWidth; x++)
            {
                if (copy[x, y].A != 0) continue;
                foreach (var (dx, dy) in n)
                    if (copy.Contains(x + dx, y + dy) && copy[x + dx, y + dy].A > 0) { c[x, y] = P.Hex(0x120808); break; }
            }
        // the head
        float hy = 12 + 4.4f * MathF.Sin(bodyEnd * 0.27f + 0.5f);
        PixelPaint.Ellipse(c, 66.5f, hy, 5.5f, 4.2f, P.Hex(0x120808));
        PixelPaint.Ellipse(c, 66.5f, hy, 4.6f, 3.4f, ramp[3]);
        PixelPaint.Ellipse(c, 66f, hy - 0.8f, 3.2f, 2f, ramp[2]);
        PixelPaint.Rect(c, 68, (int)hy - 2, 69, (int)hy - 1, ramp[0]);                   // the eye...
        PixelPaint.Plot(c, 69, (int)hy - 2, P.Hex(0x120808));                              // ...with its slit
        PixelPaint.Rect(c, 66, (int)hy + 1, 71, (int)hy + 1, ramp[4]);                    // the mouth
        // the forked tongue
        PixelPaint.Plot(c, 72, (int)hy + 1, ramp[4]); PixelPaint.Plot(c, 73, (int)hy, ramp[3]); PixelPaint.Plot(c, 73, (int)hy + 2, ramp[3]);
        // the crest of flame
        int[] crest = { 5, 4, 5, 3, 4 };
        for (int i = 0; i < crest.Length; i++)
            for (int k = 0; k < crest[i]; k++)
            {
                int x = 62 + i, y = (int)hy - 4 - k;
                if (c.Contains(x, y)) c[x, y] = ramp[Math.Min(5, 1 + k * 4 / crest[i])];
            }
        // sparks of ember above the body
        foreach (var (x, y) in new[] { (12, 2), (24, 4), (34, 1), (46, 3), (54, 1) }) if (c.Contains(x, y)) c[x, y] = ramp[2];
        return c;
    }
}
