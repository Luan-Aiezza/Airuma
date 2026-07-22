using Godot;
using System;

public enum BossMovementType { Patrol, Stationary, Dash, Transition }

public class BossEntity {
    public int Id;
    public Transform2DComponent Transform;
    public VelocityComponent Movement;
    public HitboxComponent Hitbox;
    public HealthComponent Health;
    public RenderComponent Render;
    public float PatrolDirection = 1f;
    
    // NOVOS ESTADOS DE MOVIMENTO
    public BossMovementType MovementType = BossMovementType.Patrol;
    public float DashTimer = 0f; // Controla o tempo de espera antes do bote

    // NOVAS VARIÁVEIS DE TRANSIÇÃO E ATRASO
    public BossMovementType NextMovementType = BossMovementType.Patrol; 
    public Vector2 TargetPosition; 
    public float TransitionTimer = 0f;
}