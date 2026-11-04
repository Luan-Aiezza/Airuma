using Godot;

/// <summary>A boss: the components that move and fire it, and the definition that says what its attacks are.</summary>
public class BossEntity
{
    public int Id = 0;
    /// <summary>The second body of a boss that is two has fused into the first: it takes no more part in the fight.</summary>
    public bool Merged;
    public readonly BossDefinition Definition;

    public BossEntity(BossDefinition definition) { Definition = definition; }

    /// <summary>A boss at the top of the picture with all its lives, in the shape of its first attack.</summary>
    public static BossEntity Create(BossDefinition definition) => new BossEntity(definition)
    {
        Id = 2,
        Transform = new Transform2DComponent(new Vector2(300, 140), new Vector2(1, 1)),
        Movement = new VelocityComponent { Speed = 200 },
        Hitbox = new HitboxComponent(definition.PhaseFor(definition.Lives).HitboxRadius),
        Health = new HealthComponent(definition.PhaseFor(definition.Lives).MaxHealth, definition.PhaseFor(definition.Lives).MaxHealth, definition.Lives, 0),
    };

    public Transform2DComponent Transform = new Transform2DComponent();
    public VelocityComponent Movement = new VelocityComponent();
    public HitboxComponent Hitbox = new HitboxComponent();
    public HealthComponent Health = new HealthComponent();
    /// <summary>The state of each way the boss can move (see the brains in BossBrains.cs).</summary>
    public readonly PatrolBrain Patrol = new PatrolBrain();
    public readonly RoamBrain Roam = new RoamBrain();
    public readonly DashBrain Dash = new DashBrain();
    public readonly DiveBrain Dive = new DiveBrain();
    public readonly SweepBrain Sweep = new SweepBrain();
    public readonly CrossBrain Cross = new CrossBrain();

    public BossMovementType MovementType = BossMovementType.Patrol;

    public BossMovementType NextMovementType = BossMovementType.Patrol;
    public Vector2 TargetPosition = Vector2.Zero;
    public float TransitionTimer = 0;
    /// <summary>Speed of the gliding transition; 0 means the boss's normal speed. The entrance from above the screen is slower.</summary>
    public float TransitionSpeed = 0;

    /// <summary>Fractions of damage (from the bomb's steady light) not yet taken off the health.</summary>
    public float DamageCarry = 0;

    /// <summary>What the attack in progress does besides firing: slashes, a quake, a roar.</summary>
    public BossSpecials Specials = new BossSpecials();

    /// <summary>The patterns of the attack in progress (see <see cref="PatternSpec"/>), each with its clock.</summary>
    public PatternRun[] Patterns = System.Array.Empty<PatternRun>();
    public float PatternPressure = 1, PatternSpeed = 1;

    public BulletEmitterComponent EmitterMain = new BulletEmitterComponent();
    public BulletEmitterComponent EmitterSub = new BulletEmitterComponent();
}
