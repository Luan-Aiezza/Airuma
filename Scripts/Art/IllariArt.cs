using System;
using Godot;
using P = PixelColor;
using static PixelShapes;

/// <summary>
/// Illari, the priestess of the dawn who was taken by Mama Killa, in the style of <see cref="KillaArt"/> and the same size: the same slender figure, the same pointed ears, the same long hair
/// (but golden and curly), the same clothes (the short cape of blue-violet with its round brooch, the silver belt and the long skirt with the great slit that shows a leg), but with fair skin,
/// emerald eyes, only TWO arms, no ornament on her head and no moons. In both poses her hands are in prayer, together on her chest, and her arms are kept close to her body:
///   <see cref="MakeLying"/>    asleep, lying on her back on the water, seen from the side along the horizon, her FACE UP (her eyes are closed), her head to the left with the hair spread on the
///                              water, her hands together on her chest and a bare foot up at the end; 80 x 36 pixels;
///   <see cref="MakeStanding"/> awake, standing, her eyes open (emerald), the toe of one foot touching the water; 60 x 80 pixels (the size of Killa).
/// </summary>
public static class IllariArt
{
    public const int LyingWidth = 80, LyingHeight = 36, StandingWidth = 60, StandingHeight = 80;
    /// <summary>The row of the water line under her in the lying picture, and the row of the water at her toe in the standing one (that of Killa).</summary>
    public const int LyingBaseline = 26, StandingFeet = 76;

    private const int Cx = 30;
    private static readonly P Outline = P.Hex(0x080A1C);
    private static readonly P[] Skin = { P.Hex(0xFFF2E6), P.Hex(0xF0CCB4), P.Hex(0xC09C86) };
    private static readonly P[] Silver = { P.Hex(0xFFFFFF), P.Hex(0xDCE6F8), P.Hex(0x8E9CC8) };
    private static readonly P[] Cape = { P.Hex(0x9A98E0), P.Hex(0x6462AC), P.Hex(0x3A3C80) };
    private static readonly P[] Skirt = { P.Hex(0x8C94D8), P.Hex(0x5A62A8), P.Hex(0x32387C) };
    private static readonly P[] Hair = { P.Hex(0xFFF0B0), P.Hex(0xEBBE50), P.Hex(0xA8742A) };
    private static readonly P Emerald = P.Hex(0x20D080), EmeraldDark = P.Hex(0x0E6A40);

    private static void Put(PixelCanvas c, int x, int y, P colour) => BossArt.Put(c, x, y, colour);
    private static PixelCanvas Sheet(int w, int h) => new PixelCanvas(w, h);

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
    /// Straight, smooth, loose hair: the pixels of a hair sheet are painted again as long strands, light, middle and dark in thin bands. Round the head the strands run out of it, straight; beyond, they lie along
    /// the water in the way the hair flows, with a very slight wave. (No curls: it is not tied, it is not curled: it falls and lies.)
    /// </summary>
    private static void Strands(PixelCanvas c, float headX, float headY, float flowFrom)
    {
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                if (c[x, y].A == 0) continue;
                int band;
                if (x < flowFrom) band = (int)MathF.Floor(MathF.Atan2(y - headY, x - headX) * 9f);
                else band = (int)MathF.Floor((y - 1.3f * MathF.Sin(x * 0.11f)) * 0.9f);
                int m = ((band % 5) + 5) % 5;
                // the strands end at different places, in points
                if (x >= flowFrom && x > 48 + m * 3.5f) { c[x, y] = P.Clear; continue; }
                c[x, y] = m == 0 ? Hair[0] : (m == 3 ? Hair[2] : Hair[1]);
            }
    }

    /// <summary>Curls: little discs of gold along a path, each with a highlight and a shadow, so that the hair reads as ringlets.</summary>
    private static void Curls(PixelCanvas c, (float x, float y)[] path, float r)
    {
        for (int i = 0; i < path.Length; i++)
        {
            float rr = r * (0.85f + 0.3f * ((i * 37) % 5) / 4f);
            FillEllipse(c, path[i].x, path[i].y, rr, rr, Hair);
            Put(c, (int)MathF.Round(path[i].x - rr * 0.35f), (int)MathF.Round(path[i].y - rr * 0.35f), Hair[0]);
            Put(c, (int)MathF.Round(path[i].x + rr * 0.3f), (int)MathF.Round(path[i].y + rr * 0.5f), Hair[2]);
        }
    }

    // MARK: Asleep, lying on her back on the water

    public static PixelCanvas MakeLying()
    {
        int w = LyingWidth, h = LyingHeight;
        const int b = LyingBaseline;
        var hairBack = Sheet(w, h); var cape = Sheet(w, h); var torso = Sheet(w, h); var skirt = Sheet(w, h); var leg = Sheet(w, h); var arms = Sheet(w, h);
        var head = Sheet(w, h); var hairFront = Sheet(w, h); var belt = Sheet(w, h); var glow = Sheet(w, h);

        // the hair: long, straight, smooth and loose. A great fall of it round her head, spread on the water, and a river of straight hair that trails far along her side, past her feet; it is not tied and not curled
        Fill(hairBack, Pts((17f, 11.5f), (7f, 10.5f), (1.5f, 13.5f), (0f, 19f), (0.5f, 25f), (3f, 28.5f), (12f, 30f), (26f, 30.2f), (40f, 30f), (52f, 29.2f), (62f, 27.5f), (66f, 25.6f), (52f, 25.4f), (40f, 25.6f), (22f, 25.5f), (21f, 18f)), Hair);
        Strands(hairBack, 11f, 20.4f, 21f);

        // the body on her back: the cape over her shoulders with its brooch, the bare midriff, the silver belt, and the long skirt, with the great slit that shows a leg
        Fill(cape, Pts((17f, 18.5f), (28f, 17f), (33f, 18.6f), (33f, 26f), (17f, 26f)), Cape);
        Folds(cape, 19, 25, Cape, 3, 1);
        FillEllipse(cape, 19f, 20f, 1.6f, 1.6f, Silver);
        Fill(torso, Pts((32f, 18f), (39f, 18.2f), (39f, 26f), (32f, 26f)), Skin);
        for (int y = 18; y <= 25; y++) { Put(belt, 38, y, Silver[0]); Put(belt, 39, y, Silver[2]); }
        Fill(skirt, Pts((40f, 18.2f), (58f, 19.6f), (62f, 22f), (62f, 26f), (40f, 26f)), Skirt);
        for (int x = 43; x < 61; x += 3) for (int y = 19; y < 26; y++) if (skirt[x, y].A > 0 && (y + x) % 5 != 0) skirt[x, y] = Skirt[2];
        for (int y = 18; y <= 25; y++) Put(skirt, 61, y, Silver[2]);                             // the hem
        for (int x = 46; x <= 60; x++) { Put(skirt, x, 18 + (x - 46) / 6, Silver[1]); }        // the silver trim of the slit, along the top
        // the leg shows through the slit and goes on past the hem, the foot up
        Limb(leg, new[] { (47f, 19f), (58f, 19.6f), (66f, 21.5f), (72f, 22f) }, 2.3f, 1.6f, Skin);
        FillEllipse(leg, 74f, 19.6f, 1.8f, 2.6f, Skin);

        // the praying hands, together on her chest, and her arms close to her body (the forearm lies along her side)
        Limb(arms, new[] { (21f, 21f), (25f, 19.6f), (29f, 18.6f) }, 1.9f, 1.5f, Skin);
        FillEllipse(arms, 31.6f, 18f, 2.6f, 1.5f, Skin);
        for (int x = 29; x <= 34; x++) Put(arms, x, 18, Skin[2]);                               // the line between the palms
        Put(arms, 34, 17, Skin[0]); Put(arms, 34, 19, Skin[1]);

        // the head, the FACE UP: in profile, its forehead to the left; along the top, the closed eye (a curve of lashes), the nose, the rosy cheek and the little mouth; the pointed ear hidden under the hair
        FillEllipse(head, 11f, 20.4f, 5.9f, 5.9f, Skin);
        Put(head, 13, 14, Skin[0]); Put(head, 14, 14, Skin[0]); Put(head, 14, 15, Skin[1]); Put(head, 15, 15, Skin[1]);        // the nose
        for (int x = 7; x <= 10; x++) Put(head, x, 16 + (x == 7 || x == 10 ? 0 : 1), P.Hex(0x4A3A30));                          // the closed eye
        Put(head, 11, 16, Skin[2]);
        Put(head, 12, 18, P.Hex(0xF2A090)); Put(head, 13, 18, P.Hex(0xF2A090));
        Put(head, 16, 16, P.Hex(0xC8605A)); Put(head, 17, 17, P.Hex(0xC8605A));                                                 // the mouth
        // the hair in front of the head: straight, it falls from the crown over the brow and the side of the head, over the ears (they do not show), and lies on the water
        Fill(hairFront, Pts((3.5f, 17.5f), (5.5f, 14.4f), (9f, 15f), (8.2f, 18.4f), (12.8f, 19.3f), (15f, 22.8f), (14f, 27.5f), (5f, 28f), (2.8f, 23f)), Hair);
        Strands(hairFront, 11f, 20.4f, 99f);

        FillEllipse(glow, 38f, b - 4f, 34f, 9f, new[] { P.Hex(0xFFF4C0, 38), P.Hex(0xFFF4C0, 38), P.Hex(0xFFE690, 20) });

        var fig = Sheet(w, h);
        AddOutlined(fig, hairBack, Outline); AddOutlined(fig, skirt, Outline); AddOutlined(fig, leg, Outline); AddOutlined(fig, torso, Outline); Blit(fig, belt); AddOutlined(fig, cape, Outline);
        AddOutlined(fig, head, Outline); AddOutlined(fig, hairFront, Outline); AddOutlined(fig, arms, Outline);

        var c = Sheet(w, h);
        // rings on the water round her, and her faint reflection below, fading away
        for (int x = 2; x < w - 2; x++)
            foreach (var (rx, ry, rows) in new[] { (36f, 2.4f, 2), (28f, 1.8f, 0) })
            {
                float u = (x - 38f) / (rx + 4);
                if (MathF.Abs(u) >= 1) continue;
                int y = (int)MathF.Round(b + 3 + MathF.Sqrt(1 - u * u) * ry + rows);
                if (c.Contains(x, y) && (x + y) % 2 == 0) c[x, y] = P.Hex(0xC8E0FF, 90);
            }
        Blit(c, glow);
        for (int y = 0; y <= b; y++)
            for (int x = 0; x < w; x++)
            {
                if (fig[x, y].A == 0) continue;
                int ry = b + 1 + (b - y);
                float fade = 1 - (b - y) / (float)(h - b - 1);
                if (ry < h && fade > 0 && (x + ry) % 2 == 0 && c[x, ry].A == 0) c[x, ry] = fig[x, y].WithAlpha((byte)(70 * fade));
            }
        Blit(c, fig);
        return c;
    }

    // MARK: Awake, standing

    public static PixelCanvas MakeStanding()
    {
        int w = StandingWidth, h = StandingHeight;
        var hairBack = Sheet(w, h); var cape = Sheet(w, h); var torso = Sheet(w, h); var skirt = Sheet(w, h); var leg = Sheet(w, h); var arms = Sheet(w, h);
        var head = Sheet(w, h); var hairFront = Sheet(w, h); var belt = Sheet(w, h); var glow = Sheet(w, h);

        // the long golden hair, down to her hip, curling at the ends
        Fill(hairBack, Pts((Cx - 7.6f, 11f), (Cx + 7.6f, 11f), (Cx + 9.5f, 34f), (Cx + 10f, 52f), (Cx + 6f, 58f), (Cx + 3.5f, 48f), (Cx - 3.5f, 48f), (Cx - 6f, 62f), (Cx - 11f, 58f), (Cx - 10f, 34f)), Hair);
        Curls(hairBack, new (float x, float y)[] { (Cx - 10, 36), (Cx - 11, 43), (Cx - 10.5f, 50), (Cx - 9, 57), (Cx - 6, 61), (Cx + 9.5f, 38), (Cx + 10.5f, 45), (Cx + 9.5f, 52), (Cx + 6.5f, 57) }, 3f);

        // the skirt with the great slit and the leg, as Killa's, the toe just touching the water
        Fill(skirt, Pts((Cx - 5.5f, 40f), (Cx + 5.5f, 40f), (Cx + 9f, 56f), (Cx + 13f, 73.5f), (Cx - 13f, 73.5f), (Cx - 9f, 56f)), Skirt);
        Folds(skirt, 44, 72, Skirt, 6, 8);
        for (int x = Cx - 13; x <= Cx + 13; x++) Put(skirt, x, 73, Silver[2]);
        for (int y = 44; y < h; y++)
        {
            int edge = (int)MathF.Round(Cx - 2.5f + (y - 44) * 0.2f);
            for (int x = 0; x < edge; x++) if (skirt[x, y].A > 0) skirt[x, y] = P.Clear;
            if (skirt[edge, y].A > 0) Put(skirt, edge, y, y % 2 == 0 ? Silver[1] : Silver[2]);
        }
        Limb(leg, new[] { (Cx - 3.5f, 46f), (Cx - 5.5f, 57f), (Cx - 6.2f, 67f), (Cx - 6.6f, 72.5f) }, 3.4f, 1.5f, Skin);
        foreach (var (x, y) in new[] { (-8, 73), (-7, 73), (-6, 73), (-8, 74), (-7, 74), (-8, 75), (-8, 76) }) Put(leg, Cx + x, y, y >= 75 ? Skin[1] : Skin[0]);

        // the torso, bare at the midriff, the silver belt, the short cape with its brooch
        FillEllipse(torso, Cx, 20.4f, 2.1f, 2.6f, Skin);
        Fill(torso, Pts((Cx - 6f, 23f), (Cx + 6f, 23f), (Cx + 5f, 34f), (Cx + 5.6f, 41f), (Cx - 5.6f, 41f), (Cx - 5f, 34f)), Skin);
        for (int x = Cx - 6; x <= Cx + 6; x++) { Put(belt, x, 40, Silver[0]); Put(belt, x, 41, Silver[2]); }
        FillEllipse(belt, Cx - 2f, 40.6f, 1.6f, 1.6f, Silver);
        Fill(cape, Pts((Cx - 5f, 22f), (Cx + 5f, 22f), (Cx + 13f, 26f), (Cx + 17f, 33f), (Cx + 8f, 31f), (Cx, 29f), (Cx - 8f, 31f), (Cx - 17f, 33f), (Cx - 13f, 26f)), Cape);
        Folds(cape, 25, 32, Cape, 6, 5);
        FillEllipse(cape, Cx, 24.6f, 1.9f, 1.9f, Silver);

        // the arms: only two, close to her body (down her sides, the elbows at her ribs) and then in, the hands together in prayer on her chest
        Limb(arms, new[] { (Cx - 6f, 24.5f), (Cx - 7f, 32f), (Cx - 1.8f, 29.4f) }, 2f, 1.5f, Skin);
        Limb(arms, new[] { (Cx + 6f, 24.5f), (Cx + 7f, 32f), (Cx + 1.8f, 29.4f) }, 2f, 1.5f, Skin);
        FillEllipse(arms, Cx, 28.6f, 1.7f, 2.6f, Skin);
        for (int y = 26; y <= 31; y++) Put(arms, Cx, y, Skin[2]);                              // the line between the palms
        Put(arms, Cx - 1, 25, Skin[0]); Put(arms, Cx, 25, Skin[1]);

        // the head: pale, with pointed ears, emerald eyes, the hair parted over the brow (no ornament)
        FillEllipse(head, Cx - 5f, 16.6f, 1.1f, 2f, Skin);
        FillEllipse(head, Cx + 5f, 16.6f, 1.1f, 2f, Skin);
        Put(head, Cx - 6, 14, Skin[0]); Put(head, Cx + 6, 14, Skin[0]);
        FillEllipse(head, Cx, 16.6f, 4.7f, 5.6f, Skin);
        foreach (int ex in new[] { Cx - 3, Cx + 2 })
        {
            Put(head, ex, 16, EmeraldDark); Put(head, ex + 1, 16, EmeraldDark);
            Put(head, ex, 17, Emerald); Put(head, ex + 1, 17, EmeraldDark);
            Put(head, ex, 16, P.Hex(0xFFFFFF));
            Put(head, ex, 15, Skin[2]); Put(head, ex + 1, 15, Skin[2]);                         // the lid
        }
        Put(head, Cx, 19, Skin[2]);
        Put(head, Cx - 1, 21, P.Hex(0xC8605A)); Put(head, Cx, 21, P.Hex(0xC8605A));
        Put(head, Cx - 4, 19, P.Hex(0xF2A090)); Put(head, Cx + 4, 19, P.Hex(0xF2A090));
        Fill(hairFront, Pts((Cx - 5.6f, 13.4f), (Cx, 10.4f), (Cx + 5.6f, 13.4f), (Cx + 5.4f, 18f), (Cx + 3.4f, 14.6f), (Cx + 0.4f, 13.4f), (Cx - 1.6f, 14.8f), (Cx - 3.6f, 14f), (Cx - 5.4f, 21f)), Hair);

        FillEllipse(glow, Cx, 36f, 16f, 38f, new[] { P.Hex(0xFFF4C0, 34), P.Hex(0xFFF4C0, 34), P.Hex(0xFFE690, 18) });

        var fig = Sheet(w, h);
        AddOutlined(fig, hairBack, Outline); AddOutlined(fig, leg, Outline); AddOutlined(fig, skirt, Outline); AddOutlined(fig, torso, Outline); Blit(fig, belt); AddOutlined(fig, cape, Outline);
        AddOutlined(fig, arms, Outline); AddOutlined(fig, head, Outline); AddOutlined(fig, hairFront, Outline);
        var c = Sheet(w, h);
        for (int x = 8; x < w - 8; x++)
        {
            float u = (x - Cx) / 22f;
            if (MathF.Abs(u) >= 1) continue;
            int y = (int)MathF.Round(StandingFeet + 1 + MathF.Sqrt(1 - u * u) * 2f);
            if (c.Contains(x, y) && (x + y) % 2 == 0) c[x, y] = P.Hex(0xC8E0FF, 90);
        }
        Blit(c, glow);
        Blit(c, fig);
        return c;
    }
}
