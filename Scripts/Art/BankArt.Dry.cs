using System;
using System.Collections.Generic;
using P = PixelColor;

/// <summary>
/// The banks of the black forest of stage 3: dry, dead trees, with bare, gnarled branches and hardly a leaf; grey-brown wood, no moss, a few dry leaves on the
/// ground. Made like the banks of the jungle (the same layers, the same sizes, see <see cref="BankArt"/>), so the river builds either with the same code.
/// </summary>
public static partial class BankArt
{
    private static readonly P[] DryWood = { P.Hex(0x0F0D0E), P.Hex(0x201B1A), P.Hex(0x372E2B), P.Hex(0x544740), P.Hex(0x76675C) };
    private static readonly P[] DryLeaf = { P.Hex(0x2A1E14), P.Hex(0x4A3420), P.Hex(0x6E4E2C) };

    /// <summary>The ground and the trunks and roots of dead trees, over the boat: grey and cracked, with dry leaves, no moss.</summary>
    public static PixelCanvas MakeRootsDry(Side side)
    {
        var left = MakeRootGeometry(0xB0071, 0.4f).Field;
        var field = side == Side.Left ? left : MakeRootGeometry(0xB0072, 2.2f).Field.MirroredX();
        var rng = new SeededGenerator(side == Side.Left ? 31UL : 32UL);
        var bark = new PeriodicNoise(9, 3, rng);
        var cracks = new PeriodicNoise(14, 9, rng);

        return field.Shade(s =>
        {
            float nx = (float)s.X / Width, ny = (float)s.Y / Height;
            int hash = Math.Abs((s.X * 73 + s.Y * 151) % 47);
            if (s.Edge && !s.Rim) return DryWood[0];
            // The cold light catches the upper edges, sparsely.
            if (s.Top && s.Lit > 0.4f && (s.X + s.Y * 2) % 3 == 0) return hash == 0 ? MoonGleam : MoonRim.Scaled(0.55f);
            float streak = bark.Sample(side == Side.Left ? nx : 1 - nx, ny);
            float lit = s.Lit + (streak - 0.5f) * 0.55f;
            P color = Heightfield.Pick(DryWood, lit, s.X, s.Y);
            // Cracks in the dry bark, and bare patches.
            if (cracks.Sample(side == Side.Left ? nx : 1 - nx, ny) > 0.72f && s.H > 0.25f) color = DryWood[Math.Max(0, Array.IndexOf(DryWood, color) - 1)];
            if (hash == 3) color = DryWood[1];
            // Dry leaves on the lower ground.
            if (s.H < 0.36f && hash == 11) color = DryLeaf[(s.X + s.Y) % DryLeaf.Length];
            if (s.GapSide <= 1 && s.H < 0.3f) color = color.Scaled(0.65f);
            return color;
        });
    }

    /// <summary>
    /// Bare branches reaching over the water, over the boat: long, thin and gnarled, forking again and again, with a dead leaf here and there, and a few
    /// broken twigs hanging down. (There are more branches than in the jungle, and almost no leaves.)
    /// </summary>
    public static PixelCanvas MakeBranchesDry(Side side)
    {
        var rng = new SeededGenerator(side == Side.Left ? 0xDEA1UL : 0xDEA2UL);
        var wood = new Heightfield(Width, Height);
        var leaves = new Heightfield(Width, Height);

        const int count = 7;
        for (int i = 0; i < count; i++)
        {
            float y = (i + rng.Range(0.15f, 0.85f)) * Height / count;
            float e = Edge((int)y % Height, side == Side.Left ? 0.4f : 2.2f);
            Branch(wood, leaves, rng, e - 8, y, rng.Range(-0.55f, 0.55f), rng.Range(18f, 34f), rng.Range(1.3f, 2.0f), 0);
        }
        var woodCanvas = MirroredIfRight(wood, side).Shade(s =>
        {
            if (s.Edge && !s.Rim) return DryWood[0];
            return Heightfield.Pick(DryWood, s.Lit, s.X, s.Y);
        });
        var leafCanvas = MirroredIfRight(leaves, side).Shade(s => s.Edge && !s.Rim ? DryLeaf[0] : Heightfield.Pick(DryLeaf, s.Lit, s.X, s.Y));
        var @out = new PixelCanvas(Width, Height);
        foreach (var canvas in new[] { woodCanvas, leafCanvas })
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) if (canvas[x, y].A > 0) @out[x, y] = canvas[x, y];
        return @out;
    }

    /// <summary>One bare branch, thinning along its length and forking into smaller ones, with an odd dead leaf.</summary>
    private static void Branch(Heightfield wood, Heightfield leaves, SeededGenerator rng, float x, float y, float angle, float length, float radius, int depth)
    {
        float bend = rng.Range(-7f, 7f);
        float s = 0;
        var forkAt = new[] { rng.Range(0.3f, 0.5f), rng.Range(0.55f, 0.8f) };
        var forked = new[] { false, false };
        while (s <= length)
        {
            float t = s / length;
            float px = x + MathF.Cos(angle) * s - MathF.Sin(angle) * bend * t * t + 0.9f * MathF.Sin(t * 11 + length);
            float py = y + MathF.Sin(angle) * s + MathF.Cos(angle) * bend * t * t;
            wood.Stamp(px, py, MathF.Max(0.75f, radius * (1 - t * 0.75f)), 0.5f);
            if (depth < 3)
                for (int k = 0; k < 2; k++)
                {
                    if (forked[k] || t < forkAt[k]) continue;
                    forked[k] = true;
                    float a2 = angle + (RandomBool(rng) ? 1 : -1) * rng.Range(0.45f, 1.0f);
                    Branch(wood, leaves, rng, px, py, a2, length * rng.Range(0.35f, 0.6f), radius * 0.62f, depth + 1);
                }
            // an odd dead leaf, hanging from the thin end of a twig
            if (depth >= 1 && t > 0.6f && rng.Range(0f, 1f) < 0.05f) leaves.Stamp(px + rng.Range(-1f, 1f), py + rng.Range(0.5f, 2.5f), 1.1f, 0.7f);
            s += 0.8f;
        }
    }

    /// <summary>What the water mirrors of the dry banks: a network of bare branches, black against the sky, not a canopy.</summary>
    public static PixelCanvas MakeCrownsDry(Side side)
    {
        var rng = new SeededGenerator(side == Side.Left ? 0xDC0AUL : 0xDC0BUL);
        var field = new Heightfield(Width + 8, Height);
        for (int n = 0; n < 34; n++)
        {
            float y = rng.Range(0f, (float)Height);
            Branch(field, field, rng, rng.Range(-2f, 8f), y, rng.Range(-0.7f, 0.7f), rng.Range(14f, 30f), rng.Range(1.1f, 1.7f), 1);
        }
        // the trunks, close to the bank
        for (int y = 0; y < Height; y++) for (int x = 0; x < 9; x++) if ((x + y / 5) % 4 != 0) field.Raise(x, y, 0.55f);
        return MirroredIfRight(field, side).Shade(s =>
        {
            if (s.Edge && !s.Rim) return DryWood[0];
            if (s.Top && s.Lit > 0.5f && (s.X + s.Y * 2) % 4 == 0) return MoonRim.Scaled(0.7f);
            return Heightfield.Pick(DryWood, s.Lit * 0.8f, s.X, s.Y);
        });
    }
}
