using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// How much light the player's lantern is fed with, and what weapon level that makes.
///
/// The player starts at level 1 and gathers little lights dropped by the spirits they defeat. Level 3 is reached with
/// the light of the 45% *least* generous spirits of the stage, so collecting the drops of any 45% of them is enough.
/// Level 2 comes at 35% of that (each light counts for a lot).
/// </summary>
public static class PowerCurve
{
    public const int MaxLevel = 3;

    /// <summary>How many lights each spirit of a stage leaves behind, in the order of the script.</summary>
    public static List<int> DropsIn(StageDefinition stage) =>
        stage.Waves.SelectMany(w => w.Spawns.Select(s => s.Kind.Drops())).ToList();

    /// <summary>
    /// The levels are measured against the first stage, and kept the same in every stage: the boat's energy goes with it from stage to stage, so
    /// a level has to mean the same thing everywhere. (Later stages have more spirits, so they feed the lantern faster.)
    /// </summary>
    public static List<int> DropsInStage => DropsIn(Stages.One);

    /// <summary>Level 3 needs this many times all the light that stage 1 can give, so that even gathering every light of stage 1 gets the boat only to level 2. The second stage tops it up.</summary>
    public const double Level3OfStageOne = 1.25;

    /// <summary>The share of the energy for level 3 that is enough for level 2.</summary>
    public const double Level2Share = 0.35;

    /// <summary>Energy for level 3: a quarter more than all the light of stage 1.</summary>
    public static readonly float Level3 = MathF.Round((float)(DropsInStage.Sum() * Level3OfStageOne), MidpointRounding.AwayFromZero);

    /// <summary>Energy for level 2.</summary>
    public static readonly float Level2 = MathF.Round(Level3 * (float)Level2Share, MidpointRounding.AwayFromZero);

    /// <summary>The most energy that can be stored; more is worth points instead.</summary>
    public static float MaxEnergy => Level3;

    /// <summary>The weapon level for an amount of energy. A hair of tolerance, because energy is gathered in fractions that do not add up exactly.</summary>
    public static int Level(float energy) => energy >= Level3 - 0.01f ? 3 : (energy >= Level2 - 0.01f ? 2 : 1);

    /// <summary>Progress (0...1) towards the next level; 1 at the top level.</summary>
    public static float Progress(float energy)
    {
        switch (Level(energy))
        {
            case 1: return Level2 > 0 ? energy / Level2 : 1;
            case 2: return Level3 > Level2 ? (energy - Level2) / (Level3 - Level2) : 1;
            default: return 1;
        }
    }

    /// <summary>Energy the player starts with: nothing, or full power when rehearsing a boss attack.</summary>
    public static float StartingEnergy(bool practising) => practising ? Level3 : 0;

    /// <summary>The share of the stored energy that is lost, and dropped where the player died.</summary>
    public const float LossOnDeath = 0.3f;
}
