using System.Collections.Generic;

/// <summary>Every stage of the game.</summary>
public static class Stages
{
    public static readonly StageDefinition One = new StageDefinition(
        nameof(UIText.StageTitle), nameof(UIText.StageMusic), MusicPlayer.Theme.Stage, Stage1Waves.Waves, IrysshBoss.Definition)
    {
        Hud = HudTheme.FireSerpent,
        Intro = () => Dialogues.StageOneIntro, BossArrival = () => Dialogues.BeforeBossOne,
        BossEncounter = () => Dialogues.BeforeBossTwo, Ending = () => Dialogues.AfterBoss,
    };

    /// <summary>The river of the parties. Its music is still that of stage 1, and so are the sprites.</summary>
    public static readonly StageDefinition Two = new StageDefinition(
        nameof(UIText.Stage2Title), nameof(UIText.StageMusic), MusicPlayer.Theme.Stage, Stage2Waves.Waves, BayloBoss.Definition)
    {
        Scenery = Scenery.Festival, Hud = HudTheme.Festival, Spirits = SpiritStyle.Party,
        Intro = () => Dialogues.StageTwoIntro,
        BossEncounter = () => Dialogues.StageTwoMeeting, Ending = () => Dialogues.StageTwoEnding,
    };

    /// <summary>The stages in the order they are played.</summary>
    public static readonly IReadOnlyList<StageDefinition> All = new[] { One, Two };

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
