using System;
using Godot;

/// <summary>Movement of the bosses: the patrol.</summary>
public static partial class MovementSystem
{
    /// <summary>The patrol: seconds the boss stays at a station, and the least a leg of the patrol goes (so that each is easy to follow).</summary>
    public const float PatrolPauseSeconds = 3.0f, PatrolMinLeg = 200;

    /// <summary>
    /// The boss glides to a station on one side of the field, stays there a while, then goes to one on the other side. Moving in
    /// legs with a rest between them is much easier to follow than moving all the time.
    /// </summary>
    private static void PatrolStep(BossEntity boss, Rect2 bounds, float delta)
    {
        if (boss.Patrol.Wait > 0)
        {
            boss.Patrol.Wait -= delta;
            return;
        }
        float margin = boss.Hitbox.Radius;
        float left = bounds.Position.X + margin, right = bounds.End.X - margin;
        float middle = (bounds.Position.X + bounds.End.X) / 2;
        if (!boss.Patrol.HasTarget)
        {
            // To the other side of the middle than the one it is on; a station at least a good leg away.
            bool goRight = boss.Transform.Position.X < middle - 1 || (MathF.Abs(boss.Transform.Position.X - middle) <= 1 && boss.Patrol.Direction > 0);
            float lo = goRight ? middle + 60 : left + 20, hi = goRight ? right - 20 : middle - 60;
            float x;
            int tries = 0;
            do x = lo + (hi - lo) * (float)Random.Shared.NextDouble();
            while (MathF.Abs(x - boss.Transform.Position.X) < PatrolMinLeg && ++tries < 12);
            boss.Patrol.TargetX = Math.Clamp(x, left, right);
            boss.Patrol.Direction = goRight ? -1 : 1;           // the side it will go to next
            boss.Patrol.HasTarget = true;
        }
        var position = boss.Transform.Position;
        position.X = Mathf.MoveToward(position.X, boss.Patrol.TargetX, boss.Movement.Speed * delta);
        boss.Transform.Position = position;
        if (MathF.Abs(position.X - boss.Patrol.TargetX) < 0.5f)
        {
            boss.Patrol.HasTarget = false;
            boss.Patrol.Wait = PatrolPauseSeconds;
        }
    }
}
