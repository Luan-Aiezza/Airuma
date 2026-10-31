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
