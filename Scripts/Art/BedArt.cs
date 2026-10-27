using System;
using P = PixelColor;

/// <summary>The river bed, faintly visible through the crystal-clear water: dark sand and rounded pebbles.</summary>
public static class BedArt
{
    public const int Width = SkyArt.Width;
    public const int Height = 256;

    private static readonly P[] Ramp =
    {
        P.Hex(0x0E181C), P.Hex(0x16252A), P.Hex(0x20363C),
        P.Hex(0x2E4A50), P.Hex(0x436670),
    };

    public static PixelCanvas MakeCanvas()
    {
        var rng = new SeededGenerator(0xBED5);
        var sand = new PeriodicNoise(12, 10, rng);
        var pebbles = new Heightfield(Width, Height);
        for (int i = 0; i < 240; i++)
        {
            pebbles.Stamp(rng.Range(0f, (float)Width), rng.Range(0f, (float)Height),
                          rng.Range(1.4f, 3.8f), rng.Range(0.4f, 0.9f));
        }
        var c = new PixelCanvas(Width, Height);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float n = sand.Sample((float)x / Width, (float)y / Height);
                float dithered = n * 0.55f + (Bayer.Threshold(x, y) - 0.5f) * 0.2f;
                c[x, y] = Ramp[Math.Max(0, Math.Min(2, (int)MathF.Floor(dithered * 3.2f)))];
            }
        }
        var stones = pebbles.Shade(s =>
        {
            if (s.Edge && !s.Rim) return Ramp[0];
            return Heightfield.Pick(Ramp, s.Lit, s.X, s.Y);
        });
        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) if (stones[x, y].A > 0) c[x, y] = stones[x, y];
        return c;
    }
}
