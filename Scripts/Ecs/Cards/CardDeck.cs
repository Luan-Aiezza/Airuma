using System;
using System.Collections.Generic;
using System.Linq;

public static class CardDeck
{
    public const int OfferSize = 3;

    /// <summary>
    /// Draws OfferSize different cards, favouring the common ones. Cards at their limit, or useless right now, are not offered.
    /// </summary>
    public static List<Card> Offers(RunState owned, int bombsLeft, SeededGenerator rng, float rareBias = 1)
    {
        var pool = Enum.GetValues<Card>().Where(c => owned.Count(c) < CardInfo.MaxStacks(c) && CardInfo.IsUseful(c, bombsLeft)).ToList();
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
