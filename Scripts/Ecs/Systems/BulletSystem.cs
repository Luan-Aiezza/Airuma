using System;
using System.Collections.Generic;
using Godot;

/// <summary>Every bullet: the pool, the player's weapon and its cards, the boss's emitters, and how bullets move (acceleration, curves, wind, homing).</summary>
public static class BulletSystem
{
    public const int PoolSize = 1000;

    /// <summary>A pool of inactive bullets with their defaults filled in.</summary>
    public static BulletData[] NewPool(int count = PoolSize)
    {
        var pool = new BulletData[count];
        for (int i = 0; i < count; i++) pool[i] = new BulletData();
        return pool;
    }

    public static void Spawn(
        BulletData[] pool,
        Vector2 position,
        Vector2 velocity,
        Faction faction,
        BulletStyle style,
        float scale = 1,
        float acceleration = 0,
        float curve = 0,
        BulletBehavior behavior = BulletBehavior.Normal,
        float power = 1,
        int pierce = 0,
        float burstRadius = 0,
        float burstScale = 1)
    {
        int i = -1;
        for (int k = 0; k < pool.Length; k++) if (!pool[k].IsActive) { i = k; break; }
        if (i < 0) return;
        pool[i] = new BulletData
        {
            IsActive = true,
            Fresh = true,
            Position = position,
            Velocity = velocity,
            Faction = faction,
            HitboxRadius = 5 * scale, // the hitbox grows with the scale
            Damage = 1,
            WindModifier = faction == Faction.Player ? 1.0f : 0.5f,
            Acceleration = acceleration,
            CurveRate = curve,
            Scale = scale,
            Behavior = behavior,
            Timer = 0,
            Style = style,
            Power = power,
            Pierce = pierce,
            BurstRadius = burstRadius,
            BurstScale = burstScale,
        };
    }

    /// <summary>A bullet fired by a spirit (they use the same pool and hit the player like the boss's bullets).</summary>
    public static void SpawnEnemyBullet(BulletData[] pool, Vector2 position, Vector2 velocity, BulletStyle style)
    {
        Spawn(pool, position, velocity, Faction.Boss, style, scale: 0.9f);
    }

    /// <summary>Seconds between volleys of homing wisps.</summary>
    public const float WispInterval = 0.45f;
    public const float WispSpeed = 420;
    /// <summary>How fast a wisp can turn, in degrees per second.</summary>
    public const float WispTurnRate = 260;
    public const float WispPower = 0.8f;
    /// <summary>Angle between neighbouring bolts of a fan, in degrees.</summary>
    public const float FanAngle = 7;
    /// <summary>Strength of each bolt of a fan, against the 1 of a bolt of the main stream. The fan does not grow with the weapon's level.</summary>
    public const float FanPower = 0.4f;

    public static void UpdatePlayerWeapons(PlayerEntity player, BulletData[] pool, float delta, CardEffects? effectsOrNull = null)
    {
        CardEffects effects = effectsOrNull ?? new CardEffects();
        player.Weapon.TimeSinceLastFire += delta;
        player.Weapon.TimeSinceLastWisp += delta;
        if (effects.HomingWisps > 0 && player.Input.IsShooting && player.Weapon.TimeSinceLastWisp >= WispInterval)
        {
            player.Weapon.TimeSinceLastWisp = 0;
            for (int k = 0; k < effects.HomingWisps; k++)
            {
                // They leave the lantern, one to each side in turn.
                float side = k % 2 == 0 ? -1 : 1;
                Spawn(pool,
                      player.Transform.Position + new Vector2(side * 12, -4),
                      new Vector2(side * 120, -WispSpeed),
                      Faction.Player, BulletStyle.Wisp, behavior: BulletBehavior.Homing, power: WispPower);
            }
        }

        if (!(player.Input.IsShooting && player.Weapon.TimeSinceLastFire >= player.Weapon.FireRate)) return;

        player.Weapon.TimeSinceLastFire = 0;
        const float spacing = 15;

        Vector2[] offsets;
        switch (player.Weapon.PowerLevel)
        {
            case 1: offsets = new[] { Vector2.Zero }; break;
            case 2: offsets = new[] { new Vector2(-spacing, 0), new Vector2(spacing, 0) }; break;
            case 3: offsets = new[] { Vector2.Zero, new Vector2(-spacing, 0), new Vector2(spacing, 0) }; break;
            default: offsets = new[] { Vector2.Zero }; break;
        }

        // The main stream: one, two or three bolts, by the weapon's level. The cards' effects are the same whatever the level:
        // the bolts of a volley share one burst between them.
        foreach (Vector2 offset in offsets)
        {
            Spawn(pool,
                  player.Transform.Position + offset,
                  new Vector2(0, -player.Weapon.BulletSpeed),
                  Faction.Player,
                  BulletStyle.LightBolt,
                  power: effects.DamageMultiplier,
                  pierce: effects.Pierce,
                  burstRadius: effects.BurstRadius,
                  burstScale: 1f / offsets.Length);
        }

        // The fans: a pair of lighter bolts for each, from the lantern itself, one volley after another, however strong the weapon.
        for (int k = 1; k <= effects.FanPairs; k++)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                Spawn(pool,
                      player.Transform.Position,
                      new Vector2(0, -player.Weapon.BulletSpeed).Rotated(Mathx.Radians(side * k * FanAngle)),
                      Faction.Player,
                      BulletStyle.LightBolt,
                      power: FanPower * effects.DamageMultiplier,
                      pierce: effects.Pierce,
                      burstRadius: effects.BurstRadius,
                      burstScale: 1f);
            }
        }
    }

    public static void UpdateBossWeapons(BossEntity boss, PlayerEntity player, BulletData[] pool, float delta)
    {
        // No shooting during the breather between phases.
        if (boss.MovementType == BossMovementType.Transition) return;

        if (boss.EmitterMain.IsActive)
            ProcessEmitter(boss, player, ref boss.EmitterMain, pool, delta);
        if (boss.EmitterSub.IsActive)
            ProcessEmitter(boss, player, ref boss.EmitterSub, pool, delta);
    }

    private static void ProcessEmitter(BossEntity boss, PlayerEntity player, ref BulletEmitterComponent emitter, BulletData[] pool, float delta)
    {
        emitter.TimeSinceLastFire += delta;
        if (!(emitter.TimeSinceLastFire >= emitter.FireRate)) return;
        emitter.TimeSinceLastFire = 0;

        // Continuous spin for spirals.
        emitter.BaseAngle += emitter.SpinRate;

        // Aimed attacks ignore the spin and point at the player.
        if (emitter.IsAimed)
        {
            Vector2 toPlayer = player.Transform.Position - boss.Transform.Position;
            emitter.BaseAngle = Mathx.Degrees(MathF.Atan2(toPlayer.Y, toPlayer.X));
            emitter.BaseAngle -= emitter.SpreadAngle / 2; // centre the cone
        }

        int bullets = emitter.BulletsPerVolley;
        float angleStep = bullets > 1 ? emitter.SpreadAngle / bullets : 0;
        Vector2 spawnPos = boss.Transform.Position + emitter.SpawnOffset;

        for (int i = 0; i < bullets; i++)
        {
            float angle = Mathx.Radians(emitter.BaseAngle + i * angleStep);
            Vector2 velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * emitter.BulletSpeed;
            Spawn(pool,
                  spawnPos,
                  velocity,
                  Faction.Boss,
                  emitter.BulletStyle,
                  scale: emitter.BulletScale,
                  acceleration: emitter.Acceleration,
                  curve: emitter.CurveRate,
                  behavior: emitter.BulletBehavior);
        }
    }

    public static void MoveBullets(BulletData[] pool, Rect2 screenBounds, float delta, Vector2 wind, PlayerEntity player, IList<Vector2> targets = null)
    {
        // Slightly larger kill margin so curving bullets can leave the screen and come back.
        Rect2 safeBounds = screenBounds.Grown(100);

        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].IsActive) continue;
            ref BulletData b = ref pool[i];

            if (b.Behavior == BulletBehavior.StopAndAim)
            {
                b.Timer += delta;

                // At 1.5s the bullet has braked: re-aim at the player and fire off fast.
                if (b.Timer >= 1.5f && b.Timer < 5.0f)
                {
                    Vector2 dir = Mathx.Norm(player.Transform.Position - b.Position);
                    b.Velocity = dir * 350;
                    b.Acceleration = 0;
                    b.CurveRate = 0;
                    b.Timer = 10; // lock the route
                }
            }

            if (b.Behavior == BulletBehavior.Homing && targets != null && targets.Count > 0)
            {
                Vector2 target = targets[0];
                float best = target.DistanceTo(b.Position);
                for (int t = 1; t < targets.Count; t++)
                {
                    float d = targets[t].DistanceTo(b.Position);
                    if (d < best) { best = d; target = targets[t]; }
                }
                // Turn towards the nearest foe, but only so fast: the wisp swings round rather than snapping.
                Vector2 velocity = b.Velocity;
                float wanted = MathF.Atan2(target.Y - b.Position.Y, target.X - b.Position.X);
                float turn = wanted - MathF.Atan2(velocity.Y, velocity.X);
                while (turn > MathF.PI) turn -= 2 * MathF.PI;
                while (turn < -MathF.PI) turn += 2 * MathF.PI;
                float limit = Mathx.Radians(WispTurnRate) * delta;
                b.Velocity = velocity.Rotated(MathF.Max(-limit, MathF.Min(limit, turn)));
                // And it speeds up to its cruising speed.
                float speed = MathF.Min(WispSpeed, velocity.Length() + 600 * delta);
                b.Velocity = Mathx.Norm(b.Velocity) * speed;
            }

            if (b.Acceleration != 0)
            {
                float speed = b.Velocity.Length();
                Vector2 direction = Mathx.Norm(b.Velocity);
                speed += b.Acceleration * delta;
                // Don't let the bullet reverse forever: it stops near 0 and moves on.
                if (speed < 0) speed = 0.1f;
                b.Velocity = direction * speed;
            }

            if (b.CurveRate != 0)
                b.Velocity = b.Velocity.Rotated(Mathx.Radians(b.CurveRate * delta));

            b.Position += (b.Velocity + wind * b.WindModifier) * delta;

            if (!safeBounds.HasPoint(b.Position))
                b.IsActive = false;
        }
    }

    /// <summary>
    /// Tells the scene what kinds of bullets were fired since the last call, once for each kind (it makes the sound of each).
    /// </summary>
    public static void ReportFresh(BulletData[] pool, List<WorldEvent> events)
    {
        Dictionary<BulletStyle, int> fired = null;
        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].Fresh) continue;
            pool[i].Fresh = false;
            fired ??= new Dictionary<BulletStyle, int>();
            fired[pool[i].Style] = fired.GetValueOrDefault(pool[i].Style) + 1;
        }
        if (fired == null) return;
        foreach (var pair in fired) events.Add(WorldEvent.BulletsFired(pair.Key, pair.Value));
    }

    /// <summary>What the homing wisps may turn towards: the spirits, and the boss once she is on the screen. None if no card gives wisps.</summary>
    public static List<Vector2> HomingTargets(CardEffects effects, IEnumerable<EnemyEntity> enemies, BossEntity boss, bool bossActive)
    {
        var targets = new List<Vector2>();
        if (effects.HomingWisps <= 0) return targets;
        foreach (var e in enemies) if (e.Health > 0) targets.Add(e.Position);
        if (bossActive && boss.Health.CurrentHealth > 0 && boss.Transform.Position.Y > 0) targets.Add(boss.Transform.Position);
        return targets;
    }

    public static void ClearBossBullets(BulletData[] pool)
    {
        for (int i = 0; i < pool.Length; i++)
            if (pool[i].IsActive && pool[i].Faction == Faction.Boss)
                pool[i].IsActive = false;
    }
}
