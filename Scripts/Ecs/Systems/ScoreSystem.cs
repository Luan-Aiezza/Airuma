using System;

/// <summary>
/// The score, and the prizes it earns, as the extends of Touhou: a spare lantern (a bomb) at certain scores and a spare boat (a life)
/// at others. They come in from the top of the picture, to be gathered. A whole run, spirits and boss, is worth about 480,000 points to
/// a player who does well, which makes about two lanterns and one boat.
/// </summary>
public static class ScoreSystem
{
    public const int PointsPerHit = 100;
    public const int PointsPerPhase = 50_000;
    public const int PointsPerSpiritHit = 10;
    /// <summary>Points for each unit of light gathered once the lantern is already full.</summary>
    public const int PointsPerSpareLight = 50;
    /// <summary>What a spare lantern or a spare boat is worth when there is no room for it.</summary>
    public const int PointsPerSpareBomb = 2000, PointsPerSpareLife = 10_000;

    public const int FirstBombScore = 120_000, SecondBombScore = 300_000, BombScoreStep = 250_000;
    public const int FirstLifeScore = 280_000, LifeScoreStep = 380_000;

    /// <summary>The score at which the nth (from 0) spare lantern is given.</summary>
    public static int BombScore(int n) => n == 0 ? FirstBombScore : SecondBombScore + (n - 1) * BombScoreStep;

    /// <summary>The score at which the nth (from 0) spare boat is given.</summary>
    public static int LifeScore(int n) => FirstLifeScore + n * LifeScoreStep;

    /// <summary>Adds points (times the cards' multiplier), and lets in the prizes the score has now reached.</summary>
    public static void Add(ref ScoreBoard score, int points, float multiplier, LightItem[] lights)
    {
        score.Points += (int)MathF.Round(points * multiplier, MidpointRounding.AwayFromZero);
        while (score.Points >= BombScore(score.BombPrizes))
        {
            LightSystem.SpawnReward(lights, LightKind.Bomb, RandomX());
            score.BombPrizes++;
        }
        while (score.Points >= LifeScore(score.LifePrizes))
        {
            LightSystem.SpawnReward(lights, LightKind.Life, RandomX());
            score.LifePrizes++;
        }
    }

    private static float RandomX() => 130 + 340 * (float)Random.Shared.NextDouble();
}
