using Godot;
using System;

/// <summary>
/// Pixel art for every bullet style. Each style is drawn at its natural size (bullets are not scaled, so the
/// pixel grid stays crisp) and has two frames that flicker. Art that points somewhere faces UP.
/// </summary>
public static class BulletArt
{
    public const float Scale = 4;
    public const int FrameCount = 2;

    public static (int Width, int Height) Size(BulletStyle style) => style switch
    {
        BulletStyle.Fireball => (7, 7),
        BulletStyle.FlameDart => (5, 11),
        BulletStyle.Spark => (5, 5),
        BulletStyle.VenomOrb => (5, 5),
        BulletStyle.LotusPetal => (7, 9),
        BulletStyle.MagmaOrb => (26, 26),
        BulletStyle.FireSerpent => (5, 9),
        BulletStyle.Ember => (4, 4),
        BulletStyle.LightBolt => (3, 8),
        BulletStyle.OrbGreen => (7, 7),
        BulletStyle.OrbRed => (7, 7),
        BulletStyle.RiceRed => (3, 7),
        BulletStyle.Wisp => (5, 5),
        _ => (1, 1),
    };

    public static Vector2 SpriteSize(BulletStyle style)
    {
        var s = Size(style);
        return new Vector2(s.Width * Scale, s.Height * Scale);
    }

    /// <summary>Styles that are elongated or directional are rotated to face where the bullet is going.</summary>
    public static bool Rotates(BulletStyle style) => style switch
    {
        BulletStyle.FlameDart or BulletStyle.LotusPetal or BulletStyle.FireSerpent or BulletStyle.LightBolt or BulletStyle.RiceRed => true,
        _ => false,
    };

    /// <summary>Godot Node2D.Rotation (clockwise positive, Y down) that makes up-facing art point along a game-space velocity.</summary>
    public static float GodotRotation(Vector2 v) => MathF.Atan2(v.Y, v.X) + MathF.PI / 2;

    public static Texture2D[] MakeFrames(BulletStyle style)
    {
        var result = new Texture2D[FrameCount];
        for (int f = 0; f < FrameCount; f++) result[f] = MakeCanvas(style, f).ToTexture();
        return result;
    }

    public static PixelCanvas MakeCanvas(BulletStyle style, int frame)
    {
        var s = Size(style);
        var c = new PixelCanvas(s.Width, s.Height);
        switch (style)
        {
            case BulletStyle.Fireball: DrawFireball(c, frame); break;
            case BulletStyle.FlameDart: DrawFlameDart(c, frame); break;
            case BulletStyle.Spark: DrawSpark(c, frame); break;
            case BulletStyle.VenomOrb: DrawVenomOrb(c, frame); break;
            case BulletStyle.LotusPetal: DrawLotusPetal(c, frame); break;
            case BulletStyle.MagmaOrb: DrawMagmaOrb(c, frame); break;
            case BulletStyle.FireSerpent: DrawFireSerpent(c, frame); break;
            case BulletStyle.Ember: DrawEmber(c, frame); break;
            case BulletStyle.LightBolt: DrawLightBolt(c, frame); break;
            case BulletStyle.OrbGreen: DrawOrb(c, frame, PixelColor.Hex(0x2FA84F), PixelColor.Hex(0x7BE08A)); break;
            case BulletStyle.OrbRed: DrawOrb(c, frame, PixelColor.Hex(0xC8283A), PixelColor.Hex(0xFF6A78)); break;
            case BulletStyle.RiceRed: DrawRice(c, frame); break;
            case BulletStyle.Wisp: DrawWisp(c, frame); break;
        }
        return c;
    }

    // Helpers

    private static readonly PixelColor[] Fire = FirePalette.Ramp; // white-hot ... dark red
    private static readonly PixelColor[] Green = FirePalette.Scales; // dark ... light

    private static void Put(PixelCanvas c, int x, int y, PixelColor color)
    {
        if (c.Contains(x, y)) c[x, y] = color;
    }

    /// <summary>Filled disk centred on a pixel-centre coordinate; colour steps through <paramref name="ramp"/> from the centre outwards.</summary>
    private static void Disk(PixelCanvas c, float cx, float cy, float radius, PixelColor[] ramp)
    {
        for (int y = 0; y < c.Height; y++)
        {
            for (int x = 0; x < c.Width; x++)
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                if (!(d <= radius)) continue;
                int i = Math.Min(ramp.Length - 1, (int)(d / radius * ramp.Length));
                c[x, y] = ramp[i];
            }
        }
    }

    // Styles

    /// <summary>Phase 1: a round fireball that pulses.</summary>
    private static void DrawFireball(PixelCanvas c, int frame) =>
        Disk(c, 3.5f, 3.5f, frame == 0 ? 3.6f : 3.2f, new[] { Fire[0], Fire[1], Fire[2], Fire[3], Fire[4] });

    /// <summary>Phase 2: a comet of flame with a wavering tail.</summary>
    private static void DrawFlameDart(PixelCanvas c, int frame)
    {
        Disk(c, 2.5f, 2.5f, 2.6f, new[] { Fire[0], Fire[1], Fire[2], Fire[3] });
        int wave = frame == 0 ? 0 : 1;
        // Tail: (row, columns...) narrowing and cooling, swaying between frames.
        Put(c, 1, 5, Fire[3]); Put(c, 2, 5, Fire[2]); Put(c, 3, 5, Fire[3]);
        Put(c, 2 - wave, 6, Fire[3]); Put(c, 3 - wave, 6, Fire[3]);
        Put(c, 2, 7, Fire[4]); Put(c, 2 + wave - 1, 7, Fire[4]);
        Put(c, 2 + wave - 1, 8, Fire[4]); Put(c, 2, 9, Fire[5]);
        if (frame == 1) Put(c, 2, 10, Fire[5]);
    }

    /// <summary>Phase 3 (main): a bright twinkling spark.</summary>
    private static void DrawSpark(PixelCanvas c, int frame)
    {
        Put(c, 2, 2, Fire[0]);
        if (frame == 0)
        {
            foreach (var (x, y) in new[] { (2, 1), (1, 2), (3, 2), (2, 3) }) Put(c, x, y, Fire[1]);
            foreach (var (x, y) in new[] { (2, 0), (0, 2), (4, 2), (2, 4) }) Put(c, x, y, Fire[2]);
        }
        else
        {
            foreach (var (x, y) in new[] { (1, 1), (3, 1), (1, 3), (3, 3) }) Put(c, x, y, Fire[1]);
            foreach (var (x, y) in new[] { (0, 0), (4, 0), (0, 4), (4, 4) }) Put(c, x, y, Fire[2]);
            foreach (var (x, y) in new[] { (2, 1), (1, 2), (3, 2), (2, 3) }) Put(c, x, y, Fire[0]);
        }
    }

    /// <summary>Phase 3 (secondary): a drop of serpent venom with a glowing heart.</summary>
    private static void DrawVenomOrb(PixelCanvas c, int frame)
    {
        Disk(c, 2.5f, 2.5f, 2.6f, new[] { PixelColor.Hex(0xE6FF7A), Green[3], Green[2], Green[0] });
        Put(c, 1, 1, PixelColor.Hex(0xE6FF7A));
        Put(c, 2, 2, frame == 0 ? PixelColor.Hex(0xFFE94D) : PixelColor.Hex(0xFFB23D));
    }

    /// <summary>Phase 4: a petal of the flaming lotus, tip first.</summary>
    private static void DrawLotusPetal(PixelCanvas c, int frame)
    {
        var edge = PixelColor.Hex(0xC2284A);
        var mid = Fire[3];
        // Row widths from the tip down to the rounded base.
        var rows = new[] { (3, 3), (3, 3), (2, 4), (2, 4), (1, 5), (1, 5), (1, 5), (2, 4), (3, 3) };
        for (int y = 0; y < rows.Length; y++)
        {
            var r = rows[y];
            for (int x = r.Item1; x <= r.Item2; x++)
            {
                bool onEdge = x == r.Item1 || x == r.Item2 || y == 0 || y == rows.Length - 1;
                Put(c, x, y, onEdge ? edge : mid);
            }
        }
        // A glowing vein that pulses.
        var vein = frame == 0 ? new[] { Fire[1], Fire[1], Fire[2], Fire[2] } : new[] { Fire[0], Fire[1], Fire[1], Fire[2] };
        for (int i = 0; i < 4; i++) Put(c, 3, 2 + i, vein[i]);
        Put(c, 3, 6, Fire[2]);
    }

    /// <summary>Phase 5 (big): a sphere of molten rock with a cracked crust and flame licking off the rim.</summary>
    private static void DrawMagmaOrb(PixelCanvas c, int frame)
    {
        var seed = new SeededGenerator(0x1A5A);
        var noise = new PeriodicNoise(9, 9, seed);
        float phase = frame * MathF.PI;
        float cx = 13, cy = 13;

        for (int y = 0; y < c.Height; y++)
        {
            for (int x = 0; x < c.Width; x++)
            {
                float dx = (x + 0.5f - cx) / 11.5f, dy = (y + 0.5f - cy) / 11.5f;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                float a = MathF.Atan2(dy, dx);
                float edge = 1 + 0.045f * MathF.Sin(a * 6 + phase) + 0.03f * MathF.Sin(a * 11 - phase * 1.5f);
                if (!(d < edge)) continue;

                float t = d / edge;
                if (t > 0.82f)
                {
                    // Rim of flame.
                    c[x, y] = Fire[Math.Min(5, 2 + (int)((t - 0.82f) / 0.18f * 3.5f))];
                    continue;
                }
                // Lava glows through cracks in a dark crust; the crack pattern drifts between frames.
                float n = noise.Sample(x / 26f + frame * 0.06f, y / 26f);
                float lit = 1 - t * 0.7f + (-dx - dy) * 0.25f;
                if (n > 0.6f)
                    c[x, y] = lit > 0.75f ? Fire[1] : Fire[2];
                else
                    c[x, y] = lit > 0.7f ? PixelColor.Hex(0x7A1410) : PixelColor.Hex(0x4A0C12);
            }
        }
        // Hot core.
        Disk(c, 13, 13, 3, new[] { Fire[0], Fire[1], Fire[2] });
    }

    /// <summary>Phase 5 (small): a tiny fire cobra that slithers as it flies, head first.</summary>
    private static void DrawFireSerpent(PixelCanvas c, int frame)
    {
        float phase = frame * MathF.PI;
        for (int row = 2; row < 9; row++)
        {
            int cx = 2 + (int)MathF.Round(1.1f * MathF.Sin(row * 1.1f + phase), MidpointRounding.AwayFromZero);
            bool stripe = row % 2 == 0;
            Put(c, cx, row, stripe ? Fire[3] : Fire[4]);
            Put(c, cx + 1, row, stripe ? Fire[2] : Fire[3]);
        }
        // Hooded head with yellow eyes and a flicking tongue.
        for (int x = 1; x <= 3; x++) Put(c, x, 1, Fire[3]);
        Put(c, 2, 0, Fire[2]);
        Put(c, 1, 1, Fire[1]); Put(c, 3, 1, Fire[1]);
        if (frame == 1) Put(c, 2, 0, PixelColor.Hex(0xE23B3B));
    }

    /// <summary>Phase 6: a small glowing ember.</summary>
    private static void DrawEmber(PixelCanvas c, int frame)
    {
        var outer = frame == 0 ? Fire[4] : Fire[5];
        foreach (var (x, y) in new[] { (1, 0), (2, 0), (0, 1), (3, 1), (0, 2), (3, 2), (1, 3), (2, 3) }) Put(c, x, y, outer);
        foreach (var (x, y) in new[] { (1, 1), (2, 1), (1, 2), (2, 2) }) Put(c, x, y, Fire[frame == 0 ? 2 : 3]);
        Put(c, frame == 0 ? 1 : 2, 1, Fire[1]);
    }

    /// <summary>The player's shots: slim bolts of pale lantern light, cool-toned so they never read as enemy fire.</summary>
    private static void DrawLightBolt(PixelCanvas c, int frame)
    {
        var core = PixelColor.Hex(0xE8FCFF); var edge = PixelColor.Hex(0x4FC3E8); var tail = PixelColor.Hex(0x2A7FA8);
        for (int y = 0; y < 6; y++) { Put(c, 1, y, core); Put(c, 0, y, edge); Put(c, 2, y, edge); }
        Put(c, 1, 0, frame == 0 ? core : edge);
        Put(c, 0, 0, PixelColor.Clear); Put(c, 2, 0, PixelColor.Clear);
        Put(c, 1, 6, edge); Put(c, 1, 7, tail);
        if (frame == 1) { Put(c, 0, 6, tail); Put(c, 2, 6, tail); }
    }

    /// <summary>A round "pearl" bullet: a coloured rim, a lighter body and a white heart. It pulses, glowing brighter every other frame.</summary>
    private static void DrawOrb(PixelCanvas c, int frame, PixelColor rim, PixelColor body)
    {
        var white = PixelColor.Hex(0xFFFFFF);
        float glow = frame == 0 ? 0f : 0.35f;
        Disk(c, 3.5f, 3.5f, 3.6f, new[] { white, body.Mixed(white, glow), body, rim.Mixed(white, glow) });
    }

    /// <summary>A wisp of the lantern's light: a small warm pearl that glows brighter every other frame.</summary>
    private static void DrawWisp(PixelCanvas c, int frame)
    {
        var white = PixelColor.Hex(0xFFFFFF); var body = PixelColor.Hex(0xFFE9A0); var rim = PixelColor.Hex(0xF0A030);
        float glow = frame == 0 ? 0 : 0.35f;
        Disk(c, 2.5f, 2.5f, 2.6f, new[] { white, body.Mixed(white, glow), rim.Mixed(white, glow) });
    }

    /// <summary>A grain of rice: a short, pointed bullet that flies tip first.</summary>
    private static void DrawRice(PixelCanvas c, int frame)
    {
        var rim = PixelColor.Hex(0xC8283A); var body = PixelColor.Hex(0xFF6A78); var core = PixelColor.Hex(0xFFE0E0);
        Put(c, 1, 0, rim);
        for (int y = 1; y <= 5; y++)
        {
            Put(c, 0, y, rim); Put(c, 2, y, rim);
            Put(c, 1, y, y == 2 || y == 3 ? core : body);
        }
        Put(c, 0, 1, PixelColor.Clear); Put(c, 2, 1, PixelColor.Clear);
        Put(c, 1, 6, frame == 0 ? rim : body);
        Put(c, 0, 6, PixelColor.Clear); Put(c, 2, 6, PixelColor.Clear);
    }
}
