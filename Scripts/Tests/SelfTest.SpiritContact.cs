using System;
using System.Linq;
using Godot;

/// <summary>Tests of spirits touching, shooting and being shot.</summary>
public static partial class SelfTest
{
    private static void RegisterSpiritContact(Action<string, Action> T)
    {
        T("TouchingASpiritHurtsThePlayer", TouchingASpiritHurtsThePlayer);
        T("NearMissesAreHarmless", NearMissesAreHarmless);
        T("AfterBeingHurtThereIsAGracePeriodAndTheBombProtects", AfterBeingHurtThereIsAGracePeriodAndTheBombProtects);
        T("ASingleShotKillsABlueSpiritButNotTheOthers", ASingleShotKillsABlueSpiritButNotTheOthers);
        T("TheLanternReachesFartherForLights", TheLanternReachesFartherForLights);
        T("ASpareLanternComesAtTheFirstScoreAndRefillsABomb", ASpareLanternComesAtTheFirstScoreAndRefillsABomb);
        T("ASpareBoatComesAtAHighScoreAndMendsALife", ASpareBoatComesAtAHighScoreAndMendsALife);
        T("APrizeThatIsMissedComesRoundAgain", APrizeThatIsMissedComesRoundAgain);
        T("ASpareBoatWithFullLivesIsWorthPoints", ASpareBoatWithFullLivesIsWorthPoints);
        T("ThePrizesGiveAboutTwoLanternsAndABoatInAGoodRun", ThePrizesGiveAboutTwoLanternsAndABoatInAGoodRun);
        T("TheWindBlowsHarderTheHigherTheDifficulty", TheWindBlowsHarderTheHigherTheDifficulty);
        T("TheLevelOfTheWeaponDoesNotMultiplyTheFan", TheLevelOfTheWeaponDoesNotMultiplyTheFan);
        T("TheBurstDoesNotGrowWithTheLevelEither", TheBurstDoesNotGrowWithTheLevelEither);
        T("ThePowerLevelsComeEarlierThanBefore", ThePowerLevelsComeEarlierThanBefore);
    }

    private static void TouchingASpiritHurtsThePlayer()
    {
        foreach (var kind in Enum.GetValues<SpiritKind>())
        {
            var (world, _) = WorldWithSpiritAt(V(0, -4), kind);
            int lives = world.Player.Health.Lives;
            world.Step(Dt, Idle());
            Eq(world.Player.Health.Lives, lives - 1, $"{kind} hurts on contact");
            Eq(world.Player.Transform.Position, CombatSystem.PlayerSpawn, "and the boat is put back at its start");
        }
    }

    private static void NearMissesAreHarmless()
    {
        var (world, _) = WorldWithSpiritAt(V(0, -60));
        int lives = world.Player.Health.Lives;
        StepN(world, 30);
        Eq(world.Player.Health.Lives, lives);
    }

    private static void AfterBeingHurtThereIsAGracePeriodAndTheBombProtects()
    {
        var (world, _) = WorldWithSpiritAt(V(0, -4));
        world.Step(Dt, Idle());
        int lives = world.Player.Health.Lives;
        StepN(world, 60);
        Eq(world.Player.Health.Lives, lives);

        var (bombed, _) = WorldWithSpiritAt(V(0, -4));
        var input = Idle();
        input.IsBombing = true;
        bombed.Step(Dt, input);
        Eq(bombed.Player.Health.Lives, 3, "the lantern's light shields the boat");
    }

    private static void ASingleShotKillsABlueSpiritButNotTheOthers()
    {
        foreach (var kind in Enum.GetValues<SpiritKind>())
        {
            var world = new World(withCards: false);
            world.HoldWavesForTesting();
            var spirit = new EnemyEntity(300, kind, V(300, 300), EnemyPath.Straight(Vector2.Zero));
            world.AddForTesting(spirit);
            BulletSystem.Spawn(world.Bullets, spirit.Position, Vector2.Zero, Faction.Player, BulletStyle.LightBolt);
            world.Step(Dt, Idle());
            Eq(world.Enemies.Count, kind == SpiritKind.Swarm ? 0 : 1, $"{kind} after one plain shot");
        }
    }

    private static void TheLanternReachesFartherForLights()
    {
        Greater(LightSystem.MagnetRadius, 120, "a wide pull");
        var items = LightSystem.NewPool(3);
        items[0] = new LightItem { IsActive = true, Position = V(300, 700 - 120), Velocity = Vector2.Zero, Value = 1, Age = 1 };
        float gathered = 0;
        for (int i = 0; i < 60; i++) gathered += LightSystem.Update(items, V(300, 700), World.PlayArea, 1f / 60);
        Eq(gathered, 1f, "a light 120 away is still pulled in");
    }

    private static void TakeThePrize(World world, LightKind kind)
    {
        var prize = world.Lights.First(l => l.IsActive && l.Kind == kind);
        world.Player.Transform.Position = prize.Position;
        world.TakeEvents();
        for (int i = 0; i < 20; i++) { world.Player.Health.InvincibilityTimer = 10; world.Step(Dt, Idle()); }
    }

    private static void ASpareLanternComesAtTheFirstScoreAndRefillsABomb()
    {
        var world = new World(withCards: false);
        world.HoldWavesForTesting();
        world.CalmWindForTesting();
        world.Player.Bomb.BombsLeft = 1;
        world.AddScoreForTesting(ScoreSystem.FirstBombScore - 1);
        False(world.Lights.Any(l => l.IsActive && l.Kind == LightKind.Bomb), "not yet");
        world.AddScoreForTesting(1);
        True(world.Lights.Any(l => l.IsActive && l.Kind == LightKind.Bomb), "the score reached it");
        TakeThePrize(world, LightKind.Bomb);
        Eq(world.Player.Bomb.BombsLeft, 2);
        True(world.TakeEvents().Any(e => e.Kind == WorldEventKind.BombRefilled));
        world.AddScoreForTesting(ScoreSystem.SecondBombScore - world.Score);
        Eq(world.Lights.Count(l => l.IsActive && l.Kind == LightKind.Bomb), 1, "and a second one at the second score");
    }

    private static void ASpareBoatComesAtAHighScoreAndMendsALife()
    {
        var world = new World(withCards: false);
        world.HoldWavesForTesting();
        world.CalmWindForTesting();
        world.Player.Health.Lives = 2;
        world.AddScoreForTesting(ScoreSystem.FirstLifeScore - 1);
        False(world.Lights.Any(l => l.IsActive && l.Kind == LightKind.Life), "not yet");
        world.AddScoreForTesting(1);
        True(world.Lights.Any(l => l.IsActive && l.Kind == LightKind.Life));
        TakeThePrize(world, LightKind.Life);
        Eq(world.Player.Health.Lives, 3);
        True(world.TakeEvents().Any(e => e.Kind == WorldEventKind.LifeRefilled));
    }

    private static void APrizeThatIsMissedComesRoundAgain()
    {
        var items = LightSystem.NewPool();
        LightSystem.SpawnReward(items, LightKind.Life, 100);
        var far = new Vector2(500, 700);
        for (int i = 0; i < 60 * 14; i++) LightSystem.Update(items, far, World.PlayArea, 1f / 60);
        True(items.Any(l => l.IsActive && l.Kind == LightKind.Life), "still there, high above");
        True(items.First(l => l.IsActive).Position.Y < World.PlayArea.End.Y);
    }

    private static void ASpareBoatWithFullLivesIsWorthPoints()
    {
        var world = new World(withCards: false);
        world.HoldWavesForTesting();
        LightSystem.SpawnReward(world.Lights, LightKind.Life, world.Player.Transform.Position.X, world.Player.Transform.Position.Y);
        int before = world.Score;
        for (int i = 0; i < 10; i++) world.Step(Dt, Idle());
        Eq(world.Player.Health.Lives, 3, "never more than three");
        Eq(world.Score - before, ScoreSystem.PointsPerSpareLife);
    }

    private static void ThePrizesGiveAboutTwoLanternsAndABoatInAGoodRun()
    {
        const int goodRunScore = 485_000;
        int bombs = 0, lives = 0;
        while (ScoreSystem.BombScore(bombs) <= goodRunScore) bombs++;
        while (ScoreSystem.LifeScore(lives) <= goodRunScore) lives++;
        Eq(bombs, 2); Eq(lives, 1);
    }

    private static void TheWindBlowsHarderTheHigherTheDifficulty()
    {
        Vector2 Drift(Difficulty d)
        {
            var w = new World(withCards: false, difficulty: d);
            w.HoldWavesForTesting();
            w.SetWindForTesting(Vector2.Right, 50);
            var before = w.Player.Transform.Position;
            w.Step(0.2f, Idle());
            return w.Player.Transform.Position - before;
        }
        float easy = Drift(Difficulty.Easy).X, normal = Drift(Difficulty.Normal).X, hard = Drift(Difficulty.Hard).X;
        Near(easy, 10, 0.01f, "the easy mode keeps the wind as it was");
        Greater(normal, easy); Greater(hard, normal);
    }

    private static void TheLevelOfTheWeaponDoesNotMultiplyTheFan()
    {
        int Side(int level, int pairs)
        {
            var pool = Pool(100);
            var p = new PlayerEntity();
            p.Input.IsShooting = true;
            p.Weapon.PowerLevel = level;
            p.Weapon.TimeSinceLastFire = 1;
            var run = new RunState();
            for (int i = 0; i < pairs; i++) run.Add(Card.TwinFan);
            BulletSystem.UpdatePlayerWeapons(p, pool, 0.1f, run.Effects);
            return pool.Count(b => b.IsActive && b.Velocity.X != 0);
        }
        Eq(Side(1, 1), 2); Eq(Side(2, 1), 2); Eq(Side(3, 1), 2);
        Eq(Side(1, 2), 4); Eq(Side(3, 2), 4);
        // and the fan's bolts are lighter than the main stream's
        var pool2 = Pool(100); var q = new PlayerEntity(); q.Input.IsShooting = true; q.Weapon.TimeSinceLastFire = 1;
        var one = new RunState(); one.Add(Card.TwinFan);
        BulletSystem.UpdatePlayerWeapons(q, pool2, 0.1f, one.Effects);
        Less(pool2.First(b => b.IsActive && b.Velocity.X != 0).Power, 1f);
    }

    private static void TheBurstDoesNotGrowWithTheLevelEither()
    {
        float Burst(int level)
        {
            var pool = Pool(100);
            var p = new PlayerEntity();
            p.Input.IsShooting = true;
            p.Weapon.PowerLevel = level;
            p.Weapon.TimeSinceLastFire = 1;
            var run = new RunState(); run.Add(Card.BurstingBolts);
            BulletSystem.UpdatePlayerWeapons(p, pool, 0.1f, run.Effects);
            return pool.Where(b => b.IsActive).Sum(b => b.BurstScale);
        }
        Near(Burst(1), 1, 0.001f); Near(Burst(2), 1, 0.001f); Near(Burst(3), 1, 0.001f);
    }

    private static void ThePowerLevelsComeEarlierThanBefore()
    {
        True(PowerCurve.Level3 < PowerCurve.DropsInStage.Sum() * 0.5f, "level 3 needs less than half of all the light of the stage");
    }

    // Difficulty

    private static int Volley(Difficulty d, SpiritKind kind, FirePattern pattern, float seconds = 1.1f)
    {
        var e = Enemy(EnemyPath.Straight(Vector2.Zero), kind, V(300, 200), Patterns(new ScheduledPattern(pattern, 100)));
        var pool = Pool(600);
        for (int i = 0; i < (int)(seconds * 60); i++)
        {
            EnemySystem.Move(e, 1f / 60);
            EnemySystem.Fire(e, V(300, 700), pool, World.PlayArea, 1f / 60, d);
        }
        return pool.Count(b => b.IsActive);
    }
}
