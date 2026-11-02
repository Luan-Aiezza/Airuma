using System;
using Godot;
using P = PixelColor;
using static PixelShapes;

/// <summary>
/// Ukhumo, the bear girl of the cave, on the fine grid of the other bosses (56 x 64 pixels, each shown 2 x 2 screen pixels). A wild, strong and feminine woman,
/// like a druid of the mountain, dressed in nothing but the fur of the bear: a top of fur on her breasts, a short of fur with a belt (and a blue stone) and a cloak of
/// fur round her shoulders and her back, boots that are the paws of a bear (claws and all), gloves that are two more paws, and, her mark, the head of the bear
/// worn on her head, with its ears and its muzzle on her brow. Her short, full, curly red hair frames her face; she has brown eyes. She is on her feet but
/// stooped, as if worn out by the hunger and the fight, with her shoulders low, her knees a little bent and her arms hanging, and yet she lifts her glare at the
/// one who faces her, with a defiant half-smile. She does not float like the others: she stands on the rock of her platform, so there are no ripples, only her
/// shadow. Four frames: she heaves with her breath, the cloak and the fur ruffle, the paws sway and the curls bounce.
/// </summary>
public static class UkhumoArt
{
    public const int Width = 56, Height = 64;
    /// <summary>Art pixel that is the boss's position (the middle of her chest).</summary>
    public static readonly (int x, int y) Centre = (28, 30);
    /// <summary>Row of her feet, where the platform is (the reflection and the shadow are about it).</summary>
    public const int WaterRow = 56;

    private const float Axis = 28;
    private static readonly P Outline = P.Hex(0x1C1008);
    private static readonly P[] Fur = { P.Hex(0xA87444), P.Hex(0x7A4E2A), P.Hex(0x4E2F17), P.Hex(0x2C1A0C) };         // lit, base, shade, deep
    private static readonly P[] PawPad = { P.Hex(0x6A4A44), P.Hex(0x3E2824), P.Hex(0x241614) };
    private static readonly P[] Skin = { P.Hex(0xF2C6A0), P.Hex(0xD9A07A), P.Hex(0xA8704E) };
    private static readonly P[] Hair = { P.Hex(0xF08A3A), P.Hex(0xC4521E), P.Hex(0x8A3010) };                           // lit, base, shade: red
    private static readonly P[] Hide = { P.Hex(0xC8A070), P.Hex(0x9A7448), P.Hex(0x6A4A2A) };                           // leather: the straps
    private static readonly P Claw = P.Hex(0xF4EAD0), ClawDark = P.Hex(0xB8A888), Eye = P.Hex(0x6A3A1C), EyeDark = P.Hex(0x3A1C0C);

    private static void Put(PixelCanvas c, int x, int y, P colour) => BossArt.Put(c, x, y, colour);
    private static P[] Ramp(P a, P b, P c) => new[] { a, b, c };
    private static float M(float x) => 2 * Axis - x;

    /// <summary>Ragged fur: pixels of the ramp stuck round the edge of what is drawn, here and there, so that the fur bristles.</summary>
    private static void Bristle(PixelCanvas c, SeededGenerator rng, P[] ramp, int chance = 3)
    {
        var copy = c.Clone();
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                if (copy[x, y].A == 0) continue;
                if (!copy.Contains(x, y + 1) || copy[x, y + 1].A == 0) { if (rng.Range(0, chance) == 0) Put(c, x, y + 1, ramp[2]); }
                else if (x > 0 && copy[x - 1, y].A == 0) { if (rng.Range(0, chance + 1) == 0) Put(c, x - 1, y, ramp[1]); }
                else if (x + 1 < c.Width && copy[x + 1, y].A == 0) { if (rng.Range(0, chance + 1) == 0) Put(c, x + 1, y, ramp[2]); }
            }
    }

    /// <summary>Strokes of darker fur on a furred part: short strokes pointing down, to show the fur lying.</summary>
    private static void Strokes(PixelCanvas c, SeededGenerator rng, int count, int minY, int maxY)
    {
        for (int i = 0; i < count; i++)
        {
            int x = rng.Range(0, c.Width), y = rng.Range(minY, maxY);
            if (!c.Contains(x, y) || c[x, y].A == 0) continue;
            if (c.Contains(x, y + 1) && c[x, y + 1].A > 0) { Put(c, x, y, Fur[2]); Put(c, x, y + 1, Fur[2]); }
        }
    }

    public static PixelCanvas MakeCanvas(int frame)
    {
        int breathe = frame == 2 || frame == 3 ? 1 : 0;
        float sway = new[] { 0f, 1f, 0f, -1f }[frame];                 // the paws, the cloak, the curls
        var rng = new SeededGenerator(0x0B3A5);
        var fur = Ramp(Fur[0], Fur[1], Fur[2]);
        var skin = Ramp(Skin[0], Skin[1], Skin[2]);
        var hair = Ramp(Hair[0], Hair[1], Hair[2]);

        // THE CLOAK: the mantle of fur on her back, which hangs behind her to the thighs, ragged at the hem; it moves a little.
        var cloak = new PixelCanvas(Width, Height);
        Fill(cloak, Pts((21, 24), (35, 24), (40 + sway, 34), (42 + sway * 1.2f, 46), (37, 44 - sway), (32, 48), (28, 45), (24, 48), (19, 44 + sway), (14 - sway * 1.2f, 46), (16 - sway, 34)), fur);
        Bristle(cloak, rng, fur, 2);
        Strokes(cloak, rng, 60, 26, 48);

        // THE LEGS, slim and shapely, with the knees a little bent; the boots are the paws of the bear: furry up the ankle, with the pad and the claws at the toes.
        var legs = new PixelCanvas(Width, Height);
        Limb(legs, new[] { (24.8f, 43f), (24.4f, 49f), (23.4f, 52.5f) }, 3.1f, 2.2f, skin);
        Limb(legs, new[] { (31.2f, 43f), (31.6f, 49f), (32.6f, 52.5f) }, 3.1f, 2.2f, skin);
        var boots = new PixelCanvas(Width, Height);
        foreach (float bx in new[] { 22.5f, 33.5f })
        {
            FillEllipse(boots, bx, 53.9f, 5.0f, 3.2f, fur);
            FillEllipse(boots, bx, 50.4f, 3.6f, 2.4f, fur);                                    // the fur at the ankle
            FillEllipse(boots, bx + (bx < Axis ? -0.5f : 0.5f), 55.8f, 2.8f, 1.1f, Ramp(PawPad[0], PawPad[1], PawPad[2]));       // the pad of the paw
            for (int k = -1; k <= 1; k++) { Put(boots, (int)bx + k * 2, 57, Claw); Put(boots, (int)bx + k * 2, 58, ClawDark); }  // three claws at the toes
        }
        Bristle(boots, rng, fur, 2);

        // THE SHORT, of fur, with a wrap of fur over one hip, and a belt of leather with a blue stone.
        var shorts = new PixelCanvas(Width, Height);
        Fill(shorts, Pts((22.5f, 37), (33.5f, 37), (36.5f, 42), (34, 45.5f), (29, 44), (28, 45.5f), (27, 44), (22, 45.5f), (19.5f, 42)), fur);
        Fill(shorts, Pts((28, 37), (34.5f, 37), (37, 41), (36.5f + sway * 0.5f, 47), (33.5f, 44.5f), (31.5f, 41)), Ramp(Fur[0], Fur[0], Fur[1]));         // the wrap over the right hip
        Bristle(shorts, rng, fur, 2);
        Strokes(shorts, rng, 24, 38, 45);
        for (int x = 22; x < 34; x++) { Put(shorts, x, 37, Hide[1]); Put(shorts, x, 38, x % 2 == 0 ? Hide[2] : Hide[1]); }                  // the belt
        Put(shorts, 27, 37, P.Hex(0x4FD0C0)); Put(shorts, 28, 37, P.Hex(0x9AF0E0)); Put(shorts, 27, 38, P.Hex(0x2A8C84)); Put(shorts, 28, 38, P.Hex(0x4FD0C0));   // the stone

        // THE TORSO: the neck, the slim waist and bare midriff, and the top of fur on her breasts.
        int lift = breathe;
        var torso = new PixelCanvas(Width, Height);
        Fill(torso, Pts((26, 21), (30, 21), (31, 25), (25, 25)), skin);
        Fill(torso, Pts((23, 25 - lift), (33, 25 - lift), (32, 31), (31.5f, 37), (24.5f, 37), (24, 31)), skin);
        var top = new PixelCanvas(Width, Height);
        Fill(top, Pts((22.5f, 25.5f - lift), (33.5f, 25.5f - lift), (33, 30 - lift), (30.5f, 31.2f - lift), (28, 29.5f - lift), (25.5f, 31.2f - lift), (23, 30 - lift)), fur);
        Bristle(top, rng, fur, 3);
        // the mantle of fur over her shoulders, which droop
        var mantle = new PixelCanvas(Width, Height);
        Fill(mantle, Pts((19.2f, 25.5f - lift), (24.5f, 22.5f - lift), (31.5f, 22.5f - lift), (36.8f, 25.5f - lift), (38.5f, 30), (33.5f, 28 - lift), (28, 28.6f - lift), (22.5f, 28 - lift), (17.5f, 30)), fur);
        Bristle(mantle, rng, fur, 2);
        Strokes(mantle, rng, 26, 23, 30);

        // THE ARMS, slim and strong, hanging forward from the drooping shoulders; the gloves are two paws of the bear, with a long cuff of fur and the claws.
        var arms = new PixelCanvas(Width, Height);
        Limb(arms, new[] { (22.2f, 27 - lift), (17.5f, 33.5f), (15.8f + sway * 0.5f, 40f) }, 2.4f, 1.9f, skin);
        Limb(arms, new[] { (33.8f, 27 - lift), (38.5f, 33.5f), (40.2f - sway * 0.5f, 40f) }, 2.4f, 1.9f, skin);
        var gloves = new PixelCanvas(Width, Height);
        foreach (var (gx, gy) in new[] { (15.8f + sway * 0.5f, 44.6f), (40.2f - sway * 0.5f, 44.6f) })
        {
            FillEllipse(gloves, gx, gy, 4.2f, 4.2f, fur);
            FillEllipse(gloves, gx, gy - 3.6f, 3.2f, 2.5f, fur);                              // the long cuff of fur
            int cx = (int)MathF.Round(gx);
            for (int k = -1; k <= 1; k++) { Put(gloves, cx + k * 2, (int)gy + 5, Claw); Put(gloves, cx + k * 2, (int)gy + 6, ClawDark); }
            FillEllipse(gloves, gx, gy + 1.6f, 2f, 1.4f, Ramp(PawPad[0], PawPad[1], PawPad[2]));
        }
        Bristle(gloves, rng, fur, 2);

        // THE HEAD, drawn upright and then hung a little low and to the side. First her hair: short, full and curly, red, in curls round the face and to the shoulders.
        var hairBack = new PixelCanvas(Width, Height);
        FillEllipse(hairBack, Axis, 12.5f, 9.6f, 8.6f, hair);
        foreach (var (hx, hy) in new[] { (19f, 10f), (M(19f), 10f), (17.4f, 15f), (M(17.4f), 15f), (18f, 20f), (M(18f), 20f), (19.6f, 25f), (M(19.6f), 25f), (21f, 22.5f), (M(21f), 22.5f) })
            FillEllipse(hairBack, hx + sway * 0.4f * (hy / 25f), hy, 3.1f, 3.1f, hair);
        Bristle(hairBack, rng, hair, 2);
        // her face: delicate, with a pointed chin
        var face = new PixelCanvas(Width, Height);
        FillEllipse(face, Axis, 15f, 5.3f, 5.6f, skin);
        Fill(face, Pts((23.2f, 16), (32.8f, 16), (30.2f, 21.8f), (25.8f, 21.8f)), skin);
        // the bear's head, worn on top of hers: the dome of the skull over her crown, its ears, its muzzle pointing over her brow, and fur falling behind her ears
        var bear = new PixelCanvas(Width, Height);
        FillEllipse(bear, Axis, 6.8f, 8.2f, 4.6f, fur);
        Fill(bear, Pts((19.6f, 6), (22.4f, 8.5f), (22f, 12.5f), (19.2f, 12.5f), (18f, 9)), fur);
        Fill(bear, Pts((M(19.6f), 6), (M(22.4f), 8.5f), (M(22f), 12.5f), (M(19.2f), 12.5f), (M(18f), 9)), fur);
        Bristle(bear, rng, fur, 2);
        Strokes(bear, rng, 24, 2, 12);
        var ears = new PixelCanvas(Width, Height);
        foreach (float ex in new[] { 20.4f, M(20.4f) })
        {
            FillEllipse(ears, ex, 3.2f, 3.2f, 3.2f, fur);
            FillEllipse(ears, ex + (ex < Axis ? 0.3f : -0.3f), 3.6f, 1.7f, 1.7f, Ramp(Hide[1], Fur[2], Fur[3]));        // the inside of the ear
        }
        var muzzle = new PixelCanvas(Width, Height);
        FillEllipse(muzzle, Axis, 9.2f, 4.0f, 2.6f, Ramp(Hide[0], Hide[1], Hide[2]));                        // the muzzle of the bear, pale
        FillEllipse(muzzle, Axis, 10.3f, 1.6f, 1.0f, Ramp(PawPad[1], PawPad[2], PawPad[2]));                    // its nose
        foreach (float ex in new[] { 24f, M(24f) }) { Put(muzzle, (int)ex, 6, EyeDark); Put(muzzle, (int)ex, 7, Fur[3]); }                   // the bear's own eyes, shut
        // curls of her hair over the forehead, under the muzzle
        var fringe = new PixelCanvas(Width, Height);
        foreach (var (hx, hy) in new[] { (22.2f, 12.2f), (25.2f, 12.9f), (30.8f, 12.9f), (33.8f, 12.2f) }) FillEllipse(fringe, hx + sway * 0.2f, hy, 1.8f, 1.6f, hair);
        // the face: brown eyes, heavy-lidded with tiredness, looking up from under a slanting brow; a small bag under each; and a defiant half-smile, one corner higher
        var details = new PixelCanvas(Width, Height);
        foreach (float ex in new[] { 25f, 31f })
        {
            int x0 = (int)ex - 1;
            for (int dx = 0; dx < 4; dx++) Put(details, x0 + dx, 13 + (ex < Axis ? (dx == 0 ? 1 : 0) : (dx == 3 ? 1 : 0)), Hair[2]);      // the brow, slanting down to the nose
            for (int dx = 0; dx < 3; dx++) { Put(details, x0 + dx, 14, Fur[3]); Put(details, x0 + dx, 15, dx == 1 ? EyeDark : Eye); }     // the lid, the eye
            Put(details, ex < Axis ? x0 : x0 + 2, 15, P.Hex(0xFFFFFF));
            Put(details, x0 + 1, 17, Skin[2]);                                                         // the bag under the eye
        }
        Put(details, 27, 17, Skin[2]); Put(details, 28, 17, Skin[2]);                                      // the nose
        for (int x = 25; x <= 30; x++) Put(details, x, 19, P.Hex(0x8A3030));                               // the mouth
        Put(details, 31, 18, P.Hex(0x8A3030)); Put(details, 32, 18, P.Hex(0x8A3030)); Put(details, 24, 19, P.Hex(0x8A3030));
        if (breathe == 1) { Put(details, 27, 20, P.Hex(0x4A1818)); Put(details, 28, 20, P.Hex(0x4A1818)); }   // she pants
        foreach (var (fx, fy) in new[] { (23, 17), (33, 17), (24, 18) }) Put(details, fx, fy, Skin[2]);        // freckles

        // PUT IT TOGETHER, back to front, each part with its outline. The head is hung a little low and to the side, as she is stooped.
        var head = new PixelCanvas(Width, Height);
        AddOutlined(head, hairBack, Outline);
        AddOutlined(head, face, Outline);
        AddOutlined(head, ears, Outline);
        AddOutlined(head, bear, Outline);
        AddOutlined(head, muzzle, Outline);
        Blit(head, fringe);
        Blit(head, details);
        int headDrop = 3 + breathe;
        const int headSide = 1;
        var lowered = new PixelCanvas(Width, Height);
        for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++) if (head[x, y].A > 0 && lowered.Contains(x + headSide, y + headDrop)) lowered[x + headSide, y + headDrop] = head[x, y];

        var fig = new PixelCanvas(Width, Height);
        AddOutlined(fig, cloak, Outline);
        AddOutlined(fig, legs, Outline);
        AddOutlined(fig, boots, Outline);
        AddOutlined(fig, shorts, Outline);
        AddOutlined(fig, torso, Outline);
        AddOutlined(fig, top, Outline);
        AddOutlined(fig, mantle, Outline);
        AddOutlined(fig, arms, Outline);
        AddOutlined(fig, gloves, Outline);
        Blit(fig, lowered);

        var c = new PixelCanvas(Width, Height);
        PixelPaint.Ellipse(c, 28, WaterRow + 1.5f, 19, 3.2f, new P(8, 6, 4, 110));          // her shadow on the rock
        Blit(c, fig);
        return c;
    }
}
