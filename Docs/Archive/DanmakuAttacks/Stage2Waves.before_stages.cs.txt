using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 2, the river of the parties. The blue spirits (the revellers) are back, with the green ones (the masters of
/// ceremonies) who now fire patterns of their own: a fan of confetti, a ring of balloons, a pair of streamers. The formations are new:
/// conga lines, walls with gaps, crosses, diamonds, pairs that dance round each other. (The sprites are still those of stage 1.)
/// </summary>
public static class Stage2Waves
{
    /// <summary>A fan of three bullets of confetti at the boat.</summary>
    private static List<ScheduledPattern> Confetti => SpiritFire.Confetti;
    /// <summary>A small, slow ring of balloons that turns a little each time.</summary>
    private static List<ScheduledPattern> Balloons => SpiritFire.Balloons;
    /// <summary>A short spiral of streamers, then a rest, then the spiral again.</summary>
    private static List<ScheduledPattern> Streamers => SpiritFire.Streamers;

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. Conga lines that sway in from both sides, and a wall of revellers with two gaps.
        new Wave(Cat(Column(Swarm, 9, 120, 0, 0.35f, amplitude: 110, frequency: 1.8f),
                     Column(Swarm, 9, 480, 1.5f, 0.35f, amplitude: 110, frequency: 1.8f, phase: Pi),
                     Wall(Swarm, 9, 50, 550, new[] { 2, 6 }, 6.5f),
                     SideColumn(Swarm, 7, true, 230, 3, 0.4f, amplitude: 50),
                     SideColumn(Swarm, 7, false, 300, 4.5f, 0.4f, amplitude: 50, phase: Pi))),

        // 2. The first masters of ceremonies, with confetti, between two crosses of revellers.
        new Wave(Cat(Cross(Swarm, 6, 0, 0.4f),
                     Row(Gunner, 3, 150, 450, 3.5f, speed: 110, shots: Confetti),
                     Cross(Swarm, 6, 6, 0.4f),
                     Wall(Swarm, 8, 60, 540, new[] { 3, 4 }, 9.5f),
                     Sentries(Gunner, V(-30, 60), new[] { V(90, 170), V(210, 110) }, 4f, 0.8f, 9, SpiritFire.Pinwheel),
                     Sentries(Gunner, V(630, 60), new[] { V(510, 170), V(390, 110) }, 4.4f, 0.8f, 9, SpiritFire.Pinwheel))),

        // 3. Diamonds, and masters on both sides throwing streamers.
        new Wave(Cat(Diamond(Swarm, 2, 200, 0),
                     Diamond(Swarm, 2, 400, 2),
                     Stream(Gunner, 3, V(-30, 80), V(120, 70), 3, 0.9f, shots: Streamers),
                     Stream(Gunner, 3, V(630, 80), V(-120, 70), 5, 0.9f, shots: Streamers),
                     Wall(Swarm, 7, 70, 530, new[] { 1, 5 }, 9),
                     RisingRow(Swarm, 6, 90, 510, 11, speed: 130))),

        // 4. Pairs that dance round each other, with the masters throwing balloons from the middle.
        new Wave(Cat(DancingPairs(Swarm, 8, 300, 180, 0, 0.45f),
                     Column(Gunner, 3, 300, 2.5f, 1.2f, amplitude: 40, shots: Balloons),
                     DancingPairs(Swarm, 8, 300, 300, 6, 0.4f, amplitude: 110),
                     Cross(Swarm, 5, 9, 0.4f),
                     SideColumn(Swarm, 8, true, 200, 11, 0.35f, amplitude: 70))),

        // 5. A conga of masters, walls that open in turns, and the first ring of balloons that comes with confetti.
        new Wave(Cat(Column(Gunner, 5, 300, 0, 1.0f, amplitude: 150, frequency: 1.6f, shots: Confetti),
                     Wall(Swarm, 9, 50, 550, new[] { 1, 2 }, 3),
                     Wall(Swarm, 9, 50, 550, new[] { 6, 7 }, 5.5f),
                     Wall(Swarm, 9, 50, 550, new[] { 3, 4, 5 }, 8),
                     Row(Gunner, 4, 120, 480, 7, speed: 100, shots: Balloons),
                     Sentries(Gunner, V(300, 830), new[] { V(130, 200), V(470, 200) }, 6f, 0.6f, 8, SpiritFire.Confetti))),

        // 6. The big party: everyone, from every side.
        new Wave(Cat(Diamond(Swarm, 3, 300, 0, spacing: 36),
                     DancingPairs(Swarm, 8, 300, 360, 2, 0.4f, amplitude: 120),
                     Stream(Gunner, 4, V(-30, 90), V(125, 80), 3, 0.8f, shots: Confetti),
                     Stream(Gunner, 4, V(630, 90), V(-125, 80), 3, 0.8f, shots: Streamers),
                     Cross(Swarm, 7, 8, 0.35f),
                     Column(Gunner, 3, 300, 9, 1.0f, amplitude: 60, shots: Balloons),
                     Wall(Swarm, 10, 40, 560, new[] { 4, 5 }, 12),
                     Sentries(Gunner, V(-30, 60), new[] { V(80, 150), V(180, 220) }, 5f, 0.7f, 10, SpiritFire.Pinwheel),
                     Sentries(Gunner, V(630, 60), new[] { V(520, 150), V(420, 220) }, 5.3f, 0.7f, 10, SpiritFire.Pinwheel),
                     RisingRow(Swarm, 7, 60, 540, 14))),
    };
}
