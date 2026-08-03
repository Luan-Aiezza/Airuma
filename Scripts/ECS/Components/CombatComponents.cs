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
    public bool IsActive; 
    public float FireRate;
    public float TimeSinceLastFire;
    public int BulletsPerVolley;
    public float SpreadAngle;
    public float BaseAngle;
    public float BulletSpeed;
    public float SpinRate;
    public bool IsAimed;
    
    public float Acceleration;
    public float CurveRate;
    public float BulletScale;
    public Vector2 SpawnOffset; 
    
    // ---> A VARIÁVEL QUE ESTAVA FALTANDO: <---
    public int BulletBehavior; 
}

public struct BombComponent {
    public int BombsLeft;
    public bool IsActive;
    public float Timer;
    public float Duration;
    public float Radius; // Tamanho da aura de luz
}