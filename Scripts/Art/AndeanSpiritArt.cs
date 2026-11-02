using System;
using Godot;
using P = PixelColor;

/// <summary>
/// The common enemies of stage 5, the waterfall in the Andes: lesser creatures of the myths of the region, each drawn on a 22 x 24 grid (44 x 48 on the screen):
///   Swarm  the alicanto: the night bird of the Andes, whose wings shine like gold and silver; a small dark body, a golden breast, a curved beak and glowing eyes, and
///          wide wings of metallic feathers that beat (up, level, down) over a fan of tail feathers;
///   Gunner the yacuruna: a spirit of the water, with sea-green scaly skin, a crest of fins on its head, round yellow eyes and gills, that lifts a conch shell to its mouth
///          and blows (it is a horn), and a body that ends in the tail of a fish;
///   Heavy  the condenado: a damned soul that wanders the heights, a skeleton hunched in a ragged cloak, with horns, a skull with eyes of red fire, bony hands
///          and the broken shackles and chains of its sentence, that sway.
/// </summary>
public static class AndeanSpiritArt
{
    public const int Width = 22, Height = 24, FrameCount = 3;

    private static readonly P Outline = P.Hex(0x0C0A14);

    /// <summary>The colour of the sparks when a spirit is defeated.</summary>
    public static P SparkColour(SpiritKind kind) => kind switch
    {
        SpiritKind.Swarm => P.Hex(0xFFD860),
        SpiritKind.Gunner => P.Hex(0x7AE8C8),
        _ => P.Hex(0xFF5A3A),
    };

    public static PixelCanvas MakeCanvas(SpiritKind kind, int frame)
    {
        var back = new PixelCanvas(Width, Height);
        var body = new PixelCanvas(Width, Height);
        var front = new PixelCanvas(Width, Height);
        var glow = new PixelCanvas(Width, Height);       // not outlined, under the figure
        var top = new PixelCanvas(Width, Height);        // not outlined, over the figure
        frame %= FrameCount;
        switch (kind)
        {
            case SpiritKind.Swarm: Alicanto(back, body, front, glow, top, frame); break;
            case SpiritKind.Gunner: Yacuruna(back, body, front, glow, top, frame); break;
            default: Condenado(back, body, front, glow, top, frame); break;
        }
        var fig = new PixelCanvas(Width, Height);
        foreach (var part in new[] { back, body, front }) { PixelPaint.Outline(part, Outline); PixelPaint.Composite(fig, part); }
        var c = new PixelCanvas(Width, Height);
        PixelPaint.Composite(c, glow);
        PixelPaint.Composite(c, fig);
        PixelPaint.Composite(c, top);
        return c;
    }

    // MARK: The alicanto

    private static void Alicanto(PixelCanvas back, PixelCanvas body, PixelCanvas front, PixelCanvas glow, PixelCanvas top, int frame)
    {
        P[] gold = { P.Hex(0xFFF0A0), P.Hex(0xE8B840), P.Hex(0x9A6420) };
        P[] silver = { P.Hex(0xF4F8FF), P.Hex(0xB4C4DC), P.Hex(0x6A7C98) };
        P[] dark = { P.Hex(0x4A4258), P.Hex(0x2C2638), P.Hex(0x16121E) };
        float cx = 10.5f;
        float tipY = new[] { 3f, 9.5f, 16f }[frame];
        float bob = frame == 2 ? 1 : 0;
        // the glow of its wings (it shines in the dark)
        Ellipse(glow, cx, 11 + bob, 9f, 5.5f, new[] { P.Hex(0xFFE890, 40), P.Hex(0xFFE890, 40), P.Hex(0xE8B840, 24) });
        foreach (int side in new[] { -1, 1 })
        {
            float s = side;
            // the wing: a fan of feathers from the shoulder, golden along their length and silver at their tips
            var shoulder = V(cx + s * 2f, 10.4f + bob);
            var upper = V(cx + s * 10.6f, tipY);
            var lower = V(cx + s * 3.6f, 15.8f + bob);
            for (int k = 0; k <= 12; k++)
            {
                var end = upper.Lerp(lower, k / 12f) + new Vector2(0, (k % 2) * 0.8f);
                var tipStart = shoulder.Lerp(end, 0.72f);
                Line(back, shoulder, tipStart, k % 3 == 0 ? gold[0] : gold[1]);
                Line(back, tipStart, end, k % 2 == 0 ? silver[0] : silver[1]);
            }
        }
        // the fan of the tail
        for (int k = -2; k <= 2; k++) Line(back, V(cx, 16 + bob), V(cx + k * 1.6f, 22), k % 2 == 0 ? gold[1] : silver[1]);
        // the body: dark, with a golden breast, a head with a curved beak and eyes of light
        Ellipse(body, cx, 13.5f + bob, 2.8f, 4f, dark);
        Ellipse(body, cx, 13f + bob, 1.7f, 2.8f, gold);
        Ellipse(body, cx, 8f + bob, 2.6f, 2.5f, dark);
        Poly(body, new[] { V(cx - 3f, 6.4f + bob), V(cx - 3.4f, 3.6f + bob), V(cx - 1f, 5.6f + bob) }, dark, 1);          // a crest of feathers
        Poly(body, new[] { V(cx + 3f, 6.4f + bob), V(cx + 3.4f, 3.6f + bob), V(cx + 1f, 5.6f + bob) }, dark, -1);
        Poly(front, new[] { V(cx - 1.2f, 9.2f + bob), V(cx + 1.2f, 9.2f + bob), V(cx, 12f + bob) }, new[] { P.Hex(0xFFB040), P.Hex(0xE88A20), P.Hex(0xA05A10) }, 1);   // the beak, hooked
        Put(front, (int)cx, 12 + (int)bob, P.Hex(0xA05A10));
        foreach (int dx in new[] { -2, 1 }) { Put(top, (int)cx + dx, 7 + (int)bob, P.Hex(0xFFFFE0)); Put(top, (int)cx + dx, 8 + (int)bob, P.Hex(0xFFE060)); }
    }

    // MARK: The yacuruna

    private static void Yacuruna(PixelCanvas back, PixelCanvas body, PixelCanvas front, PixelCanvas glow, PixelCanvas top, int frame)
    {
        P[] skin = { P.Hex(0x7AE0B8), P.Hex(0x3AA890), P.Hex(0x1E6A6A) };
        P[] fin = { P.Hex(0xB8F4E4), P.Hex(0x5ACCC0), P.Hex(0x2A8A94) };
        P[] shell = { P.Hex(0xFFF0DC), P.Hex(0xF0B8A8), P.Hex(0xB07868) };
        float cx = 10.5f, sway = new[] { 0f, 1f, -1f }[frame], bob = frame == 1 ? 1 : 0;
        // the tail of a fish: from the waist it narrows, curls and ends in a forked fin
        for (int y = 14; y <= 21; y++)
        {
            float t = (y - 14) / 7f;
            float half = 3.6f * (1 - t * 0.7f);
            float centre = cx + sway * t * 1.6f + t * t * 2.5f;
            for (int x = 0; x < Width; x++)
            {
                float dx = x + 0.5f - centre;
                if (MathF.Abs(dx) > half) continue;
                float f = (dx / half + 1) * 1.5f + (Bayer.Threshold(x, y) - 0.5f) * 0.7f;
                Put(back, x, y + (int)bob, ((x + y) % 4 == 0) ? skin[2] : (f < 0.8f ? skin[0] : (f < 2.2f ? skin[1] : skin[2])));      // scaled
            }
        }
        float tx = cx + sway * 1.6f + 2.5f;
        Poly(back, new[] { V(tx - 0.8f, 21f + bob), V(tx - 4.2f, 23.6f), V(tx - 1.4f, 22.4f + bob), V(tx, 23.8f), V(tx + 1.4f, 22.4f + bob), V(tx + 4.2f, 23.6f), V(tx + 0.8f, 21f + bob) }, fin, 1);
        // the body and the head: a crest of fins like a comb, round yellow eyes, gills, a wide mouth
        Ellipse(body, cx, 14f + bob, 3.8f, 3.2f, skin);
        Ellipse(body, cx, 8.6f + bob, 4.6f, 4.2f, skin);
        Poly(body, new[] { V(cx - 4f, 6f + bob), V(cx - 3.2f, 1.6f + bob), V(cx - 1.2f, 4.4f + bob), V(cx, 0.4f + bob), V(cx + 1.2f, 4.4f + bob), V(cx + 3.2f, 1.6f + bob), V(cx + 4f, 6f + bob) }, fin, 1);   // the crest
        Poly(body, new[] { V(cx - 4.8f, 9f + bob), V(cx - 7.6f, 8f + bob), V(cx - 5.4f, 11.4f + bob) }, fin, 1);         // fins on the cheeks
        Poly(body, new[] { V(cx + 4.8f, 9f + bob), V(cx + 7.6f, 8f + bob), V(cx + 5.4f, 11.4f + bob) }, fin, -1);
        foreach (int dx in new[] { -3, 1 }) { Rect(top, (int)cx + dx, 7 + (int)bob, 3, 3, P.Hex(0xFFE860)); Put(top, (int)cx + dx + 1, 8 + (int)bob, P.Hex(0x14202A)); Put(top, (int)cx + dx, 7 + (int)bob, P.Hex(0xFFFFFF)); }
        Rect(top, (int)cx - 2, 11 + (int)bob, 4, 1, P.Hex(0x14202A));                      // the mouth, wide
        // gills on the neck
        foreach (int k in new[] { 0, 2 }) { Put(top, (int)cx - 4, 12 + k + (int)bob, skin[2]); Put(top, (int)cx + 3, 12 + k + (int)bob, skin[2]); }
        // the arms: webbed hands, one holding a conch to the mouth, which it raises and lowers with the frames
        float lift = new[] { 0f, 1.5f, 0.5f }[frame];
        Limb(front, V(cx - 3.4f, 13.5f + bob), V(cx - 6.4f, 16.5f + bob), skin);
        Limb(front, V(cx + 3.4f, 13.5f + bob), V(cx + 5.6f, 13.2f + bob - lift), skin);
        Poly(front, new[] { V(cx + 4.4f, 12.8f + bob - lift), V(cx + 8.8f, 9.4f + bob - lift), V(cx + 9.6f, 12f + bob - lift), V(cx + 6.4f, 14.4f + bob - lift) }, shell, -1);   // the conch
        Put(front, (int)(cx + 8), (int)(10 + bob - lift), shell[2]); Put(front, (int)(cx + 8), (int)(11 + bob - lift), shell[2]); Put(front, (int)(cx + 9), (int)(11 + bob - lift), shell[1]);
        Ellipse(glow, cx + 7, 11 + bob - lift, 3.4f, 3.4f, new[] { P.Hex(0x9AF4E4, 60), P.Hex(0x9AF4E4, 60), P.Hex(0x5ACCC0, 40) });   // the sound it makes
    }

    // MARK: The condenado

    private static void Condenado(PixelCanvas back, PixelCanvas body, PixelCanvas front, PixelCanvas glow, PixelCanvas top, int frame)
    {
        P[] cloak = { P.Hex(0x4C4A5C), P.Hex(0x2C2A38), P.Hex(0x16141E) };
        P[] bone = { P.Hex(0xFFF4DC), P.Hex(0xD8C8A8), P.Hex(0x8A7A5E) };
        P[] iron = { P.Hex(0xB8C0CC), P.Hex(0x6A7482), P.Hex(0x343A46) };
        float cx = 10.5f, sway = new[] { 0f, 1f, -1f }[frame], bob = frame == 2 ? 1 : 0;
        // the cloak: hunched, wide at the shoulders and ragged at the hem, that flutters
        Poly(back, new[] { V(cx - 4f, 8f + bob), V(cx + 4f, 8f + bob), V(cx + 8.4f, 15f), V(cx + 7f + sway, 20f), V(cx + 4.4f, 18.6f - sway), V(cx + 2.4f, 22.4f), V(cx, 19.4f), V(cx - 2.4f, 22.8f), V(cx - 4.6f, 18.8f + sway), V(cx - 7.4f - sway, 20.4f), V(cx - 8.6f, 14.6f) }, cloak, 1);
        for (int y = 11; y <= 20; y += 3) Line(back, V(cx - 2.2f, y), V(cx - 3f - sway * 0.5f, y + 2.2f), cloak[2]);
        // the hood and the skull: a hood pulled over, the skull in its shadow, two horns, eyes of red fire, the teeth
        Ellipse(body, cx, 8f + bob, 4.6f, 4.8f, cloak);
        Poly(body, new[] { V(cx - 3f, 5f + bob), V(cx - 5.6f, 2.4f + bob), V(cx - 5.2f, -0.2f + bob), V(cx - 2.2f, 3.6f + bob) }, bone, 1);        // horns, curving up
        Poly(body, new[] { V(cx + 3f, 5f + bob), V(cx + 5.6f, 2.4f + bob), V(cx + 5.2f, -0.2f + bob), V(cx + 2.2f, 3.6f + bob) }, bone, -1);
        Ellipse(body, cx, 8.6f + bob, 3.6f, 3.6f, bone);
        Rect(body, (int)cx - 1, 11 + (int)bob, 3, 2, bone[1]);              // the jaw, narrow
        for (int x = (int)cx - 1; x <= (int)cx + 1; x++) Put(top, x, 12 + (int)bob, x % 2 == 0 ? P.Hex(0x16141E) : bone[0]);      // the teeth
        foreach (int dx in new[] { -3, 1 }) { Rect(top, (int)cx + dx, 7 + (int)bob, 3, 3, P.Hex(0x16141E)); Put(top, (int)cx + dx + 1, 8 + (int)bob, frame == 1 ? P.Hex(0xFFE060) : P.Hex(0xFF4030)); Put(top, (int)cx + dx + 1, 9 + (int)bob, P.Hex(0xFF4030)); }   // the sockets, with fire
        Put(top, (int)cx - 1, 10 + (int)bob, P.Hex(0x16141E)); Put(top, (int)cx, 10 + (int)bob, P.Hex(0x16141E));                  // the hole of the nose
        Put(top, (int)cx - 4, 10 + (int)bob, bone[2]); Put(top, (int)cx + 3, 10 + (int)bob, bone[2]);                              // the cheekbones
        Ellipse(glow, cx, 9f + bob, 6f, 5f, new[] { P.Hex(0xFF3A20, 60), P.Hex(0xFF3A20, 60), P.Hex(0xA01810, 40) });      // its fire
        // the arms, of bone, hanging, with the shackles and the chains of its sentence, that swing
        Line(front, V(cx - 4f, 12.5f + bob), V(cx - 6.4f, 17f), bone[1]); Line(front, V(cx + 4f, 12.5f + bob), V(cx + 6.4f, 17f), bone[1]);
        foreach (int side in new[] { -1, 1 })
        {
            var hand = V(cx + side * 6.6f, 17.6f);
            Rect(front, (int)hand.X - 1, (int)hand.Y, 3, 2, bone[0]);                                                  // the hand
            Rect(front, (int)hand.X - 1, (int)hand.Y + 2, 3, 1, iron[0]);                                              // the shackle
            for (int k = 0; k < 3; k++) Put(front, (int)(hand.X + side * (k % 2) * 0.0f + sway * side * 0.5f * (k + 1) / 2), (int)hand.Y + 3 + k, (k % 2 == 0) ? iron[1] : iron[0]);   // the chain
        }
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

    private static void Limb(PixelCanvas c, Vector2 a, Vector2 b, P[] ramp)
    {
        for (int i = 0; i <= 6; i++)
        {
            var layer = new PixelCanvas(c.Width, c.Height);
            var p = a.Lerp(b, i / 6f);
            PixelPaint.Ellipse(layer, p.X, p.Y, 1.1f, 1.1f, ramp[1]);
            for (int y = 0; y < c.Height; y++) for (int x = 0; x < c.Width; x++) if (layer[x, y].A > 0) c[x, y] = i % 2 == 0 ? ramp[0] : ramp[1];
        }
    }

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
