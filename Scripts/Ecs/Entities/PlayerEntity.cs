using Godot;

/// <summary>The boat: the components that move, shoot, bomb and hurt it, with the lantern's energy and the cards' wards.</summary>
public class PlayerEntity
{
    /// <summary>The boat at the start of a run: three lives, three bombs, the plain weapon.</summary>
    public static PlayerEntity Create() => new PlayerEntity
    {
        Id = 1,
        Transform = new Transform2DComponent(CombatSystem.PlayerSpawn, new Vector2(1, 1)),
        Movement = new VelocityComponent { Speed = 400 },
        Weapon = new PlayerWeaponComponent { PowerLevel = 1, FireRate = 0.1f, TimeSinceLastFire = 0, BulletSpeed = 800 },
        Hitbox = new HitboxComponent(4),
        Health = new HealthComponent(1, 1, 3, 0),
        Bomb = new BombComponent(3, false, 0, 5, 250),
        WindModifier = new WindModifierComponent(1),
    };

    public int Id = 0;
    /// <summary>Lantern energy gathered from the spirits' lights; sets the weapon level (see PowerCurve).</summary>
    public float Energy = 0;
    public Transform2DComponent Transform = new Transform2DComponent();
    public VelocityComponent Movement = new VelocityComponent();
    public PlayerInputComponent Input = new PlayerInputComponent();
    public PlayerWeaponComponent Weapon = new PlayerWeaponComponent();
    public HitboxComponent Hitbox = new HitboxComponent();
    public HealthComponent Health = new HealthComponent();
    public WindModifierComponent WindModifier = new WindModifierComponent();
    public BombComponent Bomb = new BombComponent();
    /// <summary>Hits the wards (cards) can still soak up.</summary>
    public int Shield = 0;
    /// <summary>Set when a ward took a hit; the world reads and clears it.</summary>
    public bool ShieldBroke = false;
    /// <summary>Seconds towards the mending ward mending itself.</summary>
    public float WardClock = 0;
    /// <summary>Whether the sprite should currently be drawn (blinks while invincible).</summary>
    public bool IsVisible = true;
}
