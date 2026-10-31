// Shared enums of the simulation (several small types in one file on purpose).

public enum Faction { Player = 0, Boss = 1 }

/// <summary>Visual identity of a bullet; each boss attack has its own, all of them fire or serpents.</summary>
public enum BulletStyle
{
    Fireball, FlameDart, Spark, VenomOrb, LotusPetal, MagmaOrb, FireSerpent, Ember,
    LightBolt, OrbGreen, OrbRed, RiceRed, Wisp,
}

/// <summary>Dance: the bullet sways to one side and the other on a fixed beat (see BulletSystem.DanceBeat).</summary>
public enum BulletBehavior { Normal, StopAndAim, Homing, Dance }

/// <summary>Sweep: fires a great jet of water down the field while crossing it. Roam: glides about the field in every direction. Dive: sinks, travels unseen under the water (a shadow shows where) and leaps out.</summary>
public enum BossMovementType { Patrol, Stationary, Dash, Transition, Roam, Dive, Sweep }

/// <summary>The steps of the sweep: the boss goes to the left edge, charges, sweeps its jet to the right (stopping short of the right edge), goes over to the right edge, charges and sweeps back to the left (stopping short of the left edge), then returns to the middle and rests.</summary>
public enum SweepState { ToLeft, Charge, Pass, ToRight, Charge2, Pass2, Return, Rest }

public enum DiveState { Surface, Sinking, Under, Rising }

public enum GameResult { Victory, Defeat }
