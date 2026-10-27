using Godot;

/// <summary>A boss's source of bullets: how often it fires, how many, in which directions and with what behaviour.</summary>
public struct BulletEmitterComponent
{
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

    public BulletBehavior BulletBehavior;
    public BulletStyle BulletStyle;

    public BulletEmitterComponent()
    {
        IsActive = false; FireRate = 0; TimeSinceLastFire = 0; BulletsPerVolley = 0; SpreadAngle = 0; BaseAngle = 0;
        BulletSpeed = 0; SpinRate = 0; IsAimed = false; Acceleration = 0; CurveRate = 0; BulletScale = 1;
        SpawnOffset = Vector2.Zero; BulletBehavior = BulletBehavior.Normal; BulletStyle = BulletStyle.Fireball;
    }
}
