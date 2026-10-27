using System;

/// <summary>A small burst when a spirit is defeated: a ring that spreads and a handful of sparks flying outwards.</summary>
public static class BurstArt
{
    public const int RingFrames = 4;

    public static PixelCanvas MakeRing(int frame)
    {
        int size = 21;
        var c = new PixelCanvas(size, size);
        float centre = (float)size / 2;
        float radius = 3 + frame * 2.3f;
        var color = PixelColor.Hex(0xFFFFFF, (byte)Math.Max(0, 255 - frame * 60));
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - centre, dy = y + 0.5f - centre;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                if (MathF.Abs(d - radius) <= (frame < 2 ? 1.2f : 0.8f)) c[x, y] = color;
            }
        return c;
    }

    /// <summary>A spark in the given colour: a small diamond.</summary>
    public static PixelCanvas MakeSpark(PixelColor color)
    {
        var c = new PixelCanvas(5, 5);
        foreach (var (x, y) in new[] { (2, 0), (1, 1), (2, 1), (3, 1), (0, 2), (1, 2), (2, 2), (3, 2), (4, 2), (1, 3), (2, 3), (3, 3), (2, 4) }) c[x, y] = color;
        c[2, 2] = PixelColor.Hex(0xFFFFFF);
        return c;
    }
}
