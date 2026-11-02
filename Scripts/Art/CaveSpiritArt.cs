using System;
using System.Collections.Generic;
using Godot;
using P = PixelColor;

/// <summary>
/// The common enemies of stage 4, the river of the cave: creatures of the dark, each drawn on a 22 x 24 grid (44 x 48 on the screen):
///   Swarm  the bat: a small furry body, pointed ears, red eyes and two little fangs, and wide wings of violet membrane on fine bones, that beat (up, level, down);
///   Gunner the ghost: a pale, translucent sheet with a round head and a creepy face (tall sunken eyes with a pin of light, a mouth stretched wide and jagged with teeth), a hem that waves, and a hand that holds a cold light;
///   Heavy  the imp: a small red devil with horns, pointed ears, yellow slit eyes, a fanged grin, little leathery wings, clawed hands round a flame and a tail that ends in an arrow.
/// </summary>
public static class CaveSpiritArt
{
    public const int Width = 22, Height = 24, FrameCount = 3;

    private static readonly P Outline = P.Hex(0x120A1C);

    /// <summary>The colour of the sparks when a spirit is defeated.</summary>
    public static P SparkColour(SpiritKind kind) => kind switch
    {
        SpiritKind.Swarm => P.Hex(0xB48AE0),
        SpiritKind.Gunner => P.Hex(0x9AE8F0),
        _ => P.Hex(0xFF8A3A),
    };

    public static PixelCanvas MakeCanvas(SpiritKind kind, int frame)
    {
        var back = new PixelCanvas(Width, Height);       // wings, the tail, what is behind
        var body = new PixelCanvas(Width, Height);
        var front = new PixelCanvas(Width, Height);      // hands, the light
        var glow = new PixelCanvas(Width, Height);       // not outlined, under the figure
        var top = new PixelCanvas(Width, Height);        // not outlined, over the figure (a face)
        frame %= FrameCount;
        switch (kind)
        {
            case SpiritKind.Swarm: Bat(back, body, front, glow, top, frame); break;
            case SpiritKind.Gunner: Ghost(back, body, front, glow, top, frame); break;
            default: Imp(back, body, front, glow, top, frame); break;
        }
        var fig = new PixelCanvas(Width, Height);
        foreach (var part in new[] { back, body, front }) { PixelPaint.Outline(part, Outline); PixelPaint.Composite(fig, part); }
        var c = new PixelCanvas(Width, Height);
        PixelPaint.Composite(c, glow);
        PixelPaint.Composite(c, fig);
        PixelPaint.Composite(c, top);
        return c;
    }

    // MARK: The bat

    private static void Bat(PixelCanvas back, PixelCanvas body, PixelCanvas front, PixelCanvas glow, PixelCanvas top, int frame)
    {
        P[] membrane = { P.Hex(0x7C58A8), P.Hex(0x573A80), P.Hex(0x35214F) };
        P bone = P.Hex(0xA886D0);
        P[] fur = { P.Hex(0x8A74A4), P.Hex(0x64507C), P.Hex(0x40305A) };
        float cx = 10.5f;
        // Wing beat: the tips up, level, down; the body bobs a pixel.
        float tipY = new[] { 3.5f, 9.5f, 16f }[frame];
        float bob = frame == 2 ? 1 : 0;
        foreach (int side in new[] { -1, 1 })
        {
            float s = side;
            var wing = new[]
            {
                V(cx + s * 2f, 10 + bob), V(cx + s * 6f, tipY + 3 + (frame == 0 ? 0 : 0)), V(cx + s * 10f, tipY),
                V(cx + s * 9.2f, tipY + 3.6f), V(cx + s * 7.4f, tipY + 2.6f + 1.2f), V(cx + s * 6.2f, tipY + 6f),
                V(cx + s * 4.4f, tipY + 4.4f + 1.5f), V(cx + s * 2.6f, 15 + bob),
            };
            Poly(back, wing, membrane, side);
            // the bones: from the shoulder to each tip
            foreach (var tip in new[] { wing[2], wing[3], wing[5] }) Line(back, V(cx + s * 2f, 10.5f + bob), tip, bone);
        }
        // The body: a furry ellipse, a head, ears, eyes, fangs.
        Ellipse(body, cx, 13 + bob, 2.8f, 3.8f, fur);
        Ellipse(body, cx, 8 + bob, 2.9f, 2.6f, fur);
        Poly(body, new[] { V(cx - 3f, 7.2f + bob), V(cx - 3.4f, 3f + bob), V(cx - 1f, 6f + bob) }, fur, 1);
        Poly(body, new[] { V(cx + 3f, 7.2f + bob), V(cx + 3.4f, 3f + bob), V(cx + 1f, 6f + bob) }, fur, -1);
        Put(body, (int)cx - 3, 5 + (int)bob, P.Hex(0xE090A8)); Put(body, (int)cx + 2, 5 + (int)bob, P.Hex(0xE090A8));
        foreach (int dx in new[] { -2, 1 }) { Put(front, (int)cx + dx, 8 + (int)bob, P.Hex(0xFF4A3A)); Put(front, (int)cx + dx, 7 + (int)bob, P.Hex(0x2A1E34)); }
        Put(front, (int)cx - 1, 10 + (int)bob, P.Hex(0x2A1E34)); Put(front, (int)cx, 10 + (int)bob, P.Hex(0x2A1E34));
        Put(front, (int)cx - 1, 11 + (int)bob, P.Hex(0xFFFFFF)); Put(front, (int)cx, 11 + (int)bob, P.Hex(0xFFFFFF));
        // tiny feet at the bottom, clawing
        Put(body, (int)cx - 1, 17 + (int)bob, fur[2]); Put(body, (int)cx, 17 + (int)bob, fur[2]);
    }

    // MARK: The ghost

    private static void Ghost(PixelCanvas back, PixelCanvas body, PixelCanvas front, PixelCanvas glow, PixelCanvas top, int frame)
    {
        P[] sheet = { P.Hex(0xE6F6FA), P.Hex(0xBBD8E4), P.Hex(0x7F9EB8) };
        float cx = 10.5f, sway = new[] { 0f, 1f, -1f }[frame];
        for (int y = 3; y <= 22; y++)
        {
            float half = y <= 9 ? 2.5f + (y - 3) * 0.55f : 5.6f + (y - 9) * 0.12f;
            float centre = cx + (y > 10 ? sway * (y - 10) * 0.18f + MathF.Sin(y * 0.6f + frame * 2.1f) * 0.6f : 0);
            // the hem: scalloped, and it waves with the frames
            int bottom = 21 + (int)MathF.Round(MathF.Sin(frame * 2.1f));
            for (int x = 0; x < Width; x++)
            {
                float dx = x + 0.5f - centre;
                if (MathF.Abs(dx) > half) continue;
                if (y > 18 && (int)MathF.Floor((x + frame * 2) / 2.5f) % 2 == 0 && y > bottom - 1 - (int)(MathF.Abs(dx) * 0.2f)) continue;
                if (y > 20 && Bayer.Threshold(x, y) < 0.45f) continue;                                    // the hem fades into mist
                float f = (dx / half + 1) * 1.5f + (Bayer.Threshold(x, y) - 0.5f) * 0.7f;
                body[x, y] = f < 0.8f ? sheet[0] : (f < 2.2f ? sheet[1] : sheet[2]);
            }
        }
        // The face, creepy: sunken eyes, tall and slanting, black, with a pin of cold light for pupils and dark shadows under them; and a mouth stretched too wide,
        // jagged with teeth, that gapes wider on one frame. (Drawn over the outline, so that nothing of it is lost.)
        P hole = P.Hex(0x080C1C), shadow = P.Hex(0x5C7896), pin = P.Hex(0x9AF4FF);
        foreach (int dx in new[] { -3, 1 })
        {
            int ex = (int)cx + dx;
            Rect(top, ex, 6, 2, 4, hole);                    // a tall, narrow socket
            Put(top, ex + (dx < 0 ? -1 : 2), 7, hole);       // slanting out to the sides (angry)
            Put(top, ex + (dx < 0 ? 0 : 1), 5, hole);
            Put(top, ex + (dx < 0 ? 1 : 0), 8, pin);         // the pupil: a pin of light, low in the socket
            Put(top, ex, 10, shadow); Put(top, ex + 1, 10, shadow);
        }
        int open = frame == 1 ? 4 : 3;
        Rect(top, (int)cx - 3, 11, 6, open, hole);           // the mouth, wide
        Put(top, (int)cx - 4, 11, hole); Put(top, (int)cx + 3, 11, hole);
        Put(top, (int)cx - 4, 10, hole); Put(top, (int)cx + 3, 10, hole);                   // the corners pulled up, in a grin
        for (int i = 0; i < 6; i += 2) { Put(top, (int)cx - 3 + i, 11, P.Hex(0xE8F4F8)); Put(top, (int)cx - 2 + i, 11 + open - 1, P.Hex(0xE8F4F8)); }   // jagged teeth
        // The arms: long, thin stubs that reach, the near one holding a cold light that bobs.
        float lift = new[] { 0f, 1f, 0f }[frame];
        Rect(front, 4, 13, 2, 3, sheet[1]); Rect(front, 2, 14, 2, 2, sheet[2]);
        Rect(front, 15, 13, 2, 3, sheet[1]); Rect(front, 17, 12 - (int)lift, 2, 2, sheet[0]);
        Ellipse(glow, 19.5f, 10 - lift, 2.2f, 2.2f, new[] { P.Hex(0xB8F4FF, 90), P.Hex(0xB8F4FF, 90), P.Hex(0x7AD0E8, 60) });
        Put(front, 19, 10 - (int)lift, P.Hex(0xFFFFFF)); Put(front, 20, 10 - (int)lift, P.Hex(0xB8F4FF)); Put(front, 19, 11 - (int)lift, P.Hex(0xB8F4FF));
    }

    // MARK: The imp

    private static void Imp(PixelCanvas back, PixelCanvas body, PixelCanvas front, PixelCanvas glow, PixelCanvas top, int frame)
    {
        P[] skin = { P.Hex(0xEA5A42), P.Hex(0xB93A30), P.Hex(0x7A2024) };
        P[] wing = { P.Hex(0x8A2A38), P.Hex(0x621A2A), P.Hex(0x3E1020) };
        P horn = P.Hex(0xE8DCC0), hornDark = P.Hex(0xA89878), claw = P.Hex(0xFFF0D0);
        float cx = 10.5f;
        float flap = new[] { -2f, 0f, 2.5f }[frame], bob = frame == 2 ? 1 : 0;
        // The little wings, behind the shoulders, beating.
        foreach (int side in new[] { -1, 1 })
        {
            float s = side;
            Poly(back, new[] { V(cx + s * 2f, 11 + bob), V(cx + s * 7.5f, 6 + flap * 1.3f), V(cx + s * 9.5f, 8 + flap), V(cx + s * 8f, 9.5f + flap * 0.7f), V(cx + s * 6.5f, 10.5f + flap * 0.5f), V(cx + s * 4.8f, 13f + bob) }, wing, side);
            Line(back, V(cx + s * 2.5f, 11 + bob), V(cx + s * 7.5f, 6 + flap * 1.3f), wing[0]);
        }
        // The tail: it comes out behind and curls up, ending in an arrow (drawn at the left).
        Line(back, V(cx - 1f, 18 + bob), V(cx - 4f, 20.5f), skin[1]); Line(back, V(cx - 4f, 20.5f), V(cx - 6.5f, 19.5f), skin[1]); Line(back, V(cx - 6.5f, 19.5f), V(cx - 7.5f, 17f + flap * 0.3f), skin[1]);
        Poly(back, new[] { V(cx - 7.5f, 14.8f + flap * 0.3f), V(cx - 9f, 17.5f + flap * 0.3f), V(cx - 6f, 17.5f + flap * 0.3f) }, skin, 1);
        // The body: a round belly, thin legs, the head with horns and ears.
        Ellipse(body, cx, 15.5f + bob, 3.4f, 3.6f, skin);
        Rect(body, (int)cx - 3, 19 + (int)bob, 2, 3, skin[1]); Rect(body, (int)cx + 1, 19 + (int)bob, 2, 3, skin[1]);
        Rect(body, (int)cx - 4, 22, 3, 1, skin[2]); Rect(body, (int)cx + 1, 22, 3, 1, skin[2]);
        Ellipse(body, cx, 9 + bob, 3.8f, 3.3f, skin);
        Poly(body, new[] { V(cx - 3.2f, 6.5f + bob), V(cx - 5.2f, 2f + bob), V(cx - 1.6f, 5.4f + bob) }, new[] { horn, horn, hornDark }, 1);
        Poly(body, new[] { V(cx + 3.2f, 6.5f + bob), V(cx + 5.2f, 2f + bob), V(cx + 1.6f, 5.4f + bob) }, new[] { horn, horn, hornDark }, -1);
        Poly(body, new[] { V(cx - 3.6f, 9f + bob), V(cx - 7f, 7f + bob), V(cx - 3.2f, 7.4f + bob) }, skin, 1);
        Poly(body, new[] { V(cx + 3.6f, 9f + bob), V(cx + 7f, 7f + bob), V(cx + 3.2f, 7.4f + bob) }, skin, -1);
        // The face: yellow slit eyes under angry brows, a wide grin with fangs.
        P brow = P.Hex(0x3A1018), eye = P.Hex(0xFFE04A);
        foreach (int dx in new[] { -3, 1 }) { Put(front, (int)cx + dx, 9 + (int)bob, eye); Put(front, (int)cx + dx + 1, 9 + (int)bob, eye); Put(front, (int)cx + dx, 8 + (int)bob, brow); Put(front, (int)cx + dx + 1, 7 + (int)bob, brow); }
        Rect(front, (int)cx - 2, 11 + (int)bob, 4, 1, P.Hex(0x2A0A12));
        Put(front, (int)cx - 2, 12 + (int)bob, claw); Put(front, (int)cx + 1, 12 + (int)bob, claw);
        // The arms: thin, clawed, the near one cupping a little flame that flickers.
        Line(front, V(cx - 3.2f, 13.5f + bob), V(cx - 6f, 16.5f), skin[1]); Put(front, (int)(cx - 7f), 17, claw); Put(front, (int)(cx - 6f), 17, claw);
        Line(front, V(cx + 3.2f, 13.5f + bob), V(cx + 5.5f, 16.5f), skin[1]);
        int flicker = frame % 3;
        Put(front, (int)cx + 6, 16, P.Hex(0xFFB030)); Put(front, (int)cx + 6, 15 - (flicker == 1 ? 1 : 0), P.Hex(0xFFE060)); Put(front, (int)cx + 5, 16, P.Hex(0xFF6A20)); Put(front, (int)cx + 7, 16, P.Hex(0xFF6A20));
        Put(front, (int)cx + 6, 14 - (flicker == 1 ? 1 : 0), P.Hex(0xFF6A20));
        Ellipse(glow, cx + 6.5f, 15.5f, 3f, 3f, new[] { P.Hex(0xFF9A3A, 80), P.Hex(0xFF9A3A, 80), P.Hex(0xFF6A20, 50) });
    }

    // MARK: Shapes

    private static Vector2 V(float x, float y) => new Vector2(x, y);

    private static void Put(PixelCanvas c, int x, int y, P colour) { if (c.Contains(x, y)) c[x, y] = colour; }

    private static void Rect(PixelCanvas c, int x, int y, int w, int h, P colour)
    {
        for (int j = 0; j < h; j++) for (int i = 0; i < w; i++) Put(c, x + i, y + j, colour);
    }

    private static void Line(PixelCanvas c, Vector2 a, Vector2 b, P colour)
    {
        int n = (int)MathF.Max(1, MathF.Ceiling((b - a).Length() * 1.5f));
        for (int i = 0; i <= n; i++) { var p = a.Lerp(b, (float)i / n); Put(c, (int)MathF.Floor(p.X), (int)MathF.Floor(p.Y), colour); }
    }

    /// <summary>A polygon in three tones, lit from the left (side 1) or from the right (side -1: the mirrored half of a pair).</summary>
    private static void Poly(PixelCanvas c, Vector2[] pts, P[] ramp, int side)
    {
        var layer = new PixelCanvas(c.Width, c.Height);
        PixelPaint.Poly(layer, pts, ramp[1]);
        Shade(c, layer, ramp, side);
    }

    private static void Ellipse(PixelCanvas c, float cx, float cy, float rx, float ry, P[] ramp)
    {
        var layer = new PixelCanvas(c.Width, c.Height);
        PixelPaint.Ellipse(layer, cx, cy, rx, ry, ramp[1]);
        Shade(c, layer, ramp, 1);
    }

    private static void Shade(PixelCanvas c, PixelCanvas layer, P[] ramp, int side)
    {
        int minX = c.Width, maxX = 0;
        for (int y = 0; y < c.Height; y++) for (int x = 0; x < c.Width; x++) if (layer[x, y].A > 0) { minX = Math.Min(minX, x); maxX = Math.Max(maxX, x); }
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                if (layer[x, y].A == 0) continue;
                float t = (x + 0.5f - minX) / Math.Max(1, maxX - minX + 1);
                if (side < 0) t = 1 - t;
                float f = t * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f;
                c[x, y] = f < 0.7f ? ramp[0] : (f < 2.1f ? ramp[1] : ramp[2]);
            }
    }
}
