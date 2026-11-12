/// <summary>
/// The boss of stage 6, a PROTOTYPE to fit the flow of the stage into: Mama Killa, the Moon, who took Ariah so that the night would never end. Ten attacks (6 calm and 4 enraged): the phases of the moon,
/// from the new moon to the eternal night (see Attacks.Cosmos.cs).
/// </summary>
public static class KillaBoss
{
    public const string NameKey = "KillaName";

    private static readonly BossBuilder Builder = new BossBuilder(NameKey, 32, BossStyles.Killa, health: 450, enragedBonus: 0.35f, theme: MusicPlayer.Theme.CosmicBoss, themeTitleKey: nameof(UIText.Boss6Music));

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
            Builder.Calm(Grounded(Attacks.KillaNewMoon())),
            Builder.Calm(Grounded(Attacks.KillaCrescent())),
            Builder.Calm(Grounded(Attacks.KillaTide())),
            Builder.Calm(Grounded(Attacks.KillaFullMoon())),
            Builder.Calm(Grounded(Attacks.KillaConstellations())),
            Builder.Calm(Grounded(Attacks.KillaSilverRain())),
            Builder.Enraged(Grounded(Attacks.KillaWaning())),
            Builder.Enraged(Grounded(Attacks.KillaEclipse())),
            Builder.Enraged(Grounded(Attacks.KillaPentagram())),
            Builder.Enraged(Grounded(Attacks.KillaEternalNight())));
        definition.StandY = StandY;
        return definition;
    }
}
