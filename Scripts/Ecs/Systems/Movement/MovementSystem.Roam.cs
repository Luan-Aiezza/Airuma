using System;
using Godot;

/// <summary>Movement of the bosses: the roam.</summary>
public static partial class MovementSystem
{
    /// <summary>The roam: where the boss may go, the pause between legs, the least a leg goes, and its speed as a share of the boss's own.</summary>
    public const float RoamTop = 90, RoamBottom = 330, RoamPauseSeconds = 3.0f, RoamMinLeg = 150, RoamSpeedFactor = 0.9f;

    /// <summary>Glides to a point anywhere in the upper field, rests a moment, and picks another: a dancer who goes where the music takes him.</summary>
    private static void RoamStep(BossEntity boss, Rect2 bounds, float delta)
    {
        if (boss.Roam.Wait > 0) { boss.Roam.Wait -= delta; return; }
        float m = boss.Hitbox.Radius;
        // the fused body is tall: it roams lower, so that its head stays clear of the boss bar
        bool fused = boss.Health.Lives > 0 && boss.Definition.PhaseFor(boss.Health.Lives).Fused;
        float top = fused ? RoamTop + 100 : RoamTop, bottom = fused ? RoamBottom + 60 : RoamBottom;
        if (!boss.Roam.HasTarget)
        {
            Vector2 target;
            int tries = 0;
            do target = new Vector2(bounds.Position.X + m + 20 + (bounds.Size.X - 2 * m - 40) * (float)Random.Shared.NextDouble(),
                                    top + (bottom - top) * (float)Random.Shared.NextDouble());
            while (target.DistanceTo(boss.Transform.Position) < RoamMinLeg && ++tries < 12);
            boss.Roam.Target = target;
            boss.Roam.HasTarget = true;
        }
        boss.Transform.Position = boss.Transform.Position.MoveToward(boss.Roam.Target, boss.Movement.Speed * RoamSpeedFactor * delta);
        if (boss.Transform.Position.DistanceTo(boss.Roam.Target) < 0.5f) { boss.Roam.HasTarget = false; boss.Roam.Wait = RoamPauseSeconds; }
    }
}
