using System;
using Godot;

/// <summary>Moves the boat (with the wind) and the boss (patrolling, standing, dashing or gliding between attacks).</summary>
public static partial class MovementSystem
{
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

    public static void MoveBoss(BossEntity boss, Rect2 bounds, float delta, Vector2? player = null)
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

            case BossMovementType.Roam:
                RoamStep(boss, bounds, delta);
                break;

            case BossMovementType.Dive:
                DiveStep(boss, bounds, delta, player ?? new Vector2(300, 600));
                break;

            case BossMovementType.Sweep:
                SweepStep(boss, bounds, delta);
                break;

            case BossMovementType.Cross:
                CrossStep(boss, delta);
                break;

            case BossMovementType.Dash:
                DashStep(boss, bounds, delta);
                break;
        }
        // a boss that stands on the horizon never goes above its line (to the sides and down only)
        if (boss.MovementType != BossMovementType.Transition && boss.Health.Lives > 0
            && boss.Definition.PhaseFor(boss.Health.Lives).MinY is float floor && boss.Transform.Position.Y < floor)
            boss.Transform.Position.Y = floor;
    }
}
