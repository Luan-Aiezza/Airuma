using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>A 5x7 bitmap font with Portuguese accents, for HUD text drawn in the same pixel art as the rest of the game.</summary>
public static class PixelFont
{
    public const int GlyphHeight = 7;
    public const int Advance = 6; // 5 columns + 1 of spacing

    public sealed class Rendered
    {
        public PixelCanvas Canvas;
        /// <summary>Row of the canvas where capital letters start (so labels can be aligned by their cap line).</summary>
        public int CapTop;
        public Rendered(PixelCanvas canvas, int capTop) { Canvas = canvas; CapTop = capTop; }
    }

    private static readonly Dictionary<char, string[]> Glyphs = new Dictionary<char, string[]>
    {
        ['A'] = new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
        ['B'] = new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." },
        ['C'] = new[] { ".####", "#....", "#....", "#....", "#....", "#....", ".####" },
        ['D'] = new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." },
        ['E'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" },
        ['F'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." },
        ['G'] = new[] { ".####", "#....", "#....", "#.###", "#...#", "#...#", ".###." },
        ['H'] = new[] { "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
        ['I'] = new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "#####" },
        ['J'] = new[] { "..###", "...#.", "...#.", "...#.", "...#.", "#..#.", ".##.." },
        ['K'] = new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" },
        ['L'] = new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" },
        ['M'] = new[] { "#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#" },
        ['N'] = new[] { "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#", "#...#" },
        ['O'] = new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
        ['P'] = new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." },
        ['Q'] = new[] { ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#" },
        ['R'] = new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" },
        ['S'] = new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." },
        ['T'] = new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." },
        ['U'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
        ['V'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.." },
        ['W'] = new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#" },
        ['X'] = new[] { "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#" },
        ['Y'] = new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." },
        ['Z'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####" },
        ['0'] = new[] { ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###." },
        ['1'] = new[] { "..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###." },
        ['2'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####" },
        ['3'] = new[] { "####.", "....#", "....#", ".###.", "....#", "....#", "####." },
        ['4'] = new[] { "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#." },
        ['5'] = new[] { "#####", "#....", "####.", "....#", "....#", "#...#", ".###." },
        ['6'] = new[] { ".###.", "#....", "#....", "####.", "#...#", "#...#", ".###." },
        ['7'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..." },
        ['8'] = new[] { ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###." },
        ['9'] = new[] { ".###.", "#...#", "#...#", ".####", "....#", "....#", ".###." },
        [' '] = new[] { ".....", ".....", ".....", ".....", ".....", ".....", "....." },
        ['.'] = new[] { ".....", ".....", ".....", ".....", ".....", ".##..", ".##.." },
        [','] = new[] { ".....", ".....", ".....", ".....", ".##..", "..#..", ".#..." },
        [':'] = new[] { ".....", ".##..", ".##..", ".....", ".##..", ".##..", "....." },
        ['-'] = new[] { ".....", ".....", ".....", ".###.", ".....", ".....", "....." },
        ['!'] = new[] { "..#..", "..#..", "..#..", "..#..", "..#..", ".....", "..#.." },
        ['?'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".....", "..#.." },
        ['\''] = new[] { "..#..", "..#..", ".#...", ".....", ".....", ".....", "....." },
        ['/'] = new[] { "....#", "....#", "...#.", "..#..", ".#...", "#....", "#...." },
        ['+'] = new[] { ".....", "..#..", "..#..", "#####", "..#..", "..#..", "....." },
        ['%'] = new[] { "##..#", "##..#", "...#.", "..#..", ".#...", "#..##", "#..##" },
        ['*'] = new[] { ".....", "#.#.#", ".###.", "#####", ".###.", "#.#.#", "....." },
        ['\u266A'] = new[] { "..###", "..#.#", "..#..", "..#..", "###..", "###..", "....." },
    };

    private static readonly Dictionary<char, string[]> Accents = new Dictionary<char, string[]>
    {
        ['\u0303'] = new[] { ".##.#", "#.##." }, // tilde
        ['\u0301'] = new[] { "...#.", "..#.." }, // acute
        ['\u0300'] = new[] { ".#...", "..#.." }, // grave
        ['\u0302'] = new[] { "..#..", ".#.#." }, // circumflex
    };
    private static readonly string[] Cedilla = { "..#..", ".##.." };

    private struct Cell
    {
        public char Base;
        public char? Accent;
        public bool Cedilla;
    }

    private static List<string> Elements(string text)
    {
        var list = new List<string>();
        var e = StringInfo.GetTextElementEnumerator(text);
        while (e.MoveNext()) list.Add((string)e.Current);
        return list;
    }

    private static List<Cell> Cells(string text)
    {
        var result = new List<Cell>();
        foreach (var ch in Elements(text.ToUpperInvariant()))
        {
            string d = ch.Normalize(NormalizationForm.FormD);
            char b = d.Length > 0 ? d[0] : ' ';
            char? accent = null;
            bool hasCedilla = false;
            for (int i = 1; i < d.Length; i++)
            {
                if (d[i] == '\u0327') hasCedilla = true; else accent = d[i];
            }
            result.Add(new Cell { Base = Glyphs.ContainsKey(b) ? b : '?', Accent = accent, Cedilla = hasCedilla });
        }
        return result;
    }

    /// <summary>Whether the font has a glyph for <paramref name="character"/> (a text element; any accent is removed).</summary>
    public static bool Supports(string character)
    {
        string d = character.Normalize(NormalizationForm.FormD);
        return d.Length > 0 && Glyphs.ContainsKey(d[0]);
    }

    public static bool Supports(char character) => Supports(character.ToString());

    /// <summary>Width in columns of <paramref name="text"/> (without a shadow).</summary>
    public static int Columns(string text) => Math.Max(0, new StringInfo(text).LengthInTextElements * Advance - 1);

    /// <summary>Draws <paramref name="text"/> (upper-cased). <paramref name="color"/> gets the glyph row (0..6); <paramref name="shadow"/> adds a 1px drop shadow.</summary>
    public static Rendered Render(string text, Func<int, PixelColor> color, PixelColor? shadow = null)
    {
        var list = Cells(text);
        bool hasAccent = list.Exists(c => c.Accent != null);
        bool hasCedilla = list.Exists(c => c.Cedilla);
        int top = hasAccent ? 3 : 0;
        int extra = shadow == null ? 0 : 1;
        int width = Math.Max(1, Columns(text)) + extra;
        int height = top + GlyphHeight + (hasCedilla ? 2 : 0) + extra;
        var canvas = new PixelCanvas(width, height);

        void Draw(string[] rows, int x, int y, Func<int, PixelColor> colorAt)
        {
            for (int r = 0; r < rows.Length; r++)
            {
                var row = rows[r];
                for (int c = 0; c < row.Length; c++)
                {
                    if (row[c] != '#') continue;
                    int px = x + c, py = y + r;
                    if (!canvas.Contains(px, py)) continue;
                    canvas[px, py] = colorAt(r);
                }
            }
        }

        for (int pass = 0; pass < (shadow == null ? 1 : 2); pass++)
        {
            bool isShadow = shadow != null && pass == 0;
            int offset = isShadow ? 1 : 0;
            for (int i = 0; i < list.Count; i++)
            {
                var cell = list[i];
                int x = i * Advance + offset;
                Func<int, PixelColor> paint = r => isShadow ? shadow.Value : color(r);
                Draw(Glyphs[cell.Base], x, top + offset, paint);
                if (cell.Accent is char accent && Accents.TryGetValue(accent, out var rows))
                    Draw(rows, x, offset, _ => paint(0));
                if (cell.Cedilla)
                    Draw(Cedilla, x, top + GlyphHeight + offset, _ => paint(6));
            }
        }
        return new Rendered(canvas, top);
    }
}
