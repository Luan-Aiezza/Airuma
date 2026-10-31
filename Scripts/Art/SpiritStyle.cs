using System;
using Godot;

/// <summary>How the common enemies of a stage look: a sprite for each kind of spirit (all of the same size, see <see cref="SpiritArt"/>) and the colour of
/// the sparks they burst into when they are defeated. A stage with new spirits adds a style here.</summary>
public sealed class SpiritStyle
{
    public Func<SpiritKind, int, PixelCanvas> MakeCanvas;
    public Func<SpiritKind, PixelColor> SparkColour;
    /// <summary>The size of the sprites, in art pixels (every kind of the style has the same), and the number of frames of each.</summary>
    public int Width = SpiritArt.Width, Height = SpiritArt.Height, FrameCount = SpiritArt.FrameCount;
    /// <summary>Screen pixels per art pixel.</summary>
    public float Scale = SpiritArt.Scale;
    public Vector2 SpriteSize => new Vector2(Width * Scale, Height * Scale);

    public Texture2D[] MakeFrames(SpiritKind kind)
    {
        var frames = new Texture2D[FrameCount];
        for (int f = 0; f < frames.Length; f++) frames[f] = MakeCanvas(kind, f).ToTexture();
        return frames;
    }

    /// <summary>The spirits of the forest (stage 1).</summary>
    public static readonly SpiritStyle Forest = new SpiritStyle { MakeCanvas = SpiritArt.MakeCanvas, SparkColour = kind => SpiritArt.PaletteFor(kind).Light };

    /// <summary>The spirits of the festival (stage 2).</summary>
    public static readonly SpiritStyle Party = new SpiritStyle
    {
        MakeCanvas = PartySpiritArt.MakeCanvas, SparkColour = PartySpiritArt.SparkColour, Width = PartySpiritArt.Width, Height = PartySpiritArt.Height,
        FrameCount = PartySpiritArt.FrameCount,
    };
}
