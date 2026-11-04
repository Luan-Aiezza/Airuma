using System;
using Godot;

/// <summary>Movement of the bosses: the dive.</summary>
public static partial class MovementSystem
{
    /// <summary>
    /// The dive: seconds on the surface (drifting back up to its place), sinking, under the water (the shadow lies under the boat and follows it
    /// at first, then holds still), and coming out of the water at the shadow, the sinking played backwards.
    /// </summary>
    public const float DiveSurfaceSeconds = 6.0f, DiveSinkSeconds = 0.5f, DiveUnderSeconds = 1.8f, DiveFollowShare = 0.4f, DiveTopY = 245, DiveDriftSpeed = 220, DiveSinkDepth = 45;

    private static void DiveStep(BossEntity boss, Rect2 bounds, float delta, Vector2 player)
    {
        float r = boss.Hitbox.Radius;
        boss.Dive.Clock += delta;
        switch (boss.Dive.State)
        {
            case DiveState.Surface:
                // After coming out low in the field, it swims back up to its place while it still shoots.
                boss.Transform.Position = boss.Transform.Position.MoveToward(new Vector2(boss.Transform.Position.X, DiveTopY), DiveDriftSpeed * delta);
                if (boss.Dive.Clock >= DiveSurfaceSeconds) { boss.Dive.State = DiveState.Sinking; boss.Dive.Clock = 0; }
                break;
            case DiveState.Sinking:
                boss.Transform.Position += new Vector2(0, DiveSinkDepth / DiveSinkSeconds * delta);
                if (boss.Dive.Clock >= DiveSinkSeconds)
                {
                    boss.Dive.State = DiveState.Under; boss.Dive.Clock = 0; boss.Dive.Submerged = true;
                    FollowShadow(boss, bounds, player);
                }
                break;
            case DiveState.Under:
                if (boss.Dive.Clock < DiveUnderSeconds * DiveFollowShare) FollowShadow(boss, bounds, player);
                if (boss.Dive.Clock >= DiveUnderSeconds)
                {
                    boss.Dive.State = DiveState.Rising; boss.Dive.Clock = 0; boss.Dive.Submerged = false; boss.Dive.Emerged = true;
                    boss.Transform.Position = new Vector2(boss.Dive.ExitX, boss.Dive.ExitY + DiveSinkDepth);
                }
                break;
            case DiveState.Rising:
                // The sinking backwards: it comes up out of the water, growing back to its size (the scene draws the growth).
                boss.Transform.Position = new Vector2(boss.Dive.ExitX, boss.Dive.ExitY + DiveSinkDepth * (1 - Math.Clamp(boss.Dive.Clock / DiveSinkSeconds, 0, 1)));
                if (boss.Dive.Clock >= DiveSinkSeconds)
                {
                    boss.Transform.Position = new Vector2(boss.Dive.ExitX, boss.Dive.ExitY);
                    boss.Dive.State = DiveState.Surface; boss.Dive.Clock = 0;
                }
                break;
        }
    }

    /// <summary>The shadow goes under the boat.</summary>
    private static void FollowShadow(BossEntity boss, Rect2 bounds, Vector2 player)
    {
        float r = boss.Hitbox.Radius;
        boss.Dive.ExitX = Math.Clamp(player.X, bounds.Position.X + r, bounds.End.X - r);
        boss.Dive.ExitY = Math.Clamp(player.Y, bounds.Position.Y + r + 60, bounds.End.Y - r);
    }
}
