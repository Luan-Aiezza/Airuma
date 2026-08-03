using Godot;
using System;

public static class PhaseManagerSystem {
    public static void ChangePhase(BossEntity boss, int currentLives) {
        
        // CORREÇÃO: Se a vida for 0 ou menor, o Boss foi derrotado!
        if (currentLives <= 0) {
            boss.MovementType = BossMovementType.Stationary;
            boss.Transform.Position = new Vector2(-9999, -9999); 
            boss.EmitterMain.IsActive = false; 
            boss.EmitterSub.IsActive = false;
            return; 
        }

        // 1. Reseta atributos visuais e físicos padrões
        boss.Transform.Scale = new Vector2(1, 1);
        boss.Hitbox.Radius = 40f;
        
        // 2. Desliga os emissores para "limpar" o ataque anterior
        boss.EmitterMain = new BulletEmitterComponent { IsActive = false };
        boss.EmitterSub = new BulletEmitterComponent { IsActive = false };
        
        // 3. CONFIGURAÇÃO GLOBAL DA TRANSIÇÃO (2 segundos de respiro)
        boss.MovementType = BossMovementType.Transition;
        boss.TransitionTimer = 2.0f; 

        switch (currentLives) {
            case 6: // FASE 1: O Anel Clássico (Vai pro topo patrulhar)
                boss.NextMovementType = BossMovementType.Patrol;
                boss.TargetPosition = new Vector2(300, 100); 
                
                boss.EmitterMain = new BulletEmitterComponent {
                    IsActive = true, FireRate = 1.0f, BulletsPerVolley = 36,
                    SpreadAngle = 360f, BulletSpeed = 150f, SpinRate = 5f,
                    BulletScale = 1.0f
                };
                break;

            case 5: // FASE 2: Shotgun Mirada (Vai pro centro da tela e para)
                boss.NextMovementType = BossMovementType.Stationary;
                boss.TargetPosition = new Vector2(300, 150); 
                
                boss.EmitterMain = new BulletEmitterComponent {
                    IsActive = true, FireRate = 0.8f, BulletsPerVolley = 20,
                    SpreadAngle = 120f, BulletSpeed = 250f, IsAimed = true,
                    BulletScale = 1.2f
                };
                break;

            case 4: // FASE 3: REDEMOINHO DUPLO (Spinners Laterais)
                boss.NextMovementType = BossMovementType.Stationary;
                boss.TargetPosition = new Vector2(300, 150);
                
                // Emissor Esquerdo - Gira Horário (+12f)
                boss.EmitterMain = new BulletEmitterComponent {
                    IsActive = true, FireRate = 0.08f, BulletsPerVolley = 2, 
                    SpreadAngle = 180f, BaseAngle = 0f, BulletSpeed = 180f, 
                    SpawnOffset = new Vector2(-150, 0), SpinRate = 12f,
                    BulletScale = 0.8f
                };
                
                // Emissor Direito - Gira Anti-Horário (-12f) para cruzar visualmente!
                boss.EmitterSub = new BulletEmitterComponent {
                    IsActive = true, FireRate = 0.08f, BulletsPerVolley = 2, 
                    SpreadAngle = 180f, BaseAngle = 0f, BulletSpeed = 180f, 
                    SpawnOffset = new Vector2(150, 0), SpinRate = 12f,
                    BulletScale = 0.8f
                };
                break;

            case 3: // FASE 4: Flor de Lótus Teleguiada (Aceleração Negativa, Curva e Homing)
                boss.NextMovementType = BossMovementType.Patrol;
                boss.TargetPosition = new Vector2(300, 100);
                
                boss.EmitterMain = new BulletEmitterComponent {
                    IsActive = true, FireRate = 1.5f, BulletsPerVolley = 20, 
                    SpreadAngle = 360f, BulletSpeed = 300f, 
                    Acceleration = -250f, CurveRate = 45f, 
                    BulletScale = 1.2f, SpinRate = 10f,
                    BulletBehavior = 1 // Ativa o Stop and Aim Teleguiado
                };
                break;

            case 2: // FASE 5: TRANSFORMAÇÃO! Cobra Gigante (Mix de Tamanhos)
                boss.NextMovementType = BossMovementType.Stationary;
                boss.TargetPosition = new Vector2(300, 150);
                boss.Transform.Scale = new Vector2(3f, 3f); 
                boss.Hitbox.Radius = 120f; 
                
                // Bolas GIGANTES que perseguem
                boss.EmitterMain = new BulletEmitterComponent {
                    IsActive = true, FireRate = 1.5f, BulletsPerVolley = 5, 
                    SpreadAngle = 60f, BulletSpeed = 350f, 
                    BulletScale = 4.0f, IsAimed = true 
                };

                // Teia infinita de balinhas minúsculas giratórias
                boss.EmitterSub = new BulletEmitterComponent {
                    IsActive = true, FireRate = 0.05f, BulletsPerVolley = 2, 
                    SpreadAngle = 180f, BulletSpeed = 150f, 
                    SpinRate = 35f, BulletScale = 0.5f 
                };
                break;

            case 1: // FASE 6 FINAL: Screen-Wrap Dash + RASTRO DE BALAS
                boss.NextMovementType = BossMovementType.Dash;
                boss.DashTimer = 0f; 
                boss.Transform.Scale = new Vector2(3f, 3f);
                boss.Hitbox.Radius = 100f;
                
                // NOVO: Libera anéis de balinhas pequenas durante o avanço!
                boss.EmitterMain = new BulletEmitterComponent {
                    IsActive = true, FireRate = 0.12f, BulletsPerVolley = 8,
                    SpreadAngle = 360f, BulletSpeed = 110f, SpinRate = 15f,
                    BulletScale = 0.5f // Balas minúsculas liberadas pelo corpo gigante
                };
                
                boss.TargetPosition = new Vector2(300, -50); 
                boss.Transform.Position = boss.TargetPosition; 
                break;
        }
    }
}