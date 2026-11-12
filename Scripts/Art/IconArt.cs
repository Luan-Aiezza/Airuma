using Godot;
using System;
using P = PixelColor;

/// <summary>
/// The game's icon: a bright blue star of four points in the night, with a white glow that comes out of its centre, in pixel art (after a photograph of a blue star with long rays). 32 x 32 pixels; the exported PNG is 960 x 960.
/// `Make()` has the night behind it; `Make(false)` is the star alone, on a clear background (for the places where the icon is not set in a box of its own).
/// </summary>
public static class IconArt
{
    public const int Size = 32;

    private static readonly P Deep = P.Hex(0x0F4FC8), Blue = P.Hex(0x2E86F0), Sky = P.Hex(0x66CCFF), Pale = P.Hex(0xBFEFFF), White = P.Hex(0xFFFFFF);
    private static readonly P[] Night = { P.Hex(0x030A16), P.Hex(0x061424), P.Hex(0x0A2338), P.Hex(0x0F3550), P.Hex(0x174C70), P.Hex(0x1F6590) };

    /// <summary>
    /// The star: four long rays (up, down, left, right) that thin out to a point, as thick as `thickness` pixels at the middle. Returns how bright the ray is at this pixel (0: outside it, 1: the centre
    /// of its axis near the middle): the brightness falls off along the ray and towards its edges.
    /// </summary>
    private static float Ray(float along, float across, float length, float thickness)
    {
        along = MathF.Abs(along); across = MathF.Abs(across);
        if (along > length) return 0;
        float v = 1 - along / length;
        float half = thickness * MathF.Pow(v, 1.7f);
        if (across > half) return 0;
        float u = half > 0.01f ? across / half : 0;
        return Math.Clamp(0.5f * (1 - u) + 0.55f * MathF.Pow(v, 1.4f), 0.01f, 1f);
    }

    public static PixelCanvas Make(bool night = true)
    {
        var c = new PixelCanvas(Size, Size);
        const float mid = Size / 2f;
        for (int y = 0; y < Size; y++)
            for (int x = 0; x < Size; x++)
            {
                float dx = x + 0.5f - mid, dy = y + 0.5f - mid, d = MathF.Sqrt(dx * dx + dy * dy);
                float bayer = (Bayer.Threshold(x, y) - 0.5f) * 0.7f;
                P colour = default;      // clear
                if (night)
                {
                    // the night: darker at the top, and a haze of blue round the star that thins out (dithered)
                    float t = y / (float)(Size - 1);
                    float haze = Math.Clamp(1 - d / 18f, 0, 1);
                    int level = Math.Clamp((int)MathF.Floor(t * 1.6f + haze * 3.8f + bayer), 0, Night.Length - 1);
                    colour = Night[level];
                }
                else if (d < 15f)
                {
                    // alone: only a dithered halo, clear towards the edges
                    float haze = 1 - d / 15f;
                    if (haze * 3f + bayer > 0.9f) colour = new P(46, 134, 240, (byte)Math.Clamp((int)(haze * 160), 50, 150));
                }

                // the glow that comes out of the centre, round: white in the middle, then pale, then sky (dithered at its edge)
                if (d < 1.7f) colour = White;
                else if (d < 2.9f) colour = Pale;
                else if (d < 4.6f && (1 - d / 4.6f) * 3.2f + bayer > 0.9f) colour = Sky;
                // the four rays
                float b = MathF.Max(Ray(dy, dx, 15.8f, 4.6f), Ray(dx, dy, 15.8f, 4.6f));
                if (b > 0)
                {
                    P ray = b > 0.95f ? White : (b > 0.80f ? Pale : (b > 0.54f ? Sky : (b > 0.30f ? Blue : Deep)));
                    if (colour.A == 0 || Brightness(ray) >= Brightness(colour) || d > 4.6f) colour = ray;
                }
                c[x, y] = colour;
            }
        return c;
    }

    /// <summary>The same star, small (11 x 11 pixels), clear round it: it marks the line chosen in the menus, and pulses there.</summary>
    public static PixelCanvas MakeMarker()
    {
        const int size = 11;
        var c = new PixelCanvas(size, size);
        const float mid = size / 2f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - mid, dy = y + 0.5f - mid, d = MathF.Sqrt(dx * dx + dy * dy);
                float b = MathF.Max(Ray(dy, dx, 5.8f, 3.4f), Ray(dx, dy, 5.8f, 3.4f));
                P colour = default;
                if (b > 0) colour = b > 0.95f ? White : (b > 0.80f ? Pale : (b > 0.54f ? Sky : (b > 0.30f ? Blue : Deep)));
                if (d < 1.1f) colour = White; else if (d < 1.9f && Brightness(colour) < Brightness(Pale)) colour = Pale;
                c[x, y] = colour;
            }
        return c;
    }

    private static int Brightness(P p) => p.R + p.G + p.B;

    /// <summary>The icon as a PNG at the given scale (nearest-neighbour, so the pixels stay sharp).</summary>
    public static Image MakeImage(int scale = 30)
    {
        var image = Make().ToImage();
        image.Resize(Size * scale, Size * scale, Image.Interpolation.Nearest);
        return image;
    }
}
