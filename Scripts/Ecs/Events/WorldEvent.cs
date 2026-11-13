using Godot;

public enum WorldEventKind { WaveStarted, EnemyDefeated, BossEntered, PowerUp, WardBroke, CardTaken, BombRefilled, LifeRefilled, BulletsFired, PlayerHit, BossBroken, BombFired, LightGathered, EnemyHit, BossHit, SlashStruck, Roared, QuakeStarted, GravityPulled, TimerTick, AttackBonus, TimeUp, LaserWarning, LaserFired }

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
    public static WorldEvent SlashStruck() => new WorldEvent { Kind = WorldEventKind.SlashStruck };
    public static WorldEvent Roared(Vector2 at) => new WorldEvent { Kind = WorldEventKind.Roared, Position = at };
    public static WorldEvent GravityPulled() => new WorldEvent { Kind = WorldEventKind.GravityPulled };
    public static WorldEvent QuakeStarted() => new WorldEvent { Kind = WorldEventKind.QuakeStarted };
    /// <summary>One more second of the attack's time is about to run out (Int: the seconds left, 1 to 9).</summary>
    public static WorldEvent TimerTick(int secondsLeft) => new WorldEvent { Kind = WorldEventKind.TimerTick, Int = secondsLeft };
    /// <summary>The attack was beaten in time: Int is the bonus.</summary>
    public static WorldEvent AttackBonus(int points) => new WorldEvent { Kind = WorldEventKind.AttackBonus, Int = points };
    /// <summary>The time of the attack ran out.</summary>
    public static WorldEvent TimeUp() => new WorldEvent { Kind = WorldEventKind.TimeUp };
    /// <summary>A laser has appeared as its warning line, or one that was transparent is about to turn solid again (the sound that comes before the beam).</summary>
    public static WorldEvent LaserWarning() => new WorldEvent { Kind = WorldEventKind.LaserWarning };
    /// <summary>A laser strikes.</summary>
    public static WorldEvent LaserFired() => new WorldEvent { Kind = WorldEventKind.LaserFired };
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
