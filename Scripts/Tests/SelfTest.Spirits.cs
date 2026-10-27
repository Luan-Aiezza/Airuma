using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of the spirits: their paths, their patterns and their lights.</summary>
public static partial class SelfTest
{
    private static void RegisterSpirits(Action<string, Action> T)
    {
        T("StraightPath", StraightPath);
        T("SinePathSwaysAroundItsColumn", SinePathSwaysAroundItsColumn);
        T("ArcPathCurves", ArcPathCurves);
        T("DivePathArrivesHoldsAndLeaves", DivePathArrivesHoldsAndLeaves);
        T("FairiesAreRemovedAfterLeavingTheScreen", FairiesAreRemovedAfterLeavingTheScreen);
        T("TheSwarmNeverShoots", TheSwarmNeverShoots);
        T("AGunnerShootsOneAimedBulletAtATime", AGunnerShootsOneAimedBulletAtATime);
        T("AHeavyShootsRingsFansAndSpirals", AHeavyShootsRingsFansAndSpirals);
        T("PatternsTakeTurns", PatternsTakeTurns);
        T("FairiesOnlyShootInsideTheScreen", FairiesOnlyShootInsideTheScreen);
        T("ThereAreNineWavesThatStartEasyAndEndWithAllThreeColours", ThereAreNineWavesThatStartEasyAndEndWithAllThreeColours);
    }

    private static void RunEnemy(EnemyEntity e, float seconds) { for (int i = 0; i < (int)(seconds * 60); i++) EnemySystem.Move(e, 1f / 60); }

    private static void StraightPath()
    {
        var e = Enemy(EnemyPath.Straight(V(0, 100)));
        RunEnemy(e, 2);
        Near(e.Position.Y, -30 + 200, 1);
        Near(e.Position.X, 300, 0.01f);
    }

    private static void SinePathSwaysAroundItsColumn()
    {
        var e = Enemy(EnemyPath.Sine(100, 60, 2), at: V(200, -30));
        float minX = 1000, maxX = -1000;
        for (int i = 0; i < 60 * 4; i++)
        {
            EnemySystem.Move(e, 1f / 60);
            minX = MathF.Min(minX, e.Position.X); maxX = MathF.Max(maxX, e.Position.X);
        }
        Near(minX, 140, 3);
        Near(maxX, 260, 3);
        Greater(e.Position.Y, 300);
    }

    private static void ArcPathCurves()
    {
        var e = Enemy(EnemyPath.Arc(V(200, 0), 30), at: V(-30, 100));
        RunEnemy(e, 3);
        Greater(e.Position.X, 100);
        Greater(e.Position.Y, 180, "turning downwards");
    }

    private static void DivePathArrivesHoldsAndLeaves()
    {
        var e = Enemy(EnemyPath.Dive(V(300, 175), 170, 5, V(0, -190)), SpiritKind.Heavy);
        RunEnemy(e, 1);
        Less(e.Position.Y, 175, "still on its way");
        RunEnemy(e, 3);
        Near(e.Position.Y, 175, 0.5f, "it hovers");
        Near(e.Position.X, 300, 0.5f);
        RunEnemy(e, 4);
        Less(e.Position.Y, 175 - 100, "and then it retreats");
    }

    private static void FairiesAreRemovedAfterLeavingTheScreen()
    {
        var e = Enemy(EnemyPath.Straight(V(0, 300)), at: V(300, 700));
        False(EnemySystem.IsOffScreen(e, World.PlayArea));
        RunEnemy(e, 2);
        True(EnemySystem.IsOffScreen(e, World.PlayArea));
        False(EnemySystem.IsOffScreen(Enemy(EnemyPath.Straight(Vector2.Zero), at: V(300, -30)), World.PlayArea), "just entering is not leaving");
    }

    private static List<BulletData> Fire(EnemyEntity e, float seconds, Vector2? target = null)
    {
        var pool = Pool(600);
        for (int i = 0; i < (int)(seconds * 60); i++)
        {
            EnemySystem.Move(e, 1f / 60);
            EnemySystem.Fire(e, target ?? V(300, 700), pool, World.PlayArea, 1f / 60);
        }
        return pool.Where(b => b.IsActive).ToList();
    }

    private static List<ScheduledPattern> Patterns(params ScheduledPattern[] p) => p.ToList();

    private static void TheSwarmNeverShoots()
    {
        var e = Enemy(EnemyPath.Straight(Vector2.Zero), SpiritKind.Swarm, V(300, 200));
        True(Fire(e, 10).Count == 0);
    }

    private static void AGunnerShootsOneAimedBulletAtATime()
    {
        var aimed = Patterns(new ScheduledPattern(FirePattern.Aimed(1.5f, 200), 100));
        var e = Enemy(EnemyPath.Straight(Vector2.Zero), SpiritKind.Gunner, V(300, 200), aimed, 0.5f);
        var pool = Pool(100);
        for (int i = 0; i < (int)(0.4f * 60); i++) { EnemySystem.Fire(e, V(300, 700), pool, World.PlayArea, 1f / 60); EnemySystem.Move(e, 1f / 60); }
        Eq(ActiveCount(pool), 0, "nothing before its first-shot delay");
        for (int i = 0; i < (int)(0.5f * 60); i++) { EnemySystem.Fire(e, V(300, 700), pool, World.PlayArea, 1f / 60); EnemySystem.Move(e, 1f / 60); }
        var first = pool.Where(b => b.IsActive).ToList();
        Eq(first.Count, 1, "exactly one bullet");
        if (first.Count == 1)
        {
            var bullet = first[0];
            Eq(bullet.Style, BulletStyle.OrbGreen);
            Eq(bullet.Faction, Faction.Boss, "enemy bullets hurt the player");
            Greater(bullet.Velocity.Y, 150, "straight at the boat below");
            Near(bullet.Velocity.X, 0, 1);
        }
        Eq(Fire(Enemy(EnemyPath.Straight(Vector2.Zero), SpiritKind.Gunner, V(300, 200), aimed, 0.5f), 6.2f).Count, 4, "one every 1.5s");
    }

    private static void AHeavyShootsRingsFansAndSpirals()
    {
        var ring = Enemy(EnemyPath.Straight(Vector2.Zero), SpiritKind.Heavy, V(300, 200), Patterns(new ScheduledPattern(FirePattern.Ring(1, 14, 100, 9), 100)));
        var ringBullets = Fire(ring, 1.1f);
        Eq(ringBullets.Count, 14);
        True(ringBullets.All(b => b.Style == BulletStyle.OrbRed));
        var angles = ringBullets.Select(b => MathF.Atan2(b.Velocity.Y, b.Velocity.X)).OrderBy(a => a).ToList();
        float maxGap = 0;
        for (int i = 1; i < angles.Count; i++) maxGap = MathF.Max(maxGap, angles[i] - angles[i - 1]);
        Near(maxGap, 2 * MathF.PI / 14, 0.05f);

        var fan = Enemy(EnemyPath.Straight(Vector2.Zero), SpiritKind.Heavy, V(300, 200), Patterns(new ScheduledPattern(FirePattern.Fan(1, 7, 60, 200), 100)));
        var fanBullets = Fire(fan, 1.1f);
        Eq(fanBullets.Count, 7);
        True(fanBullets.All(b => b.Style == BulletStyle.RiceRed && b.Velocity.Y > 0), "all aimed down at the player");

        var spiral = Enemy(EnemyPath.Straight(Vector2.Zero), SpiritKind.Heavy, V(300, 200), Patterns(new ScheduledPattern(FirePattern.Spiral(0.1f, 3, 8, 120), 100)));
        GreaterEq(Fire(spiral, 1.3f).Count, 27, "a lot of bullets");
    }

    private static void PatternsTakeTurns()
    {
        var e = Enemy(EnemyPath.Straight(Vector2.Zero), SpiritKind.Heavy, V(300, 200), Patterns(
            new ScheduledPattern(FirePattern.Ring(0.5f, 10, 100, 0), 2),
            new ScheduledPattern(FirePattern.Fan(0.5f, 5, 40, 100), 2)));
        var pool = Pool(600);
        var styles = new HashSet<BulletStyle>();
        for (int i = 0; i < 60 * 4; i++)
        {
            EnemySystem.Move(e, 1f / 60);
            EnemySystem.Fire(e, V(300, 700), pool, World.PlayArea, 1f / 60);
            foreach (var b in pool) if (b.IsActive && b.Timer == 0) styles.Add(b.Style);
        }
        True(styles.SetEquals(new[] { BulletStyle.OrbRed, BulletStyle.RiceRed }));
    }

    private static void FairiesOnlyShootInsideTheScreen()
    {
        var aimed = Patterns(new ScheduledPattern(FirePattern.Aimed(0.2f, 200), 100));
        var offscreen = Enemy(EnemyPath.Straight(Vector2.Zero), SpiritKind.Gunner, V(300, -30), aimed);
        True(Fire(offscreen, 3).Count == 0);
    }

    // The script

    private static void ThereAreNineWavesThatStartEasyAndEndWithAllThreeColours()
    {
        var waves = Stage1Waves.Waves;
        Eq(waves.Count, 9);
        True(waves[0].Spawns.All(s => s.Kind == SpiritKind.Swarm), "wave 1 is only the easy blue ones");
        True(waves[1].Spawns.All(s => s.Kind == SpiritKind.Swarm));
        True(waves[2].Spawns.Any(s => s.Kind == SpiritKind.Gunner), "green gunners arrive in wave 3");
        Eq(waves.FindIndex(w => w.Spawns.Any(s => s.Kind == SpiritKind.Heavy)), 4, "the first red fairy is wave 5");
        True(waves[8].Spawns.Select(s => s.Kind).Distinct().Count() == 3, "the finale has all three");
    }
}
