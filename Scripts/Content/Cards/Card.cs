public enum CardRarity { Common, Uncommon, Rare, Special }

/// <summary>The roguelike cards (see CardInfo for titles, texts, rarity and limits).</summary>
public enum Card
{
    HomingWisp, TwinFan, PiercingLight, Fervor, QuickWick, BurstingBolts,
    SpiritWard, MendingWard, SlimWake, CalmSails,
    BrighterLight, LongBurn, SearingLight, SpareLantern,
    HungryLantern, WideNet, GoldenSpirits,
    // Special cards, given by a boss that has been beaten (never in the ordinary draw)
    SerpentFlame, PartyFever, StormBolt, BearCloak,
}
