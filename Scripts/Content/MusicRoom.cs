using System.Collections.Generic;

/// <summary>One song of the Music Room: its theme, the key of its title, and the stage (0 for the first) that must be cleared to hear it, or -1 if it is always open.</summary>
public readonly record struct Track(MusicPlayer.Theme Theme, string TitleKey, int StageRequired)
{
    public bool Unlocked => StageRequired < 0 || Progress.IsCleared(StageRequired);
}

/// <summary>The songs of the game in the order a player meets them: the opening and the menu, each stage and its boss, then the endings.</summary>
public static class MusicRoom
{
    public static readonly IReadOnlyList<Track> Tracks = new[]
    {
        new Track(MusicPlayer.Theme.Menu, nameof(UIText.MenuMusic), -1),
        new Track(MusicPlayer.Theme.Stage, nameof(UIText.StageMusic), 0),
        new Track(MusicPlayer.Theme.Boss, nameof(UIText.BossMusic), 0),
        new Track(MusicPlayer.Theme.NightStage, nameof(UIText.Stage2Music), 1),
        new Track(MusicPlayer.Theme.DancerBoss, nameof(UIText.Boss2Music), 1),
        new Track(MusicPlayer.Theme.StormStage, nameof(UIText.Stage3Music), 2),
        new Track(MusicPlayer.Theme.CatBoss, nameof(UIText.Boss3Music), 2),
        new Track(MusicPlayer.Theme.CaveStage, nameof(UIText.Stage4Music), 3),
        new Track(MusicPlayer.Theme.BeastBoss, nameof(UIText.Boss4Music), 3),
        new Track(MusicPlayer.Theme.DivineStage, nameof(UIText.Stage5Music), 4),
        new Track(MusicPlayer.Theme.SerpentBoss, nameof(UIText.Boss5Music), 4),
        new Track(MusicPlayer.Theme.CosmicStage, nameof(UIText.Stage6Music), 5),
        new Track(MusicPlayer.Theme.CosmicBoss, nameof(UIText.Boss6Music), 5),
        new Track(MusicPlayer.Theme.SadEnding, nameof(UIText.SadEndingMusic), -1),
        new Track(MusicPlayer.Theme.HappyEnding, nameof(UIText.HappyEndingMusic), -1),
    };
}
