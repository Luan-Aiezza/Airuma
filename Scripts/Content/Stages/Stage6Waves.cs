using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 6, the river of stars: the last and the longest. Every kind of spirit of the game comes: the little ones in every formation, the gunners in every stream, and the red ones (type 3), which here
/// come in up to THREE at a time: a lone one, pairs, and trios, with the others round them. The shots are small figures of silver and of starlight. Each red spirit is worse than the last. Everything comes
/// from the top of the field or from its top corners.
/// </summary>
public static class Stage6Waves
{
    /// <summary>How long a red spirit stays, at most, once it has settled.</summary>
    public const float HeavyHold = 24;
    private static List<ScheduledPattern> SilverFan => SpiritFire.SilverFan;
    private static List<ScheduledPattern> StarRing => SpiritFire.StarRing;
    private static List<ScheduledPattern> VioletBolts => SpiritFire.MoonBolts;
    private static List<ScheduledPattern> GreenSpiral => SpiritFire.MoonSpiral;
    private static List<ScheduledPattern> LoneHeavy => SpiritFire.LoneMoonHeavy;
    private static List<ScheduledPattern> Left => SpiritFire.LeftMoon;
    private static List<ScheduledPattern> Right => SpiritFire.RightMoon;
    private static List<ScheduledPattern> Middle => SpiritFire.MiddleMoon;
    private static List<EnemySpec> Pair(float start) => Cat(Heavy(V(170, -30), V(170, 180), start, HeavyHold, Left), Heavy(V(430, -30), V(430, 180), start, HeavyHold, Right));
    private static List<EnemySpec> Trio(float start) =>
        Cat(Heavy(V(110, -30), V(110, 190), start, HeavyHold, Left), Heavy(V(300, -30), V(300, 150), start + 0.6f, HeavyHold, Middle), Heavy(V(490, -30), V(490, 190), start, HeavyHold, Right));

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. The crowd opens: conga lines and gunners.
        new Wave(Cat(Column(Swarm, 9, 110, 0, 0.38f, amplitude: 120, frequency: 1.9f), Column(Swarm, 9, 490, 1.4f, 0.38f, amplitude: 120, frequency: 1.9f, phase: Pi),
                     Row(Gunner, 3, 150, 450, 5, speed: 100, shots: SilverFan), Wall(Swarm, 10, 40, 560, new[] { 3, 6 }, 9.5f), Cross(Swarm, 7, 12, 0.38f),
                     SideColumn(Swarm, 6, true, 120, 3, 0.38f, amplitude: 40),
                     SideColumn(Swarm, 6, false, 150, 4.5f, 0.38f, amplitude: 40, phase: Pi))),
        // 2. Diamonds and crossing streams.
        new Wave(Cat(Diamond(Swarm, 3, 190, 0, spacing: 36), Diamond(Swarm, 3, 410, 2.4f, spacing: 36),
                     Stream(Gunner, 3, V(-30, 70), V(150, 90), 3, 1.0f, shots: SilverFan), Stream(Gunner, 3, V(630, 70), V(-150, 90), 5, 1.0f, shots: VioletBolts),
                     Cross(Swarm, 7, 9, 0.38f), Wall(Swarm, 10, 40, 560, new[] { 1, 2 }, 13),
                     Sentries(Gunner, V(-30, 60), new[] { V(100, 150) }, 5f, 0.8f, 7, SpiritFire.MoonCrescent),
                     Sentries(Gunner, V(630, 60), new[] { V(500, 150) }, 5.4f, 0.8f, 7, SpiritFire.MoonCrescent))),
        // 3. A red spirit alone among the crowd.
        new Wave(Cat(Heavy(V(300, -30), V(300, 175), 0, HeavyHold, LoneHeavy),
                     DancingPairs(Swarm, 7, 300, 260, 3, 0.45f, amplitude: 120), Cross(Swarm, 7, 9, 0.38f),
                     Stream(Gunner, 3, V(-30, 80), V(145, 80), 4, 1.0f, shots: VioletBolts), Stream(Gunner, 3, V(630, 80), V(-145, 80), 4, 1.0f, shots: SilverFan),
                     Wall(Swarm, 12, 30, 570, new[] { 2, 3 }, 16), Diamond(Swarm, 3, 300, 12, spacing: 36))),
        // 4. A breath: rings, a V and long lines of the little ones.
        new Wave(Cat(Ring(Swarm, 10, V(180, -90), 70, 0), Ring(Swarm, 10, V(420, -90), 70, 2.6f), Ring(Swarm, 12, V(300, -100), 80, 5.4f), Vee(Swarm, 7, 300, 8.5f, spacing: 40),
                     Column(Gunner, 2, 120, 3, 1.6f, amplitude: 30, shots: GreenSpiral), Column(Gunner, 2, 480, 3.7f, 1.6f, amplitude: 30, shots: GreenSpiral),
                     Cross(Swarm, 7, 11, 0.38f), Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 14),
                     Sentries(Gunner, V(300, -30), new[] { V(160, 170), V(440, 170) }, 6f, 0.8f, 7, SpiritFire.StarRing))),
        // 5. THE FIRST PAIR of red spirits, with the gunners.
        new Wave(Cat(Pair(0),
                     Column(Gunner, 2, 300, 5, 1.5f, amplitude: 70, shots: StarRing), Column(Swarm, 6, 80, 8, 0.45f, amplitude: 40), Column(Swarm, 6, 520, 8.6f, 0.45f, amplitude: 40, phase: Pi),
                     Stream(Gunner, 2, V(-30, 80), V(145, 85), 13, 1.1f, shots: SilverFan), Stream(Gunner, 2, V(630, 80), V(-145, 80), 13, 1.1f, shots: VioletBolts))),
        // 6. The crowd: everything, from the top.
        new Wave(Cat(DancingPairs(Swarm, 8, 300, 260, 0, 0.42f, amplitude: 120), Cross(Swarm, 8, 1.5f, 0.38f),
                     Row(Gunner, 3, 150, 450, 3, speed: 100, shots: VioletBolts),
                     Stream(Gunner, 3, V(-30, 90), V(145, 80), 6, 1.0f, shots: SilverFan), Stream(Gunner, 3, V(630, 90), V(-145, 80), 6, 1.0f, shots: VioletBolts),
                     Column(Gunner, 3, 300, 9, 1.3f, amplitude: 90, shots: StarRing),
                     Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 12), Diamond(Swarm, 3, 160, 14, spacing: 36), Diamond(Swarm, 3, 440, 14, spacing: 36),
                     Sentries(Gunner, V(-30, 60), new[] { V(90, 150), V(190, 200) }, 6f, 0.8f, 8, SpiritFire.SilverFan),
                     Sentries(Gunner, V(630, 60), new[] { V(510, 150), V(410, 200) }, 6.3f, 0.8f, 8, SpiritFire.MoonCrescent))),
        // 7. THE FIRST TRIO of red spirits: three at once, with the gunners.
        new Wave(Cat(Trio(0),
                     Column(Gunner, 2, 300, 6, 1.5f, amplitude: 60, shots: StarRing), Row(Gunner, 3, 150, 450, 10, speed: 100, shots: SilverFan),
                     Stream(Gunner, 2, V(-30, 80), V(145, 85), 14, 1.1f, shots: VioletBolts), Stream(Gunner, 2, V(630, 80), V(-145, 85), 14, 1.1f, shots: SilverFan))),
        // 8. A breath again, with a lone red spirit that arrives late.
        new Wave(Cat(DancingPairs(Swarm, 8, 300, 240, 0, 0.42f, amplitude: 120), Cross(Swarm, 8, 1.5f, 0.38f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 6), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 8.5f),
                     Column(Gunner, 3, 300, 9, 1.3f, amplitude: 150, frequency: 1.6f, shots: SilverFan),
                     Heavy(V(300, -30), V(300, 170), 12, HeavyHold, LoneHeavy),
                     Diamond(Swarm, 3, 160, 12, spacing: 36), Diamond(Swarm, 3, 440, 12, spacing: 36),
                     Sentries(Gunner, V(-30, 60), new[] { V(100, 150) }, 4f, 0.8f, 8, SpiritFire.MoonCrescent),
                     Sentries(Gunner, V(630, 60), new[] { V(500, 150) }, 4.3f, 0.8f, 8, SpiritFire.MoonCrescent))),
        // 9. THE SECOND PAIR, with a crowd of the little ones round them.
        new Wave(Cat(Pair(0),
                     DancingPairs(Swarm, 8, 300, 250, 3, 0.4f, amplitude: 130), Cross(Swarm, 8, 5, 0.38f), Cross(Swarm, 8, 14, 0.38f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 9), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 12.5f),
                     Ring(Swarm, 10, V(180, -90), 70, 7), Ring(Swarm, 10, V(420, -90), 70, 10), Vee(Swarm, 7, 300, 15, spacing: 40),
                     Column(Gunner, 2, 110, 12, 1.4f, amplitude: 40, shots: GreenSpiral), Column(Gunner, 2, 490, 12.6f, 1.4f, amplitude: 40, shots: GreenSpiral),
                     SideColumn(Swarm, 7, true, 130, 4, 0.38f, amplitude: 50),
                     SideColumn(Swarm, 7, false, 160, 6, 0.38f, amplitude: 50, phase: Pi))),
        // 10. THE LAST TRIO, and everything else: every kind of spirit of the game at once.
        new Wave(Cat(Trio(0),
                     DancingPairs(Swarm, 8, 300, 250, 3, 0.4f, amplitude: 130), Cross(Swarm, 8, 5, 0.38f), Cross(Swarm, 8, 14, 0.38f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 9), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 12.5f), Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 17),
                     Stream(Gunner, 3, V(-30, 80), V(145, 85), 7, 1.0f, shots: VioletBolts), Stream(Gunner, 3, V(630, 80), V(-145, 85), 7, 1.0f, shots: SilverFan),
                     Column(Gunner, 3, 300, 14, 1.3f, amplitude: 80, shots: StarRing),
                     Column(Swarm, 8, 90, 18, 0.38f, amplitude: 60), Column(Swarm, 8, 510, 18.6f, 0.38f, amplitude: 60, phase: Pi))),
    };
}
