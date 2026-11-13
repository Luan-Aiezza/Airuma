using System;
using Godot;

/// <summary>
/// Everything a bullet of the enemy side can be, in one value: where, how fast and at what angle it leaves, what it looks like, and how it moves afterwards. The patterns of the later bosses and
/// spirits are made of these (see <see cref="Pat"/>): a shot is made once and fired many times, changed a little each time with the `With` methods.
/// </summary>
public struct Shot
{
    public BulletStyle Style;
    public float Speed;
    /// <summary>Scale of the hitbox (5 px of radius for each) unless <see cref="Hit"/> is set.</summary>
    public float Scale;
    /// <summary>The radius of the hitbox in px (0: 5 x the scale).</summary>
    public float Hit;
    /// <summary>Enlargement of the picture (1: as drawn).</summary>
    public float Zoom;
    public float Accel, Curve;
    public BulletBehavior Behavior;
    public Vector2 Pull;
    public float Fuse, LaunchSpeed, LaunchAngle, LaunchAccel;
    public int SplitCount;
    public float SplitSpread, SplitSpeed, ChildScale;
    public bool SplitAimed;
    public BulletStyle ChildStyle;
    public int Bounces;
    public float WindFactor;
    /// <summary>For a Launch whose heading is "away from this point" (a figure that flies as one and then bursts outward from its centre).</summary>
    public Vector2 Anchor;
    public float Blink;
    public bool IsInert;
    public float Cruise;
    public int TurnMode, TurnTimes;
    public float TurnInterval, TurnRotation, TurnSpeed;
    public float PopSpeed, PopTime, ExFor;

    /// <summary>How much of the wind the bullets of the patterns feel (the old emitters keep 0.5): a little, so that the wind is still felt, but not so much that a rain of bullets leaves a void at one side of the field.</summary>
    public const float DefaultWind = 0.15f;

    public Shot(BulletStyle style, float speed)
    {
        Style = style; Speed = speed; Scale = 1; Hit = 0; Zoom = 1; Accel = 0; Curve = 0; Behavior = BulletBehavior.Normal; Pull = Vector2.Zero;
        Fuse = 0; LaunchSpeed = 0; LaunchAngle = float.NaN; LaunchAccel = 0; SplitCount = 0; SplitSpread = 360; SplitSpeed = 0; ChildScale = 1; SplitAimed = false; ChildStyle = style; Bounces = 0; WindFactor = DefaultWind;
    }

    /// <summary>The wind does not move it: the figures that make a way (walls with a gap, lattices, combs, crescents, polygons, stars, constellations) must stay where they were drawn.</summary>
    /// <summary>The bullet cannot hurt the boat (and is drawn faded) until its first turn: bullets that rise and stop before they fall on the boat.</summary>
    public Shot Inert() { var s = this; s.IsInert = true; return s; }
    public Shot Steady() { var s = this; s.WindFactor = 0; return s; }
    public Shot Sized(float scale) { var s = this; s.Scale = scale; return s; }
    public Shot Speeded(float speed) { var s = this; s.Speed = speed; return s; }
    public Shot Accelerating(float accel) { var s = this; s.Accel = accel; return s; }
    public Shot Curving(float degreesPerSecond) { var s = this; s.Curve = degreesPerSecond; return s; }
    public Shot Styled(BulletStyle style) { var s = this; s.Style = style; return s; }
    public Shot Zoomed(float zoom) { var s = this; s.Zoom = zoom; return s; }
    /// <summary>The hitbox has this radius in px, whatever the picture.</summary>
    public Shot Hitting(float radius) { var s = this; s.Hit = radius; return s; }
    public Shot Dancing(float curve) { var s = this; s.Behavior = BulletBehavior.Dance; s.Curve = curve; return s; }
    public Shot Zigzagging(float curve) { var s = this; s.Behavior = BulletBehavior.Zigzag; s.Curve = curve; return s; }
    /// <summary>Gravity: the bullet is pulled by `pull` (px/s each second), so that it flies in an arc.</summary>
    public Shot Pulled(Vector2 pull) { var s = this; s.Pull = pull; return s; }
    /// <summary>Stays (braking by its acceleration) for `fuse` seconds and then goes off at `speed`, along `angle` (NaN: at the boat).</summary>
    public Shot Launched(float fuse, float speed, float angle = float.NaN, float accel = 0) { var s = this; s.Behavior = BulletBehavior.Launch; s.Fuse = fuse; s.LaunchSpeed = speed; s.LaunchAngle = angle; s.LaunchAccel = accel; return s; }
    /// <summary>Laid out in place (barely moving, so that it keeps its heading) and, after `fuse` seconds, let go along the heading it was fired at, at `speed`: the figures that hold their shape and then burst outward.</summary>
    public Shot LaunchedOut(float fuse, float speed, float accel = 0) { var s = this; s.Speed = 1; s.Behavior = BulletBehavior.Launch; s.Fuse = fuse; s.LaunchSpeed = speed; s.LaunchAngle = float.PositiveInfinity; s.LaunchAccel = accel; return s; }
    /// <summary>Breaks into `count` bullets of `child` after `fuse` seconds: a ring, or a fan `spread` degrees wide (aimed if `aimed`).</summary>
    public Shot Splitting(float fuse, int count, BulletStyle child, float speed, float spread = 360, bool aimed = false, float childScale = 1) { var s = this; s.Behavior = BulletBehavior.Split; s.Fuse = fuse; s.SplitCount = count; s.ChildStyle = child; s.SplitSpeed = speed; s.SplitSpread = spread; s.SplitAimed = aimed; s.ChildScale = childScale; return s; }
    /// <summary>A star of a constellation: it flies out, braking, hangs a while, linked by thin lines (which hurt) to the others of its volley, and falls.</summary>
    public Shot Constellation(float brake) { var s = this; s.Behavior = BulletBehavior.Constellation; s.Accel = -MathF.Abs(brake); return s; }
    /// <summary>Flies as it was fired for `fuse` seconds and then bursts: every bullet of the figure goes off straight away from `centre` (where the centre of the figure is by then), at `speed`.</summary>
    public Shot Scattering(float fuse, float speed, Vector2 centre, float accel = 0) { var s = this; s.Behavior = BulletBehavior.Launch; s.Fuse = fuse; s.LaunchSpeed = speed; s.LaunchAngle = float.NegativeInfinity; s.LaunchAccel = accel; s.Anchor = centre; return s; }
    /// <summary>The lines of the constellation take turns to open: of every three, one is open at a time, each for `seconds`.</summary>
    public Shot Blinking(float seconds) { var s = this; s.Blink = seconds; return s; }
    /// <summary>It can be pushed (a roar shoves it hard), and settles back to its own speed afterwards.</summary>
    public Shot Heavy() { var s = this; s.Cruise = s.Speed; return s; }
    /// <summary>It slows evenly to a stop in `interval` seconds and then TURNS (`mode`: 0 by `rotation` degrees more, 1 towards the boat plus `rotation`, 2 to the heading `rotation`) and sets off at `speed` (0: at the speed it was fired); `times` times.</summary>
    public Shot Turning(int mode, float interval, int times, float rotation, float speed = 0) { var s = this; s.Behavior = BulletBehavior.Turn; s.TurnMode = mode; s.TurnInterval = interval; s.TurnTimes = times; s.TurnRotation = rotation; s.TurnSpeed = speed > 0 ? speed : s.Speed; return s; }
    /// <summary>It is shot out with `extra` px/s more than its speed, and the extra dies away in `seconds`: the little burst of a bullet leaving the boss.</summary>
    public Shot Popping(float extra = 300, float seconds = 0.27f) { var s = this; s.PopSpeed = extra; s.PopTime = seconds; return s; }
    /// <summary>Its curve and its acceleration last only `seconds`: after that it flies straight, at the speed it has by then.</summary>
    public Shot EasingOutAfter(float seconds) { var s = this; s.ExFor = seconds; return s; }
    public Shot Bouncing(int times) { var s = this; s.Behavior = BulletBehavior.Bounce; s.Bounces = times; return s; }
}

public static partial class BulletSystem
{
    /// <summary>Fires a shot from a point at an angle (degrees); returns its slot (-1 if the pool is full).</summary>
    public static int Fire(BulletData[] pool, Vector2 at, float angle, in Shot shot)
    {
        float a = Mathx.Radians(angle);
        int i = Spawn(pool, at, new Vector2(MathF.Cos(a), MathF.Sin(a)) * shot.Speed, Faction.Boss, shot.Style, scale: shot.Scale, acceleration: shot.Accel, curve: shot.Curve, behavior: shot.Behavior);
        if (i < 0) return -1;
        ref BulletData b = ref pool[i];
        if (shot.Hit > 0) b.HitboxRadius = shot.Hit;
        b.Zoom = shot.Zoom;
        b.Pull = shot.Pull;
        b.Fuse = shot.Fuse; b.LaunchSpeed = shot.LaunchSpeed; b.LaunchAngle = shot.LaunchAngle; b.LaunchAccel = shot.LaunchAccel;
        b.SplitCount = shot.SplitCount; b.SplitSpread = shot.SplitSpread; b.SplitSpeed = shot.SplitSpeed; b.ChildScale = shot.ChildScale; b.SplitAimed = shot.SplitAimed; b.ChildStyle = shot.ChildStyle;
        b.Bounces = shot.Bounces;
        b.Anchor = shot.Anchor;
        b.Blink = shot.Blink;
        b.Inert = shot.IsInert;
        b.Cruise = shot.Cruise;
        b.TurnMode = shot.TurnMode; b.TurnTimes = shot.TurnTimes; b.TurnInterval = shot.TurnInterval; b.TurnRotation = shot.TurnRotation; b.TurnSpeed = shot.TurnSpeed;
        b.TurnBase = shot.Speed; b.TurnHeading = Mathx.Radians(angle);
        b.PopSpeed = shot.PopSpeed; b.PopTime = shot.PopTime; b.PopLeft = shot.PopTime; b.ExFor = shot.ExFor;
        b.WindModifier = shot.WindFactor;
        return i;
    }

    /// <summary>
    /// A bullet on a polar track: at time t it is at `radius + radialSpeed t + radialAccel t^2 / 2` from `anchor`, at the angle `theta + omega t` (degrees, degrees/s). Arms of a spiral galaxy,
    /// the petals of a flower, wheels: many such bullets together draw the figure, and it keeps its shape however the boss moves.
    /// </summary>
    public static int FirePolar(BulletData[] pool, Vector2 anchor, float radius, float radialSpeed, float theta, float omega, in Shot shot, float radialAccel = 0)
    {
        int i = Fire(pool, anchor, theta, shot);
        if (i < 0) return -1;
        ref BulletData b = ref pool[i];
        b.Behavior = BulletBehavior.Polar;
        b.Anchor = anchor; b.Radius = radius; b.RadialSpeed = radialSpeed; b.RadialAcceleration = radialAccel; b.Theta = theta; b.Omega = omega;
        b.Timer = 0;
        float a = Mathx.Radians(theta);
        b.Position = anchor + new Vector2(MathF.Cos(a), MathF.Sin(a)) * radius;
        b.WindModifier = 0;
        return i;
    }
}
