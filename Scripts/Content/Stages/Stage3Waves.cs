using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 3, the lake of the hail. All three kinds of spirits come now. The red ones (type 3), with their great health and their long,
/// elaborate patterns, come alone and only twice (in later stages they will come more, and with the others). More spirits than in stage 2, in
/// formations that cross the field in diagonals, and gunners whose shots are hail, dancing hail and bolts that zigzag.
/// </summary>
public static class Stage3Waves
{
    private static List<ScheduledPattern> HailFan => new List<ScheduledPattern> { P(FirePattern.Fan(1.6f, 3, 34, 185).Styled(BulletStyle.Hail), 1000) };
    private static List<ScheduledPattern> HailRing => new List<ScheduledPattern> { P(FirePattern.Ring(2.1f, 8, 105, 8).Styled(BulletStyle.Hail).Dancing(70), 1000) };
    private static List<ScheduledPattern> Bolts => new List<ScheduledPattern> { P(FirePattern.Fan(0.9f, 3, 30, 210).Styled(BulletStyle.Spark).Zigzag(60), 2.7f), P(FirePattern.None, 1.8f) };
    private static List<ScheduledPattern> HailSpiral => new List<ScheduledPattern> { P(FirePattern.Spiral(0.3f, 2, 24, 135).Styled(BulletStyle.Hail), 2.4f), P(FirePattern.None, 1.8f) };

    /// <summary>The first red spirit: a dancing ring of hail, a wide fan at the boat, a spiral of hail, and a rest, over and over.</summary>
    private static List<ScheduledPattern> FirstHeavy => new List<ScheduledPattern>
    {
        P(FirePattern.Ring(0.55f, 14, 120, 11).Styled(BulletStyle.Hail).Dancing(70), 3.6f),
        P(FirePattern.Fan(0.7f, 7, 70, 230).Styled(BulletStyle.Hail), 3.2f),
        P(FirePattern.Spiral(0.1f, 3, 21, 150).Styled(BulletStyle.Hail), 3.2f),
        P(FirePattern.None, 1.4f),
    };

    /// <summary>The second one: more arms, bolts that zigzag, and a double ring that turns the other way.</summary>
    private static List<ScheduledPattern> SecondHeavy => new List<ScheduledPattern>
    {
        P(FirePattern.Spiral(0.09f, 4, 17, 160).Styled(BulletStyle.Hail), 3.4f),
        P(FirePattern.Fan(0.5f, 5, 50, 220).Styled(BulletStyle.Spark).Zigzag(60), 3.0f),
        P(FirePattern.Ring(0.5f, 18, 125, -13).Styled(BulletStyle.Hail).Dancing(80), 3.6f),
        P(FirePattern.Fan(0.7f, 9, 80, 235).Styled(BulletStyle.Hail), 2.8f),
        P(FirePattern.None, 1.2f),
    };

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. Two long conga lines of revellers, and a row of heralds throwing hail.
        new Wave(Cat(Column(Swarm, 11, 120, 0, 0.32f, amplitude: 120, frequency: 1.9f),
                     Column(Swarm, 11, 480, 1.4f, 0.32f, amplitude: 120, frequency: 1.9f, phase: Pi),
                     Row(Gunner, 4, 130, 470, 5, speed: 105, shots: HailFan),
                     Wall(Swarm, 10, 40, 560, new[] { 3, 6 }, 9.5f))),

        // 2. Diamonds, and streams of heralds that come in from the corners, crossing.
        new Wave(Cat(Diamond(Swarm, 3, 200, 0, spacing: 36), Diamond(Swarm, 3, 400, 2.4f, spacing: 36),
                     Stream(Gunner, 4, V(-30, 70), V(150, 90), 3, 0.8f, shots: HailFan),
                     Stream(Gunner, 4, V(630, 70), V(-150, 90), 5, 0.8f, shots: Bolts),
                     Cross(Swarm, 8, 9, 0.35f))),

        // 3. The first red spirit, alone.
        new Wave(Heavy(V(300, -30), V(300, 185), 0, 26, FirstHeavy)),

        // 4. Dancing pairs, heralds with rings of dancing hail, and walls that open in turns.
        new Wave(Cat(DancingPairs(Swarm, 10, 300, 200, 0, 0.4f, amplitude: 110),
                     Column(Gunner, 4, 300, 2.5f, 1.2f, amplitude: 60, shots: HailRing),
                     DancingPairs(Swarm, 10, 300, 340, 6, 0.38f, amplitude: 120),
                     Wall(Swarm, 10, 40, 560, new[] { 1, 2 }, 10), Wall(Swarm, 10, 40, 560, new[] { 7, 8 }, 12.5f))),

        // 5. Rings of revellers that come down together, a V, and heralds that throw spirals of hail.
        new Wave(Cat(Ring(Swarm, 12, V(180, -90), 70, 0), Ring(Swarm, 12, V(420, -90), 70, 2.4f), Ring(Swarm, 14, V(300, -100), 80, 5),
                     Vee(Swarm, 9, 300, 8, spacing: 40),
                     Column(Gunner, 3, 120, 3, 1.4f, amplitude: 30, shots: HailSpiral), Column(Gunner, 3, 480, 3.7f, 1.4f, amplitude: 30, shots: HailSpiral),
                     Cross(Swarm, 8, 11, 0.35f))),

        // 6. The crowd: everything, from every side.
        new Wave(Cat(Column(Swarm, 9, 80, 0, 0.3f, amplitude: 60), Column(Swarm, 9, 520, 0.6f, 0.3f, amplitude: 60, phase: Pi),
                     Stream(Gunner, 5, V(-30, 90), V(145, 80), 3, 0.7f, shots: Bolts), Stream(Gunner, 5, V(630, 90), V(-145, 80), 3, 0.7f, shots: HailFan),
                     Diamond(Swarm, 3, 300, 7, spacing: 36),
                     Column(Gunner, 4, 300, 8, 1.1f, amplitude: 90, shots: HailRing),
                     Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 12), Cross(Swarm, 8, 14, 0.3f))),

        // 7. The second red spirit, alone.
        new Wave(Heavy(V(300, -30), V(300, 170), 0, 30, SecondHeavy)),

        // 8. The last crowd, before the lake opens.
        new Wave(Cat(DancingPairs(Swarm, 12, 300, 220, 0, 0.35f, amplitude: 120), Cross(Swarm, 9, 1.5f, 0.33f),
                     Row(Gunner, 5, 100, 500, 3, speed: 100, shots: Bolts),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 6), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 8.5f),
                     Column(Gunner, 5, 300, 9, 1.0f, amplitude: 150, frequency: 1.6f, shots: HailFan),
                     Diamond(Swarm, 3, 160, 12, spacing: 36), Diamond(Swarm, 3, 440, 12, spacing: 36))),
    };
}
