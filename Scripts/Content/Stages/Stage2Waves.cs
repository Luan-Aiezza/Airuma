using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 2, the river of the parties: only the spirits of types 1 and 2. The blue spirits (the revellers) are back, with the green ones (the masters of ceremonies) who now fire small figures of
/// their own: a fan of notes, a ring of balloons, a pair of streamers. Everything comes from the top of the field, or from its top corners; nothing comes from below. A little longer and busier than stage 1.
/// </summary>
public static class Stage2Waves
{
    private static List<ScheduledPattern> Confetti => SpiritFire.Confetti;
    private static List<ScheduledPattern> Balloons => SpiritFire.Balloons;
    private static List<ScheduledPattern> Streamers => SpiritFire.Streamers;

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. Conga lines that sway in from both sides, and a wall of revellers with two gaps.
        new Wave(Cat(Column(Swarm, 7, 120, 0, 0.4f, amplitude: 110, frequency: 1.8f),
                     Column(Swarm, 7, 480, 1.5f, 0.4f, amplitude: 110, frequency: 1.8f, phase: Pi),
                     Wall(Swarm, 9, 50, 550, new[] { 2, 6 }, 6.5f),
                     SideColumn(Swarm, 6, true, 120, 3, 0.45f, amplitude: 40),
                     SideColumn(Swarm, 6, false, 150, 4.5f, 0.45f, amplitude: 40, phase: Pi))),
        // 2. The first masters of ceremonies, with notes, between two crosses of revellers.
        new Wave(Cat(Cross(Swarm, 5, 0, 0.45f),
                     Row(Gunner, 2, 200, 400, 3.5f, speed: 100, shots: Confetti),
                     Cross(Swarm, 5, 6, 0.45f),
                     Wall(Swarm, 8, 60, 540, new[] { 3, 4 }, 9.5f),
                     Sentries(Gunner, V(-30, 60), new[] { V(110, 150) }, 4f, 0.8f, 7, SpiritFire.Pinwheel),
                     Sentries(Gunner, V(630, 60), new[] { V(490, 150) }, 4.4f, 0.8f, 7, SpiritFire.Pinwheel))),
        // 3. Diamonds, and masters on both sides throwing streamers.
        new Wave(Cat(Diamond(Swarm, 2, 200, 0),
                     Diamond(Swarm, 2, 400, 2),
                     Stream(Gunner, 2, V(-30, 80), V(120, 70), 3, 1.1f, shots: Streamers),
                     Stream(Gunner, 2, V(630, 80), V(-120, 70), 5, 1.1f, shots: Streamers),
                     Wall(Swarm, 7, 70, 530, new[] { 1, 5 }, 9))),
        // 4. Pairs that dance round each other, with the masters throwing balloons from the middle.
        new Wave(Cat(DancingPairs(Swarm, 6, 300, 180, 0, 0.5f),
                     Column(Gunner, 2, 300, 2.5f, 1.6f, amplitude: 40, shots: Balloons),
                     DancingPairs(Swarm, 6, 300, 300, 6, 0.5f, amplitude: 110),
                     Cross(Swarm, 4, 9, 0.45f),
                     SideColumn(Swarm, 6, true, 130, 11, 0.45f, amplitude: 50))),
        // 5. A conga of masters, and walls that open in turns.
        new Wave(Cat(Column(Gunner, 3, 300, 0, 1.4f, amplitude: 150, frequency: 1.6f, shots: Confetti),
                     Wall(Swarm, 9, 50, 550, new[] { 1, 2 }, 3),
                     Wall(Swarm, 9, 50, 550, new[] { 6, 7 }, 5.5f),
                     Wall(Swarm, 9, 50, 550, new[] { 3, 4, 5 }, 8),
                     Row(Gunner, 3, 150, 450, 7, speed: 100, shots: Balloons),
                     Sentries(Gunner, V(300, -30), new[] { V(130, 170), V(470, 170) }, 6f, 0.8f, 7, SpiritFire.Confetti))),
        // 6. The big party: everyone, from the top.
        new Wave(Cat(Diamond(Swarm, 3, 300, 0, spacing: 36),
                     DancingPairs(Swarm, 7, 300, 360, 2, 0.5f, amplitude: 120),
                     Stream(Gunner, 3, V(-30, 90), V(125, 80), 3, 1.0f, shots: Confetti),
                     Stream(Gunner, 3, V(630, 90), V(-125, 80), 3, 1.0f, shots: Streamers),
                     Cross(Swarm, 6, 8, 0.4f),
                     Column(Gunner, 2, 300, 9, 1.3f, amplitude: 60, shots: Balloons),
                     Wall(Swarm, 10, 40, 560, new[] { 4, 5 }, 12),
                     Sentries(Gunner, V(-30, 60), new[] { V(90, 160) }, 5f, 0.7f, 8, SpiritFire.Pinwheel),
                     Sentries(Gunner, V(630, 60), new[] { V(510, 160) }, 5.3f, 0.7f, 8, SpiritFire.Pinwheel))),
    };
}
