using Godot;
using System;

public class BossEntity
{
    public int Id;
    public Transform2DComponent Transform;
    public VelocityComponent Movement;
    public HitboxComponent Hitbox;
    public HealthComponent Health;
    public RenderComponent Render;
    
    // O estado de patrulha agora é um dado exclusivo da entidade!
    public float PatrolDirection = 1f; 
}