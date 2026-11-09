using System;
using P = PixelColor;

/// <summary>
/// The moon of the gravity attack of the last boss (see <see cref="GravitySpec"/>), in pixel art like the other moons of the game: a pale disc of four flat tones (lit from the upper left, as the moon of the night
/// scene is), craters one tone darker, and a rim. Two sizes: the usual (15 x 15 art pixels, 60 px on the screen) and the swollen one that it takes when it is about to pull (17 x 17).
/// </summary>
public static class GravityMoonArt
{
    public const float Scale = 4;
    public static readonly int[] Sizes = { 15, 17 };

    private static readonly P[] Tone = { P.Hex(0xFFFFFF), P.Hex(0xE8EEFF), P.Hex(0xB4C2E4), P.Hex(0x8898C4) };
    private static readonly P Rim = P.Hex(0x5A6AA0);

    public static PixelCanvas Make(int variant)
    {
        int n = Sizes[variant];
        var c = new PixelCanvas(n, n);
        float r = n / 2f - 0.3f, mid = n / 2f;
        // craters, as fractions of the radius: (x, y, radius)
        (float x, float y, float r)[] craters = { (-0.4f, -0.3f, 0.26f), (0.35f, -0.5f, 0.2f), (0.45f, 0.2f, 0.28f), (-0.5f, 0.35f, 0.2f), (0.0f, 0.55f, 0.15f) };
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                float dx = x + 0.5f - mid, dy = y + 0.5f - mid, d = MathF.Sqrt(dx * dx + dy * dy);
                if (d > r) continue;
                float lit = 1 - (dx * 0.5f + dy * 0.6f) / (2 * r) - 0.1f;
                int t = lit > 0.86f ? 0 : (lit > 0.6f ? 1 : (lit > 0.36f ? 2 : 3));
                foreach (var cr in craters)
                    if (MathF.Sqrt((dx / r - cr.x) * (dx / r - cr.x) + (dy / r - cr.y) * (dy / r - cr.y)) < cr.r) t = Math.Min(3, t + 1);
                c[x, y] = Tone[t];
            }
        // the rim: the outermost pixels, in a dark blue, with the upper left edge left lit
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                if (c[x, y].A == 0) continue;
                bool edge = false;
                foreach (var (ox, oy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    if (!c.Contains(x + ox, y + oy) || c[x + ox, y + oy].A == 0) edge = true;
                if (edge) c[x, y] = (x + y < n - 1) ? Tone[2] : Rim;
            }
        return c;
    }
}
