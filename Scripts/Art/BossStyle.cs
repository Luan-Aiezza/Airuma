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
}
