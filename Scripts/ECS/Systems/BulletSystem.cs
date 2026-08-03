using Godot;
using System;

public static class BulletSystem
{
    // Função auxiliar internalizada no sistema
    // Adicione a escala, aceleração e curva nos parâmetros!
    private static void SpawnBullet(BulletData[] pool, Vector2 position, Vector2 velocity, int faction, float scale = 1f, float accel = 0f, float curve = 0f, int behavior = 0) {
        for (int i = 0; i < pool.Length; i++) {
            if (!pool[i].IsActive) {
                pool[i].IsActive = true;
                pool[i].Damage = 1;
                pool[i].HitboxRadius = 5f * scale; // A hitbox cresce com a escala!
                pool[i].Position = position;
                pool[i].Velocity = velocity;
                pool[i].Faction = faction;
                pool[i].WindModifier = faction == 0 ? 1.0f : 0.5f; 
                
                pool[i].Scale = scale;
                pool[i].Acceleration = accel;
                pool[i].CurveRate = curve;
                pool[i].Behavior = behavior;
                pool[i].Timer = 0f;
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

    public static void UpdateBossWeapons(BossEntity boss, PlayerEntity player, BulletData[] pool, float delta) {
        // Se está naqueles 2 segundos de respiro/transição, não atira!
        if (boss.MovementType == BossMovementType.Transition) {
            return; 
        }

        // Processa o Emissor Principal (se estiver ativo)
        if (boss.EmitterMain.IsActive) {
            ProcessEmitter(boss, player, ref boss.EmitterMain, pool, delta);
        }

        // Processa o Emissor Secundário (se estiver ativo)
        if (boss.EmitterSub.IsActive) {
            ProcessEmitter(boss, player, ref boss.EmitterSub, pool, delta);
        }
    }

// O método auxiliar que faz a matemática pesada para qualquer emissor que enviarmos
    private static void ProcessEmitter(BossEntity boss, PlayerEntity player, ref BulletEmitterComponent emitter, BulletData[] pool, float delta) {
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

            // NOVO: Calcula o ponto de origem com o deslocamento (SpawnOffset)
            Vector2 spawnPos = boss.Transform.Position + emitter.SpawnOffset;

            for (int i = 0; i < bullets; i++) {
                float angleDeg = emitter.BaseAngle + (i * angleStep);
                float angleRad = Mathf.DegToRad(angleDeg);
                Vector2 dir = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
                Vector2 vel = dir * emitter.BulletSpeed;
                
                // NOVO: Passa os modificadores de Danmaku para a Pool!
                // Assumindo que a sua função SpawnBullet já foi atualizada para receber scale, accel e curve.
                SpawnBullet(pool, spawnPos, vel, 1, emitter.BulletScale, emitter.Acceleration, emitter.CurveRate, emitter.BulletBehavior);
            }
        }
    }

    // Adicione o parâmetro 'Vector2 windVector'
    public static void MoveBullets(BulletData[] pool, Rect2 screenBounds, float delta, Vector2 globalWind, PlayerEntity player) {
        for (int i = 0; i < pool.Length; i++) {
            if (pool[i].IsActive) {

                if (pool[i].Behavior == 1) {
                    pool[i].Timer += delta;
                    
                    // Aos 1.5 segundos a bala congela. Aos 1.5s exatos, ela mira e dispara!
                    if (pool[i].Timer >= 1.5f && pool[i].Timer < 5.0f) {
                        if (player != null) {
                            Vector2 dirToPlayer = (player.Transform.Position - pool[i].Position).Normalized();
                            
                            // Reescreve a física da bala:
                            pool[i].Velocity = dirToPlayer * 350f; // Dispara rápido na direção do player
                            pool[i].Acceleration = 0f;             // Desliga o freio anterior
                            pool[i].CurveRate = 0f;                // Desliga o giro da flor
                            
                            // Joga o timer para um valor alto para garantir que ela não mire de novo (trava a rota)
                            pool[i].Timer = 10.0f; 
                        }
                    }
                }

                // Aplica a Aceleração
                if (pool[i].Acceleration != 0) {
                    float currentSpeed = pool[i].Velocity.Length();
                    Vector2 direction = pool[i].Velocity.Normalized();
                    currentSpeed += pool[i].Acceleration * delta;
                    
                    // Impede que a bala dê ré eternamente, ela para no 0 e avança
                    if (currentSpeed < 0) currentSpeed = 0.1f; 
                    pool[i].Velocity = direction * currentSpeed;
                }

                // Aplica a Curva (Angular Velocity)
                if (pool[i].CurveRate != 0) {
                    float curveRads = Mathf.DegToRad(pool[i].CurveRate * delta);
                    pool[i].Velocity = pool[i].Velocity.Rotated(curveRads);
                }

                // Movimento normal + Vento
                pool[i].Position += (pool[i].Velocity + (globalWind * pool[i].WindModifier)) * delta;

                // Expandimos levemente a margem de destruição para balas que fazem curva fora da tela e voltam
                Rect2 safeBounds = screenBounds.Grow(100f);
                if (!safeBounds.HasPoint(pool[i].Position)) {
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