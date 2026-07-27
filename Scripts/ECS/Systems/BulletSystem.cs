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
        if (player.Input.IsShooting && player.Weapon.TimeSinceLastFire >= player.Weapon.FireRate)
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

    public static void UpdateBossWeapons(BossEntity boss, PlayerEntity player, ref BulletEmitterComponent emitter, BulletData[] pool, float delta) {
        
        // NOVO: Se está naqueles 2 segundos de respiro/transição, não atira!
        if (boss.MovementType == BossMovementType.Transition) {
            return; 
        }

        emitter.TimeSinceLastFire += delta;

        if (emitter.TimeSinceLastFire >= emitter.FireRate) {
            emitter.TimeSinceLastFire = 0f;

            // Adiciona o giro contínuo para espirais
            emitter.BaseAngle += emitter.SpinRate;

            // Se o ataque for mirado, a BaseAngle ignora o giro e aponta direto pro jogador
            if (emitter.IsAimed && player != null) {
                Vector2 dirToPlayer = player.Transform.Position - boss.Transform.Position;
                emitter.BaseAngle = Mathf.RadToDeg(Mathf.Atan2(dirToPlayer.Y, dirToPlayer.X));
                // Centraliza o cone
                emitter.BaseAngle -= emitter.SpreadAngle / 2f; 
            }

            int bullets = emitter.BulletsPerVolley;
            float angleStep = bullets > 1 ? emitter.SpreadAngle / bullets : 0;

            for (int i = 0; i < bullets; i++) {
                float angleDeg = emitter.BaseAngle + (i * angleStep);
                float angleRad = Mathf.DegToRad(angleDeg);
                Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                Vector2 vel = dir * emitter.BulletSpeed;
                SpawnBullet(pool, boss.Transform.Position, vel, 1);
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

    public static void ClearBossBullets(BulletData[] pool)
    {
        for (int i = 0; i < pool.Length; i++)
        {
            // Faction 1 = Balas do Chefe. Se estiver ativa, nós desativamos!
            if (pool[i].IsActive && pool[i].Faction == 1)
            {
                pool[i].IsActive = false;
                
                // FUTURO: Aqui nós vamos mandar o "Spawner de Itens" criar 
                // um item de pontuação na exata coordenada desta bala (pool[i].Position)
            }
        }
    }
}