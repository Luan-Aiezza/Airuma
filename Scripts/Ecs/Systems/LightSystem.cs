using System;
using System.Linq;
using Godot;

/// <summary>The little lights that fall from the spirits, and the prizes of the score: how they fall, are pulled by the lantern and are gathered.</summary>
public static class LightSystem
{
    public const int Capacity = 260;
    public const float Gravity = 240;
    public const float MaxFallSpeed = 130;
    /// <summary>The boat picks a light up when it is this close.</summary>
    public const float PickupRadius = 28;
    /// <summary>Within this distance the boat's lantern pulls lights towards it.</summary>
    public const float MagnetRadius = 135;
    public const float MagnetSpeed = 430;

    public static LightItem[] NewPool(int count = Capacity)
    {
        var items = new LightItem[count];
        for (int i = 0; i < count; i++) items[i] = new LightItem();
        return items;
    }

    /// <summary>Lets go of a light that pops out a little and then falls.</summary>
    public static void Spawn(LightItem[] items, Vector2 position, float value, float scatter = 1)
    {
        int i = Array.FindIndex(items, it => !it.IsActive);
        if (i < 0) return;
        var rng = Random.Shared;
        float vx = -70 + 140 * (float)rng.NextDouble();
        float vy = -170 + 110 * (float)rng.NextDouble();
        items[i] = new LightItem { IsActive = true, Position = position, Velocity = new Vector2(vx, vy) * scatter, Value = value, Age = 0 };
    }

    /// <summary>
    /// A prize for the score, a spare lantern (a bomb) or a spare boat (a life), enters from the top of the picture and sinks slowly.
    /// If it is missed it comes round again at the top, so it can still be gathered.
    /// </summary>
    public static void SpawnReward(LightItem[] items, LightKind kind, float x, float y = RewardEntryY)
    {
        int i = Array.FindIndex(items, it => !it.IsActive);
        if (i < 0) return;
        items[i] = new LightItem { Kind = kind, IsActive = true, Position = new Vector2(x, y), Velocity = new Vector2(0, RewardSpeed), Value = 1, Age = 0 };
    }

    public const float RewardEntryY = -20;
    public const float RewardSpeed = 70;

    /// <summary>Moves the lights and returns how much energy the player gathered.</summary>
    public static float Update(LightItem[] items, Vector2 player, Rect2 bounds, float delta, float magnetScale = 1) =>
        Update(items, player, bounds, delta, out _, out _, magnetScale);

    public static float Update(LightItem[] items, Vector2 player, Rect2 bounds, float delta, out int bombs, float magnetScale = 1) =>
        Update(items, player, bounds, delta, out bombs, out _, magnetScale);

    /// <summary>Moves the lights; returns the energy gathered, how many spare lanterns (bomb refills) and how many spare boats (lives) were picked up.</summary>
    public static float Update(LightItem[] items, Vector2 player, Rect2 bounds, float delta, out int bombs, out int lives, float magnetScale = 1)
    {
        float gathered = 0;
        bombs = 0;
        lives = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (!items[i].IsActive) continue;
            items[i].Age += delta;
            Vector2 toPlayer = player - items[i].Position;
            float distance = toPlayer.Length();

            if (distance < PickupRadius)
            {
                if (items[i].Kind == LightKind.Bomb) bombs++; else if (items[i].Kind == LightKind.Life) lives++; else gathered += items[i].Value;
                items[i].IsActive = false;
                continue;
            }
            if (distance < MagnetRadius * magnetScale && items[i].Age > 0.25f)
            {
                items[i].Velocity = Mathx.Norm(toPlayer) * MagnetSpeed;
            }
            else if (items[i].Kind != LightKind.Energy)
            {
                items[i].Velocity = new Vector2(0, RewardSpeed);   // a prize sinks at an even pace
            }
            else
            {
                items[i].Velocity.Y = MathF.Min(items[i].Velocity.Y + Gravity * delta, MaxFallSpeed);
                items[i].Velocity.X *= MathF.Max(0, 1 - 1.5f * delta);
            }
            items[i].Position += items[i].Velocity * delta;

            // A prize that is missed comes round again at the top.
            if (items[i].Kind != LightKind.Energy && items[i].Position.Y > bounds.End.Y + 30)
            {
                items[i].Position = new Vector2(items[i].Position.X, RewardEntryY);
                items[i].Age = 0;
                continue;
            }

            // Lights that fall out of the picture are lost.
            if (items[i].Position.Y > bounds.End.Y + 30 || items[i].Position.X < bounds.Position.X - 60 || items[i].Position.X > bounds.End.X + 60)
                items[i].IsActive = false;
        }
        return gathered;
    }
}
