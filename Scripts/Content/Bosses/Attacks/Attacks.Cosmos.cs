using Godot;

/// <summary>
/// The attacks of the river of stars (stage 6, Mama Killa). Her first six attacks are those of the storm (stage 3), kept until she has attacks of her own; the four new ones are common attacks, made of the
/// same simple emitters as the others (two emitters each), to be adjusted one by one: two more in her first form, the crescent and the tide, and two more in her fury, the eclipse and the eternal night.
/// </summary>
public static partial class Attacks
{
    /// <summary>The crescent moon: fans of stars at the boat, and a ring of silver orbs that turns.</summary>
    public static BossPhase KillaCrescent() => Attack("AttackKillaCrescent", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.4f, BulletsPerVolley = 7, SpreadAngle = 60, BulletSpeed = 200, IsAimed = true, BulletScale = 1.0f, BulletStyle = BulletStyle.Star,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.9f, BulletsPerVolley = 18, SpreadAngle = 360, BulletSpeed = 120, SpinRate = 9, BulletScale = 1.0f, BulletStyle = BulletStyle.OrbGreen,
        });

    /// <summary>The tide: a curtain of hail with a gap that swings, and fans of hail at the boat.</summary>
    public static BossPhase KillaTide() => Attack("AttackKillaTide", BossMovementType.Stationary, new Vector2(300, 140),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.5f, BulletSpeed = 115, BulletScale = 1.9f, BulletStyle = BulletStyle.Hail,
            Curtain = true, CurtainSlots = 24, CurtainGap = 125, CurtainSwing = 80, CurtainCentre = 300, CurtainPeriod = 8,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.6f, BulletsPerVolley = 5, SpreadAngle = 40, BulletSpeed = 195, IsAimed = true, BulletScale = 0.9f, BulletStyle = BulletStyle.Hail,
        });

    /// <summary>The eclipse (enraged): pentagrams of stars hang about the field, over a ring of dancing orbs.</summary>
    public static BossPhase KillaEclipse() => Attack("AttackKillaEclipse", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 2.0f, BulletsPerVolley = 5, SpreadAngle = 360, BulletSpeed = 175, SpinRate = 23, Acceleration = -250,
            BulletScale = 1.0f, BulletStyle = BulletStyle.Star, BulletBehavior = BulletBehavior.Constellation, Link = true, LinkSkip = 2,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat, BulletsPerVolley = 14, SpreadAngle = 360, BulletSpeed = 125, SpinRate = 12,
            CurveRate = 80, AlternateCurve = true, BulletScale = 1.0f, BulletBehavior = BulletBehavior.Dance, BulletStyle = BulletStyle.VenomOrb,
        });

    /// <summary>The eternal night (enraged): two gaps in a curtain of hail, a spiral of orbs, and a wide fan at the boat.</summary>
    public static BossPhase KillaEternalNight() => Attack("AttackKillaEternalNight", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.5f, BulletSpeed = 130, BulletScale = 1.9f, BulletStyle = BulletStyle.Hail,
            Curtain = true, CurtainSlots = 24, CurtainGap = 110, CurtainSwing = 60, CurtainCentre = 300, CurtainPeriod = 7, CurtainMirror = true,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.12f, BulletsPerVolley = 3, SpreadAngle = 360, BulletSpeed = 140, SpinRate = 17, BulletScale = 0.9f, BulletStyle = BulletStyle.OrbRed,
        });
}
