using Godot;

/// <summary>
/// The attacks of the storm (stage 3, Thaqai): hail (white spheres), constellations (stars linked by thin lines that hang for a while), lightning
/// (bolts that zigzag), and curtains of hail with a gap that swings from side to side, which make the boat move in small steps (the micro-movement
/// of the later bosses). The boss is fast and crosses the field in diagonals.
/// </summary>
public static partial class Attacks
{
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
    /// A pentagram of stars thrown out from the boss as she roams: they brake, hang linked by thin lines (which hurt) and then fall. Between
    /// the pentagrams, a slow trickle of hail at the boat.
    /// </summary>
    public static BossPhase StarPentagram() => Attack("AttackStarPentagram", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 2.0f, BulletsPerVolley = 5, SpreadAngle = 360, BulletSpeed = 175, SpinRate = 23, Acceleration = -250,
            BulletScale = 1.0f, BulletStyle = BulletStyle.Star, BulletBehavior = BulletBehavior.Constellation, Link = true, LinkSkip = 2,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.8f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 150, IsAimed = true, BulletScale = 0.9f, BulletStyle = BulletStyle.Hail,
        });

    /// <summary>Fast crossings of the field in diagonals; bolts of lightning that zigzag fly at the boat, and hail falls from where she passes.</summary>
    public static BossPhase ZigzagLightning() => Attack("AttackZigzagLightning", BossMovementType.Cross, new Vector2(300, 150),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.4f, BulletsPerVolley = 3, SpreadAngle = 36, BulletSpeed = 230, IsAimed = true, CurveRate = 62,
            BulletScale = 0.9f, BulletStyle = BulletStyle.Spark, BulletBehavior = BulletBehavior.Zigzag,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.14f, BulletsPerVolley = 1, BaseAngle = 90, BulletSpeed = 150, BulletScale = 0.8f, BulletStyle = BulletStyle.Hail,
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
