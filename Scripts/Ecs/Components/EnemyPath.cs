using Godot;

public enum EnemyPathKind { Straight, Sine, Arc, Dive, SideSine }

/// <summary>How an enemy moves. All of them start off-screen.</summary>
public struct EnemyPath
{
    public EnemyPathKind Kind;
    /// <summary>Straight: the velocity. Arc: the starting velocity.</summary>
    public Vector2 Velocity;
    /// <summary>Sine: the speed down. Dive: the speed towards the target.</summary>
    public float Speed;
    public float Amplitude;
    public float Frequency;
    /// <summary>Arc: degrees per second the velocity turns.</summary>
    public float Turn;
    public Vector2 Target;
    public float Hold;
    public Vector2 Exit;

    /// <summary>Crosses the field level, in the direction of `velocity`, swaying up and down: the conga line that comes in from the side.</summary>
    public static EnemyPath SideSine(Vector2 velocity, float amplitude, float frequency) =>
        new EnemyPath { Kind = EnemyPathKind.SideSine, Velocity = velocity, Amplitude = amplitude, Frequency = frequency };
    /// <summary>A constant velocity.</summary>
    public static EnemyPath Straight(Vector2 velocity) => new EnemyPath { Kind = EnemyPathKind.Straight, Velocity = velocity };
    /// <summary>Moves down at speed, swaying from side to side.</summary>
    public static EnemyPath Sine(float speed, float amplitude, float frequency) =>
        new EnemyPath { Kind = EnemyPathKind.Sine, Speed = speed, Amplitude = amplitude, Frequency = frequency };
    /// <summary>Starts with velocity, which turns by turn degrees per second: a curve.</summary>
    public static EnemyPath Arc(Vector2 velocity, float turn) => new EnemyPath { Kind = EnemyPathKind.Arc, Velocity = velocity, Turn = turn };
    /// <summary>Flies to target, stays there for hold seconds, then leaves with velocity exit.</summary>
    public static EnemyPath Dive(Vector2 target, float speed, float hold, Vector2 exit) =>
        new EnemyPath { Kind = EnemyPathKind.Dive, Target = target, Speed = speed, Hold = hold, Exit = exit };
}
