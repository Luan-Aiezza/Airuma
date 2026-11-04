using Godot;
using System;
using P = PixelColor;

/// <summary>
/// The game's icon: only the lantern of the menu (see <see cref="MenuArt.MakeLantern"/>), with its flame lit, in the middle of the dark, and its warm light on the night around it. 48 x 48 pixels.
/// </summary>
public static class IconArt
{
    public const int Size = 48;

    private static readonly P[] Night = { P.Hex(0x061116), P.Hex(0x0A1B21), P.Hex(0x10282D), P.Hex(0x183840) };   // dark ... paler towards the light
    private static readonly P[] Glow = { P.Hex(0x1C2C2D), P.Hex(0x2C3C36), P.Hex(0x474A34), P.Hex(0x70582C) };

    public static PixelCanvas Make()
    {
        var c = new PixelCanvas(Size, Size);
        int lx = (Size - MenuArt.LanternWidth) / 2, ly = (Size - MenuArt.LanternHeight) / 2;       // where the lantern's top-left corner is: in the middle
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

        // The lantern.
        var lantern = MenuArt.MakeLantern(0);
        for (int y = 0; y < lantern.Height; y++)
            for (int x = 0; x < lantern.Width; x++)
                if (lantern[x, y].A > 0) c[lx + x, ly + y] = lantern[x, y];

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
