using Godot;
using System;
using System.Linq;

/// <summary>
/// The little lights that spirits drop: warm specks in the colour of the boat's lantern, with a soft halo.
/// Two frames, so they twinkle. Drawn additively.
/// </summary>
public static class LightArt
{
    public const float Scale = 4;
    public const int Size = 5;
    public const int FrameCount = 2;

    public static PixelCanvas MakeCanvas(int frame)
    {
        var c = new PixelCanvas(Size, Size);
        var core = PixelColor.Hex(0xFFF6C0);
        var mid = PixelColor.Hex(0xFFD66B);
        var halo = PixelColor.Hex(0xFF9A1F, (byte)(frame == 0 ? 110 : 60));
        c[2, 2] = core;
        foreach (var (x, y) in new[] { (1, 2), (3, 2), (2, 1), (2, 3) }) c[x, y] = mid;
        foreach (var (x, y) in new[] { (1, 1), (3, 1), (1, 3), (3, 3) }) c[x, y] = halo;
        // A twinkle: the second frame stretches into a little cross.
        if (frame == 1) foreach (var (x, y) in new[] { (2, 0), (2, 4), (0, 2), (4, 2) }) c[x, y] = mid;
        return c;
    }

    // MARK: The power items: red squares with a "P"

    /// <summary>The square of a power item is always 9 x 9 art pixels; the three sizes are the three scales it is shown at: 2 (18 px, the small one, type 1), 3 (27 px, the medium) and 4 (36 px, the large, type 3).</summary>
    public const int PowerSide = 9;
    public static readonly float[] PowerScales = { 2, 3, 4 };
    public static float PowerPixels(int size) => PowerSide * PowerScales[size];

    private static readonly string[] SmallP = { "###", "#.#", "###", "#..", "#.." };

    /// <summary>
    /// A power item: a plain red square, as unlike the bullets of the game as can be, with a dark rim, a lit edge at the top and the left, a shade at the bottom and the right, and a white "P" (for power) with a
    /// dark shadow. It is the same picture at every size (`size` is only kept for the callers); the second frame passes a glint across the corner.
    /// </summary>
    public static PixelCanvas MakePowerCanvas(int size, int frame)
    {
        int n = PowerSide;
        var c = new PixelCanvas(n, n);
        var rim = PixelColor.Hex(0x4A0610); var lit = PixelColor.Hex(0xFF7A6A); var fill = PixelColor.Hex(0xE02424); var shade = PixelColor.Hex(0x9A0E18);
        for (int y = 0; y < n; y++)
            for (int x = 0; x < n; x++)
            {
                bool border = x == 0 || y == 0 || x == n - 1 || y == n - 1;
                if (border) { c[x, y] = rim; continue; }
                c[x, y] = (x == 1 || y == 1) ? lit : ((x == n - 2 || y == n - 2) ? shade : fill);
            }
        // the corners are cut by one pixel, so that the square is a little button
        foreach (var (x, y) in new[] { (0, 0), (n - 1, 0), (0, n - 1), (n - 1, n - 1) }) c[x, y] = new PixelColor(0, 0, 0, 0);
        var glyph = SmallP;
        int k = 1;
        int gw = glyph[0].Length * k, gh = glyph.Length * k;
        int ox = (n - gw) / 2, oy = (n - gh) / 2;
        var white = PixelColor.Hex(0xFFFFFF); var ink = PixelColor.Hex(0x6A0A14);
        for (int pass = 0; pass < 2; pass++)
            for (int gy = 0; gy < glyph.Length; gy++)
                for (int gx = 0; gx < glyph[gy].Length; gx++)
                {
                    if (glyph[gy][gx] != '#') continue;
                    for (int j = 0; j < k; j++)
                        for (int i = 0; i < k; i++)
                        {
                            int px = ox + gx * k + i + (pass == 0 ? 1 : 0), py = oy + gy * k + j + (pass == 0 ? 1 : 0);
                            if (px > 0 && py > 0 && px < n - 1 && py < n - 1) c[px, py] = pass == 0 ? ink : white;
                        }
                }
        // the glint: a bright pixel (or two) that moves along the top edge in the second frame
        var shine = PixelColor.Hex(0xFFE6DC);
        int g1 = frame == 0 ? 2 : n - 3;
        c[Math.Clamp(g1, 1, n - 2), 1] = shine;
        return c;
    }

    public static Texture2D[] MakePowerFrames(int size) =>
        Enumerable.Range(0, FrameCount).Select(f => (Texture2D)MakePowerCanvas(size, f).ToTexture()).ToArray();

    /// <summary>The spare lantern that now and then drops: the lantern of the HUD in a pale blue-white glow that pulses (13 x 13 pixels).</summary>
    public const int BombSize = 13;

    public static PixelCanvas MakeBombCanvas(int frame)
    {
        var c = new PixelCanvas(BombSize, BombSize);
        float radius = frame == 0 ? 6.4f : 5.6f;
        for (int y = 0; y < BombSize; y++)
            for (int x = 0; x < BombSize; x++)
            {
                float d = MathF.Sqrt((x - 6f) * (x - 6f) + (y - 6f) * (y - 6f));
                if (d > radius) continue;
                float t = 1 - d / radius;
                // soft rings of a pale glow
                byte alpha = (byte)(t > 0.66f ? 120 : (t > 0.33f ? 80 : 40));
                if ((x + y) % 2 == 0 && t < 0.33f) alpha = 0;
                if (alpha > 0) c[x, y] = PixelColor.Hex(0xBFE8FF, alpha);
            }
        var icon = HudArt.LanternIcon(true);
        for (int y = 0; y < icon.Height; y++)
            for (int x = 0; x < icon.Width; x++)
                if (icon[x, y].A > 0) c[x + 3, y + 3] = icon[x, y];
        return c;
    }

    /// <summary>The spare boat that is given for a high score: the boat of the HUD in a warm red-gold glow that pulses (13 x 13 pixels).</summary>
    public static PixelCanvas MakeLifeCanvas(int frame)
    {
        var c = new PixelCanvas(BombSize, BombSize);
        float radius = frame == 0 ? 6.4f : 5.6f;
        for (int y = 0; y < BombSize; y++)
            for (int x = 0; x < BombSize; x++)
            {
                float d = MathF.Sqrt((x - 6f) * (x - 6f) + (y - 6f) * (y - 6f));
                if (d > radius) continue;
                float t = 1 - d / radius;
                byte alpha = (byte)(t > 0.66f ? 130 : (t > 0.33f ? 90 : 45));
                if ((x + y) % 2 == 0 && t < 0.33f) alpha = 0;
                if (alpha > 0) c[x, y] = PixelColor.Hex(0xFF8A5A, alpha);
            }
        var icon = HudArt.HeartIcon(true);
        int ox = (BombSize - icon.Width) / 2, oy = (BombSize - icon.Height) / 2;
        for (int y = 0; y < icon.Height; y++)
            for (int x = 0; x < icon.Width; x++)
                if (icon[x, y].A > 0) c[x + ox, y + oy] = icon[x, y];
        return c;
    }

    public static Texture2D[] MakeLifeFrames() =>
        Enumerable.Range(0, FrameCount).Select(f => (Texture2D)MakeLifeCanvas(f).ToTexture()).ToArray();

    public static Texture2D[] MakeBombFrames() =>
        Enumerable.Range(0, FrameCount).Select(f => (Texture2D)MakeBombCanvas(f).ToTexture()).ToArray();

    public static Texture2D[] MakeFrames() =>
        Enumerable.Range(0, FrameCount).Select(f => (Texture2D)MakeCanvas(f).ToTexture()).ToArray();
}
