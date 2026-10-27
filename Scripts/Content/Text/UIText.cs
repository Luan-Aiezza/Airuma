using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Every piece of text the player reads (apart from the boss's attack names, which live with the phases).
/// Kept in one place so the game can be translated, and so tests can check that the pixel font covers all of it.</summary>
public static class UIText
{
    public struct CardText { public readonly string Title; public readonly string Detail; public CardText(string title, string detail) { Title = title; Detail = detail; } }


    /// Every piece of text the player reads (apart from the boss's attack names, which live with the phases).
    /// Kept in one place so the game can be translated, and so tests can check that the pixel font covers all of it.
    public static readonly string Title = "Airumã";

    // Menu
    public static readonly string MenuStart = "Start";
    public static readonly string MenuBossPractice = "Boss Practice";
    public static readonly string MenuScores = "Scores";
    public static readonly string MenuMusicRoom = "Music Room";
    public static readonly string MenuConfig = "Config";
    public static readonly string Back = "Back";
    public static readonly string Menu = "Menu";
    public static readonly string ComingSoon = "Coming soon";
    public static readonly string PracticeTitle = "Boss Practice";
    public static readonly string ScoresTitle = "Scores";
    public static readonly string NoScore = "-------";
    public static readonly string Cleared = "Clear";
    public static readonly string MenuHint = "Z: Select   X: Back";

    // Side panel
    public static readonly string HiScore = "Best";
    public static readonly string Score = "Score";
    public static readonly string Lives = "Lives";
    public static readonly string Lantern = "Lantern";
    public static readonly string Power = "Power";
    public static readonly string Wind = "Wind";

    public static readonly string WindCalm = "Calm";
    public static readonly string WindNone = "No wind";
    public static readonly string North = "North";
    public static readonly string South = "South";
    public static readonly string East = "East";
    public static readonly string West = "West";
    public static readonly string[] WindStrengths = { "Weak", "Medium", "Strong" };

    public static readonly string FairyName = "Airu";
    public static readonly string FairyRole = "Lamp Fairy";

    // End screen
    public static readonly string Victory = "Victory!";
    public static readonly string Defeat = "Defeat...";
    public static readonly string Restart = "Restart";
    public static readonly string Paused = "Paused";
    public static readonly string Continue = "Continue";
    public static readonly string GiveUp = "Give Up";
    public static readonly string PauseHint = "Esc: Continue";
    public static readonly string NewRecord = "New record!";

    // Airu's lines, in reaction to what happens
    public static readonly string SayStart = "Let's go!";
    public static readonly string SayHit = "Ouch! Hang in there!";
    public static readonly string SayBomb = "The light protects!";
    public static readonly string SayEnraged = "She's furious!";
    public static readonly string SaySerpent = "She turned into a serpent!";
    public static readonly string SayVictory = "We did it!";
    public static readonly string SayDefeat = "Don't give up! Try again.";
    public static readonly string SayPowerUp = "More light!";
    public static readonly string SayFullPower = "Full light!";
    public static readonly string SayBombRefill = "A spare light for the lantern!";
    public static readonly string SayLifeRefill = "A spare boat! Don't sink it!";


    // Cards
    public static readonly CardText CardHomingWisp = new CardText("Homing Wisp", "A wisp of light follows your lantern and hunts the nearest foe.");
    public static readonly CardText CardTwinFan = new CardText("Twin Fan", "Two lighter bolts fan out from every shot.");
    public static readonly CardText CardPiercingLight = new CardText("Piercing Light", "Your bolts pass through one more foe.");
    public static readonly CardText CardFervor = new CardText("Fervor", "+25% damage with every shot.");
    public static readonly CardText CardQuickWick = new CardText("Quick Wick", "+20% fire rate.");
    public static readonly CardText CardBurstingBolts = new CardText("Bursting Bolts", "Bolts burst on impact and hurt the foes around.");
    public static readonly CardText CardSpiritWard = new CardText("Spirit Ward", "A ward of light blocks the next hit.");
    public static readonly CardText CardMendingWard = new CardText("Mending Ward", "A ward blocks a hit, and mends itself every 45 seconds.");
    public static readonly CardText CardSlimWake = new CardText("Slim Wake", "Your boat is harder to hit: smaller hitbox.");
    public static readonly CardText CardCalmSails = new CardText("Calm Sails", "The wind pushes you 40% less.");
    public static readonly CardText CardBrighterLight = new CardText("Brighter Light", "+20% bomb radius.");
    public static readonly CardText CardLongBurn = new CardText("Long Burn", "Bombs last 1 second longer.");
    public static readonly CardText CardSearingLight = new CardText("Searing Light", "+50% bomb damage.");
    public static readonly CardText CardSpareLantern = new CardText("Spare Lantern", "Refills one bomb.");
    public static readonly CardText CardHungryLantern = new CardText("Hungry Lantern", "Lights give +50% energy.");
    public static readonly CardText CardWideNet = new CardText("Wide Net", "+40% reach for pulling in lights.");
    public static readonly CardText CardGoldenSpirits = new CardText("Golden Spirits", "+25% score.");
    public static readonly string ChooseCard = "Choose a card";
    public static readonly string ChooseCardStart = "A new stage begins";
    public static readonly string ChooseCardBoss = "Before the boss";
    public static readonly string CardsHint = "Left Right: Pick   Z: Take";
    public static readonly string YourCards = "Your cards";
    public static readonly string SayCard = "Nice pick!";
    public static readonly string SayWard = "The ward held!";


    // Dialogue: the boy and his lantern, at the start of stage 1
    public static readonly string BoyName = "Atli";
    public static readonly string Intro01 = "Atli. Quick question. Where are we?";
    public static readonly string Intro02 = "On the river! Obviously!";
    public static readonly string Intro03 = "WHICH part of the river?!";
    public static readonly string Intro04 = "The... wet part. Hehe.";
    public static readonly string Intro05 = "That tree looks familiar. So did twelve others.";
    public static readonly string Intro06 = "Trees look alike. That's what makes them trees.";
    public static readonly string Intro07 = "We're LOST, Atli!";
    public static readonly string Intro08 = "Scenic route. Fast. Whoosh!";
    public static readonly string Intro09 = "Then scenic-route LEFT! Your other left! Argh!";
    public static readonly string Intro10 = "Wait... rustle, rustle... something's moving.";
    public static readonly string Intro11 = "Locals! Ask for directions. Politely!";
    public static readonly string Intro12 = "They're glowing. And cranky. HOLD ON!";
    // Dialogue: the lake, before the boss, and the end
    public static readonly string BossA1 = "Atli, the trees are gone! A lake!";
    public static readonly string BossA2 = "Open water, full moon. Wow...";
    public static readonly string BossA3 = "We've never been here, genius.";
    public static readonly string BossA4 = "So it can't be the wrong way!";
    public static readonly string BossA5 = "...The water just went very still.";
    public static readonly string BossB1 = "WHO splashes in my lake at this hour?!";
    public static readonly string BossB2 = "My beauty sleep takes TIME, you know.";
    public static readonly string BossB3 = "We... were just passing through?";
    public static readonly string BossB4 = "Through my bedchamber! With a LANTERN!";
    public static readonly string BossB5 = "Polite lantern. I only shine at things.";
    public static readonly string BossB6 = "Then shine on your GRAVE!";
    public static readonly string BossB7 = "Airu, stay lit. Long night ahead.";
    public static readonly string EndE1 = "Is she... asleep again?";
    public static readonly string EndE2 = "Out cold. Snoring, even. Zzz...";
    public static readonly string EndE3 = "Her beauty sleep nearly ROASTED us!";
    public static readonly string EndE4 = "Maybe she should sleep forever.";
    public static readonly string EndE5 = "Agreed. Tiptoe now. Run later!";
    public static readonly string EndE6 = "So... which way is out?";
    public static readonly string EndE7 = "...Don't look at me. Hmph.";
    public static readonly string DialogueHint = "Z: Next   X: Skip";

    public static List<string> DialogueTexts => new List<string> { BoyName, DialogueHint, Intro01, Intro02, Intro03, Intro04, Intro05, Intro06,
        Intro07, Intro08, Intro09, Intro10, Intro11, Intro12,
        BossA1, BossA2, BossA3, BossA4, BossA5, BossB1, BossB2, BossB3, BossB4, BossB5, BossB6, BossB7, EndE1, EndE2, EndE3, EndE4, EndE5, EndE6, EndE7, Morph1, Morph2 };

    // Music titles, shown when a part of the game begins
    public static readonly string StageMusic = "\u266A Ember Serpent";
    public static readonly string BossMusic = "\u266A Inferno Serpent";
    public static readonly string BossTwoMusic = "\u266A Serpent Ablaze";
    public static readonly string SerpentName = "Great Serpent";
    public static readonly string Morph1 = "ENOOOUGH!";
    public static readonly string Morph2 = "BEHOLD my true form!";
    public static readonly string LockedBoss = "?????";

    // Difficulty
    public static readonly string DifficultyTitle = "Difficulty";
    public static readonly string DifficultyEasy = "Easy";
    public static readonly string DifficultyNormal = "Normal";
    public static readonly string DifficultyHard = "Hard";

    // Stage
    public static readonly string StageTitle = "Stage 1";


    /// Everything above, for tests.


    public static List<string> CardTexts => Enum.GetValues<Card>().SelectMany(c => new[] { CardInfo.Title(c), CardInfo.Detail(c) }).ToList();

    public static List<string> AiruSays => new List<string> { SayStart, SayHit, SayBomb, SayEnraged, SaySerpent, SayVictory, SayDefeat, SayPowerUp, SayFullPower, SayCard, SayWard, SayBombRefill, SayLifeRefill };

    /// <summary>Everything above, for tests.</summary>
    public static List<string> All()
    {
        var list = new List<string> { Title, MenuStart, MenuBossPractice, MenuScores, MenuMusicRoom, MenuConfig, Back, Menu, ComingSoon, PracticeTitle,
            ScoresTitle, NoScore, Cleared, MenuHint, HiScore, Score, Lives, Lantern, Power, Wind, WindCalm, WindNone, North, South, East, West,
            StageTitle, FairyName, FairyRole, Victory, Defeat, Restart, Paused, Continue, GiveUp, PauseHint, NewRecord, IrysshBoss.Name, SerpentName, LockedBoss };
        list.AddRange(new[] { ChooseCard, ChooseCardStart, ChooseCardBoss, CardsHint, YourCards });
        list.AddRange(CardTexts);
        list.AddRange(WindStrengths);
        list.AddRange(AiruSays);
        list.AddRange(DialogueTexts);
        list.AddRange(new[] { StageMusic, BossMusic, BossTwoMusic, DifficultyTitle, DifficultyEasy, DifficultyNormal, DifficultyHard });
        for (int l = 1; l <= 6; l++) { var n = Stages.One.Boss.AttackName(l); if (n != null) list.Add(n); }
        return list;
    }
}