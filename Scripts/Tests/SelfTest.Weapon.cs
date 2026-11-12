using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static partial class SelfTest
{
    private static void RegisterWeapon(Action<string, Action> T)
    {
        T("TheHomingAttackFiresRoundLightsThatHitForHalf", TheHomingAttackFiresRoundLightsThatHitForHalf);
        T("TheHomingLightsHitFoesThatMoveFast", TheHomingLightsHitFoesThatMoveFast);
        T("TheHomingAttackGivesAThirdOfTheDamageOfTheStream", TheHomingAttackGivesAThirdOfTheDamageOfTheStream);
        T("TheHomingAttackFiresAtHalfTheCadenceOfTheStream", TheHomingAttackFiresAtHalfTheCadenceOfTheStream);
        T("TheLightsBendToTheFoeThatWasHitLast", TheLightsBendToTheFoeThatWasHitLast);
        T("TheHomingLightsFindAFoeThatTheStreamMisses", TheHomingLightsFindAFoeThatTheStreamMisses);
        T("TheStreamIsStillTheDefaultAttack", TheStreamIsStillTheDefaultAttack);
    }

    private static int LightsOf(BulletData[] pool, BulletStyle style) => pool.Count(b => b.IsActive && b.Style == style);

    private static void TheHomingAttackFiresRoundLightsThatHitForHalf()
    {
        foreach (int level in new[] { 1, 2, 3 })
        {
            var pool = Pool(100); var p = new PlayerEntity(); p.Input.IsShooting = true; p.Weapon.PowerLevel = level; p.Weapon.TimeSinceLastFire = 1;
            BulletSystem.UpdatePlayerWeapons(p, pool, 0.1f, new CardEffects(), WeaponStyle.Homing);
            Eq(LightsOf(pool, BulletStyle.HomingOrb), level, $"level {level}: as many lights as the stream has bolts");
            Eq(LightsOf(pool, BulletStyle.LightBolt), 0, "and no bolts");
            True(pool.Where(b => b.IsActive).All(b => b.Behavior == BulletBehavior.Amulet && b.Faction == Faction.Player), "they bend to the foe, like the amulets of Reimu");
            True(pool.Where(b => b.IsActive).All(b => MathF.Abs(b.Power - BulletSystem.HomingPower) < 0.001f), "each hits for two thirds of a bolt (at half the cadence: a third of the damage of the stream)");
            if (level >= 2) True(pool.Where(b => b.IsActive).Any(b => b.Velocity.X < 0) && pool.Any(b => b.IsActive && b.Velocity.X > 0), "one along each diagonal");
        }
        Near(BulletSystem.HomingPower, 2f / 3f, 0.0001f);
    }

    private static void TheHomingAttackFiresAtHalfTheCadenceOfTheStream()
    {
        int Fired(WeaponStyle weapon)
        {
            var pool = Pool(300); var p = new PlayerEntity(); p.Input.IsShooting = true; p.Weapon.PowerLevel = 2;
            int count = 0;
            for (int i = 0; i < 60 * 4; i++)
            {
                BulletSystem.UpdatePlayerWeapons(p, pool, Dt, new CardEffects(), weapon);
                for (int k = 0; k < pool.Length; k++) if (pool[k].IsActive && pool[k].Fresh) { count++; pool[k].Fresh = false; }
                for (int k = 0; k < pool.Length; k++) pool[k].IsActive = false;
            }
            return count;
        }
        int stream = Fired(WeaponStyle.Stream), homing = Fired(WeaponStyle.Homing);
        Near(homing / (float)stream, 0.5f, 0.06f, $"half as many lights as bolts in the same time ({homing} against {stream})");
    }

    private static void TheLightsBendToTheFoeThatWasHitLast()
    {
        var pool = Pool(20);
        var p = new PlayerEntity(); p.Transform.Position = V(300, 600);
        BulletSystem.Spawn(pool, V(300, 600), new Vector2(0, -BulletSystem.AmuletSpeed), Faction.Player, BulletStyle.HomingOrb, behavior: BulletBehavior.Amulet);
        var near = V(300, 300); var hit = V(450, 330);
        var field = new Rect2(-2000, -2000, 5000, 5000);
        for (int i = 0; i < 20; i++) BulletSystem.MoveBullets(pool, field, 1 / 60f, Vector2.Zero, p, new List<Vector2> { near, hit }, hit);
        var b = pool.First(x => x.IsActive);
        Greater(b.Velocity.X, 100, "it turns to the foe that was hit last, not to the nearest");
        // when the time to bend is over it only keeps its heading
        for (int i = 0; i < (int)(60 * BulletSystem.AmuletSteerSeconds) + 20; i++) BulletSystem.MoveBullets(pool, field, 1 / 60f, Vector2.Zero, p, new List<Vector2> { V(-1000, -1000) }, null);
        var before = pool.First(x => x.IsActive).Velocity;
        for (int i = 0; i < 10; i++) BulletSystem.MoveBullets(pool, field, 1 / 60f, Vector2.Zero, p, new List<Vector2> { V(0, 0) }, null);
        Near(pool.First(x => x.IsActive).Velocity.X, before.X, 0.01f, "no more turning");
        Near(pool.First(x => x.IsActive).Velocity.Length(), BulletSystem.AmuletSpeed, 1f, "at its cruising speed");
    }

    private static void TheHomingLightsFindAFoeThatTheStreamMisses()
    {
        int HealthAfter(WeaponStyle weapon)
        {
            var world = new World(withCards: false, seed: 5, weapon: weapon);
            world.CalmWindForTesting();
            world.HoldWavesForTesting();
            var foe = new EnemyEntity(900, SpiritKind.Heavy, V(520, 260), EnemyPath.Straight(Vector2.Zero));
            world.Enemies.Add(foe);
            world.Player.Transform.Position = V(300, 600);
            for (int i = 0; i < 60 * 4; i++)
            {
                world.Player.Health.InvincibilityTimer = 5;
                world.Step(Dt, new PlayerInputComponent { IsShooting = true });
            }
            return foe.Health;
        }
        int max = SpiritKind.Heavy.MaxHealth();
        Eq(HealthAfter(WeaponStyle.Stream), max, "the straight bolts miss a foe off to the side");
        Less(HealthAfter(WeaponStyle.Homing), max, "the lights turn to it and hit");
    }

    private static void TheStreamIsStillTheDefaultAttack()
    {
        var world = new World(withCards: false);
        Eq(world.Weapon, WeaponStyle.Stream);
        var pool = Pool(100); var p = new PlayerEntity(); p.Input.IsShooting = true; p.Weapon.TimeSinceLastFire = 1;
        BulletSystem.UpdatePlayerWeapons(p, pool, 0.1f, new CardEffects());
        Eq(LightsOf(pool, BulletStyle.LightBolt), 1);
        Eq(LightsOf(pool, BulletStyle.HomingOrb), 0);
    }

    private static void TheHomingLightsHitFoesThatMoveFast()
    {
        // three foes that sway across the field at up to 500 px/s: nearly every light finds one
        var w = new World(withCards: false, seed: 3, weapon: WeaponStyle.Homing); w.CalmWindForTesting(); w.HoldWavesForTesting();
        var foes = new List<EnemyEntity>();
        for (int i = 0; i < 3; i++) { var f = new EnemyEntity(900 + i, SpiritKind.Heavy, V(100 + i * 200, 150 + i * 70), EnemyPath.Straight(Vector2.Zero)); foes.Add(f); w.Enemies.Add(f); }
        w.Player.Weapon.PowerLevel = 3; w.Player.Energy = PowerCurve.Level3; w.Player.Transform.Position = V(300, 600);
        double dealt = 0;
        for (int t = 0; t < 60 * 8; t++)
        {
            w.Player.Health.InvincibilityTimer = 5;
            for (int i = 0; i < foes.Count; i++) { foes[i].Position = V(300 + (i - 1) * 40 + 220 * MathF.Sin(t / 60f * 500f / 220f + i * 2), foes[i].Position.Y); foes[i].Health = 100000; }
            w.Step(Dt, new PlayerInputComponent { IsShooting = true });
            dealt += foes.Sum(f => 100000 - f.Health);
        }
        int fired = (w.Player.Weapon.Volleys + 1) / 2 * 3;
        Greater((float)(dealt / (fired * BulletSystem.HomingPower)), 0.8f, "at least four lights in five hit, though the foes move fast");
    }

    private static void TheHomingAttackGivesAThirdOfTheDamageOfTheStream()
    {
        float PerSecond(WeaponStyle weapon, int level)
        {
            var pool = Pool(300); var p = new PlayerEntity(); p.Input.IsShooting = true; p.Weapon.PowerLevel = level;
            float total = 0;
            for (int i = 0; i < 60 * 4; i++)
            {
                BulletSystem.UpdatePlayerWeapons(p, pool, Dt, new CardEffects(), weapon);
                for (int k = 0; k < pool.Length; k++) if (pool[k].IsActive && pool[k].Fresh) { total += pool[k].Damage * pool[k].Power; pool[k].Fresh = false; pool[k].IsActive = false; }
            }
            return total / 4;
        }
        foreach (int level in new[] { 1, 2, 3 })
            Near(PerSecond(WeaponStyle.Homing, level) / PerSecond(WeaponStyle.Stream, level), 1f / 3f, 0.07f, $"level {level}");
    }
}
