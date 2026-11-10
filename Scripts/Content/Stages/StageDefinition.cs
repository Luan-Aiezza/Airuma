using System.Collections.Generic;

/// <summary>
/// A stage as data: the waves of spirits that come first, then the boss, with the titles and the music of each part. A new stage is a
/// new definition (new waves, a new boss); the simulation runs whichever it is given.
/// </summary>
public sealed class StageDefinition
{
    private readonly string _titleKey, _musicTitleKey;
    public string Title => Loc.Get(_titleKey);
    public string MusicTitle => Loc.Get(_musicTitleKey);
    public readonly MusicPlayer.Theme Theme;
    public readonly IReadOnlyList<Wave> Waves;
    public readonly BossDefinition Boss;
    /// <summary>How the stage looks: the scenery of the river, and the HUD panel (each stage has its own, made after its boss).</summary>
    public Scenery Scenery = Scenery.Jungle;
    public System.Func<HudTheme> Hud = HudTheme.Original;
    /// <summary>The special card this stage's boss may leave behind when beaten (null: none).</summary>
    public Card? BossCard;
    /// <summary>How the common enemies of the stage look.</summary>
    public SpiritStyle Spirits = SpiritStyle.Forest;

    /// <summary>The talks of the stage, as functions (a talk is made when it is needed, in the language of the moment); any may be null.
    /// The opening, before the waves; the arrival of the boss; the boss appearing and speaking; and what is said after her defeat.</summary>
    public System.Func<IReadOnlyList<DialogueLine>> Intro, BossArrival, BossEncounter, Ending;

    /// <summary>The stage ends with a great light that whitens the screen and then goes dark (the card of the end is chosen in the dark), and the next stage opens from the dark.</summary>
    public bool LightTransition;
    /// <summary>The stage opens from the dark: the screen is black and clears while the boat rises.</summary>
    public bool OpensFromDark;
    /// <summary>The keys of the paragraphs of the story that is told, over a picture, after the last fight; null if the stage has none.</summary>
    public System.Func<string[]> Epilogue;

    /// <summary>All the talks of the stage that it has, as lines.</summary>
    public IEnumerable<IReadOnlyList<DialogueLine>> Talks()
    {
        foreach (var talk in new[] { Intro, BossArrival, BossEncounter, Ending }) if (talk != null) yield return talk();
    }

    public StageDefinition(string titleKey, string musicTitleKey, MusicPlayer.Theme theme, IReadOnlyList<Wave> waves, BossDefinition boss)
    {
        _titleKey = titleKey; _musicTitleKey = musicTitleKey; Theme = theme; Waves = waves; Boss = boss;
    }
}
