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
    public static float BulletFactor(this Difficulty d) => d.Pick(1f, 1.35f, 1.7f);

    /// <summary>The value for the difficulty. (The three levels are one step harder than they used to be: easy is the old normal, normal the old hard, and hard is harder than any before.)</summary>
    public static T Pick<T>(this Difficulty d, T easy, T normal, T hard) => d switch { Difficulty.Easy => easy, Difficulty.Hard => hard, _ => normal };

    /// <summary>How hard the wind blows on the boat and the shots: the strengths of the wind are those of the easy mode; the others add to them.</summary>
    public static float WindFactor(this Difficulty d) => d.Pick(1.2f, 1.4f, 1.6f);

    /// <summary>`count` bullets scaled for the difficulty (never fewer than `min`).</summary>
    public static int Scale(this Difficulty d, int count, int min = 1)
    {
        if (d == Difficulty.Easy) return count;
        return Math.Max(min, (int)MathF.Round(count * d.BulletFactor(), MidpointRounding.AwayFromZero));
    }
}
