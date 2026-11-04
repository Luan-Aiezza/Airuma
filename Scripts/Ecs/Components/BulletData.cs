using Godot;

/// <summary>One slot of the fixed-size bullet pool.</summary>
public struct BulletData
{
    public bool IsActive;
    /// <summary>Set when the bullet is fired, so the world can tell the scene about it (for the sound) exactly once.</summary>
    public bool Fresh;
    public Vector2 Position;
    public Vector2 Velocity;
    public Faction Faction;
    public float HitboxRadius;
    public int Damage;
    public float WindModifier;

    public float Acceleration;
    public float CurveRate;
    public float Scale;

    public BulletBehavior Behavior;
    public float Timer;
    /// <summary>The stars of a constellation: the constellation they belong to (0: none) and the star that this one is linked to (-1: none).</summary>
    public int Chain, Prev;
    public BulletStyle Style;

    // The motion beyond a straight line (see BulletBehavior): a gravity that pulls the bullet; the fuse of a Launch / Split; what a Launch does; what a Split makes; the bounces left; a polar track.
    public Vector2 Pull;
    public float Fuse;
    public float LaunchSpeed, LaunchAngle;
    public int SplitCount;
    public float SplitSpread, SplitSpeed, ChildScale;
    public bool SplitAimed;
    public BulletStyle ChildStyle;
    public int Bounces;
    /// <summary>How much the picture of the bullet is enlarged (1: as drawn). The hitbox does not follow it: it is set on its own.</summary>
    public float Zoom;
    /// <summary>What a Launch / Split gives its bullets as acceleration after the fuse.</summary>
    public float LaunchAccel;
    public Vector2 Anchor;
    public float Radius, RadialSpeed, RadialAcceleration, Theta, Omega;
    /// <summary>The place of this star in its constellation, and, if above 0, the seconds in which the lines of the constellation take turns to open (see <see cref="BulletSystem.Links"/>).</summary>
    public int LinkIndex;
    public float Blink;
    /// <summary>If above 0, the speed the bullet settles back to after something has pushed it (the great boulders, after a roar), and which the roar pushes.</summary>
    public float Cruise;
    // The turns (see BulletBehavior.Turn): how it turns (0: by a fixed angle more, 1: towards the boat, plus the angle, 2: to an absolute heading), the seconds between turns, how many, the angle, the speed after, and the state.
    public int TurnMode, TurnTimes, TurnCount;
    public float TurnInterval, TurnRotation, TurnSpeed, TurnBase, TurnHeading;
    // A burst of extra speed at the start that dies away (pop), and a curve/acceleration that lasts only a while (ex).
    public float PopSpeed, PopTime, PopLeft, ExFor, ExClock;

    // The player's shots, as the cards change them.
    /// <summary>Damage multiplier on top of Damage.</summary>
    public float Power;
    /// <summary>How many more foes it can pass through.</summary>
    public int Pierce;
    /// <summary>If above 0, the shot bursts on impact and hurts the foes this close to it.</summary>
    public float BurstRadius;
    /// <summary>The share of the burst this bolt deals (the bolts of a volley share one burst, whatever the weapon level).</summary>
    public float BurstScale;
    /// <summary>The last foe it went through, so it does not hit it again on every step.</summary>
    public int LastHit;

    public BulletData()
    {
        IsActive = false; Position = Vector2.Zero; Velocity = Vector2.Zero; Faction = Faction.Player; HitboxRadius = 0;
        Damage = 0; WindModifier = 1; Acceleration = 0; CurveRate = 0; Scale = 1; Behavior = BulletBehavior.Normal;
        Timer = 0; Chain = 0; Prev = -1; Style = BulletStyle.Fireball; Power = 1; Pierce = 0; BurstRadius = 0; BurstScale = 1; LastHit = -1;
        Zoom = 1; Pull = Vector2.Zero; Fuse = 0; LaunchSpeed = 0; LaunchAngle = 0; SplitCount = 0; SplitSpread = 360; SplitSpeed = 0; ChildScale = 1; SplitAimed = false; ChildStyle = BulletStyle.Fireball; Bounces = 0; LaunchAccel = 0; LinkIndex = 0; Blink = 0; Cruise = 0; TurnMode = 0; TurnTimes = 0; TurnCount = 0; TurnInterval = 0; TurnRotation = 0; TurnSpeed = 0; TurnBase = 0; TurnHeading = 0; PopSpeed = 0; PopTime = 0; PopLeft = 0; ExFor = 0; ExClock = 0; Anchor = Vector2.Zero; Radius = 0; RadialSpeed = 0; RadialAcceleration = 0; Theta = 0; Omega = 0;
    }
}
