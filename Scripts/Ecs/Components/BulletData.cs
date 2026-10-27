using Godot;

/// <summary>One slot of the fixed-size bullet pool.</summary>
public struct BulletData
{
    public bool IsActive;
    /// <summary>Set when the bullet is fired, so the world can tell the scene about it (for the sound) exactly once.</summary>
    public bool Fresh;
    public Vector2 Position;
    public Vector2 Velocity;
    public Faction Faction;
    public float HitboxRadius;
    public int Damage;
    public float WindModifier;

    public float Acceleration;
    public float CurveRate;
    public float Scale;

    public BulletBehavior Behavior;
    public float Timer;
    public BulletStyle Style;

    // The player's shots, as the cards change them.
    /// <summary>Damage multiplier on top of Damage.</summary>
    public float Power;
    /// <summary>How many more foes it can pass through.</summary>
    public int Pierce;
    /// <summary>If above 0, the shot bursts on impact and hurts the foes this close to it.</summary>
    public float BurstRadius;
    /// <summary>The share of the burst this bolt deals (the bolts of a volley share one burst, whatever the weapon level).</summary>
    public float BurstScale;
    /// <summary>The last foe it went through, so it does not hit it again on every step.</summary>
    public int LastHit;

    public BulletData()
    {
        IsActive = false; Position = Vector2.Zero; Velocity = Vector2.Zero; Faction = Faction.Player; HitboxRadius = 0;
        Damage = 0; WindModifier = 1; Acceleration = 0; CurveRate = 0; Scale = 1; Behavior = BulletBehavior.Normal;
        Timer = 0; Style = BulletStyle.Fireball; Power = 1; Pierce = 0; BurstRadius = 0; BurstScale = 1; LastHit = -1;
    }
}
