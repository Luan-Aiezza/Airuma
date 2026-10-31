using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of the building blocks of the waves, and of the waves of stage 1.</summary>
public static partial class SelfTest
{
    private static void RegisterWaves(Action<string, Action> T)
    {
        T("TheThreeKindsBehaveAsDescribed", TheThreeKindsBehaveAsDescribed);
        T("TheFormationsDiffer", TheFormationsDiffer);
        T("EveryoneEntersFromOutsideThePlayArea", EveryoneEntersFromOutsideThePlayArea);
        T("TheWavesOfStageOneGetBusier", TheWavesOfStageOneGetBusier);
    }

    private static void TheThreeKindsBehaveAsDescribed()
    {
        var specs = WaveBuilder.Cat(WaveBuilder.Column(SpiritKind.Swarm, 3, 300, 0, 0.3f),
                                    WaveBuilder.Column(SpiritKind.Gunner, 3, 200, 0, 0.3f),
                                    WaveBuilder.Heavy(V(300, -30), V(300, 175), 0, 10, new List<ScheduledPattern> { new ScheduledPattern(FirePattern.Ring(0.5f, 10, 150, 5), 4), new ScheduledPattern(FirePattern.Fan(0.8f, 5, 50, 240), 4) }));
        foreach (var spec in specs)
            switch (spec.Kind)
            {
                case SpiritKind.Swarm: True(spec.Patterns.Count == 0, "blue fairies do not shoot"); break;
                case SpiritKind.Gunner:
                    True(spec.Patterns.Count > 0 && spec.Patterns[0].Pattern.Kind == FirePatternKind.Aimed, "green fairies shoot aimed single bullets");
                    Eq(spec.Patterns.Count, 1);
                    break;
                case SpiritKind.Heavy: GreaterEq(spec.Patterns.Count, 2, "red fairies have patterns"); break;
            }
    }

    private static void TheFormationsDiffer()
    {
        List<Vector2> Sorted(List<EnemySpec> specs) => specs.Select(s => s.Position).OrderBy(p => p.X).ToList();
        // A V: symmetrical, with the point lower than the arms.
        var vee = Sorted(WaveBuilder.Vee(SpiritKind.Swarm, 9, 300, 0));
        Eq(vee.Count, 9);
        Near(vee.First().Y, vee.Last().Y, 0.01f);
        Greater(vee[4].Y, vee[0].Y);
        // A line: everybody at the same height.
        var line = Sorted(WaveBuilder.Row(SpiritKind.Swarm, 7, 100, 500, 6.8f));
        Eq(line.Select(p => p.Y).Distinct().Count(), 1);
        Eq(line.Count, 7);
        // A ring: around one centre.
        var ring = Sorted(WaveBuilder.Ring(SpiritKind.Swarm, 10, V(300, -70), 62, 0));
        Eq(ring.Count, 10);
        float cx = ring.Sum(p => p.X) / ring.Count, cy = ring.Sum(p => p.Y) / ring.Count;
        foreach (var p in ring) Near(MathF.Sqrt((p.X - cx) * (p.X - cx) + (p.Y - cy) * (p.Y - cy)), 62, 0.5f);
    }

    private static void EveryoneEntersFromOutsideThePlayArea()
    {
        foreach (var wave in Stage1Waves.Waves)
            foreach (var spec in wave.Spawns)
            {
                bool outside = spec.Position.Y < 0 || spec.Position.X < 0 || spec.Position.X > 600 || spec.Position.Y > 775;
                True(outside, $"{spec.Kind} at {spec.Position} would pop in view");
            }
    }

    private static void TheWavesOfStageOneGetBusier()
    {
        var counts = Stage1Waves.Waves.Select(w => w.Spawns.Count).ToList();
        Greater(counts[^1], counts[0]);
        Greater(counts.Max(), 25, "the busiest wave has many blue ones");
    }

    // Stage flow

    private static float PlayStage(World world, bool killing = true, float maxSeconds = 400)
    {
        float t = 0;
        while (t < maxSeconds && world.Part == StagePart.Waves)
        {
            world.Player.Health.InvincibilityTimer = 10;
            if (killing) foreach (var e in world.Enemies) e.Health = 0;
            world.Step(1f / 60, Idle());
            t += 1f / 60;
        }
        return t;
    }
}
