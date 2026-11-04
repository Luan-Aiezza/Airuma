using System;
using Godot;

/// <summary>
/// What a pattern of bullets is given each time it fires: where the boss is, where the boat is, the field, the time of the attack and how many times this pattern has fired. A pattern is a
/// function of these only (nothing it remembers), so it is the same however often it is run, and its figures can be drawn on paper.
/// </summary>
public sealed class PatternContext
{
    public BulletData[] Pool;
    /// <summary>The lines of the boss's cuts and lasers (a laser is one of these, drawn as a beam).</summary>
    public System.Collections.Generic.List<SlashData> Slashes;
    /// <summary>The body the bullets come from (the boss, or its twin).</summary>
    public Vector2 Origin;
    public Vector2 Player;
    public Rect2 Field;
    public Difficulty Difficulty;
    /// <summary>How much heavier than written the attack is made (1: as written): the counts grow by its square root and the patterns fire that much more often. A knob to tune the bullets on the screen.</summary>
    public float Pressure = 1;
    /// <summary>How much faster than written the bullets fly (1: as written): each boss is a little quicker than the one before.</summary>
    public float Speed = 1;
    /// <summary>Seconds since the pattern began (after its delay).</summary>
    public float Time;
    /// <summary>How many times it has fired in all, and in the present burst (a pattern with a rhythm of on and off), and the number of the burst.</summary>
    public int Count, Step, Burst;

    public float Left => Field.Position.X;
    public float Right => Field.End.X;
    public float Top => 0;
    public float Bottom => Field.End.Y;
    public float MiddleX => Field.Position.X + Field.Size.X / 2;
    public float Width => Field.Size.X;

    /// <summary>Degrees from the origin to the boat.</summary>
    public float Aim => AimFrom(Origin);

    public float AimFrom(Vector2 from) => Mathx.Degrees(MathF.Atan2(Player.Y - from.Y, Player.X - from.X));

    /// <summary>`count` bullets scaled for the difficulty (never fewer than `min`).</summary>
    public int N(int count, int min = 1)
    {
        int n = Difficulty.Scale(count, min);
        return Pressure == 1 ? n : Math.Max(min, (int)MathF.Round(n * MathF.Sqrt(Pressure)));
    }

    /// <summary>The width of a gap for the difficulty: wider in the easy mode, narrower in the hard.</summary>
    public float Gap(float width) => width * (Difficulty == Difficulty.Easy ? 1.35f : (Difficulty == Difficulty.Hard ? 0.88f : 1f));

    public static float Rand(float low, float high) => low + (float)Random.Shared.NextDouble() * (high - low);

    /// <summary>True when no enemy bullet within `band` px of `at` (along the Y axis) is closer than `distance` to it.</summary>
    public bool IsFree(Vector2 at, float distance, float band)
    {
        float d2 = distance * distance;
        for (int i = 0; i < Pool.Length; i++)
        {
            ref var b = ref Pool[i];
            if (!b.IsActive || b.Faction != Faction.Boss) continue;
            float dy = b.Position.Y - at.Y, dx = b.Position.X - at.X;
            if (MathF.Abs(dy) <= band && dx * dx + dy * dy < d2) return false;
        }
        return true;
    }

    public int Fire(Vector2 at, float angle, in Shot shot)
    {
        if (Speed == 1) return BulletSystem.Fire(Pool, at, angle, shot);
        var s = shot;
        s.Speed *= Speed; s.LaunchSpeed *= Speed; s.SplitSpeed *= Speed; s.Accel *= Speed; s.LaunchAccel *= Speed; s.Pull *= Speed;
        return BulletSystem.Fire(Pool, at, angle, s);
    }

    public int FirePolar(Vector2 anchor, float radius, float radialSpeed, float theta, float omega, in Shot shot, float radialAccel = 0)
        => BulletSystem.FirePolar(Pool, anchor, radius, radialSpeed * Speed, theta, omega, shot, radialAccel * Speed);
}

/// <summary>One pattern of an attack: how often it fires, after how long it begins, whether it comes in bursts, and the function that fires it. An attack has as many as it likes at once.</summary>
public sealed class PatternSpec
{
    public float Interval = 1;
    public float Delay;
    /// <summary>It fires for `OnFor` seconds and then rests `OffFor` seconds, again and again (infinite `OnFor`: it never rests).</summary>
    public float OnFor = float.PositiveInfinity, OffFor;
    /// <summary>It comes from the second body of a boss that is two.</summary>
    public bool FromTwin;
    /// <summary>It fires only when the boss lets go of its jet (<see cref="BossBrains"/> sweep: the moment the beam goes on), `BeamDelay` seconds after, once for each jet.</summary>
    public bool OnBeam;
    public float BeamDelay;
    /// <summary>It only fires while the boss's quake is on.</summary>
    public bool QuakeOnly;
    /// <summary>Not in the easy mode / only in the hard mode.</summary>
    public bool NotEasy, HardOnly;
    public Action<PatternContext> Fire;

    public PatternSpec After(float seconds) { Delay = seconds; return this; }
    public PatternSpec InBursts(float on, float off) { OnFor = on; OffFor = off; return this; }
    public PatternSpec WithTheJet(float delay = 0) { OnBeam = true; BeamDelay = delay; return this; }
    public PatternSpec Twin() { FromTwin = true; return this; }
    public PatternSpec OnQuake() { QuakeOnly = true; return this; }
    public PatternSpec NotOnEasy() { NotEasy = true; return this; }
    public PatternSpec OnlyOnHard() { HardOnly = true; return this; }

    public bool Applies(Difficulty difficulty) => !(NotEasy && difficulty == Difficulty.Easy) && !(HardOnly && difficulty != Difficulty.Hard);
}

/// <summary>The state of a pattern while its attack lasts.</summary>
public struct PatternRun
{
    public PatternSpec Spec;
    public float Clock;
    public int Shots, BurstShots, Burst;
    public float BeamClock;
    public bool BeamDone;
    public float NextAt;
}
