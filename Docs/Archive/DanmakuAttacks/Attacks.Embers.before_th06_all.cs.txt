using Godot;

/// <summary>The attacks of fire and embers: the Iryssh uses four of them; the other two are kept for later bosses.</summary>
public static partial class Attacks
{
    /// <summary>Simple, but alive: three arms of fire that spiral out of her as she goes from station to station, and, now and then, a slow ring of orbs that comes between them.</summary>
    public static BossPhase RingOfEmbers() => Attack("AttackRingOfEmbers", BossMovementType.Patrol, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.14f, BulletsPerVolley = 3,
            SpreadAngle = 360, BulletSpeed = 165, SpinRate = 8, BulletScale = 0.9f, BulletStyle = BulletStyle.Fireball,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 2.6f, BulletsPerVolley = 18, SpreadAngle = 360, BulletSpeed = 115,
            SpinRate = 10, BulletScale = 1.2f, BulletStyle = BulletStyle.OrbRed,
        });

    /// <summary>An aimed shotgun. Hard adds a fast dart straight at the boat between the volleys.</summary>
    public static BossPhase PiercingTongue() => Attack("AttackPiercingTongue", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.8f, BulletsPerVolley = 20,
            SpreadAngle = 120, BulletSpeed = 250, IsAimed = true, BulletScale = 1.2f, BulletStyle = BulletStyle.FlameDart,
        },
        hardExtra: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.4f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 420,
            IsAimed = true, BulletScale = 1.0f, BulletStyle = BulletStyle.FlameDart,
        });

    /// <summary>Two spinners, one at each side.</summary>
    public static BossPhase TwinWhirlwinds() => Attack("AttackTwinWhirlwinds", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.08f, BulletsPerVolley = 2,
            SpreadAngle = 180, BaseAngle = 0, BulletSpeed = 180, SpinRate = 12,
            BulletScale = 0.8f, SpawnOffset = new Vector2(-150, 0), BulletStyle = BulletStyle.Spark,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.08f, BulletsPerVolley = 2,
            SpreadAngle = 180, BaseAngle = 0, BulletSpeed = 180, SpinRate = 12,
            BulletScale = 0.8f, SpawnOffset = new Vector2(150, 0), BulletStyle = BulletStyle.VenomOrb,
        });

    /// <summary>A lotus whose petals brake, curve and then dart at the boat. Hard adds a ring of slow embers.</summary>
    public static BossPhase BlazingBloom() => Attack("AttackBlazingBloom", BossMovementType.Patrol, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.5f, BulletsPerVolley = 20,
            SpreadAngle = 360, BulletSpeed = 300, SpinRate = 10,
            Acceleration = -250, CurveRate = 45, BulletScale = 1.2f,
            BulletBehavior = BulletBehavior.StopAndAim, BulletStyle = BulletStyle.LotusPetal,
        },
        hardExtra: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.2f, BulletsPerVolley = 10, SpreadAngle = 360, BulletSpeed = 140,
            SpinRate = 8, BulletScale = 0.9f, BulletStyle = BulletStyle.Fireball,
        });

    /// <summary>The serpent: big magma orbs at the boat, and a thin spiral of serpents of fire.</summary>
    public static BossPhase FuryOfTheRiver() => Attack("AttackFuryOfTheRiver", BossMovementType.Stationary, new Vector2(300, 150),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.5f, BulletsPerVolley = 5,
            SpreadAngle = 60, BulletSpeed = 350, IsAimed = true, BulletScale = 4.0f, BulletStyle = BulletStyle.MagmaOrb,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.05f, BulletsPerVolley = 2,
            SpreadAngle = 180, BulletSpeed = 150, SpinRate = 35, BulletScale = 0.5f, BulletStyle = BulletStyle.FireSerpent,
        });

    /// <summary>The serpent lunges down the screen from one of three lanes, leaving embers in its wake (none on easy; see EmbersOneStepDown).</summary>
    public static BossPhase TideOfEmbers() => Attack("AttackTideOfEmbers", BossMovementType.Dash, new Vector2(300, -50), startsAtTarget: true,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.12f, BulletsPerVolley = 8,
            SpreadAngle = 360, BulletSpeed = 110, SpinRate = 15, BulletScale = 0.5f, BulletStyle = BulletStyle.Ember,
        }, embersOneStepDown: true);
}
