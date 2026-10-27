using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Pixel art for the HUD, in the spirit of Amazonian folk art: urucum red, genipapo black, tabatinga cream,
/// woven bark textures, diamond "grafismos", feathers, and the lantern the story is built around.
/// </summary>
public static class HudArt
{
    public const float Scale = 4;

    public static class Colors
    {
        public static readonly PixelColor Ink = PixelColor.Hex(0x1B1620);       // genipapo black
        public static readonly PixelColor DeepInk = PixelColor.Hex(0x120D11);
        public static readonly PixelColor Bark = PixelColor.Hex(0x2A1B14);
        public static readonly PixelColor BarkLight = PixelColor.Hex(0x33221A);
        public static readonly PixelColor Wood = PixelColor.Hex(0x6B4426);
        public static readonly PixelColor WoodLight = PixelColor.Hex(0x8A5A35);
        public static readonly PixelColor Urucum = PixelColor.Hex(0xB3262B);    // red
        public static readonly PixelColor UrucumDark = PixelColor.Hex(0x7E1A20);
        public static readonly PixelColor Cream = PixelColor.Hex(0xE8D8B0);     // tabatinga
        public static readonly PixelColor Ochre = PixelColor.Hex(0xD9A441);
        public static readonly PixelColor Gold = PixelColor.Hex(0xF2C14E);
        public static readonly PixelColor Dim = PixelColor.Hex(0x3A2E28);
        public static readonly PixelColor Feather = PixelColor.Hex(0x2A6FDB);   // macaw blue
    }

    public static PixelCanvas Canvas(string[] rows, Dictionary<char, PixelColor> legend)
    {
        int w = 0;
        foreach (var r in rows) w = Math.Max(w, r.Length);
        var c = new PixelCanvas(w, rows.Length);
        for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++)
                if (legend.TryGetValue(rows[y][x], out var color)) c[x, y] = color;
        return c;
    }

    // MARK: Panel

    public const int PanelWidth = 88;   // art pixels (352 points)
    public const int PanelHeight = 194; // 776 points

    /// <summary>Woven bark panel with the grafismo band on its left edge. Plaques are drawn in as dark inset boxes.</summary>
    public static PixelCanvas MakePanel(IEnumerable<Rect2> plaques, HudTheme theme = null)
    {
        theme ??= HudTheme.Original();
        var c = new PixelCanvas(PanelWidth, PanelHeight);
        for (int y = 0; y < PanelHeight; y++)
            for (int x = 0; x < PanelWidth; x++)
                c[x, y] = theme.Background(x, y);
        // Grafismo band, outlined in ink with a wooden bead beside it.
        for (int y = 0; y < PanelHeight; y++)
        {
            for (int x = 0; x < 8; x++)
                c[x, y] = theme.Band(x, y);
            c[8, y] = theme.Ink;
            c[9, y] = theme.BeadLight;
            c[10, y] = theme.BeadDark;
        }
        // Right edge and top/bottom rims: ink and urucum.
        for (int y = 0; y < PanelHeight; y++)
        {
            c[PanelWidth - 1, y] = theme.Ink;
            c[PanelWidth - 2, y] = theme.RimInner;
        }
        for (int x = 0; x < PanelWidth; x++)
        {
            c[x, 0] = theme.Ink;
            c[x, 1] = theme.RimInner;
            c[x, PanelHeight - 1] = theme.Ink;
            c[x, PanelHeight - 2] = theme.RimInner;
        }
        foreach (var plaque in plaques) DrawPlaque(c, plaque, theme);
        return c;
    }

    /// <summary>A dark inset box with a cream dotted border and notched corners.</summary>
    private static void DrawPlaque(PixelCanvas c, Rect2 r, HudTheme theme)
    {
        int x0 = (int)r.Position.X, y0 = (int)r.Position.Y, x1 = (int)(r.Position.X + r.Size.X) - 1, y1 = (int)(r.Position.Y + r.Size.Y) - 1;
        for (int y = y0; y <= y1; y++)
        {
            for (int x = x0; x <= x1; x++)
            {
                bool corner = (x == x0 || x == x1) && (y == y0 || y == y1);
                if (corner) continue;
                bool edge = x == x0 || x == x1 || y == y0 || y == y1;
                if (edge)
                    c[x, y] = (x + y) % 2 == 0 ? theme.PlaqueEdgeA : theme.PlaqueEdgeB;
                else
                    c[x, y] = theme.PlaqueFill;
            }
        }
    }

    /// <summary>Thin carved frame laid over the play area's edges.</summary>
    public static PixelCanvas MakeFrameStrip(int length, bool horizontal, HudTheme theme = null)
    {
        theme ??= HudTheme.Original();
        const int thickness = 3;
        var c = new PixelCanvas(horizontal ? length : thickness, horizontal ? thickness : length);
        for (int i = 0; i < length; i++)
        {
            bool dot = i % 4 == 0;
            for (int t = 0; t < thickness; t++)
            {
                var color = t == 1 ? (dot ? theme.FrameDot : theme.FrameBase) : theme.FrameLine;
                if (horizontal) c[i, t] = color; else c[t, i] = color;
            }
        }
        return c;
    }

    // MARK: Card slots

    /// <summary>A square slot (16 x 16) of the card strip: the card's icon in a frame of its rarity's colour, or, with no card, a
    /// dim locked slot with a little padlock.</summary>
    public static PixelCanvas CardSlot(Card? card)
    {
        const int size = 16;
        var c = new PixelCanvas(size, size, Colors.DeepInk);
        PixelColor border = card is Card taken ? CardArt.RarityColor(CardInfo.Rarity(taken)) : Colors.Dim;
        for (int i = 0; i < size; i++) { c[i, 0] = border; c[i, size - 1] = border; c[0, i] = border; c[size - 1, i] = border; }
        c[0, 0] = c[size - 1, 0] = c[0, size - 1] = c[size - 1, size - 1] = Colors.DeepInk;
        if (card is Card shown)
        {
            var icon = CardArt.Icon(shown);
            for (int y = 0; y < icon.Height; y++)
                for (int x = 0; x < icon.Width; x++)
                    if (icon[x, y].A > 0) c[x + 1, y + 1] = icon[x, y];
        }
        else
        {
            string[] lockRows = { ".####.", "#....#", "#....#", "######", "##..##", "##..##", "######" };
            for (int y = 0; y < lockRows.Length; y++)
                for (int x = 0; x < lockRows[y].Length; x++)
                    if (lockRows[y][x] == '#') c[5 + x, 4 + y] = Colors.Dim;
        }
        return c;
    }

    // MARK: Icons

    /// <summary>A little sailboat seen from the side: one per life.</summary>
    public static PixelCanvas BoatIcon(bool filled)
    {
        string[] rows = { "....M....", "....MW...", "....MWW..", "....MWWW.", "....M....", "hhhhhhhhh", ".hHHHHHh.", "..hhhhh.." };
        if (filled)
            return Canvas(rows, new Dictionary<char, PixelColor>
            {
                ['M'] = PixelColor.Hex(0x3A2616), ['W'] = PixelColor.Hex(0xF2EAD2),
                ['h'] = PixelColor.Hex(0xB07A48), ['H'] = PixelColor.Hex(0x8A5A35),
            });
        return Canvas(rows, new Dictionary<char, PixelColor>
        {
            ['M'] = Colors.Dim, ['W'] = Colors.Dim, ['h'] = Colors.Dim, ['H'] = PixelColor.Hex(0x241C19),
        });
    }

    public static PixelCanvas LanternIcon(bool lit)
    {
        string[] rows = { "..FFF..", ".FFFFF.", ".FoLoF.", ".FLYLF.", ".FoLoF.", ".FFFFF.", "..FFF.." };
        if (lit)
            return Canvas(rows, new Dictionary<char, PixelColor>
            {
                ['F'] = PixelColor.Hex(0xC79A3A), ['o'] = PixelColor.Hex(0xFFB23D),
                ['L'] = PixelColor.Hex(0xFFF2A8), ['Y'] = PixelColor.Hex(0xFFFFD8),
            });
        var glass = PixelColor.Hex(0x241C19);
        return Canvas(rows, new Dictionary<char, PixelColor>
        {
            ['F'] = Colors.Dim, ['o'] = glass, ['L'] = glass, ['Y'] = glass,
        });
    }

    public static PixelCanvas FlameIcon(bool lit)
    {
        string[] rows = { "..#..", "..##.", ".###.", ".###.", "#####", "#####", ".###." };
        var ramp = FirePalette.Ramp;
        var colors = new[] { ramp[1], ramp[1], ramp[2], ramp[2], ramp[3], ramp[3], ramp[4] };
        var c = new PixelCanvas(5, 7);
        for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++)
                if (rows[y][x] == '#') c[x, y] = lit ? colors[y] : Colors.Dim;
        return c;
    }

    public static PixelCanvas Compass()
    {
        const int size = 17;
        var c = new PixelCanvas(size, size);
        const float centre = 8.5f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = MathF.Sqrt((x + 0.5f - centre) * (x + 0.5f - centre) + (y + 0.5f - centre) * (y + 0.5f - centre));
                if (d <= 8.2f) c[x, y] = d > 7.0f ? Colors.Ochre : Colors.DeepInk;
            }
        }
        // Cardinal ticks.
        foreach (var (x, y) in new[] { (8, 1), (8, 15), (1, 8), (15, 8) }) c[x, y] = Colors.Cream;
        return c;
    }

    public const int WindPulseFrames = 4;
    public const int WindPulseSize = 29;

    /// <summary>A golden ring that spreads out from the compass, getting fainter: one frame of the wind-change pulse.</summary>
    public static PixelCanvas WindPulse(int frame)
    {
        int size = WindPulseSize;
        var c = new PixelCanvas(size, size);
        float centre = size / 2f;
        float radius = 9.0f + frame * 1.8f;
        var color = Colors.Gold.WithAlpha((byte)Math.Max(0, 255 - frame * 62));
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = MathF.Sqrt(MathF.Pow(x + 0.5f - centre, 2) + MathF.Pow(y + 0.5f - centre, 2));
                // A ring two pixels thick, thinning to one as it spreads, broken up by dithering.
                float thickness = frame < 2 ? 1.1f : 0.7f;
                if (MathF.Abs(d - radius) <= thickness && (frame < 2 || Bayer.Threshold(x, y) < 0.75f)) c[x, y] = color;
            }
        }
        return c;
    }

    /// <summary>Wind arrow pointing up, fletched with macaw feathers; more feathers mean a stronger wind.</summary>
    public static PixelCanvas WindArrow(int strength)
    {
        var c = new PixelCanvas(7, 13);
        for (int y = 3; y <= 11; y++) c[3, y] = Colors.Cream;
        foreach (var (x, y) in new[] { (3, 0), (2, 1), (3, 1), (4, 1), (1, 2), (2, 2), (3, 2), (4, 2), (5, 2) }) c[x, y] = Colors.Gold;
        int[] feathers = { 10, 8, 6 };
        for (int i = 0; i < Math.Max(1, Math.Min(3, strength)); i++)
        {
            int y = feathers[i];
            c[2, y] = Colors.Feather; c[4, y] = Colors.Feather;
            c[1, y + 1] = Colors.Urucum; c[5, y + 1] = Colors.Urucum;
        }
        return c;
    }

    // MARK: Bars and banners

    /// <summary>Frame for the boss health bar: carved ends in urucum, ink body. The fill goes inside, in rows 2...4.</summary>
    public static PixelCanvas BossBarFrame(int width)
    {
        var c = new PixelCanvas(width, 7);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < 7; y++)
            {
                bool rim = y == 0 || y == 6;
                bool line = y == 1 || y == 5;
                c[x, y] = rim ? Colors.Ink : (line ? Colors.Ochre : Colors.DeepInk);
            }
        }
        var capCells = new[] { (1, 0), (0, 1), (1, 1), (2, 1), (0, 2), (1, 2), (2, 2), (0, 3), (1, 3), (2, 3), (0, 4), (1, 4), (2, 4), (1, 5), (1, 6) };
        foreach (int cap in new[] { 0, width - 3 })
        {
            foreach (var (dx, dy) in capCells)
                c[cap + dx, dy] = dy == 0 || dy == 6 ? Colors.Ink : Colors.Urucum;
            c[cap + 1, 3] = Colors.Gold;
        }
        return c;
    }

    /// <summary>Ribbon behind an attack's name. Solid on the right, fading out on the left.</summary>
    public static PixelCanvas Ribbon(int width)
    {
        var c = new PixelCanvas(width, 9);
        for (int x = 0; x < width; x++)
        {
            float fade = Math.Min(1f, x / 28f);
            for (int y = 0; y < 9; y++)
            {
                if (!(Bayer.Threshold(x, y) < fade)) continue;
                PixelColor color;
                switch (y)
                {
                    case 0: case 8: color = Colors.Ochre; break;
                    case 1: case 7: color = Colors.Ink; break;
                    default: color = (x % 14 == 0 && y == 4) ? Colors.Gold : Colors.UrucumDark; break;
                }
                c[x, y] = color;
            }
        }
        return c;
    }

    /// <summary>Wooden frame used for the dialogue box and the end screen.</summary>
    public static PixelCanvas Box(int width, int height)
    {
        var c = new PixelCanvas(width, height, Colors.DeepInk);
        for (int x = 0; x < width; x++)
        {
            c[x, 0] = Colors.Ink;
            c[x, 1] = Colors.WoodLight;
            c[x, height - 1] = Colors.Ink;
            c[x, height - 2] = Colors.WoodLight;
        }
        for (int y = 0; y < height; y++)
        {
            c[0, y] = Colors.Ink;
            c[1, y] = Colors.WoodLight;
            c[width - 1, y] = Colors.Ink;
            c[width - 2, y] = Colors.WoodLight;
        }
        for (int x = 4; x < width - 4; x += 4) { c[x, 2] = Colors.Cream; c[x, height - 3] = Colors.Cream; }
        for (int y = 4; y < height - 4; y += 4) { c[2, y] = Colors.Cream; c[width - 3, y] = Colors.Cream; }
        foreach (var (x, y) in new[] { (0, 0), (width - 1, 0), (0, height - 1), (width - 1, height - 1) })
        {
            for (int dy = 0; dy < 3; dy++)
            {
                for (int dx = 0; dx < 3; dx++)
                {
                    int px = x == 0 ? dx : x - dx, py = y == 0 ? dy : y - dy;
                    c[px, py] = (dx + dy) % 2 == 0 ? Colors.Urucum : Colors.Gold;
                }
            }
        }
        return c;
    }

    /// <summary>A wooden plaque for the restart button.</summary>
    public static PixelCanvas Plaque(int width, int height)
    {
        var c = new PixelCanvas(width, height, Colors.Wood);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (y == 0 || y == height - 1 || x == 0 || x == width - 1) c[x, y] = Colors.Ink;
                else if (y == 1 || x == 1) c[x, y] = Colors.WoodLight;
                else if (y == height - 2 || x == width - 2) c[x, y] = Colors.UrucumDark;
            }
        }
        return c;
    }


    public static void Blit(PixelCanvas src, PixelCanvas dst, int cx, int cy)
    {
        int ox = cx - src.Width / 2, oy = cy - src.Height / 2;
        for (int y = 0; y < src.Height; y++)
            for (int x = 0; x < src.Width; x++)
                if (src[x, y].A > 0 && dst.Contains(ox + x, oy + y))
                    dst[ox + x, oy + y] = src[x, y];
    }

    // MARK: Airu, the lantern fairy

    /// <summary>Framed portrait of Airu (18x18): the flame of the lantern, in two frames, so that it flickers.</summary>
    public static PixelCanvas FairyPortrait(int frame)
    {
        const int size = 18;
        var c = new PixelCanvas(size, size);
        var night = PixelColor.Hex(0x1A1830); var glow = PixelColor.Hex(0x6A431C);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float d = MathF.Sqrt((x - 9f) * (x - 9f) + (y - 10f) * (y - 10f));
                float i = MathF.Max(0, 1 - d / 9);
                c[x, y] = night.Mixed(glow, i * 1.2f > Bayer.Threshold(x, y) ? 0.9f : 0.1f);
            }
        }
        // Airu is the flame inside the lantern: the same flame as in the menu, flickering between its two frames.
        void Put(int x, int y, PixelColor color) { if (c.Contains(x, y)) c[x, y] = color; }
        MenuArt.DrawFlame(c, frame, (size - MenuArt.FlameWidth) / 2, 4);
        // Sparkles.
        var sparkle = PixelColor.Hex(0xFFFFD8);
        foreach (var (x, y) in (frame == 0 ? new[] { (3, 3), (15, 5), (2, 13) } : new[] { (14, 3), (3, 6), (15, 13) })) Put(x, y, sparkle);
        // Frame.
        for (int i = 0; i < size; i++)
        {
            var items = new (int, int, PixelColor)[]
            {
                (i, 0, Colors.Ink), (i, size - 1, Colors.Ink), (0, i, Colors.Ink), (size - 1, i, Colors.Ink),
                (i, 1, Colors.Ochre), (i, size - 2, Colors.Ochre), (1, i, Colors.Ochre), (size - 2, i, Colors.Ochre),
            };
            foreach (var (x, y, color) in items)
                if ((i > 0 && i < size - 1) || color == Colors.Ink) c[x, y] = color;
        }
        return c;
    }
}
