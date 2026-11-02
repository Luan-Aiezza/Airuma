using System;
using Godot;

/// <summary>The state of the dance of the two bodies of a boss that is two: who is the one that moves, and where it is in its leg.</summary>
public sealed class TwinDance
{
    /// <summary>0: the boss moves and the twin is parked; 1: the other way round.</summary>
    public int Mover;
    public bool Returning, HasTarget;
    public float Wait = 1.5f;
    public Vector2 Target;
    public void Reset() { Mover = 0; Returning = false; HasTarget = false; Wait = 1.5f; }
}

public static partial class MovementSystem
{
    /// <summary>How far down the screen the one that moves may go (a share of the field's height), the pause at the end of a leg, the stillness (both parked) before the next one leaves, so the boat can take position and attack, and its speed as a share of the boss's own.</summary>
    public const float DanceBottom = 0.62f, DanceLegPause = 1.0f, DanceParkedPause = 5.0f, DanceSpeedFactor = 1.1f;

    /// <summary>
    /// The two bodies take turns: one of them stays parked at its place at the top, where the boat can hit it, while the other goes anywhere on the screen; when the one that moves
    /// is back at its place, the other leaves.
    /// </summary>
    public static void DanceStep(TwinDance dance, BossEntity boss, BossEntity twin, Vector2 bossHome, Vector2 twinHome, Rect2 bounds, float delta)
    {
        var mover = dance.Mover == 0 ? boss : twin;
        var parked = dance.Mover == 0 ? twin : boss;
        var home = dance.Mover == 0 ? bossHome : twinHome;
        if (dance.Wait > 0) { dance.Wait -= delta; return; }
        float speed = mover.Movement.Speed * DanceSpeedFactor * delta;
        if (!dance.Returning)
        {
            if (!dance.HasTarget)
            {
                float m = mover.Hitbox.Radius;
                Vector2 target;
                int tries = 0;
                do target = new Vector2(bounds.Position.X + m + 20 + (bounds.Size.X - 2 * m - 40) * (float)Random.Shared.NextDouble(),
                                        Layout.BossY + (bounds.Size.Y * DanceBottom - Layout.BossY) * (float)Random.Shared.NextDouble());   // never above the horizon: to the sides (at the height of its place) or below it
                while ((target.DistanceTo(parked.Transform.Position) < 100 || target.DistanceTo(mover.Transform.Position) < 120) && ++tries < 20);
                dance.Target = target;
                dance.HasTarget = true;
            }
            mover.Transform.Position = mover.Transform.Position.MoveToward(dance.Target, speed);
            if (mover.Transform.Position.DistanceTo(dance.Target) < 0.5f) { dance.Returning = true; dance.Wait = DanceLegPause; }
        }
        else
        {
            mover.Transform.Position = mover.Transform.Position.MoveToward(home, speed);
            if (mover.Transform.Position.DistanceTo(home) < 0.5f) { dance.Returning = false; dance.HasTarget = false; dance.Mover = 1 - dance.Mover; dance.Wait = DanceParkedPause; }
        }
    }
}
