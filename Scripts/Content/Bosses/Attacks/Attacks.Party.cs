using Godot;

/// <summary>The attacks of the party (stage 2, Baylo): his bullets dance, sway on a half-second beat, and he dives and fires a great jet.</summary>
public static partial class Attacks
{
    /// <summary>Two rings that turn in opposite senses with neighbours swaying against each other, while the dancer roams the field; between the beats, a fan at the boat.</summary>
    public static BossPhase PartyTwoStep() => Attack("AttackDoubleStep", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat, BulletsPerVolley = 12, SpreadAngle = 360, BulletSpeed = 135, SpinRate = 14,
            CurveRate = 100, AlternateCurve = true, BulletScale = 0.9f, BulletBehavior = BulletBehavior.Dance, BulletStyle = BulletStyle.VenomOrb,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat * 3, BulletsPerVolley = 5, SpreadAngle = 50, BulletSpeed = 210,
            IsAimed = true, BulletScale = 0.9f, BulletStyle = BulletStyle.OrbGreen,
        });

    /// <summary>Aimed bursts of confetti that sway as they come, and a curtain of confetti that falls from the dancer wherever he goes.</summary>
    public static BossPhase ConfettiRain() => Attack("AttackConfettiRain", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat * 2, BulletsPerVolley = 9, SpreadAngle = 80, BulletSpeed = 200,
            IsAimed = true, CurveRate = 80, AlternateCurve = true, BulletScale = 1.0f, BulletBehavior = BulletBehavior.Dance, BulletStyle = BulletStyle.RiceRed,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.12f, BulletsPerVolley = 1, BaseAngle = 90, BulletSpeed = 90,
            Acceleration = 45, BulletScale = 0.8f, BulletStyle = BulletStyle.Spark,
        });

    /// <summary>A turning wheel of swaying petals, one volley a beat, and claps: two thin arms of orbs that spin the other way.</summary>
    public static BossPhase SambaWheel() => Attack("AttackSambaWheel", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat, BulletsPerVolley = 16, SpreadAngle = 360, BulletSpeed = 120, SpinRate = 11,
            CurveRate = 70, AlternateCurve = true, BulletScale = 1.0f, BulletBehavior = BulletBehavior.Dance, BulletStyle = BulletStyle.LotusPetal,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.1f, BulletsPerVolley = 2, SpreadAngle = 180, BulletSpeed = 150, SpinRate = -17,
            BulletScale = 0.7f, BulletStyle = BulletStyle.OrbGreen,
        });

    /// <summary>
    /// The dolphin dives and a shadow lies under the boat, following it a while and then holding still; he comes out of the water at the
    /// shadow (the dive backwards), with a fan of spray. Ringed bubbles that sway while he is on the surface. Hard adds a three-way at the boat.
    /// </summary>
    public static BossPhase GreatLeap() => Attack("AttackDolphinLeap", BossMovementType.Dive, new Vector2(300, MovementSystem.DiveTopY),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat, BulletsPerVolley = 10, SpreadAngle = 360, BulletSpeed = 130, SpinRate = 9,
            CurveRate = 90, AlternateCurve = true, BulletScale = 1.0f, BulletBehavior = BulletBehavior.Dance, BulletStyle = BulletStyle.VenomOrb,
        },
        hardExtra: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.0f, BulletsPerVolley = 3, SpreadAngle = 30, BulletSpeed = 260,
            IsAimed = true, BulletScale = 0.8f, BulletStyle = BulletStyle.OrbGreen,
        });

    /// <summary>
    /// A great, wide jet of water. The boss goes to the left edge and charges, then sweeps the jet to the right, stopping short of the right
    /// edge so that the boat has a place to stand; crosses to the right edge, charges again and sweeps back to the left, stopping short of the
    /// left edge; then returns to the middle and rests a few seconds, for the boat to hit it. It fires nothing but the jet.
    /// </summary>
    public static BossPhase WaterJet() => Attack("AttackPartyWave", BossMovementType.Sweep, new Vector2(300, MovementSystem.SweepY),
        main: new BulletEmitterComponent());
}
