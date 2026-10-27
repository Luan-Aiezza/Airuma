using System;

/// <summary>
/// The three difficulties. They change the enemies' bullets (fewer of them in the easy mode, more, and in some patterns extra ones, in the hard mode) and the
/// strength of the wind (the values of the easy mode, a little more in normal, a little more again in hard). Normal is the game as it was designed.
/// </summary>
public enum Difficulty { Easy, Normal, Hard }

public static class DifficultyExt
{
    public static string Title(this Difficulty d) => d switch { Difficulty.Easy => UIText.DifficultyEasy, Difficulty.Hard => UIText.DifficultyHard, _ => UIText.DifficultyNormal };

    /// <summary>How the number of bullets of a pattern is multiplied.</summary>
    public static float BulletFactor(this Difficulty d) => d switch { Difficulty.Easy => 0.6f, Difficulty.Hard => 1.35f, _ => 1f };

    /// <summary>How hard the wind blows on the boat and the shots: the strengths of the wind are those of the easy mode; the others add to them.</summary>
    public static float WindFactor(this Difficulty d) => d switch { Difficulty.Easy => 1f, Difficulty.Hard => 1.4f, _ => 1.2f };

    /// <summary>`count` bullets scaled for the difficulty (never fewer than `min`).</summary>
    public static int Scale(this Difficulty d, int count, int min = 1)
    {
        if (d == Difficulty.Normal) return count;
        return Math.Max(min, (int)MathF.Round(count * d.BulletFactor(), MidpointRounding.AwayFromZero));
    }
}
