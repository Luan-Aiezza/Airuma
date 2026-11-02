using System;
using P = PixelColor;

/// <summary>
/// The waterfall of the seven colours (stage 5), seen as the boat is pulled up it: the water, white and blue, rushes up between two walls of wet rock, dark
/// and streaked by the running water, and breaks against them in foam and spray. Made with the same layers and sizes as <see cref="BankArt"/> (so the river builds it with the
/// same code, see <see cref="Scenery"/>); the right wall is the mirror of a left one made with other rock. Nothing hangs over the water. The water mirrors a night sky with
/// mist in it.
/// </summary>
public static class WaterfallArt
{
    private static readonly P[] Rock = { P.Hex(0x1A2028), P.Hex(0x2C3846), P.Hex(0x425468), P.Hex(0x62788E), P.Hex(0x9AB0C6) };
    private static readonly P Foam = P.Hex(0xF4FBFF), FoamBlue = P.Hex(0xBFE0F4);

    private const int Width = BankArt.Width, Height = BankArt.Height;

    private static int WallEdge(int y, float phase, PeriodicNoise bumps) =>
        BankArt.Edge(y, phase) + (int)MathF.Round((bumps.Sample(0.5f, (float)y / Height) - 0.5f) * 11) + 2;

    private static PixelCanvas Mirror(PixelCanvas c)
    {
        var m = new PixelCanvas(c.Width, c.Height);
        for (int y = 0; y < c.Height; y++) for (int x = 0; x < c.Width; x++) m[c.Width - 1 - x, y] = c[x, y];
        return m;
    }

    private static PixelCanvas Side(BankArt.Side side, Func<ulong, float, PixelCanvas> make) =>
        side == BankArt.Side.Left ? make(0xF411UL, 0.9f) : Mirror(make(0xF412UL, 2.6f));

    /// <summary>The wall of rock, over the boat: dark and wet, streaked from top to bottom by the water that runs down it, with a pale sheen on the ledges.</summary>
    public static PixelCanvas MakeGround(BankArt.Side side) => Side(side, (seed, phase) =>
    {
        var rng = new SeededGenerator(seed);
        var bumps = new PeriodicNoise(3, 17, rng);
        var grain = new PeriodicNoise(14, 10, rng);          // many columns, few rows: it streaks from top to bottom
        var strata = new PeriodicNoise(2, 32, rng);
        var c = new PixelCanvas(Width, Height);
        for (int y = 0; y < Height; y++)
        {
            int edge = WallEdge(y, phase, bumps);
            for (int x = 0; x < edge && x < Width; x++)
            {
                float nx = (float)x / Width, ny = (float)y / Height;
                int depth = edge - x;
                float lit = 0.1f + grain.Sample(nx, ny) * 0.6f + strata.Sample(nx, ny) * 0.3f + (depth <= 2 ? 0.45f : 0) - (depth > 12 ? 0.15f : 0);
                var colour = Heightfield.Pick(Rock, lit, x, y);
                // water running down the rock: pale, thin, vertical lines
                if ((x * 7 + (int)(grain.Sample(nx * 3, ny) * 9)) % 11 == 0 && depth > 1 && (y + x) % 5 < 3) colour = P.Hex(0x7FA0BC);
                c[x, y] = colour;
            }
        }
        return c;
    });

    /// <summary>What the water mirrors of the walls: dark rock with a ragged edge.</summary>
    public static PixelCanvas MakeReflection(BankArt.Side side) => Side(side, (seed, phase) =>
    {
        var rng = new SeededGenerator(seed ^ 0x99);
        var bumps = new PeriodicNoise(3, 17, new SeededGenerator(seed));
        var c = new PixelCanvas(Width + 8, Height);
        for (int y = 0; y < Height; y++)
        {
            int edge = WallEdge(y, phase, bumps) + 6;
            for (int x = 0; x < edge && x < c.Width; x++) c[x, y] = Rock[(x * 3 + y) % 6 == 0 ? 2 : (x + y * 2) % 7 == 0 ? 1 : 0];
        }
        return c;
    });

    /// <summary>The water breaking against the rock, violently: four variants (the scene changes between them many times a second) of a band of white foam along the edge,
    /// thick and ragged, and drops of spray thrown out into the river.</summary>
    public static PixelCanvas[] MakeRipples(BankArt.Side side)
    {
        var result = new PixelCanvas[4];
        for (int v = 0; v < 4; v++)
        {
            var left = new PixelCanvas(Width, Height);
            var bumps = new PeriodicNoise(3, 17, new SeededGenerator(side == BankArt.Side.Left ? 0xF411UL : 0xF412UL));
            var rng = new SeededGenerator(0xF0A0UL + (ulong)v * 977 + (side == BankArt.Side.Left ? 0UL : 31UL));
            for (int y = 0; y < Height; y++)
            {
                int edge = WallEdge(y, side == BankArt.Side.Left ? 0.9f : 2.6f, bumps);
                // the foam: a thick band, ragged
                int thickness = 2 + (int)(rng.NextFloat() * 4);
                for (int k = 0; k < thickness; k++) left.SetWrappedY(edge + k, y, (k + y + v) % 3 == 0 ? FoamBlue : Foam);
                // the spray
                if (rng.NextFloat() < 0.14f) { int d = 6 + (int)(rng.NextFloat() * 8); left.SetWrappedY(edge + d, y, Foam.WithAlpha(210)); if (rng.NextFloat() < 0.5f) left.SetWrappedY(edge + d + 1, y - 1, FoamBlue.WithAlpha(170)); }
            }
            result[v] = side == BankArt.Side.Left ? left : Mirror(left);
        }
        return result;
    }

    /// <summary>The night sky that the water mirrors: deep blue, stars, and streaks of mist rising with the water.</summary>
    public static PixelCanvas MakeSky()
    {
        var rng = new SeededGenerator(0xF1A5);
        var mist = new PeriodicNoise(18, 4, rng);
        int w = SkyArt.Width, h = SkyArt.Height;
        var c = new PixelCanvas(w, h);
        P top = P.Hex(0x14305A), bottom = P.Hex(0x4A80AC);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float t = (float)y / h;
                var colour = top.Mixed(bottom, t);
                float m = mist.Sample((float)x / w, t);
                if (m > 0.42f)
                {
                    float level = MathF.Floor((m - 0.42f) * 6 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / 3;
                    if (level > 0) colour = colour.Mixed(P.Hex(0xDCEEFA), Math.Min(0.8f, level * 0.5f));
                }
                c[x, y] = colour;
            }
        for (int i = 0; i < 130; i++)
        {
            int x = rng.Range(0, w), y = rng.Range(0, h);
            c[x, y] = i % 5 == 0 ? P.Hex(0xFFFFFF) : P.Hex(0xAFC8FF);
        }
        return c;
    }
}
