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

    /// <summary>How many lights each spirit of the stage leaves behind, in the order of the script.</summary>
    public static List<int> DropsInStage =>
        Stages.One.Waves.SelectMany(w => w.Spawns.Select(s => s.Kind.Drops())).ToList();

    /// <summary>The share of the stage's spirits whose light is enough for level 3, and the share of that for level 2.</summary>
    public const double Level3Share = 0.45, Level2Share = 0.35;

    /// <summary>Energy for level 3: what the weakest 45% of the stage's spirits give.</summary>
    public static readonly float Level3 = ComputeLevel3();

    private static float ComputeLevel3()
    {
        var drops = DropsInStage.OrderBy(d => d).ToList();
        int share = (int)Math.Floor(drops.Count * Level3Share);
        return drops.Take(share).Sum();
    }

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
