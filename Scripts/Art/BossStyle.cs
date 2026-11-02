using System;
using Godot;

/// <summary>
/// How a boss looks, as data: the frames of its sprite, its size and the pixel that is its position, where the water line is (for the
/// reflection and the rings), its glow, whether it spins when it moves, and the colours of its rage. A new boss is a new style (see
/// <see cref="BossStyles"/>); the boss view (<see cref="BossView"/>) draws whichever it is given.
/// </summary>
public sealed class BossStyle
{
    public const int FrameCount = 4;

    /// <summary>Draws a frame (0...FrameCount-1) of the sprite.</summary>
    public Func<int, PixelCanvas> MakeFrame;
    public int Width, Height;
    /// <summary>The art pixel that is the boss's position (the middle of its body).</summary>
    public (int x, int y) Centre;
    /// <summary>Screen pixels per art pixel.</summary>
    public float Scale = 2;
    /// <summary>Row of the water surface in the art, where the reflection is mirrored about.</summary>
    public int WaterRow;
    /// <summary>The glow behind the sprite: the canvas size, the radius, and the two colours.</summary>
    public int GlowSize = 72;
    public float GlowRadius = 32;
    public uint GlowInner = 0xFFB347, GlowOuter = 0xE8481A;
    /// <summary>Spins on the spot while it roams (with the effect and the sound of a spin).</summary>
    public bool Spins;
    /// <summary>Whether the water mirrors the boss (not if she stands on rock).</summary>
    public bool Reflects = true;
    /// <summary>The style of the second body, if the boss is two persons who share their life (see <see cref="BossEntity"/> Twin): each is drawn by its own style.</summary>
    public BossStyle Twin;
    /// <summary>The style of the boss when its two bodies have fused into one (the phases marked <see cref="BossPhase.Fused"/>).</summary>
    public BossStyle Fused;
    /// <summary>The rage: the colours of its glow, sparks and rings, and the tint the river takes.</summary>
    public Color RageMain = new Color(1f, 0.45f, 0.15f), RageAccent = new Color(1f, 0.85f, 0.4f), SceneTint = new Color(1f, 0.74f, 0.68f);

    public Vector2 SpriteSize => new Vector2(Width * Scale, Height * Scale);
    /// <summary>Where the boss's position sits in the sprite, as a fraction of its size from the top-left.</summary>
    public Vector2 Anchor => new Vector2((float)Centre.x / Width, (float)Centre.y / Height);
    /// <summary>How far below the boss's position (screen px) the water surface is.</summary>
    public float Waterline => (WaterRow - Centre.y) * Scale;

    public ImageTexture[] MakeFrames()
    {
        var frames = new ImageTexture[FrameCount];
        for (int i = 0; i < FrameCount; i++) frames[i] = MakeFrame(i).ToTexture();
        return frames;
    }

    public ImageTexture[] MakeGlowFrames() =>
        Array.ConvertAll(new[] { GlowRadius, GlowRadius * 0.9f }, r => GlowArt.MakeCanvas(GlowSize, r, PixelColor.Hex(GlowInner), PixelColor.Hex(GlowOuter),
                                                                      new GlowArt.Tiers { Strong = 80, Medium = 50, Faint = 24 }).ToTexture());
}

/// <summary>The styles of the bosses.</summary>
public static class BossStyles
{
    /// <summary>The Iryssh: a tall woman of fire (48 x 61 px).</summary>
    public static readonly BossStyle Iryssh = new BossStyle
    {
        MakeFrame = IrysshWomanArt.MakeCanvas, Width = IrysshWomanArt.Width, Height = IrysshWomanArt.Height,
        Centre = IrysshWomanArt.Centre, WaterRow = IrysshWomanArt.WaterRow,
    };

    /// <summary>Baylo: a dancer in a white suit and hat, with a pink glow and a hot pink rage; he spins when he moves.</summary>
    public static readonly BossStyle Baylo = new BossStyle
    {
        MakeFrame = BayloArt.MakeCanvas, Width = BayloArt.Width, Height = BayloArt.Height, Centre = BayloArt.Centre, WaterRow = BayloArt.WaterRow,
        GlowInner = 0xFFB0DC, GlowOuter = 0xE0508A, Spins = true,
        RageMain = new Color(1f, 0.35f, 0.7f), RageAccent = new Color(1f, 0.9f, 0.5f), SceneTint = new Color(1f, 0.72f, 0.9f),
    };

    /// <summary>Thaqai: a girl with the ears and the tail of a dark grey cat with bolts of white; a cold glow, a white and blue rage and a scenery that turns grey and blue.</summary>
    public static readonly BossStyle Thaqai = new BossStyle
    {
        MakeFrame = ThaqaiArt.MakeCanvas, Width = ThaqaiArt.Width, Height = ThaqaiArt.Height, Centre = ThaqaiArt.Centre, WaterRow = ThaqaiArt.WaterRow,
        GlowSize = 80,
        GlowInner = 0xCFE4FF, GlowOuter = 0x6A8AD8,
        RageMain = new Color(0.55f, 0.7f, 1f), RageAccent = new Color(1f, 1f, 1f), SceneTint = new Color(0.78f, 0.85f, 1f),
    };

    /// <summary>Ukhumo: a bear girl in the fur of a bear, who stands on the rock of her platform (no reflection); a cold glow, a brown and cream rage, a scenery that turns brown.</summary>
    public static readonly BossStyle Ukhumo = new BossStyle
    {
        MakeFrame = UkhumoArt.MakeCanvas, Width = UkhumoArt.Width, Height = UkhumoArt.Height, Centre = UkhumoArt.Centre, WaterRow = UkhumoArt.WaterRow,
        Reflects = false,
        GlowInner = 0xE6F6FF, GlowOuter = 0x5A8FC8,
        RageMain = new Color(0.65f, 0.5f, 0.3f), RageAccent = new Color(1f, 0.95f, 0.8f), SceneTint = new Color(0.9f, 0.86f, 0.8f),
    };

    /// <summary>Mama Killa, the Moon (stage 6), for now in the borrowed art of the divine person of stage 5, smaller than when she is fused (so that her head is clear of the boss bar) and in the silver of the moon.</summary>
    public static readonly BossStyle Killa = new BossStyle
    {
        MakeFrame = KuychuArt.MakeFused, Width = KuychuArt.Width, Height = KuychuArt.Height, Centre = (KuychuArt.Centre.x, 55), WaterRow = KuychuArt.WaterRow, Scale = 1.6f,
        GlowSize = 100, GlowRadius = 48,
        GlowInner = 0xF4F8FF, GlowOuter = 0x9AB0E8, Spins = false,
        RageMain = new Color(0.85f, 0.9f, 1f), RageAccent = new Color(1f, 1f, 0.9f), SceneTint = new Color(0.85f, 0.9f, 1f),
    };

    /// <summary>Kuychu, the first of the two (the sun's, on the left): a divine person in a long gown, 60 x 80 px, with a glow of gold; the rage of the pair is the colours of the rainbow.</summary>
    public static readonly BossStyle Kuychu = new BossStyle
    {
        MakeFrame = KuychuArt.MakeLeft, Width = KuychuArt.Width, Height = KuychuArt.Height, Centre = KuychuArt.Centre, WaterRow = KuychuArt.WaterRow,
        GlowSize = 100, GlowRadius = 44,
        GlowInner = 0xFFF4C0, GlowOuter = 0xE8A838, Spins = false,
        RageMain = new Color(1f, 0.8f, 0.4f), RageAccent = new Color(1f, 0.95f, 0.8f), SceneTint = new Color(0.9f, 0.92f, 1f),
        Twin = new BossStyle
        {
            MakeFrame = KuychuArt.MakeRight, Width = KuychuArt.Width, Height = KuychuArt.Height, Centre = KuychuArt.Centre, WaterRow = KuychuArt.WaterRow,
            GlowSize = 100, GlowRadius = 44,
            GlowInner = 0xDDF8FF, GlowOuter = 0x4FA8E8, Spins = false,
            RageMain = new Color(0.55f, 0.85f, 1f), RageAccent = new Color(1f, 0.95f, 0.8f), SceneTint = new Color(0.9f, 0.92f, 1f),
        },
        Fused = new BossStyle
        {
            // one body, taller and bigger: the left half of the one and the right half of the other (the feet stay on the horizon)
            MakeFrame = KuychuArt.MakeFused, Width = KuychuArt.Width, Height = KuychuArt.Height, Centre = (KuychuArt.Centre.x, 55), WaterRow = KuychuArt.WaterRow, Scale = 2.5f,
            GlowSize = 100, GlowRadius = 48,
            GlowInner = 0xFFF4D8, GlowOuter = 0x7AA8E8, Spins = false,
            RageMain = new Color(0.8f, 0.85f, 1f), RageAccent = new Color(1f, 0.95f, 0.8f), SceneTint = new Color(0.9f, 0.92f, 1f),
        },
    };
}
