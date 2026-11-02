using System;
using Godot;
using P = PixelColor;
using static PixelShapes;

/// <summary>
/// Mama Killa, the Moon, goddess of the night (the boss of stage 6), after the artwork of the goddess: a beautiful, pale woman of blue-grey skin and black eyes, with long black hair, a
/// fan-shaped ornament of silver studded with dots floating over her head, a short cape of blue-violet over her shoulders with a round brooch at the throat, a silver belt and a long skirt
/// with a great slit that shows one bare leg, whose toe just touches the water line (she floats). She has FOUR ARMS: the upper ones are open at her sides, and over the left hand floats a bright silver crescent moon, and over the right one a bright silver full
/// moon; the lower ones hold in front of her body a BLACK full moon, with a thin rim of silver light. 60 x 80 pixels (shown at 1.6 on the screen), four frames: she breathes, the cape, the
/// hair and the skirt sway, the three moons float, and the ornament rises and falls.
/// </summary>
public static class KillaArt
{
    public const int Width = 60, Height = 80;
    public static readonly (int x, int y) Centre = (30, 55);
    public const int WaterRow = 76;

    private const int Cx = 30;
    private static readonly P Outline = P.Hex(0x080A1C);
    private static readonly P[] Skin = { P.Hex(0xE2ECF4), P.Hex(0xB4C6DC), P.Hex(0x7488AE) };
    private static readonly P[] Silver = { P.Hex(0xFFFFFF), P.Hex(0xDCE6F8), P.Hex(0x8E9CC8) };
    private static readonly P[] Cape = { P.Hex(0x9A98E0), P.Hex(0x6462AC), P.Hex(0x3A3C80) };
    private static readonly P[] Skirt = { P.Hex(0x8C94D8), P.Hex(0x5A62A8), P.Hex(0x32387C) };
    private static readonly P[] Hair = { P.Hex(0x5A6498), P.Hex(0x262A4C), P.Hex(0x080A18) };
    private static readonly P[] Cloud = { P.Hex(0xF4F8FF), P.Hex(0xBCD0EC), P.Hex(0x7890C0) };
    private static readonly P[] Dark = { P.Hex(0x2A2C4C), P.Hex(0x12142C), P.Hex(0x04050E) };

    private static void Put(PixelCanvas c, int x, int y, P colour) => BossArt.Put(c, x, y, colour);
    private static PixelCanvas Sheet() => new PixelCanvas(Width, Height);

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

    /// <summary>Clears what lies inside a disc.</summary>
    private static void Cut(PixelCanvas c, float cx, float cy, float r)
    {
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                if (dx * dx + dy * dy < r * r) c[x, y] = P.Clear;
            }
    }

    /// <summary>An open hand, palm up, with its fingers a little spread.</summary>
    private static void Hand(PixelCanvas c, float x, float y, int side)
    {
        FillEllipse(c, x, y, 2.2f, 1.3f, Skin);
        for (int i = 0; i < 4; i++) Put(c, (int)MathF.Round(x + side * (2.6f + i * 0.7f)), (int)MathF.Round(y - 1.4f + i * 0.9f - (i == 0 ? 0 : 0.4f)), i % 2 == 0 ? Skin[0] : Skin[1]);
    }

    public static PixelCanvas MakeFrame(int frame)
    {
        int bob = frame == 2 || frame == 3 ? 1 : 0;
        float sway = new[] { 0f, 1f, 0f, -1f }[frame];
        float[] floatA = { 0f, -1f, 0f, 1f }, floatB = { 0f, 1f, 0f, -1f };
        float f0 = floatA[frame], f1 = floatB[frame], f2 = floatA[(frame + 1) % 4];
        var hairBack = Sheet(); var cape = Sheet(); var torso = Sheet(); var skirt = Sheet(); var leg = Sheet(); var lowerArms = Sheet(); var upperArms = Sheet(); var head = Sheet(); var hairFront = Sheet();
        var headdress = Sheet(); var blackMoon = Sheet(); var moons = Sheet(); var glow = Sheet(); var belt = Sheet();

        // the long black hair, falling behind her to the hip, a little longer on one side
        Fill(hairBack, Pts((Cx - 7.6f, 11f + bob), (Cx + 7.6f, 11f + bob), (Cx + 9.5f, 34f), (Cx + 10f + sway, 52f), (Cx + 6f, 58f), (Cx + 3.5f, 48f), (Cx - 3.5f, 48f), (Cx - 6f, 62f), (Cx - 11f - sway, 58f), (Cx - 10f, 34f)), Hair);
        for (int y = 16; y < 60; y++) { int x = Cx - 9 + (y * 5) % 19; if (hairBack.Contains(x, y) && hairBack[x, y].A > 0 && y % 3 != 0) hairBack[x, y] = Hair[0]; }

        // the skirt: long, blue-violet, with a GREAT slit up the front (a deep neckline of the skirt): all the left side of it is cut away from the waist down, and her bare leg shows, its
        // toe just touching the water line (row 76) while the hem of the rest hangs a little above it
        Fill(skirt, Pts((Cx - 5.5f, 40f), (Cx + 5.5f, 40f), (Cx + 9f, 56f), (Cx + 13f + sway, 73.5f), (Cx - 13f - sway, 73.5f), (Cx - 9f, 56f)), Skirt);
        Folds(skirt, 44, 72, Skirt, 6, 8);
        for (int x = Cx - 13; x <= Cx + 13; x++) Put(skirt, x, 73, Silver[2]);                  // the hem
        for (int y = 44; y < Height; y++)
        {
            int edge = (int)MathF.Round(Cx - 2.5f + (y - 44) * 0.2f);
            for (int x = 0; x < edge; x++) if (skirt[x, y].A > 0) skirt[x, y] = P.Clear;
            if (skirt[edge, y].A > 0) Put(skirt, edge, y, y % 2 == 0 ? Silver[1] : Silver[2]);      // a silver trim along the slit
        }
        // the leg: thigh, knee, a slender calf and a pointed foot that just touches the water
        Limb(leg, new[] { (Cx - 3.5f, 46f), (Cx - 5.5f, 57f), (Cx - 6.2f, 67f), (Cx - 6.6f, 72.5f) }, 3.4f, 1.5f, Skin);
        foreach (var (x, y) in new[] { (-8, 73), (-7, 73), (-6, 73), (-8, 74), (-7, 74), (-8, 75), (-8, 76) }) Put(leg, Cx + x, y, y >= 75 ? Skin[1] : Skin[0]);

        // the torso, bare at the midriff, and the silver belt
        FillEllipse(torso, Cx, 20.4f + bob, 2.1f, 2.6f, Skin);                                                             // the neck
        Fill(torso, Pts((Cx - 6f, 23f + bob), (Cx + 6f, 23f + bob), (Cx + 5f, 34f), (Cx + 5.6f, 41f), (Cx - 5.6f, 41f), (Cx - 5f, 34f)), Skin);
        for (int x = Cx - 6; x <= Cx + 6; x++) { Put(belt, x, 40, Silver[0]); Put(belt, x, 41, Silver[2]); }
        FillEllipse(belt, Cx - 2f, 40.6f, 1.6f, 1.6f, Silver);

        // the short cape of blue-violet over the shoulders, with the round brooch at the throat
        Fill(cape, Pts((Cx - 5f, 22f + bob), (Cx + 5f, 22f + bob), (Cx + 13f, 26f + bob), (Cx + 17f + sway * 0.5f, 33f), (Cx + 8f, 31f), (Cx, 29f), (Cx - 8f, 31f), (Cx - 17f - sway * 0.5f, 33f), (Cx - 13f, 26f + bob)), Cape);
        Folds(cape, 25, 32, Cape, 6, 5);
        FillEllipse(cape, Cx, 24.6f + bob, 1.9f, 1.9f, Silver);

        // the head: pale, with black eyes and pointed ears; black hair parted over the brow; and over it the fan-shaped ornament of silver
        FillEllipse(head, Cx - 5f, 16.6f + bob, 1.1f, 2f, Skin);                                                          // the ears
        FillEllipse(head, Cx + 5f, 16.6f + bob, 1.1f, 2f, Skin);
        Put(head, Cx - 6, 14 + bob, Skin[0]); Put(head, Cx + 6, 14 + bob, Skin[0]);
        FillEllipse(head, Cx, 16.6f + bob, 4.7f, 5.6f, Skin);
        Put(head, Cx - 3, 16 + bob, P.Hex(0x04050E)); Put(head, Cx - 2, 16 + bob, P.Hex(0x04050E)); Put(head, Cx - 3, 17 + bob, P.Hex(0x04050E)); Put(head, Cx - 2, 17 + bob, P.Hex(0x04050E));
        Put(head, Cx + 2, 16 + bob, P.Hex(0x04050E)); Put(head, Cx + 3, 16 + bob, P.Hex(0x04050E)); Put(head, Cx + 2, 17 + bob, P.Hex(0x04050E)); Put(head, Cx + 3, 17 + bob, P.Hex(0x04050E));
        Put(head, Cx - 3, 15 + bob, Skin[2]); Put(head, Cx - 2, 15 + bob, Skin[2]); Put(head, Cx + 2, 15 + bob, Skin[2]); Put(head, Cx + 3, 15 + bob, Skin[2]);   // the brows
        Put(head, Cx, 19 + bob, Skin[2]);
        Put(head, Cx - 1, 21 + bob, P.Hex(0x5A6AA0)); Put(head, Cx, 21 + bob, P.Hex(0x5A6AA0));
        Fill(hairFront, Pts((Cx - 5.6f, 13f + bob), (Cx, 10.4f + bob), (Cx + 5.6f, 13f + bob), (Cx + 5.4f, 18f + bob), (Cx + 3.4f, 14.6f + bob), (Cx + 0.4f, 13.4f + bob), (Cx - 1.6f, 14.8f + bob), (Cx - 3.6f, 14f + bob), (Cx - 5.4f, 21f + bob)), Hair);
        // the ornament: half a disc of silver, its rim darker, studded with dots
        float oy = 9.5f + bob - f2 * 0.0f;
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
            {
                float dx = (x + 0.5f - Cx) / 11.5f, dy = (y + 0.5f - oy) / 8.2f;
                if (dy > 0.05f || dx * dx + dy * dy > 1) continue;
                float edge = dx * dx + dy * dy;
                headdress[x, y] = edge > 0.72f ? Silver[2] : (dx < -0.2f ? Silver[0] : Silver[1]);
            }
        for (int k = 0; k < 9; k++)
        {
            float a = MathF.PI + (k + 0.5f) * MathF.PI / 9f;
            Put(headdress, (int)MathF.Round(Cx + MathF.Cos(a) * 9.2f), (int)MathF.Round(oy + MathF.Sin(a) * 6.3f), Dark[0]);
        }
        for (int x = Cx - 11; x <= Cx + 11; x++) Put(headdress, x, (int)MathF.Round(oy), Silver[2]);

        // the four arms: the upper ones open at her sides, palms up; the lower ones come round in front of her body and hold the black moon
        Limb(upperArms, new[] { (Cx - 6f, 25f + bob), (Cx - 15f, 29f), (Cx - 23.5f, 31f) }, 2f, 1.4f, Skin); Hand(upperArms, Cx - 25f, 31.6f, -1);
        Limb(upperArms, new[] { (Cx + 6f, 25f + bob), (Cx + 15f, 28f), (Cx + 23.5f, 30f) }, 2f, 1.4f, Skin); Hand(upperArms, Cx + 25f, 30.6f, 1);
        float my = 41f + bob * 0.5f;
        Limb(lowerArms, new[] { (Cx - 5f, 33f), (Cx - 9.5f, 41f), (Cx - 5f, 47.5f) }, 1.8f, 1.3f, Skin);
        Limb(lowerArms, new[] { (Cx + 5f, 33f), (Cx + 9.5f, 41f), (Cx + 5f, 47.5f) }, 1.8f, 1.3f, Skin);
        foreach (int side in new[] { -1, 1 }) for (int i = 0; i < 3; i++) Put(lowerArms, Cx + side * (3 - i), 46 - i + (int)bob, Skin[i % 2]);   // the fingers, under the moon

        // the black moon, in front of her body: a dark disc with faint craters, edged by a thin rim of silver light
        FillEllipse(blackMoon, Cx, my, 6.4f, 6.4f, Dark);
        foreach (var (dx, dy, r) in new[] { (-2f, -2f, 1.4f), (2.4f, 1.6f, 1.6f), (-1.5f, 3f, 1f) }) FillEllipse(blackMoon, Cx + dx, my + dy, r, r, new[] { Dark[0], Dark[0], Dark[1] });

        // the two bright moons, floating over her hands: a full silver moon over one, a silver crescent over the other
        float fx = Cx - 25f, fy = 20.5f + f0;                                                                              // the full moon (over the right hand)
        FillEllipse(moons, fx, fy, 5.6f, 5.6f, Silver);
        foreach (var (dx, dy, r) in new[] { (-1.5f, -1.5f, 1.3f), (1.8f, 1.2f, 1.6f), (-0.5f, 2.5f, 0.9f) }) FillEllipse(moons, fx + dx, fy + dy, r, r, new[] { Silver[2], Silver[2], Silver[2] });
        float cx2 = Cx + 25f, cy2 = 20f + f1;                                                                              // the crescent (over the left hand)
        FillEllipse(moons, cx2, cy2, 6.6f, 6.6f, Silver);
        Cut(moons, cx2 + 3.2f, cy2 - 1.4f, 5.6f);
        // the glow of each moon, and a soft one round her
        float pulse = frame % 2 == 0 ? 1f : 0.9f;
        FillEllipse(glow, fx, fy, 9f * pulse, 9f * pulse, new[] { P.Hex(0xDCE8FF, 80), P.Hex(0xC0D4FF, 80), P.Hex(0x7A8ADC, 50) });
        FillEllipse(glow, cx2 - 1.5f, cy2, 9f * pulse, 9f * pulse, new[] { P.Hex(0xDCE8FF, 70), P.Hex(0xC0D4FF, 70), P.Hex(0x7A8ADC, 45) });
        FillEllipse(glow, Cx, my, 9.5f, 9.5f, new[] { P.Hex(0x8A8AE0, 55), P.Hex(0x6A6AC0, 55), P.Hex(0x3A3C80, 40) });
        FillEllipse(glow, Cx, 17f + bob, 15f, 15f, new[] { P.Hex(0xB8C8FF, 40), P.Hex(0xB8C8FF, 40), P.Hex(0x6A7ADC, 25) });

        var fig = Sheet();
        AddOutlined(fig, hairBack, Outline); AddOutlined(fig, leg, Outline); AddOutlined(fig, skirt, Outline); AddOutlined(fig, torso, Outline); Blit(fig, belt); AddOutlined(fig, cape, Outline);
        AddOutlined(fig, lowerArms, Outline); AddOutlined(fig, blackMoon, P.Hex(0xA8B8E8));       // the rim of silver light
        AddOutlined(fig, upperArms, Outline); AddOutlined(fig, head, Outline); AddOutlined(fig, hairFront, Outline); AddOutlined(fig, headdress, Outline);
        var c = Sheet();
        Blit(c, glow);
        Blit(c, fig);
        Blit(c, moons);
        return c;
    }
}
