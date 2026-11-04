/// <summary>
/// The boss of stage 3, a PROTOTYPE to fit the story and the gameplay into: Thaqai, the cat of the stars, fast, crossing the lake in diagonals.
/// Six attacks (the third boss has 4 calm and 2 enraged): the hail that falls in curtains, the constellations of stars linked by thin lines, the
/// lightning that zigzags, and the fine weave of hail that makes the boat move in small steps; then the sky that falls and the eclipse. For now
/// she wears the art of the first boss.
/// </summary>
public static class ThaqaiBoss
{
    public const string NameKey = "ThaqaiName";

    private static readonly BossBuilder Builder = new BossBuilder(NameKey, 32, BossStyles.Thaqai, health: 350, enragedHealth: 550, theme: MusicPlayer.Theme.CatBoss, themeTitleKey: nameof(UIText.Boss3Music));

    public static readonly BossDefinition Definition = Builder.Build(
        Builder.Calm(Attacks.HailCurtain()),
        Builder.Calm(Attacks.StarPentagram()),
        Builder.Calm(Attacks.ZigzagLightning()),
        Builder.Calm(Attacks.CatPlay()),
        Builder.Enraged(Attacks.SkyFalls()),
        Builder.Enraged(Attacks.Eclipse()));
}
