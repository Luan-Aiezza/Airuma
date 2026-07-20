using Godot;
using System;

public struct BulletData
{
    public bool IsActive;
    public Vector2 Position;
    public Vector2 Velocity;
    public int Faction;        // 0: Jogador, 1: Inimigo
    public float HitboxRadius;
    public int Damage;
}