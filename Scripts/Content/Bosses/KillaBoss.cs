/// <summary>
/// The boss of stage 6, a PROTOTYPE to fit the flow of the stage into: Mama Killa, the Moon, who took Illari so that the night would never end. Six attacks (4 calm and 2 enraged:
/// the crescent moon and then the full one). For now she wears borrowed art (the divine person of stage 5) and the attacks of the cat of the stars (stage 3), whose mistress she is,
/// ending with the eclipse; the attacks of the phases of the moon, with the tide that turns the wind, are still to be made.
/// </summary>
public static class KillaBoss
{
    public const string NameKey = "KillaName";

    private static readonly BossBuilder Builder = new BossBuilder(NameKey, 32, BossStyles.Killa, health: 420, enragedHealth: 680);

    public static readonly BossDefinition Definition = Builder.Build(
        Builder.Calm(Attacks.StarPentagram()),
        Builder.Calm(Attacks.ZigzagLightning()),
        Builder.Calm(Attacks.FineWeave()),
        Builder.Calm(Attacks.HailCurtain()),
        Builder.Enraged(Attacks.SkyFalls()),
        Builder.Enraged(Attacks.Eclipse()));
}
