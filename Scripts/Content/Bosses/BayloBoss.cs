/// <summary>
/// The boss of stage 2, a PROTOTYPE to fit the story into: Baylo, the river dancer, in his white suit and hat. He keeps that shape to the end.
/// Five attacks: three calm ones, in which he roams the whole field and his bullets dance on a beat, and two enraged ones (other music, extra
/// effects, a hotter river): he dives, with a shadow under the boat showing where he will come out, and sweeps a great jet of water across the
/// field. The second form he had (the great dolphin, see Art/Retired) is kept in the project but not used.
/// </summary>
public static class BayloBoss
{
    public const string NameKey = "BayloName";

    private static readonly BossBuilder Builder = new BossBuilder(NameKey, 26, BossStyles.Baylo, theme: MusicPlayer.Theme.DancerBoss, themeTitleKey: nameof(UIText.Boss2Music));

    public static readonly BossDefinition Definition = Builder.Build(
        Builder.Calm(Attacks.PartyTwoStep()),
        Builder.Calm(Attacks.ConfettiRain()),
        Builder.Calm(Attacks.SambaWheel()),
        Builder.Enraged(Attacks.GreatLeap()),
        Builder.Enraged(Attacks.WaterJet()));
}
