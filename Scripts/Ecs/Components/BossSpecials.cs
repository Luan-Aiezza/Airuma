using System;
using Godot;

/// <summary>
/// What a boss attack does besides firing bullets, as data (see <see cref="SpecialsSystem"/>):
///   slashes   lines that cut down the field, shown first as a thin warning and then striking, serrated and red (the claws of a bear);
///   a quake   the boss stamps and the whole screen shakes for a while (the emitters marked <c>QuakeOnly</c> fire only then: the ceiling falling);
///   a roar    the boss roars and the boat is pushed away from her.
/// </summary>
public struct SlashSpec
{
    public bool IsActive;
    /// <summary>Seconds between one volley of slashes and the next.</summary>
    public float Interval;
    /// <summary>Seconds of warning before they strike, and seconds that the strike lasts.</summary>
    public float Telegraph, Strike;
    /// <summary>Width of a slash, and the distance between the parallel slashes of a volley (px).</summary>
    public float Width, Spacing;
    /// <summary>How many parallel slashes in a volley.</summary>
    public int Count;
    /// <summary>The lean of the slashes from straight down, in degrees (positive: the bottom goes to the right).</summary>
    public float Angle;
    public SlashAim Aim;
    /// <summary>After each cut, the line where it cut lets go of a trail of bullets that spread out.</summary>
    public bool Trail;
}

public struct QuakeSpec
{
    public bool IsActive;
    /// <summary>Seconds from the end of one quake to the start of the next, and seconds a quake lasts.</summary>
    public float Interval, Duration;
    /// <summary>How hard the screen shakes (1: a few px).</summary>
    public float Intensity;
}

/// <summary>
/// A moon that advances from the boss to `Target` (in `Arrive` seconds) and stays there; every `Interval` seconds it warns for `Warn` seconds (it swells and glows) and then pulls the boat towards it for
/// `Duration` seconds, up to `Pull` px/s (the boat, at 400 px/s, can pull away; held to the slow speed it cannot). Touching the moon hurts.
/// </summary>
public struct GravitySpec
{
    public bool IsActive;
    public float Interval, Warn, Duration, Pull, Arrive, Radius;
    public Vector2 Target;
}

/// <summary>What the scene needs to draw the moon of a gravity attack: where it is, how big, how near its pull (Warning 0...1, Pull 0...1 while it pulls).</summary>
public struct MoonData
{
    public bool Active;
    public Vector2 Position;
    public float Radius, Warning, Pull;
}

public struct RoarSpec
{
    public bool IsActive;
    public float Interval;
    /// <summary>Speed (px/s) with which the boat is pushed at the start of the roar, and how long the push lasts.</summary>
    public float Push, Seconds;
}

/// <summary>One slash of the claws, in the world: where its line begins (above the field), which way it goes, and how old it is.</summary>
public struct SlashData
{
    public Vector2 Top;
    public float Angle;      // radians from straight down
    public float Age, Telegraph, Strike, Width;
    /// <summary>The first of its volley (it is the one that makes the sound).</summary>
    public bool Leader;
    public bool Struck;
    /// <summary>How long the line is (px): 1300 down the field; a cut from any direction is long enough to cross the whole field from wherever it passes through.</summary>
    public float Len;
    public bool Trail;
    public bool Trailed;
    /// <summary>A laser, not a cut of claws: it is drawn as a beam of the colour `Tint`, it may turn about its origin (`Spin`, radians a second) and may keep to the boss (`Follow`, at `Offset` from it).</summary>
    public bool Beam;
    public Color Tint;
    public float Spin;
    /// <summary>A laser that goes transparent (and harmless) for the last `GhostFor` seconds of every `Blink` seconds, so that the boat may cross it. 0: never.</summary>
    public float Blink;
    public const float GhostFor = 0.8f;
    public bool Follow;
    public Vector2 Offset;

    public float TotalLength => Len > 0 ? Len : Length;
    public Vector2 Direction => new Vector2(MathF.Sin(Angle), MathF.Cos(Angle));
    public Vector2 Bottom => Top + Direction * TotalLength;
    public const float Length = 1300;
    public bool Warning => Age < Telegraph;
    public bool Ghost => Beam && Blink > 0 && Age >= Telegraph && (Age - Telegraph) % Blink >= Blink - GhostFor;
    public bool Striking => Age >= Telegraph && Age < Telegraph + Strike && !Ghost;
    public const float Fade = 0.35f;
    public bool Gone => Age >= Telegraph + Strike + Fade;
}

/// <summary>The running state of a boss's specials: its clocks, and the specs of the attack in progress.</summary>
public sealed class BossSpecials
{
    public SlashSpec Slashes;
    public QuakeSpec Quake;
    public RoarSpec Roar;
    public float SlashClock, QuakeClock, RoarClock;
    public GravitySpec Gravity;
    /// <summary>The moon of a gravity attack: its age, where it is, the clock to its next pull, the warning and the pull left.</summary>
    public float MoonAge, GravityClock, WarnLeft, PullLeft;
    public Vector2 MoonStart, MoonPos;
    public bool MoonStarted;
    /// <summary>Seconds of quake left (above 0: the screen shakes), and of the push of the roar.</summary>
    public float QuakeLeft, RoarLeft;
    public int Volleys;

    public bool Any => Slashes.IsActive || Quake.IsActive || Roar.IsActive || Gravity.IsActive;
}
