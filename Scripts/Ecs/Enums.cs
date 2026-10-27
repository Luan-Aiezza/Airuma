// Shared enums of the simulation (several small types in one file on purpose).

public enum Faction { Player = 0, Boss = 1 }

/// <summary>Visual identity of a bullet; each boss attack has its own, all of them fire or serpents.</summary>
public enum BulletStyle
{
    Fireball, FlameDart, Spark, VenomOrb, LotusPetal, MagmaOrb, FireSerpent, Ember,
    LightBolt, OrbGreen, OrbRed, RiceRed, Wisp,
}

public enum BulletBehavior { Normal, StopAndAim, Homing }

public enum BossMovementType { Patrol, Stationary, Dash, Transition }

public enum GameResult { Victory, Defeat }
