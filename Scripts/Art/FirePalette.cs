/// <summary>Colours shared by the boss and bullet art.</summary>
public static class FirePalette
{
    /// <summary>Fire from white-hot (0) to dark red (5).</summary>
    public static readonly PixelColor[] Ramp =
    {
        PixelColor.Hex(0xFFF3B0), PixelColor.Hex(0xFFD23F), PixelColor.Hex(0xFF9A1F),
        PixelColor.Hex(0xF25C12), PixelColor.Hex(0xC42A12), PixelColor.Hex(0x7A1410),
    };

    /// <summary>Serpent scales from dark (0) to light (3).</summary>
    public static readonly PixelColor[] Scales =
    {
        PixelColor.Hex(0x123A26), PixelColor.Hex(0x1F5C3A), PixelColor.Hex(0x2F8A4E), PixelColor.Hex(0x55B35F),
    };
}
