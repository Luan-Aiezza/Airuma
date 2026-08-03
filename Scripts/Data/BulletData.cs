using Godot;
using System;

public struct BulletData {
    public bool IsActive;
    public Vector2 Position;
    public Vector2 Velocity;
    public int Faction;
    public float HitboxRadius;
    public int Damage;
    public float WindModifier;
    
    // Variáveis de Padrões Avançados (Danmaku)
    public float Acceleration;   
    public float CurveRate;      
    public float Scale;          
    
    // ---> AS VARIÁVEIS QUE ESTAVAM FALTANDO: <---
    public int Behavior; // 0 = Normal, 1 = Stop and Aim
    public float Timer;  // Cronômetro interno da bala
}