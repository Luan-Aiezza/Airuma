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
}
