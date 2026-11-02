using System;
using System.Collections.Generic;
using Godot;
using P = PixelColor;
using static PixelShapes;

/// <summary>
/// The common enemies of stage 6, the river of stars in the realm of Killa: beings of the sky of the Andes, each drawn on a 22 x 24 grid (44 x 48 on the screen):
///   Swarm  the celestial ñusta: a little princess of the sky, a winged angel of the Inca myths, with a ring of light over her head, black braids, a crimson unku (tunic) marked with
///          the chakana (the stepped cross) and small wings of white feathers that beat (up, level, down);
///   Gunner the Apu Kuntur: the sacred condor, messenger of the mountain gods to the sky, with great black wings edged with white, a white ruff, a bare red head, a golden sun on its
///          breast and a halo of light, its wings beating;
///   Heavy  the new moon: a dark round body with craters, edged by a bright silver crescent, one great pale eye like an eclipse, a corona of spikes and three small
///          stars that circle it.
/// </summary>
public static class CelestialSpiritArt
{
    public const int Width = 22, Height = 24, FrameCount = 3;

    private static readonly P Outline = P.Hex(0x0A0818);

    /// <summary>The colour of the sparks when a spirit is defeated.</summary>
    public static P SparkColour(SpiritKind kind) => kind switch
    {
        SpiritKind.Swarm => P.Hex(0xFFE890),
        SpiritKind.Gunner => P.Hex(0xA8F0FF),
        _ => P.Hex(0xC8B8FF),
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
            case SpiritKind.Swarm: CelestialNusta(back, body, front, glow, top, frame); break;
            case SpiritKind.Gunner: ApuKuntur(back, body, front, glow, top, frame); break;
            default: NewMoon(back, body, front, glow, top, frame); break;
        }
        var fig = new PixelCanvas(Width, Height);
        foreach (var part in new[] { back, body, front }) { PixelPaint.Outline(part, Outline); PixelPaint.Composite(fig, part); }
        var c = new PixelCanvas(Width, Height);
        PixelPaint.Composite(c, glow);
        PixelPaint.Composite(c, fig);
        PixelPaint.Composite(c, top);
        return c;
    }

    /// <summary>A halo: dithered discs, fainter outwards.</summary>
    private static void Halo(PixelCanvas glow, float cx, float cy, float r, P colour)
    {
        for (int y = 0; y < glow.Height; y++)
            for (int x = 0; x < glow.Width; x++)
            {
                float d = MathF.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / r;
                if (d < 1 && (x + y) % 2 == 0) glow[x, y] = colour.WithAlpha((byte)(30 + 60 * (1 - d)));
            }
    }

    // MARK: The celestial ñusta

    private static void CelestialNusta(PixelCanvas back, PixelCanvas body, PixelCanvas front, PixelCanvas glow, PixelCanvas top, int frame)
    {
        P[] white = { P.Hex(0xFFFFFF), P.Hex(0xDCE6F8), P.Hex(0x9AA8C4) };
        P[] crimson = { P.Hex(0xE86250), P.Hex(0xB62A30), P.Hex(0x7A1A24) };
        P[] skin = { P.Hex(0xF4CDA4), P.Hex(0xDDA878), P.Hex(0xB8805A) };
        P[] hair = { P.Hex(0x4A4258), P.Hex(0x2C2234), P.Hex(0x120C1A) };
        P[] gold = { P.Hex(0xFFF0A8), P.Hex(0xF2C14E), P.Hex(0xB8862B) };
        float cx = 10.5f;
        float tip = new[] { 3f, 8.5f, 14f }[frame];                       // the wings up, level and down
        float bob = frame == 1 ? 1 : 0;
        // the small wings of white feathers
        foreach (int side in new[] { -1, 1 })
        {
            Fill(back, Pts((cx + side * 2f, 12f + bob), (cx + side * 9.8f, tip), (cx + side * 9f, tip + 4.5f), (cx + side * 6f, tip + 7f), (cx + side * 2f, 16f + bob)), white);
            for (int k = 0; k < 3; k++) PixelPaint.Plot(back, (int)(cx + side * (5 + k * 1.6f)), (int)(tip + 2 + k * 1.8f + (12 - tip) * (1 - (5 + k * 1.6f) / 9.8f) * 0), P.Hex(0xB8C4E0));
        }
        Halo(glow, cx, 13f + bob, 10f, P.Hex(0xFFE890));
        // the unku: a crimson tunic to the knee, a gold band at the hem and the chakana on the chest
        Fill(body, Pts((cx - 3f, 13f + bob), (cx + 3f, 13f + bob), (cx + 4.4f, 20f + bob), (cx - 4.4f, 20f + bob)), crimson);
        for (int x = (int)(cx - 4); x <= (int)(cx + 4); x++) { PixelPaint.Plot(front, x, (int)(19 + bob), gold[1]); PixelPaint.Plot(front, x, (int)(20 + bob), gold[2]); }
        PixelPaint.Plot(front, 10, (int)(15 + bob), gold[0]); PixelPaint.Plot(front, 10, (int)(16 + bob), gold[0]); PixelPaint.Plot(front, 10, (int)(17 + bob), gold[0]);
        PixelPaint.Plot(front, 9, (int)(16 + bob), gold[0]); PixelPaint.Plot(front, 11, (int)(16 + bob), gold[0]);
        // the little arms, open
        PixelPaint.Plot(front, 6, (int)(15 + bob), skin[1]); PixelPaint.Plot(front, 15, (int)(15 + bob), skin[1]);
        PixelPaint.Plot(front, 5, (int)(14 + bob), skin[0]); PixelPaint.Plot(front, 16, (int)(14 + bob), skin[0]);
        // the head: black hair with two braids, a calm face
        FillEllipse(body, cx, 9.4f + bob, 3.3f, 3.5f, hair);
        FillEllipse(body, cx, 10.2f + bob, 2.5f, 2.7f, skin);
        PixelPaint.Plot(front, 9, (int)(10 + bob), P.Hex(0x120C1A)); PixelPaint.Plot(front, 12, (int)(10 + bob), P.Hex(0x120C1A));
        PixelPaint.Plot(front, 10, (int)(12 + bob), P.Hex(0xB8602C)); PixelPaint.Plot(front, 11, (int)(12 + bob), P.Hex(0xB8602C));
        for (int y = 11; y <= 15; y++) { PixelPaint.Plot(back, 6, (int)(y + bob), hair[1]); PixelPaint.Plot(back, 15, (int)(y + bob), hair[1]); }
        PixelPaint.Plot(front, 6, (int)(16 + bob), crimson[0]); PixelPaint.Plot(front, 15, (int)(16 + bob), crimson[0]);
        // the ring of light over her head, and a thread of light below her
        for (int k = 0; k < 24; k++)
        {
            float a = k * MathF.PI * 2 / 24;
            PixelPaint.Plot(top, (int)MathF.Round(cx + MathF.Cos(a) * 4.6f), (int)MathF.Round(4.6f + bob + MathF.Sin(a) * 1.5f), gold[k % 4 == 0 ? 0 : 1]);
        }
        for (int i = 0; i < 4; i++) PixelPaint.Plot(top, 10 + (i % 2), (int)(21 + bob + i), P.Hex(0xFFE890, (byte)(150 - i * 35)));
    }

    // MARK: The Apu Kuntur

    private static void ApuKuntur(PixelCanvas back, PixelCanvas body, PixelCanvas front, PixelCanvas glow, PixelCanvas top, int frame)
    {
        P[] black = { P.Hex(0x4A4458), P.Hex(0x26202E), P.Hex(0x100C16) };
        P[] white = { P.Hex(0xFFFFFF), P.Hex(0xDCE6F8), P.Hex(0x9AA8C4) };
        P[] gold = { P.Hex(0xFFF0A8), P.Hex(0xF2C14E), P.Hex(0xB8862B) };
        float cx = 10.5f;
        float tip = new[] { 2f, 8f, 13.5f }[frame];
        float bob = frame == 1 ? 1 : 0;
        foreach (int side in new[] { -1, 1 })
        {
            // the great wing, black, with the long fingers of its tip
            Fill(back, Pts((cx + side * 2f, 10f + bob), (cx + side * 10.6f, tip), (cx + side * 9.4f, tip + 3.5f), (cx + side * 6.5f, tip + 6.5f), (cx + side * 2f, 16f + bob)), black);
            // the white band of the secondary feathers
            Fill(front, Pts((cx + side * 3f, 12f + bob), (cx + side * 8f, tip + 2.8f), (cx + side * 6.4f, tip + 6f), (cx + side * 3f, 15f + bob)), white);
            for (int k = 0; k < 3; k++) PixelPaint.Plot(front, (int)(cx + side * (4.5f + k * 1.6f)), (int)(tip + 3.5f + k * 1.2f + (tip < 8 ? 0 : 0)), black[2]);
        }
        Halo(glow, cx, 12f, 11f, P.Hex(0xFFE890));
        // the tail, dark with white tips
        Fill(body, Pts((cx - 2f, 17f + bob), (cx + 2f, 17f + bob), (cx + 3.2f, 22f + bob), (cx - 3.2f, 22f + bob)), black);
        for (int x = (int)(cx - 3); x <= (int)(cx + 3); x++) PixelPaint.Plot(front, x, (int)(22 + bob), white[1]);
        // the body, the white ruff round the neck and the bare red head with its hooked beak
        FillEllipse(body, cx, 14f + bob, 2.7f, 4.4f, black);
        FillEllipse(body, cx, 10.8f + bob, 3.3f, 1.7f, white);
        FillEllipse(body, cx, 8f + bob, 2.1f, 2.4f, new[] { P.Hex(0xE8786A), P.Hex(0xB8403A), P.Hex(0x7A2024) });
        PixelPaint.Plot(front, 9, (int)(7 + bob), P.Hex(0xFFF0A8)); PixelPaint.Plot(front, 12, (int)(7 + bob), P.Hex(0xFFF0A8));
        PixelPaint.Plot(front, 10, (int)(9 + bob), P.Hex(0xE8C860)); PixelPaint.Plot(front, 11, (int)(9 + bob), P.Hex(0xE8C860)); PixelPaint.Plot(front, 10, (int)(10 + bob), P.Hex(0x7A5A20));
        // the crest on its head
        PixelPaint.Plot(front, 10, (int)(5 + bob), P.Hex(0xB8403A)); PixelPaint.Plot(front, 11, (int)(5 + bob), P.Hex(0xB8403A));
        // the sun of gold on its breast, with little rays
        PixelPaint.Ellipse(front, cx, 14.4f + bob, 1.7f, 1.7f, gold[1]);
        PixelPaint.Plot(front, 10, (int)(14 + bob), gold[0]);
        foreach (var (dx, dy) in new[] { (0, -3), (0, 3), (-3, 0), (3, 0) }) PixelPaint.Plot(front, (int)(cx - 0.5f) + dx, (int)(14 + bob) + dy, gold[1]);
        // the halo of light over its head
        for (int k = 0; k < 24; k++)
        {
            float a = k * MathF.PI * 2 / 24;
            PixelPaint.Plot(top, (int)MathF.Round(cx + MathF.Cos(a) * 4.2f), (int)MathF.Round(3.2f + bob + MathF.Sin(a) * 1.3f), k % 4 == 0 ? gold[0] : P.Hex(0xDCE6F8));
        }
    }

    // MARK: The new moon

    private static void NewMoon(PixelCanvas back, PixelCanvas body, PixelCanvas front, PixelCanvas glow, PixelCanvas top, int frame)
    {
        P[] night = { P.Hex(0x58508E), P.Hex(0x2E2A5E), P.Hex(0x16143A) };
        P silver = P.Hex(0xE8EEFF), silverDark = P.Hex(0x9AA6D0);
        float cx = 11f, cy = 13f;
        // the corona: spikes of pale light behind the disc, turning a little
        for (int k = 0; k < 12; k++)
        {
            float a = k * MathF.PI / 6 + frame * 0.09f;
            float len = (k % 2 == 0 ? 10.4f : 9.4f);
            for (float r = 7.4f; r <= len; r += 0.5f)
                PixelPaint.Plot(back, (int)MathF.Round(cx + MathF.Cos(a) * r), (int)MathF.Round(cy + MathF.Sin(a) * r), r > len - 1.2f ? silverDark : P.Hex(0x6A62B8));
        }
        // the dark disc, and the bright crescent on its lit edge (the left)
        PixelPaint.Ellipse(body, cx, cy, 8f, 8f, silver);
        PixelPaint.Ellipse(body, cx + 1.6f, cy, 7.2f, 8f, night[1]);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                if (body[x, y] == night[1]) body[x, y] = night[(x + y * 2 + (int)((x - cx + 8) * 0.4f)) % 7 == 0 ? 0 : (x > cx + 3 ? 2 : 1)];
        // craters
        foreach (var (x, y, r) in new[] { (9, 8, 1.4f), (14, 16, 1.7f), (8, 17, 1.1f), (15, 9, 1.0f) }) PixelPaint.Ellipse(front, x, y, r, r, night[2]);
        Halo(glow, cx, cy, 12f, P.Hex(0x9A8AF0));
        // the great eye, like an eclipse: a ring of pale light round a dark pupil
        PixelPaint.Ellipse(front, 12f, 13f, 3.6f, 3f, P.Hex(0xC8F4FF));
        PixelPaint.Ellipse(front, 12f, 13f, 2.2f, 2.2f, P.Hex(0x120C30));
        PixelPaint.Plot(front, 11, 12, P.Hex(0xFFFFFF));
        // three small stars that circle it
        for (int k = 0; k < 3; k++)
        {
            float a = frame * MathF.PI * 2 / 9 + k * MathF.PI * 2 / 3;
            int sx = (int)MathF.Round(cx + MathF.Cos(a) * 9.8f), sy = (int)MathF.Round(cy + MathF.Sin(a) * 9.4f);
            PixelPaint.Plot(top, sx, sy, P.Hex(0xFFF2B0));
            foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) PixelPaint.Plot(top, sx + dx, sy + dy, P.Hex(0xFFE890, 150));
        }
    }
}
