using Godot;

/// <summary>
/// The attacks of the waterfall (stage 5, Kuychu, the guardian between the two worlds), a PROTOTYPE made from what the Amaru can do in the myth: the great
/// two-headed serpent of the rivers, the rainbow and the earthquake. The boss is two persons who share one life (the one on the left is the boss itself, the one on the right is its
/// twin, see <see cref="BossDefinition.HasTwin"/>); they fire in turns, each from where it stands, and they move apart. The water of its rivers comes as jets and as walls
/// (<see cref="SlashSpec"/>, the level ones from the side); the earth shakes when it moves under the border (<see cref="QuakeSpec"/>); and the rainbow is its bullets in the seven colours
/// (<see cref="BulletEmitterComponent.Rainbow"/>). In the first half they take turns to roam the whole screen while the other stays parked at the top (<see cref="TwinDance"/>); one of them is always where the boat can hit it.
/// </summary>
public static partial class Attacks
{
    private static readonly Vector2 LeftSide = new Vector2(170, Layout.BossY), RightSide = new Vector2(430, Layout.BossY);

    /// <summary>The left person fires and then the right one answers: each throws fans of bullets at the boat, in turns, never together.</summary>
    public static BossPhase TwoHeads() => Attack("AttackTwoHeads", BossMovementType.Stationary, LeftSide,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.6f, BulletsPerVolley = 5, SpreadAngle = 45, BulletSpeed = 215, IsAimed = true, BulletScale = 1.0f, BulletStyle = BulletStyle.OrbGreen,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.6f, TimeSinceLastFire = -0.8f, BulletsPerVolley = 5, SpreadAngle = 45, BulletSpeed = 215, IsAimed = true, BulletScale = 1.0f, BulletStyle = BulletStyle.OrbRed, FromTwin = true,
        }, dance: true);

    /// <summary>The rainbow: from each of the two, seven arms of bullets in the seven colours, turning in opposite senses, bend as they go in great arcs that cross in the middle.</summary>
    public static BossPhase SevenArcs() => Attack("AttackSevenArcs", BossMovementType.Stationary, LeftSide,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.24f, BulletsPerVolley = 7, SpreadAngle = 360, BulletSpeed = 150, SpinRate = 3.4f, CurveRate = 32, BulletScale = 0.95f, Rainbow = true, BulletStyle = BulletStyle.OrbRed,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.24f, BulletsPerVolley = 7, SpreadAngle = 360, BulletSpeed = 150, SpinRate = -3.4f, CurveRate = -32, BulletScale = 0.95f, Rainbow = true, BulletStyle = BulletStyle.OrbRed, FromTwin = true,
        }, dance: true);

    /// <summary>The earthquake: the border shakes, the screen with it, and drops of the falls come down from above in a crowd (the left one stamps); between the tremors, fans at the boat from the right one.</summary>
    public static BossPhase BorderQuake() => Attack("AttackBorderQuake", BossMovementType.Stationary, LeftSide,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.1f, BulletsPerVolley = 1, BulletSpeed = 300, BulletScale = 1.3f, BulletStyle = BulletStyle.Hail, Ceiling = true, QuakeOnly = true,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.4f, BulletsPerVolley = 3, SpreadAngle = 30, BulletSpeed = 200, IsAimed = true, BulletScale = 1.0f, BulletStyle = BulletStyle.VenomOrb, FromTwin = true,
        },
        quake: new QuakeSpec { IsActive = true, Interval = 3.0f, Duration = 3.6f, Intensity = 1f }, dance: true);

    /// <summary>The tide: walls of water cross the field from the side, level, one above the other, each warned of by a line; the boat has to go up or down between them. The left one stands; the right one drifts about.</summary>
    public static BossPhase Tide() => Attack("AttackTide", BossMovementType.Stationary, LeftSide,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.0f, BulletsPerVolley = 2, SpreadAngle = 12, BulletSpeed = 230, IsAimed = true, BulletScale = 0.9f, BulletStyle = BulletStyle.Wisp,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.4f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 190, IsAimed = true, BulletScale = 1.1f, BulletStyle = BulletStyle.LotusPetal, FromTwin = true,
        },
        slashes: new SlashSpec { IsActive = true, Interval = 2.7f, Telegraph = 1.1f, Strike = 0.45f, Width = 44, Spacing = 200, Count = 2, Angle = 84, Aim = SlashAim.Player },
        twinTarget: RightSide, dance: false);

    /// <summary>The trial of worth: the two fire together, face to face, rings that turn in opposite senses and cross in the middle in a lattice of colours.</summary>
    public static BossPhase Trial() => Attack("AttackTrial", BossMovementType.Stationary, LeftSide,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.75f, BulletsPerVolley = 16, SpreadAngle = 360, BulletSpeed = 130, SpinRate = 7.5f, BulletScale = 0.95f, Rainbow = true, BulletStyle = BulletStyle.OrbRed,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.75f, BulletsPerVolley = 16, SpreadAngle = 360, BulletSpeed = 130, SpinRate = -7.5f, BulletScale = 0.95f, Rainbow = true, BulletStyle = BulletStyle.OrbRed, FromTwin = true,
        }, dance: true);

    // From here the two are ONE (the second half of the fight): the second glides to the first and they fuse, and the one body that is left fires from the two halves of itself.
    private static readonly Vector2 Fusion = new Vector2(300, Layout.BossY + 50);   // the fused body is big: lower, so that its head is clear of the boss bar

    /// <summary>The broken rainbow (enraged, fused): the one body glides about the field while two spirals of the seven colours, turning in opposite senses, curve out of it, and jets of water cut down the field.</summary>
    public static BossPhase BrokenRainbow() => Attack("AttackBrokenRainbow", BossMovementType.Stationary, Fusion,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.24f, BulletsPerVolley = 7, SpreadAngle = 360, BulletSpeed = 160, SpinRate = -4.1f, CurveRate = 40, BulletScale = 0.95f, Rainbow = true, BulletStyle = BulletStyle.OrbRed,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.24f, TimeSinceLastFire = -0.12f, BulletsPerVolley = 7, SpreadAngle = 360, BulletSpeed = 160, SpinRate = 4.1f, CurveRate = -40, BulletScale = 0.95f, Rainbow = true, BulletStyle = BulletStyle.OrbRed,
        },
        slashes: new SlashSpec { IsActive = true, Interval = 2.2f, Telegraph = 0.85f, Strike = 0.25f, Width = 36, Spacing = 155, Count = 3, Angle = 0, Aim = SlashAim.Player },
        fused: true);

    /// <summary>The tremor between two worlds (enraged, fused): the one body stamps, the screen shakes almost without pause while the drops of the falls pour down, fans of the seven colours fly at the boat, and a roar throws it back.</summary>
    public static BossPhase TwoWorlds() => Attack("AttackTwoWorlds", BossMovementType.Stationary, Fusion,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.075f, BulletsPerVolley = 1, BulletSpeed = 330, BulletScale = 1.3f, BulletStyle = BulletStyle.Hail, Ceiling = true, QuakeOnly = true,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.2f, BulletsPerVolley = 9, SpreadAngle = 70, BulletSpeed = 230, IsAimed = true, BulletScale = 1.0f, Rainbow = true, BulletStyle = BulletStyle.OrbRed,
        },
        quake: new QuakeSpec { IsActive = true, Interval = 1.0f, Duration = 4.2f, Intensity = 1.4f },
        roar: new RoarSpec { IsActive = true, Interval = 4.6f, Push = 220, Seconds = 0.5f },
        fused: true);

    /// <summary>The guardian (enraged, fused): everything of the border at once, from the one body that has two halves. A fan of the seven colours from the left half and another from the right half,
    /// in turns; jets and walls of water on the slant, the tremor and the roar.</summary>
    public static BossPhase Guardian() => Attack("AttackGuardian", BossMovementType.Stationary, Fusion,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.3f, BulletsPerVolley = 7, SpreadAngle = 55, BulletSpeed = 235, IsAimed = true, BulletScale = 1.0f, Rainbow = true, BulletStyle = BulletStyle.OrbRed, SpawnOffset = new Vector2(-34, 0),
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.3f, TimeSinceLastFire = -0.65f, BulletsPerVolley = 7, SpreadAngle = 55, BulletSpeed = 235, IsAimed = true, BulletScale = 1.0f, Rainbow = true, BulletStyle = BulletStyle.OrbGreen, SpawnOffset = new Vector2(34, 0),
        },
        slashes: new SlashSpec { IsActive = true, Interval = 2.4f, Telegraph = 0.9f, Strike = 0.3f, Width = 38, Spacing = 145, Count = 4, Angle = 20, Aim = SlashAim.Alternate },
        quake: new QuakeSpec { IsActive = true, Interval = 5.0f, Duration = 2.2f, Intensity = 1.1f },
        roar: new RoarSpec { IsActive = true, Interval = 6.5f, Push = 220, Seconds = 0.5f },
        fused: true);
}
