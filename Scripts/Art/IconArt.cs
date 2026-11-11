using Godot;
using System;
using P = PixelColor;

/// <summary>
/// The game's icon, as simple as can be: the inside of the lantern, very close, and its flame filling it. Four bars of the iron frame at the edges, the glass dark amber, the flame in the middle (orange, yellow and a white heart),
/// and the warm light of the flame on the glass around it. 32 x 32 pixels (the exported PNG is 960 x 960).
/// </summary>
public static class IconArt
{
    public const int Size = 32;

    private static readonly P Iron = P.Hex(0x101818), Slate = P.Hex(0x405058), Steel = P.Hex(0x7E9296);
    private static readonly P[] Glass = { P.Hex(0x2C140A), P.Hex(0x4A2410), P.Hex(0x6E3814), P.Hex(0x92501C) };   // dark ... warm, nearer the flame

    public static PixelCanvas Make()
    {
        var c = new PixelCanvas(Size, Size);
        const float fx = 16f, fy = 18f;                                  // the middle of the flame

        // The glass: dark amber, warmer in flat rings towards the flame.
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float d = MathF.Sqrt((x + 0.5f - fx) * (x + 0.5f - fx) + (y + 0.5f - fy) * (y + 0.5f - fy) * 0.8f);
                float warm = 1 - d / 22f;
                int g = Math.Clamp((int)(warm * 3.4f), 0, 3);
                c[x, y] = Glass[g];
            }

        // The frame of the lantern: a bar down each side, a bar at the top and at the foot, with a dark line outside each and a pale one inside.
        for (int y = 0; y < Size; y++)
        {
            c[0, y] = Iron; c[Size - 1, y] = Iron;
            c[1, y] = Slate; c[Size - 2, y] = Slate; c[2, y] = Steel; c[Size - 3, y] = Slate;
        }
        for (int x = 0; x < Size; x++)
        {
            c[x, 0] = Iron; c[x, Size - 1] = Iron;
            c[x, 1] = Slate; c[x, Size - 2] = Slate; c[x, 2] = x > 2 && x < Size - 3 ? Steel : c[x, 2];
        }

        // The flame: a drop, wide at the belly and pointed at the tip, in three layers.
        Flame(c, fx, 4f, 28f, 10.5f, 0.25f, FirePalette.Ramp[2]);        // orange
        Flame(c, fx, 9f, 27f, 7.4f, 0.15f, FirePalette.Ramp[1]);       // yellow
        Flame(c, fx, 15f, 26f, 4.2f, 0.05f, FirePalette.Ramp[0]);        // the white-hot heart
        return c;
    }

    /// <summary>One layer of the flame: from `top` to `bottom` in rows, as wide as `half` pixels from the middle at the fullest point; the tip leans `lean` pixels to one side.</summary>
    private static void Flame(PixelCanvas c, float cx, float top, float bottom, float half, float lean, P colour)
    {
        for (int y = (int)top; y <= (int)bottom; y++)
        {
            float t = (y + 0.5f - top) / (bottom - top);
            if (t < 0 || t > 1) continue;
            float w = half * MathF.Sin(MathF.PI * MathF.Pow(t, 1.5f));
            float mid = cx + lean * (1 - t) * 6f;
            for (int x = (int)MathF.Floor(mid - w); x <= (int)MathF.Ceiling(mid + w); x++)
                if (c.Contains(x, y) && MathF.Abs(x + 0.5f - mid) <= w) c[x, y] = colour;
        }
    }

    /// <summary>The icon as a PNG at the given scale (nearest-neighbour, so the pixels stay sharp).</summary>
    public static Image MakeImage(int scale = 30)
    {
        var image = Make().ToImage();
        image.Resize(Size * scale, Size * scale, Image.Interpolation.Nearest);
        return image;
    }
}
