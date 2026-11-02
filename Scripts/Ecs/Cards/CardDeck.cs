using System;
using System.Collections.Generic;
using System.Linq;

public static class CardDeck
{
    public const int OfferSize = 3;

    /// <summary>
    /// Draws OfferSize different cards, favouring the common ones. Cards at their limit, or useless right now, are not offered.
    /// </summary>
    public static List<Card> Offers(RunState owned, int bombsLeft, SeededGenerator rng, float rareBias = 1, Card? special = null, float specialChance = 0)
    {
        var offers = Ordinary(owned, bombsLeft, rng, rareBias);
        // A special card of the boss that was beaten takes the place of one of the three, with a chance that grows with the score.
        if (special is Card gift && owned.Count(gift) < CardInfo.MaxStacks(gift) && offers.Count > 0 && rng.Range(0f, 1f) < specialChance)
            offers[rng.Range(0f, 1f) < 0.5f ? offers.Count - 1 : 0] = gift;
        return offers;
    }

    /// <summary>The chance of a special card after a boss, for the points made in that stage: a little for any score, more for a good one, up to 80%.</summary>
    public static float SpecialChance(int stageScore) => 0.1f + 0.7f * Math.Clamp(stageScore / (float)GoodStageScore, 0f, 1f);

    /// <summary>What a good player makes in one stage.</summary>
    public const int GoodStageScore = 250_000;

    private static List<Card> Ordinary(RunState owned, int bombsLeft, SeededGenerator rng, float rareBias)
    {
        var pool = Enum.GetValues<Card>().Where(c => owned.Count(c) < CardInfo.MaxStacks(c) && CardInfo.Rarity(c) != CardRarity.Special && CardInfo.IsUseful(c, bombsLeft)).ToList();
        var chosen = new List<Card>();
        while (chosen.Count < OfferSize && pool.Count > 0)
        {
            var weights = pool.Select(c => CardInfo.Rarity(c) == CardRarity.Common
                ? (float)CardInfo.Weight(CardInfo.Rarity(c)) : CardInfo.Weight(CardInfo.Rarity(c)) * rareBias).ToList();
            float roll = rng.Range(0f, weights.Sum());
            int index = pool.Count - 1;
            for (int i = 0; i < weights.Count; i++)
            {
                if (roll < weights[i]) { index = i; break; }
                roll -= weights[i];
            }
            chosen.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return chosen;
    }
}
