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

    /// <summary>
    /// The power items are red squares with a "P", in three sizes: a small one is worth 1, a medium one 2 to 4 and a large one 5 or more (see <see cref="Size"/>). The stronger the spirit, the larger the square it leaves,
    /// instead of many small ones.
    /// </summary>
    public const float MediumFrom = 2, LargeFrom = 5;

    /// <summary>0 (small), 1 (medium) or 2 (large): the size of the square that an item of this value is.</summary>
    public static int Size(float value) => value >= LargeFrom ? 2 : (value >= MediumFrom ? 1 : 0);

    /// <summary>How close the boat has to be to pick an item up: the larger the square, the wider.</summary>
    public static float PickupRadiusOf(LightItem item) => item.Kind != LightKind.Energy ? PickupRadius : PickupRadius + 8 * Size(item.Value);

    /// <summary>Lets go of a power item (a square: small, medium or large by its value) that pops out a little and then falls.</summary>
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

    /// <summary>Moves the lights; returns the energy gathered, how many spare lanterns (bomb refills) and how many spare boats (lives) were picked up. With `collectAll` every light, bomb and boat in the picture flies to the boat.</summary>
    public static float Update(LightItem[] items, Vector2 player, Rect2 bounds, float delta, out int bombs, out int lives, float magnetScale = 1, bool collectAll = false)
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

            if (distance < PickupRadiusOf(items[i]))
            {
                if (items[i].Kind == LightKind.Bomb) bombs++; else if (items[i].Kind == LightKind.Life) lives++; else gathered += items[i].Value;
                items[i].IsActive = false;
                continue;
            }
            if ((collectAll || distance < MagnetRadius * magnetScale) && items[i].Age > 0.25f)
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
