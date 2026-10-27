using System;
using System.Collections.Generic;
using Godot;

/// <summary>Who hurts whom: bullets and bodies against the boat and the boss, the player's shots and the bomb against the spirits, and health.</summary>
public static class CombatSystem
{
    public static readonly Vector2 PlayerSpawn = new Vector2(300, 700);

    /// <summary>Seconds of grace after being hurt by touching something.</summary>
    public const float ContactGrace = 2.0f;

    /// <summary>
    /// Resolves bullet and body collisions. Touching the boss or any spirit hurts the player. Returns how many player
    /// shots hit the boss.
    /// </summary>
    public static int ProcessCollisions(BulletData[] pool, PlayerEntity player, BossEntity boss, bool bossActive = true,
                                        IList<EnemyEntity> enemies = null)
    {
        int bossHits = 0;
        if (player.Health.InvincibilityTimer <= 0 && !player.Bomb.IsActive)
        {
            // Body contact with the boss, or with a spirit.
            bool touchesBoss = bossActive
                && player.Transform.Position.DistanceTo(boss.Transform.Position) < player.Hitbox.Radius + boss.Hitbox.Radius;
            bool touchesSpirit = false;
            if (enemies != null)
                foreach (var e in enemies)
                    if (e.Health > 0 && player.Transform.Position.DistanceTo(e.Position) < player.Hitbox.Radius + e.Kind.HitboxRadius())
                    { touchesSpirit = true; break; }
            if (touchesBoss || touchesSpirit)
                Hurt(player, 1, ContactGrace);
        }

        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].IsActive) continue;
            ref BulletData b = ref pool[i];
            if (b.Faction == Faction.Boss && player.Health.InvincibilityTimer <= 0 && !player.Bomb.IsActive)
            {
                if (b.Position.DistanceTo(player.Transform.Position) < b.HitboxRadius + player.Hitbox.Radius)
                {
                    Hurt(player, b.Damage, 0);
                    b.IsActive = false;
                }
            }
            else if (bossActive && b.Faction == Faction.Player && boss.Health.CurrentHealth > 0 && b.LastHit != boss.Id)
            {
                if (b.Position.DistanceTo(boss.Transform.Position) < b.HitboxRadius + boss.Hitbox.Radius)
                {
                    boss.DamageCarry += b.Damage * b.Power;
                    int whole = (int)boss.DamageCarry;
                    boss.DamageCarry -= whole;
                    boss.Health.CurrentHealth -= whole;
                    bossHits += 1;
                    if (b.Pierce > 0)
                    {
                        b.Pierce -= 1;
                        b.LastHit = boss.Id;
                    }
                    else
                    {
                        b.IsActive = false;
                    }
                }
            }
        }
        return bossHits;
    }

    /// <summary>Seconds of grace after a ward soaks up a hit.</summary>
    public const float WardGrace = 1.0f;

    /// <summary>A hit on the player: a ward, if there is one, takes it instead of a life.</summary>
    public static void Hurt(PlayerEntity player, int damage, float grace)
    {
        if (player.Shield > 0)
        {
            player.Shield -= 1;
            player.ShieldBroke = true;
            player.Health.InvincibilityTimer = WardGrace;
        }
        else
        {
            player.Health.CurrentHealth -= damage;
            player.Health.InvincibilityTimer = MathF.Max(player.Health.InvincibilityTimer, grace);
        }
    }

    /// <summary>
    /// Takes amount (which may be a fraction) off a spirit; the fraction left over is kept for the next hit.
    /// Returns whether that finished it.
    /// </summary>
    public static bool Damage(EnemyEntity enemy, float amount)
    {
        enemy.HitFlash = 0.08f;
        enemy.DamageCarry += amount;
        int whole = (int)enemy.DamageCarry;
        enemy.DamageCarry -= whole;
        enemy.Health -= whole;
        return enemy.Health <= 0;
    }

    /// <summary>Returns true when the boss lost a life (i.e. moved to a new phase).</summary>
    public static bool ProcessHealth(PlayerEntity player, BossEntity boss, float delta, bool bossActive = true)
    {
        if (player.Health.InvincibilityTimer > 0)
        {
            player.Health.InvincibilityTimer -= delta;
            // Blink while invincible.
            player.IsVisible = (int)MathF.Floor(player.Health.InvincibilityTimer * 10) % 2 == 0;
        }
        else
        {
            player.IsVisible = true;
        }

        if (player.Health.CurrentHealth <= 0 && player.Health.Lives > 0)
        {
            player.Health.Lives -= 1;
            if (player.Health.Lives > 0)
            {
                player.Health.CurrentHealth = player.Health.MaxHealth;
                player.Health.InvincibilityTimer = 3.0f;
                player.Transform.Position = PlayerSpawn;
            }
            else
            {
                player.IsVisible = false;
            }
        }

        bool bossPhaseChanged = false;
        if (bossActive && boss.Health.CurrentHealth <= 0 && boss.Health.Lives > 0)
        {
            boss.Health.Lives -= 1;
            // Refill the bar only if there are lives left.
            if (boss.Health.Lives > 0)
                boss.Health.CurrentHealth = boss.Health.MaxHealth;
            bossPhaseChanged = true;
        }
        return bossPhaseChanged;
    }

    // The bomb

    /// <summary>
    /// While the bomb is lit, everything inside its light is hurt: spirits badly, the boss a little less. Returns the
    /// spirits it finished off.
    /// </summary>
    public static List<(SpiritKind kind, Vector2 position)> ProcessBombDamage(PlayerEntity player, IList<EnemyEntity> enemies, BossEntity boss,
        bool bossActive, float delta, float damageMultiplier = 1)
    {
        var defeated = new List<(SpiritKind kind, Vector2 position)>();
        if (!player.Bomb.IsActive) return defeated;
        foreach (var enemy in enemies)
        {
            if (!(enemy.Health > 0)) continue;
            if (!(player.Transform.Position.DistanceTo(enemy.Position) <= player.Bomb.Radius)) continue;
            if (Damage(enemy, BombSystem.SpiritDamagePerSecond * damageMultiplier * delta)) defeated.Add((enemy.Kind, enemy.Position));
        }
        if (bossActive && boss.Health.CurrentHealth > 0
            && player.Transform.Position.DistanceTo(boss.Transform.Position) <= player.Bomb.Radius + boss.Hitbox.Radius)
        {
            boss.DamageCarry += BombSystem.BossDamagePerSecond * damageMultiplier * delta;
            int whole = (int)boss.DamageCarry;
            boss.DamageCarry -= whole;
            boss.Health.CurrentHealth -= whole;
        }
        return defeated;
    }

    // Spirits

    public class EnemyHitReport
    {
        public int Hits = 0;
        /// <summary>The kinds of spirit that were hit and are still standing (the swarm never is: one shot kills it).</summary>
        public List<SpiritKind> Survivors = new List<SpiritKind>();
        public List<(SpiritKind kind, Vector2 position)> Defeated = new List<(SpiritKind kind, Vector2 position)>();
    }

    /// <summary>The player's shots against the spirits: each shot is used up on the first spirit it touches, unless it pierces.</summary>
    public static EnemyHitReport ProcessEnemyHits(BulletData[] pool, IList<EnemyEntity> enemies)
    {
        var report = new EnemyHitReport();
        if (enemies.Count == 0) return report;
        for (int i = 0; i < pool.Length; i++)
        {
            if (!(pool[i].IsActive && pool[i].Faction == Faction.Player)) continue;
            ref BulletData b = ref pool[i];
            foreach (var enemy in enemies)
            {
                if (!(enemy.Health > 0 && enemy.Id != b.LastHit)) continue;
                if (!(b.Position.DistanceTo(enemy.Position) < b.HitboxRadius + enemy.Kind.HitboxRadius())) continue;
                float strength = b.Damage * b.Power;
                if (Damage(enemy, strength)) report.Defeated.Add((enemy.Kind, enemy.Position));
                else report.Survivors.Add(enemy.Kind);
                report.Hits += 1;
                if (b.BurstRadius > 0)
                {
                    // The bolt bursts and scorches whoever is close.
                    foreach (var other in enemies)
                    {
                        if (other != enemy && other.Health > 0
                            && other.Position.DistanceTo(enemy.Position) < b.BurstRadius + other.Kind.HitboxRadius())
                        {
                            if (Damage(other, strength * BurstShare * b.BurstScale)) report.Defeated.Add((other.Kind, other.Position));
                        }
                    }
                }
                if (b.Pierce > 0)
                {
                    b.Pierce -= 1;
                    b.LastHit = enemy.Id;
                }
                else
                {
                    b.IsActive = false;
                }
                break;
            }
        }
        return report;
    }

    /// <summary>The share of a bolt's strength that a burst deals to the foes around the one it hit.</summary>
    public const float BurstShare = 0.6f;
}
