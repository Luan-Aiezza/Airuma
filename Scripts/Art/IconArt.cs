using Godot;
using System;
using static PixelPaint;
using P = PixelColor;

/// <summary>
/// The game's icon: a lantern in pixel art (64 x 64), inspired by the chunky indigo lantern of "Candies n Curses": a big ring on
/// top, a heavy roof, two posts and a base, and inside the glass a round golden light with a little face of two dark bars.
/// </summary>
public static class IconArt
{
    public const int Size = 64;

    private static readonly P Ink = P.Hex(0x15102B);
    private static readonly P[] Metal = { P.Hex(0x8C74D8), P.Hex(0x6A54B4), P.Hex(0x4B3B8A), P.Hex(0x33286A) };   // light .. dark
    private static readonly P Hollow = P.Hex(0x1A1432), HollowLight = P.Hex(0x2A2150);
    private static readonly P[] Gold = { P.Hex(0xFFF8C4), P.Hex(0xFFE45E), P.Hex(0xFBC02D), P.Hex(0xE08A1E), P.Hex(0xB8601A) };

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    public static PixelCanvas Make()
    {
        var art = new PixelCanvas(Size, Size);
        var body = new PixelCanvas(Size, Size);

        // the hollow of the lantern, with a warm glow on its floor and sides
        Rect(body, 17, 33, 46, 53, Hollow);
        Rect(body, 17, 33, 46, 36, P.Hex(0x120D26));
        for (int y = 36; y <= 52; y++)
            for (int x = 17; x <= 46; x++)
            {
                float d = Math.Abs(x - 31.5f) / 15f * 0.7f + Math.Abs(y - 46) / 10f * 0.5f;
                if (d < 0.8f && (x + y) % 2 == 0) Plot(body, x, y, P.Hex(0x4A3A2A));
            }

        // the posts and their glowing slits
        foreach (int x0 in new[] { 11, 47 })
        {
            Rect(body, x0, 34, x0 + 5, 54, Metal[1]);
            Rect(body, x0, 34, x0 + 1, 54, Metal[0]);
            Rect(body, x0 + 4, 34, x0 + 5, 54, Metal[2]);
            Rect(body, x0 + 2, 40, x0 + 3, 48, Gold[1]);
            Rect(body, x0 + 2, 42, x0 + 3, 46, Gold[0]);
        }

        // the golden light: a round body with two small bumps on top and a face
        Ellipse(body, 32, 46.5f, 13.2f, 11.4f, Gold[4]);
        Ellipse(body, 31.6f, 46f, 12.6f, 10.8f, Gold[3]);
        Ellipse(body, 31f, 45.2f, 11.6f, 10f, Gold[2]);
        Ellipse(body, 30.2f, 44.2f, 10f, 8.6f, Gold[1]);
        Ellipse(body, 25f, 37.2f, 3.2f, 3f, Gold[2]); Ellipse(body, 24.6f, 36.8f, 2.5f, 2.3f, Gold[1]);
        Ellipse(body, 40f, 37.8f, 3.2f, 3f, Gold[3]); Ellipse(body, 39.6f, 37.4f, 2.5f, 2.3f, Gold[2]);
        Ellipse(body, 30.6f, 47f, 6.6f, 5.6f, Gold[0]);              // the white-hot face plate
        Rect(body, 28, 45, 29, 49, Hollow); Rect(body, 33, 45, 34, 49, Hollow);   // the eyes
        Rect(body, 28, 45, 28, 45, HollowLight);
        // sparks of light round it
        foreach (var (x, y) in new[] { (17, 55), (47, 36), (20, 40), (44, 52) }) Plot(body, x, y, Gold[1]);

        // the base: a heavy plate with a rim of light along its top
        Poly(body, new[] { V(9, 54), V(54, 54), V(56, 60), V(7, 60) }, Metal[1]);
        Rect(body, 9, 54, 54, 55, Metal[0]);
        Rect(body, 7, 59, 56, 60, Metal[3]);
        Rect(body, 11, 56, 52, 57, Metal[2]);
        Rect(body, 14, 54, 49, 54, Gold[2].WithAlpha(210));
        for (int x = 16; x <= 48; x += 8) Plot(body, x, 56, Metal[0]);
        Rect(body, 14, 60, 19, 61, Metal[3]); Rect(body, 44, 60, 49, 61, Metal[3]);   // little feet

        // the roof: two bands, the upper one narrower, with rivets
        Poly(body, new[] { V(15, 22), V(48, 22), V(50, 27), V(13, 27) }, Metal[1]);
        Rect(body, 15, 22, 47, 23, Metal[0]);
        Poly(body, new[] { V(6, 27), V(57, 27), V(59, 35), V(4, 35) }, Metal[1]);
        Rect(body, 6, 27, 57, 28, Metal[0]);
        Rect(body, 5, 33, 58, 35, Metal[2]);
        Rect(body, 9, 31, 54, 31, Metal[2]);
        foreach (var (x, y) in new[] { (9, 30), (54, 30), (12, 25), (51, 25) }) { Rect(body, x, y, x + 1, y + 1, Metal[3]); Plot(body, x, y, Metal[0]); }
        for (int x = 18; x <= 40; x += 11) Rect(body, x, 24, x + 4, 25, Metal[2]);        // a worn-looking panel
        Rect(body, 14, 29, 22, 30, P.Hex(0xA48CE8));                                     // a shine on the roof

        // the ring on top, and the clasp that holds it
        Rect(body, 28, 19, 35, 23, Metal[2]); Rect(body, 29, 19, 30, 23, Metal[1]);
        Rect(body, 29, 17, 34, 18, Metal[3]);
        for (int y = 0; y < 22; y++)
            for (int x = 14; x < 50; x++)
            {
                float dx = (x + 0.5f - 32) / 11.5f, dy = (y + 0.5f - 10) / 9.5f;
                float d = dx * dx + dy * dy;
                float idx = (x + 0.5f - 32) / 6f, idy = (y + 0.5f - 10.5f) / 4.7f;
                float inner = idx * idx + idy * idy;
                if (d > 1 || inner < 1) continue;
                P col = d > 0.78f ? Metal[3] : ((x + 0.5f < 32 && y + 0.5f < 10) ? Metal[0] : (y + 0.5f > 11 ? Metal[2] : Metal[1]));
                Plot(body, x, y, col);
            }

        Outline(body, Ink);
        Composite(art, body);
        return art;
    }

    /// <summary>The icon as a PNG at the given scale (nearest-neighbour, so the pixels stay sharp).</summary>
    public static Image MakeImage(int scale = 16)
    {
        var image = Make().ToImage();
        image.Resize(Size * scale, Size * scale, Image.Interpolation.Nearest);
        return image;
    }
}
