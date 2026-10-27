using Godot;
using System;
using System.Linq;

/// <summary>
/// Pixel art for the Iryssh, drawn pixel by pixel in code.
///
/// Form 1 is a tall woman with long hair of fire, snakes in her hair, a serpent crown and necklace; she is
/// submerged to the waist, so only her bust shows. Form 2 is a giant flaming serpent seen from above, of
/// which only the head is visible. Each form has <see cref="FrameCount"/> frames (flames lick, ripples expand).
/// </summary>
public static class BossArt
{
    /// <summary>Screen pixels per art pixel for effects (rings, sparks).</summary>
    public const float Scale = 4;
    /// <summary>The serpent and its body are drawn on the same finer grid as the woman.</summary>
    public const float SerpentScale = 2;
    /// <summary>The woman is drawn on a finer grid (like the Minish Cap bosses): 2 screen pixels per art pixel.</summary>
    public const float HumanScale = 2;

    public static float ScaleFor(BossForm form) => form == BossForm.Human ? HumanScale : SerpentScale;
    public const int FrameCount = 4;

    // Canvas sizes, and the art pixel that is the boss's position (its hitbox centre).
    public static readonly (int width, int height) HumanSize = (IrysshWomanArt.Width, IrysshWomanArt.Height);
    public static readonly (int x, int y) HumanCentre = (IrysshWomanArt.Centre.x, IrysshWomanArt.Centre.y);
    public static readonly (int width, int height) SerpentSize = (SerpentBossArt.Width, SerpentBossArt.Height);
    public static readonly (int x, int y) SerpentCentre = (SerpentBossArt.Centre.x, SerpentBossArt.Centre.y);

    public static (int width, int height) Size(BossForm form) => form == BossForm.Human ? HumanSize : SerpentSize;
    public static (int x, int y) Centre(BossForm form) => form == BossForm.Human ? HumanCentre : SerpentCentre;

    public static Vector2 SpriteSize(BossForm form)
    {
        var s = Size(form);
        return new Vector2(s.width * ScaleFor(form), s.height * ScaleFor(form));
    }

    /// <summary>
    /// Where the boss's position sits inside the sprite, as a fraction of its size measured from the TOP-LEFT (Y down):
    /// use Sprite2D.Centered=false and Offset = -SpriteSize * Anchor.
    /// </summary>
    public static Vector2 Anchor(BossForm form)
    {
        var s = Size(form); var c = Centre(form);
        return new Vector2((float)c.x / s.width, (float)c.y / s.height);
    }

    /// <summary>How far below the boss's position (in pixels, screen Y down = positive) the water surface is: what its reflection is mirrored about.</summary>
    public static float Waterline(BossForm form) => form switch
    {
        BossForm.Human => (IrysshWomanArt.WaterRow - HumanCentre.y) * HumanScale,
        _ => 0, // the serpent casts no reflection
    };

    /// <summary>`charging` is the serpent's second attack: its neck is not rising out of the water but trails a body.</summary>
    public static ImageTexture[] MakeFrames(BossForm form, bool charging = false) =>
        Enumerable.Range(0, FrameCount).Select(frame =>
            (form == BossForm.Human ? IrysshWomanArt.MakeCanvas(frame)
                                    : SerpentBossArt.MakeHead(frame, charging)).ToTexture()).ToArray();

    /// <summary>The serpent's frames on a larger canvas: what the flame shader burns round.</summary>
    public static ImageTexture[] MakeFlameFrames(bool charging) =>
        Enumerable.Range(0, FrameCount).Select(frame => SerpentBossArt.Padded(SerpentBossArt.MakeHead(frame, charging), continueTop: charging, emitter: SerpentBossArt.IsEmitter).ToTexture()).ToArray();

    // MARK: Glow

    public const int HumanGlowSize = 72;
    public const int SerpentGlowSize = 122;

    public static ImageTexture[] MakeGlowFrames(BossForm form)
    {
        int size = form == BossForm.Human ? HumanGlowSize : SerpentGlowSize;
        float radius = form == BossForm.Human ? 32 : 60;
        return new[] { radius, radius * 0.9f }.Select(r =>
            GlowArt.MakeCanvas(size, r, PixelColor.Hex(0xFFB347), PixelColor.Hex(0xE8481A),
                               new GlowArt.Tiers { Strong = 80, Medium = 50, Faint = 24 }).ToTexture()).ToArray();
    }

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
