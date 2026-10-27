using System;
using System.Collections.Generic;
using P = PixelColor;

/// <summary>
/// The river's banks, seen very close: only the roots of the great trees remain in view, with a few branches,
/// leaves and vines reaching over the water. A third layer, the crowns, is never drawn directly: the water
/// reflects it.
///
/// Everything is lit by the moon from the upper left, built from height maps (see Heightfield), and tiles
/// vertically so it can scroll forever. The right bank is a mirrored copy of different geometry, shaded with the
/// same light so shadows stay consistent.
/// </summary>
public static class BankArt
{
    public const int Width = 40;
    public const int Height = 256;
    /// <summary>Screen points per art pixel.</summary>
    public const float PixelScale = 4;

    // MARK: Palettes

    private static readonly P[] RootRamp =
    {
        P.Hex(0x120C0A), P.Hex(0x24160F), P.Hex(0x3A2418),
        P.Hex(0x5A3A26), P.Hex(0x7A5236),
    };
    private static readonly P[] MossRamp = { P.Hex(0x1C3B27), P.Hex(0x2E5A38), P.Hex(0x4C8A4F) };
    private static readonly P[] LeafRamp =
    {
        P.Hex(0x0D2A1F), P.Hex(0x16432E), P.Hex(0x23603E),
        P.Hex(0x37834F), P.Hex(0x5BAE74),
    };
    /// <summary>Crowns only exist as a reflection, so they are drawn dark and cool, tinted by the water.</summary>
    private static readonly P[] CrownRamp = MakeCrownRamp();
    private static P[] MakeCrownRamp()
    {
        P[] src =
        {
            P.Hex(0x123A2A), P.Hex(0x1D5A3C), P.Hex(0x2C7C4E),
            P.Hex(0x45A065), P.Hex(0x6CC48A),
        };
        var r = new P[src.Length];
        for (int i = 0; i < src.Length; i++) r[i] = src[i].Scaled(0.6f).Mixed(P.Hex(0x0B2A3C), 0.3f);
        return r;
    }
    private static readonly P MoonRim = P.Hex(0x9CC4DA);
    private static readonly P MoonGleam = P.Hex(0xC8E6F0);
    private static readonly P[] Vine = { P.Hex(0x17402A), P.Hex(0x2A6B3F) };

    public enum Side { Left, Right }

    // MARK: Geometry helpers

    /// <summary>Where the left bank's ground ends (in pixels from the screen edge) on a row; periodic in Height.</summary>
    public static int Edge(int y, float phase)
    {
        float t = (float)y / Height * 2 * MathF.PI;
        return (int)BossArt.Rnd(16 + 4 * MathF.Sin(t * 2 + phase) + 3 * MathF.Sin(t * 5 + phase * 2.3f));
    }

    private sealed class Geometry
    {
        public Heightfield Field;
        public List<(int x, int y)> Tips;
    }

    private static Heightfield MirroredIfRight(Heightfield f, Side side) => side == Side.Right ? f.MirroredX() : f;

    private static Geometry MakeRootGeometry(ulong seed, float phase)
    {
        var rng = new SeededGenerator(seed);
        var field = new Heightfield(Width, Height);
        var tips = new List<(int x, int y)>();

        // Ground: a mound that slopes down to the water.
        var lumps = new PeriodicNoise(5, 10, rng);
        for (int y = 0; y < Height; y++)
        {
            int e = Edge(y, phase);
            for (int x = 0; x < e; x++)
            {
                float slope = (float)(e - x) / 12;
                float s = MathF.Min(1, slope);
                float bse = 0.10f + 0.32f * (s * s * (3 - 2 * s));
                field.Raise(x, y, bse + 0.10f * lumps.Sample((float)x / Width, (float)y / Height));
            }
        }

        // Buttress ridges running along the bank.
        for (int n = 0; n < 12; n++)
        {
            float y0 = rng.Range(0f, (float)Height);
            float x0 = rng.Range(3f, 12f);
            float length = rng.Range(30f, 70f);
            float radius = rng.Range(2.0f, 3.6f);
            float wobble = rng.Range(0f, 6f);
            float s = 0;
            while (s < length)
            {
                float t = s / length;
                field.Stamp(x0 + 3 * MathF.Sin(t * 5 + wobble), y0 + s, radius * (0.6f + 0.4f * MathF.Sin(t * MathF.PI)), 0.55f);
                s += 0.8f;
            }
        }

        // Great roots reaching into the river.
        int count = 9;
        for (int i = 0; i < count; i++)
        {
            float y = (i + rng.Range(0.1f, 0.9f)) * Height / count;
            float e = Edge((int)y % Height, phase);
            float angle = rng.Range(-0.55f, 0.55f);
            AddRoot(field, tips, rng, e - 7, y, angle,
                    rng.Range(16f, 26f), rng.Range(3.4f, 5.2f), 0);
        }
        return new Geometry { Field = field, Tips = tips };
    }

    private static void AddRoot(Heightfield field, List<(int x, int y)> tips, SeededGenerator rng,
                                float x, float y, float angle, float length, float radius, int depth)
    {
        float bend = rng.Range(-5f, 5f);
        float wobble = rng.Range(0f, 6f);
        var forkAt = new[] { rng.Range(0.3f, 0.5f), rng.Range(0.6f, 0.8f) };
        var forked = new[] { false, false };
        float s = 0;
        float px = x, py = y;
        while (s <= length)
        {
            float t = s / length;
            var along = (MathF.Cos(angle), MathF.Sin(angle));
            var perp = (-along.Item2, along.Item1);
            float curve = bend * t * t + 0.8f * MathF.Sin(t * 9 + wobble);
            px = x + along.Item1 * s + perp.Item1 * curve;
            py = y + along.Item2 * s + perp.Item2 * curve;
            float r = MathF.Max(1.1f, radius * MathF.Pow(1 - t * 0.78f, 0.9f));
            field.Stamp(px, py, r, 0.42f + r * 0.11f);
            if (depth < 2)
            {
                for (int k = 0; k < 2; k++)
                {
                    if (!(!forked[k] && t >= forkAt[k])) continue;
                    forked[k] = true;
                    float side = RandomBool(rng) ? 1 : -1;
                    float a2 = angle + side * rng.Range(0.5f, 0.9f);
                    float len2 = length * rng.Range(0.35f, 0.55f);
                    AddRoot(field, tips, rng, px, py, a2, len2, r * 0.7f, depth + 1);
                }
            }
            s += 0.7f;
        }
        if (depth == 0) tips.Add(((int)px, (int)py));
    }

    /// <summary>A coin toss from the generator: bit 17 of the next value.</summary>
    private static bool RandomBool(SeededGenerator rng) => ((rng.NextULong() >> 17) & 1) == 0;

    // MARK: Roots

    public static PixelCanvas MakeRoots(Side side)
    {
        var left = MakeRootGeometry(0xB0071, 0.4f).Field;
        var field = side == Side.Left ? left : MakeRootGeometry(0xB0072, 2.2f).Field.MirroredX();
        var rng = new SeededGenerator(side == Side.Left ? 11UL : 12UL);
        var moss = new PeriodicNoise(7, 12, rng);

        var bark = new PeriodicNoise(9, 3, rng); // long streaks running with the roots
        P[] litter = { P.Hex(0x6B4A2A), P.Hex(0x8A5A2A), P.Hex(0x3F6B3A), P.Hex(0x58421F) };

        return field.Shade(s =>
        {
            float nx = (float)s.X / Width, ny = (float)s.Y / Height;
            float mossy = moss.Sample(side == Side.Left ? nx : 1 - nx, ny);
            int hash = Math.Abs((s.X * 73 + s.Y * 151) % 47);

            if (s.Edge && !s.Rim) return RootRamp[0];
            // Moonlight catches the upper edges, sparsely.
            if (s.Top && s.Lit > 0.4f && (s.X + s.Y * 2) % 3 == 0) return hash == 0 ? MoonGleam : MoonRim.Scaled(0.75f);

            float streak = bark.Sample(side == Side.Left ? nx : 1 - nx, ny);
            float lit = s.Lit + (streak - 0.5f) * 0.55f;
            P color;
            if (mossy > 0.5f && lit > 0.25f && s.H > 0.2f)
            {
                color = Heightfield.Pick(MossRamp, lit, s.X, s.Y);
            }
            else
            {
                color = Heightfield.Pick(RootRamp, lit, s.X, s.Y);
                if (hash == 3) color = RootRamp[1]; // bark pits
            }
            // Fallen leaves and twigs on the lower ground.
            if (s.H < 0.36f && hash == 11) color = litter[(s.X + s.Y) % litter.Length];
            // Wet, dark ground right where it meets the water.
            if (s.GapSide <= 1 && s.H < 0.3f) color = color.Scaled(0.65f);
            return color;
        });
    }

    /// <summary>Foam rings where the roots meet the water, in two sizes that the scene alternates between.</summary>
    public static PixelCanvas[] MakeRipples(Side side)
    {
        var left = MakeRootGeometry(0xB0071, 0.4f);
        var geo = side == Side.Left ? left : MakeRootGeometry(0xB0072, 2.2f);
        var foam = P.Hex(0xBFE3EE);
        var result = new PixelCanvas[2];
        for (int variant = 0; variant < 2; variant++)
        {
            var c = new PixelCanvas(Width, Height);
            foreach (var tip in geo.Tips)
            {
                int tx = side == Side.Left ? tip.x : Width - 1 - tip.x;
                float rx = 2.5f + variant * 1.5f, ry = 1.2f + variant * 0.6f;
                for (int i = 0; i < 28; i++)
                {
                    if (!(i % 3 != 0 && i < 18)) continue;
                    float a = (float)i / 28 * 2 * MathF.PI;
                    int x = tx + (int)BossArt.Rnd(MathF.Cos(a) * rx) + 1, y = tip.y + (int)BossArt.Rnd(MathF.Sin(a) * ry);
                    c.SetWrappedY(x, y, foam.WithAlpha((byte)(variant == 0 ? 120 : 70)));
                }
            }
            result[variant] = c;
        }
        return result;
    }

    // MARK: Branches

    /// <summary>Branches and leaves reaching over the water from the trees, with hanging vines.</summary>
    public static PixelCanvas MakeBranches(Side side)
    {
        var rng = new SeededGenerator(side == Side.Left ? 0xBEA1UL : 0xBEA2UL);
        var wood = new Heightfield(Width, Height);
        var leaves = new Heightfield(Width, Height);
        var vines = new Heightfield(Width, Height);

        int count = 4;
        for (int i = 0; i < count; i++)
        {
            float y = (i + rng.Range(0.15f, 0.85f)) * Height / count;
            float e = Edge((int)y % Height, side == Side.Left ? 0.4f : 2.2f);
            float angle = rng.Range(-0.5f, 0.5f);
            float length = rng.Range(16f, 31f);
            float radius = rng.Range(1.7f, 2.6f);
            float bend = rng.Range(-6f, 6f);

            float s = 0;
            while (s <= length)
            {
                float t = s / length;
                float px = e - 8 + MathF.Cos(angle) * s - MathF.Sin(angle) * bend * t * t;
                float py = y + MathF.Sin(angle) * s + MathF.Cos(angle) * bend * t * t;
                wood.Stamp(px, py, MathF.Max(0.9f, radius * (1 - t * 0.7f)), 0.5f);

                if (t > 0.15f && rng.Range(0f, 1f) < 0.75f)
                {
                    // A cluster of leaves along the branch.
                    int n = rng.Range(2, 4);
                    for (int q = 0; q < n; q++)
                    {
                        float lx = px + rng.Range(-4.5f, 4.5f);
                        float ly = py + rng.Range(-4.5f, 4.5f);
                        float dir = rng.Range(0f, 2 * MathF.PI);
                        for (int k = 0; k < 3; k++)
                        {
                            leaves.Stamp(lx + MathF.Cos(dir) * k * 1.1f, ly + MathF.Sin(dir) * k * 1.1f,
                                         k == 1 ? 1.7f : 1.3f, rng.Range(0.5f, 0.95f));
                        }
                    }
                }
                if (t > 0.3f && rng.Range(0f, 1f) < 0.12f)
                {
                    // A vine hanging down from the branch.
                    float drop = rng.Range(8f, 20f);
                    float d = 0;
                    while (d < drop)
                    {
                        vines.Stamp(px + MathF.Sin(d * 0.5f) * 0.8f, py + d, 0.9f, 0.45f);
                        if ((int)d % 5 == 4) leaves.Stamp(px + ((int)d % 2 == 0 ? 1.6f : -1.6f), py + d, 1.3f, 0.7f);
                        d += 0.8f;
                    }
                }
                s += 0.9f;
            }
        }

        var woodCanvas = MirroredIfRight(wood, side).Shade(s =>
        {
            if (s.Edge && !s.Rim) return RootRamp[0];
            return Heightfield.Pick(RootRamp, s.Lit, s.X, s.Y);
        });
        var vineCanvas = MirroredIfRight(vines, side).Shade(s => Vine[s.Lit > 0.5f ? 1 : 0]);
        var leafCanvas = MirroredIfRight(leaves, side).Shade(s =>
        {
            if (s.Edge && !s.Rim) return LeafRamp[0];
            if (s.Top && s.Lit > 0.5f && (s.X + s.Y * 2) % 3 == 0) return MoonRim;
            return Heightfield.Pick(LeafRamp, s.Lit, s.X, s.Y);
        });

        var @out = new PixelCanvas(Width, Height);
        foreach (var canvas in new[] { woodCanvas, vineCanvas, leafCanvas })
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) if (canvas[x, y].A > 0) @out[x, y] = canvas[x, y];
        return @out;
    }

    // MARK: Crowns (reflection only)

    /// <summary>The tree crowns high above the banks. They never appear directly: the water mirrors them.</summary>
    public static PixelCanvas MakeCrowns(Side side)
    {
        var rng = new SeededGenerator(side == Side.Left ? 0xC40AUL : 0xC40BUL);
        var field = new Heightfield(Width + 8, Height);
        for (int n = 0; n < 95; n++)
        {
            float cx = rng.Range(-2f, Width * 0.55f);
            float cy = rng.Range(0f, (float)Height);
            float radius = rng.Range(3.5f, 8f);
            field.Stamp(cx, cy, radius, rng.Range(0.5f, 1.0f));
        }
        // The canopy is solid close to the bank and ragged further out.
        for (int y = 0; y < Height; y++) for (int x = 0; x < 14; x++) field.Raise(x, y, 0.55f);
        return MirroredIfRight(field, side).Shade(s =>
        {
            if (s.Edge && !s.Rim) return CrownRamp[0];
            if (s.Top && s.Lit > 0.4f && (s.X + s.Y * 2) % 3 == 0) return MoonRim;
            return Heightfield.Pick(CrownRamp, s.Lit, s.X, s.Y);
        });
    }
}
