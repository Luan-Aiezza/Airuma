using P = PixelColor;

/// <summary>A bird of the Andes seen from above, as the wind carries it (a condor: black, with a white ruff and white patches on its wings): head up, three frames of its wings (raised, level and lowered).</summary>
public static class BirdArt
{
    public const int Width = 21, Height = 13, FrameCount = 3;

    public static PixelCanvas MakeCanvas(int frame)
    {
        var c = new PixelCanvas(Width, Height);
        P black = P.Hex(0x161820), dark = P.Hex(0x2C3040), white = P.Hex(0xF0F4FA), shade = P.Hex(0xA8B4C8), beak = P.Hex(0xE8D8A8);
        int cx = Width / 2;
        // the wings: from the shoulders out, drooping or lifting with the frame
        int[] tipRise = { 3, 0, -3 };                      // how much the tips are higher (+) or lower (-) than the shoulders
        foreach (int side in new[] { -1, 1 })
            for (int i = 1; i <= 9; i++)
            {
                float t = i / 9f;
                int x = cx + side * i;
                int yTop = 5 - (int)(tipRise[frame % 3] * t) + (int)(t * t * 3) - 1, yBottom = yTop + (int)(3.5f - t * 2.2f);
                for (int y = yTop; y <= yBottom; y++)
                    if (c.Contains(x, y)) c[x, y] = (i >= 4 && i <= 7 && y > yTop && y < yBottom) ? white : (y == yTop ? dark : black);
                // the feathers at the tip
                if (i >= 8) for (int k = 1; k <= 2; k++) if (c.Contains(x, yBottom + k)) c[x, yBottom + k] = dark;
            }
        // the body, the ruff, the neck and the head
        for (int y = 4; y <= 11; y++) { c[cx, y] = black; if (y >= 5 && y <= 10) { c[cx - 1, y] = black; c[cx + 1, y] = y < 8 ? dark : black; } }
        c[cx, 11] = dark; c[cx - 1, 11] = black; c[cx + 1, 11] = black;
        for (int x = cx - 2; x <= cx + 2; x++) c[x, 4] = x == cx ? shade : white;
        c[cx, 3] = black; c[cx - 1, 3] = dark; c[cx + 1, 3] = dark;
        c[cx, 2] = black; c[cx, 1] = beak;
        return c;
    }
}
