using System;
using Godot;

/// <summary>Movement of the bosses: the dash.</summary>
public static partial class MovementSystem
{
    /// <summary>The dash: seconds spent drifting down before lunging (she stops where the other bosses stand, her feet on the horizon of the water, never above it), the lunge's speed (times the boss's own), and the three lanes it comes back in: left, centre, right.</summary>
    /// <summary>The boss waits at least five seconds, still, before every lunge (the drift down from the top takes about two of them; the rest is spent standing still), on every difficulty.</summary>
    private const float DashWaitSeconds = 3.6f, DashWaitY = Layout.BossY, DashSpeedFactor = 2.5f, DashStartY = -50;
    private static readonly float[] DashLanes = { 100, 300, 500 };   // left, centre, right

    /// <summary>The dash: drifts down from the top, lunges down the screen, and comes back at the top in another lane.</summary>
    private static void DashStep(BossEntity boss, Rect2 bounds, float delta)
    {
        boss.Dash.Timer += delta;

        if (boss.Dash.Timer < DashWaitSeconds)
        {
            // Wait phase: drift down slowly from the top before lunging.
            Vector2 target = new Vector2(boss.Transform.Position.X, DashWaitY);
            boss.Transform.Position = boss.Transform.Position.MoveToward(target, boss.Movement.Speed * 0.5f * delta);
        }
        else
        {
            // Attack phase: plunge straight down.
            boss.Movement.Velocity = new Vector2(0, boss.Movement.Speed * DashSpeedFactor);
            boss.Transform.Position += boss.Movement.Velocity * delta;

            // Screen wrap: past the bottom, reappear at the top in another lane.
            if (boss.Transform.Position.Y >= bounds.End.Y + boss.Hitbox.Radius)
            {
                boss.Dash.Timer = 0;
                boss.Dash.LastX = NextDashLane(boss.Dash.LastX);
                boss.Transform.Position = new Vector2(boss.Dash.LastX, DashStartY);
            }
        }
    }

    /// <summary>A lane at random, but not the one the last dash came from.</summary>
    public static float NextDashLane(float last)
    {
        float lane;
        do lane = DashLanes[Random.Shared.Next(DashLanes.Length)];
        while (MathF.Abs(lane - last) < 1);
        return lane;
    }
}
