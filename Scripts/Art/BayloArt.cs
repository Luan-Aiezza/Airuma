using System;
using System.Collections.Generic;
using Godot;
using P = PixelColor;
using static PixelShapes;

/// <summary>
/// Baylo in his human form, on the same fine grid as the woman (a figure about 48 x 58 pixels, each shown 2 x 2 screen pixels): a dancer
/// in a white suit and a white hat tipped down over his face (the brim hides it, only the chin shows), with dark curly hair at the
/// sides, a pale blue shirt and a dark tie, white gloves and black shoes over white socks. His pose is a dance step: one foot forward,
/// the other on its toes, one hand holding the brim of the hat and the other thrown out to the side. He dances on the water, and rings
/// spread from under his feet. Four frames: he bobs, the hand taps the brim, the jacket sways. The scene turns him on the spot while
/// he moves (by squeezing and flipping the sprite), so the canvas holds only the front view.
/// </summary>
public static class BayloArt
{
    public const int Width = 48, Height = 60;
    /// <summary>Art pixel that is the boss's position (the middle of his chest).</summary>
    public static readonly (int x, int y) Centre = (24, 27);
    /// <summary>Row of the water surface, where the rings spread under his feet.</summary>
    public const int WaterRow = 52;

    private static readonly P Outline = P.Hex(0x1C1026);
    private static readonly P[] White = { P.Hex(0xFFFFFF), P.Hex(0xE6E9F4), P.Hex(0xB9C0DA), P.Hex(0x838CB4) };   // lit, mid, shade, deep shade
    private static readonly P[] Skin = { P.Hex(0xFBE3CF), P.Hex(0xEDC4A6), P.Hex(0xC8967C) };   // a pale skin: lit, mid, shade
    private static readonly P[] Shirt = { P.Hex(0xBDE2FA), P.Hex(0x8FC8F0), P.Hex(0x5C98CC) };
    private static readonly P[] Curl = { P.Hex(0x1C100C), P.Hex(0x3A2418), P.Hex(0x6A4630) };   // dark brown curls: shade, mid, glint
    private static readonly P[] Shoe = { P.Hex(0x0E0A12), P.Hex(0x2A2234), P.Hex(0x6A5A84) };
    private static readonly P Band = P.Hex(0x14101C), Tie = P.Hex(0x1A1420), Pink = P.Hex(0xE0508A), Gold = P.Hex(0xFFD060);
    private static readonly P Shade = P.Hex(0x1E1228);

    private static void Put(PixelCanvas c, int x, int y, P colour) => BossArt.Put(c, x, y, colour);

    public static PixelCanvas MakeCanvas(int frame)
    {
        int bob = frame == 2 || frame == 3 ? 1 : 0;
        int tap = frame == 1 ? -1 : (frame == 3 ? 1 : 0);        // the hand on the brim taps
        float sway = frame == 1 ? 1 : (frame == 3 ? -1 : 0);     // the hem of the jacket

        var behind = new PixelCanvas(Width, Height);   // the far arm and back leg
        var body = new PixelCanvas(Width, Height);     // torso, near leg, neck
        var front = new PixelCanvas(Width, Height);    // hat

        // The back leg, on its toes: long and thin.
        Limb(behind, new[] { (22f, 36f), (19.5f, 44f), (17.5f, 49f) }, 2.3f, 1.7f, White);
        // The arm thrown out to the side.
        Limb(behind, new[] { (31f, 21f), (37f, 26f), (42f + sway * 0.5f, 31f) }, 2.0f, 1.5f, White);

        // The jacket: narrow shoulders, a slim body, a hem that barely flares.
        Fill(body, Pts((17, 20), (24, 19), (31, 20), (31.5f, 28), (29.5f + sway, 38), (24, 39), (18.5f - sway, 38), (16.5f, 28)), White);
        // The front leg, stepping forward.
        Limb(body, new[] { (26f, 37f), (29.5f, 43f), (32.5f, 49.5f) }, 2.4f, 1.9f, White);
        // The shirt and the tie in the opening of the jacket.
        PixelPaint.Poly(body, Pts((21.5f, 20.5f), (26.5f, 20.5f), (24, 32)), Shirt[1]);
        for (int y = 21; y < 31; y++) Put(body, 24, y, Tie);
        Put(body, 23, 28, Tie); Put(body, 23, 29, Tie); Put(body, 24, 31, Tie);
        // Lapels: a bright edge and a shaded one, running from the collar to the button.
        for (int i = 0; i < 11; i++) { Put(body, 21 + i / 4, 21 + i, White[0]); Put(body, 27 - i / 4, 21 + i, White[2]); }
        Put(body, 24, 33, Gold); Put(body, 24, 36, Gold);
        // The pocket square, pink.
        Put(body, 28, 27, Pink); Put(body, 29, 27, Pink); Put(body, 29, 26, Pink);
        // The face under the brim: the eyes are in its shadow (drawn with the hat), then the nose, the mouth and the chin, lit from the left.
        PixelPaint.Rect(body, 21, 15, 26, 18, Skin[1]);
        PixelPaint.Rect(body, 22, 19, 25, 19, Skin[1]);
        PixelPaint.Rect(body, 22, 20, 25, 20, Skin[2]);                                        // the neck
        PixelPaint.Rect(body, 21, 15, 22, 18, Skin[0]);                                        // the lit cheek
        PixelPaint.Rect(body, 25, 16, 26, 18, Skin[2]);                                        // the shaded cheek
        Put(body, 24, 17, Skin[0]); Put(body, 24, 18, Skin[0]); Put(body, 25, 18, Skin[2]);    // the nose and its shadow
        Put(body, 23, 19, P.Hex(0xB8665E)); Put(body, 24, 19, P.Hex(0xB8665E));                // the mouth

        // The curls are in front of the raised arm and under the hat.
        var curls = new PixelCanvas(Width, Height);
        foreach (var (cx, cy) in new[] { (18, 15), (17, 18), (19, 21), (30, 15), (31, 18), (29, 21) })
        {
            PixelPaint.Ellipse(curls, cx, cy, 1.9f, 1.8f, Curl[1]);
            Put(curls, cx - 1, cy - 1, Curl[2]); Put(curls, cx + 1, cy + 1, Curl[0]);
        }

        // The hat: a crown with a dark band, and a brim tipped forward (low on the left, so that it hangs over the face).
        Fill(front, Pts((20.5f, 4.5f), (24, 3.5f), (27.5f, 4.5f), (28, 9), (20, 9)), White);
        PixelPaint.Rect(front, 20, 7, 28, 8, Band);
        Put(front, 26, 7, Pink); Put(front, 27, 7, Pink); Put(front, 27, 8, Pink);                  // a little feather at the band
        for (int y = 5; y <= 6; y++) Put(front, 24, y, White[2]);                                  // the pinch in the crown
        Fill(front, Pts((14, 12.5f), (16, 10), (24, 9.5f), (32, 10), (34, 11.5f), (32, 13.5f), (24, 14.5f), (16, 15.5f)), White);
        for (int x = 16; x < 32; x++) Put(front, x, 14 + (x < 20 ? 1 : 0), x < 24 ? White[3] : White[2]); // the brim's lower edge
        // Under the brim, the face is in shadow.
        PixelPaint.Rect(front, 21, 15, 26, 16, Shade);                                              // the eyes, in the shadow of the brim

        // The hand that holds the brim (raised, bent at the elbow) and the white glove.
        var arm = new PixelCanvas(Width, Height);
        Limb(arm, new[] { (18f, 21f), (12f, 24.5f), (14.5f, 16f + tap) }, 2.0f, 1.7f, White);
        PixelPaint.Ellipse(arm, 14.5f, 12.5f + tap, 2.0f, 1.9f, White[0]);
        Put(arm, 13, 12 + tap, P.Hex(0xFFFFFF)); Put(arm, 15, 14 + tap, White[2]);
        // The open glove at the end of the other arm, with its fingers spread.
        PixelPaint.Ellipse(behind, 42.5f + sway * 0.5f, 32, 1.9f, 2.1f, White[0]);
        foreach (var (dx, dy) in new[] { (1, 3), (2, 3), (3, 2), (3, 0) }) Put(behind, 41 + dx + (int)(sway * 0.5f), 31 + dy, White[1]);

        // Socks and shoes: the front foot flat and pointing out, the back foot on its toes.
        var shoes = new PixelCanvas(Width, Height);
        PixelPaint.Rect(shoes, 31, 48, 33, 49, White[1]);
        PixelPaint.Poly(shoes, Pts((29, 50), (35, 50), (38, 52), (37, 53.5f), (29, 53.5f)), Shoe[0]);
        Put(shoes, 31, 51, Shoe[2]); Put(shoes, 32, 51, Shoe[2]); Put(shoes, 35, 52, Shoe[1]);
        PixelPaint.Rect(shoes, 16, 46, 18, 47, White[1]);
        PixelPaint.Poly(shoes, Pts((13.5f, 48), (18, 47), (19.5f, 51), (16, 53), (13.5f, 51.5f)), Shoe[0]);
        Put(shoes, 15, 49, Shoe[2]); Put(shoes, 14, 51, Shoe[1]);

        // Compose part by part, each with its own outline, so that the arms and legs stand out from the body.
        var fig = new PixelCanvas(Width, Height);
        void Add(PixelCanvas part) => AddOutlined(fig, part, Outline);
        Add(behind); Add(shoes); Add(body); Add(arm); Add(curls); Add(front);

        var c = new PixelCanvas(Width, Height);
        PixelPaint.Ellipse(c, 24, WaterRow + 1, 13, 3, new P(10, 8, 24, 90));          // his shadow on the water
        BossArt.DrawRipples(c, 24, WaterRow, 18, 4.2f, frame, false);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (fig[x, y].A > 0 && y + bob < Height) c[x, y + bob] = fig[x, y];
        BossArt.DrawRipples(c, 24, WaterRow, 18, 4.2f, frame, true);
        return c;
    }
}
