using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 3, the lake of the hail. The first red spirit (type 3) comes now, and it comes ALONE (nothing else in its wave), twice. The others are the spirits of types 1 and 2, in formations
/// that cross the field in diagonals, with gunners whose shots are small figures of hail, dancing hail and bolts that zigzag. Everything comes from the top of the field or from its top corners.
/// </summary>
public static class Stage3Waves
{
    private static List<ScheduledPattern> HailFan => SpiritFire.HailFan;
    private static List<ScheduledPattern> HailRing => SpiritFire.HailRing;
    private static List<ScheduledPattern> Bolts => SpiritFire.Bolts;
    private static List<ScheduledPattern> HailSpiral => SpiritFire.HailSpiral;

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. Two long conga lines of revellers, and a row of heralds throwing hail.
        new Wave(Cat(Column(Swarm, 8, 120, 0, 0.4f, amplitude: 120, frequency: 1.9f),
                     Column(Swarm, 8, 480, 1.4f, 0.4f, amplitude: 120, frequency: 1.9f, phase: Pi),
                     Row(Gunner, 3, 150, 450, 5, speed: 100, shots: HailFan),
                     Wall(Swarm, 10, 40, 560, new[] { 3, 6 }, 9.5f),
                     SideColumn(Swarm, 6, true, 120, 3, 0.4f, amplitude: 40),
                     SideColumn(Swarm, 6, false, 150, 4.5f, 0.4f, amplitude: 40, phase: Pi))),
        // 2. Diamonds, and streams of heralds that come in from the corners.
        new Wave(Cat(Diamond(Swarm, 3, 200, 0, spacing: 36), Diamond(Swarm, 3, 400, 2.4f, spacing: 36),
                     Stream(Gunner, 3, V(-30, 70), V(150, 90), 3, 1.0f, shots: HailFan),
                     Stream(Gunner, 3, V(630, 70), V(-150, 90), 5, 1.0f, shots: Bolts),
                     Cross(Swarm, 6, 9, 0.4f),
                     Sentries(Gunner, V(-30, 60), new[] { V(100, 150) }, 5f, 0.8f, 7, SpiritFire.StormStars),
                     Sentries(Gunner, V(630, 60), new[] { V(500, 150) }, 5.4f, 0.8f, 7, SpiritFire.StormStars))),
        // 3. The first red spirit, alone.
        new Wave(Heavy(V(300, -30), V(300, 185), 0, 24, SpiritFire.FirstStormHeavy)),
        // 4. Dancing pairs, heralds with rings of dancing hail, and walls that open in turns.
        new Wave(Cat(DancingPairs(Swarm, 7, 300, 200, 0, 0.45f, amplitude: 110),
                     Column(Gunner, 3, 300, 2.5f, 1.5f, amplitude: 60, shots: HailRing),
                     DancingPairs(Swarm, 7, 300, 340, 6, 0.45f, amplitude: 120),
                     Wall(Swarm, 10, 40, 560, new[] { 1, 2 }, 10), Wall(Swarm, 10, 40, 560, new[] { 7, 8 }, 12.5f),
                     Sentries(Gunner, V(300, -30), new[] { V(160, 170), V(440, 170) }, 6f, 0.8f, 7, SpiritFire.HailRing))),
        // 5. Rings of revellers that come down together, a V, and heralds that throw spirals of hail.
        new Wave(Cat(Ring(Swarm, 10, V(180, -90), 70, 0), Ring(Swarm, 10, V(420, -90), 70, 2.6f), Ring(Swarm, 12, V(300, -100), 80, 5.4f),
                     Vee(Swarm, 7, 300, 8.5f, spacing: 40),
                     Column(Gunner, 2, 120, 3, 1.6f, amplitude: 30, shots: HailSpiral), Column(Gunner, 2, 480, 3.7f, 1.6f, amplitude: 30, shots: HailSpiral),
                     Cross(Swarm, 6, 11, 0.4f),
                     Sentries(Gunner, V(-30, 60), new[] { V(80, 140), V(180, 190) }, 6f, 0.8f, 7, SpiritFire.HailFan),
                     Sentries(Gunner, V(630, 60), new[] { V(520, 140), V(420, 190) }, 6.3f, 0.8f, 7, SpiritFire.HailFan))),
        // 6. The crowd: everything, from the top.
        new Wave(Cat(Column(Swarm, 7, 80, 0, 0.4f, amplitude: 60), Column(Swarm, 7, 520, 0.6f, 0.4f, amplitude: 60, phase: Pi),
                     Stream(Gunner, 3, V(-30, 90), V(145, 80), 3, 0.9f, shots: Bolts), Stream(Gunner, 3, V(630, 90), V(-145, 80), 3, 0.9f, shots: HailFan),
                     Diamond(Swarm, 3, 300, 7, spacing: 36),
                     Column(Gunner, 3, 300, 8, 1.3f, amplitude: 90, shots: HailRing),
                     Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 12), Cross(Swarm, 6, 14, 0.4f),
                     SideColumn(Swarm, 6, false, 130, 10, 0.4f, amplitude: 50, phase: Pi))),
        // 7. The second red spirit, alone.
        new Wave(Heavy(V(300, -30), V(300, 170), 0, 26, SpiritFire.SecondStormHeavy)),
    };
}
