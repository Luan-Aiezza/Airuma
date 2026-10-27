/// <summary>The bomb: while it is lit it clears the boss's bullets within its reach.</summary>
public static class BombSystem
{
    /// <summary>
    /// The lantern's light burns spirits hard: every spirit in reach loses this much health per second (enough to
    /// finish off a red one in a few seconds), while the boss, who is sturdier, loses this much of a bar per second.
    /// </summary>
    public const float SpiritDamagePerSecond = 45;
    public const float BossDamagePerSecond = 14;

    public static void Process(PlayerEntity player, BulletData[] pool, float delta)
    {
        if (player.Input.IsBombing && !player.Bomb.IsActive && player.Bomb.BombsLeft > 0)
        {
            player.Bomb.IsActive = true;
            player.Bomb.BombsLeft -= 1;
            player.Bomb.Timer = player.Bomb.Duration;
        }

        if (!player.Bomb.IsActive) return;
        player.Bomb.Timer -= delta;

        // The aura of light neutralises any boss bullet that touches it.
        for (int i = 0; i < pool.Length; i++)
        {
            if (pool[i].IsActive && pool[i].Faction == Faction.Boss)
            {
                if (player.Transform.Position.DistanceTo(pool[i].Position) <= player.Bomb.Radius)
                    pool[i].IsActive = false;
            }
        }

        if (player.Bomb.Timer <= 0)
            player.Bomb.IsActive = false;
    }
}
