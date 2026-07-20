using Godot;
using System;

public static class MovementSystem
{
    public static void ProcessPlayerInput(PlayerEntity player)
    {
        Vector2 direction = Vector2.Zero;
        if (Input.IsActionPressed("ui_right")) direction.X += 1;
        if (Input.IsActionPressed("ui_left")) direction.X -= 1;
        if (Input.IsActionPressed("ui_down")) direction.Y += 1;
        if (Input.IsActionPressed("ui_up")) direction.Y -= 1;

        player.Input.InputDirection = direction.Normalized();
    }

    public static void MovePlayer(PlayerEntity player, Rect2 bounds, float delta)
    {
        player.Movement.Velocity = player.Input.InputDirection * player.Movement.Speed;
        player.Transform.Position += player.Movement.Velocity * delta;

        float clampedX = Mathf.Clamp(player.Transform.Position.X, bounds.Position.X, bounds.End.X);
        float clampedY = Mathf.Clamp(player.Transform.Position.Y, bounds.Position.Y, bounds.End.Y);
        player.Transform.Position = new Vector2(clampedX, clampedY);
    }

    public static void MoveBoss(BossEntity boss, Rect2 bounds, float delta)
    {
        boss.Movement.Velocity = new Vector2(boss.PatrolDirection * boss.Movement.Speed, 0);
        boss.Transform.Position += boss.Movement.Velocity * delta;

        float margin = 50f;
        if (boss.Transform.Position.X >= bounds.End.X - margin)
            boss.PatrolDirection = -1f;
        else if (boss.Transform.Position.X <= bounds.Position.X + margin)
            boss.PatrolDirection = 1f;
    }
}