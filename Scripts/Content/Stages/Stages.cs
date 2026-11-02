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
        Scenery = Scenery.Waterfall, Spirits = SpiritStyle.Andes,
        Intro = () => Dialogues.StageFiveIntro,
        BossEncounter = () => Dialogues.StageFiveMeeting, Ending = () => Dialogues.StageFiveEnding,
    };

    /// <summary>The stages in the order they are played.</summary>
    public static readonly IReadOnlyList<StageDefinition> All = new[] { One, Two, Three, Four, Five };

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
