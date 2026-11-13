using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of the flow of the stage: waves, cards, the boss.</summary>
public static partial class SelfTest
{
    private static void RegisterStageFlow(Action<string, Action> T)
    {
        T("TheSerpentHasMoreHealthThanTheWoman", TheSerpentHasMoreHealthThanTheWoman);
        T("TheHealthOfTheBossesGrowsAndAStillBossHasTwiceAsMuch", TheHealthOfTheBossesGrowsAndAStillBossHasTwiceAsMuch);
        T("AStageIsOnlyDataTheWorldRuns", AStageIsOnlyDataTheWorldRuns);
        T("TheFirstStageIsMadeOfItsWavesAndItsBoss", TheFirstStageIsMadeOfItsWavesAndItsBoss);
        T("ARunStartsWithFairiesAndTheBossWaits", ARunStartsWithFairiesAndTheBossWaits);
        T("BossPracticeSkipsTheFairies", BossPracticeSkipsTheFairies);
        T("AllTheWavesComeBeforeTheBoss", AllTheWavesComeBeforeTheBoss);
        T("TheBossEntersAndStartsItsFirstAttack", TheBossEntersAndStartsItsFirstAttack);
        T("TheWavesWaitUntilTheFairiesAreDealtWith", TheWavesWaitUntilTheFairiesAreDealtWith);
        T("DefeatingAFairyScoresAndReportsIt", DefeatingAFairyScoresAndReportsIt);
        T("PlayerShotsAreUsedUpOnTheFirstFairyTheyHit", PlayerShotsAreUsedUpOnTheFirstFairyTheyHit);
        T("BossShotsAreIgnoredWhileTheFairiesCome", BossShotsAreIgnoredWhileTheFairiesCome);
        T("ResetStartsTheStageOver", ResetStartsTheStageOver);
    }

    private static void ARunStartsWithFairiesAndTheBossWaits()
    {
        var world = new World(withCards: false);
        Eq(world.Part, StagePart.Waves);
        Less(world.Boss.Transform.Position.Y, -200, "the boss is off-screen");
        StepN(world, 60 * 10);
        False(world.Bullets.Any(b => b.IsActive && b.Faction == Faction.Boss && b.Style == BulletStyle.Fireball), "no boss bullets");
        Eq(world.Boss.Health.CurrentHealth, IrysshBoss.Definition.PhaseFor(4).MaxHealth);
    }

    private static void BossPracticeSkipsTheFairies()
    {
        Eq(new World(practicePhase: 1).Part, StagePart.Boss);
        Eq(new World(practicePhase: 6).Part, StagePart.Boss);
    }

    private static void AllTheWavesComeBeforeTheBoss()
    {
        var world = new World(withCards: false);
        var waveStarts = new List<int>();
        bool bossEntered = false;
        float t = 0;
        while (t < 500 && world.Part == StagePart.Waves)
        {
            world.Player.Health.InvincibilityTimer = 10;
            foreach (var e in world.Enemies) e.Health = 0;
            world.Step(1f / 60, Idle());
            foreach (var ev in world.TakeEvents())
            {
                if (ev.Kind == WorldEventKind.WaveStarted) waveStarts.Add(ev.Int);
                if (ev.Kind == WorldEventKind.BossEntered) bossEntered = true;
            }
            t += 1f / 60;
        }
        True(waveStarts.SequenceEqual(Enumerable.Range(0, Stages.One.Waves.Count)), "every wave, in order");
        Eq(world.Part, StagePart.Boss);
        True(bossEntered);
        Greater(t, Stages.One.Waves.Count * WaveSystem.PauseBetweenWaves, "the waves take time");
    }

    private static void TheBossEntersAndStartsItsFirstAttack()
    {
        var world = new World(withCards: false);
        PlayStage(world);
        Eq(world.Boss.Health.Lives, Stages.One.Boss.Lives);
        Eq(world.Boss.MovementType, BossMovementType.Transition);
        True(world.Boss.EmitterMain.IsActive || world.Boss.Patterns.Length > 0, "it fires");
        Less(world.Boss.Transform.Position.Y, 140);
        StepN(world, 60 * 3);
        Near(world.Boss.Transform.Position.Y, 140, 25, "and settles");
    }

    private static void TheWavesWaitUntilTheFairiesAreDealtWith()
    {
        var world = new World(withCards: false);
        int maxWave = 0;
        for (int i = 0; i < 60 * 20; i++)
        {
            world.Player.Health.InvincibilityTimer = 10;
            world.Step(1f / 60, Idle());
            maxWave = Math.Max(maxWave, world.WaveIndex);
        }
        LessEq(maxWave, 3);
    }

    private static void DefeatingAFairyScoresAndReportsIt()
    {
        var world = new World(withCards: false);
        world.StartWave(0);
        EnemyEntity found = null;
        for (int i = 0; i < 60 * 4 && found == null; i++)
        {
            world.Step(Dt, Idle());
            found = world.Enemies.FirstOrDefault();
        }
        True(found != null, "no fairy came in");
        if (found == null) return;
        world.TakeEvents();
        int before = world.Score;
        // A shot right on top of it.
        Place(world, new BulletData { IsActive = true, Position = found.Position, Velocity = Vector2.Zero, Faction = Faction.Player, HitboxRadius = 5, Damage = 99, Style = BulletStyle.LightBolt });
        world.Step(Dt, Idle());
        Eq(world.Score, before + ScoreSystem.PointsPerSpiritHit + SpiritKind.Swarm.Points());
        False(world.Enemies.Contains(found));
        True(world.TakeEvents().Any(e => e.Kind == WorldEventKind.EnemyDefeated && e.Spirit == SpiritKind.Swarm));
    }

    private static void PlayerShotsAreUsedUpOnTheFirstFairyTheyHit()
    {
        var pool = Pool(5);
        var a = new EnemyEntity(1, SpiritKind.Heavy, V(100, 100), EnemyPath.Straight(Vector2.Zero));
        var b = new EnemyEntity(2, SpiritKind.Heavy, V(105, 100), EnemyPath.Straight(Vector2.Zero));
        pool[0] = new BulletData { IsActive = true, Position = V(102, 100), Faction = Faction.Player, HitboxRadius = 5, Damage = 1, Style = BulletStyle.LightBolt };
        var report = CombatSystem.ProcessEnemyHits(pool, new List<EnemyEntity> { a, b });
        Eq(report.Hits, 1);
        Eq(a.Health + b.Health, 2 * SpiritKind.Heavy.MaxHealth() - 1);
        False(pool[0].IsActive);
    }

    private static void BossShotsAreIgnoredWhileTheFairiesCome()
    {
        var world = new World(withCards: false);
        world.Bullets[0] = new BulletData { IsActive = true, Position = V(300, 300), Velocity = Vector2.Zero, Faction = Faction.Player, HitboxRadius = 5, Damage = 1, Style = BulletStyle.LightBolt };
        world.Boss.Transform.Position = V(300, 300);
        int before = world.Boss.Health.CurrentHealth;
        world.Step(Dt, Idle());
        Eq(world.Boss.Health.CurrentHealth, before);
    }

    private static void ResetStartsTheStageOver()
    {
        var world = new World(withCards: false);
        PlayStage(world);
        Eq(world.Part, StagePart.Boss);
        world.Reset();
        Eq(world.Part, StagePart.Waves);
        Eq(world.WaveIndex, 0);
        True(world.Enemies.Count == 0);
    }

    // Spirit contact

    private static (World, EnemyEntity) WorldWithSpiritAt(Vector2 offset, SpiritKind kind = SpiritKind.Swarm)
    {
        var world = new World(withCards: false);
        world.StartWave(0);
        var spirit = new EnemyEntity(7, kind, world.Player.Transform.Position + offset, EnemyPath.Straight(Vector2.Zero));
        world.AddForTesting(spirit);
        return (world, spirit);
    }

    /// <summary>A stage with one wave and a boss of two attacks, made on the spot: the world runs it without knowing it is not the first.</summary>
    private static void AStageIsOnlyDataTheWorldRuns()
    {
        var wave = new Wave(new List<EnemySpec>
        {
            new EnemySpec { Time = 0, Kind = SpiritKind.Swarm, Position = V(300, -30), Path = EnemyPath.Straight(V(0, 300)) },
        });
        BossPhase Attack(string name, float x) => new BossPhase
        {
            AttackNameKey = name, NameKey = "Tester", HitboxRadius = 20, Movement = BossMovementType.Stationary, Target = V(x, 150),
            Main = new BulletEmitterComponent { IsActive = true, FireRate = 0.5f, BulletsPerVolley = 4, SpreadAngle = 360, BulletSpeed = 100 },
        };
        var boss = new BossDefinition("Tester", new[] { Attack("Test: One", 200), Attack("Test: Two", 400) });
        var stage = new StageDefinition("Stage X", "music", MusicPlayer.Theme.Stage, new[] { wave }, boss);

        var world = new World(withCards: false, stage: stage);
        world.CalmWindForTesting();
        Eq(world.Boss.Health.Lives, 2, "one life for each attack");
        Eq(world.Part, StagePart.Waves);
        for (int i = 0; i < 60 * 20 && world.Part == StagePart.Waves; i++) world.Step(Dt, Idle());
        Eq(world.Part, StagePart.Boss, "the one wave was enough");
        Eq(world.Boss.Hitbox.Radius, 20f);
        Eq(world.Boss.TargetPosition.X, 200f, "the first attack");
        world.Boss.Health.CurrentHealth = 0;
        world.Step(Dt, Idle());
        Eq(world.Boss.Health.Lives, 1);
        Eq(world.Boss.TargetPosition.X, 400f, "the second attack");
        Eq(boss.AttackName(1), "Test: Two");
        True(boss.AttackName(3) == null && boss.AttackName(0) == null);
    }

    private static void TheFirstStageIsMadeOfItsWavesAndItsBoss()
    {
        var stage = Stages.One;
        Eq(stage.Waves.Count, 5);
        Eq(stage.Boss.Lives, 4, "four attacks");
        True(!stage.Boss.PhaseFor(4).Enraged && !stage.Boss.PhaseFor(3).Enraged, "the first two attacks are calm");
        True(stage.Boss.PhaseFor(2).Enraged && stage.Boss.PhaseFor(1).Enraged, "the last two are enraged");
        True(stage.Boss.PhaseFor(2).Theme == stage.Boss.PhaseFor(3).Theme, "one song for the whole fight");
        Eq(stage.Boss.AttackName(4), Loc.Get("AttackRingOfEmbers"), "the first attack");
        Eq(stage.Boss.AttackName(3), Loc.Get("AttackBlazingBloom"), "the homing lotus");
        Eq(new World(withCards: false).StartingBossLives, 4);
        Eq(new World(practicePhase: 3).StartingBossLives, 2);
    }

    /// <summary>
    /// The health of the bosses grows from stage to stage (300, 330, 360, 390, 420, 450 written for the first form), the second form has a little more (10, 15, 20, 25, 30 and 35 per cent more),
    /// and an attack in which the boss stands still has twice as much as one in which it moves.
    /// </summary>
    private static void TheHealthOfTheBossesGrowsAndAStillBossHasTwiceAsMuch()
    {
        var calm = new[] { 300, 330, 360, 390, 420, 450 };
        var bonus = new[] { 0.10f, 0.15f, 0.20f, 0.25f, 0.30f, 0.35f };
        for (int i = 0; i < 6; i++)
        {
            var boss = Stages.All[i].Boss;
            int enraged = (int)Math.Round(calm[i] * (1 + bonus[i]));
            foreach (var phase in boss.Phases)
                Eq(phase.MaxHealth, (int)(BossBuilder.HealthOf(phase.Enraged ? enraged : calm[i], phase.Movement) * phase.HealthScale), $"boss {i + 1}, {phase.NameKey}: {(phase.Enraged ? "enraged" : "calm")}, {phase.Movement}");
            var still = boss.Phases.FirstOrDefault(p => p.Movement == BossMovementType.Stationary && !p.Enraged);
            var moving = boss.Phases.FirstOrDefault(p => p.Movement != BossMovementType.Stationary && !p.Enraged);
            if (still != null && moving != null) Greater(still.MaxHealth, moving.MaxHealth * 3 / 2, $"boss {i + 1}: a still attack has twice the health");
        }
        // the whole fight is longer from boss to boss
        for (int i = 1; i < 6; i++)
            Greater(Stages.All[i].Boss.Phases.Sum(p => p.MaxHealth), Stages.All[i - 1].Boss.Phases.Sum(p => p.MaxHealth), $"boss {i + 1} has more health in all than boss {i}");
    }

    private static void TheSerpentHasMoreHealthThanTheWoman()
    {
        var def = IrysshBoss.Definition;
        for (int lives = 4; lives >= 3; lives--) Eq(def.PhaseFor(lives).MaxHealth, BossBuilder.HealthOf(300, BossMovementType.Patrol), "the woman");
        Eq(def.PhaseFor(2).MaxHealth, BossBuilder.HealthOf(330, BossMovementType.Stationary), "the serpent, her true form: a tenth more, and twice as much while she stands still");
        Eq(def.PhaseFor(1).MaxHealth, BossBuilder.HealthOf(330, BossMovementType.Dash), "and when she moves");
        var world = new World(practicePhase: 1);
        world.CalmWindForTesting();
        Eq(world.Boss.Health.MaxHealth, def.PhaseFor(4).MaxHealth);
        world.Boss.Health.Lives = 3;
        world.Boss.Health.CurrentHealth = 0;
        world.Step(Dt, Idle());
        Eq(world.Boss.Health.Lives, 2);
        Eq(world.Boss.Health.MaxHealth, def.PhaseFor(2).MaxHealth, "the bar is refilled for the serpent");
        Eq(world.Boss.Health.CurrentHealth, def.PhaseFor(2).MaxHealth);
    }
}
