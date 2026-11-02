using System;
using Godot;

/// <summary>Pixel art for the wind's visual feedback: small leaves that tumble along. The scene turns them in quarter turns.</summary>
public static class WindArt
{
    public const float Scale = 4;
    public const int LeafTones = 5;
    public const int LeafFrames = 2;

    private static readonly PixelColor[][] Tones =
    {
        new[] { PixelColor.Hex(0x1F5C3A), PixelColor.Hex(0x2F8A4E), PixelColor.Hex(0x56B35F) },
        new[] { PixelColor.Hex(0x2F8A4E), PixelColor.Hex(0x56B35F), PixelColor.Hex(0x8FD0B0) },
        new[] { PixelColor.Hex(0x16432E), PixelColor.Hex(0x23603E), PixelColor.Hex(0x37834F) },
        new[] { PixelColor.Hex(0x6B4A1F), PixelColor.Hex(0xA3702A), PixelColor.Hex(0xC98A3A) },
        new[] { PixelColor.Hex(0x3F6B2A), PixelColor.Hex(0x7BA33A), PixelColor.Hex(0xB5C95A) },
    };

    private static readonly int[][] Frame0Rows =
        { new[] { 2, 3, 4 }, new[] { 1, 2, 3, 4, 5 }, new[] { 0, 1, 2, 3, 4, 5 }, new[] { 1, 2, 3, 4, 5 }, new[] { 2, 3, 4 } };
    private static readonly int[][] Frame1Rows =
        { new int[0], new[] { 2, 3, 4 }, new[] { 0, 1, 2, 3, 4, 5 }, new[] { 2, 3, 4 }, new int[0] };

    /// <summary>A small leaf with a pale midrib. Frame 1 is the leaf seen turning edge-on, so it flutters as it tumbles.</summary>
    public static PixelCanvas MakeLeaf(int tone, int frame)
    {
        var c = new PixelCanvas(7, 5);
        var palette = Tones[tone % Tones.Length];
        var rows = frame == 0 ? Frame0Rows : Frame1Rows;
        for (int y = 0; y < rows.Length; y++)
            foreach (int x in rows[y]) c[x, y] = y == 2 ? palette[2] : (y < 2 ? palette[1] : palette[0]);
        c[0, 2] = palette[0]; // the stem
        return c;
    }

    /// <summary>[tone][frame]</summary>
    public static Texture2D[][] MakeLeafTextures()
    {
        var result = new Texture2D[LeafTones][];
        for (int tone = 0; tone < LeafTones; tone++)
        {
            result[tone] = new Texture2D[LeafFrames];
            for (int f = 0; f < LeafFrames; f++) result[tone][f] = MakeLeaf(tone, f).ToTexture();
        }
        return result;
    }

    // MARK: Stars (the wind of the river of stars)

    public const int StarTones = 3, StarFrames = 2;

    /// <summary>A little star that twinkles (frame 0 a cross of light, frame 1 a smaller one with a halo): white, cyan or gold.</summary>
    public static PixelCanvas MakeStar(int tone, int frame)
    {
        var c = new PixelCanvas(9, 9);
        var core = new[] { PixelColor.Hex(0xFFFFFF), PixelColor.Hex(0xD8F6FF), PixelColor.Hex(0xFFF2B0) }[tone % StarTones];
        var arm = new[] { PixelColor.Hex(0xB8C8F0), PixelColor.Hex(0x58C8E8), PixelColor.Hex(0xE8B040) }[tone % StarTones];
        c[4, 4] = core;
        foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) c[4 + dx, 4 + dy] = frame == 0 ? core : arm;
        if (frame == 0) foreach (var (dx, dy) in new[] { (2, 0), (-2, 0), (0, 2), (0, -2) }) c[4 + dx, 4 + dy] = arm;
        else foreach (var (dx, dy) in new[] { (1, 1), (-1, -1), (1, -1), (-1, 1) }) c[4 + dx, 4 + dy] = arm.WithAlpha(140);
        return c;
    }

    /// <summary>[tone][frame]</summary>
    public static Texture2D[][] MakeStarTextures()
    {
        var result = new Texture2D[StarTones][];
        for (int tone = 0; tone < StarTones; tone++)
        {
            result[tone] = new Texture2D[StarFrames];
            for (int f = 0; f < StarFrames; f++) result[tone][f] = MakeStar(tone, f).ToTexture();
        }
        return result;
    }

    // MARK: Lanterns (the wind of the river of the parties)

    public const int LanternFrames = 2;

    /// <summary>A little cylindrical paper lantern with a halo, seen from the side. Frame 1 is the flame flickering a little brighter.</summary>
    public static PixelCanvas MakeLantern(int tone, int frame)
    {
        var c = new PixelCanvas(11, 13);
        var glow = new[] { PixelColor.Hex(0xFF5A3A), PixelColor.Hex(0xFFA03A), PixelColor.Hex(0xFFD870), PixelColor.Hex(0xFF70A8), PixelColor.Hex(0x50E0D0) }[tone % FestivalArt.LanternTones];
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                float d = MathF.Sqrt(MathF.Pow((x - 5) / 5.5f, 2) + MathF.Pow((y - 6) / 6.5f, 2));
                if (d < 1 && (x + y) % 2 == 0) c[x, y] = glow.WithAlpha((byte)(frame == 0 ? 55 : 90));
            }
        FestivalArt.DrawLantern(c, 5, 2, 7, 9, tone, cord: false, tassel: true);
        return c;
    }

    /// <summary>[tone][frame]</summary>
    public static Texture2D[][] MakeLanternTextures()
    {
        var result = new Texture2D[FestivalArt.LanternTones][];
        for (int tone = 0; tone < result.Length; tone++)
        {
            result[tone] = new Texture2D[LanternFrames];
            for (int f = 0; f < LanternFrames; f++) result[tone][f] = MakeLantern(tone, f).ToTexture();
        }
        return result;
    }
}
