/// <summary>
/// The boss of the first stage. She rises from the river as a woman and keeps that shape to the end. Five attacks, one life each: the first three
/// calm (the spiral of embers, and the lotus whose petals stop and then dart at the boat), the third calm too (the great fireballs that break), the last two enraged (the lunge down the screen with its two trails that fall like waves, and the tail of the serpent of hell: two giant lasers), with other music, extra effects and a redder river. The other attacks she had (see <see cref="Attacks"/>) are kept
/// for later bosses.
/// </summary>
public static class IrysshBoss
{
    public const string NameKey = "IrysshName";

    // Hitbox: a third of the width of the sprite (96 points).
    private static readonly BossBuilder Builder = new BossBuilder(NameKey, 32, BossStyles.Iryssh, enragedBonus: 0.10f);

    public static readonly BossDefinition Definition = Builder.Build(
        Builder.Calm(Attacks.RingOfEmbers()),
        Builder.Calm(Attacks.BlazingBloom()),
        Builder.Calm(Attacks.ShatteringFireballs()),
        Builder.Enraged(Attacks.TideOfEmbers()),
        Builder.Enraged(Attacks.SerpentTail()));
}
