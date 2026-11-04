using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 5, the waterfall of the seven colours: the hardest so far. The red spirits (type 3) now come in PAIRS, one at each side, with patterns that
/// mirror each other (one turns one way, the other the other way); a pair comes alone with the gunners, then another, and the last one is joined by a
/// crowd of the little spirits (type 1). A lone red spirit still comes now and then, with the others, as in stage 4. The shots are the colours of the rainbow.
/// </summary>
public static class Stage5Waves
{
    /// <summary>How long a red spirit stays, at most, once it has settled.</summary>
    public const float HeavyHold = 24;

    private static List<ScheduledPattern> RainbowFan => SpiritFire.RainbowFan;
    private static List<ScheduledPattern> BlueRing => SpiritFire.BlueRing;
    private static List<ScheduledPattern> VioletBolts => SpiritFire.VioletBolts;
    private static List<ScheduledPattern> GreenSpiral => SpiritFire.GreenSpiral;

    /// <summary>The left one of a pair: spirals that turn clockwise, a wide fan and a ring of dancing blue.</summary>
    private static List<ScheduledPattern> LeftOfPair => SpiritFire.LeftOfPair;

    /// <summary>The right one: the same, turning the other way, in the other colours.</summary>
    private static List<ScheduledPattern> RightOfPair => SpiritFire.RightOfPair;

    /// <summary>The second pair, quicker: bolts that zigzag and double rings, one turning each way.</summary>
    private static List<ScheduledPattern> LeftOfSecondPair => SpiritFire.LeftOfSecondPair;

    private static List<ScheduledPattern> RightOfSecondPair => SpiritFire.RightOfSecondPair;

    /// <summary>The red spirit that comes alone among the others.</summary>
    private static List<ScheduledPattern> LoneHeavy => SpiritFire.RainbowHeavy;

    /// <summary>The pair of red spirits, one at each side of the field, arriving together.</summary>
    private static List<EnemySpec> Pair(float start, List<ScheduledPattern> left, List<ScheduledPattern> right) =>
        Cat(Heavy(V(170, -30), V(170, 180), start, HeavyHold, left), Heavy(V(430, -30), V(430, 180), start, HeavyHold, right));

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. Conga lines, and gunners that throw the colours.
        new Wave(Cat(Column(Swarm, 12, 110, 0, 0.3f, amplitude: 120, frequency: 1.9f), Column(Swarm, 12, 490, 1.4f, 0.3f, amplitude: 120, frequency: 1.9f, phase: Pi),
                     Row(Gunner, 5, 120, 480, 5, speed: 105, shots: RainbowFan),
                     Wall(Swarm, 10, 40, 560, new[] { 3, 6 }, 9.5f), Cross(Swarm, 9, 12, 0.32f),
                     SideColumn(Swarm, 8, true, 220, 3, 0.35f, amplitude: 60),
                     SideColumn(Swarm, 8, false, 290, 4.5f, 0.35f, amplitude: 60, phase: Pi))),

        // 2. Diamonds, and crossing streams of gunners.
        new Wave(Cat(Diamond(Swarm, 3, 190, 0, spacing: 36), Diamond(Swarm, 3, 410, 2.4f, spacing: 36),
                     Stream(Gunner, 5, V(-30, 70), V(150, 90), 3, 0.8f, shots: RainbowFan), Stream(Gunner, 5, V(630, 70), V(-150, 90), 5, 0.8f, shots: VioletBolts),
                     Cross(Swarm, 9, 9, 0.35f), Wall(Swarm, 10, 40, 560, new[] { 1, 2 }, 13),
                     Sentries(Gunner, V(-30, 60), new[] { V(80, 150), V(190, 210) }, 5f, 0.8f, 9, SpiritFire.Rainbow),
                     Sentries(Gunner, V(630, 60), new[] { V(520, 150), V(410, 210) }, 5.4f, 0.8f, 9, SpiritFire.Rainbow))),

        // 3. THE FIRST PAIR of red spirits, with the gunners.
        new Wave(Cat(Pair(0, LeftOfPair, RightOfPair),
                     Column(Gunner, 4, 300, 5, 1.3f, amplitude: 70, shots: BlueRing), Column(Gunner, 3, 80, 9, 1.3f, amplitude: 30, shots: GreenSpiral), Column(Gunner, 3, 520, 9.6f, 1.3f, amplitude: 30, shots: GreenSpiral),
                     Stream(Gunner, 4, V(-30, 80), V(145, 85), 14, 0.9f, shots: RainbowFan), Stream(Gunner, 4, V(630, 80), V(-145, 85), 14, 0.9f, shots: VioletBolts))),

        // 4. Rings of spirits that come down together, a V, and gunners with spirals.
        new Wave(Cat(Ring(Swarm, 12, V(180, -90), 70, 0), Ring(Swarm, 12, V(420, -90), 70, 2.4f), Ring(Swarm, 14, V(300, -100), 80, 5), Vee(Swarm, 9, 300, 8, spacing: 40),
                     Column(Gunner, 3, 120, 3, 1.4f, amplitude: 30, shots: GreenSpiral), Column(Gunner, 3, 480, 3.7f, 1.4f, amplitude: 30, shots: GreenSpiral),
                     Cross(Swarm, 9, 11, 0.35f), Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 14),
                     RisingRow(Swarm, 7, 80, 520, 12, speed: 130),
                     Sentries(Gunner, V(300, 830), new[] { V(140, 190), V(460, 190) }, 6f, 0.6f, 9, SpiritFire.BlueRing))),

        // 5. A red spirit alone among the crowd (as in stage 4).
        new Wave(Cat(Heavy(V(300, -30), V(300, 175), 0, HeavyHold, LoneHeavy),
                     DancingPairs(Swarm, 12, 300, 260, 3, 0.35f, amplitude: 120), Cross(Swarm, 10, 9, 0.33f),
                     Stream(Gunner, 5, V(-30, 80), V(145, 80), 4, 0.9f, shots: VioletBolts), Stream(Gunner, 5, V(630, 80), V(-145, 80), 4, 0.9f, shots: RainbowFan),
                     Wall(Swarm, 12, 30, 570, new[] { 2, 3 }, 16), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 20), Diamond(Swarm, 3, 300, 12, spacing: 36))),

        // 6. The crowd: everything, from every side.
        new Wave(Cat(DancingPairs(Swarm, 12, 300, 260, 0, 0.35f, amplitude: 120), Cross(Swarm, 10, 1.5f, 0.32f),
                     Row(Gunner, 5, 100, 500, 3, speed: 100, shots: VioletBolts),
                     Stream(Gunner, 5, V(-30, 90), V(145, 80), 6, 0.7f, shots: RainbowFan), Stream(Gunner, 5, V(630, 90), V(-145, 80), 6, 0.7f, shots: VioletBolts),
                     Column(Gunner, 4, 300, 9, 1.1f, amplitude: 90, shots: BlueRing),
                     Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 12), Diamond(Swarm, 3, 160, 14, spacing: 36), Diamond(Swarm, 3, 440, 14, spacing: 36),
                     Sentries(Gunner, V(-30, 60), new[] { V(70, 140), V(170, 200), V(260, 120) }, 6f, 0.7f, 9, SpiritFire.RainbowFan),
                     Sentries(Gunner, V(630, 60), new[] { V(530, 140), V(430, 200), V(340, 120) }, 6.3f, 0.7f, 9, SpiritFire.Rainbow),
                     RisingRow(Swarm, 8, 60, 540, 13))),

        // 7. THE SECOND PAIR, quicker, with the gunners.
        new Wave(Cat(Pair(0, LeftOfSecondPair, RightOfSecondPair),
                     Column(Gunner, 4, 300, 4, 1.2f, amplitude: 80, shots: BlueRing), Row(Gunner, 5, 100, 500, 8, speed: 100, shots: RainbowFan),
                     Column(Gunner, 4, 110, 12, 1.2f, amplitude: 40, shots: GreenSpiral), Column(Gunner, 4, 490, 12.6f, 1.2f, amplitude: 40, shots: GreenSpiral),
                     Stream(Gunner, 5, V(-30, 80), V(145, 85), 16, 0.8f, shots: VioletBolts), Stream(Gunner, 5, V(630, 80), V(-145, 85), 16, 0.8f, shots: RainbowFan))),

        // 8. A breath: spirits of the first kind in long lines and walls.
        new Wave(Cat(DancingPairs(Swarm, 12, 300, 240, 0, 0.35f, amplitude: 120), Cross(Swarm, 10, 1.5f, 0.32f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 6), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 8.5f),
                     Column(Gunner, 5, 300, 9, 1.0f, amplitude: 150, frequency: 1.6f, shots: RainbowFan),
                     Diamond(Swarm, 3, 160, 12, spacing: 36), Diamond(Swarm, 3, 440, 12, spacing: 36),
                     Sentries(Gunner, V(-30, 60), new[] { V(80, 130), V(180, 190) }, 4f, 0.7f, 10, SpiritFire.Rainbow),
                     Sentries(Gunner, V(630, 60), new[] { V(520, 130), V(420, 190) }, 4.3f, 0.7f, 10, SpiritFire.Rainbow),
                     RisingRow(Swarm, 8, 60, 540, 9),
                     SideColumn(Swarm, 8, true, 330, 11, 0.3f, amplitude: 80))),

        // 9. THE LAST PAIR, with a crowd of the little spirits of the first kind round them.
        new Wave(Cat(Pair(0, LeftOfSecondPair, RightOfSecondPair),
                     DancingPairs(Swarm, 14, 300, 250, 3, 0.32f, amplitude: 130), Cross(Swarm, 10, 5, 0.3f), Cross(Swarm, 10, 14, 0.3f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 9), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 12.5f), Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 17),
                     Ring(Swarm, 12, V(180, -90), 70, 7), Ring(Swarm, 12, V(420, -90), 70, 10), Vee(Swarm, 9, 300, 15, spacing: 40),
                     Column(Swarm, 10, 90, 18, 0.3f, amplitude: 60), Column(Swarm, 10, 510, 18.6f, 0.3f, amplitude: 60, phase: Pi))),
    };
}
