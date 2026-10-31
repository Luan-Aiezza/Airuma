using System;
using System.Collections.Generic;
using Godot;
using P = PixelColor;

/// <summary>
/// The common enemies of stage 2, the river of the parties: spirits of the festival, small humanlike figures in the way of the kokiri (a child's
/// body in a tunic and a pointed cap, hands and boots), but faceless: the head is smooth and blank, in the shade of the cap, and the only life is in
/// the clothes. A serious festival, in deep colours with gold trim, and a few specks of muted confetti in the air round them. They are ghosts: they
/// have no legs, the tunic goes on into the winding tail of a ghost that fades into mist, and they float and sway a little.
///   Swarm  the reveller: a teal tunic with a gold belt, a pointed cap with a gold bell, empty hands;
///   Gunner the herald: an indigo tunic with a high collar, a cap with a red feather, a brass horn that it raises and lowers;
///   Heavy  the drummer (for later stages): an ochre tunic, a great drum at the waist, a crest of plumes.
/// Each is 18 x 24 art pixels (36 x 48 on the screen), a little bigger than the spirits of the forest, to have room for the arms and the tail.
/// </summary>
public static class PartySpiritArt
{
    public const int Width = 18, Height = 24;

    private static readonly P Outline = P.Hex(0x14091E);
    private static readonly P[] Skin = { P.Hex(0xF0E4EE), P.Hex(0xD8C6DC), P.Hex(0xA896B4) };          // pale and blank: lit, base, in the shade
    private static readonly P[] Gold = { P.Hex(0xFFE9A0), P.Hex(0xE0A93A), P.Hex(0x8A5E1E) };
    private static readonly P[] Confetti = { P.Hex(0xB8487A), P.Hex(0x3AA8B0), P.Hex(0xC8982E), P.Hex(0x7A9A48) };

    private struct Dress
    {
        public P[] Tunic, Cap, Trim;
        public P Spark;
    }

    private static Dress DressFor(SpiritKind kind) => kind switch
    {
        SpiritKind.Swarm => new Dress { Tunic = Ramp(0x2B9C94, 0x1D6F6C, 0x124442), Cap = Ramp(0x8E2F5E, 0x651F44, 0x3E1229), Trim = Gold, Spark = P.Hex(0xE0A93A) },
        SpiritKind.Gunner => new Dress { Tunic = Ramp(0x4A55B8, 0x323C8A, 0x1E2458), Cap = Ramp(0x3A2E6A, 0x261E4A, 0x150F2E), Trim = Gold, Spark = P.Hex(0x7A8AF0) },
        _ => new Dress { Tunic = Ramp(0xC8842E, 0x985F1E, 0x5E3A10), Cap = Ramp(0xA8322E, 0x7A2220, 0x4A1212), Trim = Gold, Spark = P.Hex(0xE8C24A) },
    };

    private static P[] Ramp(uint light, uint base_, uint dark) => new[] { P.Hex(light), P.Hex(base_), P.Hex(dark) };

    /// <summary>The colour of the sparks when a spirit is defeated.</summary>
    public static P SparkColour(SpiritKind kind) => DressFor(kind).Spark;

    public const int FrameCount = 3;

    public static PixelCanvas MakeCanvas(SpiritKind kind, int frame)
    {
        var d = DressFor(kind);
        float sway = new[] { 0f, 1f, -1f }[frame % 3];          // the body leans, the arms swing the other way
        int bob = frame % 3 == 1 ? 1 : 0;

        var back = new PixelCanvas(Width, Height);       // the arm behind, the horn
        var body = new PixelCanvas(Width, Height);       // legs, tunic, head, cap
        var front = new PixelCanvas(Width, Height);      // the arm in front, the drum

        // The body: a tunic from the shoulders that goes on into the tail of a ghost, which narrows, winds from side to side and fades into mist.
        for (int y = 12; y <= 23; y++)
        {
            float half = y <= 15 ? 3.0f + (y - 12) * 0.4f : 4.2f * MathF.Pow(1 - (y - 15) / 9.5f, 0.8f) + 0.4f;
            float centre = 8.5f + sway * 0.4f + (y > 15 ? sway * (y - 15) * 0.3f + MathF.Sin(y * 0.7f + frame * 2.1f) * (y - 15) * 0.22f : 0);
            for (int x = 0; x < Width; x++)
            {
                float dx = x + 0.5f - centre;
                if (MathF.Abs(dx) > half) continue;
                if (y > 20 && Bayer.Threshold(x, y) < (y - 20) / 4f * 0.8f) continue;       // the tail fades into mist
                float lit = dx / half;                                                          // -1 (lit side) .. 1 (shaded side)
                float f = (lit + 1) * 1.5f + (Bayer.Threshold(x, y) - 0.5f) * 0.7f;
                body[x, y] = f < 0.8f ? d.Tunic[0] : (f < 2.2f ? d.Tunic[1] : d.Tunic[2]);
            }
        }
        Rect(body, 5.7f + sway * 0.4f, 15.2f, 11.4f + sway * 0.4f, 16.2f, d.Trim[1]); Put(body, 8, 15, d.Trim[0]);   // the belt
        Rect(body, 6.6f, 11.8f, 11.4f, 12.7f, d.Trim[1]);                           // the collar
        // The head: blank, smooth, with the shade of the cap over it.
        Ellipse(body, 8.5f + sway * 0.4f, 8.6f, 3.2f, 3.4f, Skin);
        for (int x = 5; x <= 12; x++) { Put(body, x + (int)(sway * 0.4f), 9, Skin[2]); if (x > 6 && x < 11) Put(body, x + (int)(sway * 0.4f), 10, Skin[2].Mixed(Skin[1], 0.5f)); }   // the shade under the cap
        // The cap: a cone down to the brow, whose tip droops to one side and carries a bell (or a feather, or plumes).
        Poly(body, new[] { V(4.8f + sway * 0.4f, 8.8f), V(12.2f + sway * 0.4f, 8.8f), V(10.8f + sway, 4.4f), V(8.5f + sway * 1.5f, 0.6f), V(6.4f + sway * 0.6f, 4.4f) }, d.Cap);
        Rect(body, 5.2f + sway * 0.4f, 8f, 11.8f + sway * 0.4f, 8.9f, d.Trim[2]);         // the band of the cap

        // The arms: sleeves of the tunic, pale hands. The far arm hangs; the near one swings.
        DrawArm(back, 4.2f + sway * 0.3f, 12.6f, -1, sway * 0.8f, d);
        DrawArm(front, 12.8f + sway * 0.3f, 12.6f, 1, -sway * 0.8f, d);

        Decorate(kind, d, frame, back, body, front, sway);

        // Confetti in the air: a few specks, muted.
        var air = new PixelCanvas(Width, Height);
        (int, int)[][] specks = { new[] { (1, 4), (16, 9), (2, 15) }, new[] { (0, 9), (16, 4), (15, 16) }, new[] { (2, 2), (17, 12), (1, 18) } };
        int n = 0;
        foreach (var (x, y) in specks[frame % 3]) Put(air, x, y, Confetti[n++ % Confetti.Length]);

        var fig = new PixelCanvas(Width, Height);
        void Add(PixelCanvas part)
        {
            (int, int)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    if (part[x, y].A != 0) continue;
                    foreach (var (dx, dy) in dirs)
                        if (part.Contains(x + dx, y + dy) && part[x + dx, y + dy].A > 0) { fig[x, y] = Outline; break; }
                }
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) if (part[x, y].A > 0) fig[x, y] = part[x, y];
        }
        Add(back); Add(body); Add(front);

        var c = new PixelCanvas(Width, Height);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                if (air[x, y].A > 0) c[x, y] = air[x, y];
                if (fig[x, y].A > 0 && y + bob < Height) c[x, y + bob] = fig[x, y];
            }
        return c;
    }

    /// <summary>What sets each kind apart: the bell on the cap, the feather and the horn, the drum and the plumes.</summary>
    private static void Decorate(SpiritKind kind, Dress d, int frame, PixelCanvas back, PixelCanvas body, PixelCanvas front, float sway)
    {
        int tip = (int)Math.Round(8.5f + sway * 1.5f);
        switch (kind)
        {
            case SpiritKind.Swarm:
                // a gold bell at the tip of the cap
                Put(body, tip, 0, Gold[0]); Put(body, tip + 1, 1, Gold[1]); Put(body, tip, 1, Gold[2]);
                break;
            case SpiritKind.Gunner:
                // a red feather in the cap, and a brass horn in the near hand, raised and lowered with the frames
                for (int i = 0; i < 4; i++) Put(body, tip + 1 + i / 2, 2 - i, i % 2 == 0 ? P.Hex(0xE23C34) : P.Hex(0x8E1F2A));
                int lift = frame % 3 == 1 ? 2 : (frame % 3 == 2 ? 1 : 0);
                for (int i = 0; i < 4; i++) Put(front, 13 + i, 17 - lift - i / 2, Gold[i % 2]);
                Put(front, 17 - 1, 17 - lift - 3, Gold[0]); Put(front, 16, 17 - lift - 4, Gold[0]); Put(front, 17, 17 - lift - 4, Gold[1]);
                break;
            default:
                // a crest of plumes, and a great drum at the waist, red with a gold rim
                P[] plumes = { P.Hex(0xB8487A), P.Hex(0x3AA8B0), Gold[1], P.Hex(0x7A9A48), P.Hex(0xA8322E) };
                for (int i = -2; i <= 2; i++) for (int k = 0; k < 3; k++) Put(body, tip + i + (k > 0 ? i / 2 : 0), 1 - k + (k == 2 ? 1 : 0), plumes[i + 2]);
                Ellipse(front, 8.5f, 17f, 4.4f, 2.8f, d.Cap);
                for (int x = 4; x <= 13; x++) { Put(front, x, 15 + (x == 4 || x == 13 ? 1 : 0), Gold[1]); Put(front, x, 19 - (x == 4 || x == 13 ? 1 : 0), Gold[1]); }
                break;
        }
    }

    private static void DrawArm(PixelCanvas layer, float shoulderX, float shoulderY, int side, float swing, Dress d)
    {
        float handX = shoulderX + side * 0.8f + swing * 0.7f, handY = shoulderY + 5;
        Limb(layer, shoulderX, shoulderY, handX, handY, d.Tunic);
        Rect(layer, handX - 0.6f, handY - 0.2f, handX + 1f, handY + 1.4f, Skin[1]);
    }

    // MARK: Shapes

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static void Put(PixelCanvas c, int x, int y, P colour) { if (c.Contains(x, y)) c[x, y] = colour; }

    private static void Rect(PixelCanvas c, float x0, float y0, float x1, float y1, P colour)
    {
        for (int y = (int)Math.Floor(y0); y < Math.Ceiling(y1); y++) for (int x = (int)Math.Floor(x0); x < Math.Ceiling(x1); x++) Put(c, x, y, colour);
    }

    /// <summary>A polygon filled in three tones, lit from the left.</summary>
    private static void Poly(PixelCanvas c, Vector2[] pts, P[] ramp)
    {
        var layer = new PixelCanvas(c.Width, c.Height);
        PixelPaint.Poly(layer, pts, ramp[1]);
        Shade(c, layer, ramp);
    }

    private static void Ellipse(PixelCanvas c, float cx, float cy, float rx, float ry, P[] ramp)
    {
        var layer = new PixelCanvas(c.Width, c.Height);
        PixelPaint.Ellipse(layer, cx, cy, rx, ry, ramp[1]);
        Shade(c, layer, ramp);
    }

    private static void Shade(PixelCanvas c, PixelCanvas layer, P[] ramp)
    {
        int minX = c.Width, maxX = 0;
        for (int y = 0; y < c.Height; y++) for (int x = 0; x < c.Width; x++) if (layer[x, y].A > 0) { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); }
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                if (layer[x, y].A == 0) continue;
                float t = (x + 0.5f - minX) / Math.Max(1, maxX - minX + 1);
                float f = t * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f;
                c[x, y] = f < 0.7f ? ramp[0] : (f < 2.1f ? ramp[1] : ramp[2]);
            }
    }

    /// <summary>A sleeve: a thick line from the shoulder to the hand.</summary>
    private static void Limb(PixelCanvas c, float x0, float y0, float x1, float y1, P[] ramp)
    {
        for (int i = 0; i <= 8; i++)
        {
            float t = i / 8f;
            PixelPaint.Ellipse(c, x0 + (x1 - x0) * t, y0 + (y1 - y0) * t, 1.1f, 1.1f, ramp[i % 3 == 0 ? 0 : 1]);
        }
        Put(c, (int)x1, (int)y1 - 1, ramp[2]);
    }
}
