using Godot;
using System;

public struct HitboxComponent 
{ 
    public float Radius; 
}

public struct HealthComponent 
{ 
    public int CurrentHealth; 
    public int MaxHealth; 
    public int Lives; 
    public float InvincibilityTimer; 
}

public struct PlayerWeaponComponent 
{ 
    public int PowerLevel; 
    public float FireRate; 
    public float TimeSinceLastFire; 
    public float BulletSpeed; 
}

public struct BulletEmitterComponent {
    public float FireRate;
    public float TimeSinceLastFire;
    public int BulletsPerVolley;
    public float SpreadAngle;
    public float BaseAngle;
    public float BulletSpeed;
    
    // NOVOS PARÂMETROS DANMAKU
    public float SpinRate; // Faz o ângulo girar a cada tiro (Espirais)
    public bool IsAimed;   // Força o tiro a mirar no jogador
}

public struct BombComponent {
    public int BombsLeft;
    public bool IsActive;
    public float Timer;
    public float Duration;
    public float Radius; // Tamanho da aura de luz
}