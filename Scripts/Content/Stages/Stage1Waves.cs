using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 1. It is the first stage and it is simple: only the blue spirits of the swarm come, and they do not shoot. They
/// start gently, in columns and in lines, and go on to rings and to groups that close in from both sides. (The green gunners and the
/// red spirit are for later stages; the blocks to build their waves are in <see cref="WaveBuilder"/>.)
/// </summary>
public static class Stage1Waves
{
    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. Blue swarms, in columns and then in a line.
        new Wave(Cat(Column(Swarm, 8, 140, 0, 0.35f),
                     Column(Swarm, 8, 460, 3.2f, 0.35f, phase: Pi),
                     Row(Swarm, 7, 100, 500, 6.8f))),

        // 2. A V, then two streams that curve in from the sides.
        new Wave(Cat(Vee(Swarm, 9, 300, 0),
                     Stream(Swarm, 7, V(-30, 110), V(190, 40), 3, 0.3f, turn: 25),
                     Stream(Swarm, 7, V(630, 110), V(-190, 40), 3, 0.3f, turn: -25),
                     Row(Swarm, 6, 150, 450, 7, speed: 170))),

        // 3. Rings of blue spirits.
        new Wave(Cat(Ring(Swarm, 10, V(300, -70), 62, 0),
                     Ring(Swarm, 12, V(190, -90), 70, 3.5f),
                     Ring(Swarm, 12, V(410, -90), 70, 3.5f),
                     Column(Swarm, 6, 90, 7, 0.35f, phase: 0),
                     Column(Swarm, 6, 510, 7, 0.35f, phase: Pi))),

        // 4. Pincers closing in from both sides, with a long snake between them.
        new Wave(Cat(Stream(Swarm, 8, V(-30, 100), V(140, 110), 0, 0.4f, turn: -15),
                     Stream(Swarm, 8, V(630, 100), V(-140, 110), 0, 0.4f, turn: 15),
                     Column(Swarm, 10, 300, 3, 0.3f, amplitude: 120, frequency: 2.5f),
                     Vee(Swarm, 7, 300, 8))),

        // 5. A line, two rings and two columns, all at once.
        new Wave(Cat(Row(Swarm, 12, 40, 560, 0, speed: 150),
                     Ring(Swarm, 10, V(190, -70), 60, 3),
                     Ring(Swarm, 10, V(410, -70), 60, 4.5f),
                     Column(Swarm, 8, 100, 6, 0.35f),
                     Column(Swarm, 8, 500, 6, 0.35f, phase: Pi))),
    };
}
