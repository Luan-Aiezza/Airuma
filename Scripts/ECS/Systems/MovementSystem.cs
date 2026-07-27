using Godot;
using System;

public static class MovementSystem
{
    private static readonly Random _random = new Random();
    public static void ProcessPlayerInput(PlayerEntity player) {
        Vector2 inputDir = Vector2.Zero;
        if (Input.IsKeyPressed(Key.Left)) inputDir.X -= 1;
        if (Input.IsKeyPressed(Key.Right)) inputDir.X += 1;
        if (Input.IsKeyPressed(Key.Up)) inputDir.Y -= 1;
        if (Input.IsKeyPressed(Key.Down)) inputDir.Y += 1;

        player.Input.Direction = inputDir.Normalized();
        
        // CONTROLES DE DANMAKU CLÁSSICOS
        player.Input.IsShooting = Input.IsKeyPressed(Key.Z); // Tiro no Z
        player.Input.IsBombing = Input.IsKeyPressed(Key.X);  // Bomba no X
    }

    public static void MovePlayer(PlayerEntity player, Rect2 bounds, float delta, Vector2 windVector)
    {
        player.Movement.Velocity = player.Input.Direction * player.Movement.Speed;
        
        // Calcula a velocidade total = Movimento do Jogador + (Vento * Modificador)
        Vector2 totalVelocity = player.Movement.Velocity + (windVector * player.WindModifier.Value);
        
        // Aplica na posição
        player.Transform.Position += totalVelocity * delta;

        float clampedX = Mathf.Clamp(player.Transform.Position.X, bounds.Position.X, bounds.End.X);
        float clampedY = Mathf.Clamp(player.Transform.Position.Y, bounds.Position.Y, bounds.End.Y);
        player.Transform.Position = new Vector2(clampedX, clampedY);
    }

    public static void MoveBoss(BossEntity boss, Rect2 bounds, float delta) {
        
        // 1. MODO TRANSIÇÃO: Desliza suavemente até o alvo
        if (boss.MovementType == BossMovementType.Transition) {
            boss.Transform.Position = boss.Transform.Position.MoveToward(boss.TargetPosition, boss.Movement.Speed * delta);
            boss.TransitionTimer -= delta;

            // Fim do respiro de 2 segundos! Começa o ataque.
            if (boss.TransitionTimer <= 0) {
                boss.MovementType = boss.NextMovementType;
            }
            return; // Bloqueia outros movimentos
        }
        
        // 2. MODO PARADO
        else if (boss.MovementType == BossMovementType.Stationary) {
            return; 
        } 
        
        // 3. MODO PATRULHA
        else if (boss.MovementType == BossMovementType.Patrol) {
            boss.Movement.Velocity = new Vector2(boss.PatrolDirection * boss.Movement.Speed, 0);
            boss.Transform.Position += boss.Movement.Velocity * delta;
            float margin = 50f;
            if (boss.Transform.Position.X >= bounds.End.X - margin) boss.PatrolDirection = -1f;
            else if (boss.Transform.Position.X <= bounds.Position.X + margin) boss.PatrolDirection = 1f;
        }
        
        // 4. MODO INVESTIDA (Screen-Wrap de Cima para Baixo!)
        else if (boss.MovementType == BossMovementType.Dash) {
            boss.DashTimer += delta;
            
            // FASE DE ESPERA: Aparece descendo lentamente do topo antes de dar o bote
            if (boss.DashTimer < 1.5f) {
                boss.Transform.Position = boss.Transform.Position.MoveToward(new Vector2(boss.Transform.Position.X, 100), (boss.Movement.Speed * 0.5f) * delta);
            } 
            // FASE DE ATAQUE: Despenca verticalmente destruindo tudo
            else {
                boss.Movement.Velocity = new Vector2(0, boss.Movement.Speed * 5f); 
                boss.Transform.Position += boss.Movement.Velocity * delta;
                
                // O SCREEN-WRAP: Passou do fundo da tela? 
                if (boss.Transform.Position.Y >= bounds.End.Y + boss.Hitbox.Radius) {
                    boss.DashTimer = 0f; // Reseta o relógio
                    
                    // Vai lá pra cima!
                    float startY = -50f; 
                    
                    // Sorteia a via (Esquerda, Centro, Direita)
                    int randomPos = _random.Next(0, 3);
                    float startX = 300f; // Padrão: Centro
                    if (randomPos == 0) startX = 100f;      // Esquerda
                    else if (randomPos == 2) startX = 500f; // Direita
                    
                    // Aplica a nova posição no topo para começar o ciclo de novo
                    boss.Transform.Position = new Vector2(startX, startY);
                }
            }
        }
    }
}