/// <summary>
/// The boss of stage 5, a PROTOTYPE to fit the story and the gameplay into: Kuychu, the guardian between the two worlds, with the two serpents that he wears. Eight
/// attacks (the fifth boss has 5 calm and 3 enraged), made from what the Amaru does in the myth (see <see cref="Attacks"/>): the heads that fire in turns, the rainbow,
/// the earthquake, the tide and the trial head to head; then the broken rainbow, the tremor between two worlds and the guardian. He is two persons who are one being (see <see cref="KuychuArt"/>), and fire from the heads of the serpent that joins them.
/// </summary>
public static class KuychuBoss
{
    public const string NameKey = "KuychuName";

    private static readonly BossBuilder Builder = new BossBuilder(NameKey, 44, BossStyles.Kuychu, health: 400, enragedHealth: 650, theme: MusicPlayer.Theme.SerpentBoss, themeTitleKey: nameof(UIText.Boss5Music));

    public static readonly BossDefinition Definition = Builder.Build(
        Builder.Calm(Attacks.TwoHeads()),
        Builder.Calm(Attacks.SevenArcs()),
        Builder.Calm(Attacks.BorderQuake()),
        Builder.Calm(Attacks.Tide()),
        Builder.Calm(Attacks.Trial()),
        Builder.Enraged(Attacks.BrokenRainbow()),
        Builder.Enraged(Attacks.TwoWorlds()),
        Builder.Enraged(Attacks.Guardian()));
}
