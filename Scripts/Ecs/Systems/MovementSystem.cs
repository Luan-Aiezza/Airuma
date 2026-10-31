using System;
using Godot;

/// <summary>Moves the boat (with the wind) and the boss (patrolling, standing, dashing or gliding between attacks).</summary>
public static class MovementSystem
{
    /// <summary>The dash: seconds spent drifting down before lunging, the lunge's speed (times the boss's own), and the lanes it comes back in.</summary>
    private const float DashWaitSeconds = 1.5f, DashWaitY = 100, DashSpeedFactor = 5, DashStartY = -50;
    private static readonly float[] DashLanes = { 100, 200, 300, 400, 500 };

    /// <summary>The patrol: seconds the boss stays at a station, and the least a leg of the patrol goes (so that each is easy to follow).</summary>
    public const float PatrolPauseSeconds = 1.3f, PatrolMinLeg = 200;

    /// <summary>The boat moves at this share of its speed while focusing.</summary>
    public const float FocusSpeedFactor = 0.4f;

    public static void MovePlayer(PlayerEntity player, Rect2 bounds, float delta, Vector2 wind)
    {
        float speed = player.Movement.Speed * (player.Input.IsFocused ? FocusSpeedFactor : 1);
        player.Movement.Velocity = player.Input.Direction * speed;

        // Total velocity = player movement + (wind * modifier)
        Vector2 total = player.Movement.Velocity + wind * player.WindModifier.Value;
        player.Transform.Position += total * delta;

        player.Transform.Position.X = MathF.Min(MathF.Max(player.Transform.Position.X, bounds.Position.X), bounds.End.X);
        player.Transform.Position.Y = MathF.Min(MathF.Max(player.Transform.Position.Y, bounds.Position.Y), bounds.End.Y);
    }

    public static void MoveBoss(BossEntity boss, Rect2 bounds, float delta)
    {
        switch (boss.MovementType)
        {
            case BossMovementType.Transition:
                // Glide to the target while the boss is not attacking.
                boss.Transform.Position = boss.Transform.Position.MoveToward(boss.TargetPosition,
                    (boss.TransitionSpeed > 0 ? boss.TransitionSpeed : boss.Movement.Speed) * delta);
                boss.TransitionTimer -= delta;
                if (boss.TransitionTimer <= 0)
                {
                    boss.MovementType = boss.NextMovementType;
                    boss.TransitionSpeed = 0;
                }
                break;

            case BossMovementType.Stationary:
                break;

            case BossMovementType.Patrol:
                PatrolStep(boss, bounds, delta);
                break;

            case BossMovementType.Dash:
                boss.DashTimer += delta;

                if (boss.DashTimer < DashWaitSeconds)
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

                    // Screen wrap: past the bottom, reappear at the top in a random lane.
                    if (boss.Transform.Position.Y >= bounds.End.Y + boss.Hitbox.Radius)
                    {
                        boss.DashTimer = 0;
                        boss.LastDashX = NextDashLane(boss.LastDashX);
                        boss.Transform.Position = new Vector2(boss.LastDashX, DashStartY);
                    }
                }
                break;
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

    /// <summary>
    /// The boss glides to a station on one side of the field, stays there a while, then goes to one on the other side. Moving in
    /// legs with a rest between them is much easier to follow than moving all the time.
    /// </summary>
    private static void PatrolStep(BossEntity boss, Rect2 bounds, float delta)
    {
        if (boss.PatrolWait > 0)
        {
            boss.PatrolWait -= delta;
            return;
        }
        float margin = boss.Hitbox.Radius;
        float left = bounds.Position.X + margin, right = bounds.End.X - margin;
        float middle = (bounds.Position.X + bounds.End.X) / 2;
        if (!boss.PatrolHasTarget)
        {
            // To the other side of the middle than the one it is on; a station at least a good leg away.
            bool goRight = boss.Transform.Position.X < middle - 1 || (MathF.Abs(boss.Transform.Position.X - middle) <= 1 && boss.PatrolDirection > 0);
            float lo = goRight ? middle + 60 : left + 20, hi = goRight ? right - 20 : middle - 60;
            float x;
            int tries = 0;
            do x = lo + (hi - lo) * (float)Random.Shared.NextDouble();
            while (MathF.Abs(x - boss.Transform.Position.X) < PatrolMinLeg && ++tries < 12);
            boss.PatrolTargetX = Math.Clamp(x, left, right);
            boss.PatrolDirection = goRight ? -1 : 1;           // the side it will go to next
            boss.PatrolHasTarget = true;
        }
        var position = boss.Transform.Position;
        position.X = Mathf.MoveToward(position.X, boss.PatrolTargetX, boss.Movement.Speed * delta);
        boss.Transform.Position = position;
        if (MathF.Abs(position.X - boss.PatrolTargetX) < 0.5f)
        {
            boss.PatrolHasTarget = false;
            boss.PatrolWait = PatrolPauseSeconds;
        }
    }
}
