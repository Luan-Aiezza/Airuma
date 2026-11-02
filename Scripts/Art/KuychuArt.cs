using System;
using Godot;
using P = PixelColor;
using static PixelShapes;

/// <summary>
/// Kuychu, the guardian between the two worlds, is TWO divine persons who are one being (as the Amaru is made of many parts): they have one name, one mind and one voice
/// and share one life, but they stand apart and move apart. They are tall and serene, dressed in long, flowing gowns with the mantles and the weaving of the peoples of the Andes
/// made divine, like a high elf: a light about each head, a gown that falls to the floor, hands open. They wear no crown and no rainbow; each wears a hood, whose shadow covers the face.
///   The one on the left (<see cref="MakeLeft"/>) is the sun's: a long gown of cream wool with a golden collar like a disc of the sun, a mantle of crimson and gold with steps of tocapu
///   (the woven squares of the Incas) along its edges, a sash of gold, a hood of crimson, and a staff of gold that ends in a serpent.
///   The one on the right (<see cref="MakeRight"/>) is the water's: a long gown of silver-blue, falling in tiers like a pollera, a veil-mantle (an aguayo made light) in indigo, a hood of indigo,
///   long hair of silver that falls to the waist, and a drop of water hovering over her palm.
/// Each picture is 60 x 80 pixels (each shown 2 x 2 screen pixels). Four frames: they breathe, the mantles and the hair sway, the lights pulse, and rings spread on the water.
/// </summary>
public static class KuychuArt
{
    public const int Width = 60, Height = 80;
    /// <summary>Art pixel that is the boss's position (the middle of the body).</summary>
    public static readonly (int x, int y) Centre = (30, 50);
    /// <summary>Row of the water, where the rings spread under the hem.</summary>
    public const int WaterRow = 76;

    private const int Cx = 30;
    private static readonly P Outline = P.Hex(0x1C1020);
    private static readonly P[] Skin = { P.Hex(0xF4CDA4), P.Hex(0xDDA878), P.Hex(0xB8805A) };
    private static readonly P[] Cream = { P.Hex(0xFFFBEE), P.Hex(0xF0E0C4), P.Hex(0xC8AE8E) };
    private static readonly P[] Crimson = { P.Hex(0xE86250), P.Hex(0xB62A30), P.Hex(0x7A1A24) };
    private static readonly P[] Gold = { P.Hex(0xFFF0A8), P.Hex(0xF2C14E), P.Hex(0xB8862B) };
    private static readonly P[] Silver = { P.Hex(0xFFFFFF), P.Hex(0xDCE6F4), P.Hex(0x9AA8C4) };
    private static readonly P[] Ice = { P.Hex(0xE8F6FF), P.Hex(0xA8D0F0), P.Hex(0x5E8CC8) };
    private static readonly P[] Indigo = { P.Hex(0x6A62D8), P.Hex(0x3C34A4), P.Hex(0x221C68) };
    private static readonly P[] Teal = { P.Hex(0x58D4C4), P.Hex(0x2A9E98), P.Hex(0x186A6A) };
    private static readonly P[] DarkHair = { P.Hex(0x5A4A66), P.Hex(0x2C2234), P.Hex(0x120C1A) };
    private static readonly P[] Rainbow = { P.Hex(0xE23C34), P.Hex(0xF28A2A), P.Hex(0xFFD23F), P.Hex(0x3AB04A), P.Hex(0x2F8AE8), P.Hex(0x4A3AB8), P.Hex(0x9A4AC8) };
    private static readonly P Ink = P.Hex(0x1A0F0A);

    private static void Put(PixelCanvas c, int x, int y, P colour) => BossArt.Put(c, x, y, colour);

    private static PixelCanvas Sheet() => new PixelCanvas(Width, Height);

    /// <summary>The lines of the folds of a gown: darker streaks running down it, fanning out towards the hem.</summary>
    private static void Folds(PixelCanvas c, int top, int bottom, P[] ramp, int count, float spread)
    {
        for (int k = 0; k < count; k++)
        {
            float u = (k + 0.5f) / count - 0.5f;
            for (int y = top; y <= bottom; y++)
            {
                float t = (y - top) / (float)(bottom - top);
                int x = (int)MathF.Round(Cx + u * (4 + spread * t) * 2);
                if (c.Contains(x, y) && c[x, y].A > 0 && (y + k) % 7 != 0) c[x, y] = ramp[k % 2 == 0 ? 2 : 0];
            }
        }
    }

    /// <summary>
    /// The head under a hood: the face is drawn and then lost in the hood's shadow (darker from the brow down to the nose, and a little lighter at the mouth and chin), with the eyes
    /// as two small pale lights; over it the hood, a dome that falls to the shoulders, with an oval left open.
    /// </summary>
    private static void Hooded(PixelCanvas head, int bob, P[] cloth, P[] eyes)
    {
        FillEllipse(head, Cx, 17.4f + bob, 4.9f, 5.8f, Skin);
        var shade = P.Hex(0x120C1A);
        for (int y = 0; y < head.Height; y++)
            for (int x = 0; x < head.Width; x++)
            {
                if (head[x, y].A == 0) continue;
                float depth = y - 17.4f - bob;                                     // the shadow is deepest at the top of the face
                float amount = depth < 1 ? 0.88f : (depth < 3.5f ? 0.72f : 0.5f);
                head[x, y] = head[x, y].Mixed(shade, amount);
            }
        Put(head, Cx - 3, 17 + bob, eyes[0]); Put(head, Cx - 2, 17 + bob, eyes[0]); Put(head, Cx + 2, 17 + bob, eyes[1]); Put(head, Cx + 3, 17 + bob, eyes[1]);
        var hood = Sheet();
        // a soft cowl, not a helmet: it hugs the head, comes to a gentle point above it and drapes over the shoulders in folds
        Fill(hood, Pts((Cx - 9f, 29f + bob), (Cx - 7.4f, 15f + bob), (Cx - 5f, 9.2f + bob), (Cx - 1.2f, 5.6f + bob), (Cx, 4.6f + bob), (Cx + 1.2f, 5.6f + bob), (Cx + 5f, 9.2f + bob), (Cx + 7.4f, 15f + bob), (Cx + 9f, 29f + bob), (Cx + 4.2f, 25.4f + bob), (Cx - 4.2f, 25.4f + bob)), cloth);
        for (int y = 0; y < hood.Height; y++)
            for (int x = 0; x < hood.Width; x++)
            {
                float dx = (x + 0.5f - Cx) / 4.7f, dy = (y + 0.5f - 17.8f - bob) / 6.1f;
                if (dx * dx + dy * dy <= 1) hood[x, y] = P.Clear;                  // the opening: the face shows through it, in shadow
            }
        // the shadow that the hood throws on the opening's edge, and the folds of the cloth
        for (int y = 0; y < hood.Height; y++)
            for (int x = 0; x < hood.Width; x++)
                if (hood[x, y].A > 0 && y < 24 + bob)
                {
                    float dx = (x + 0.5f - Cx) / 5.3f, dy = (y + 0.5f - 17.8f - bob) / 6.7f;
                    if (dx * dx + dy * dy <= 1) hood[x, y] = cloth[2];
                }
        for (int y = 12 + bob; y <= 28 + bob; y++) { foreach (int sd in new[] { -1, 1 }) { int x = Cx + sd * (int)MathF.Round(6.2f + (y - 12) * 0.12f); if (hood.Contains(x, y) && hood[x, y].A > 0 && y % 4 != 0) hood[x, y] = cloth[2]; } }
        Blit(head, hood);
    }

    private static PixelCanvas Finish(PixelCanvas glow, PixelCanvas fig, int frame)
    {
        var c = Sheet();
        PixelPaint.Ellipse(c, Cx, WaterRow + 0.5f, 22, 3.2f, new P(8, 10, 22, 90));
        BossArt.DrawRipples(c, Cx, WaterRow, 24, 4.4f, frame, false);
        Blit(c, glow);
        Blit(c, fig);
        BossArt.DrawRipples(c, Cx, WaterRow, 24, 4.4f, frame, true);
        return c;
    }

    // MARK: The two as one

    /// <summary>
    /// The two fused into one body (the second half of the fight): one tall hooded figure, the left half of the one that is the sun's and the right half of the one that is the water's, joined
    /// down the middle: a hood of crimson and a hood of indigo, a gown of cream and a gown of ice, a mantle of crimson and a mantle of indigo, the staff of gold in the left hand and the
    /// drop of water over the right palm, a gold light for one eye and a pale blue one for the other.
    /// </summary>
    public static PixelCanvas MakeFused(int frame)
    {
        int bob = frame == 2 || frame == 3 ? 1 : 0;
        var l = LeftFigure(frame, bob); var r = RightFigure(frame, bob);
        var c = Sheet();
        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) c[x, y] = x < Cx ? l[x, y] : r[x, y];
        // a thin seam down the middle of the body, where the two are joined, in a pale light
        for (int y = 26; y <= 74; y++) if (c[Cx, y].A > 0) c[Cx, y] = (y % 3 == 0) ? P.Hex(0xFFFFE8) : c[Cx, y].Mixed(P.Hex(0xFFFFE8), 0.45f);
        return c;
    }

    // MARK: The one on the left: the sun's

    public static PixelCanvas MakeLeft(int frame) => LeftFigure(frame, null);

    private static PixelCanvas LeftFigure(int frame, int? bobOverride)
    {
        int bob = bobOverride ?? (frame == 2 || frame == 3 ? 1 : 0);
        float sway = new[] { 0f, 1f, 0f, -1f }[frame];
        var mantle = Sheet(); var gown = Sheet(); var sash = Sheet(); var arms = Sheet(); var hair = Sheet(); var head = Sheet(); var staff = Sheet(); var glow = Sheet();

        // the mantle: crimson, wide, behind her, falling from the shoulders to the floor; along its edges, steps of gold like the tocapu of the Incas
        Fill(mantle, Pts((Cx - 10f, 22f + bob), (Cx + 10f, 22f + bob), (Cx + 20f + sway, 75f), (Cx - 20f - sway, 75f)), Crimson);
        for (int y = 26; y <= 74; y++)
        {
            int half = (int)MathF.Round(10 + (y - 22) * 0.19f + sway * (y - 22) / 50f);
            if ((y / 3) % 2 == 0) { Put(mantle, Cx - half + 1, y, Gold[1]); Put(mantle, Cx - half + 2, y, Gold[1]); Put(mantle, Cx + half - 1, y, Gold[1]); Put(mantle, Cx + half - 2, y, Gold[1]); }
            else { Put(mantle, Cx - half + 3, y, Gold[2]); Put(mantle, Cx + half - 3, y, Gold[2]); }
        }
        Folds(mantle, 28, 74, Crimson, 8, 10);

        // the gown: cream wool to the floor, a gold band at the hem and a bright line down the middle
        Fill(gown, Pts((Cx - 7.5f, 26f + bob), (Cx + 7.5f, 26f + bob), (Cx + 8.5f, 42f), (Cx + 14f, 75f), (Cx - 14f, 75f), (Cx - 8.5f, 42f)), Cream);
        for (int x = Cx - 14; x <= Cx + 14; x++) { Put(gown, x, 73, Gold[1]); Put(gown, x, 74, (x % 2 == 0) ? Gold[0] : Gold[2]); Put(gown, x, 75, Gold[2]); }
        Folds(gown, 34, 72, Cream, 7, 7);
        // the collar: a disc of gold like the sun, with rays
        FillEllipse(gown, Cx, 28f + bob, 8f, 4.4f, Gold);
        FillEllipse(gown, Cx, 28f + bob, 3.6f, 2.6f, new[] { Gold[0], Gold[0], Gold[1] });
        for (int k = 0; k < 10; k++) { float a = k * MathF.PI / 5; Put(gown, (int)MathF.Round(Cx + MathF.Cos(a) * 8.6f), (int)MathF.Round(28 + bob + MathF.Sin(a) * 5f), Gold[0]); }
        // the sash of gold at the waist
        for (int x = Cx - 8; x <= Cx + 8; x++) for (int y = 41; y <= 43; y++) Put(sash, x, y, y == 42 ? Gold[0] : Gold[1]);
        for (int y = 44; y <= 54; y++) { Put(sash, Cx + 1, y, Gold[1]); Put(sash, Cx + 2, y, Gold[2]); }                    // the end of the sash

        // the arms: long wide sleeves; the right one (ours: the left of the picture) holds a staff, the other is open, the palm up, with the light of the sun over it
        Limb(arms, new[] { (Cx - 8f, 28f + bob), (Cx - 14f, 38f), (Cx - 17f, 46f) }, 3f, 2.3f, Cream); FillEllipse(arms, Cx - 17.5f, 47.6f, 2f, 2f, Skin); FillEllipse(arms, Cx - 16f, 44.4f, 3f, 1.6f, Gold);
        Limb(arms, new[] { (Cx + 8f, 28f + bob), (Cx + 15f, 35f), (Cx + 18f, 33f - bob) }, 3f, 2.3f, Cream); FillEllipse(arms, Cx + 18.4f, 31.2f - bob, 2f, 1.7f, Skin); FillEllipse(arms, Cx + 15.6f, 34.4f, 3f, 1.6f, Gold);
        FillEllipse(glow, Cx + 19f, 26f - bob, 4.6f, 4.6f, new[] { P.Hex(0xFFF0A8, 120), P.Hex(0xFFE060, 120), P.Hex(0xF2A02A, 70) });
        PixelPaint.Ellipse(glow, Cx + 19f, 26f - bob, 2f, 2f, P.Hex(0xFFFFE0));
        // the staff of gold, with a serpent's head at its top
        for (int y = 14; y <= 76; y++) { Put(staff, Cx - 19, y, y % 6 < 3 ? Gold[1] : Gold[2]); Put(staff, Cx - 18, y, Gold[0]); }
        FillEllipse(staff, Cx - 18.4f, 11.6f, 2.8f, 3.2f, Gold); FillEllipse(staff, Cx - 17.4f, 8.8f, 2.2f, 1.8f, Gold);
        Put(staff, Cx - 18, 8, Ink); Put(staff, Cx - 19, 12, Crimson[0]); Put(staff, Cx - 17, 14, Crimson[0]);

        // the hair: dark, long, loose over her shoulders; the head, under a hood of crimson whose shadow covers the face; a light about it
        Fill(hair, Pts((Cx - 7.6f, 11f + bob), (Cx + 7.6f, 11f + bob), (Cx + 9f + sway * 0.5f, 40f), (Cx + 5f, 52f), (Cx - 5f, 52f), (Cx - 9f - sway * 0.5f, 40f)), DarkHair);
        Hooded(head, bob, Crimson, new P[] { Gold[0], Gold[0] });
        FillEllipse(glow, Cx, 14f + bob, 11f, 11f, new[] { P.Hex(0xFFF0A8, 55), P.Hex(0xFFF0A8, 55), P.Hex(0xF2C14E, 30) });                       // the light round her head

        var fig = Sheet();
        AddOutlined(fig, staff, Outline); AddOutlined(fig, mantle, Outline); AddOutlined(fig, hair, Outline); AddOutlined(fig, gown, Outline); Blit(fig, sash);
        AddOutlined(fig, arms, Outline); AddOutlined(fig, head, Outline);
        return Finish(glow, fig, frame);
    }

    // MARK: The one on the right: the water's

    public static PixelCanvas MakeRight(int frame) => RightFigure(frame, null);

    private static PixelCanvas RightFigure(int frame, int? bobOverride)
    {
        int bob = bobOverride ?? (frame == 1 || frame == 2 ? 1 : 0);
        float sway = new[] { 0f, 1f, 0f, -1f }[frame];
        var veil = Sheet(); var gown = Sheet(); var arms = Sheet(); var hair = Sheet(); var head = Sheet(); var glow = Sheet();

        // the veil-mantle: indigo, light, from the head and the shoulders to the floor
        Fill(veil, Pts((Cx - 9f, 15f + bob), (Cx + 9f, 15f + bob), (Cx + 21f - sway, 75f), (Cx - 21f + sway, 75f)), Indigo);
        for (int y = 20; y <= 74; y++)
        {
            int half = (int)MathF.Round(9 + (y - 15) * 0.2f - sway * (y - 15) / 60f);
            Put(veil, Cx - half + 1, y, Indigo[0]); Put(veil, Cx + half - 1, y, Indigo[0]);
        }
        Folds(veil, 22, 74, Indigo, 8, 10);

        // the gown: silver-blue, to the floor, in tiers like a pollera, with a hem of white lace, a belt of silver with a turquoise stone, a neckline of ice
        Fill(gown, Pts((Cx - 7f, 24f + bob), (Cx + 7f, 24f + bob), (Cx + 8f, 40f), (Cx + 14f, 75f), (Cx - 14f, 75f), (Cx - 8f, 40f)), Ice);
        foreach (int tier in new[] { 50, 60, 69 })
            for (int x = Cx - 15; x <= Cx + 15; x++) if (gown[x, tier].A > 0) { Put(gown, x, tier, Teal[1]); Put(gown, x, tier + 1, (x % 2 == 0) ? Silver[0] : Silver[1]); }
        for (int x = Cx - 15; x <= Cx + 15; x++) { Put(gown, x, 74, (x % 2 == 0) ? Silver[0] : Silver[2]); Put(gown, x, 75, Silver[1]); }
        Folds(gown, 30, 72, Ice, 7, 7);
        for (int x = Cx - 7; x <= Cx + 7; x++) { Put(gown, x, 40, Silver[1]); Put(gown, x, 41, Silver[0]); }
        FillEllipse(gown, Cx, 40.8f, 2.2f, 2.2f, Teal);
        FillEllipse(gown, Cx, 26.5f + bob, 5.4f, 2.4f, Silver);                                                       // the neckline

        // the arms: long, wide sleeves; both open, the palms up; over the left palm (ours: the right of the picture) hovers a drop of water with a rainbow in it
        Limb(arms, new[] { (Cx - 7f, 27f + bob), (Cx - 14f, 36f), (Cx - 17f, 33f - bob) }, 3f, 2.3f, Ice); FillEllipse(arms, Cx - 17.6f, 31.2f - bob, 2f, 1.7f, Skin);
        Limb(arms, new[] { (Cx + 7f, 27f + bob), (Cx + 14f, 36f), (Cx + 17f, 33f - bob) }, 3f, 2.3f, Ice); FillEllipse(arms, Cx + 17.6f, 31.2f - bob, 2f, 1.7f, Skin);
        FillEllipse(glow, Cx + 17.6f, 23f - bob, 4.4f, 4.4f, new[] { P.Hex(0xC8F0FF, 110), P.Hex(0xB0E4FF, 110), P.Hex(0x5EA8E8, 70) });
        Put(glow, Cx + 17, (int)(22 - bob), P.Hex(0xFFFFFF));

        // the hair: silver, very long, falling to the waist and moving; the head, under a hood of indigo whose shadow covers the face; a light about it
        Fill(hair, Pts((Cx - 7.4f, 11f + bob), (Cx + 7.4f, 11f + bob), (Cx + 10f + sway * 0.7f, 42f), (Cx + 7f, 58f), (Cx - 7f, 58f), (Cx - 10f - sway * 0.7f, 42f)), Silver);
        Folds(hair, 20, 56, Silver, 5, 5);
        Hooded(head, bob, Indigo, new P[] { Ice[0], Ice[0] });
        FillEllipse(glow, Cx, 13f + bob, 11f, 11f, new[] { P.Hex(0xC8F0FF, 55), P.Hex(0xC8F0FF, 55), P.Hex(0x5EA8E8, 30) });

        var fig = Sheet();
        AddOutlined(fig, veil, Outline); AddOutlined(fig, hair, Outline); AddOutlined(fig, gown, Outline); AddOutlined(fig, arms, Outline); AddOutlined(fig, head, Outline);
        return Finish(glow, fig, frame);
    }
}
