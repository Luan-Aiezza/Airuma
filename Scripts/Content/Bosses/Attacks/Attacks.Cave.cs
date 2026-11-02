using Godot;

/// <summary>
/// The attacks of the cave (stage 4, Ukhumo, the hungry bear girl, who never leaves the rock platform where she is trapped: she cannot swim). They are the attacks
/// of an animal, of claws and of brute force, and each is different from the others in what it asks of the player, not only in its bullets:
///   the claws        three red slashes cut down the field, warned of by a thin line (<see cref="SlashSpec"/>);
///   the roar         she roars and the boat is pushed away from her, into the bullets (<see cref="RoarSpec"/>);
///   the quake        she stamps, the whole screen shakes and stalactites fall from the ceiling (<see cref="QuakeSpec"/>);
///   the boulder      a great stone is thrown that breaks in the air into a ring of small ones (<see cref="BulletBehavior.Burst"/>).
/// </summary>
public static partial class Attacks
{
    /// <summary>The claws: three red, jagged cuts come down the field one beside the other, each warned of by a thin line; between them there is room to stand. Fans of the marks of claws fly at the boat between the cuts.</summary>
    public static BossPhase ClawRake() => Attack("AttackClawRake", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.7f, BulletsPerVolley = 3, SpreadAngle = 30, BulletSpeed = 190, IsAimed = true, BulletScale = 1.0f, BulletStyle = BulletStyle.Claw,
        },
        slashes: new SlashSpec { IsActive = true, Interval = 2.6f, Telegraph = 0.95f, Strike = 0.22f, Width = 34, Spacing = 125, Count = 3, Angle = 0, Aim = SlashAim.Player });

    /// <summary>The roar: every few seconds she roars and the boat is thrown back from her, away from the cave, towards the rings of claw marks that spread from her and the stones she flings.</summary>
    public static BossPhase RoarOfTheMountain() => Attack("AttackRoar", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.5f, BulletsPerVolley = 24, SpreadAngle = 360, BulletSpeed = 125, SpinRate = 7.5f, BulletScale = 1.0f, BulletStyle = BulletStyle.Claw,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.1f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 215, IsAimed = true, BulletScale = 1.1f, BulletStyle = BulletStyle.Rock,
        },
        roar: new RoarSpec { IsActive = true, Interval = 3.6f, Push = 240, Seconds = 0.55f });

    /// <summary>The stamp: she strikes the ground, the whole screen shakes for a few seconds and stalactites fall from the ceiling at random; between the quakes, stones at the boat.</summary>
    public static BossPhase Stomp() => Attack("AttackStomp", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.11f, BulletsPerVolley = 1, BulletSpeed = 330, BulletScale = 1.3f, BulletStyle = BulletStyle.Stalactite, Ceiling = true, QuakeOnly = true,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.0f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 190, IsAimed = true, BulletScale = 1.0f, BulletStyle = BulletStyle.Rock,
        },
        quake: new QuakeSpec { IsActive = true, Interval = 3.2f, Duration = 3.4f, Intensity = 1f });

    /// <summary>Brute force: she throws a great stone at the boat, which breaks in the air into a ring of small ones; now and then she brings a single heavy claw down on the boat.</summary>
    public static BossPhase BruteForce() => Attack("AttackBruteForce", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 2.3f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 185, IsAimed = true, BulletScale = 3.0f, BulletStyle = BulletStyle.Rock, BulletBehavior = BulletBehavior.Burst,
        },
        slashes: new SlashSpec { IsActive = true, Interval = 3.8f, Telegraph = 1.0f, Strike = 0.25f, Width = 46, Spacing = 0, Count = 1, Angle = 0, Aim = SlashAim.Player });

    /// <summary>Fury, the claws crossed: the cuts come on the slant, three at a time, from one side and then from the other, quicker, over fans of claw marks.</summary>
    public static BossPhase CrossingClaws() => Attack("AttackCrossingClaws", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 2.0f, BulletsPerVolley = 5, SpreadAngle = 60, BulletSpeed = 230, IsAimed = true, BulletScale = 1.0f, BulletStyle = BulletStyle.Claw,
        },
        slashes: new SlashSpec { IsActive = true, Interval = 1.8f, Telegraph = 0.8f, Strike = 0.2f, Width = 30, Spacing = 105, Count = 3, Angle = 30, Aim = SlashAim.Alternate });

    /// <summary>Fury, the cave comes down: the screen shakes almost without a pause, stalactites fall in a crowd and great stones fall that break in a ring when they have fallen a while; now and then a roar throws the boat back.</summary>
    public static BossPhase CaveIn() => Attack("AttackCaveIn", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.085f, BulletsPerVolley = 1, BulletSpeed = 360, BulletScale = 1.3f, BulletStyle = BulletStyle.Stalactite, Ceiling = true, QuakeOnly = true,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.55f, BulletsPerVolley = 1, BulletSpeed = 200, BulletScale = 2.4f, BulletStyle = BulletStyle.Rock, BulletBehavior = BulletBehavior.Burst, Ceiling = true, QuakeOnly = true,
        },
        quake: new QuakeSpec { IsActive = true, Interval = 1.0f, Duration = 4.4f, Intensity = 1.4f },
        roar: new RoarSpec { IsActive = true, Interval = 5.0f, Push = 210, Seconds = 0.5f });

    /// <summary>Fury, the feast: everything at once. Great stones that break, rings of claw marks, four slanting cuts at a time, short quakes and a roar.</summary>
    public static BossPhase Feast() => Attack("AttackFeast", BossMovementType.Stationary, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 2.8f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 195, IsAimed = true, BulletScale = 3.0f, BulletStyle = BulletStyle.Rock, BulletBehavior = BulletBehavior.Burst,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 2.2f, BulletsPerVolley = 20, SpreadAngle = 360, BulletSpeed = 140, SpinRate = 9, BulletScale = 1.0f, BulletStyle = BulletStyle.Claw,
        },
        slashes: new SlashSpec { IsActive = true, Interval = 2.3f, Telegraph = 0.85f, Strike = 0.22f, Width = 30, Spacing = 95, Count = 4, Angle = 12, Aim = SlashAim.Alternate },
        quake: new QuakeSpec { IsActive = true, Interval = 4.5f, Duration = 2.2f, Intensity = 1.1f },
        roar: new RoarSpec { IsActive = true, Interval = 7.0f, Push = 220, Seconds = 0.5f });
}
