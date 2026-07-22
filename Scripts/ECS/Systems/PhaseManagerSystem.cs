using Godot;
using System;

public static class PhaseManagerSystem {
    public static void ChangePhase(BossEntity boss, int currentLives, ref BulletEmitterComponent emitter) {
		// CORREÇÃO: Se a vida for 0 ou menor, o Boss foi derrotado!
        if (currentLives <= 0) {
            // Congela a boss, joga ela pra fora da tela e para os tiros.
            // (Futuramente, aqui chamaremos o sistema de Explosão e a Tela de Vitória)
            boss.MovementType = BossMovementType.Stationary;
            boss.Transform.Position = new Vector2(-9999, -9999); 
            emitter.FireRate = 9999f; 
            return; // O 'return' impede que o restante do código abaixo seja lido!
        }

        // Reseta atributos padrões
        boss.Transform.Scale = new Vector2(1, 1);
        boss.Hitbox.Radius = 40f;
        emitter.IsAimed = false;
        emitter.SpinRate = 0f;
        
        // CONFIGURAÇÃO GLOBAL DA TRANSIÇÃO (2 segundos de respiro)
        boss.MovementType = BossMovementType.Transition;
        boss.TransitionTimer = 2.0f; 

        switch (currentLives) {
            case 6: // FASE 1: O Anel Clássico (Vai pro topo patrulhar)
                boss.NextMovementType = BossMovementType.Patrol;
                boss.TargetPosition = new Vector2(300, 100); 
                emitter.FireRate = 1.0f;
                emitter.BulletsPerVolley = 36;
                emitter.SpreadAngle = 360f;
                emitter.BulletSpeed = 150f;
                emitter.SpinRate = 5f;
                break;

            case 5: // FASE 1: Shotgun Mirada (Vai pro centro da tela e para)
                boss.NextMovementType = BossMovementType.Stationary;
                boss.TargetPosition = new Vector2(300, 150); 
                emitter.FireRate = 0.8f;
                emitter.BulletsPerVolley = 5; 
                emitter.SpreadAngle = 60f; 
                emitter.BulletSpeed = 250f;
                emitter.IsAimed = true; 
                break;

            case 4: // FASE 1: Espiral Dupla Rápida (Fica no centro)
                boss.NextMovementType = BossMovementType.Stationary;
                boss.TargetPosition = new Vector2(300, 150);
                emitter.FireRate = 0.1f; 
                emitter.BulletsPerVolley = 2; 
                emitter.SpreadAngle = 180f; 
                emitter.BulletSpeed = 200f;
                emitter.SpinRate = 15f; 
                break;

            case 3: // FASE 1: Flor de Lótus (Volta pro topo patrulhar)
                boss.NextMovementType = BossMovementType.Patrol;
                boss.TargetPosition = new Vector2(300, 100);
                emitter.FireRate = 0.5f;
                emitter.BulletsPerVolley = 12;
                emitter.SpreadAngle = 360f;
                emitter.BulletSpeed = 180f;
                emitter.SpinRate = 10f;
                break;

            case 2: // FASE 2: TRANSFORMAÇÃO! Cobra Gigante (Meio da tela)
                boss.NextMovementType = BossMovementType.Stationary;
                boss.TargetPosition = new Vector2(300, 150);
                boss.Transform.Scale = new Vector2(3f, 3f); 
                boss.Hitbox.Radius = 120f; 
                emitter.FireRate = 0.2f;
                emitter.BulletsPerVolley = 20;
                emitter.SpreadAngle = 360f;
                emitter.BulletSpeed = 100f;
                emitter.SpinRate = 7f;
                break;

            case 1: // FASE 2 FINAL: Screen-Wrap Dash
                boss.NextMovementType = BossMovementType.Dash;
                boss.DashTimer = 0f; 
                boss.Transform.Scale = new Vector2(3f, 3f);
                boss.Hitbox.Radius = 100f;
                emitter.FireRate = 9999f; // Desliga tiros
                
                // Exceção: Como faz sentido ela sumir e aparecer pro ataque final, 
                // teletransportamos ela direto pro topo e pulamos o deslize visual (mas mantemos o atraso de 2s)
                boss.TargetPosition = new Vector2(300, -50); 
                boss.Transform.Position = boss.TargetPosition; 
                break;
        }
    }
}