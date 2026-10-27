using System.Collections.Generic;

/// <summary>What a kind of spirit is worth and how much it can take. They all look alike and differ in colour, and in what they do.</summary>
public readonly struct SpiritStats
{
    public readonly int MaxHealth;
    public readonly float HitboxRadius;
    /// <summary>How many little lights it leaves behind, each worth one unit of the lantern's energy.</summary>
    public readonly int Drops;
    /// <summary>Score for defeating one.</summary>
    public readonly int Points;

    public SpiritStats(int maxHealth, float hitboxRadius, int drops, int points)
    {
        MaxHealth = maxHealth; HitboxRadius = hitboxRadius; Drops = drops; Points = points;
    }
}

/// <summary>The spirits of the game, in a table: a new kind of spirit is a new value of <see cref="SpiritKind"/> and a new row here.</summary>
public static class SpiritCatalog
{
    private static readonly Dictionary<SpiritKind, SpiritStats> Table = new Dictionary<SpiritKind, SpiritStats>
    {
        // The blue swarm falls to a single shot, even the plain one; the green and red ones take more.
        [SpiritKind.Swarm] = new SpiritStats(maxHealth: 1, hitboxRadius: 9, drops: 1, points: 300),
        [SpiritKind.Gunner] = new SpiritStats(maxHealth: 7, hitboxRadius: 10, drops: 2, points: 700),
        [SpiritKind.Heavy] = new SpiritStats(maxHealth: 130, hitboxRadius: 13, drops: 8, points: 5000),
    };

    public static int MaxHealth(this SpiritKind kind) => Table[kind].MaxHealth;
    public static float HitboxRadius(this SpiritKind kind) => Table[kind].HitboxRadius;
    public static int Drops(this SpiritKind kind) => Table[kind].Drops;
    public static int Points(this SpiritKind kind) => Table[kind].Points;
}
