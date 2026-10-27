using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of the script of the stage.</summary>
public static partial class SelfTest
{
    private static void RegisterWaves(Action<string, Action> T)
    {
        T("TheThreeKindsBehaveAsDescribed", TheThreeKindsBehaveAsDescribed);
        T("BluesComeInBigGroupsGreensInSmallOnesAndRedAlone", BluesComeInBigGroupsGreensInSmallOnesAndRedAlone);
        T("TheFormationsDiffer", TheFormationsDiffer);
        T("LaterWavesMixColoursFromDifferentSidesAtTheSameTime", LaterWavesMixColoursFromDifferentSidesAtTheSameTime);
        T("EveryoneEntersFromOutsideThePlayArea", EveryoneEntersFromOutsideThePlayArea);
        T("TheWavesGetBusier", TheWavesGetBusier);
    }

    private static void TheThreeKindsBehaveAsDescribed()
    {
        foreach (var wave in Stage1Waves.Waves)
            foreach (var spec in wave.Spawns)
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

    private static void BluesComeInBigGroupsGreensInSmallOnesAndRedAlone()
    {
        for (int i = 0; i < Stage1Waves.Waves.Count; i++)
        {
            var wave = Stage1Waves.Waves[i];
            int reds = wave.Spawns.Count(s => s.Kind == SpiritKind.Heavy);
            LessEq(reds, 1, $"wave {i + 1}: red fairies come alone");
            int blues = wave.Spawns.Count(s => s.Kind == SpiritKind.Swarm);
            int greens = wave.Spawns.Count(s => s.Kind == SpiritKind.Gunner);
            if (blues > 0) GreaterEq(blues, 6, $"wave {i + 1}: blue ones come in excess");
            if (greens > 0) True(greens >= 3 && greens <= 11, $"wave {i + 1}: green ones in small groups");
        }
    }

    private static void TheFormationsDiffer()
    {
        var w = Stage1Waves.Waves;
        List<Vector2> Sorted(SpiritKind kind, int wave, float at) =>
            w[wave].Spawns.Where(s => s.Kind == kind && s.Time == at).Select(s => s.Position).OrderBy(p => p.X).ToList();
        // A V: symmetrical, with the point lower than the arms.
        var vee = Sorted(SpiritKind.Swarm, 1, 0);
        Eq(vee.Count, 9);
        Near(vee.First().Y, vee.Last().Y, 0.01f);
        Greater(vee[4].Y, vee[0].Y);
        // A line: everybody at the same height.
        var line = Sorted(SpiritKind.Swarm, 0, 6.8f);
        Eq(line.Select(p => p.Y).Distinct().Count(), 1);
        Eq(line.Count, 7);
        // A ring: around one centre.
        var ring = Sorted(SpiritKind.Swarm, 3, 0);
        Eq(ring.Count, 10);
        float cx = ring.Sum(p => p.X) / ring.Count, cy = ring.Sum(p => p.Y) / ring.Count;
        foreach (var p in ring) Near(MathF.Sqrt((p.X - cx) * (p.X - cx) + (p.Y - cy) * (p.Y - cy)), 62, 0.5f);
    }

    private static void LaterWavesMixColoursFromDifferentSidesAtTheSameTime()
    {
        foreach (int index in new[] { 5, 7, 8 })
        {
            var early = new Dictionary<SpiritKind, HashSet<string>>();
            foreach (var spec in Stage1Waves.Waves[index].Spawns.Where(s => s.Time <= 2))
            {
                string side = spec.Position.X < 150 ? "left" : (spec.Position.X > 450 ? "right" : "middle");
                if (!early.ContainsKey(spec.Kind)) early[spec.Kind] = new HashSet<string>();
                early[spec.Kind].Add(side);
            }
            GreaterEq(early.Keys.Count, 2, $"wave {index + 1} starts with two colours at once");
            Greater(early.Values.SelectMany(s => s).Distinct().Count(), 1, "from different sides");
        }
        // The finale's red fairy comes in while a green group is on the screen.
        var finale = Stage1Waves.Waves[8].Spawns;
        var red = finale.First(s => s.Kind == SpiritKind.Heavy);
        True(finale.Any(s => s.Kind == SpiritKind.Gunner && MathF.Abs(s.Time - red.Time) < 2 && s.Position.X < 100), "green from the left");
        Greater(red.Position.X, 500, "while the red one comes from the right");
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

    private static void TheWavesGetBusier()
    {
        var counts = Stage1Waves.Waves.Select(w => w.Spawns.Count).ToList();
        Greater(counts[8], counts[0]);
        Greater(Stage1Waves.Waves[8].LastSpawnTime, Stage1Waves.Waves[0].LastSpawnTime - 5);
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
