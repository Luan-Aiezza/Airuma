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
        BulletStyle.LightBolt => (5, 10),
        BulletStyle.OrbGreen => (7, 7),
        BulletStyle.OrbRed => (7, 7),
        BulletStyle.RiceRed => (3, 7),
        BulletStyle.Wisp => (5, 5),
        BulletStyle.Hail => (7, 7),
        BulletStyle.Star => (7, 7),
        BulletStyle.Shard => (5, 11),
        BulletStyle.Claw => (7, 11),
        BulletStyle.Rock => (7, 7),
        BulletStyle.Stalactite => (5, 13),
        BulletStyle.BoltYellow or BulletStyle.BoltBlue or BulletStyle.BoltRed => (5, 11),
        _ => FamilySize(style),
    };

    public static Vector2 SpriteSize(BulletStyle style)
    {
        var s = Size(style);
        return new Vector2(s.Width * Scale, s.Height * Scale);
    }

    /// <summary>Styles that are elongated or directional are rotated to face where the bullet is going.</summary>
    public static bool Rotates(BulletStyle style) => style switch
    {
        BulletStyle.FlameDart or BulletStyle.LotusPetal or BulletStyle.FireSerpent or BulletStyle.LightBolt or BulletStyle.RiceRed or BulletStyle.Shard or BulletStyle.Claw or BulletStyle.Stalactite or BulletStyle.BoltYellow or BulletStyle.BoltBlue or BulletStyle.BoltRed => true,
        _ => Family(style) is Shape.Rice or Shape.Kunai,
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
            case BulletStyle.Hail: DrawHail(c, frame); break;
            case BulletStyle.Star: DrawStar(c, frame); break;
            case BulletStyle.Shard: DrawShard(c, frame); break;
            case BulletStyle.Claw: DrawClaw(c, frame); break;
            case BulletStyle.Rock: DrawRock(c, frame); break;
            case BulletStyle.Stalactite: DrawStalactite(c, frame); break;
            case BulletStyle.BoltYellow: DrawBolt(c, frame, PixelColor.Hex(0xC88818), PixelColor.Hex(0xFFD84A), PixelColor.Hex(0xFFF8D0)); break;
            case BulletStyle.BoltRed: DrawBolt(c, frame, PixelColor.Hex(0x9A1020), PixelColor.Hex(0xFF3A3A), PixelColor.Hex(0xFFE0D8)); break;
            case BulletStyle.BoltBlue: DrawBolt(c, frame, PixelColor.Hex(0x2A5FD8), PixelColor.Hex(0x6AA0FF), PixelColor.Hex(0xE8F4FF)); break;
            default: DrawFamily(c, style, frame); break;
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

    /// <summary>A hailstone: a sphere of white ice with a pale blue shade and a glint that moves.</summary>
    private static void DrawHail(PixelCanvas c, int frame)
    {
        Disk(c, 3f, 3f, 3.3f, new[] { PixelColor.Hex(0xFFFFFF), PixelColor.Hex(0xDDEFFF), PixelColor.Hex(0x9CC4EC), PixelColor.Hex(0x4E7CB8) });
        Put(c, frame == 0 ? 2 : 4, frame == 0 ? 2 : 2, PixelColor.Hex(0xFFFFFF));
        Put(c, 4, 4, PixelColor.Hex(0x4E7CB8));
    }

    /// <summary>A shard of cave ice: a long pointed crystal (the point is its head, so that it falls point first), pale blue with a white edge that glints.</summary>
    private static void DrawShard(PixelCanvas c, int frame)
    {
        var white = PixelColor.Hex(0xFFFFFF); var light = PixelColor.Hex(0xDDF4FF); var mid = PixelColor.Hex(0x8CC8F0); var dark = PixelColor.Hex(0x3E78B8);
        Put(c, 2, 0, white);
        Put(c, 1, 1, light); Put(c, 2, 1, white); Put(c, 3, 1, mid);
        for (int y = 2; y <= 6; y++) { Put(c, 0, y, y < 4 ? mid : dark); Put(c, 1, y, light); Put(c, 2, y, y == 3 && frame == 1 ? white : light); Put(c, 3, y, mid); Put(c, 4, y, dark); }
        Put(c, 0, 7, dark); Put(c, 1, 7, mid); Put(c, 2, 7, mid); Put(c, 3, 7, dark);
        Put(c, 1, 8, dark); Put(c, 2, 8, mid); Put(c, 3, 8, dark);
        Put(c, 2, 9, dark);
        if (frame == 0) Put(c, 1, 4, white);
    }

    /// <summary>The mark of a claw: a curved blade, white-hot at its edge and red behind it, with a dark tail (the point is its head). It glints on the second frame.</summary>
    private static void DrawClaw(PixelCanvas c, int frame)
    {
        string[] rows =
        {
            "...W...", "..WWR..", "..WRR..", ".WWRR..", ".WWRD..", ".WRRD..", ".WRD...", "WWRD...", "WRD....", "WD.....", "D......",
        };
        var white = PixelColor.Hex(frame == 0 ? 0xFFF4ECu : 0xFFFFFFu); var red = PixelColor.Hex(0xE0281C); var dark = PixelColor.Hex(0x7A0E12);
        for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++)
                switch (rows[y][x]) { case 'W': Put(c, x, y, white); break; case 'R': Put(c, x, y, red); break; case 'D': Put(c, x, y, dark); break; }
        if (frame == 1) { Put(c, 3, 1, white); Put(c, 2, 4, white); }
    }

    /// <summary>A stone: a rough, round chunk of grey-brown rock with cracks, lit from the upper left.</summary>
    private static void DrawRock(PixelCanvas c, int frame)
    {
        Disk(c, 3f, 3f, 3.3f, new[] { PixelColor.Hex(0xC8BEB0), PixelColor.Hex(0x9C9082), PixelColor.Hex(0x6E6458), PixelColor.Hex(0x3E362E) });
        var crack = PixelColor.Hex(0x2A241E);
        Put(c, 3, 2, crack); Put(c, 4, 3, crack); Put(c, 4, 4, crack); Put(c, frame == 0 ? 2 : 1, 4, crack);
        Put(c, 1, 1, PixelColor.Hex(0xE6DED0));
    }

    /// <summary>A stalactite broken from the ceiling: a long, heavy spike of grey rock with a pale edge, pointed at its head so that it falls point first.</summary>
    private static void DrawStalactite(PixelCanvas c, int frame)
    {
        var edge = PixelColor.Hex(0xD0C8BC); var lit = PixelColor.Hex(0xA09484); var mid = PixelColor.Hex(0x756A5C); var dark = PixelColor.Hex(0x453D34); var deep = PixelColor.Hex(0x2A241E);
        int[] half = { 0, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 1 };
        for (int y = 0; y < half.Length; y++)
            for (int x = 2 - half[y]; x <= 2 + half[y]; x++)
            {
                var col = x == 2 - half[y] ? edge : (x == 2 - half[y] + 1 ? lit : (x == 2 + half[y] ? dark : mid));
                if (half[y] == 0) col = edge;
                Put(c, x, y, col);
            }
        Put(c, 2, 6 + frame, deep); Put(c, 3, 8 - frame, deep);
        Put(c, 1, 12, deep); Put(c, 3, 12, deep);
    }

    /// <summary>A star of a constellation: a bright four-pointed star that twinkles.</summary>
    private static void DrawStar(PixelCanvas c, int frame)
    {
        var core = PixelColor.Hex(0xFFFFFF); var mid = PixelColor.Hex(0xFFF2B0); var edge = PixelColor.Hex(0xE8B64A);
        Put(c, 3, 3, core);
        foreach (var (x, y) in new[] { (3, 2), (2, 3), (4, 3), (3, 4) }) Put(c, x, y, mid);
        foreach (var (x, y) in new[] { (3, 1), (1, 3), (5, 3), (3, 5) }) Put(c, x, y, frame == 0 ? edge : mid);
        foreach (var (x, y) in new[] { (3, 0), (0, 3), (6, 3), (3, 6) }) if (frame == 0) Put(c, x, y, edge);
        if (frame == 1) foreach (var (x, y) in new[] { (2, 2), (4, 2), (2, 4), (4, 4) }) Put(c, x, y, edge);
    }

    /// <summary>The player's shots: slim bolts of pale lantern light, cool-toned so they never read as enemy fire.</summary>
    private static void DrawLightBolt(PixelCanvas c, int frame)
    {
        var core = PixelColor.Hex(0xE8FCFF); var mid = PixelColor.Hex(0x9BE3F6); var edge = PixelColor.Hex(0x4FC3E8); var tail = PixelColor.Hex(0x2A7FA8);
        // A fat bolt with a pointed head: the core down the middle, paler towards the edges.
        Put(c, 2, 0, frame == 0 ? core : mid);
        for (int x = 1; x <= 3; x++) Put(c, x, 1, x == 2 ? core : mid);
        for (int y = 2; y <= 7; y++)
        {
            Put(c, 0, y, edge); Put(c, 4, y, edge);
            Put(c, 1, y, mid); Put(c, 3, y, mid);
            Put(c, 2, y, core);
        }
        for (int x = 1; x <= 3; x++) Put(c, x, 8, x == 2 ? mid : edge);
        Put(c, 2, 9, tail);
        if (frame == 1) { Put(c, 1, 9, tail); Put(c, 3, 9, tail); Put(c, 0, 7, tail); Put(c, 4, 7, tail); }
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

    /// <summary>A little bolt of lightning, pointing up: a zigzag with a bright core, a glow of its colour round it, and a flicker of its tail on the second frame.</summary>
    private static void DrawBolt(PixelCanvas c, int frame, PixelColor rim, PixelColor body, PixelColor core)
    {
        var path = frame == 0
            ? new (int x, int y)[] { (3, 0), (3, 1), (2, 2), (1, 3), (2, 4), (3, 5), (3, 6), (2, 7), (1, 8), (2, 9), (2, 10) }
            : new (int x, int y)[] { (3, 0), (2, 1), (2, 2), (1, 3), (1, 4), (2, 5), (3, 6), (3, 7), (2, 8), (1, 9), (2, 10) };
        foreach (var (x, y) in path) { Put(c, x - 1, y, rim); Put(c, x + 1, y, rim); }
        foreach (var (x, y) in path) Put(c, x, y, body);
        for (int i = 0; i < path.Length; i += 2) Put(c, path[i].x, path[i].y, core);
        Put(c, path[0].x, path[0].y, core);
    }

    // MARK: The danmaku family (colours and shapes)

    private enum Shape { None, Orb, Bead, Rice, Kunai, Star, Giant }
    private enum Hue { Red, Blue, Yellow, Green, Violet, White, Orange, Cyan, Pink, Gold }

    private static Shape Family(BulletStyle style) => style switch
    {
        >= BulletStyle.OrbBlue and <= BulletStyle.OrbPink => Shape.Orb,
        >= BulletStyle.BeadRed and <= BulletStyle.BeadPink => Shape.Bead,
        >= BulletStyle.RiceBlue and <= BulletStyle.RiceCyan => Shape.Rice,
        >= BulletStyle.KunaiRed and <= BulletStyle.KunaiViolet => Shape.Kunai,
        >= BulletStyle.StarBlue and <= BulletStyle.StarPink => Shape.Star,
        >= BulletStyle.GiantRed and <= BulletStyle.GiantWhite => Shape.Giant,
        _ => Shape.None,
    };

    private static (int Width, int Height) FamilySize(BulletStyle style) => Family(style) switch
    {
        Shape.Orb or Shape.Star => (7, 7),
        Shape.Bead => (5, 5),
        Shape.Rice => (3, 7),
        Shape.Kunai => (5, 9),
        Shape.Giant => (25, 25),
        _ => (1, 1),
    };

    private static Hue HueOf(BulletStyle style) => style switch
    {
        BulletStyle.OrbBlue or BulletStyle.BeadBlue or BulletStyle.RiceBlue or BulletStyle.KunaiBlue or BulletStyle.StarBlue or BulletStyle.GiantBlue => Hue.Blue,
        BulletStyle.OrbYellow or BulletStyle.BeadYellow or BulletStyle.RiceYellow or BulletStyle.KunaiYellow => Hue.Yellow,
        BulletStyle.GiantGold or BulletStyle.StarGold => Hue.Gold,
        BulletStyle.BeadGreen or BulletStyle.RiceGreen or BulletStyle.KunaiGreen or BulletStyle.StarGreen or BulletStyle.GiantGreen => Hue.Green,
        BulletStyle.OrbViolet or BulletStyle.BeadViolet or BulletStyle.RiceViolet or BulletStyle.KunaiViolet or BulletStyle.StarViolet or BulletStyle.GiantViolet => Hue.Violet,
        BulletStyle.OrbWhite or BulletStyle.BeadWhite or BulletStyle.RiceWhite or BulletStyle.GiantWhite => Hue.White,
        BulletStyle.OrbOrange => Hue.Orange,
        BulletStyle.OrbCyan or BulletStyle.BeadCyan or BulletStyle.RiceCyan => Hue.Cyan,
        BulletStyle.OrbPink or BulletStyle.BeadPink or BulletStyle.StarPink => Hue.Pink,
        _ => Hue.Red,
    };

    /// <summary>The three tones of a colour: its rim (dark), its body, and its glow (nearly white).</summary>
    private static (PixelColor Rim, PixelColor Body, PixelColor Glow) Tones(Hue hue) => hue switch
    {
        Hue.Blue => (PixelColor.Hex(0x2A5FD8), PixelColor.Hex(0x6AA0FF), PixelColor.Hex(0xE0ECFF)),
        Hue.Yellow => (PixelColor.Hex(0xD8A818), PixelColor.Hex(0xFFE45A), PixelColor.Hex(0xFFF8D0)),
        Hue.Green => (PixelColor.Hex(0x2FA84F), PixelColor.Hex(0x7BE08A), PixelColor.Hex(0xE0FFE6)),
        Hue.Violet => (PixelColor.Hex(0x7A32C8), PixelColor.Hex(0xB47CFF), PixelColor.Hex(0xF0E0FF)),
        Hue.White => (PixelColor.Hex(0x8A9AB0), PixelColor.Hex(0xE4ECF8), PixelColor.Hex(0xFFFFFF)),
        Hue.Orange => (PixelColor.Hex(0xD8601C), PixelColor.Hex(0xFF9A4A), PixelColor.Hex(0xFFE6C8)),
        Hue.Cyan => (PixelColor.Hex(0x1A9AB8), PixelColor.Hex(0x5AE0F0), PixelColor.Hex(0xE0FCFF)),
        Hue.Pink => (PixelColor.Hex(0xD8387A), PixelColor.Hex(0xFF84B4), PixelColor.Hex(0xFFE0EE)),
        Hue.Gold => (PixelColor.Hex(0xC88818), PixelColor.Hex(0xFFD84A), PixelColor.Hex(0xFFF6C0)),
        _ => (PixelColor.Hex(0xC8283A), PixelColor.Hex(0xFF6A78), PixelColor.Hex(0xFFE0E0)),
    };

    private static void DrawFamily(PixelCanvas c, BulletStyle style, int frame)
    {
        var (rim, body, glow) = Tones(HueOf(style));
        switch (Family(style))
        {
            case Shape.Orb: DrawOrb(c, frame, rim, body); break;
            case Shape.Bead:
                Disk(c, 2.5f, 2.5f, 2.6f, new[] { glow, body.Mixed(glow, frame == 0 ? 0f : 0.35f), rim });
                break;
            case Shape.Rice:
                Put(c, 1, 0, rim);
                for (int y = 1; y <= 5; y++) { Put(c, 0, y, rim); Put(c, 2, y, rim); Put(c, 1, y, y == 2 || y == 3 ? glow : body); }
                Put(c, 0, 1, PixelColor.Clear); Put(c, 2, 1, PixelColor.Clear);
                Put(c, 1, 6, frame == 0 ? rim : body); Put(c, 0, 6, PixelColor.Clear); Put(c, 2, 6, PixelColor.Clear);
                break;
            case Shape.Kunai:
            {
                // a blade with a point, a ring at its heel: faces up
                Put(c, 2, 0, glow);
                Put(c, 1, 1, body); Put(c, 2, 1, glow); Put(c, 3, 1, body);
                for (int y = 2; y <= 5; y++) { Put(c, 1, y, body); Put(c, 2, y, y == 3 && frame == 1 ? glow : glow.Mixed(body, 0.4f)); Put(c, 3, y, rim); }
                Put(c, 0, 4, rim); Put(c, 4, 4, rim); Put(c, 2, 6, rim);
                Put(c, 1, 7, rim); Put(c, 3, 7, rim); Put(c, 2, 8, rim);
                break;
            }
            case Shape.Star:
            {
                Put(c, 3, 3, PixelColor.Hex(0xFFFFFF));
                foreach (var (x, y) in new[] { (3, 2), (2, 3), (4, 3), (3, 4) }) Put(c, x, y, glow);
                foreach (var (x, y) in new[] { (3, 1), (1, 3), (5, 3), (3, 5) }) Put(c, x, y, frame == 0 ? body : glow);
                foreach (var (x, y) in new[] { (3, 0), (0, 3), (6, 3), (3, 6) }) if (frame == 0) Put(c, x, y, rim);
                if (frame == 1) foreach (var (x, y) in new[] { (2, 2), (4, 2), (2, 4), (4, 4) }) Put(c, x, y, rim);
                break;
            }
            case Shape.Giant:
            {
                // a great orb: a dark rim, a body that brightens towards a glint at its upper left, a ring of light inside the rim, and a slow pulse
                float pulse = frame == 0 ? 0 : 0.6f;
                for (int y = 0; y < 25; y++)
                    for (int x = 0; x < 25; x++)
                    {
                        float dx = x + 0.5f - 12.5f, dy = y + 0.5f - 12.5f;
                        float d = MathF.Sqrt(dx * dx + dy * dy);
                        if (d > 12.2f - pulse * 0.4f) continue;
                        float t = d / 12.2f;
                        var color = t > 0.9f ? rim : (t > 0.78f ? rim.Mixed(body, 0.55f) : body);
                        float hx = x + 0.5f - 8.5f, hy = y + 0.5f - 8.5f;
                        float h = MathF.Sqrt(hx * hx + hy * hy);
                        if (t <= 0.78f) color = h < 2.6f ? PixelColor.Hex(0xFFFFFF) : (h < 5.2f ? glow : (h < 8.5f ? body.Mixed(glow, 0.35f) : body));
                        if (t > 0.6f && t <= 0.78f && frame == 1 && (x + y) % 3 == 0) color = body.Mixed(glow, 0.5f);
                        c[x, y] = color;
                    }
                break;
            }
        }
    }
}
