/// <summary>Every stage of the game.</summary>
public static class Stages
{
    public static readonly StageDefinition One = new StageDefinition(
        UIText.StageTitle, UIText.StageMusic, MusicPlayer.Theme.Stage, Stage1Waves.Waves, IrysshBoss.Definition);
}
