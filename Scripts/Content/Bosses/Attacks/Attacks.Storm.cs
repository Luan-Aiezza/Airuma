using Godot;

/// <summary>
/// The attacks of the storm (stage 3, Thaqai, the lady of lightning and the night sky): hail (white spheres), constellations (stars linked by thin lines that hang for a while), lightning (bolts that zigzag and,
/// new, LASERS of lightning that strike from the sky after a warning), and curtains of hail with a gap that swings from side to side. Made as the later bosses' are: few patterns at once (see Docs/Danmaku.md),
/// gaps for the boat (<see cref="Pat.MinGap"/>), and the ways of shooting of Touhou 6 (Docs/Touhou6.md).
/// </summary>
public static partial class Attacks
{
    private static Shot Drizzle(float speed) => new Shot(BulletStyle.BeadWhite, speed).Sized(0.8f);
    private static Shot Star(float speed, float brake) => new Shot(BulletStyle.Star, speed).Constellation(brake);

    private static readonly Color LightningYellow = new Color(1f, 0.92f, 0.4f), LightningBlue = new Color(0.55f, 0.75f, 1f);

    // A curtain is a row of hail across the field. The hitbox of a bullet grows with its scale (not its picture), so the curtains have big hitboxes (1.9) and 24 slots, 25 px apart: a row cannot be slipped
    // through between two bullets, and the boat has to go through the gap (wide, and under the boss, who stands still).

    /// <summary>A slow row of hail falls across the whole field with a wide gap that swings, and, between the rows, a fan of sparks that zigzag at the boat.</summary>
    public static BossPhase HailCurtain() => Attack("AttackHailCurtain", BossMovementType.Stationary, new Vector2(300, 140), patterns: new[]
    {
        Pat.Every(1.3f, c => Pat.Wall(c, Edge.Top, 24, 0.5f + 0.13f * Pat.Wave(c.Time, 8), 170, new Shot(BulletStyle.Hail, 100).Sized(1.9f))),
        Pat.Every(2.4f, c => Pat.Aimed(c, c.Origin, 3, 26, new Shot(BulletStyle.Spark, 165).Sized(0.9f).Zigzagging(55))).After(0.5f),
    })
        .Pressured(1.0f);

    /// <summary>
    /// Great pentagrams of stars come out of the boss and advance on the boat as one figure; after a moment each bursts: the stars of its lines fly out from its centre and its five points, bigger stars, break into
    /// small rings of little stars. Besides them there are only the stars of a constellation that hangs now and then about the field. Everything in this attack is made by stars.
    /// </summary>
    public static BossPhase StarPentagram() => Attack("AttackStarPentagram", BossMovementType.Roam, Centre, patterns: new[]
    {
        Pat.Every(4.6f, c => AdvancingPentagram(c, c.Aim + (c.Count % 2 == 0 ? 0 : 28), 105, c.Count * 17f, c.Count % 2 == 0 ? BulletStyle.Star : BulletStyle.StarBlue, c.Count % 2 == 0 ? BulletStyle.StarGold : BulletStyle.StarViolet)),
        Pat.Every(4.6f, c => Pat.Constellation(c, c.Origin, 5, c.Count * 23f, 2, Star(175, 250))).After(2.3f).NotOnEasy(),
    })
        .Pressured(2.5f);

    /// <summary>
    /// A pentagram of `radius` px (its lines of stars: `edge`; its five points, bigger stars: `tip`) that flies at the boat as one figure for 2 seconds, held in shape (the wind does not move it), and then bursts:
    /// the stars of the lines go off straight out of its centre, and each point breaks into a ring of little stars.
    /// </summary>
    private static void AdvancingPentagram(PatternContext c, float heading, float radius, float rotation, BulletStyle edge, BulletStyle tip, int perEdgeNormal = 3)
    {
        const float speed = 85, fuse = 2.0f;
        var centre = c.Origin;
        var burst = centre + Pat.Dir(heading) * speed * fuse;                 // where the centre of the figure is when it bursts
        int perEdge = perEdgeNormal;
        for (int i = 0; i < 5; i++)
        {
            var a = Pat.Round(centre, radius, rotation + 72f * (i * 2 % 5));
            var b = Pat.Round(centre, radius, rotation + 72f * ((i + 1) * 2 % 5));
            for (int j = 1; j < perEdge; j++)                                 // (j = 0 is the point itself, made apart below)
                c.Fire(a.Lerp(b, (float)j / perEdge), heading, new Shot(edge, speed).Sized(0.9f).Steady().Scattering(fuse, 150, burst));
            c.Fire(a, heading, new Shot(tip, speed).Zoomed(1.5f).Hitting(10).Steady().Splitting(fuse, 5, BulletStyle.StarGold, 140));
        }
    }

    /// <summary>
    /// Fast crossings of the field in diagonals; fans of sparks that zigzag at the boat, and LIGHTNING: after a warning line, bolts strike from the sky (one on the boat and two beside it, wide apart), thin
    /// lasers that last half a second.
    /// </summary>
    public static BossPhase ZigzagLightning() => Attack("AttackZigzagLightning", BossMovementType.Cross, new Vector2(300, 150), patterns: new[]
    {
        Pat.Every(1.8f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 3, 2, 0, 20, 220, 160, new Shot(c.Count % 2 == 0 ? BulletStyle.BoltYellow : BulletStyle.BoltBlue, 0).Zigzagging(PatternContext.Rand(30, 50)).Popping())),
        Pat.Every(3.4f, c =>
        {
            float boat = System.Math.Clamp(c.Player.X, c.Left + 40, c.Right - 40);
            var xs = new System.Collections.Generic.List<float> { boat };
            for (int tries = 0; xs.Count < 3 && tries < 30; tries++)
            {
                float x = PatternContext.Rand(c.Left + 40, c.Right - 40);
                bool apart = true; foreach (float o in xs) if (System.Math.Abs(o - x) < 170) apart = false;
                if (apart) xs.Add(x);
            }
            foreach (float x in xs) Pat.Laser(c, new Vector2(x, -10), 90 + PatternContext.Rand(-8, 8), 26, 1.1f, 0.5f, c.Count % 2 == 0 ? LightningYellow : LightningBlue);
        }).After(1.0f),
    })
        .Pressured(2.5f);

    /// <summary>
    /// The cat at play: she goes between the top left and the top right of the field and rests ten seconds at each. Balls of yarn that bounce off the sides of the field, in a fan, and claw marks in combs that go
    /// out all round her, turning a little each time.
    /// </summary>
    public static BossPhase CatPlay() => Attack("AttackCatPlay", BossMovementType.Shuttle, new Vector2(110, 130), patterns: new[]
    {
        Pat.Every(1.8f, c => Pat.Aimed(c, c.Origin, 5, 100, new Shot(c.Count % 2 == 0 ? BulletStyle.OrbViolet : BulletStyle.OrbPink, 190).Sized(1.2f).Bouncing(3))),
        Pat.Every(2.2f, c =>
        {
            float turn = c.Count * 17f;
            for (int k = 0; k < 5; k++) Pat.Comb(c, c.Origin + Pat.Dir(turn + 72f * k) * 34, turn + 72f * k, 4, 30, new Shot(BulletStyle.Claw, 170));
        }).After(0.9f),
    })
        .Pressured(2.5f);

    /// <summary>
    /// The sky falls: curtains of hail with two gaps, one the mirror of the other, and, from the sides, many slow bolts of lightning (no constellations of stars in the middle any more).
    /// </summary>
    public static BossPhase SkyFalls() => Attack("AttackSkyFalls", BossMovementType.Stationary, Centre, patterns: new[]
    {
        Pat.Every(1.4f, c => Pat.Wall(c, Edge.Top, 24, 0.27f + 0.08f * Pat.Wave(c.Time, 7), 150, new Shot(BulletStyle.Hail, 105).Sized(1.9f), mirror: true)),
        Pat.Every(1.3f, c => Pat.Rain(c, c.Count % 2 == 0 ? Edge.Left : Edge.Right, 3, 34, new Shot(c.Count % 4 < 2 ? BulletStyle.BoltYellow : BulletStyle.BoltBlue, 75))),
    })
        .Pressured(1.0f);

    /// <summary>The eclipse: a curtain of hail with a gap that swings, and, from the two sides, slow bolts of lightning, yellow and red, that cross the field, and rings of stars that burst out, stop and fly at the boat.</summary>
    public static BossPhase Eclipse() => Attack("AttackEclipse", BossMovementType.Stationary, new Vector2(300, 150), patterns: new[]
    {
        Pat.Every(1.3f, c => Pat.Wall(c, Edge.Top, 24, 0.5f + 0.13f * Pat.Wave(c.Time, 7), 170, new Shot(BulletStyle.Hail, 105).Sized(1.9f))),
        Pat.Every(1.4f, c => Pat.Rain(c, c.Count % 2 == 0 ? Edge.Left : Edge.Right, 2, 36, new Shot(c.Count % 4 < 2 ? BulletStyle.BoltYellow : BulletStyle.BoltRed, 65))),
        Pat.Every(3.6f, c => StopAndAimRing(c, c.Origin, 10, c.Count * 17f, new[] { BulletStyle.StarGold, BulletStyle.StarBlue }, 130, 0.8f, 150)).After(1.4f),
    })
        .Pressured(1.0f);

    /// <summary>
    /// (Not used by a boss now: it keeps the old curtain emitter of the engine under test.) A fine weave of hail: rows that fall quickly with a narrow gap that swings slowly, and a single slow hailstone at the boat.
    /// </summary>
    public static BossPhase FineWeave() => Attack("AttackFineWeave", BossMovementType.Stationary, new Vector2(300, 130),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.42f, BulletSpeed = 125, BulletScale = 1.9f, BulletStyle = BulletStyle.Hail,
            Curtain = true, CurtainSlots = 24, CurtainGap = 120, CurtainSwing = 80, CurtainCentre = 300, CurtainPeriod = 6.5f,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.1f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 140, IsAimed = true, BulletScale = 1.1f, BulletStyle = BulletStyle.Hail,
        })
        .Pressured(1.0f);
}
