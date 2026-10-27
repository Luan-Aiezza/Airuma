using Godot;
using System;
using P = PixelColor;

/// <summary>
/// Pixel art for the player, seen from above and behind (as Link is when he walks away from us in Minish Cap, on the same
/// fine pixel grid: a figure about 17 x 25 pixels, each shown <see cref="Scale"/> = 2 screen points): a young Andean boy
/// standing on a slim board, holding the staff of a sail that stands ahead of him, with his lantern hung from that staff.
/// He has black hair (no hat) and plain clothes: a simple terracotta wool shirt, cream wool trousers and leather sandals.
/// The boy and the lantern are the
/// focus; the board is a plain pale plank and the sail a muted cloth. Bow up (towards the boss).
/// </summary>
public static class PlayerArt
{
    public const float Scale = 2;
    public const int Width = 43;
    public const int Height = 40;

    /// <summary>Lantern core in art pixels (column, row from the top): it hangs from the sail's staff, ahead of the boy.</summary>
    public static readonly (int x, int y) LanternPixel = (10, 9);

    /// <summary>Offset of the lantern from the sprite's centre in Godot screen space (Y DOWN).</summary>
    public static Vector2 LanternOffset => new Vector2((LanternPixel.x + 0.5f - Width / 2f) * Scale, (LanternPixel.y + 0.5f - Height / 2f) * Scale);

    public static Vector2 SpriteSize => new Vector2(Width * Scale, Height * Scale);

    private const int Cx = 21; // the boy's and the board's centre column

    // Palette

    private static readonly P Outline = P.Hex(0x1C120E);
    private static readonly P[] Board = { P.Hex(0x8A6B42), P.Hex(0xB89460), P.Hex(0xD6B982) }; // dark, mid, light
    private static readonly P[] Skin = { P.Hex(0xE0A872), P.Hex(0xC08254), P.Hex(0x9A6038) };   // light, mid, shade
    private static readonly P[] Hair = { P.Hex(0x2A1D22), P.Hex(0x43313A), P.Hex(0x7C6270) };   // blue-black, soft, glint
    private static readonly P[] Red = { P.Hex(0xE2493C), P.Hex(0xC23030), P.Hex(0x8E2024) };    // light, mid, shade
    private static readonly P[] Shirt = { P.Hex(0xD9694B), P.Hex(0xB84A36), P.Hex(0x8A3428) }; // plain terracotta wool
    private static readonly P[] Wool = { P.Hex(0xE8DCC0), P.Hex(0xC9BA98), P.Hex(0xA3956F) };   // trousers: lit, mid, shade
    private static readonly P Leather = P.Hex(0x6B4426), LeatherDark = P.Hex(0x45291A);
    private static readonly P[] Sail = { P.Hex(0xE2D8BC), P.Hex(0xB9AD8C), P.Hex(0x8C8264) };   // lit, mid, seam
    private static readonly P[] Wood = { P.Hex(0xB28A52), P.Hex(0x7A5A34) };
    private static readonly P Iron = P.Hex(0x2A1E14);
    private static readonly P Brass = P.Hex(0xD9A441);

    // Drawing

    public static Texture2D[] MakeFrames() => new Texture2D[] { MakeCanvas(0).ToTexture(), MakeCanvas(1).ToTexture() };

    public static PixelCanvas MakeCanvas(int frame)
    {
        var c = new PixelCanvas(Width, Height);
        void Put(int x, int y, P color) { if (c.Contains(x, y)) c[x, y] = color; }
        void Span(int y, int x0, int x1, Func<int, P> color) { if (x0 <= x1) for (int x = x0; x <= x1; x++) Put(x, y, color(x)); }
        // Mirror-symmetric painting around column Cx; the right-hand side is a little darker (light comes from the lantern).
        void Sym(int d, int y, P color)
        {
            Put(Cx - d, y, color);
            Put(Cx + d, y, d == 0 ? color : color.Scaled(0.88f));
        }

        // The board: a slim pointed plank, pale and plain, with a rim and a seam down the middle, from bow to stern.
        float[] half = { 0, 0.9f, 1.7f, 2.3f, 2.7f, 3.0f, 3.1f, 3.1f, 3.1f, 3.1f, 3.1f, 3.1f, 3.1f, 3.1f, 3.1f, 3.1f, 3.0f, 2.7f, 2.1f, 1.2f };
        for (int y = 2; y <= 39; y++)
        {
            float h = half[Math.Min(19, y / 2)] * 2f;
            int x0 = (int)MathF.Round(Cx - h), x1 = (int)MathF.Round(Cx + h);
            Span(y, x0, x1, x =>
            {
                if (x == x0 || x == x1) return Board[0];
                if (x == Cx) return Board[1].Mixed(Board[0], 0.35f);
                return x < Cx ? Board[2] : Board[1];
            });
        }
        for (int y = 8; y <= 36; y += 8) Span(y, Cx - 5, Cx + 5, x => x < Cx ? Board[1] : Board[0].Mixed(Board[1], 0.4f)); // cross planks

        // The staff of the sail stands at the bow, near the left rim of the board, with a brass cap.
        for (int y = 2; y <= 15; y++) { Put(16, y, y % 6 == 0 ? Wood[1] : Wood[0]); Put(17, y, Wood[1]); }
        Put(16, 0, Brass); Put(17, 0, Brass); Put(16, 1, Brass); Put(17, 1, Brass.Scaled(0.8f));

        // The sail is ahead of the boy: cloth billowing right of the staff, brighter on the lantern's side, with a seam, a
        // woven red trim and a boom under it. It flutters.
        int[] widths = { 6, 10, 12, 10, 6 };
        for (int i = 0; i < widths.Length; i++)
        {
            int w = widths[i];
            int flutter = frame == 1 && (i == 1 || i == 2) ? 1 : 0;
            for (int r = 0; r < 2; r++)
            {
                int y = 2 + i * 2 + r;
                for (int x = 18; x <= 17 + w + flutter; x++)
                {
                    float t = (float)(x - 17) / (w + 1);
                    Put(x, y, t < 0.34f ? Sail[0] : (t < 0.78f ? Sail[1] : Sail[2]));
                }
                if (w >= 10) Put(17 + w / 2 + 1, y, Sail[1].Mixed(Sail[2], 0.6f));
            }
        }
        for (int x = 19; x <= 28; x += 2) Put(x, 10, Red[1]);                    // a woven trim of red dashes
        Span(12, 18, 29, x => x == 18 ? Wood[0] : Wood[1]);
        Span(13, 18, 29, x => x % 2 == 0 ? Red[2] : Wood[1]);                     // the boom, wrapped in red wool

        // The lantern hangs from the staff by a hook, left of it: the brightest thing in the picture.
        Put(15, 5, Iron); Put(14, 5, Iron); Put(13, 5, Iron);
        var flame = frame == 0 ? P.Hex(0xFFE27A) : P.Hex(0xFFF6C0);
        for (int x = 9; x <= 12; x++) { Put(x, 6, Brass); Put(x, 13, Brass); }                    // caps
        for (int y = 7; y <= 12; y++) { Put(8, y, Iron); Put(13, y, Iron); }                        // frame
        for (int y = 7; y <= 12; y++)
            for (int x = 9; x <= 12; x++)
            {
                bool core = x >= 10 && x <= 11 && y >= 8 && y <= 11;
                Put(x, y, core ? (y <= 9 ? flame.Mixed(P.Hex(0xFFFFFF), 0.85f) : flame) : (y >= 11 ? P.Hex(0xFFB23D) : P.Hex(0xFFD066)));
            }
        Put(8, 6, Iron); Put(13, 6, Iron); Put(8, 13, Iron); Put(13, 13, Iron);
        Put(10, 14, Iron); Put(11, 14, Iron);                                                      // base knob

        DrawBoy(Sym);

        // A dark outline round everything.
        var outC = c.Clone();
        var nb = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                if (c[x, y].A != 0) continue;
                foreach (var (dx, dy) in nb)
                    if (c.Contains(x + dx, y + dy) && c[x + dx, y + dy].A > 0) { outC[x, y] = Outline; break; }
            }
        return outC;
    }

    private static void DrawBoy(Action<int, int, P> Sym)
    {
        // Seen from above, the head hides most of him: only the shoulders, the hands and the feet show round it.
        // Feet: simple leather sandals, with a little of the trousers above them.
        for (int y = 36; y <= 37; y++)
            for (int d = 1; d <= 3; d++) Sym(d, y, y == 37 ? LeatherDark : (d == 3 ? Skin[1] : Skin[0]));
        Sym(1, 36, Leather); Sym(2, 36, Leather);
        for (int y = 33; y <= 35; y++)
            for (int d = 1; d <= 3; d++) Sym(d, y, d == 1 ? Wool[0] : (d == 2 ? Wool[1] : Wool[2]));

        // A plain terracotta shirt: just the shoulders below the head, short sleeves and hands at the sides.
        int[] shirtHalf = { 5, 6, 6, 6, 5, 5 }; // rows 28..33
        for (int i = 0; i < shirtHalf.Length; i++)
        {
            int y = 28 + i, hw = shirtHalf[i];
            for (int d = 0; d <= hw; d++)
                Sym(d, y, d >= hw ? Shirt[2] : (d <= 1 ? Shirt[0] : Shirt[1]));
        }
        for (int y = 30; y <= 31; y++) { Sym(7, y, Shirt[1]); }
        for (int y = 32; y <= 33; y++) { Sym(7, y, Skin[y == 33 ? 1 : 0]); Sym(8, y, Skin[1]); }

        // The head from behind and above, big: a rounded mass of black hair with soft strands, a little cowlick on top
        // and the tips of the ears showing below the hair.
        for (int y = 11; y <= 29; y++)
            for (int d = 0; d <= 10; d++)
            {
                float ex = d / 9.6f, ey = (y + 0.5f - 20.2f) / 9.2f;
                if (y > 25) ex = d / (9.6f - (y - 25) * 1.5f);
                if (ex * ex + ey * ey >= 1) continue;
                float rim = ex * ex + ey * ey;
                P col = ((d + y / 3) % 3 == 0) ? Hair[1] : Hair[0];
                if (rim > 0.8f) col = Hair[0];
                else if (y <= 17 && d <= 6 && (d * 2 + y) % 5 == 0) col = Hair[2];
                else if (y >= 19 && d == 3 + (y % 2) && y <= 26) col = Hair[1];
                Sym(d, y, col);
            }
        Sym(0, 10, Hair[1]); Sym(1, 10, Hair[0]); Sym(0, 9, Hair[0]);        // cowlick
        // sheen over the crown
        Sym(1, 12, Hair[2]); Sym(2, 13, Hair[2]); Sym(3, 13, Hair[2]); Sym(4, 14, Hair[2]); Sym(5, 15, Hair[2]); Sym(6, 16, Hair[2]); Sym(7, 18, Hair[2]);
        Sym(2, 17, Hair[2]); Sym(3, 18, Hair[2]);
        // ears peeking out under the hair
        Sym(10, 22, Skin[0]); Sym(10, 23, Skin[1]); Sym(9, 23, Skin[2]);
    }

    // Lantern glow

    public const int AuraSize = 58;

    /// <summary>Warm radial light, dithered into a few opacity steps so it stays pixel art. Meant to be drawn additively.</summary>
    public static PixelCanvas MakeAuraCanvas(float radius) =>
        GlowArt.MakeCanvas(AuraSize, radius, P.Hex(0xFFE28C), P.Hex(0xFF9632), new GlowArt.Tiers(90, 56, 28));

    public static Texture2D[] MakeAuraFrames() =>
        new Texture2D[] { MakeAuraCanvas(28).ToTexture(), MakeAuraCanvas(25).ToTexture() };
}
