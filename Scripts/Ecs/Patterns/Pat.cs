using System;
using Godot;

/// <summary>Where on the border of the field bullets come from.</summary>
public enum Edge { Top, Bottom, Left, Right }

/// <summary>
/// The library of figures that the attacks of the bosses and the spirits are made of: rings, fans, walls with gaps, polygons and stars, roses, spokes, lattices, rain from any side, wheels and
/// spiral arms. Each draws one volley, with a <see cref="Shot"/> that says what the bullets are. The counts are the ones of the normal mode; the easy and hard modes scale them (unless the figure
/// must keep its shape: a polygon, a star).
/// </summary>
public static class Pat
{
    public static PatternSpec Every(float interval, Action<PatternContext> fire) => new PatternSpec { Interval = interval, Fire = fire };

    /// <summary>A rhythm of the beat of the dance: half a second.</summary>
    public const float Beat = BulletSystem.DanceBeat;

    public static Vector2 Dir(float degrees) { float a = Mathx.Radians(degrees); return new Vector2(MathF.Cos(a), MathF.Sin(a)); }

    /// <summary>A point on a circle round `centre`.</summary>
    public static Vector2 Round(Vector2 centre, float radius, float degrees) => centre + Dir(degrees) * radius;

    /// <summary>A triangle wave from -1 to 1 and back with the given period (a gap that sweeps across).</summary>
    public static float Triangle(float time, float period) { float x = time / period % 1f; if (x < 0) x += 1; return x < 0.5f ? 4 * x - 1 : 3 - 4 * x; }

    /// <summary>A sine wave with the given period.</summary>
    public static float Wave(float time, float period) => MathF.Sin(MathF.Tau * time / period);

    // MARK: Rings and fans, the way Touhou 6 shoots them

    /// <summary>How a volley is laid out (the modes of the bullet instructions of the Touhou 6 scripts).</summary>
    public enum Aim { FanAimed, Fan, CircleAimed, Circle, OffsetCircleAimed, OffsetCircle, RandomAngle, RandomSpeed, Random }

    /// <summary>
    /// A volley laid out as the scripts of Touhou 6 do it: `layers` layers of `count` bullets each. The speed goes from `speed1` (the first layer) towards `speed2` (the last). In a fan, `angle2` is the angle between
    /// neighbours (they go out alternately to the two sides of the middle one); in a circle, `angle2` is how much each layer is turned from the one before (layers of rings that make a spiral), and `angle1` turns it
    /// all. The aimed modes add the angle to the boat. (A random mode takes the angle between `angle1 - angle2`... as the original does: from `angle1` for `angle2` degrees.)
    /// </summary>
    public static void Shoot(PatternContext c, Vector2 at, Aim aim, int count, int layers, float angle1, float angle2, float speed1, float speed2, in Shot shot, bool scale = true)
    {
        if (scale) count = c.N(count, 1);
        if (layers < 1) layers = 1;
        float toBoat = c.AimFrom(at);
        for (int layer = 0; layer < layers; layer++)
            for (int i = 0; i < count; i++)
            {
                float speed = speed1 - (speed1 - speed2) * layer / layers;
                float angle = 0;
                switch (aim)
                {
                    case Aim.FanAimed:
                    case Aim.Fan:
                        angle = (count & 1) == 1 ? ((i + 1) / 2) * angle2 : (i / 2) * angle2 + angle2 * 0.5f;
                        if ((i & 1) == 1) angle = -angle;
                        if (aim == Aim.FanAimed) angle += toBoat;
                        angle += angle1;
                        break;
                    case Aim.CircleAimed:
                    case Aim.Circle:
                        angle = (aim == Aim.CircleAimed ? toBoat : 0) + i * 360f / count + layer * angle2 + angle1;
                        break;
                    case Aim.OffsetCircleAimed:
                    case Aim.OffsetCircle:
                        angle = (aim == Aim.OffsetCircleAimed ? toBoat : 0) + 180f / count + i * 360f / count + angle1;
                        break;
                    case Aim.RandomAngle: angle = angle1 + PatternContext.Rand(0, angle2); break;
                    case Aim.RandomSpeed: speed = PatternContext.Rand(speed2, speed1); angle = i * 360f / count + layer * angle2 + angle1; break;
                    default: angle = angle1 + PatternContext.Rand(0, angle2); speed = PatternContext.Rand(speed2, speed1); break;
                }
                c.Fire(at, angle, shot.Speeded(speed));
            }
    }

    // MARK: Lasers

    /// <summary>
    /// A laser: a straight beam from `from` along `heading` (degrees), through and beyond the whole field. First a thin line of its colour warns of it for `warn` seconds; then it strikes for `strike` seconds, `width`
    /// px thick; then it fades. It may turn about its origin (`spin`, degrees a second) and may go with the boss (`follow`).
    /// </summary>
    public static void Laser(PatternContext c, Vector2 from, float heading, float width, float warn, float strike, Color tint, float spin = 0, bool follow = false, float blink = 0)
    {
        if (c.Slashes == null) return;
        float h = Mathx.Radians(heading);
        float time = c.Difficulty == Difficulty.Easy ? 1.25f : (c.Difficulty == Difficulty.Hard ? 0.9f : 1);
        c.Slashes.Add(new SlashData
        {
            Top = from, Angle = MathF.PI / 2 - h, Telegraph = warn * time, Strike = strike, Width = width, Leader = true, Len = 2300, Beam = true, Tint = tint,
            Spin = -Mathx.Radians(spin), Follow = follow, Blink = blink, Offset = follow ? from - c.Origin : Vector2.Zero,
        });
    }

    // MARK: Rings and fans

    /// <summary>`n` bullets evenly round, the first at `start` degrees.</summary>
    public static void Ring(PatternContext c, Vector2 at, int n, float start, in Shot shot, bool scale = true, bool alternate = false)
    {
        if (scale) n = c.N(n, 4);
        for (int k = 0; k < n; k++) c.Fire(at, start + 360f * k / n, alternate && k % 2 == 1 ? shot.Curving(-shot.Curve) : shot);
    }

    /// <summary>The bullets of a figure in turn the styles in `styles` (confetti: every colour).</summary>
    public static void Ring(PatternContext c, Vector2 at, int n, float start, in Shot shot, BulletStyle[] styles, bool alternate = false)
    {
        n = c.N(n, 4);
        for (int k = 0; k < n; k++) { var s = shot.Styled(styles[k % styles.Length]); c.Fire(at, start + 360f * k / n, alternate && k % 2 == 1 ? s.Curving(-s.Curve) : s); }
    }

    /// <summary>`n` bullets in a fan `spread` degrees wide, centred on `centre`.</summary>
    public static void Arc(PatternContext c, Vector2 at, int n, float centre, float spread, in Shot shot, bool scale = true, bool alternate = false)
    {
        if (scale) n = c.N(n, 1);
        for (int k = 0; k < n; k++) c.Fire(at, n > 1 ? centre - spread / 2 + spread * k / (n - 1) : centre, alternate && k % 2 == 1 ? shot.Curving(-shot.Curve) : shot);
    }

    /// <summary>A fan with a style for each bullet in turn (a fan of the colours of the rainbow).</summary>
    public static void Arc(PatternContext c, Vector2 at, int n, float centre, float spread, in Shot shot, BulletStyle[] styles)
    {
        n = c.N(n, 1);
        for (int k = 0; k < n; k++) c.Fire(at, n > 1 ? centre - spread / 2 + spread * k / (n - 1) : centre, shot.Styled(styles[k % styles.Length]));
    }

    /// <summary>A fan at the boat.</summary>
    public static void Aimed(PatternContext c, Vector2 at, int n, float spread, in Shot shot, bool scale = true, bool alternate = false) => Arc(c, at, n, c.AimFrom(at), spread, shot, scale, alternate);

    /// <summary>Rings one inside the other: the same bullets at several speeds, so that the ring becomes a series of rings that keep their gaps.</summary>
    public static void Layers(PatternContext c, Vector2 at, int n, float start, in Shot shot, params float[] speeds)
    {
        for (int j = 0; j < speeds.Length; j++) Ring(c, at, n, start + (j % 2 == 0 ? 0 : 180f / Math.Max(1, c.N(n, 4))), shot.Speeded(speeds[j]));
    }

    /// <summary>Spokes: `spokes` straight lines of bullets out of `at`, each of `perSpoke` bullets at speeds `shot.Speed`, `+dv`, `+2dv`...: turned between volleys they are the arms of a windmill.</summary>
    public static void Spokes(PatternContext c, Vector2 at, int spokes, int perSpoke, float start, float dv, in Shot shot, bool scale = false)
    {
        if (scale) spokes = c.N(spokes, 3);
        for (int s = 0; s < spokes; s++)
            for (int j = 0; j < perSpoke; j++) c.Fire(at, start + 360f * s / spokes, shot.Speeded(shot.Speed + j * dv));
    }

    /// <summary>A flower: a ring whose bullets are slower or faster by their angle, `petals` times round, so that after a moment the ring is drawn as a rose of that many petals (`depth` 0...1 is how deep).</summary>
    public static void Rose(PatternContext c, Vector2 at, int petals, int n, float start, float depth, in Shot shot, bool scale = true)
    {
        if (scale) n = c.N(n, petals * 2);
        for (int k = 0; k < n; k++)
        {
            float theta = 360f * k / n;
            float m = (1 - depth) + depth * (0.5f + 0.5f * MathF.Cos(Mathx.Radians(petals * theta)));
            c.Fire(at, start + theta, shot.Speeded(shot.Speed * m));
        }
    }

    // MARK: Figures

    /// <summary>A star of a constellation that goes from `from` and hangs exactly at `to` (it brakes to a stop there when it stops flying); returns its slot, to be linked with <see cref="BulletSystem.LinkStars"/>.</summary>
    public static int StarTo(PatternContext c, Vector2 from, Vector2 to, in Shot star)
    {
        float t = BulletSystem.StarFlySeconds, d = from.DistanceTo(to);
        float speed = 2 * d / t;                                            // (it brakes evenly to a stop in `t` seconds, and covers half the speed times t)
        var s = star.Speeded(speed).Constellation(speed / t).Steady();
        return c.Fire(from, Mathx.Degrees(MathF.Atan2(to.Y - from.Y, to.X - from.X)), s);
    }

    /// <summary>A constellation: `n` stars thrown out evenly round, braking, to hang linked each to the one `skip` places on (1: a polygon, 2: a pentagram...) by thin lines that hurt, and then fall.</summary>
    public static void Constellation(PatternContext c, Vector2 at, int n, float start, int skip, in Shot star)
    {
        var slots = new int[n];
        for (int k = 0; k < n; k++) slots[k] = c.Fire(at, start + 360f * k / n, star.Steady());
        BulletSystem.LinkStars(c.Pool, slots, Math.Max(1, skip));
    }

    /// <summary>Bullets along the segment from a to b, all going off at `angle`; with a fuse that grows by `ripple` seconds along it, the figure is let go like a wave.</summary>
    public static void Line(PatternContext c, Vector2 a, Vector2 b, int n, float angle, in Shot shot, float ripple = 0)
    {
        for (int k = 0; k < n; k++)
        {
            var s = shot.Steady(); s.Fuse += k * ripple;
            c.Fire(a.Lerp(b, n > 1 ? (float)k / (n - 1) : 0.5f), angle, s);
        }
    }

    /// <summary>`n` bullets in a straight line `spacing` px apart, across the way they go (a comb), centred on `at`, all going off at `heading` degrees: a slanting wall with gaps between the teeth.</summary>
    public static void Comb(PatternContext c, Vector2 at, float heading, int n, float spacing, in Shot shot, float ripple = 0)
    {
        var across = Dir(heading + 90);
        for (int k = 0; k < n; k++)
        {
            var s = shot.Steady(); s.Fuse += k * ripple;
            c.Fire(at + across * ((k - (n - 1) / 2f) * spacing), heading, s);
        }
    }

    /// <summary>
    /// A crescent moon of bullets that flies as one: its horns point the way it goes (`heading`). Its outer arc has `n` bullets on a circle of `radius` round `at`; its inner arc, a circle a little smaller and
    /// moved forward, has the same number, and they meet at the horns. All go off at `heading`, so the shape keeps its form for as long as they fly.
    /// </summary>
    public static void Crescent(PatternContext c, Vector2 at, float heading, float radius, int n, in Shot shot0, float thickness = 0.42f)
    {
        var shot = shot0.Steady();
        var forward = Dir(heading);
        var side = Dir(heading + 90);
        float h = Mathx.Radians(80), shift = thickness * radius;
        float r2 = MathF.Sqrt((radius * MathF.Cos(h) + shift) * (radius * MathF.Cos(h) + shift) + (radius * MathF.Sin(h)) * (radius * MathF.Sin(h)));
        float v0 = MathF.Atan2(radius * MathF.Sin(h), radius * MathF.Cos(h) + shift);
        for (int k = 0; k < n; k++)
        {
            float t = n > 1 ? (float)k / (n - 1) : 0.5f;
            float u = h * (2 * t - 1), v = v0 * (2 * t - 1);
            c.Fire(at + forward * (-radius * MathF.Cos(u)) + side * (-radius * MathF.Sin(u)), heading, shot);
            if (k > 0 && k < n - 1) c.Fire(at + forward * (shift - r2 * MathF.Cos(v)) + side * (-r2 * MathF.Sin(v)), heading, shot);
        }
    }

    /// <summary>
    /// A regular polygon of bullets (`perSide` along each side) with its first vertex at `rotation` degrees. The bullets go off straight out of the centre (`outward`), or all along `heading`. Fired
    /// with a Launch shot, the figure is laid out and held and then let go. The count does not change with the difficulty: a polygon keeps its sides.
    /// </summary>
    public static void Polygon(PatternContext c, Vector2 centre, int sides, float radius, float rotation, int perSide, in Shot shot, bool outward = true, float heading = 90, float ripple = 0)
    {
        for (int i = 0; i < sides; i++)
        {
            var a = Round(centre, radius, rotation + 360f * i / sides);
            var b = Round(centre, radius, rotation + 360f * (i + 1) / sides);
            for (int j = 0; j < perSide; j++)
            {
                var p = a.Lerp(b, (float)j / perSide);
                var s = shot.Steady(); s.Fuse += (i * perSide + j) * ripple;
                c.Fire(p, outward ? Mathx.Degrees(MathF.Atan2(p.Y - centre.Y, p.X - centre.X)) : heading, s);
            }
        }
    }

    /// <summary>A star polygon {points/skip}: the points are joined each to the one `skip` places on (5, 2: the pentagram; 7, 3: the great heptagram), `perEdge` bullets along each line.</summary>
    public static void Star(PatternContext c, Vector2 centre, int points, int skip, float radius, float rotation, int perEdge, in Shot shot, bool outward = true, float heading = 90, float ripple = 0)
    {
        for (int i = 0; i < points; i++)
        {
            var a = Round(centre, radius, rotation + 360f * (i * skip % points) / points);
            var b = Round(centre, radius, rotation + 360f * ((i + 1) * skip % points) / points);
            for (int j = 0; j < perEdge; j++)
            {
                var p = a.Lerp(b, (float)j / perEdge);
                var s = shot.Steady(); s.Fuse += (i * perEdge + j) * ripple;
                c.Fire(p, outward ? Mathx.Degrees(MathF.Atan2(p.Y - centre.Y, p.X - centre.X)) : heading, s);
            }
        }
    }

    // MARK: The borders of the field

    public static Vector2 EdgePoint(PatternContext c, Edge edge, float t, float outside = 14) => edge switch
    {
        Edge.Top => new Vector2(c.Left + c.Width * t, -outside),
        Edge.Bottom => new Vector2(c.Left + c.Width * t, c.Bottom + outside),
        Edge.Left => new Vector2(c.Left - outside, c.Bottom * t),
        _ => new Vector2(c.Right + outside, c.Bottom * t),
    };

    /// <summary>The angle that points into the field from an edge.</summary>
    public static float Inward(Edge edge) => edge switch { Edge.Top => 90, Edge.Bottom => -90, Edge.Left => 0, _ => 180 };

    /// <summary>
    /// A wall across the field from an edge: `slots` bullets in a row, going in, with a gap of `gapWidth` px (of the length of the edge) centred at `gapCentre` (0...1 of the edge; a second,
    /// mirrored gap if `mirror`). The bullets are `tilt` degrees off the straight (a slanting wall).
    /// </summary>
    public static void Wall(PatternContext c, Edge edge, int slots, float gapCentre, float gapWidth, in Shot shot, bool mirror = false, float tilt = 0)
    {
        float length = edge is Edge.Top or Edge.Bottom ? c.Width : c.Bottom;
        float gap = c.Gap(gapWidth) / length;
        for (int i = 0; i < slots; i++)
        {
            float t = (i + 0.5f) / slots;
            if (MathF.Abs(t - gapCentre) < gap / 2) continue;
            if (mirror && MathF.Abs(t - (1 - gapCentre)) < gap / 2) continue;
            c.Fire(EdgePoint(c, edge, t), Inward(edge) + tilt, shot.Steady());
        }
    }

    /// <summary>`n` bullets at random places of the edge, going in `spread` degrees off the straight, at speeds from 80% to 120%.</summary>
    /// <summary>How far beyond the ends of the edge (a share of its length) the rain starts, so that the wind finds bullets to carry in.</summary>
    private const float RainReach = 0.09f;

    public static void Rain(PatternContext c, Edge edge, int n, float spread, in Shot shot, bool scale = true)
    {
        if (scale) n = c.N(n, 1);
        for (int k = 0; k < n; k++)
        {
            // a drop is not let fall next to another that has only just come out: the gaps rule (see MinGap)
            Vector2 at = Vector2.Zero; bool free = false;
            for (int attempt = 0; attempt < 4 && !free; attempt++)
            {
                at = EdgePoint(c, edge, PatternContext.Rand(-RainReach, 1 + RainReach));
                free = c.IsFree(at, MinCentre, 140);
            }
            if (free) c.Fire(at, Inward(edge) + PatternContext.Rand(-spread / 2, spread / 2), shot.Speeded(shot.Speed * PatternContext.Rand(0.9f, 1.1f)));
        }
    }

    /// <summary>
    /// THE GAPS RULE: between two bullets of an enemy pattern there must always be room for the boat. The boat's hitbox is 8 px across, and the gap (edge to edge of the hitboxes) must be 2 to 3 times that: at least
    /// <see cref="MinGap"/> (16) and, when there is room, <see cref="ComfortGap"/> (24). A bullet's hitbox is about 10 px across, so the centres of two bullets are kept at least <see cref="MinCentre"/> apart.
    /// </summary>
    public const float PlayerHitbox = 8, MinGap = 16, ComfortGap = 24, MinCentre = 30;

    /// <summary>`n` bullets evenly along an edge (the part of it from `from` to `to`, 0...1), all going in.</summary>
    public static void Along(PatternContext c, Edge edge, int n, float from, float to, in Shot shot, float tilt = 0, bool scale = true, float ripple = 0)
    {
        if (scale) n = c.N(n, 2);
        for (int k = 0; k < n; k++)
        {
            var s = shot.Steady(); s.Fuse += k * ripple;
            c.Fire(EdgePoint(c, edge, from + (to - from) * (k + 0.5f) / n), Inward(edge) + tilt, s);
        }
    }

    /// <summary>
    /// A lattice: bullets going down from the top and bullets going across from the sides, in lines `spacing` px apart, offset by `phase` px, so that they weave a net. Each family has a gap of its own
    /// where the net is open; used with a slow shot it is a fine mesh to be threaded.
    /// </summary>
    public static void Lattice(PatternContext c, float spacing, float phase, in Shot down, in Shot across, float gapX, float gapY, float gapWidth)
    {
        float g = c.Gap(gapWidth);
        for (float x = c.Left + phase % spacing; x < c.Right; x += spacing)
            if (MathF.Abs(x - gapX) > g / 2) c.Fire(new Vector2(x, -14), 90, down.Steady());
        for (float y = phase % spacing; y < c.Bottom * 0.75f; y += spacing)
            if (MathF.Abs(y - gapY) > g / 2) c.Fire(new Vector2(c.Left - 14, y), 0, across.Steady());
    }

    // MARK: Wheels and arms

    /// <summary>`n` bullets on polar tracks round `anchor`, evenly spaced from `theta0`: they all turn at `omega` degrees a second and go out at `radialSpeed`. Volleys with a growing `theta0` draw spiral arms.</summary>
    public static void PolarRing(PatternContext c, Vector2 anchor, int n, float theta0, float radius0, float radialSpeed, float omega, in Shot shot, float radialAccel = 0, bool scale = true)
    {
        if (scale) n = c.N(n, 3);
        for (int k = 0; k < n; k++) c.FirePolar(anchor, radius0, radialSpeed, theta0 + 360f * k / n, omega, shot, radialAccel);
    }

    /// <summary>The same, each arm in the next style of `styles` (the seven arms of a rainbow galaxy).</summary>
    public static void PolarRing(PatternContext c, Vector2 anchor, int n, float theta0, float radius0, float radialSpeed, float omega, in Shot shot, BulletStyle[] styles, float radialAccel = 0)
    {
        n = c.N(n, 3);
        for (int k = 0; k < n; k++) c.FirePolar(anchor, radius0, radialSpeed, theta0 + 360f * k / n, omega, shot.Styled(styles[k % styles.Length]), radialAccel);
    }

    /// <summary>A point of the circle of "options" that orbit the boss: `k` of `n`, at `radius`, the circle turned by `angle`.</summary>
    public static Vector2 Orbit(Vector2 centre, float radius, int k, int n, float angle) => Round(centre, radius, angle + 360f * k / n);
}
