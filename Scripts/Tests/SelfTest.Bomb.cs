using System;
using System.Linq;
using Godot;

/// <summary>Tests of the bomb.</summary>
public static partial class SelfTest
{
    private static void RegisterBomb(Action<string, Action> T)
    {
        T("TheLightHurtsSpiritsInReachButNotFarOnes", TheLightHurtsSpiritsInReachButNotFarOnes);
        T("TheBombFinishesWeakSpiritsQuicklyAndHeavyOnesInAFewSeconds", TheBombFinishesWeakSpiritsQuicklyAndHeavyOnesInAFewSeconds);
        T("SpiritsKilledByTheBombScoreAndLeaveLights", SpiritsKilledByTheBombScoreAndLeaveLights);
        T("TheBombDestroysEveryBulletInReach", TheBombDestroysEveryBulletInReach);
        T("TheBombHurtsTheBossToo", TheBombHurtsTheBossToo);
        T("TheBombDoesNothingWhenItIsNotLit", TheBombDoesNothingWhenItIsNotLit);
        T("TheBombStillLastsFiveSecondsAndCostsOne", TheBombStillLastsFiveSecondsAndCostsOne);
    }

    private static void TheLightHurtsSpiritsInReachButNotFarOnes()
    {
        var world = BombedWorld((SpiritKind.Heavy, V(300, 560)), (SpiritKind.Heavy, V(300, 60)));
        Run(world, 0.5f);
        var near = world.Enemies.First(e => e.Position.Y > 300);
        var far = world.Enemies.First(e => e.Position.Y < 300);
        Less(near.Health, SpiritKind.Heavy.MaxHealth(), "the near one is burning");
        Eq(far.Health, SpiritKind.Heavy.MaxHealth(), "the far one is untouched");
    }

    private static void TheBombFinishesWeakSpiritsQuicklyAndHeavyOnesInAFewSeconds()
    {
        var world = BombedWorld((SpiritKind.Swarm, V(250, 600)), (SpiritKind.Gunner, V(350, 600)), (SpiritKind.Heavy, V(300, 520)));
        Run(world, 0.4f);
        False(world.Enemies.Any(e => e.Kind == SpiritKind.Swarm), "blue ones go at once");
        False(world.Enemies.Any(e => e.Kind == SpiritKind.Gunner), "so do green ones");
        var heavy = world.Enemies.FirstOrDefault(e => e.Kind == SpiritKind.Heavy);
        True(heavy != null, "but a red one holds out a moment");
        Less(heavy?.Health ?? 999, SpiritKind.Heavy.MaxHealth());
        Run(world, 3.5f);
        False(world.Enemies.Any(e => e.Kind == SpiritKind.Heavy), "and falls within the bomb's few seconds");
    }

    private static void SpiritsKilledByTheBombScoreAndLeaveLights()
    {
        var world = BombedWorld((SpiritKind.Gunner, V(300, 560)));
        world.TakeEvents();
        int before = world.Score;
        Run(world, 0.6f);
        Eq(world.Score - before, SpiritKind.Gunner.Points());
        Eq(world.Lights.Count(l => l.IsActive) + (int)world.Player.Energy, SpiritKind.Gunner.Drops(), "its lights drop (or were gathered)");
        True(world.TakeEvents().Any(e => e.Kind == WorldEventKind.EnemyDefeated && e.Spirit == SpiritKind.Gunner));
    }

    private static void TheBombDestroysEveryBulletInReach()
    {
        var world = BombedWorld();
        var p = world.Player.Transform.Position;
        for (int i = 0; i < 8; i++)
            Place(world, new BulletData { IsActive = true, Position = p + V(i * 25 - 90, -120), Velocity = Vector2.Zero, Faction = Faction.Boss, HitboxRadius = 5, Style = BulletStyle.OrbRed });
        Place(world, new BulletData { IsActive = true, Position = V(p.X, 20), Velocity = Vector2.Zero, Faction = Faction.Boss, HitboxRadius = 5, Style = BulletStyle.OrbRed });
        world.Step(Dt, Idle());
        Eq(world.Bullets.Count(b => b.IsActive && b.Faction == Faction.Boss), 1, "only the one outside the light survives");
    }

    private static void TheBombHurtsTheBossToo()
    {
        var world = new World(practicePhase: 1);
        world.CalmWindForTesting();
        world.Boss.Transform.Position = V(300, 540);
        world.Boss.MovementType = BossMovementType.Stationary;
        world.Boss.EmitterMain.IsActive = false;
        world.Boss.EmitterSub.IsActive = false;
        var input = Idle();
        input.IsBombing = true;
        world.Step(Dt, input);
        int start = world.Boss.Health.CurrentHealth;
        Run(world, 4.9f);
        int lost = start - world.Boss.Health.CurrentHealth;
        int expected = (int)(BombSystem.BossDamagePerSecond * 4.9f);
        Near(lost, expected, 3);
        Greater(lost, 40, "a big bite");
        Less(lost, world.Boss.Health.MaxHealth, "but it does not finish a bar by itself");
    }

    private static void TheBombDoesNothingWhenItIsNotLit()
    {
        var world = new World(withCards: false);
        world.StartWave(0);
        world.AddForTesting(new EnemyEntity(1, SpiritKind.Gunner, V(300, 560), EnemyPath.Straight(Vector2.Zero)));
        Run(world, 1);
        Eq(world.Enemies[0].Health, SpiritKind.Gunner.MaxHealth());
    }

    private static void TheBombStillLastsFiveSecondsAndCostsOne()
    {
        var world = BombedWorld();
        Eq(world.Player.Bomb.BombsLeft, 2);
        True(world.Player.Bomb.IsActive);
        Run(world, 4.8f);
        True(world.Player.Bomb.IsActive);
        Run(world, 0.4f);
        False(world.Player.Bomb.IsActive);
    }

    // Cards

    private static EnemyEntity Spirit(Vector2 p, int id = 500) => new EnemyEntity(id, SpiritKind.Heavy, p, EnemyPath.Straight(Vector2.Zero));
}
