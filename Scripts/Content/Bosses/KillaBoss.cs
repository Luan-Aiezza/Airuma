/// <summary>
/// The boss of stage 6, a PROTOTYPE to fit the flow of the stage into: Mama Killa, the Moon, who took Illari so that the night would never end. Six attacks (4 calm and 2 enraged:
/// the crescent moon and then the full one). For now she wears borrowed art (the divine person of stage 5) and the attacks of the cat of the stars (stage 3), whose mistress she is,
/// ending with the eclipse; the attacks of the phases of the moon, with the tide that turns the wind, are still to be made.
/// </summary>
public static class KillaBoss
{
    public const string NameKey = "KillaName";

    private static readonly BossBuilder Builder = new BossBuilder(NameKey, 32, BossStyles.Killa, health: 420, enragedHealth: 680, theme: MusicPlayer.Theme.CosmicBoss, themeTitleKey: nameof(UIText.Boss6Music));

    /// <summary>Where her position is when her skirt touches the horizon of the water: she stands exactly under its line.</summary>
    public static readonly float StandY = Layout.Horizon - BossStyles.Killa.Waterline;

    /// <summary>She stands under the horizon: her attacks begin there and she never goes above it, only to the sides and down.</summary>
    private static BossPhase Grounded(BossPhase attack)
    {
        attack.Target = new Godot.Vector2(attack.Target.X, StandY);
        attack.TwinTarget = attack.Target;
        attack.MinY = StandY;
        return attack;
    }

    public static readonly BossDefinition Definition = Build();

    private static BossDefinition Build()
    {
        var definition = Builder.Build(
            Builder.Calm(Grounded(Attacks.StarPentagram())),
            Builder.Calm(Grounded(Attacks.ZigzagLightning())),
            Builder.Calm(Grounded(Attacks.FineWeave())),
            Builder.Calm(Grounded(Attacks.HailCurtain())),
            Builder.Calm(Grounded(Attacks.KillaCrescent())),
            Builder.Calm(Grounded(Attacks.KillaTide())),
            Builder.Enraged(Grounded(Attacks.SkyFalls())),
            Builder.Enraged(Grounded(Attacks.Eclipse())),
            Builder.Enraged(Grounded(Attacks.KillaEclipse())),
            Builder.Enraged(Grounded(Attacks.KillaEternalNight())));
        definition.StandY = StandY;
        return definition;
    }
}
