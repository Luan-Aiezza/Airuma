using System;
using Godot;

/// <summary>Moves the boat (with the wind) and the boss (patrolling, standing, dashing or gliding between attacks).</summary>
public static class MovementSystem
{
    /// <summary>The dash: seconds spent drifting down before lunging, the lunge's speed (times the boss's own), and the lanes it comes back in.</summary>
    private const float DashWaitSeconds = 1.5f, DashWaitY = 100, DashSpeedFactor = 5, DashStartY = -50;
    private static readonly float[] DashLanes = { 100, 300, 500 };

    public static void MovePlayer(PlayerEntity player, Rect2 bounds, float delta, Vector2 wind)
    {
        player.Movement.Velocity = player.Input.Direction * player.Movement.Speed;

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
            {
                boss.Movement.Velocity = new Vector2(boss.PatrolDirection * boss.Movement.Speed, 0);
                boss.Transform.Position += boss.Movement.Velocity * delta;
                // The hitbox, not the centre, may reach the edge of the field.
                float margin = boss.Hitbox.Radius;
                if (boss.Transform.Position.X >= bounds.End.X - margin)
                    boss.PatrolDirection = -1;
                else if (boss.Transform.Position.X <= bounds.Position.X + margin)
                    boss.PatrolDirection = 1;
                break;
            }

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
                        boss.Transform.Position = new Vector2(DashLanes[Random.Shared.Next(DashLanes.Length)], DashStartY);
                    }
                }
                break;
        }
    }
}
