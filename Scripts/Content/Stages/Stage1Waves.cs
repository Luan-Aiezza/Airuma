using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// The nine waves of stage 1. They start gently with blue swarms, bring in green gunners, then a red spirit on its own,
/// and from the middle onwards mix the three at once, from different sides, as the stages of Touhou do.
/// </summary>
public static class Stage1Waves
{
    // Building blocks

    private static readonly List<ScheduledPattern> GunnerShots = new List<ScheduledPattern> { new ScheduledPattern(FirePattern.Aimed(1.5f, 220), 1000) };
    private const float Top = -34;

    private static List<ScheduledPattern> ShotsFor(SpiritKind kind) => kind == SpiritKind.Gunner ? GunnerShots : new List<ScheduledPattern>();

    private static List<EnemySpec> Cat(params List<EnemySpec>[] parts) => parts.SelectMany(p => p).ToList();

    /// <summary>Enemies one after another from the same spot, following the same sway.</summary>
    private static List<EnemySpec> Column(SpiritKind kind, int count, float x, float start, float interval,
                                          float speed = 150, float amplitude = 70, float frequency = 2.2f, float phase = 0)
    {
        return Enumerable.Range(0, count).Select(i => new EnemySpec
        {
            Time = start + i * interval, Kind = kind, Position = new Vector2(x, Top),
            Path = EnemyPath.Sine(speed, amplitude, frequency), Phase = phase, Patterns = ShotsFor(kind),
        }).ToList();
    }

    /// <summary>A line of enemies side by side, all arriving together.</summary>
    private static List<EnemySpec> Row(SpiritKind kind, int count, float from, float to, float start, float speed = 140)
    {
        return Enumerable.Range(0, count).Select(i =>
        {
            float t = count > 1 ? (float)i / (count - 1) : 0.5f;
            return new EnemySpec
            {
                Time = start, Kind = kind, Position = new Vector2(from + (to - from) * t, Top), Path = EnemyPath.Straight(new Vector2(0, speed)),
                Patterns = ShotsFor(kind),
            };
        }).ToList();
    }

    /// <summary>A V with its point first. count should be odd.</summary>
    private static List<EnemySpec> Vee(SpiritKind kind, int count, float centre, float start, float spacing = 44, float speed = 150)
    {
        int half = count / 2;
        return Enumerable.Range(-half, 2 * half + 1).Select(i => new EnemySpec
        {
            Time = start, Kind = kind, Position = new Vector2(centre + i * spacing, Top - Math.Abs(i) * spacing * 0.9f),
            Path = EnemyPath.Straight(new Vector2(0, speed)), Patterns = ShotsFor(kind),
        }).ToList();
    }

    /// <summary>A ring of enemies that comes down as one.</summary>
    private static List<EnemySpec> Ring(SpiritKind kind, int count, Vector2 centre, float radius, float start, float speed = 120)
    {
        return Enumerable.Range(0, count).Select(i =>
        {
            float a = 2 * MathF.PI * i / count;
            return new EnemySpec
            {
                Time = start, Kind = kind, Position = centre + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius,
                Path = EnemyPath.Straight(new Vector2(0, speed)), Patterns = ShotsFor(kind),
            };
        }).ToList();
    }

    /// <summary>A stream of enemies from the same point, all curving the same way.</summary>
    private static List<EnemySpec> Stream(SpiritKind kind, int count, Vector2 from, Vector2 velocity, float start, float interval, float turn = 0)
    {
        return Enumerable.Range(0, count).Select(i => new EnemySpec
        {
            Time = start + i * interval, Kind = kind, Position = from,
            Path = turn == 0 ? EnemyPath.Straight(velocity) : EnemyPath.Arc(velocity, turn), Patterns = ShotsFor(kind),
        }).ToList();
    }

    /// <summary>A red spirit that arrives alone, settles in front of the player and works through its patterns.</summary>
    private static List<EnemySpec> Heavy(Vector2 from, Vector2 to, float start, float hold, List<ScheduledPattern> patterns)
    {
        return new List<EnemySpec>
        {
            new EnemySpec
            {
                Time = start, Kind = SpiritKind.Heavy, Position = from, Path = EnemyPath.Dive(to, 170, hold, new Vector2(0, -190)),
                Patterns = patterns, FirstShotDelay = 0.5f,
            },
        };
    }

    private static Vector2 V(float x, float y) => new Vector2(x, y);
    private static ScheduledPattern P(FirePattern pattern, float duration) => new ScheduledPattern(pattern, duration);
    private const SpiritKind Swarm = SpiritKind.Swarm, Gunner = SpiritKind.Gunner;
    private const float Pi = MathF.PI;

    // The waves

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

        // 3. The first green gunners, from one side and then the other.
        new Wave(Cat(Stream(Gunner, 4, V(-30, 70), V(120, 75), 0, 0.7f),
                     Stream(Gunner, 4, V(630, 70), V(-120, 75), 4, 0.7f),
                     Column(Swarm, 6, 300, 8, 0.35f, amplitude: 90))),

        // 4. Rings of blue spirits.
        new Wave(Cat(Ring(Swarm, 10, V(300, -70), 62, 0),
                     Ring(Swarm, 12, V(190, -90), 70, 3.5f),
                     Ring(Swarm, 12, V(410, -90), 70, 3.5f),
                     Column(Swarm, 6, 90, 7, 0.35f, phase: 0),
                     Column(Swarm, 6, 510, 7, 0.35f, phase: Pi))),

        // 5. The first red spirit, alone, with blue ones sweeping by to distract.
        new Wave(Cat(Heavy(V(300, -30), V(300, 175), 0, 10, new List<ScheduledPattern>
                     {
                         P(FirePattern.Ring(0.55f, 14, 150, 9), 4.8f),
                         P(FirePattern.Fan(0.8f, 5, 50, 240), 4.7f),
                     }),
                     Row(Swarm, 7, 80, 520, 5.5f),
                     Row(Swarm, 7, 120, 480, 9))),

        // 6. Green and blue at once, from opposite sides.
        new Wave(Cat(Stream(Gunner, 5, V(630, 60), V(-150, 80), 0, 0.6f, turn: 12),
                     Row(Swarm, 8, 50, 290, 0.5f),
                     Ring(Swarm, 8, V(300, -60), 50, 5),
                     Column(Gunner, 3, 100, 5.5f, 0.8f))),

        // 7. Green pincers closing in, with a long blue snake between them.
        new Wave(Cat(Stream(Gunner, 4, V(-30, 100), V(140, 110), 0, 0.6f, turn: -15),
                     Stream(Gunner, 4, V(630, 100), V(-140, 110), 0, 0.6f, turn: 15),
                     Column(Swarm, 10, 300, 2, 0.3f, amplitude: 120, frequency: 2.5f),
                     Vee(Swarm, 7, 300, 7),
                     Row(Gunner, 3, 150, 450, 8, speed: 100))),

        // 8. A red spirit from the right, a green group from the left, blue ones in between.
        new Wave(Cat(Heavy(V(640, -20), V(470, 160), 0, 10, new List<ScheduledPattern>
                     {
                         P(FirePattern.Spiral(0.075f, 3, 7.5f, 150), 4.5f),
                         P(FirePattern.Ring(0.7f, 18, 140, 6), 4.5f),
                     }),
                     Stream(Gunner, 5, V(-30, 60), V(130, 85), 1, 0.6f),
                     Column(Swarm, 8, 280, 3, 0.35f),
                     Row(Swarm, 8, 60, 360, 8))),

        // 9. All three colours together.
        new Wave(Cat(Row(Swarm, 12, 40, 560, 0, speed: 150),
                     Stream(Gunner, 6, V(-30, 80), V(130, 90), 1.5f, 0.5f),
                     Heavy(V(640, -20), V(450, 170), 1.5f, 12, new List<ScheduledPattern>
                     {
                         P(FirePattern.Fan(0.55f, 7, 70, 250), 3.5f),
                         P(FirePattern.Ring(0.6f, 16, 160, 11), 3.5f),
                         P(FirePattern.Spiral(0.08f, 4, 9, 150), 3.5f),
                     }),
                     Ring(Swarm, 10, V(190, -70), 60, 6),
                     Ring(Swarm, 10, V(410, -70), 60, 7.5f),
                     Stream(Gunner, 5, V(630, 90), V(-130, 90), 9, 0.5f),
                     Column(Swarm, 8, 100, 10, 0.35f),
                     Column(Swarm, 8, 500, 10, 0.35f, phase: Pi))),
    };
}
