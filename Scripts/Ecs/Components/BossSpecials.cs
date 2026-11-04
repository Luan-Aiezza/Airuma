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

    public float TotalLength => Len > 0 ? Len : Length;
    public Vector2 Direction => new Vector2(MathF.Sin(Angle), MathF.Cos(Angle));
    public Vector2 Bottom => Top + Direction * TotalLength;
    public const float Length = 1300;
    public bool Warning => Age < Telegraph;
    public bool Striking => Age >= Telegraph && Age < Telegraph + Strike;
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
    /// <summary>Seconds of quake left (above 0: the screen shakes), and of the push of the roar.</summary>
    public float QuakeLeft, RoarLeft;
    public int Volleys;

    public bool Any => Slashes.IsActive || Quake.IsActive || Roar.IsActive;
}
