using System;
using Godot;

/// <summary>Movement of the bosses: the cross.</summary>
public static partial class MovementSystem
{
    /// <summary>The cross: how fast the boss goes from corner to corner, and how long it rests at the end of each leg (it never moves on before).</summary>
    public const float CrossSpeed = 620, CrossRestSeconds = 3.0f;

    /// <summary>Goes to the next corner, fast, rests there, and goes on to the next.</summary>
    private static void CrossStep(BossEntity boss, float delta)
    {
        if (boss.Cross.Rest > 0) { boss.Cross.Rest -= delta; return; }
        var target = CrossBrain.Corners[boss.Cross.Next];
        boss.Transform.Position = boss.Transform.Position.MoveToward(target, CrossSpeed * delta);
        if (boss.Transform.Position.DistanceTo(target) < 0.5f)
        {
            boss.Cross.Next = (boss.Cross.Next + 1) % CrossBrain.Corners.Length;
            boss.Cross.Rest = CrossRestSeconds;
        }
    }
}
