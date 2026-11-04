using Godot;

/// <summary>
/// The attacks of the storm (stage 3, Thaqai): hail (white spheres), constellations (stars linked by thin lines that hang for a while), lightning
/// (bolts that zigzag), and curtains of hail with a gap that swings from side to side, which make the boat move in small steps (the micro-movement
/// of the later bosses). The boss is fast and crosses the field in diagonals.
/// </summary>
public static partial class Attacks
{
    private static Shot Drizzle(float speed) => new Shot(BulletStyle.BeadWhite, speed).Sized(0.8f);
    private static Shot Star(float speed, float brake) => new Shot(BulletStyle.Star, speed).Constellation(brake);

    // A curtain is a row of hail across the field. The hitbox of a bullet grows with its scale (not its picture), so the curtains have big hitboxes (1.9) and
    // 24 slots, 25 px apart: a row cannot be slipped through between two bullets, and the boat has to go through the gap.

    /// <summary>A slow row of hail that falls across the whole field with a wide gap that swings, and fans of hail at the boat between the rows.</summary>
    public static BossPhase HailCurtain() => Attack("AttackHailCurtain", BossMovementType.Stationary, new Vector2(300, 140),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.55f, BulletSpeed = 110, BulletScale = 1.9f, BulletStyle = BulletStyle.Hail,
            Curtain = true, CurtainSlots = 24, CurtainGap = 120, CurtainSwing = 170, CurtainCentre = 300, CurtainPeriod = 10,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.8f, BulletsPerVolley = 5, SpreadAngle = 40, BulletSpeed = 190, IsAimed = true, BulletScale = 0.9f, BulletStyle = BulletStyle.Hail,
        });

    /// <summary>
    /// Great pentagrams of stars come out of the boss and advance on the boat as one figure; after a moment each bursts: the stars of its lines fly out from its centre and its five points, bigger stars, break into
    /// rings of little stars that spread. Between them, hail at the boat, a drizzle of beads, and a constellation hung about the field.
    /// </summary>
    public static BossPhase StarPentagram() => Attack("AttackStarPentagram", BossMovementType.Roam, Centre, patterns: new[]
    {
        Pat.Every(2.6f, c => AdvancingPentagram(c, c.Aim, 105, c.Count * 17f, BulletStyle.Star, BulletStyle.StarGold)),
        Pat.Every(2.6f, c => AdvancingPentagram(c, c.Aim + (c.Count % 2 == 0 ? 32 : -32), 80, c.Count * -23f + 36, BulletStyle.StarBlue, BulletStyle.StarViolet)).After(1.3f),
        Pat.Every(0.8f, c => Pat.Aimed(c, c.Origin, 3, 24, new Shot(BulletStyle.Hail, 150).Sized(0.9f))),
        Pat.Every(0.1f, c => Pat.Rain(c, Edge.Top, 3, 6, Drizzle(130))),
        Pat.Every(2.4f, c => Pat.Constellation(c, c.Origin, 5, c.Count * 23f, 2, Star(175, 250))).After(0.6f).NotOnEasy(),
    });

    /// <summary>
    /// A pentagram of `radius` px (its lines of stars: `edge`; its five points, bigger stars: `tip`) that flies at the boat as one figure for 2 seconds, held in shape (the wind does not move it), and then bursts:
    /// the stars of the lines go off straight out of its centre, and each point breaks into a ring of little stars.
    /// </summary>
    private static void AdvancingPentagram(PatternContext c, float heading, float radius, float rotation, BulletStyle edge, BulletStyle tip)
    {
        const float speed = 85, fuse = 2.0f;
        var centre = c.Origin;
        var burst = centre + Pat.Dir(heading) * speed * fuse;                 // where the centre of the figure is when it bursts
        int perEdge = c.Difficulty == Difficulty.Easy ? 4 : 6;
        for (int i = 0; i < 5; i++)
        {
            var a = Pat.Round(centre, radius, rotation + 72f * (i * 2 % 5));
            var b = Pat.Round(centre, radius, rotation + 72f * ((i + 1) * 2 % 5));
            for (int j = 1; j < perEdge; j++)                                 // (j = 0 is the point itself, made apart below)
                c.Fire(a.Lerp(b, (float)j / perEdge), heading, new Shot(edge, speed).Sized(0.9f).Steady().Scattering(fuse, 150, burst));
            c.Fire(a, heading, new Shot(tip, speed).Zoomed(1.5f).Hitting(10).Steady().Splitting(fuse, 8, BulletStyle.StarGold, 140));
        }
    }

    /// <summary>Fast crossings of the field in diagonals; fans of sparks of lightning that zigzag at the boat, and a storm of little bolts, of every speed, sent in every direction: from the boss, from the top and from the sides.</summary>
    public static BossPhase ZigzagLightning() => Attack("AttackZigzagLightning", BossMovementType.Cross, new Vector2(300, 150),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.4f, BulletsPerVolley = 5, SpreadAngle = 50, BulletSpeed = 230, IsAimed = true, CurveRate = 62,
            BulletScale = 0.9f, BulletStyle = BulletStyle.Spark, BulletBehavior = BulletBehavior.Zigzag,
        },
        patterns: new[]
        {
            // little bolts from the boss in all the directions below her, every one at its own speed, some zigzagging
            Pat.Every(0.08f, c =>
            {
                for (int k = 0; k < c.N(3, 1); k++)
                {
                    var bolt = new Shot(c.Count % 2 == 0 ? BulletStyle.BoltYellow : BulletStyle.BoltBlue, PatternContext.Rand(140, 300));
                    if ((c.Count + k) % 3 == 0) bolt = bolt.Zigzagging(PatternContext.Rand(30, 55));
                    c.Fire(c.Origin, 90 + PatternContext.Rand(-80, 80), bolt);
                }
            }),
            // bolts that fall from the top at random places and slants, quick
            Pat.Every(0.14f, c => Pat.Rain(c, Edge.Top, 3, 70, new Shot(BulletStyle.BoltYellow, 250))),
            // bolts from the sides, level and slanting
            Pat.Every(0.22f, c => Pat.Rain(c, c.Count % 2 == 0 ? Edge.Left : Edge.Right, 3, 70, new Shot(BulletStyle.BoltBlue, 230))),
            Pat.Every(1.6f, c => Pat.Aimed(c, c.Origin, 9, 90, new Shot(c.Count % 2 == 0 ? BulletStyle.BoltBlue : BulletStyle.BoltYellow, 290))).After(0.7f).NotOnEasy(),
            Pat.Every(2.6f, c => Pat.Polygon(c, c.Origin, 6, 62, c.Count * 13f, 3, new Shot(BulletStyle.Spark, 1).Sized(0.9f).LaunchedOut(0.55f, 210))).After(1),
        });

    /// <summary>
    /// The cat at play: she crosses the field in quick diagonals, throwing balls of yarn that bounce off the sides of the field, claw marks in combs that sweep across the boat, a hail of small beads from the sides,
    /// and rings of pearls that open slowly.
    /// </summary>
    public static BossPhase CatPlay() => Attack("AttackCatPlay", BossMovementType.Cross, new Vector2(300, 150), patterns: new[]
    {
        Pat.Every(1.4f, c => Pat.Aimed(c, c.Origin, 9, 100, new Shot(c.Count % 2 == 0 ? BulletStyle.OrbViolet : BulletStyle.OrbPink, 200).Sized(1.2f).Bouncing(3))),
        Pat.Every(1.1f, c => { for (int k = -1; k <= 1; k++) Pat.Comb(c, c.Origin + Pat.Dir(c.Aim + k * 26) * 30, c.Aim + k * 26, 5, 20, new Shot(BulletStyle.Claw, 220)); }).After(0.5f),
        Pat.Every(0.12f, c => Pat.Rain(c, c.Count % 2 == 0 ? Edge.Left : Edge.Right, 2, 40, new Shot(BulletStyle.BeadWhite, 150).Sized(0.8f))),
        Pat.Every(2.0f, c => Pat.Ring(c, c.Origin, 22, c.Count * 9f, new Shot(BulletStyle.BeadPink, 95).Sized(0.9f))).After(1),
        Pat.Every(0.1f, c => Pat.Rain(c, Edge.Top, 2, 6, Drizzle(140))),
    });

    /// <summary>
    /// A fine weave of hail: rows that fall quickly with a narrow gap that swings slowly, so that the boat follows it in small moves (hold Shift),
    /// and a single slow hailstone at the boat now and then.
    /// </summary>
    public static BossPhase FineWeave() => Attack("AttackFineWeave", BossMovementType.Stationary, new Vector2(300, 130),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.42f, BulletSpeed = 125, BulletScale = 1.9f, BulletStyle = BulletStyle.Hail,
            Curtain = true, CurtainSlots = 24, CurtainGap = 66, CurtainSwing = 130, CurtainCentre = 300, CurtainPeriod = 7.5f,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.1f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 140, IsAimed = true, BulletScale = 1.1f, BulletStyle = BulletStyle.Hail,
        });

    /// <summary>The sky falls: curtains of hail with two gaps, one the mirror of the other, while triangles of stars hang about the field.</summary>
    public static BossPhase SkyFalls() => Attack("AttackSkyFalls", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.48f, BulletSpeed = 130, BulletScale = 1.9f, BulletStyle = BulletStyle.Hail,
            Curtain = true, CurtainSlots = 24, CurtainGap = 82, CurtainSwing = 150, CurtainCentre = 300, CurtainPeriod = 8, CurtainMirror = true,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 2.4f, BulletsPerVolley = 3, SpreadAngle = 360, BulletSpeed = 170, SpinRate = 31, Acceleration = -240,
            BulletScale = 1.0f, BulletStyle = BulletStyle.Star, BulletBehavior = BulletBehavior.Constellation, Link = true, LinkSkip = 1,
        });

    /// <summary>The eclipse: she crosses the field in diagonals, hanging heptagrams of stars behind her, over a fine weave of hail.</summary>
    public static BossPhase Eclipse() => Attack("AttackEclipse", BossMovementType.Cross, new Vector2(300, 150),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.6f, BulletsPerVolley = 7, SpreadAngle = 360, BulletSpeed = 190, SpinRate = 17, Acceleration = -270,
            BulletScale = 1.0f, BulletStyle = BulletStyle.Star, BulletBehavior = BulletBehavior.Constellation, Link = true, LinkSkip = 3,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.5f, BulletSpeed = 105, BulletScale = 1.9f, BulletStyle = BulletStyle.Hail,
            Curtain = true, CurtainSlots = 24, CurtainGap = 78, CurtainSwing = 120, CurtainCentre = 300, CurtainPeriod = 8,
        });
}
