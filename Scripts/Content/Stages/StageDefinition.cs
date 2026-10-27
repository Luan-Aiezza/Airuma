using System.Collections.Generic;

/// <summary>
/// A stage as data: the waves of spirits that come first, then the boss, with the titles and the music of each part. A new stage is a
/// new definition (new waves, a new boss); the simulation runs whichever it is given.
/// </summary>
public sealed class StageDefinition
{
    public readonly string Title;
    public readonly string MusicTitle;
    public readonly MusicPlayer.Theme Theme;
    public readonly IReadOnlyList<Wave> Waves;
    public readonly BossDefinition Boss;

    public StageDefinition(string title, string musicTitle, MusicPlayer.Theme theme, IReadOnlyList<Wave> waves, BossDefinition boss)
    {
        Title = title; MusicTitle = musicTitle; Theme = theme; Waves = waves; Boss = boss;
    }
}
