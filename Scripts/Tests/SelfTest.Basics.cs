using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of movement, bullets, the bomb's clearing, the boss's attacks, the score and the texts.</summary>
public static partial class SelfTest
{
    private static void RegisterBasics(Action<string, Action> T)
    {
        T("WindPushesPlayerAndBossBulletsHalfAsMuch", WindPushesPlayerAndBossBulletsHalfAsMuch);
        T("PlayerIsClampedToPlayArea", PlayerIsClampedToPlayArea);
        T("BulletsLeavingTheScreenAreRecycled", BulletsLeavingTheScreenAreRecycled);
        T("AcceleratingBulletNeverReverses", AcceleratingBulletNeverReverses);
        T("StopAndAimRetargetsPlayer", StopAndAimRetargetsPlayer);
        T("BombClearsOnlyBossBulletsInRadiusAndConsumesCharge", BombClearsOnlyBossBulletsInRadiusAndConsumesCharge);
        T("PhaseOneSetupAndTransitionSilencesBoss", PhaseOneSetupAndTransitionSilencesBoss);
        T("BossDefeatGivesVictory", BossDefeatGivesVictory);
        T("RunningOutOfLivesGivesDefeat", RunningOutOfLivesGivesDefeat);
        T("ResetRestoresInitialState", ResetRestoresInitialState);
        T("TheTextIsInEnglish", TheTextIsInEnglish);
        T("EveryBossPhaseHasAnAttackName", EveryBossPhaseHasAnAttackName);
        T("TheBossComesDownFromAboveAndOnlyThenAttacks", TheBossComesDownFromAboveAndOnlyThenAttacks);
        T("ScoreGrowsWithHitsAndPhases", ScoreGrowsWithHitsAndPhases);
        T("HighScoreOnlyKeepsTheBest", HighScoreOnlyKeepsTheBest);
        T("ActorsReachTheMiddleOfTheTreesButNoFurther", ActorsReachTheMiddleOfTheTreesButNoFurther);
        T("BossHitboxIsTwoThirdsOfItsSprite", BossHitboxIsTwoThirdsOfItsSprite);
        T("BossPatrolKeepsItsHitboxInsideTheField", BossPatrolKeepsItsHitboxInsideTheField);
        T("DashLanesFitTheSerpent", DashLanesFitTheSerpent);
        T("BossChangesFormAtPhaseFive", BossChangesFormAtPhaseFive);
        T("PracticeStartsAtTheChosenAttack", PracticeStartsAtTheChosenAttack);
        T("PracticeEndsInVictoryWhenThatAttackIsBeaten", PracticeEndsInVictoryWhenThatAttackIsBeaten);
        T("TheTableKeepsTheBestFiveInOrder", TheTableKeepsTheBestFiveInOrder);
        T("SubmitReportsANewBestOnly", SubmitReportsANewBestOnly);
        T("TheThreeKindsGetSteadilyTougher", TheThreeKindsGetSteadilyTougher);
    }

    private static void WindPushesPlayerAndBossBulletsHalfAsMuch()
    {
        var player = new PlayerEntity();
        player.Movement.Speed = 400;
        player.WindModifier.Value = 1;
        MovementSystem.MovePlayer(player, World.PlayArea, 1, V(50, 0));
        Near(player.Transform.Position.X, 50, 0.001f);

        var pool = Pool(2);
        pool[0] = new BulletData { IsActive = true, Position = V(300, 300), Faction = Faction.Boss, WindModifier = 0.5f };
        pool[1] = new BulletData { IsActive = true, Position = V(300, 300), Faction = Faction.Player, WindModifier = 1 };
        BulletSystem.MoveBullets(pool, World.PlayArea, 1, V(40, 0), player);
        Near(pool[0].Position.X, 320, 0.001f);
        Near(pool[1].Position.X, 340, 0.001f);
    }

    private static void PlayerIsClampedToPlayArea()
    {
        var player = new PlayerEntity();
        player.Movement.Speed = 400;
        player.Input.Direction = V(-1, 0);
        MovementSystem.MovePlayer(player, World.PlayArea, 10, Vector2.Zero);
        Eq(player.Transform.Position.X, 0f);
    }

    private static void BulletsLeavingTheScreenAreRecycled()
    {
        var player = new PlayerEntity();
        var pool = Pool(1);
        pool[0] = new BulletData { IsActive = true, Position = V(300, 5), Velocity = V(0, -800), Faction = Faction.Player };
        BulletSystem.MoveBullets(pool, World.PlayArea, 1, Vector2.Zero, player);
        False(pool[0].IsActive);
    }

    private static void AcceleratingBulletNeverReverses()
    {
        var player = new PlayerEntity();
        var pool = new[] { new BulletData { IsActive = true, Position = V(300, 300), Velocity = V(0, 100), Faction = Faction.Boss, Acceleration = -250 } };
        for (int i = 0; i < 60; i++) BulletSystem.MoveBullets(pool, World.PlayArea, 1f / 60, Vector2.Zero, player);
        GreaterEq(pool[0].Velocity.Y, 0);
    }

    private static void StopAndAimRetargetsPlayer()
    {
        var player = new PlayerEntity();
        player.Transform.Position = V(300, 700);
        var pool = new[] { new BulletData { IsActive = true, Position = V(300, 300), Velocity = Vector2.Zero, Faction = Faction.Boss, Behavior = BulletBehavior.StopAndAim, Timer = 1.49f } };
        BulletSystem.MoveBullets(pool, World.PlayArea, 0.02f, Vector2.Zero, player);
        Near(pool[0].Velocity.Y, 350, 0.01f);
        Near(pool[0].Velocity.X, 0, 0.01f);
    }

    private static void BombClearsOnlyBossBulletsInRadiusAndConsumesCharge()
    {
        var player = new PlayerEntity();
        player.Transform.Position = V(300, 700);
        player.Bomb = new BombComponent(3, false, 0, 5, 250);
        player.Input.IsBombing = true;
        var pool = new[]
        {
            new BulletData { IsActive = true, Position = V(300, 600), Faction = Faction.Boss },
            new BulletData { IsActive = true, Position = V(300, 100), Faction = Faction.Boss },
            new BulletData { IsActive = true, Position = V(300, 600), Faction = Faction.Player },
        };
        BombSystem.Process(player, pool, 1f / 60);
        Eq(player.Bomb.BombsLeft, 2);
        False(pool[0].IsActive);
        True(pool[1].IsActive);
        True(pool[2].IsActive);
    }

    private static void PhaseOneSetupAndTransitionSilencesBoss()
    {
        var world = new World(withCards: false);
        world.SkipToBoss();
        Eq(world.Boss.MovementType, BossMovementType.Transition);
        True(world.Boss.EmitterMain.IsActive);
        Eq(world.Boss.EmitterMain.BulletsPerVolley, 36);

        // No boss bullets while the 2s transition runs.
        StepN(world, 60);
        False(world.Bullets.Any(b => b.IsActive && b.Faction == Faction.Boss));
    }

    private static void BossDefeatGivesVictory()
    {
        var world = new World(withCards: false);
        world.SkipToBoss();
        for (int i = 0; i < 6; i++)
        {
            world.Boss.Health.CurrentHealth = 0;
            world.Step(Dt, Idle());
        }
        Eq(world.Boss.Health.Lives, 0);
        Eq(world.Result, (GameResult?)GameResult.Victory);
    }

    private static void RunningOutOfLivesGivesDefeat()
    {
        var world = new World(withCards: false);
        for (int i = 0; i < 3; i++)
        {
            world.Player.Health.CurrentHealth = 0;
            world.Step(Dt, Idle());
        }
        Eq(world.Result, (GameResult?)GameResult.Defeat);
    }

    private static void ResetRestoresInitialState()
    {
        var world = new World(withCards: false);
        world.Player.Health.CurrentHealth = 0;
        world.Step(Dt, Idle());
        world.Reset();
        Eq(world.Player.Health.Lives, 3);
        Eq(world.Boss.Health.Lives, 6);
        True(world.Result == null);
    }

    private static void TheTextIsInEnglish()
    {
        string joined = string.Join(" ", UIText.All()).ToLowerInvariant();
        foreach (var word in new[] { "pontos", "vidas", "vento", "lamparina", "força", "fada", "vitória", "derrota", "reiniciar", "recorde",
                                     "cobra", "fogo", "sacerdotisa", "solstício", "vamos", "conseguimos", "boitat", "jaci" })
            False(joined.Contains(word), word);
        Eq(UIText.Title, "Airumã");
    }

    private static void EveryBossPhaseHasAnAttackName()
    {
        for (int lives = 1; lives <= 6; lives++) True(Stages.One.Boss.AttackName(lives) != null);
        True(Stages.One.Boss.AttackName(0) == null);
    }

    private static void TheBossComesDownFromAboveAndOnlyThenAttacks()
    {
        var world = new World(practicePhase: 1);
        True(world.Boss.Transform.Position.Y < 0, "she starts above the screen");
        for (int i = 0; i < 60; i++) world.Step(Dt, Idle());
        True(world.Boss.Transform.Position.Y > -170, "she is coming down");
        True(!world.Bullets.Any(b => b.IsActive && b.Faction == Faction.Boss), "she does not attack while she comes in");
        for (int i = 0; i < (int)(PhaseManagerSystem.EntranceSeconds * 60) + 10; i++) world.Step(Dt, Idle());
        Near(world.Boss.Transform.Position.Y, Layout.BossY, 1.0f);
        Eq(world.Boss.MovementType, BossMovementType.Patrol);
        for (int i = 0; i < 90; i++) world.Step(Dt, Idle());
        True(world.Bullets.Any(b => b.IsActive && b.Faction == Faction.Boss), "and then the attack begins");
    }

    private static void ScoreGrowsWithHitsAndPhases()
    {
        var world = new World(practicePhase: 1);
        world.Boss.Transform.Position = new Vector2(300, Layout.BossY); // she has come down from above
        Eq(world.Score, 0);
        world.Bullets[0] = new BulletData { IsActive = true, Position = world.Boss.Transform.Position, Faction = Faction.Player, HitboxRadius = 5, Damage = 1, Style = BulletStyle.LightBolt };
        world.Step(Dt, Idle());
        Eq(world.Score, ScoreSystem.PointsPerHit);

        world.Boss.Health.CurrentHealth = 0;
        world.Step(Dt, Idle());
        Eq(world.Score, ScoreSystem.PointsPerHit + ScoreSystem.PointsPerPhase);

        world.Reset();
        Eq(world.Score, 0);
    }

    private static void HighScoreOnlyKeepsTheBest()
    {
        var saved = HighScore.Current;
        try
        {
            HighScore.Current = 1000;
            False(HighScore.Submit(500));
            Eq(HighScore.Current, 1000);
            True(HighScore.Submit(2500));
            Eq(HighScore.Current, 2500);
        }
        finally { HighScore.Current = saved; }
    }

    private static void ActorsReachTheMiddleOfTheTreesButNoFurther()
    {
        Eq(Layout.SceneX(0), Layout.BankWidth / 2);
        Eq(Layout.SceneX(Layout.PlayWidth), Layout.RiverWidth - Layout.BankWidth / 2);
        var player = new PlayerEntity();
        player.Movement.Speed = 400;
        player.Input.Direction = V(1, 0);
        MovementSystem.MovePlayer(player, World.PlayArea, 10, Vector2.Zero);
        Eq(player.Transform.Position.X, 600f);
    }

    private static void BossHitboxIsTwoThirdsOfItsSprite()
    {
        var definition = IrysshBoss.Definition;
        foreach (var phase in definition.Phases)
            Near(phase.HitboxRadius * 2 / BossArt.SpriteSize(phase.Form).X, 2f / 3f, 0.02f);
        var boss = new BossEntity(definition);
        foreach (int lives in new[] { 6, 2, 1 })
        {
            PhaseManagerSystem.ChangePhase(boss, lives);
            Eq(boss.Hitbox.Radius, definition.PhaseFor(lives).HitboxRadius);
        }
    }

    private static void BossPatrolKeepsItsHitboxInsideTheField()
    {
        var boss = new BossEntity(IrysshBoss.Definition);
        PhaseManagerSystem.ChangePhase(boss, 6);
        boss.Movement.Speed = 200;
        boss.MovementType = BossMovementType.Patrol;
        boss.Transform.Position = V(300, 100);
        float lowest = 1000, highest = -1000;
        for (int i = 0; i < 60 * 20; i++)
        {
            MovementSystem.MoveBoss(boss, World.PlayArea, 1f / 60);
            lowest = MathF.Min(lowest, boss.Transform.Position.X);
            highest = MathF.Max(highest, boss.Transform.Position.X);
        }
        GreaterEq(lowest, boss.Hitbox.Radius - 4);
        LessEq(highest, World.PlayArea.End.X - boss.Hitbox.Radius + 4);
        Greater(highest - lowest, 300, "it still patrols across the river");
    }

    private static void DashLanesFitTheSerpent()
    {
        float r = IrysshBoss.Definition.PhaseFor(1).HitboxRadius;
        foreach (float lane in new float[] { 100, 300, 500 })
        {
            GreaterEq(lane, r);
            LessEq(lane, World.PlayArea.End.X - r);
        }
    }

    private static void BossChangesFormAtPhaseFive()
    {
        var boss = new BossEntity(IrysshBoss.Definition);
        boss.Health.Lives = 4; Eq(boss.Form, BossForm.Human);
        boss.Health.Lives = 3; Eq(boss.Form, BossForm.Human);
        boss.Health.Lives = 2; Eq(boss.Form, BossForm.Serpent);
        boss.Health.Lives = 1; Eq(boss.Form, BossForm.Serpent);
    }

    private static void PracticeStartsAtTheChosenAttack()
    {
        for (int phase = 1; phase <= 6; phase++)
        {
            var world = new World(practicePhase: phase);
            Eq(world.Boss.Health.Lives, 7 - phase);
            Eq(world.Boss.Form, phase <= 4 ? BossForm.Human : BossForm.Serpent);
            True(world.Boss.EmitterMain.IsActive);
        }
        Eq(new World(practicePhase: 99).Boss.Health.Lives, 1, "the phase is kept in range");
        Eq(new World(withCards: false).Boss.Health.Lives, 6, "a normal run is unchanged");
    }

    private static void PracticeEndsInVictoryWhenThatAttackIsBeaten()
    {
        var world = new World(practicePhase: 3);
        world.Boss.Health.CurrentHealth = 0;
        world.Step(Dt, Idle());
        Eq(world.Result, (GameResult?)GameResult.Victory);

        var normal = new World(withCards: false);
        normal.SkipToBoss();
        normal.Boss.Health.CurrentHealth = 0;
        normal.Step(Dt, Idle());
        True(normal.Result == null, "a normal run goes on to the next attack");
    }

    private static void TheTableKeepsTheBestFiveInOrder()
    {
        var saved = HighScore.Entries;
        try
        {
            HighScore.Entries = new List<ScoreEntry>();
            foreach (int s in new[] { 500, 9000, 100, 7000, 300, 8000, 200 }) HighScore.Submit(s);
            True(HighScore.Entries.Select(e => e.Score).SequenceEqual(new[] { 9000, 8000, 7000, 500, 300 }));
            Eq(HighScore.Current, 9000);
        }
        finally { HighScore.Entries = saved; }
    }

    private static void SubmitReportsANewBestOnly()
    {
        var saved = HighScore.Entries;
        try
        {
            HighScore.Entries = new List<ScoreEntry>();
            True(HighScore.Submit(1000));
            False(HighScore.Submit(800), "it makes the table but is not the best");
            True(HighScore.Submit(2000, cleared: true));
            Eq(HighScore.Entries[0], new ScoreEntry(2000, true));
            False(HighScore.Submit(0), "an empty run is not recorded");
        }
        finally { HighScore.Entries = saved; }
    }

    private static void TheThreeKindsGetSteadilyTougher()
    {
        Eq(SpiritKind.Swarm.MaxHealth(), 1, "a blue one falls to a single shot");
        Less(SpiritKind.Swarm.MaxHealth(), SpiritKind.Gunner.MaxHealth());
        Less(SpiritKind.Gunner.MaxHealth(), SpiritKind.Heavy.MaxHealth());
        Less(SpiritKind.Swarm.Points(), SpiritKind.Gunner.Points());
        Less(SpiritKind.Gunner.Points(), SpiritKind.Heavy.Points());
        Greater(SpiritKind.Heavy.MaxHealth(), 60, "a red fairy takes a while to bring down");
    }

    // Spirits

    private static EnemyEntity Enemy(EnemyPath path, SpiritKind kind = SpiritKind.Swarm, Vector2? at = null, List<ScheduledPattern> patterns = null, float delay = 0)
        => new EnemyEntity(1, kind, at ?? V(300, -30), path, 0, patterns, delay);
}
