using System;
using System.Collections.Generic;
using P = PixelColor;

/// <summary>
/// The cave of stage 4: grey rock walls on both sides, with moss that glows green and stalagmites rising from them, and, on the water, the shadows of the stalactites of the ceiling; a ceiling that the still water
/// reflects, lit by patches of glowing moss; and, for the boss fight, an open part of the cave with a
/// round rock platform in the middle at the top, lit by the moon, where the boss stands. Made with the same layers and sizes as <see cref="BankArt"/>
/// (so the river builds it with the same code, see <see cref="Scenery"/>); the left bank is made first and the right one is its mirror, with other rock.
/// </summary>
public static class CaveArt
{
    private static readonly P[] Rock = { P.Hex(0x10161A), P.Hex(0x222B31), P.Hex(0x35424B), P.Hex(0x4C5E6A), P.Hex(0x6A8090) };
    private static readonly P[] Moss = { P.Hex(0x1A5240), P.Hex(0x2A8A5E), P.Hex(0x4CC27E), P.Hex(0xA8F0B0) };
    private static readonly P Drip = P.Hex(0xDDF4FF);

    private const int Width = BankArt.Width, Height = BankArt.Height;

    // MARK: The walls

    /// <summary>Where the wall ends on a row (in px from the edge of the picture), ragged with boulders; periodic in Height.</summary>
    private static int WallEdge(int y, float phase, PeriodicNoise bumps) =>
        BankArt.Edge(y, phase) + (int)MathF.Round((bumps.Sample(0.5f, (float)y / Height) - 0.5f) * 9);

    private static PixelCanvas Mirror(PixelCanvas c)
    {
        var m = new PixelCanvas(c.Width, c.Height);
        for (int y = 0; y < c.Height; y++) for (int x = 0; x < c.Width; x++) m[c.Width - 1 - x, y] = c[x, y];
        return m;
    }

    private static PixelCanvas Side(BankArt.Side side, Func<ulong, float, PixelCanvas> make) =>
        side == BankArt.Side.Left ? make(0xCA71UL, 0.4f) : Mirror(make(0xCA72UL, 2.2f));

    /// <summary>The rock of the wall, over the boat: grey, cracked, with moss that glows near the water.</summary>
    public static PixelCanvas MakeGround(BankArt.Side side) => Side(side, (seed, phase) =>
    {
        var rng = new SeededGenerator(seed);
        var bumps = new PeriodicNoise(3, 23, rng);
        var grain = new PeriodicNoise(10, 32, rng);
        var cracks = new PeriodicNoise(16, 24, rng);
        var mossy = new PeriodicNoise(8, 26, rng);
        var c = new PixelCanvas(Width, Height);
        for (int y = 0; y < Height; y++)
        {
            int edge = WallEdge(y, phase, bumps);
            for (int x = 0; x < edge && x < Width; x++)
            {
                float nx = (float)x / Width, ny = (float)y / Height;
                int depth = edge - x;
                float lit = 0.15f + grain.Sample(nx, ny) * 0.65f + (depth <= 2 ? 0.4f : 0) - (depth > 14 ? 0.15f : 0);
                // the wall slopes up from the water: lit from the glow of the moss and the light of the boat, darker away from it
                var colour = Heightfield.Pick(Rock, lit, x, y);
                if (cracks.Sample(nx, ny) > 0.74f && depth > 2) colour = Rock[Math.Max(0, Array.IndexOf(Rock, colour) - 1)];
                float m = mossy.Sample(nx, ny);
                if (m > 0.62f && depth < 4 + (m - 0.62f) * 28)
                {
                    float glow = Math.Clamp((m - 0.62f) * 3f + (6 - depth) * 0.05f, 0, 0.8f);
                    colour = Heightfield.Pick(Moss, glow, x + 1, y);
                }
                c[x, y] = colour;
            }
        }
        // Stalagmites along the edge: cones of rock that rise from the ground into the water, pointed at the top, lit on the left, with moss at their feet.
        var rng2 = new SeededGenerator(seed ^ 0x77);
        const int count = 16;
        for (int i = 0; i < count; i++)
        {
            int baseY = (int)((i + rng2.Range(0.1f, 0.9f)) * Height / count);
            int edge = WallEdge(baseY % Height, phase, bumps);
            int n = 1 + (int)(rng2.NextFloat() * 2.5f);
            for (int k = 0; k < n; k++)
            {
                int cx = edge + (int)rng2.Range(0f, 3f) + k * 3 - 1;
                int by = baseY + k * 3 + (int)rng2.Range(-2f, 2f);
                int len = (int)(rng2.Range(7f, 15f) * (k == 0 ? 1.2f : 0.8f));
                int w = (int)rng2.Range(4f, 7f);
                for (int j = 0; j <= len; j++)
                {
                    float t = (float)j / len;                                            // 0 at the tip, 1 at the foot
                    float half = w / 2f * MathF.Pow(t, 0.8f);
                    int y = by - len + j;
                    for (int x = (int)MathF.Floor(cx - half); x <= (int)MathF.Ceiling(cx + half); x++)
                    {
                        if (x < 0 || x >= Width) continue;
                        float across = half < 0.5f ? 0.5f : (x - (cx - half)) / (2 * half);
                        float lit = 1.0f - across * 0.6f - t * 0.25f;
                        var colour = Heightfield.Pick(Rock, lit, x, y);
                        if (x == (int)MathF.Floor(cx - half) && j > 1) colour = Rock[1];
                        if (j <= 1) colour = Rock[4];
                        if (t > 0.8f && rng2.NextFloat() < 0.35f) colour = Moss[1];
                        c.SetWrappedY(x, y, colour);
                    }
                }
            }
        }
        return c;
    });

    /// <summary>What the water mirrors of the walls: dark rock with the shadows of stalactites along its edge, and the moss glowing in it.</summary>
    public static PixelCanvas MakeReflection(BankArt.Side side) => Side(side, (seed, phase) =>
    {
        var rng = new SeededGenerator(seed ^ 0x99);
        var bumps = new PeriodicNoise(3, 23, new SeededGenerator(seed));
        var mossy = new PeriodicNoise(5, 17, new SeededGenerator(seed ^ 3));
        var c = new PixelCanvas(Width + 8, Height);
        for (int y = 0; y < Height; y++)
        {
            int edge = WallEdge(y, phase, bumps) + 6;
            for (int x = 0; x < edge && x < c.Width; x++)
            {
                float ny = (float)y / Height;
                var colour = Rock[(x * 3 + y) % 5 == 0 ? 2 : (x + y * 2) % 7 == 0 ? 1 : 0];
                if (mossy.Sample((float)x / Width, ny) > 0.6f && edge - x < 12) colour = Moss[(x + y) % 3 == 0 ? 2 : 1].Mixed(Rock[0], 0.35f);
                c[x, y] = colour;
            }
        }
        // The shadows of the stalactites of the ceiling, falling on the water along the edge: teardrops, wide where they hang and pointed at the bottom, some of
        // them a little off the rock, as a shadow on the water is.
        for (int i = 0; i < 24; i++)
        {
            int y0 = rng.Range(0, Height), x0 = WallEdge(y0, phase, bumps) + 6 + (int)rng.Range(-2f, 9f), len = (int)rng.Range(8f, 20f), w = (int)rng.Range(3f, 7f);
            for (int j = 0; j < len; j++)
            {
                float half = w / 2f * MathF.Pow(1 - (float)j / len, 0.8f);
                for (int x = (int)MathF.Floor(x0 - half); x <= (int)MathF.Ceiling(x0 + half); x++) c.SetWrappedY(x, y0 + j, Rock[0]);
            }
        }
        return c;
    });

    /// <summary>The water is still: only a faint, pale line where it touches the rock, which comes and goes between the two variants.</summary>
    public static PixelCanvas[] MakeRipples(BankArt.Side side)
    {
        var result = new PixelCanvas[2];
        var foam = P.Hex(0xBFE3EE);
        for (int variant = 0; variant < 2; variant++)
        {
            var left = new PixelCanvas(Width, Height);
            var bumps = new PeriodicNoise(3, 23, new SeededGenerator(side == BankArt.Side.Left ? 0xCA71UL : 0xCA72UL));
            for (int y = 0; y < Height; y++)
                if ((y + variant * 2) % 5 < 2)
                {
                    int edge = WallEdge(y, side == BankArt.Side.Left ? 0.4f : 2.2f, bumps);
                    left.SetWrappedY(edge, y, foam.WithAlpha((byte)(variant == 0 ? 70 : 40)));
                }
            result[variant] = side == BankArt.Side.Left ? left : Mirror(left);
        }
        return result;
    }

    // MARK: The ceiling, as the water reflects it

    /// <summary>The roof of the cave: rock, with patches of moss that glow (green, a little blue) and tiny lights of fireflies.</summary>
    public static PixelCanvas MakeSky()
    {
        var rng = new SeededGenerator(0xCA7E);
        var big = new PeriodicNoise(5, 5, rng);
        var small = new PeriodicNoise(13, 13, rng);
        var moss = new PeriodicNoise(12, 11, rng);
        int w = SkyArt.Width, h = SkyArt.Height;
        var c = new PixelCanvas(w, h);
        P top = P.Hex(0x10222A), bottom = P.Hex(0x1C3A40);
        P[] stone = { P.Hex(0x0E1A20), P.Hex(0x172A32), P.Hex(0x223A44), P.Hex(0x30505A) };
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float nx = (float)x / w, ny = (float)y / h;
                float n = big.Sample(nx, ny) * 0.65f + small.Sample(nx, ny) * 0.35f;
                var baseColour = top.Mixed(bottom, ny);
                float f = n * stone.Length + (Bayer.Threshold(x, y) - 0.5f) * 0.8f;
                var colour = baseColour.Mixed(stone[Math.Clamp((int)MathF.Floor(f), 0, stone.Length - 1)], 0.7f);
                float m = moss.Sample(nx, ny);
                if (m > 0.64f)
                {
                    float glow = Math.Clamp((m - 0.64f) * 3.2f, 0, 1);
                    colour = colour.Mixed(Heightfield.Pick(Moss, glow * 0.7f, x, y), 0.3f + glow * 0.45f);
                }
                c[x, y] = colour;
            }
        for (int i = 0; i < 150; i++)
        {
            int x = rng.Range(0, w), y = rng.Range(0, h);
            if (moss.Sample((float)x / w, (float)y / h) > 0.5f || i % 4 == 0) c[x, y] = i % 3 == 0 ? Moss[3] : Moss[2];
        }
        return c;
    }

    // MARK: The platform of the boss

    public const int PlatformWidth = 62, PlatformHeight = 34;
    /// <summary>The row of the platform's art where the boss's feet are (the middle of the top).</summary>
    public const int PlatformTopCentreY = 13;

    /// <summary>
    /// A round platform of rock rising from the water, seen from above and in front: a flat top, pale where the moon lights it, cracked, with pebbles, and a wall
    /// round it, dark and stratified, with moss glowing where it meets the water.
    /// </summary>
    public static PixelCanvas MakePlatform()
    {
        var rng = new SeededGenerator(0x91A7);
        var grain = new PeriodicNoise(8, 6, rng);
        var cracks = new PeriodicNoise(11, 8, rng);
        var c = new PixelCanvas(PlatformWidth, PlatformHeight);
        float cx = (PlatformWidth - 1) / 2f, cy = PlatformTopCentreY, rx = 29.5f, ry = 12.5f, wall = 9;
        P[] stone = { P.Hex(0x2C3840), P.Hex(0x435560), P.Hex(0x61798A), P.Hex(0x93B0C4), P.Hex(0xD0E6F2) };
        for (int y = 0; y < PlatformHeight; y++)
            for (int x = 0; x < PlatformWidth; x++)
            {
                float dx = (x - cx) / rx, dyTop = (y - cy) / ry, dyBase = (y - (cy + wall)) / ry;
                float dTop = dx * dx + dyTop * dyTop, dBase = dx * dx + dyBase * dyBase;
                float nx = (float)x / PlatformWidth, ny = (float)y / PlatformHeight;
                if (dTop <= 1)
                {
                    // the top: the moon lights the middle most
                    float light = 1 - MathF.Sqrt(dTop) * 0.85f + (grain.Sample(nx, ny) - 0.5f) * 0.35f;
                    var colour = Heightfield.Pick(stone, light, x, y);
                    if (cracks.Sample(nx, ny) > 0.73f) colour = stone[Math.Max(0, Array.IndexOf(stone, colour) - 2)];
                    if (dTop > 0.82f) colour = stone[Math.Min(4, Array.IndexOf(stone, colour) + 1)];        // the lit rim
                    c[x, y] = colour;
                }
                else if (dBase <= 1)
                {
                    // the wall: dark, in strata, darker towards its foot, with moss at the water
                    float depth = Math.Clamp((y - cy) / (ry + wall), 0, 1);
                    float light = 0.55f - depth * 0.4f - MathF.Abs(dx) * 0.25f + ((x + y / 3) % 5 == 0 ? -0.15f : 0) + (grain.Sample(nx * 2, ny) - 0.5f) * 0.3f;
                    var colour = Heightfield.Pick(Rock, light, x, y);
                    if (dBase > 0.7f && grain.Sample(nx + 0.3f, ny * 1.7f) > 0.5f) colour = Heightfield.Pick(Moss, 0.1f + (dBase - 0.7f) * 1.2f, x, y);
                    if (dBase > 0.93f) colour = Rock[0];
                    c[x, y] = colour;
                }
            }
        // pebbles on the top
        for (int i = 0; i < 9; i++)
        {
            int x = (int)rng.Range(cx - rx * 0.7f, cx + rx * 0.7f), y = (int)rng.Range(cy - ry * 0.55f, cy + ry * 0.6f);
            if (c.Contains(x, y) && c[x, y].A > 0) { c[x, y] = stone[1]; c[x + 1, y] = stone[3]; }
        }
        return c;
    }

    /// <summary>The shaft of moonlight that falls from the hole in the ceiling on to the platform: a pale cone, brighter in its core, to be drawn additively.</summary>
    public static PixelCanvas MakeMoonbeam()
    {
        const int w = 64, h = 50;
        var c = new PixelCanvas(w, h);
        for (int y = 0; y < h; y++)
        {
            float half = 9 + y * 0.52f, fade = MathF.Min(1, y / 8f) * (0.75f + 0.25f * y / h);
            for (int x = 0; x < w; x++)
            {
                float across = MathF.Abs(x + 0.5f - w / 2f) / half;
                if (across >= 1) continue;
                float a = (1 - across * across) * fade;
                float band = MathF.Floor(a * 4 + (Bayer.Threshold(x, y) - 0.5f) * 0.9f) / 4;
                if (band > 0) c[x, y] = P.Hex(0xCFE8FF, (byte)(Math.Clamp(band, 0, 1) * 70));
            }
        }
        return c;
    }

    // MARK: Life in the cave

    public const int FishWidth = 14, FishHeight = 7;

    /// <summary>The shadow of a fish seen from above, swimming right: a body, a tail that swings between the two frames (drawn black; the scene makes it faint).</summary>
    public static PixelCanvas MakeFish(int frame)
    {
        var c = new PixelCanvas(FishWidth, FishHeight);
        var k = P.Hex(0x000000);
        for (int x = 4; x < 13; x++)
        {
            float t = (x - 4) / 8f;
            int half = (int)MathF.Round(2.2f * MathF.Sin(MathF.PI * MathF.Pow(t, 0.8f)));
            for (int y = 3 - half; y <= 3 + half; y++) c[x, y] = k;
        }
        int sway = frame == 0 ? -1 : 1;
        c[3, 3] = k; c[2, 3 + sway] = k; c[1, 3 + sway * 2] = k; c[0, 3 + sway * 2] = k; c[0, 3 + sway * 3 > 6 ? 6 : 3 + sway * 3] = k;
        c[1, 3 + sway] = k; c[2, 3] = k;
        c[7, 0] = k; c[7, 6] = k; c[8, 1] = k; c[8, 5] = k;       // the fins
        return c;
    }

    /// <summary>A firefly: a bright core with a soft halo (drawn additively).</summary>
    public static PixelCanvas MakeFirefly()
    {
        var c = new PixelCanvas(5, 5);
        c[2, 2] = P.Hex(0xF8FFC0);
        foreach (var (x, y) in new[] { (1, 2), (3, 2), (2, 1), (2, 3) }) c[x, y] = P.Hex(0xB8F068, 170);
        foreach (var (x, y) in new[] { (1, 1), (3, 1), (1, 3), (3, 3), (0, 2), (4, 2), (2, 0), (2, 4) }) c[x, y] = P.Hex(0x70C040, 60);
        return c;
    }
}
