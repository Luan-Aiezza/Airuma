using System;
using Godot;

/// <summary>Movement of the bosses: the shuttle.</summary>
public static partial class MovementSystem
{
    /// <summary>The shuttle: how fast the boss goes between the top left and the top right, and how long it rests at each (it never moves on before).</summary>
    public const float ShuttleSpeed = 430, ShuttleRestSeconds = 10;
    public static readonly Vector2[] ShuttleSpots = { new Vector2(110, 130), new Vector2(490, 130) };

    /// <summary>Goes to the next spot (top left, top right), rests there ten seconds, and goes back; it uses the same bookkeeping as the cross.</summary>
    private static void ShuttleStep(BossEntity boss, float delta)
    {
        if (boss.Cross.Rest > 0) { boss.Cross.Rest -= delta; return; }
        var target = ShuttleSpots[boss.Cross.Next % ShuttleSpots.Length];
        boss.Transform.Position = boss.Transform.Position.MoveToward(target, ShuttleSpeed * delta);
        if (boss.Transform.Position.DistanceTo(target) < 0.5f)
        {
            boss.Cross.Next = (boss.Cross.Next + 1) % ShuttleSpots.Length;
            boss.Cross.Rest = ShuttleRestSeconds;
        }
    }
}
