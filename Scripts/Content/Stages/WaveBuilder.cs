using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// The building blocks of the waves of every stage: a column of spirits that sway, a row, a V, a ring, a stream that curves, a red
/// spirit that arrives alone, a wall with gaps, a cross, a diamond, pairs that dance round each other. A stage is a list of waves put together
/// from these (see <see cref="Stage1Waves"/> and <see cref="Stage2Waves"/>). Every builder takes the shots its spirits fire (by default the
/// ones of the kind: the blue ones do not shoot, the green ones fire an aimed bullet).
/// </summary>
public static class WaveBuilder
{
    public static readonly List<ScheduledPattern> GunnerShots = new List<ScheduledPattern> { new ScheduledPattern(FirePattern.Aimed(1.5f, 220), 1000) };
    public const float Top = -34;

    public static List<ScheduledPattern> ShotsFor(SpiritKind kind) => kind == SpiritKind.Gunner ? GunnerShots : new List<ScheduledPattern>();

    public static List<EnemySpec> Cat(params List<EnemySpec>[] parts) => parts.SelectMany(p => p).ToList();

    /// <summary>Enemies one after another from the same spot, following the same sway.</summary>
    public static List<EnemySpec> Column(SpiritKind kind, int count, float x, float start, float interval,
                                          float speed = 150, float amplitude = 70, float frequency = 2.2f, float phase = 0, List<ScheduledPattern> shots = null)
    {
        return Enumerable.Range(0, count).Select(i => new EnemySpec
        {
            Time = start + i * interval, Kind = kind, Position = new Vector2(x, Top),
            Path = EnemyPath.Sine(speed, amplitude, frequency), Phase = phase, Patterns = shots ?? ShotsFor(kind),
        }).ToList();
    }

    /// <summary>A line of enemies side by side, all arriving together.</summary>
    public static List<EnemySpec> Row(SpiritKind kind, int count, float from, float to, float start, float speed = 140, List<ScheduledPattern> shots = null)
    {
        return Enumerable.Range(0, count).Select(i =>
        {
            float t = count > 1 ? (float)i / (count - 1) : 0.5f;
            return new EnemySpec
            {
                Time = start, Kind = kind, Position = new Vector2(from + (to - from) * t, Top), Path = EnemyPath.Straight(new Vector2(0, speed)),
                Patterns = shots ?? ShotsFor(kind),
            };
        }).ToList();
    }

    /// <summary>A V with its point first. count should be odd.</summary>
    public static List<EnemySpec> Vee(SpiritKind kind, int count, float centre, float start, float spacing = 44, float speed = 150, List<ScheduledPattern> shots = null)
    {
        int half = count / 2;
        return Enumerable.Range(-half, 2 * half + 1).Select(i => new EnemySpec
        {
            Time = start, Kind = kind, Position = new Vector2(centre + i * spacing, Top - Math.Abs(i) * spacing * 0.9f),
            Path = EnemyPath.Straight(new Vector2(0, speed)), Patterns = shots ?? ShotsFor(kind),
        }).ToList();
    }

    /// <summary>A ring of enemies that comes down as one.</summary>
    public static List<EnemySpec> Ring(SpiritKind kind, int count, Vector2 centre, float radius, float start, float speed = 120, List<ScheduledPattern> shots = null)
    {
        return Enumerable.Range(0, count).Select(i =>
        {
            float a = 2 * MathF.PI * i / count;
            return new EnemySpec
            {
                Time = start, Kind = kind, Position = centre + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius,
                Path = EnemyPath.Straight(new Vector2(0, speed)), Patterns = shots ?? ShotsFor(kind),
            };
        }).ToList();
    }

    /// <summary>A stream of enemies from the same point, all curving the same way.</summary>
    public static List<EnemySpec> Stream(SpiritKind kind, int count, Vector2 from, Vector2 velocity, float start, float interval, float turn = 0, List<ScheduledPattern> shots = null)
    {
        return Enumerable.Range(0, count).Select(i => new EnemySpec
        {
            Time = start + i * interval, Kind = kind, Position = from,
            Path = turn == 0 ? EnemyPath.Straight(velocity) : EnemyPath.Arc(velocity, turn), Patterns = shots ?? ShotsFor(kind),
        }).ToList();
    }

    /// <summary>A wall across the field with gaps to slip through: `slots` places in a row, the ones listed in `gaps` left empty.</summary>
    public static List<EnemySpec> Wall(SpiritKind kind, int slots, float from, float to, int[] gaps, float start, float speed = 130, List<ScheduledPattern> shots = null)
    {
        return Enumerable.Range(0, slots).Where(i => !gaps.Contains(i)).Select(i => new EnemySpec
        {
            Time = start, Kind = kind, Position = new Vector2(from + (to - from) * i / Math.Max(1, slots - 1), Top),
            Path = EnemyPath.Straight(new Vector2(0, speed)), Patterns = shots ?? ShotsFor(kind),
        }).ToList();
    }

    /// <summary>Two lines that come in from the top corners and cross in the middle.</summary>
    public static List<EnemySpec> Cross(SpiritKind kind, int count, float start, float interval, float speed = 150, List<ScheduledPattern> shots = null) =>
        Cat(Stream(kind, count, V(-30, Top), V(speed, speed * 0.75f), start, interval, shots: shots),
            Stream(kind, count, V(630, Top), V(-speed, speed * 0.75f), start, interval, shots: shots));

    /// <summary>A diamond of enemies that comes down as one: `half` rows widening, then `half` narrowing.</summary>
    public static List<EnemySpec> Diamond(SpiritKind kind, int half, float centre, float start, float spacing = 40, float speed = 140, List<ScheduledPattern> shots = null)
    {
        var list = new List<EnemySpec>();
        for (int row = 0; row <= 2 * half; row++)
        {
            int width = Math.Min(row, 2 * half - row) + 1;
            for (int k = 0; k < width; k++)
                list.Add(new EnemySpec
                {
                    Time = start, Kind = kind, Position = new Vector2(centre + (k - (width - 1) / 2f) * spacing, Top - (2 * half - row) * spacing * 0.9f),
                    Path = EnemyPath.Straight(new Vector2(0, speed)), Patterns = shots ?? ShotsFor(kind),
                });
        }
        return list;
    }

    /// <summary>Two columns that sway in opposite phases, so that they weave round each other like dancers.</summary>
    public static List<EnemySpec> DancingPairs(SpiritKind kind, int count, float centre, float gap, float start, float interval, float amplitude = 90, List<ScheduledPattern> shots = null) =>
        Cat(Column(kind, count, centre - gap / 2, start, interval, amplitude: amplitude, phase: 0, shots: shots),
            Column(kind, count, centre + gap / 2, start, interval, amplitude: amplitude, phase: MathF.PI, shots: shots));

    /// <summary>A red spirit that arrives alone, settles in front of the player and works through its patterns.</summary>
    public static List<EnemySpec> Heavy(Vector2 from, Vector2 to, float start, float hold, List<ScheduledPattern> patterns)
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

    public static Vector2 V(float x, float y) => new Vector2(x, y);
    public static ScheduledPattern P(FirePattern pattern, float duration) => new ScheduledPattern(pattern, duration);
    public const SpiritKind Swarm = SpiritKind.Swarm, Gunner = SpiritKind.Gunner;
    public const float Pi = MathF.PI;
}
