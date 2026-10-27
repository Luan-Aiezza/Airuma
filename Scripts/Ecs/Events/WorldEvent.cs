using Godot;

public enum WorldEventKind { WaveStarted, EnemyDefeated, BossEntered, PowerUp, WardBroke, CardTaken, BombRefilled, LifeRefilled, BulletsFired, PlayerHit, BossBroken, BombFired, LightGathered, EnemyHit, BossHit }

/// <summary>Something that happened in the world that the scene may want to show or tell.</summary>
public class WorldEvent
{
    public WorldEventKind Kind;
    /// <summary>WaveStarted: the wave index. PowerUp: the new level.</summary>
    public int Int;
    /// <summary>EnemyDefeated: the kind.</summary>
    public SpiritKind Spirit;
    /// <summary>EnemyDefeated: where.</summary>
    public Vector2 Position;
    /// <summary>CardTaken: the card.</summary>
    public Card Card;
    /// <summary>BulletsFired: the style of the bullets that were just fired (Int is how many).</summary>
    public BulletStyle Style;

    public static WorldEvent WaveStarted(int index) => new WorldEvent { Kind = WorldEventKind.WaveStarted, Int = index };
    public static WorldEvent EnemyDefeated(SpiritKind kind, Vector2 position) => new WorldEvent { Kind = WorldEventKind.EnemyDefeated, Spirit = kind, Position = position };
    public static WorldEvent BossEntered() => new WorldEvent { Kind = WorldEventKind.BossEntered };
    public static WorldEvent PowerUp(int level) => new WorldEvent { Kind = WorldEventKind.PowerUp, Int = level };
    public static WorldEvent WardBroke() => new WorldEvent { Kind = WorldEventKind.WardBroke };
    public static WorldEvent BulletsFired(BulletStyle style, int count) => new WorldEvent { Kind = WorldEventKind.BulletsFired, Style = style, Int = count };
    public static WorldEvent EnemyHit(SpiritKind kind) => new WorldEvent { Kind = WorldEventKind.EnemyHit, Spirit = kind };
    public static WorldEvent BossHit() => new WorldEvent { Kind = WorldEventKind.BossHit };
    public static WorldEvent BombFired() => new WorldEvent { Kind = WorldEventKind.BombFired };
    public static WorldEvent LightGathered() => new WorldEvent { Kind = WorldEventKind.LightGathered };
    public static WorldEvent PlayerHit() => new WorldEvent { Kind = WorldEventKind.PlayerHit };
    public static WorldEvent BossBroken() => new WorldEvent { Kind = WorldEventKind.BossBroken };
    public static WorldEvent LifeRefilled() => new WorldEvent { Kind = WorldEventKind.LifeRefilled };
    public static WorldEvent BombRefilled() => new WorldEvent { Kind = WorldEventKind.BombRefilled };
    public static WorldEvent CardTaken(Card card) => new WorldEvent { Kind = WorldEventKind.CardTaken, Card = card };

    public override bool Equals(object obj) =>
        obj is WorldEvent e && e.Kind == Kind && e.Int == Int && e.Spirit == Spirit && e.Position == Position && e.Card == Card && e.Style == Style;
    public override int GetHashCode() => System.HashCode.Combine(Kind, Int, Spirit, Position, Card, Style);
}
