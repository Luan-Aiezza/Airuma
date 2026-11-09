using Godot;
using P = PixelColor;

/// <summary>
/// The faces of the dialogues: `Square(speaker, stage, emotion, frame)` is the picture for the square at the left of the dialogue box. For now only the lantern spirit (the flame, see <see cref="FlamePortrait"/>) has a picture;
/// the others show the empty frame until their portraits are made again.
/// </summary>
public static class PortraitArt
{
    private const int EmptySize = 32;

    /// <summary>The picture of the speaker showing `emotion` (the flame, 64 x 64; `frame` (0 or 1) is for it, which flickers). Null if there is none yet.</summary>
    public static PixelCanvas Face(DialogueSpeaker speaker, int stage, Emotion emotion, int frame = 0) =>
        speaker == DialogueSpeaker.Airu ? FlamePortrait.Make(emotion, frame) : null;

    /// <summary>The whole square for the box: the face over its backdrop, in a frame (68 x 68 for the flame). Without a face, the empty frame (34 x 34).</summary>
    public static PixelCanvas Square(DialogueSpeaker speaker, int stage, Emotion emotion, int frame)
    {
        var face = Face(speaker, stage, emotion, frame);
        if (face != null) return PortraitKit.Framed(face, P.Hex(0x1A1830), P.Hex(0x3A2A40));
        return PortraitKit.Framed(new PixelCanvas(EmptySize, EmptySize), P.Hex(0x1A1830), P.Hex(0x3A2A40));
    }

    /// <summary>(For the tools of the headless harness.) The numbers are those of <see cref="Face"/>.</summary>
    public static PixelCanvas Make(int who, int emotion) => Face((DialogueSpeaker)(who % 7), who / 7 + 1, (Emotion)emotion);
}
