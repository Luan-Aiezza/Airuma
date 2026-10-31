using Godot;
using System;

/// <summary>
/// Shared helpers for the pixel art of the bosses and their effects: the scale of the effects, a ramp colour picked with ordered dithering,
/// plotting, rounding, and the foam rings that spread on the water round a boss. The art of each boss is in its own class (see
/// <see cref="BossStyles"/> for how a boss is put together from it).
/// </summary>
public static class BossArt
{
    /// <summary>Screen pixels per art pixel for effects (rings, sparks).</summary>
    public const float Scale = 4;
    /// <summary>The serpent and its body (retired art, see Art/Retired) are drawn on a finer grid: 2 screen pixels per art pixel.</summary>
    public const float SerpentScale = 2;
    public const int FrameCount = 4;

    // MARK: Palette

    public static readonly PixelColor[] Skin =
    {
        PixelColor.Hex(0xD9A06A), PixelColor.Hex(0xB97C4D), PixelColor.Hex(0x8F5A35), PixelColor.Hex(0x5E3A24),
    };
    public static PixelColor[] Scales => FirePalette.Scales;
    public static readonly PixelColor Gold = PixelColor.Hex(0xF2C14E);
    public static readonly PixelColor Ink = PixelColor.Hex(0x1A0F0A);
    public static readonly PixelColor Lips = PixelColor.Hex(0xB3342F);
    public static readonly PixelColor Foam = PixelColor.Hex(0xD6EBDD);

    /// <summary>Picks a ramp colour for `t` in 0...1, with ordered dithering between steps.</summary>
    public static PixelColor Pick(PixelColor[] ramp, float t, int x, int y)
    {
        float f = t * ramp.Length + (Bayer.Threshold(x, y) - 0.5f) * 0.8f;
        return ramp[Math.Max(0, Math.Min(ramp.Length - 1, (int)MathF.Floor(f)))];
    }

    public static void Put(PixelCanvas c, int x, int y, PixelColor color)
    {
        if (c.Contains(x, y)) c[x, y] = color;
    }

    /// <summary>Rounds halves away from zero.</summary>
    internal static float Rnd(float v) => MathF.Round(v, MidpointRounding.AwayFromZero);

    /// <summary>Expanding foam rings on the water. `front` draws the half nearest the viewer, otherwise the far half.</summary>
    public static void DrawRipples(PixelCanvas c, int cx, int cy, float rx, float ry, int frame, bool front)
    {
        for (int k = 0; k < 2; k++)
        {
            float f = Trunc((float)frame / FrameCount + k * 0.5f);
            float r = 0.45f + 0.55f * f;
            var color = Foam.WithAlpha((byte)(210 * (1 - f)));
            int steps = 140;
            for (int i = 0; i < steps; i++)
            {
                if (i % 3 == 0) continue;
                float angle = 2 * MathF.PI * i / steps;
                float s = MathF.Sin(angle);
                if (front != (s >= 0)) continue;
                Put(c, cx + (int)Rnd(MathF.Cos(angle) * rx * r), cy + (int)Rnd(s * ry * r), color);
            }
        }
    }

    /// <summary>The fractional part (the remainder of dividing by 1).</summary>
    internal static float Trunc(float v) => v % 1f;
}
