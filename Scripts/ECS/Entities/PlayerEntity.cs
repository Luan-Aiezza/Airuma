using Godot;
using System;

public class PlayerEntity
{
    public int Id;
    public Transform2DComponent Transform;
    public VelocityComponent Movement;
    public PlayerInputComponent Input;
    public PlayerWeaponComponent Weapon;
    public HitboxComponent Hitbox;
    public HealthComponent Health;
    public RenderComponent Render;
    public WindModifierComponent WindModifier; // <-- Nova linha
    public BombComponent Bomb;
}