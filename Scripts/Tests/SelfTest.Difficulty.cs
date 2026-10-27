using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of the difficulty levels.</summary>
public static partial class SelfTest
{
    private static void RegisterDifficulty(Action<string, Action> T)
    {
        T("RingsFanAndSpiralsGrowWithTheDifficulty", RingsFanAndSpiralsGrowWithTheDifficulty);
        T("AGunnersShotIsHalvedOnEasyAndTripledOnHard", AGunnersShotIsHalvedOnEasyAndTripledOnHard);
        T("TheBossFiresFewerBulletsOnEasyAndMoreOnHard", TheBossFiresFewerBulletsOnEasyAndMoreOnHard);
        T("DifficultyChangesNothingButTheBullets", DifficultyChangesNothingButTheBullets);
        T("TheScoreTableRemembersTheDifficulty", TheScoreTableRemembersTheDifficulty);
        T("TheBombAndGatheringLightAreReportedForTheirSounds", TheBombAndGatheringLightAreReportedForTheirSounds);
        T("TheRunRemembersTheOrderOfTheCardsForTheHud", TheRunRemembersTheOrderOfTheCardsForTheHud);
        T("ShotsThatDoNotKillAreReportedForTheirSound", ShotsThatDoNotKillAreReportedForTheirSound);
        T("LevelsFollowTheEnergy", LevelsFollowTheEnergy);
    }

    private static void RingsFanAndSpiralsGrowWithTheDifficulty()
    {
        var ring = FirePattern.Ring(1, 14, 100, 9);
        True(Volley(Difficulty.Easy, SpiritKind.Heavy, ring) < Volley(Difficulty.Normal, SpiritKind.Heavy, ring));
        Eq(Volley(Difficulty.Normal, SpiritKind.Heavy, ring), 14, "normal is the game as designed");
        True(Volley(Difficulty.Hard, SpiritKind.Heavy, ring) > 14 + 1, "hard has more, and an extra aimed one");
        var fan = FirePattern.Fan(1, 7, 60, 200);
        True(Volley(Difficulty.Easy, SpiritKind.Heavy, fan) < 7 && Volley(Difficulty.Hard, SpiritKind.Heavy, fan) > 7);
        var spiral = FirePattern.Spiral(0.1f, 3, 8, 120);
        True(Volley(Difficulty.Easy, SpiritKind.Heavy, spiral) < Volley(Difficulty.Normal, SpiritKind.Heavy, spiral));
        True(Volley(Difficulty.Hard, SpiritKind.Heavy, spiral) > Volley(Difficulty.Normal, SpiritKind.Heavy, spiral));
    }

    private static void AGunnersShotIsHalvedOnEasyAndTripledOnHard()
    {
        var aimed = FirePattern.Aimed(0.5f, 200);
        int normal = Volley(Difficulty.Normal, SpiritKind.Gunner, aimed, 2.6f);
        int easy = Volley(Difficulty.Easy, SpiritKind.Gunner, aimed, 2.6f);
        int hard = Volley(Difficulty.Hard, SpiritKind.Gunner, aimed, 2.6f);
        True(easy < normal, $"easy {easy} < normal {normal}");
        Eq(hard, normal * 3, "three-way volleys on hard");
    }

    private static void TheBossFiresFewerBulletsOnEasyAndMoreOnHard()
    {
        foreach (int lives in new[] { 6, 5, 4, 3, 2, 1 })
        {
            int Total(Difficulty d)
            {
                var boss = new BossEntity(IrysshBoss.Definition) { Transform = new Transform2DComponent(), Health = new HealthComponent { Lives = lives } };
                PhaseManagerSystem.ChangePhase(boss, lives, d);
                return (boss.EmitterMain.IsActive ? boss.EmitterMain.BulletsPerVolley : 0) + (boss.EmitterSub.IsActive ? boss.EmitterSub.BulletsPerVolley : 0);
            }
            True(Total(Difficulty.Easy) < Total(Difficulty.Normal), $"phase with {lives} lives: easy has fewer");
            True(Total(Difficulty.Hard) > Total(Difficulty.Normal), $"phase with {lives} lives: hard has more");
        }
    }

    private static void DifficultyChangesNothingButTheBullets()
    {
        foreach (int lives in new[] { 6, 5, 4, 3, 2, 1 })
        {
            BossEntity Make(Difficulty d)
            {
                var boss = new BossEntity(IrysshBoss.Definition) { Transform = new Transform2DComponent(), Health = new HealthComponent { Lives = lives, CurrentHealth = 100, MaxHealth = 100 } };
                PhaseManagerSystem.ChangePhase(boss, lives, d);
                return boss;
            }
            var normal = Make(Difficulty.Normal); var hard = Make(Difficulty.Hard); var easy = Make(Difficulty.Easy);
            Eq(hard.EmitterMain.BulletSpeed, normal.EmitterMain.BulletSpeed);
            Eq(easy.EmitterMain.FireRate, normal.EmitterMain.FireRate);
            Eq(hard.Health.MaxHealth, normal.Health.MaxHealth);
            Eq(hard.TargetPosition, normal.TargetPosition);
        }
        foreach (var kind in Enum.GetValues<SpiritKind>())
            Eq(new EnemyEntity(1, kind, Vector2.Zero, EnemyPath.Straight(Vector2.Zero)).Health, kind.MaxHealth());
        Eq(new World(difficulty: Difficulty.Hard).Player.Health.Lives, new World(difficulty: Difficulty.Easy).Player.Health.Lives);
    }

    private static void TheScoreTableRemembersTheDifficulty()
    {
        var saved = HighScore.Entries;
        try
        {
            HighScore.Entries = new List<ScoreEntry>();
            HighScore.Submit(1000, false, Difficulty.Hard);
            HighScore.Submit(500, true, Difficulty.Easy);
            var table = HighScore.Entries;
            Eq(table[0].Difficulty, Difficulty.Hard);
            Eq(table[1].Difficulty, Difficulty.Easy);
        }
        finally { HighScore.Entries = saved; }
    }

    private static void TheBombAndGatheringLightAreReportedForTheirSounds()
    {
        var world = new World(withCards: false);
        world.HoldWavesForTesting();
        world.CalmWindForTesting();
        var input = Idle(); input.IsBombing = true;
        world.Step(Dt, input);
        True(world.TakeEvents().Any(e => e.Kind == WorldEventKind.BombFired), "the bomb flares");
        world.Step(Dt, input);
        False(world.TakeEvents().Any(e => e.Kind == WorldEventKind.BombFired), "once, not on every step");
        LightSystem.Spawn(world.Lights, world.Player.Transform.Position, 1, 0);
        world.Step(Dt, Idle());
        True(world.TakeEvents().Any(e => e.Kind == WorldEventKind.LightGathered), "a light is gathered");
    }

    private static void TheRunRemembersTheOrderOfTheCardsForTheHud()
    {
        var run = new RunState();
        run.Add(Card.Fervor); run.Add(Card.SlimWake); run.Add(Card.Fervor); run.Add(Card.SlimWake);
        True(run.History.SequenceEqual(new[] { Card.Fervor, Card.SlimWake, Card.Fervor }), "a card at its limit is not counted twice: " + string.Join(",", run.History));
        Eq(RunState.MaxCards, 12);
        Eq(new World().Run.History.Count, 0);
        var world = new World(seed: 3);
        world.Choose(0);
        Eq(world.Run.History.Count, 1);
        world.Reset();
        Eq(world.Run.History.Count, 0, "lost with the run");
        True(HudTheme.ForStage(1).Name != HudTheme.ForStage(99).Name, "stage 1 has a look of its own");
    }

    private static void ShotsThatDoNotKillAreReportedForTheirSound()
    {
        var world = new World(withCards: false);
        world.HoldWavesForTesting();
        var gunner = new EnemyEntity(300, SpiritKind.Gunner, V(300, 300), EnemyPath.Straight(Vector2.Zero));
        var swarm = new EnemyEntity(301, SpiritKind.Swarm, V(200, 300), EnemyPath.Straight(Vector2.Zero));
        world.AddForTesting(gunner); world.AddForTesting(swarm);
        BulletSystem.Spawn(world.Bullets, gunner.Position, Vector2.Zero, Faction.Player, BulletStyle.LightBolt);
        BulletSystem.Spawn(world.Bullets, swarm.Position, Vector2.Zero, Faction.Player, BulletStyle.LightBolt);
        world.Step(Dt, Idle());
        var events = world.TakeEvents();
        True(events.Any(e => e.Kind == WorldEventKind.EnemyHit && e.Spirit == SpiritKind.Gunner), "the gunner is hit and stands");
        False(events.Any(e => e.Kind == WorldEventKind.EnemyHit && e.Spirit == SpiritKind.Swarm), "the swarm dies in one shot: no hit sound");
        True(events.Any(e => e.Kind == WorldEventKind.EnemyDefeated && e.Spirit == SpiritKind.Swarm));

        var boss = new World(practicePhase: 1);
        boss.Boss.Transform.Position = V(300, Layout.BossY);
        boss.Boss.MovementType = BossMovementType.Stationary;
        boss.Boss.EmitterMain.IsActive = false; boss.Boss.EmitterSub.IsActive = false;
        BulletSystem.Spawn(boss.Bullets, boss.Boss.Transform.Position, Vector2.Zero, Faction.Player, BulletStyle.LightBolt);
        boss.Step(Dt, Idle());
        True(boss.TakeEvents().Any(e => e.Kind == WorldEventKind.BossHit), "the boss is hit");
    }

    // Power

    private static void LevelsFollowTheEnergy()
    {
        Eq(PowerCurve.Level(0), 1);
        Eq(PowerCurve.Level(PowerCurve.Level2 - 1), 1);
        Eq(PowerCurve.Level(PowerCurve.Level2), 2);
        Eq(PowerCurve.Level(PowerCurve.Level3 - 1), 2);
        Eq(PowerCurve.Level(PowerCurve.Level3), 3);
        Less(PowerCurve.Level2, PowerCurve.Level3);
        Greater(PowerCurve.Level2, 10, "level 2 is earned, not given");
    }
}
