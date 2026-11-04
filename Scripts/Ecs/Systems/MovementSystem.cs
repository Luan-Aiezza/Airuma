using System;
using Godot;

/// <summary>Moves the boat (with the wind) and the boss (patrolling, standing, dashing or gliding between attacks).</summary>
public static partial class MovementSystem
{
    /// <summary>The boat moves at this share of its speed while focusing.</summary>
    public const float FocusSpeedFactor = 0.4f;

    /// <summary>The player's reach beyond the edge of a solid thing: the boat's hull.</summary>
    public const float BoatRadius = 14;

    /// <summary>Whether the point is within the platform's rock: its top (an ellipse), its foot (another, lower) and the wall between them; grown by `grow`.</summary>
    public static bool InsidePlatform(World.Platform p, Vector2 at, float grow)
    {
        float rx = p.Rx + grow, ry = p.Ry + grow;
        bool Ellipse(float cy) { float dx = (at.X - p.Centre.X) / rx, dy = (at.Y - cy) / ry; return dx * dx + dy * dy <= 1; }
        if (Ellipse(p.Centre.Y) || Ellipse(p.Centre.Y + p.Wall)) return true;
        return MathF.Abs(at.X - p.Centre.X) <= rx && at.Y >= p.Centre.Y && at.Y <= p.Centre.Y + p.Wall;
    }

    /// <summary>Keeps the boat out of the platform: it slides along the rock (each axis on its own), and if the rock comes down onto it, it is pushed out the nearest way.</summary>
    private static void AvoidPlatform(PlayerEntity player, World.Platform? platform, Vector2 before)
    {
        if (platform is not World.Platform p) return;
        ref Vector2 pos = ref player.Transform.Position;
        if (!InsidePlatform(p, pos, BoatRadius)) return;
        if (!InsidePlatform(p, before, BoatRadius))
        {
            var target = pos;
            pos = new Vector2(target.X, before.Y);
            if (InsidePlatform(p, pos, BoatRadius)) pos = new Vector2(before.X, before.Y);
            var withY = new Vector2(pos.X, target.Y);
            if (!InsidePlatform(p, withY, BoatRadius)) pos = withY;
            return;
        }
        // already inside: out by the nearest of four ways
        Vector2 best = pos; float bestDistance = float.MaxValue;
        foreach (var dir in new[] { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1) })
            for (float d = 2; d < 500; d += 2)
            {
                var q = pos + dir * d;
                if (!InsidePlatform(p, q, BoatRadius)) { if (d < bestDistance) { bestDistance = d; best = q; } break; }
            }
        pos = best;
    }

    public static void MovePlayer(PlayerEntity player, Rect2 bounds, float delta, Vector2 wind, World.Platform? platform = null)
    {
        var before = player.Transform.Position;
        float speed = player.Movement.Speed * (player.Input.IsFocused ? FocusSpeedFactor : 1);
        player.Movement.Velocity = player.Input.Direction * speed;

        // Total velocity = player movement + (wind * modifier)
        Vector2 total = player.Movement.Velocity + wind * player.WindModifier.Value;
        player.Transform.Position += total * delta;

        player.Transform.Position.X = MathF.Min(MathF.Max(player.Transform.Position.X, bounds.Position.X), bounds.End.X);
        player.Transform.Position.Y = MathF.Min(MathF.Max(player.Transform.Position.Y, bounds.Position.Y), bounds.End.Y);
        AvoidPlatform(player, platform, before);
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

            case BossMovementType.Shuttle:
                ShuttleStep(boss, delta);
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
