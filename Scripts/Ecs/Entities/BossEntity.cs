using Godot;

/// <summary>A boss: the components that move and fire it, and the definition that says what its attacks are.</summary>
public class BossEntity
{
    public int Id = 0;
    public readonly BossDefinition Definition;

    public BossEntity(BossDefinition definition) { Definition = definition; }

    /// <summary>A boss at the top of the picture with all its lives, in the shape of its first attack.</summary>
    public static BossEntity Create(BossDefinition definition) => new BossEntity(definition)
    {
        Id = 2,
        Transform = new Transform2DComponent(new Vector2(300, 140), new Vector2(1, 1)),
        Movement = new VelocityComponent { Speed = 200 },
        Hitbox = new HitboxComponent(definition.PhaseFor(definition.Lives).HitboxRadius),
        Health = new HealthComponent(100, 100, definition.Lives, 0),
        PatrolDirection = 1,
    };

    public Transform2DComponent Transform = new Transform2DComponent();
    public VelocityComponent Movement = new VelocityComponent();
    public HitboxComponent Hitbox = new HitboxComponent();
    public HealthComponent Health = new HealthComponent();
    public float PatrolDirection = 1;

    /// <summary>The shape of the attack it is on.</summary>
    public BossForm Form => Definition.PhaseFor(Health.Lives).Form;

    public BossMovementType MovementType = BossMovementType.Patrol;
    public float DashTimer = 0;

    public BossMovementType NextMovementType = BossMovementType.Patrol;
    public Vector2 TargetPosition = Vector2.Zero;
    public float TransitionTimer = 0;
    /// <summary>Speed of the gliding transition; 0 means the boss's normal speed. The entrance from above the screen is slower.</summary>
    public float TransitionSpeed = 0;

    /// <summary>Fractions of damage (from the bomb's steady light) not yet taken off the health.</summary>
    public float DamageCarry = 0;

    public BulletEmitterComponent EmitterMain = new BulletEmitterComponent();
    public BulletEmitterComponent EmitterSub = new BulletEmitterComponent();
}
