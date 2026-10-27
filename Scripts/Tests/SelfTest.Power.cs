using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of the lantern's energy, the prizes and the wind.</summary>
public static partial class SelfTest
{
    private static void RegisterPower(Action<string, Action> T)
    {
        T("TheLightOfAny70PercentOfTheSpiritsReachesLevelThree", TheLightOfAny70PercentOfTheSpiritsReachesLevelThree);
        T("CollectingMuchLessIsNotEnough", CollectingMuchLessIsNotEnough);
        T("TheStageHasEnoughSpiritsToGoAroundWithRoomToSpare", TheStageHasEnoughSpiritsToGoAroundWithRoomToSpare);
        T("ProgressRisesWithinEachLevel", ProgressRisesWithinEachLevel);
        T("ARunStartsAtLevelOneAndPracticeAtFullPower", ARunStartsAtLevelOneAndPracticeAtFullPower);
        T("TheWeaponGetsStrongerWithTheLevel", TheWeaponGetsStrongerWithTheLevel);
        T("DefeatedSpiritsLetGoOfLights", DefeatedSpiritsLetGoOfLights);
        T("LightsFallAndAreLostIfNotCaught", LightsFallAndAreLostIfNotCaught);
        T("TheLanternPullsNearbyLightsInAndGathersThem", TheLanternPullsNearbyLightsInAndGathersThem);
        T("GatheringLightRaisesTheLevelAndTellsAiru", GatheringLightRaisesTheLevelAndTellsAiru);
        T("SurplusLightIsWorthPoints", SurplusLightIsWorthPoints);
        T("DyingLosesPartOfTheEnergyAndDropsItWhereTheBoatWasLost", DyingLosesPartOfTheEnergyAndDropsItWhereTheBoatWasLost);
        T("TheLostLevelCanBeWonBackByGatheringTheFallenLight", TheLostLevelCanBeWonBackByGatheringTheFallenLight);
        T("LightLeftToFallAwayIsLostForGood", LightLeftToFallAwayIsLostForGood);
        T("DyingWithNoEnergyDropsNothing", DyingWithNoEnergyDropsNothing);
        T("TheLastDeathEndsTheGameWithoutDroppingAnything", TheLastDeathEndsTheGameWithoutDroppingAnything);
    }

    private static void TheLightOfAny70PercentOfTheSpiritsReachesLevelThree()
    {
        var drops = PowerCurve.DropsInStage;
        int seventy = (int)(drops.Count * PowerCurve.Level3Share);
        var rng = new Random(7);
        for (int i = 0; i < 200; i++)
        {
            float energy = drops.OrderBy(_ => rng.Next()).Take(seventy).Sum();
            GreaterEq(energy, PowerCurve.Level3);
        }
        GreaterEq(drops.OrderBy(d => d).Take(seventy).Sum(), PowerCurve.Level3);
    }

    private static void CollectingMuchLessIsNotEnough()
    {
        var drops = PowerCurve.DropsInStage.OrderBy(d => d).ToList();
        int sixty = (int)(drops.Count * (PowerCurve.Level3Share - 0.1));
        Less(drops.Take(sixty).Sum(), PowerCurve.Level3, "the weakest 35% do not make level 3");
        Greater(PowerCurve.Level3, drops.Count * 0.4f);
    }

    private static void TheStageHasEnoughSpiritsToGoAroundWithRoomToSpare()
    {
        int total = PowerCurve.DropsInStage.Sum();
        Greater(total, PowerCurve.Level3 * 1.4f, "perfect play has a surplus");
    }

    private static void ProgressRisesWithinEachLevel()
    {
        Eq(PowerCurve.Progress(0), 0f);
        Near(PowerCurve.Progress(PowerCurve.Level2), 0, 0.001f);
        Eq(PowerCurve.Progress(PowerCurve.Level3), 1f);
        float mid = PowerCurve.Progress((PowerCurve.Level2 + PowerCurve.Level3) / 2);
        Near(mid, 0.5f, 0.05f);
    }

    private static void ARunStartsAtLevelOneAndPracticeAtFullPower()
    {
        var run = new World(withCards: false);
        Eq(run.Player.Energy, 0f);
        Eq(run.Player.Weapon.PowerLevel, 1);
        var practice = new World(practicePhase: 3);
        Eq(practice.Player.Energy, PowerCurve.Level3);
        Eq(practice.Player.Weapon.PowerLevel, 3);
    }

    private static void TheWeaponGetsStrongerWithTheLevel()
    {
        int Shots(int level)
        {
            var player = new PlayerEntity();
            player.Weapon = new PlayerWeaponComponent { PowerLevel = level, FireRate = 0.1f, TimeSinceLastFire = 1, BulletSpeed = 800 };
            player.Input.IsShooting = true;
            var pool = Pool(20);
            BulletSystem.UpdatePlayerWeapons(player, pool, 0.1f);
            return ActiveCount(pool);
        }
        True(new[] { 1, 2, 3 }.Select(Shots).SequenceEqual(new[] { 1, 2, 3 }));
    }

    private static void Defeat(SpiritKind kind, World world, Vector2 position)
    {
        var spirit = new EnemyEntity(900, kind, position, EnemyPath.Straight(Vector2.Zero));
        world.AddForTesting(spirit);
        Place(world, new BulletData { IsActive = true, Position = position, Faction = Faction.Player, HitboxRadius = 5, Damage = 999, Style = BulletStyle.LightBolt });
        world.Step(Dt, Idle());
    }

    private static void DefeatedSpiritsLetGoOfLights()
    {
        foreach (var kind in Enum.GetValues<SpiritKind>())
        {
            var world = new World(withCards: false);
            world.StartWave(0);
            Defeat(kind, world, V(300, 200));
            Eq(world.Lights.Count(l => l.IsActive), kind.Drops(), kind.ToString());
        }
        Less(SpiritKind.Swarm.Drops(), SpiritKind.Gunner.Drops());
        Less(SpiritKind.Gunner.Drops(), SpiritKind.Heavy.Drops());
    }

    private static void LightsFallAndAreLostIfNotCaught()
    {
        var items = LightSystem.NewPool(5);
        LightSystem.Spawn(items, V(100, 100), 1);
        float maxY = 100;
        for (int i = 0; i < 60 * 3; i++)
        {
            LightSystem.Update(items, V(550, 750), World.PlayArea, 1f / 60);
            if (items[0].IsActive) maxY = MathF.Max(maxY, items[0].Position.Y);
        }
        Greater(maxY, 300, "it falls");
        True(items[0].IsActive, "it still has a way to go after three seconds");
        for (int i = 0; i < 60 * 10; i++) LightSystem.Update(items, V(550, 750), World.PlayArea, 1f / 60);
        False(items[0].IsActive, "and in the end it drifts out of the picture");
    }

    private static void TheLanternPullsNearbyLightsInAndGathersThem()
    {
        var items = LightSystem.NewPool(5);
        items[0] = new LightItem { IsActive = true, Position = V(300, 640), Velocity = Vector2.Zero, Value = 1, Age = 1 };
        float gathered = 0;
        for (int i = 0; i < 60; i++) gathered += LightSystem.Update(items, V(300, 700), World.PlayArea, 1f / 60);
        Eq(gathered, 1f);
        False(items[0].IsActive);

        // Far away it is not pulled.
        items[1] = new LightItem { IsActive = true, Position = V(100, 300), Velocity = Vector2.Zero, Value = 1, Age = 1 };
        LightSystem.Update(items, V(500, 700), World.PlayArea, 1f / 60);
        Near(items[1].Position.X, 100, 3);
    }

    private static void GatheringLightRaisesTheLevelAndTellsAiru()
    {
        var world = new World(withCards: false);
        var seen = new List<int>();
        foreach (float target in new[] { PowerCurve.Level2, PowerCurve.Level3 })
        {
            while (world.Player.Energy < target)
            {
                world.Step(Dt, Idle());
                // A light right on the boat.
                LightSystem.Spawn(world.Lights, world.Player.Transform.Position, 1, 0);
                world.Step(Dt, Idle());
                foreach (var ev in world.TakeEvents()) if (ev.Kind == WorldEventKind.PowerUp) seen.Add(ev.Int);
            }
        }
        True(seen.SequenceEqual(new[] { 2, 3 }), string.Join(",", seen));
        Eq(world.Player.Weapon.PowerLevel, 3);
        Eq(world.Player.Energy, PowerCurve.Level3, "the lantern holds no more");
    }

    private static void SurplusLightIsWorthPoints()
    {
        var world = new World(practicePhase: 1); // full power already
        LightSystem.Spawn(world.Lights, world.Player.Transform.Position, 3, 0);
        int before = world.Score;
        world.Step(Dt, Idle());
        Eq(world.Score - before, 3 * ScoreSystem.PointsPerSpareLight);
        Eq(world.Player.Energy, PowerCurve.Level3);
    }

    private static void Die(World world, Vector2 place)
    {
        world.Player.Transform.Position = place;
        world.Player.Health.InvincibilityTimer = 0;
        world.Player.Health.CurrentHealth = 0;
        world.Step(Dt, Idle());
    }

    private static void DyingLosesPartOfTheEnergyAndDropsItWhereTheBoatWasLost()
    {
        var world = new World(practicePhase: 1);
        float before = world.Player.Energy;
        Die(world, V(220, 480));
        Eq(world.Player.Health.Lives, 2);

        float lost = before - world.Player.Energy;
        Eq(lost, MathF.Round(before * PowerCurve.LossOnDeath, MidpointRounding.AwayFromZero));
        Greater(lost, 0);
        Less(world.Player.Energy, before);

        var dropped = world.Lights.Where(l => l.IsActive).ToList();
        GreaterEq(dropped.Count, 3);
        Near(dropped.Sum(l => l.Value), lost, 0.01f, "all of it falls");
        foreach (var light in dropped)
        {
            Near(light.Position.X, 220, 40);
            Near(light.Position.Y, 480, 40);
        }
        Eq(world.Player.Transform.Position, CombatSystem.PlayerSpawn, "the boat restarts at the bottom");
    }

    private static void TheLostLevelCanBeWonBackByGatheringTheFallenLight()
    {
        var world = new World(practicePhase: 1);
        world.CalmWindForTesting();
        Die(world, V(300, 560));
        Less(world.Player.Energy, PowerCurve.Level3);
        Eq(world.Player.Weapon.PowerLevel, 2, "one level lost");

        var input = Idle();
        for (int i = 0; i < 60 * 6; i++)
        {
            world.Player.Health.InvincibilityTimer = 10;
            var active = world.Lights.Where(l => l.IsActive).Select(l => l.Position).ToList();
            if (active.Count > 0)
            {
                var target = active.OrderBy(p => p.DistanceTo(world.Player.Transform.Position)).First();
                input.Direction = Mathx.Norm(target - world.Player.Transform.Position);
            }
            else input.Direction = Vector2.Zero;
            world.Step(Dt, input);
        }
        Near(world.Player.Energy, PowerCurve.Level3, 0.5f);
        Eq(world.Player.Weapon.PowerLevel, 3);
    }

    private static void LightLeftToFallAwayIsLostForGood()
    {
        var world = new World(practicePhase: 1);
        Die(world, V(300, 300));
        float after = world.Player.Energy;
        for (int i = 0; i < 60 * 12; i++)
        {
            world.Player.Health.InvincibilityTimer = 10;
            world.Player.Transform.Position = V(560, 60);
            world.Step(Dt, Idle());
        }
        True(world.Lights.All(l => !l.IsActive));
        Near(world.Player.Energy, after, 0.01f, "it was never gathered");
    }

    private static void DyingWithNoEnergyDropsNothing()
    {
        var world = new World(withCards: false);
        Die(world, V(300, 400));
        True(world.Lights.All(l => !l.IsActive));
        Eq(world.Player.Weapon.PowerLevel, 1);
    }

    private static void TheLastDeathEndsTheGameWithoutDroppingAnything()
    {
        var world = new World(practicePhase: 1);
        world.Player.Health.Lives = 1;
        Die(world, V(300, 400));
        Eq(world.Result, (GameResult?)GameResult.Defeat);
    }

    // The bomb

    private static World BombedWorld(params (SpiritKind kind, Vector2 pos)[] spirits)
    {
        var world = new World(withCards: false);
        world.HoldWavesForTesting();
        world.CalmWindForTesting();
        for (int i = 0; i < spirits.Length; i++)
            world.AddForTesting(new EnemyEntity(800 + i, spirits[i].kind, spirits[i].pos, EnemyPath.Straight(Vector2.Zero)));
        var input = Idle();
        input.IsBombing = true;
        world.Step(Dt, input);
        return world;
    }
}
