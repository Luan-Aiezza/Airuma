using System.Collections.Generic;

/// <summary>Every stage of the game.</summary>
public static class Stages
{
    public static readonly StageDefinition One = new StageDefinition(
        nameof(UIText.StageTitle), nameof(UIText.StageMusic), MusicPlayer.Theme.Stage, Stage1Waves.Waves, IrysshBoss.Definition)
    {
        BossCard = Card.SerpentFlame, Hud = HudTheme.FireSerpent,
        Intro = () => Dialogues.StageOneIntro, BossArrival = () => Dialogues.BeforeBossOne,
        BossEncounter = () => Dialogues.BeforeBossTwo, Ending = () => Dialogues.AfterBoss,
    };

    /// <summary>The river of the parties. Its music is still that of stage 1, and so are the sprites.</summary>
    public static readonly StageDefinition Two = new StageDefinition(
        nameof(UIText.Stage2Title), nameof(UIText.StageMusic), MusicPlayer.Theme.Stage, Stage2Waves.Waves, BayloBoss.Definition)
    {
        BossCard = Card.PartyFever, Scenery = Scenery.Festival, Hud = HudTheme.Festival, Spirits = SpiritStyle.Party,
        Intro = () => Dialogues.StageTwoIntro,
        BossEncounter = () => Dialogues.StageTwoMeeting, Ending = () => Dialogues.StageTwoEnding,
    };

    /// <summary>The lake of the hail. The black forest in a storm: its scenery is the storm, its spirits are small animals; its HUD has the storm and the stars.</summary>
    public static readonly StageDefinition Three = new StageDefinition(
        nameof(UIText.Stage3Title), nameof(UIText.StageMusic), MusicPlayer.Theme.Stage, Stage3Waves.Waves, ThaqaiBoss.Definition)
    {
        BossCard = Card.StormBolt, Scenery = Scenery.Storm, Hud = HudTheme.Storm, Spirits = SpiritStyle.Animals,
        Intro = () => Dialogues.StageThreeIntro,
        BossEncounter = () => Dialogues.StageThreeMeeting, Ending = () => Dialogues.StageThreeEnding,
    };

    /// <summary>The river inside the mountain: dark, with ice. Its art (banks, spirits, boss) is still borrowed: this is a sketch to play.</summary>
    public static readonly StageDefinition Four = new StageDefinition(
        nameof(UIText.Stage4Title), nameof(UIText.StageMusic), MusicPlayer.Theme.Stage, Stage4Waves.Waves, UkhumoBoss.Definition)
    {
        BossCard = Card.BearCloak, Scenery = Scenery.Cave, Hud = HudTheme.Cave, Spirits = SpiritStyle.Cave,
        Intro = () => Dialogues.StageFourIntro,
        BossEncounter = () => Dialogues.StageFourMeeting, Ending = () => Dialogues.StageFourEnding,
    };

    /// <summary>The waterfall of the seven colours: a prototype (the art of the other stages is borrowed). Its red spirits come in pairs.</summary>
    public static readonly StageDefinition Five = new StageDefinition(
        nameof(UIText.Stage5Title), nameof(UIText.StageMusic), MusicPlayer.Theme.Stage, Stage5Waves.Waves, KuychuBoss.Definition)
    {
        Scenery = Scenery.Waterfall, Hud = HudTheme.Andes, Spirits = SpiritStyle.Andes, LightTransition = true,
        Intro = () => Dialogues.StageFiveIntro,
        BossEncounter = () => Dialogues.StageFiveMeeting, Ending = () => Dialogues.StageFiveEnding,
    };

    /// <summary>
    /// The river of stars, in the realm of the spirits, the domain of Mama Killa: the last stage, and the one with every kind of spirit (the red ones in up to three). It opens from the
    /// dark after the light that ends stage 5; the priestess sleeps on the ground in the middle of the river until the goddess sends her away, and is brought back after the fight; and the
    /// end of the story is told after it. Its art is still borrowed from the other stages.
    /// </summary>
    public static readonly StageDefinition Six = new StageDefinition(
        nameof(UIText.Stage6Title), nameof(UIText.StageMusic), MusicPlayer.Theme.Stage, Stage6Waves.Waves, KillaBoss.Definition)
    {
        Scenery = Scenery.Storm, Hud = HudTheme.Storm, Spirits = SpiritStyle.Andes,
        OpensFromDark = true, Priestess = true,
        Intro = () => Dialogues.StageSixIntro, BossArrival = () => Dialogues.StageSixArrival,
        BossEncounter = () => Dialogues.StageSixMeeting, Ending = () => Dialogues.StageSixEnding,
        Epilogue = () => Dialogues.StageSixEpilogue,
    };

    /// <summary>The stages in the order they are played.</summary>
    public static readonly IReadOnlyList<StageDefinition> All = new[] { One, Two, Three, Four, Five, Six };

    /// <summary>The place of a stage in the order they are played (0 for the first), or -1.</summary>
    public static int IndexOf(StageDefinition stage)
    {
        for (int i = 0; i < All.Count; i++) if (All[i] == stage) return i;
        return -1;
    }

    /// <summary>The stage that follows `stage`, or null after the last.</summary>
    public static StageDefinition After(StageDefinition stage)
    {
        int index = IndexOf(stage);
        return index >= 0 && index + 1 < All.Count ? All[index + 1] : null;
    }
}
