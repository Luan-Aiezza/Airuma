/// <summary>
/// The boss of stage 4: Ukhumo, the bear girl of the cave, hungry (she wants Atli's toasted corn), trapped on her rock platform. Seven attacks (the fourth boss has 4
/// calm and 3 enraged), the attacks of an animal, each asking something different of the player (see <see cref="Attacks"/>): the claws that cut the field, the roar that
/// throws the boat back, the stamp that shakes the screen and brings the ceiling down, the stone that breaks in the air; then the claws crossed, the cave coming down,
/// and the feast, that is all of them.
/// </summary>
public static class UkhumoBoss
{
    public const string NameKey = "UkhumoName";

    private static readonly BossBuilder Builder = new BossBuilder(NameKey, 32, BossStyles.Ukhumo, health: 380, enragedBonus: 0.25f, theme: MusicPlayer.Theme.BeastBoss, themeTitleKey: nameof(UIText.Boss4Music));

    public static readonly BossDefinition Definition = Builder.Build(
        Builder.Calm(Attacks.ClawRake()),
        Builder.Calm(Attacks.RoarOfTheMountain()),
        Builder.Calm(Attacks.Stomp()),
        Builder.Calm(Attacks.BruteForce()),
        Builder.Enraged(Attacks.CrossingClaws()),
        Builder.Enraged(Attacks.CaveIn()),
        Builder.Enraged(Attacks.Feast()));
}
