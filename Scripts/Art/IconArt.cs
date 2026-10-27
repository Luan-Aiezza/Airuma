using Godot;
using System;
using P = PixelColor;

/// <summary>
/// The game's icon: the lantern of the menu (see <see cref="MenuArt.MakeLantern"/>) hanging by its chain in the dark of the forest,
/// with its warm light on the night around it and a little green and a few fireflies in the corners, in the colours of the pillars.
/// 48 x 48 pixels.
/// </summary>
public static class IconArt
{
    public const int Size = 48;

    private static readonly P[] Night = { P.Hex(0x061116), P.Hex(0x0A1B21), P.Hex(0x10282D), P.Hex(0x183840) };   // dark ... paler towards the light
    private static readonly P[] Glow = { P.Hex(0x1C2C2D), P.Hex(0x2C3C36), P.Hex(0x474A34), P.Hex(0x70582C) };

    public static PixelCanvas Make()
    {
        var c = new PixelCanvas(Size, Size);
        int lx = (Size - MenuArt.LanternWidth) / 2, ly = 10;                    // where the lantern's top-left corner is
        float fx = lx + 10.5f, fy = ly + 21f;                                   // the middle of its flame

        // The night, a little paler lower down, and the lantern's light on it: rings of warm colour, dithered.
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float t = y / (float)Size;
                int band = Math.Min(3, (int)(t * 2.2f + (Bayer.Threshold(x, y) - 0.5f) * 0.9f + 0.4f));
                P col = Night[Math.Max(0, band)];
                float d = MathF.Sqrt((x + 0.5f - fx) * (x + 0.5f - fx) + (y + 0.5f - fy) * (y + 0.5f - fy) * 1.15f);
                float light = 1 - d / 24f;
                if (light > 0)
                {
                    int g = Math.Min(3, (int)(light * 4.2f + (Bayer.Threshold(x, y) - 0.5f) * 1.1f));
                    if (g >= 0 && light * 4.2f > 0.9f) col = Glow[g];
                }
                c[x, y] = col;
            }

        // The chain, from the top of the picture to the ring of the lantern.
        for (int y = 0; y < ly; y++)
        {
            int x = lx + 10;
            c[x, y] = y % 3 == 2 ? SidePillarArt.Stone[3] : SidePillarArt.Stone[1];
            c[x + 1, y] = y % 3 == 1 ? SidePillarArt.Stone[2] : SidePillarArt.Out;
            if (y % 3 != 1) c[x - 1, y] = SidePillarArt.Out;
        }

        // The lantern.
        var lantern = MenuArt.MakeLantern(0);
        for (int y = 0; y < lantern.Height; y++)
            for (int x = 0; x < lantern.Width; x++)
                if (lantern[x, y].A > 0) c[lx + x, ly + y] = lantern[x, y];

        // Green creeping up from the corners at the bottom, with a flower or two, as on the pillars.
        var leaf = SidePillarArt.Leaf; var flower = SidePillarArt.Flower;
        foreach (int side in new[] { -1, 1 })
        {
            int x0 = side < 0 ? 0 : Size - 1;
            for (int k = 0; k < 15; k++)
            {
                int x = x0 - side * (k / 3 + (k % 2)), y = Size - 1 - k;
                if (c.Contains(x, y)) c[x, y] = leaf[1 + k % 3];
                int xl = x0 - side * (3 + k / 2), yl = Size - 2 - k;
                if (k % 3 == 1 && c.Contains(xl, yl)) { c[xl, yl] = leaf[3]; if (c.Contains(xl - side, yl)) c[xl - side, yl] = leaf[2]; }
            }
            int fxp = x0 - side * 4, fyp = Size - 10;
            foreach (var (ox, oy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }) c[fxp + ox, fyp + oy] = flower[1];
            c[fxp, fyp] = flower[3];
        }

        // A few fireflies.
        foreach (var (x, y) in new[] { (6, 14), (41, 18), (9, 33), (38, 38), (31, 6) })
        {
            c[x, y] = P.Hex(0xE8F2A0);
            c[x + 1, y] = P.Hex(0x7FA860, 150); c[x - 1, y] = P.Hex(0x7FA860, 150);
        }
        return c;
    }

    /// <summary>The icon as a PNG at the given scale (nearest-neighbour, so the pixels stay sharp).</summary>
    public static Image MakeImage(int scale = 20)
    {
        var image = Make().ToImage();
        image.Resize(Size * scale, Size * scale, Image.Interpolation.Nearest);
        return image;
    }
}
