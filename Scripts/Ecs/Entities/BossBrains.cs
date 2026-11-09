using Godot;

// The state of each way a boss can move (see MovementSystem). A boss has one of each, and a new kind of movement adds one here, its own file of
// MovementSystem.*.cs, and a case in MoveBoss. Every brain forgets what it was doing when a new attack begins (`Reset`).

/// <summary>The patrol goes from station to station and stays a while at each: where it is going, and how long it still waits.</summary>
public sealed class PatrolBrain
{
    public float Direction = 1, TargetX, Wait;
    public bool HasTarget;
    public void Reset() { HasTarget = false; Wait = 0; }
}

/// <summary>The roam goes to a point anywhere in the upper field, rests there, and picks another.</summary>
public sealed class RoamBrain
{
    public Vector2 Target;
    public bool HasTarget;
    public float Wait;
    public void Reset() { HasTarget = false; Wait = 0; }
}

/// <summary>The dash lunges down the screen and comes back at the top; it never comes from the same lane twice running.</summary>
public sealed class DashBrain
{
    public float Timer, LastX = 300;
    public void Reset(float startX) { Timer = 0; LastX = startX; }
}

/// <summary>
/// The dive: the step it is on, the time in it, and where it will come out (the shadow). Under the water the boss can neither hurt nor be hurt,
/// nor fire; the moment it comes out is marked once (`Emerged`) so that the splash is fired once.
/// </summary>
public sealed class DiveBrain
{
    public DiveState State = DiveState.Surface;
    public float Clock, ExitX = 300, ExitY = 500;
    public bool Submerged, Emerged;
    public void Reset() { State = DiveState.Surface; Clock = 0; Submerged = false; }
}

/// <summary>The sweep: the step it is on, the time in it, and the jet (a warning line first, then the jet itself).</summary>
public sealed class SweepBrain
{
    /// <summary>Half the width of the jet.</summary>
    public const float BeamHalfWidth = 75;

    public SweepState State = SweepState.ToLeft;
    public float Clock;
    public bool BeamWarn, BeamOn;
    /// <summary>The difficulty: how wide the gaps left at the edges are (easy more, hard less), and how fast the jet crosses.</summary>
    public float GapScale = 1, SpeedScale = 1;

    public void Reset(Difficulty difficulty)
    {
        State = SweepState.ToLeft; Clock = 0; BeamWarn = BeamOn = false;
        GapScale = difficulty.Pick(1f, 0.8f, 0.65f);
        SpeedScale = difficulty.Pick(1f, 1.15f, 1.3f);
    }
}

/// <summary>The cross: fast legs from corner to corner of the field (diagonal and straight, in turn), with a rest at the end of each.</summary>
public sealed class CrossBrain
{
    /// <summary>The corners it goes to, in order: top left, bottom right (a diagonal), top right (straight up), bottom left (a diagonal), back to the top left.</summary>
    public static readonly Vector2[] Corners = { new Vector2(90, 120), new Vector2(510, 420), new Vector2(510, 120), new Vector2(90, 420) };

    public int Next;
    public float Rest;
    public void Reset() { Next = 0; Rest = 0.6f; }
}
