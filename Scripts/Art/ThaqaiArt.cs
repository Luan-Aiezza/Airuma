using System;
using Godot;
using P = PixelColor;
using static PixelShapes;

/// <summary>
/// Thaqai, the cat of the stars, as a girl, on the fine grid of the other bosses (56 x 62 pixels, each shown 2 x 2 screen pixels). Everything about her
/// is taken from the cat of the myth, a dark grey cat with white stripes like lightning: the cat ears, the simple, short, sleeveless dress of the same dark
/// grey with a white bolt down it, a ruff of white fur at the neck and white paws for hands. Her hair is long, black and a little curly, and her eyes are a cat's,
/// pale green with a slit pupil. Her tail is great and full and white, with faint bolts across it, and it floats on the water, curled in a C: she LIES on it,
/// hugging it with both arms, her head on it and her knees drawn up, and floats along on it. Four frames: she breathes, the tip of the tail flicks, an ear
/// twitches and the bolts flash; rings spread on the water round the tail.
/// </summary>
public static class ThaqaiArt
{
    /// <summary>The picture is drawn on a taller canvas (`Canvas` rows) and cropped at the top (`Crop` rows are cut), since she lies low.</summary>
    private const int Canvas = 62, Crop = 8;
    public const int Width = 56, Height = Canvas - Crop;
    /// <summary>Art pixel that is the boss's position (the middle of her body).</summary>
    public static readonly (int x, int y) Centre = (28, 32);
    /// <summary>Row of the water surface, where the rings spread under the tail.</summary>
    public const int WaterRow = 49;

    private const float Axis = 28;
    private static readonly P Outline = P.Hex(0x0C0E16), TailOutline = P.Hex(0x46557A), TailBolt = P.Hex(0x8FA3CC);
    private static readonly P[] Fur = { P.Hex(0x6E7488), P.Hex(0x474C5E), P.Hex(0x2C303F), P.Hex(0x181B26) };            // the dark grey of the cat: light, base, shade, deep
    private static readonly P[] Fluff = { P.Hex(0xFFFFFF), P.Hex(0xEEF3FC), P.Hex(0xC4D2EC), P.Hex(0x97AACE) };            // the white of the tail: lit, base, shade, underside
    private static readonly P[] Bolt = { P.Hex(0xFFFFFF), P.Hex(0xDCE8FF), P.Hex(0xA8BCE0) };                              // the white stripes
    private static readonly P[] Skin = { P.Hex(0xF8E2D6), P.Hex(0xE6C2AE), P.Hex(0xB98F82) };
    private static readonly P[] Hair = { P.Hex(0x4A5472), P.Hex(0x171A24), P.Hex(0x0A0B10) };                              // glint, base, deep: black
    private static readonly P Iris = P.Hex(0xE2F24C), IrisDark = P.Hex(0x9AB82A), Pupil = P.Hex(0x08090E), Pink = P.Hex(0xE89AB4), PinkDark = P.Hex(0xA8607A);

    private static void Put(PixelCanvas c, int x, int y, P colour) => BossArt.Put(c, x, y, colour);
    private static P[] Ramp(P a, P b, P c) => new[] { a, b, c };
    private static float M(float x) => 2 * Axis - x;                  // `x` mirrored about the axis of the head

    /// <summary>`src` moved by (dx, dy).</summary>
    private static PixelCanvas Shifted(PixelCanvas src, int dx, int dy)
    {
        var c = new PixelCanvas(src.Width, src.Height);
        for (int y = 0; y < src.Height; y++) for (int x = 0; x < src.Width; x++) if (src[x, y].A > 0 && c.Contains(x + dx, y + dy)) c[x + dx, y + dy] = src[x, y];
        return c;
    }

    public static PixelCanvas MakeCanvas(int frame)
    {
        int breathe = frame == 2 || frame == 3 ? 1 : 0;
        float sway = new[] { 0f, 1f, 0f, -1f }[frame];                 // the tip of the tail
        bool flash = frame == 3;
        var rng = new SeededGenerator(0x7A9A1);
        var fur = Ramp(Fur[0], Fur[1], Fur[2]);

        // THE TAIL: a great, full white tail curled in a C on the water, from her hips round under her to the tip, which curls up at the left.
        var tail = new PixelCanvas(Width, Canvas);
        var white = Ramp(Fluff[1], Fluff[1], Fluff[1]);
        (float x, float y, float r)[] spine = { (47, 37, 3.6f), (52, 46, 5.5f), (43, 53, 6f), (27, 54, 6f), (12, 49, 5.5f), (6, 41, 4.2f), (7 + sway, 33.5f, 2.4f) };
        for (int i = 0; i < spine.Length - 1; i++)
            Limb(tail, new[] { (spine[i].x, spine[i].y), (spine[i + 1].x, spine[i + 1].y) }, spine[i].r, spine[i + 1].r, white);
        // the fur, lit from above: light along the top of the tube, shade along its underside
        var lit = new PixelCanvas(Width, Canvas);
        for (int y = 0; y < Canvas; y++)
            for (int x = 0; x < Width; x++)
            {
                if (tail[x, y].A == 0) continue;
                int up = 0, down = 0;
                while (y - up - 1 >= 0 && tail[x, y - up - 1].A > 0 && up < 6) up++;
                while (y + down + 1 < Canvas && tail[x, y + down + 1].A > 0 && down < 6) down++;
                float f = (Bayer.Threshold(x, y) - 0.5f) * 1.2f;
                lit[x, y] = up + f < 1.5f ? Fluff[0] : (down + f < 1.2f ? Fluff[3] : (down + f < 3.2f ? Fluff[2] : Fluff[1]));
            }
        tail = lit;
        // tufts: spikes of fur along the edge of the tail, and strands inside it
        var tufts = new PixelCanvas(Width, Canvas);
        for (int y = 0; y < Canvas; y++)
            for (int x = 0; x < Width; x++)
            {
                if (tail[x, y].A == 0) continue;
                bool top = !tail.Contains(x, y - 1) || tail[x, y - 1].A == 0, bottom = !tail.Contains(x, y + 1) || tail[x, y + 1].A == 0;
                bool side = !tail.Contains(x - 1, y) || tail[x - 1, y].A == 0 || !tail.Contains(x + 1, y) || tail[x + 1, y].A == 0;
                if (top && rng.Range(0, 3) == 0) { Put(tufts, x, y - 1, Fluff[0]); if (rng.Range(0, 2) == 0) Put(tufts, x + 1, y - 2, Fluff[1]); }
                else if (bottom && rng.Range(0, 3) == 0) { Put(tufts, x, y + 1, Fluff[3]); if (rng.Range(0, 2) == 0) Put(tufts, x - 1, y + 2, Fluff[2]); }
                else if (side && rng.Range(0, 5) == 0) Put(tufts, x + (tail.Contains(x + 1, y) && tail[x + 1, y].A > 0 ? -1 : 1), y, Fluff[1]);
                if (!top && !bottom && !side && rng.Range(0, 26) == 0) { Put(tufts, x, y, Fluff[3]); Put(tufts, x + 1, y + 1, Fluff[2]); Put(tufts, x + 2, y + 2, Fluff[3]); }   // a strand
            }
        Blit(tail, tufts);
        // faint bolts across the tail
        var stripes = new PixelCanvas(Width, Canvas);
        foreach (int sx in new[] { 17, 26, 36, 46 })
        {
            int top = -1, bottom = -1;
            for (int y = 40; y < Canvas; y++) if (tail[sx, y].A > 0) { if (top < 0) top = y; bottom = y; }
            if (top >= 0 && sx > 14 && tail[sx, 52].A > 0) ZigZag(stripes, sx, MathF.Max(top, 47), bottom - MathF.Max(top, 47) + 1, 0f, flash ? Bolt[2] : TailBolt);
        }
        ZigZag(stripes, 49f, 43f, 8, 0f, flash ? Bolt[2] : TailBolt, horizontal: true);

        var body = new PixelCanvas(Width, Canvas);      // the far arm, the dress, the legs
        var near = new PixelCanvas(Width, Canvas);      // the near arm and the paws, over the tail
        var skin = Ramp(Skin[0], Skin[1], Skin[2]);

        // The far arm, round the left bend of the tail, and the legs, drawn up.
        Limb(body, new[] { (25f, 41f), (17f, 45f), (13f, 48f) }, 2.1f, 1.8f, skin);
        Limb(body, new[] { (44f, 45f), (50.5f, 42.5f), (52f, 47f) }, 2.1f, 1.9f, skin);
        FillEllipse(body, 51.5f, 49f, 2.5f, 1.9f, Ramp(Bolt[0], Bolt[1], Bolt[2]));
        // The dress: simple and short and sleeveless, dark grey, over her body as she lies; the skirt over her hips, with a white bolt along it and down it.
        Limb(body, new[] { (27f, 42f), (43f, 43f) }, 5.2f, 5.4f, fur);
        Fill(body, Pts((38, 38), (47, 39), (49, 47), (37, 47)), fur);
        ZigZag(stripes, 30f, 41f, 11, 0f, flash ? Bolt[0] : Bolt[1], horizontal: true);
        ZigZag(stripes, 42f, 39f, 8, 0f, flash ? Bolt[0] : Bolt[1]);

        // The near arm, hugging the tail, and the white paw.
        Limb(near, new[] { (28f, 40f), (33f, 46f), (25f, 50f) }, 2.2f, 1.9f, skin);
        FillEllipse(near, 24.5f, 50.5f, 2.8f, 2.3f, Ramp(Bolt[0], Bolt[1], Bolt[2]));
        Put(near, 22, 52, Bolt[2]); Put(near, 24, 53, Bolt[2]);
        FillEllipse(near, 13.2f, 48.4f, 2.6f, 2.2f, Ramp(Bolt[0], Bolt[1], Bolt[2]));

        // THE HEAD, drawn upright and then laid on the tail: a round face, the ears, long, black, wavy hair, cat's eyes, the ruff.
        var headFace = new PixelCanvas(Width, Canvas);
        FillEllipse(headFace, Axis, 14, 6.6f, 7.2f, Skin);
        // The hair: long, black and a little curly: a cap with a few curls at the hairline, a short wavy lock at the near side of the face, and the long hair
        // streaming out behind her head, along her back, in waves that end in curls, with glints along it; the ends move a little.
        var hair = new PixelCanvas(Width, Canvas);
        FillEllipse(hair, Axis, 10f, 8.8f, 7.6f, Hair);
        float tipMove = sway * 0.9f;
        var strands = Ramp(Hair[0], Hair[1], Hair[2]);
        Fill(hair, Pts((19.5f, 7), (23.5f, 9), (22.5f, 21), (17f, 25), (16.5f, 15)), strands);
        Fill(hair, Pts((33, 5), (37, 6), (50, 8), (61, 10 + tipMove), (60, 14 + tipMove), (48, 15), (37, 13), (34, 10)), strands);
        Fill(hair, Pts((34, 9), (46, 13), (58, 17 + tipMove), (55, 20 + tipMove), (44, 18), (36, 15)), strands);
        // waves along the locks: bumps on the edge, alternating, and curls at the ends
        for (int x = 38; x < 60; x += 4)
        {
            float t = (x - 38) / 22f;
            FillEllipse(hair, x, 6.5f + t * 2f + (x % 8 == 2 ? -1.2f : 1.2f) + tipMove * t, 2.1f, 1.9f, strands);
            FillEllipse(hair, x + 2, 14f + t * 4f + (x % 8 == 2 ? 1.1f : -0.4f) + tipMove * t, 2.0f, 1.8f, strands);
        }
        FillEllipse(hair, 60f, 13f + tipMove, 2.6f, 2.6f, strands);
        FillEllipse(hair, 56f, 21f + tipMove, 2.6f, 2.6f, strands);
        FillEllipse(hair, 18f, 25f, 2.3f, 2.3f, strands);
        foreach (var (cx2, cy2) in new[] { (19f, 6f), (21f, 3.5f), (M(21f), 3.5f), (M(19f), 6f), (18f, 11f), (17.5f, 16f) }) FillEllipse(hair, cx2, cy2, 2.2f, 2.2f, strands);
        for (int k = 0; k < 3; k++)
            for (int x = 38; x < 58 - k; x++) if ((x + k * 2) % 3 != 0) Put(hair, x, 8 + (x - 38) * 4 / 18 + k * 2 + (int)MathF.Round(MathF.Sin(x * 0.7f + k) * 1.3f) + (int)(tipMove * (x - 38) / 18f), Hair[0]);
        foreach (var (gx, gy) in new[] { (22, 3), (26, 2), (30, 2), (34, 3), (18, 10), (17, 15), (19, 21) }) Put(hair, gx, gy, Hair[0]);
        float twitch = frame == 1 ? -1.4f : 0;
        var ears = new PixelCanvas(Width, Canvas);
        Fill(ears, Pts((18.5f, 9), (19f + twitch, -0.5f), (26.5f, 6)), fur);
        Fill(ears, Pts((M(18.5f), 9), (M(19f), -0.5f), (M(26.5f), 6)), fur);
        PixelPaint.Poly(ears, Pts((20.2f, 7.4f), (20.3f + twitch * 0.6f, 2.4f), (24.4f, 6)), Pink);
        PixelPaint.Poly(ears, Pts((M(20.2f), 7.4f), (M(20.3f), 2.4f), (M(24.4f), 6)), Pink);
        Put(ears, 20, 6, PinkDark); Put(ears, (int)M(20), 6, PinkDark);
        var earBolts = new PixelCanvas(Width, Canvas);
        ZigZag(earBolts, 21f + twitch * 0.4f, 1.5f, 5, 0.05f, Bolt[1]); ZigZag(earBolts, M(21f), 1.5f, 5, -0.05f, Bolt[1]);
        for (int y = 0; y < Canvas; y++) for (int x = 0; x < Width; x++) if (earBolts[x, y].A > 0 && !(ears[x, y].A > 0 && ears[x, y] != Pink)) earBolts[x, y] = P.Clear;
        // The fringe: bangs over the forehead, parted in the middle, in soft curls.
        var fringe = new PixelCanvas(Width, Canvas);
        Fill(fringe, Pts((21.5f, 6), (28f, 4.2f), (34.5f, 6), (35.5f, 11f), (32.5f, 9.4f), (28.5f, 10.8f), (27.5f, 10.8f), (23.5f, 9.4f), (20.5f, 11f)), strands);
        foreach (var (fx, fy) in new[] { (22.5f, 10.4f), (25.5f, 9.6f), (30.5f, 9.6f), (33.5f, 10.4f) }) FillEllipse(fringe, fx, fy, 1.9f, 1.8f, strands);
        for (int y = 6; y < 10; y++) { Put(fringe, 25, y, Hair[2]); Put(fringe, 31, y, Hair[2]); }
        Put(fringe, 28, 7, Hair[2]); Put(fringe, 28, 8, Hair[2]);
        foreach (var (gx, gy) in new[] { (23, 7), (33, 7), (26, 9), (30, 9) }) Put(fringe, gx, gy, Hair[0]);

        var face = new PixelCanvas(Width, Canvas);
        foreach (float ex in new[] { 24.5f, 31.5f })
        {
            int x0 = (int)ex - 1;
            for (int dx = 0; dx < 4; dx++) Put(face, x0 + dx, 13, Fur[3]);
            for (int dx = 0; dx < 3; dx++) for (int dy = 0; dy < 3; dy++) Put(face, x0 + dx, 14 + dy, dy == 2 ? IrisDark : Iris);
            for (int dy = 0; dy < 3; dy++) Put(face, x0 + 1, 14 + dy, Pupil);
            Put(face, ex < Axis ? x0 : x0 + 2, 14, P.Hex(0xFFFFFF));
            Put(face, ex < Axis ? x0 - 1 : x0 + 3, 13, Fur[3]);
        }
        Put(face, 27, 17, Pink); Put(face, 28, 17, Pink);
        Put(face, 26, 19, Skin[2]); Put(face, 27, 18, Skin[2]); Put(face, 28, 18, Skin[2]); Put(face, 29, 19, Skin[2]);
        foreach (int dy in new[] { 0, 2 }) { Put(face, 20, 16 + dy, Skin[2]); Put(face, 21, 16 + dy, Skin[2]); Put(face, 35, 16 + dy, Skin[2]); Put(face, 36, 16 + dy, Skin[2]); }
        var ruff = new PixelCanvas(Width, Canvas);
        FillEllipse(ruff, Axis, 21, 8.4f, 3.2f, Ramp(Bolt[0], Bolt[1], Bolt[2]));
        foreach (var (tx, ty) in new[] { (20, 22), (22, 24), (25, 24), (28, 24), (31, 24), (34, 24), (36, 22), (19, 21), (37, 21) }) Put(ruff, tx, ty, ty >= 24 ? Bolt[2] : Bolt[1]);
        var head = new PixelCanvas(Width, Canvas);
        AddOutlined(head, ruff, Outline);
        AddOutlined(head, hair, Outline);
        AddOutlined(head, headFace, Outline);
        AddOutlined(head, ears, Outline);
        Blit(head, earBolts);
        AddOutlined(head, fringe, Outline);
        Blit(head, face);
        // her head rests on the tail at the left, a little under her body's line
        head = Shifted(head, -8, 22 + breathe);

        // The bolts are cut to the dress and the tail.
        var cutBody = new PixelCanvas(Width, Canvas);
        var cutTail = new PixelCanvas(Width, Canvas);
        for (int y = 0; y < Canvas; y++)
            for (int x = 0; x < Width; x++)
                if (stripes[x, y].A > 0) { if (body[x, y].A > 0 && body[x, y] != skin[0] && body[x, y] != skin[1] && body[x, y] != skin[2]) cutBody[x, y] = stripes[x, y]; else if (tail[x, y].A > 0) cutTail[x, y] = stripes[x, y]; }

        // Put it together: the tail, then her, then the arm and the head over them.
        var fig = new PixelCanvas(Width, Canvas);
        AddOutlined(fig, tail, TailOutline);
        Blit(fig, cutTail);
        var lying = new PixelCanvas(Width, Canvas);
        Blit(lying, body);
        lying = Shifted(lying, 0, breathe);
        AddOutlined(fig, lying, Outline);
        Blit(fig, Shifted(cutBody, 0, breathe));
        AddOutlined(fig, Shifted(near, 0, breathe), Outline);
        Blit(fig, head);

        const int waterRow = WaterRow + Crop;
        var c = new PixelCanvas(Width, Canvas);
        PixelPaint.Ellipse(c, 28, waterRow + 0.5f, 26, 3.5f, new P(8, 10, 22, 90));          // the shadow on the water
        BossArt.DrawRipples(c, 28, waterRow, 27, 4.6f, frame, false);
        for (int y = 0; y < Canvas; y++) for (int x = 0; x < Width; x++) if (fig[x, y].A > 0) c[x, y] = fig[x, y];
        BossArt.DrawRipples(c, 28, waterRow, 27, 4.6f, frame, true);
        // the empty rows at the top are cut
        var cropped = new PixelCanvas(Width, Height);
        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) cropped[x, y] = c[x, y + Crop];
        return cropped;
    }

    /// <summary>A bolt of lightning: a zigzag of `length` px going down (or across, if `horizontal`) from a point, leaning a little.</summary>
    private static void ZigZag(PixelCanvas c, float x0, float y0, float length, float lean, P colour, bool horizontal = false)
    {
        float x = x0, y = y0;
        int dir = 1;
        for (float s = 0; s < length; s += 1)
        {
            Put(c, (int)MathF.Round(x), (int)MathF.Round(y), colour);
            if ((int)s % 4 == 3) dir = -dir;                     // each leg of the zigzag is four pixels long, then it turns
            if (horizontal) { x += 1; y += dir * 1.2f; }
            else { y += 1; x += dir * 1.25f + lean; }
        }
    }
}
