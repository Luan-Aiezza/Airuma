// Shared enums of the simulation (several small types in one file on purpose).

public enum Faction { Player = 0, Boss = 1 }

/// <summary>Visual identity of a bullet; each boss attack has its own, all of them fire or serpents.</summary>
public enum BulletStyle
{
    Fireball, FlameDart, Spark, VenomOrb, LotusPetal, MagmaOrb, FireSerpent, Ember,
    LightBolt, OrbGreen, OrbRed, RiceRed, Wisp, Hail, Star, Shard, Claw, Rock, Stalactite,
    // The danmaku family: orbs (7 x 7) in every colour, beads (5 x 5, for the thick streams), rice (3 x 7, they point where they go), kunai (5 x 9), little stars in colours (7 x 7)
    // and the great orbs (25 x 25). Colours: the name says which.
    OrbBlue, OrbYellow, OrbViolet, OrbWhite, OrbOrange, OrbCyan, OrbPink,
    BeadRed, BeadBlue, BeadYellow, BeadGreen, BeadViolet, BeadWhite, BeadCyan, BeadPink,
    RiceBlue, RiceYellow, RiceGreen, RiceViolet, RiceWhite, RiceCyan,
    KunaiRed, KunaiBlue, KunaiGreen, KunaiYellow, KunaiViolet,
    StarBlue, StarGold, StarGreen, StarViolet, StarPink,
    GiantRed, GiantBlue, GiantGold, GiantGreen, GiantViolet, GiantWhite,
    // little bolts of lightning (5 x 11), that point where they go
    BoltYellow, BoltBlue, BoltRed,
    // a great boulder covered with moss (64 x 64: about three times the boat) and a great bite, a jaw with its teeth (24 x 24), that points where it goes
    MossBoulder, Bite,
    // a giant stalactite (16 x 40, a spike of 64 x 160 px), drawn with care, that points where it goes
    GiantStalactite,
    // music: little notes (7 x 11, that sway, they do not turn) in gold, pink and cyan, and a pair of beamed notes (9 x 11) in gold
    NoteGold, NotePink, NoteCyan, NoteBeamed,
    // the player's: the small golden bolts of the cards' fans (3 x 8, that point where they go), unlike the cyan bolts of the main stream
    FanBolt,
}

/// <summary>Dance: the bullet sways to one side and the other on a fixed beat (see BulletSystem.DanceBeat). Zigzag: it turns by its curve's degrees every fraction of a second, to one side and the other, like lightning. Constellation: it is a star that flies out, stops, hangs a while linked by thin lines to other stars, and then falls. Burst: a great stone that flies and, after a moment, breaks into a ring of small ones.</summary>
public enum BulletBehavior
{
    Normal, StopAndAim, Homing, Dance, Zigzag, Constellation, Burst,
    /// <summary>Waits (brakes) for `Fuse` seconds and then goes off at `LaunchSpeed`, along `LaunchAngle` (or at the boat, if that is NaN): bullets laid out in a figure that are let go all at once, or one after the other.</summary>
    Launch,
    /// <summary>Flies for `Fuse` seconds and then breaks into `SplitCount` bullets of `ChildStyle` (a ring, or a fan `SplitSpread` degrees wide, aimed at the boat if `SplitAimed`).</summary>
    Split,
    /// <summary>Bounces `Bounces` times off the sides of the field (and the top) before it goes on.</summary>
    Bounce,
    /// <summary>Moves on a polar track round `Anchor`: radius and angle both change with time (spiral arms, rosettes, wheels). Its velocity is only for the picture.</summary>
    Polar,
    /// <summary>Slows evenly to a stop in `TurnInterval` seconds and then turns (by an angle more, towards the boat, or to a set heading) and sets off again at `TurnSpeed`; again, `TurnTimes` times. (Of the bullets of Touhou: the ones that stop, then turn.)</summary>
    Turn,
}

/// <summary>Where a slash of the boss's claws is aimed: through the boat, at random, alternating from one side to the other, or from ANY direction (through the boat, at any angle).</summary>
public enum SlashAim { Player, Random, Alternate, Anywhere }

/// <summary>Shuttle: goes between the top left and the top right of the field, and rests a long while (10 seconds) at each before it goes back. Cross: fast legs across the field, diagonal and straight, with a rest at the end of each. Sweep: fires a great jet of water down the field while crossing it. Roam: glides about the field in every direction. Dive: sinks, travels unseen under the water (a shadow shows where) and leaps out.</summary>
public enum BossMovementType { Patrol, Stationary, Dash, Transition, Roam, Dive, Sweep, Cross, Shuttle }

/// <summary>The steps of the sweep: the boss goes to the left edge, charges, sweeps its jet to the right (stopping short of the right edge), goes over to the right edge, charges and sweeps back to the left (stopping short of the left edge), then returns to the middle and rests.</summary>
public enum SweepState { ToLeft, Charge, Pass, ToRight, Charge2, Pass2, Return, Rest }

public enum DiveState { Surface, Sinking, Under, Rising }

public enum GameResult { Victory, Defeat }
