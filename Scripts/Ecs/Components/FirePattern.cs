using Godot;

public enum FirePatternKind { None, Aimed, Ring, Fan, Spiral }

/// <summary>How an enemy shoots, for as long as a ScheduledPattern lasts.</summary>
public struct FirePattern
{
    public FirePatternKind Kind;
    public float Interval;
    public int Count;
    public float Speed;
    /// <summary>Ring: degrees the start angle turns by each time.</summary>
    public float Spin;
    /// <summary>Fan: degrees wide.</summary>
    public float Spread;
    /// <summary>Spiral: number of arms.</summary>
    public int Arms;
    /// <summary>Spiral: degrees each shot turns by.</summary>
    public float Step;

    public static FirePattern None => new FirePattern { Kind = FirePatternKind.None, Interval = 1 };
    /// <summary>A single bullet at the player.</summary>
    public static FirePattern Aimed(float interval, float speed) => new FirePattern { Kind = FirePatternKind.Aimed, Interval = interval, Speed = speed };
    /// <summary>An evenly spaced ring of bullets whose start angle turns by spin degrees each time.</summary>
    public static FirePattern Ring(float interval, int count, float speed, float spin) =>
        new FirePattern { Kind = FirePatternKind.Ring, Interval = interval, Count = count, Speed = speed, Spin = spin };
    /// <summary>A fan of bullets centred on the player, spread degrees wide.</summary>
    public static FirePattern Fan(float interval, int count, float spread, float speed) =>
        new FirePattern { Kind = FirePatternKind.Fan, Interval = interval, Count = count, Spread = spread, Speed = speed };
    /// <summary>Arms of bullets that turn by step degrees each shot.</summary>
    public static FirePattern Spiral(float interval, int arms, float step, float speed) =>
        new FirePattern { Kind = FirePatternKind.Spiral, Interval = interval, Arms = arms, Step = step, Speed = speed };
}

public struct ScheduledPattern
{
    public FirePattern Pattern;
    public float Duration;
    public ScheduledPattern(FirePattern pattern, float duration) { Pattern = pattern; Duration = duration; }
}
