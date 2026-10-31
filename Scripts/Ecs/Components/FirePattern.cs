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

    // How the bullets look and behave (the same options as a boss's emitter): the style (null: the usual one of the kind), the size, and how they move.
    public BulletStyle? Style;
    public float Scale;                     // 0: the usual size
    public BulletBehavior Behavior;
    public float Curve;
    public bool AlternateCurve;

    /// <summary>The bullets of the pattern in another style.</summary>
    public FirePattern Styled(BulletStyle style, float scale = 0) { var p = this; p.Style = style; p.Scale = scale; return p; }
    /// <summary>Bullets that dance (see <see cref="BulletBehavior.Dance"/>): they sway on the beat, and with `alternate` neighbours sway against each other.</summary>
    public FirePattern Dancing(float curve, bool alternate = true) { var p = this; p.Behavior = BulletBehavior.Dance; p.Curve = curve; p.AlternateCurve = alternate; return p; }

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
