using Godot;
using System;

public static class BulletSystem
{
    // Função auxiliar internalizada no sistema
    private static void SpawnBullet(BulletData[] pool, Vector2 position, Vector2 velocity, int faction)
    {
        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].IsActive)
            {
                pool[i].IsActive = true;
                pool[i].Damage = 1;
                pool[i].HitboxRadius = 5f;
                pool[i].Position = position;
                pool[i].Velocity = velocity;
                pool[i].Faction = faction;
                pool[i].WindModifier = 1.0f; // <-- A bala recebe 100% da força do vento
                break; 
            }
        }
    }

    public static void UpdatePlayerWeapons(PlayerEntity player, BulletData[] pool, float delta)
    {
        player.Weapon.TimeSinceLastFire += delta;
        if (Input.IsActionPressed("ui_select") && player.Weapon.TimeSinceLastFire >= player.Weapon.FireRate)
        {
            player.Weapon.TimeSinceLastFire = 0f;
            int level = player.Weapon.PowerLevel;
            float spacing = 15f; 

            Vector2[] spawnOffsets = level switch
            {
                1 => new[] { Vector2.Zero },
                2 => new[] { new Vector2(-spacing, 0), new Vector2(spacing, 0) },
                3 => new[] { Vector2.Zero, new Vector2(-spacing, 0), new Vector2(spacing, 0) },
                _ => new[] { Vector2.Zero }
            };

            foreach (var offset in spawnOffsets)
            {
                Vector2 spawnPos = player.Transform.Position + offset;
                Vector2 bulletVel = new Vector2(0, -player.Weapon.BulletSpeed);
                SpawnBullet(pool, spawnPos, bulletVel, 0);
            }
        }
    }

    public static void UpdateBossWeapons(BossEntity boss, ref BulletEmitterComponent emitter, BulletData[] pool, float delta)
    {
        emitter.TimeSinceLastFire += delta;
        if (emitter.TimeSinceLastFire >= emitter.FireRate)
        {
            emitter.TimeSinceLastFire = 0f;
            emitter.BaseAngle += 10f; 

            int bulletsToSpawn = emitter.BulletsPerVolley;
            float angleStep = emitter.SpreadAngle / bulletsToSpawn;

            for (int i = 0; i < bulletsToSpawn; i++)
            {
                float angleRadians = Mathf.DegToRad(emitter.BaseAngle + (i * angleStep));
                Vector2 direction = new Vector2(Mathf.Cos(angleRadians), Mathf.Sin(angleRadians));
                Vector2 velocity = direction * emitter.BulletSpeed;
                SpawnBullet(pool, boss.Transform.Position, velocity, 1);
            }
        }
    }

    // Adicione o parâmetro 'Vector2 windVector'
    public static void MoveBullets(BulletData[] pool, Rect2 bounds, float delta, Vector2 windVector)
    {
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].IsActive)
            {
                // Soma a velocidade natural da bala com a força do vento
                Vector2 totalVelocity = pool[i].Velocity + (windVector * pool[i].WindModifier);
                pool[i].Position += totalVelocity * delta;
                
                if (!bounds.HasPoint(pool[i].Position))
                {
                    pool[i].IsActive = false;
                }
            }
        }
    }
}