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

public struct BulletEmitterComponent 
{ 
    public float FireRate; 
    public float TimeSinceLastFire; 
    public int BulletsPerVolley; 
    public float SpreadAngle; 
    public float BaseAngle; 
    public float BulletSpeed; 
}