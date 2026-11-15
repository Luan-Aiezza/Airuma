using System;
using Godot;
using P = PixelColor;

/// <summary>
/// The faces of the dialogues: `Square(speaker, stage, emotion, frame)` is the picture for the square at the left of the dialogue box. For now the lantern spirit (the flame, see <see cref="FlamePortrait"/>) Atli (<see cref="AtliPortrait"/>, over a gradient of a sunset) and the Iryssh of stage 1 (<see cref="IrysshPortrait"/>, over one of embers) have a picture;
/// the others show the empty frame until their portraits are made again.
/// </summary>
public static class PortraitArt
{
    private const int EmptySize = 32;

    /// <summary>The picture of the speaker showing `emotion` (the flame, 64 x 64; `frame` (0 or 1) is for it, which flickers). Null if there is none yet.</summary>
    public static PixelCanvas Face(DialogueSpeaker speaker, int stage, Emotion emotion, int frame = 0) =>
        speaker == DialogueSpeaker.Airu ? FlamePortrait.Make(emotion, frame)
        : speaker == DialogueSpeaker.Atli ? AtliPortrait.Make(emotion)
        : speaker == DialogueSpeaker.Boss && stage == 1 ? IrysshPortrait.Make(emotion)
        : null;

    /// <summary>The whole square for the box: the face over its backdrop, in a frame (68 x 68 for the flame). Without a face, the empty frame (34 x 34).</summary>
    public static PixelCanvas Square(DialogueSpeaker speaker, int stage, Emotion emotion, int frame)
    {
        var face = Face(speaker, stage, emotion, frame);
        if (face != null && face.Width == 128 && speaker != DialogueSpeaker.Airu)
        {
            // the drawings of Atli and the Iryssh have a clear background: they go over the container's gradient (a sunset for him, embers for her)
            var back = speaker == DialogueSpeaker.Atli ? SunsetBackdrop() : EmberBackdrop();
            for (int y = 0; y < back.Height; y++) for (int x = 0; x < back.Width; x++) if (face[x, y].A > 0) back[x, y] = face[x, y];
            return BigFramed(back);
        }
        if (face != null) return PortraitKit.Framed(face, P.Hex(0x1A1830), P.Hex(0x3A2A40));
        return PortraitKit.Framed(new PixelCanvas(EmptySize, EmptySize), P.Hex(0x1A1830), P.Hex(0x3A2A40));
    }

    /// <summary>The backdrop of the portrait of Atli while it has no picture: a gradient of a sunset, from yellow at the top to orange at the bottom (128 x 128).</summary>
    private static PixelCanvas SunsetBackdrop()
    {
        var c = new PixelCanvas(128, 128);
        var top = P.Hex(0xFFD95C); var bottom = P.Hex(0xF2762C);
        for (int y = 0; y < 128; y++)
        {
            var colour = top.Mixed(bottom, y / 127f);
            for (int x = 0; x < 128; x++) c[x, y] = colour;
        }
        return c;
    }

    /// <summary>The backdrop of the portrait of the Iryssh: a gradient of embers, from deep red at the top to orange at the bottom (128 x 128).</summary>
    private static PixelCanvas EmberBackdrop()
    {
        var c = new PixelCanvas(128, 128);
        var top = P.Hex(0x4A1210); var bottom = P.Hex(0x92401A);
        for (int y = 0; y < 128; y++)
        {
            var colour = top.Mixed(bottom, y / 127f);
            for (int x = 0; x < 128; x++) c[x, y] = colour;
        }
        return c;
    }

    /// <summary>A face of 128 x 128 in a frame of 4 pixels (136 x 136, shown as it is, not stretched): the wood of the other frames, in two tones, with the dark line at both edges.</summary>
    private static PixelCanvas BigFramed(PixelCanvas face)
    {
        const int f = 4;
        var c = new PixelCanvas(face.Width + 2 * f, face.Height + 2 * f);
        var ink = P.Hex(0x120D11); var wood = P.Hex(0x7A4A1E); var light = P.Hex(0xC8923E);
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                int d = Math.Min(Math.Min(x, y), Math.Min(c.Width - 1 - x, c.Height - 1 - y));
                bool topLeft = x + y < c.Width;
                c[x, y] = d == 0 || d == f - 1 ? ink : d == 1 ? (topLeft ? light : wood) : d == 2 ? (topLeft ? wood : light) : ink;
            }
        for (int y = 0; y < face.Height; y++) for (int x = 0; x < face.Width; x++) c[x + f, y + f] = face[x, y];
        return c;
    }

    /// <summary>(For the tools of the headless harness.) The numbers are those of <see cref="Face"/>.</summary>
    public static PixelCanvas Make(int who, int emotion) => Face((DialogueSpeaker)(who % 7), who / 7 + 1, (Emotion)emotion);
}
