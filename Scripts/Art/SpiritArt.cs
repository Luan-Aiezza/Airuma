using Godot;
using System;
using P = PixelColor;

/// <summary>
/// Pixel art for the common enemies of stage 1: forest spirits, in the spirit of the kodama (little spirits of the trees):
/// a pale, glowing, ghost-like body that floats on a trailing wisp, a hollow face with dark oval eyes and a round mouth, moss
/// and leaves growing on it, and a sprout or antlers of twigs on top. Same fine pixel grid as the boss and the boy. They are
/// side characters, so they stay simple; only the colours change: a blue glow and a sprout for the swarm, a green glow and a
/// pair of leaves for the gunners, a red glow, red leaves and antlers of twigs for the heavy ones. Little motes of light drift
/// round them.
/// </summary>
public static class SpiritArt
{
    public const float Scale = 2;
    /// <summary>Sprite size in art pixels: a 14x18 figure with a one-pixel outline.</summary>
    public const int Width = 16;
    public const int Height = 20;
    public const int FrameCount = 3;

    public struct Palette
    {
        /// <summary>The body: base, lit side, shaded side (tinted by the kind).</summary>
        public PixelColor Body, Light, Dark;
        /// <summary>The motes of light round the spirit.</summary>
        public PixelColor Sprout;
        /// <summary>The leaves and moss: base and shade.</summary>
        public PixelColor Tunic, TunicDark;
        public Palette(PixelColor body, PixelColor light, PixelColor dark, PixelColor sprout, PixelColor tunic, PixelColor tunicDark)
        { Body = body; Light = light; Dark = dark; Sprout = sprout; Tunic = tunic; TunicDark = tunicDark; }
    }

    public static Palette PaletteFor(SpiritKind kind) => kind switch
    {
        SpiritKind.Swarm => new Palette(P.Hex(0xE6F0FF), P.Hex(0xA9D0FF), P.Hex(0x7EA0D8), P.Hex(0x9FD4FF), P.Hex(0x58D0B0), P.Hex(0x2F8F78)),
        SpiritKind.Gunner => new Palette(P.Hex(0xEAF8E0), P.Hex(0xB4F0B0), P.Hex(0x8CC48A), P.Hex(0xE8F59A), P.Hex(0x52C46A), P.Hex(0x2B8645)),
        _ => new Palette(P.Hex(0xFFE9E0), P.Hex(0xFFB0A0), P.Hex(0xD88A80), P.Hex(0xFFC24A), P.Hex(0xE8483C), P.Hex(0xA42A22)),
    };

    private static readonly P Eye = P.Hex(0x1C1228), Glint = P.Hex(0xFFFFFF), Outline = P.Hex(0x1C1228);
    private static readonly P Twig = P.Hex(0x8A5A35), TwigDark = P.Hex(0x5A3A22);

    public static Texture2D[] MakeFrames(SpiritKind kind)
    {
        var result = new Texture2D[FrameCount];
        for (int f = 0; f < FrameCount; f++) result[f] = MakeCanvas(kind, f).ToTexture();
        return result;
    }

    private static readonly (int, int)[] Neighbours = { (1, 0), (-1, 0), (0, 1), (0, -1) };

    /// <summary>What is drawn on the spirit's body: the brush, its palette, and the frame (the face, what it wears, what grows on it).</summary>
    public delegate void Decorator(SpiritBrush brush, Palette palette, int frame);

    /// <summary>Plots on the body of a spirit (14 x 18 art pixels); `Sym` plots a pixel at distance d from the axis and its mirror image.</summary>
    public sealed class SpiritBrush
    {
        public readonly PixelCanvas Body;
        public SpiritBrush(PixelCanvas body) { Body = body; }
        public void Put(int x, int y, P color) { if (Body.Contains(x, y)) Body[x, y] = color; }
        public void Sym(int d, int y, P color) { Put(7 + d, y, color); Put(6 - d, y, color); }
    }

    /// <summary>A forest spirit of the kind, drawn with the forest decorations.</summary>
    public static PixelCanvas MakeCanvas(SpiritKind kind, int frame) => Build(PaletteFor(kind), frame, ForestDecor(kind), null);

    /// <summary>
    /// The shape every spirit has: a round head and a tail of mist that sways, lit from the upper left, with a one-pixel outline and little motes of
    /// light that drift round it. What the spirit wears and its face are drawn by `decorate`; the motes are of `moteColours` (the palette's sprout if null).
    /// </summary>
    public static PixelCanvas Build(Palette p, int frame, Decorator decorate, P[] moteColours)
    {
        var body = new PixelCanvas(Width - 2, Height - 2); // 14x18
        var brush = new SpiritBrush(body);
        // The body: a round head and a tail of mist that sways and fades.
        float[] head = { 2.2f, 3.6f, 4.6f, 5.2f, 5.6f, 5.6f, 5.3f, 4.7f, 3.9f };  // rows 4..12
        float[] tail = { 3.0f, 2.2f, 1.5f, 0.9f, 0.5f };                           // rows 13..17
        float swayAmount = new[] { 0f, 1.2f, -1.2f }[frame % 3];
        for (int y = 4; y <= 17; y++)
        {
            float hw = y <= 12 ? head[y - 4] : tail[y - 13];
            float shift = y > 12 ? swayAmount * (y - 12) / 5f : 0;
            for (int x = 0; x < 14; x++)
            {
                float dx = x + 0.5f - (7 + shift);
                if (MathF.Abs(dx) > hw) continue;
                // lit from the upper left; shade at the lower right; the mist thins out below
                float lit = -dx / hw * 0.6f - (y - 8) * 0.12f;
                P col = lit > 0.35f ? p.Light.Mixed(p.Body, 0.5f) : (lit < -0.45f ? p.Dark : p.Body);
                if (y > 12)
                {
                    // dithered fade into mist
                    float fade = (y - 12) / 6f;
                    if (Bayer.Threshold(x, y) < fade * 0.8f) continue;
                    col = col.Mixed(p.Light, 0.35f);
                }
                body[x, y] = col;
            }
        }

        decorate(brush, p, frame);

        // Motes of light drifting round the spirit.
        (int, int)[][] motes = {
            new[] { (1, 6), (12, 13), (11, 4) },
            new[] { (0, 10), (13, 6), (2, 15) },
            new[] { (2, 3), (12, 10), (1, 14) },
        };
        var moteSet = new System.Collections.Generic.HashSet<P>(moteColours ?? new[] { p.Sprout });
        int moteIndex = 0;
        foreach (var (mx, my) in motes[frame % 3]) { if (body.Contains(mx, my)) body[mx, my] = (moteColours ?? new[] { p.Sprout })[moteIndex++ % (moteColours ?? new[] { p.Sprout }).Length]; }

        // One-pixel outline (a dark one for the solid parts).
        var outC = new PixelCanvas(Width, Height);
        for (int y = 0; y < body.Height; y++)
        {
            for (int x = 0; x < body.Width; x++)
            {
                if (body[x, y].A != 0) continue;
                foreach (var (ox, oy) in Neighbours)
                {
                    if (body.Contains(x + ox, y + oy) && body[x + ox, y + oy].A > 0 && !moteSet.Contains(body[x + ox, y + oy])) { outC[x + 1, y + 1] = Outline; break; }
                }
            }
            for (int x = 0; x < body.Width; x++)
            {
                if (body[x, y].A == 0) continue;
                foreach (var (dx, dy) in Neighbours)
                    if (!body.Contains(x + dx, y + dy)) outC[x + 1 + dx, y + 1 + dy] = Outline;
            }
        }
        for (int y = 0; y < body.Height; y++)
            for (int x = 0; x < body.Width; x++)
                if (body[x, y].A > 0) outC[x + 1, y + 1] = body[x, y];
        return outC;
    }

    /// <summary>The face, the moss and what grows on top, for the forest spirits: a sprout (swarm), a pair of leaves (gunners), antlers of twigs (heavy).</summary>
    public static Decorator ForestDecor(SpiritKind kind) => (brush, p, frame) =>
    {
        void Put(int x, int y, P color) => brush.Put(x, y, color);
        void Sym(int d, int y, P color) => brush.Sym(d, y, color);
        // Hollow face: two dark oval eyes with a glint and a small round mouth.
        for (int y = 7; y <= 9; y++) { Sym(2, y, Eye); Sym(3, y, y == 8 ? Eye : Eye); }
        Sym(2, 7, Eye); Sym(3, 7, Eye.Mixed(p.Body, 0.35f));
        Sym(3, 7, Glint.Mixed(p.Body, 0.1f)); // glint in the outer top corner
        Put(6, 11, Eye); Put(7, 11, Eye); Put(6, 10, Eye.Mixed(p.Body, 0.45f)); Put(7, 10, Eye.Mixed(p.Body, 0.45f));

        // Moss and little leaves growing on the head and shoulders.
        Sym(4, 11, p.Tunic); Sym(3, 12, p.TunicDark); Sym(4, 10, p.Tunic.Mixed(p.TunicDark, 0.4f));
        Sym(5, 8, p.TunicDark);

        // On top: a sprout (swarm), a pair of leaves (gunners), or antlers of twigs and a red leaf (heavy).
        int lean = frame % 3 == 1 ? 1 : (frame % 3 == 2 ? -1 : 0);
        switch (kind)
        {
            case SpiritKind.Swarm:
                Put(6, 3, p.TunicDark); Put(6, 2, p.TunicDark); Put(7, 3, p.TunicDark);
                Put(7 + lean, 1, p.Tunic); Put(8 + lean, 1, p.Tunic); Put(8 + lean, 0, p.Tunic.Mixed(p.Light, 0.3f));
                Put(5 + lean / 2, 1, p.Tunic); Put(4 + lean / 2, 0, p.Tunic);
                break;
            case SpiritKind.Gunner:
                Put(6, 3, p.TunicDark); Put(7, 3, p.TunicDark); Put(6, 2, p.TunicDark);
                for (int i = 0; i < 3; i++) { Put(7 + i + lean, 2 - i / 2 - 0, p.Tunic); Put(5 - i + lean, 2 - i / 2, p.Tunic); }
                Put(9 + lean, 1, p.Tunic.Mixed(p.Light, 0.4f)); Put(3 + lean, 1, p.Tunic.Mixed(p.Light, 0.4f));
                Put(8 + lean, 1, p.TunicDark); Put(4 + lean, 1, p.TunicDark);
                break;
            default:
                // antlers: two twigs forking up and out from the head
                foreach (var (dd, yy) in new[] { (2, 3), (3, 2), (3, 1), (4, 0), (4, 3), (5, 2), (5, 1), (6, 1) })
                    Sym(dd, yy, yy % 2 == 0 ? Twig : TwigDark);
                Sym(1, 4, TwigDark); Sym(1, 3, Twig);
                Put(6, 3, p.TunicDark); Put(7, 3, p.TunicDark);
                Put(6 + lean, 2, p.Tunic); Put(7 + lean, 2, p.Tunic); Put(7 + lean, 1, p.Tunic.Mixed(p.Light, 0.4f));
                break;
        }

    };
}
