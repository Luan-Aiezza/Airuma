using System.Collections.Generic;
using Godot;

/// <summary>A forest spirit: where it is, which path it follows and which patterns it fires, with its health.</summary>
public class EnemyEntity
{
    public readonly int Id;
    public readonly SpiritKind Kind;
    public Vector2 Position;
    public readonly Vector2 Origin;
    public readonly EnemyPath Path;
    public Vector2 Velocity = Vector2.Zero;
    public float Age = 0;
    public int Health;
    /// <summary>A phase offset for the swaying paths, so groups can sway in step or in opposition.</summary>
    public readonly float Phase;

    public List<ScheduledPattern> Patterns;
    public int PatternIndex = 0;
    /// <summary>How many volleys it has fired (the easy mode skips every second one of a single aimed shot).</summary>
    public int Shots = 0;
    public float PatternClock = 0;
    public float ShotClock;
    public float Spin = 0;

    // For Dive
    public bool Arrived = false;
    public float HoldClock = 0;

    /// <summary>Time left of the white flash after a hit.</summary>
    public float HitFlash = 0;
    /// <summary>Fractions of damage (from the bomb's steady light) not yet taken off the health.</summary>
    public float DamageCarry = 0;

    public EnemyEntity(int id, SpiritKind kind, Vector2 position, EnemyPath path, float phase = 0,
                       List<ScheduledPattern> patterns = null, float firstShotDelay = 0.6f)
    {
        Id = id;
        Kind = kind;
        Position = position;
        Origin = position;
        Path = path;
        Phase = phase;
        Patterns = patterns ?? new List<ScheduledPattern>();
        // The first shot comes firstShotDelay seconds after the enemy is ready to shoot.
        ShotClock = (Patterns.Count > 0 ? Patterns[0].Pattern.Interval : 0) - firstShotDelay;
        Health = kind.MaxHealth();
    }
}
