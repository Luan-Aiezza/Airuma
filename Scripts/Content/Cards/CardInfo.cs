using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The cards of the run: each one a boon the player takes, one of three offered at the start of the stage and again
/// before the boss. They pile up for as long as the run lasts, and are all lost when the run ends in defeat.
/// </summary>
public static class CardInfo
{
    public static int Weight(CardRarity rarity) => rarity switch { CardRarity.Common => 10, CardRarity.Uncommon => 6, _ => 3 };

    private static UIText.CardText Text(Card card) => card switch
    {
        Card.HomingWisp => UIText.CardHomingWisp,
        Card.TwinFan => UIText.CardTwinFan,
        Card.PiercingLight => UIText.CardPiercingLight,
        Card.Fervor => UIText.CardFervor,
        Card.QuickWick => UIText.CardQuickWick,
        Card.BurstingBolts => UIText.CardBurstingBolts,
        Card.SpiritWard => UIText.CardSpiritWard,
        Card.MendingWard => UIText.CardMendingWard,
        Card.SlimWake => UIText.CardSlimWake,
        Card.CalmSails => UIText.CardCalmSails,
        Card.BrighterLight => UIText.CardBrighterLight,
        Card.LongBurn => UIText.CardLongBurn,
        Card.SearingLight => UIText.CardSearingLight,
        Card.SpareLantern => UIText.CardSpareLantern,
        Card.HungryLantern => UIText.CardHungryLantern,
        Card.WideNet => UIText.CardWideNet,
        _ => UIText.CardGoldenSpirits,
    };

    public static string Title(Card card) => Text(card).Title;

    public static string Detail(Card card) => Text(card).Detail;

    public static CardRarity Rarity(Card card)
    {
        switch (card)
        {
            case Card.TwinFan: case Card.Fervor: case Card.QuickWick: case Card.SpiritWard: case Card.CalmSails:
            case Card.BrighterLight: case Card.LongBurn: case Card.SpareLantern: case Card.WideNet: case Card.GoldenSpirits:
                return CardRarity.Common;
            case Card.HomingWisp: case Card.PiercingLight: case Card.SearingLight: case Card.HungryLantern:
                return CardRarity.Uncommon;
            default:
                return CardRarity.Rare;
        }
    }

    /// <summary>How many times it can be taken in one run.</summary>
    public static int MaxStacks(Card card)
    {
        switch (card)
        {
            case Card.MendingWard: case Card.SlimWake: return 1;
            case Card.TwinFan: case Card.BurstingBolts: case Card.CalmSails: case Card.HungryLantern: case Card.SpiritWard: return 2;
            case Card.SpareLantern: return 99;
            default: return 3;
        }
    }

    /// <summary>Some cards are no use right now (a spare lantern when all three are full).</summary>
    public static bool IsUseful(Card card, int bombsLeft) => card != Card.SpareLantern || bombsLeft < 3;

    /// <summary>The card's name in camel case ("homingWisp"), used to order cards with equal counts.</summary>
    public static string CaseName(Card card)
    {
        string n = card.ToString();
        return char.ToLowerInvariant(n[0]) + n.Substring(1);
    }
}
