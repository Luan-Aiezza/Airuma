using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

/// <summary>Every piece of text the player reads (apart from the boss's attack names, which live with the phases).
/// Kept in one place so the game can be translated, and so tests can check that the pixel font covers all of it.</summary>
public static class UIText
{
    /// <summary>The title and the text of a card, looked up in the current language ("CardX.Title" and "CardX.Detail").</summary>
    public readonly struct CardText
    {
        private readonly string _key;
        public CardText(string key) { _key = key; }
        public string Title => Loc.Get(_key + ".Title");
        public string Detail => Loc.Get(_key + ".Detail");
    }


    /// Every piece of text the player reads (apart from the boss's attack names, which live with the phases).
    /// Kept in one place so the game can be translated, and so tests can check that the pixel font covers all of it.
    public static string Title => Loc.Get(nameof(Title));

    // Menu
    public static string MenuStart => Loc.Get(nameof(MenuStart));
    public static string MenuBossPractice => Loc.Get(nameof(MenuBossPractice));
    public static string MenuScores => Loc.Get(nameof(MenuScores));
    public static string MenuMusicRoom => Loc.Get(nameof(MenuMusicRoom));
    public static string MenuQuit => Loc.Get(nameof(MenuQuit));
    public static string MenuConfig => Loc.Get(nameof(MenuConfig));
    public static string Back => Loc.Get(nameof(Back));
    public static string Menu => Loc.Get(nameof(Menu));
    public static string ComingSoon => Loc.Get(nameof(ComingSoon));
    public static string PracticeStages => Loc.Get(nameof(PracticeStages));
    public static string PracticeBosses => Loc.Get(nameof(PracticeBosses));
    public static string PracticeStageTitle => Loc.Get(nameof(PracticeStageTitle));
    public static string PracticeTitle => Loc.Get(nameof(PracticeTitle));
    public static string ScoresTitle => Loc.Get(nameof(ScoresTitle));
    public static string NoScore => Loc.Get(nameof(NoScore));
    public static string Cleared => Loc.Get(nameof(Cleared));
    public static string MenuHint => Loc.Get(nameof(MenuHint));
    public static string CfgTitle => Loc.Get(nameof(CfgTitle));
    public static string CfgLives => Loc.Get(nameof(CfgLives));
    public static string CfgBombs => Loc.Get(nameof(CfgBombs));
    public static string CfgMusic => Loc.Get(nameof(CfgMusic));
    public static string CfgSfx => Loc.Get(nameof(CfgSfx));
    public static string CfgMode => Loc.Get(nameof(CfgMode));
    public static string CfgFullscreen => Loc.Get(nameof(CfgFullscreen));
    public static string CfgWindowed => Loc.Get(nameof(CfgWindowed));
    public static string CfgControls => Loc.Get(nameof(CfgControls));
    public static string CfgDefaults => Loc.Get(nameof(CfgDefaults));
    public static string CfgExit => Loc.Get(nameof(CfgExit));
    public static string CfgKeyUp => Loc.Get(nameof(CfgKeyUp));
    public static string CfgKeyDown => Loc.Get(nameof(CfgKeyDown));
    public static string CfgKeyLeft => Loc.Get(nameof(CfgKeyLeft));
    public static string CfgKeyRight => Loc.Get(nameof(CfgKeyRight));
    public static string CfgKeyShoot => Loc.Get(nameof(CfgKeyShoot));
    public static string CfgKeyBomb => Loc.Get(nameof(CfgKeyBomb));
    public static string CfgKeyFocus => Loc.Get(nameof(CfgKeyFocus));
    public static string CfgPressKey => Loc.Get(nameof(CfgPressKey));
    public static string CfgHint => Loc.Get(nameof(CfgHint));
    public static string Stage6Title => Loc.Get(nameof(Stage6Title));
    public static string KillaName => Loc.Get(nameof(KillaName));
    public static string IllariName => Loc.Get(nameof(IllariName));
    public static string Stage2Music => Loc.Get(nameof(Stage2Music));
    public static string Boss2Music => Loc.Get(nameof(Boss2Music));
    public static string Stage3Music => Loc.Get(nameof(Stage3Music));
    public static string Boss3Music => Loc.Get(nameof(Boss3Music));
    public static string Stage4Music => Loc.Get(nameof(Stage4Music));
    public static string Boss4Music => Loc.Get(nameof(Boss4Music));
    public static string Stage5Music => Loc.Get(nameof(Stage5Music));
    public static string Boss6Music => Loc.Get(nameof(Boss6Music));
    public static string Boss5Music => Loc.Get(nameof(Boss5Music));
    public static string Stage6Music => Loc.Get(nameof(Stage6Music));
    public static string IntroHint => Loc.Get(nameof(IntroHint));
    public static string MenuMusic => Loc.Get(nameof(MenuMusic));
    public static string SadEndingMusic => Loc.Get(nameof(SadEndingMusic));
    public static string HappyEndingMusic => Loc.Get(nameof(HappyEndingMusic));
    public static string MusicRoomTitle => Loc.Get(nameof(MusicRoomTitle));
    public static string RestartsLeft => Loc.Get(nameof(RestartsLeft));
    public static string GameOver => Loc.Get(nameof(GameOver));
    public static string GameOverMusic => Loc.Get(nameof(GameOverMusic));
    public static string TheEnd => Loc.Get(nameof(TheEnd));
    public static string DemoLabel => Loc.Get(nameof(DemoLabel));

    // Side panel
    public static string HiScore => Loc.Get(nameof(HiScore));
    public static string Score => Loc.Get(nameof(Score));
    public static string Lives => Loc.Get(nameof(Lives));
    public static string Lantern => Loc.Get(nameof(Lantern));
    public static string Power => Loc.Get(nameof(Power));
    public static string Wind => Loc.Get(nameof(Wind));

    public static string WindCalm => Loc.Get(nameof(WindCalm));
    public static string WindNone => Loc.Get(nameof(WindNone));
    public static string North => Loc.Get(nameof(North));
    public static string South => Loc.Get(nameof(South));
    public static string East => Loc.Get(nameof(East));
    public static string West => Loc.Get(nameof(West));
    public static string[] WindStrengths => new[] { Loc.Get("WindWeak"), Loc.Get("WindMedium"), Loc.Get("WindStrong") };

    public static string FairyName => Loc.Get(nameof(FairyName));
    public static string FairyRole => Loc.Get(nameof(FairyRole));

    // End screen
    public static string Victory => Loc.Get(nameof(Victory));
    public static string Defeat => Loc.Get(nameof(Defeat));
    public static string Restart => Loc.Get(nameof(Restart));
    public static string Paused => Loc.Get(nameof(Paused));
    public static string Continue => Loc.Get(nameof(Continue));
    public static string GiveUp => Loc.Get(nameof(GiveUp));
    public static string PauseHint => Loc.Get(nameof(PauseHint));
    public static string NewRecord => Loc.Get(nameof(NewRecord));

    // Airu's lines, in reaction to what happens
    public static string SayStart => Loc.Get(nameof(SayStart));
    public static string SayHit => Loc.Get(nameof(SayHit));
    public static string SayBomb => Loc.Get(nameof(SayBomb));
    public static string SayRage => Loc.Get(nameof(SayRage));
    public static string SayEnraged => Loc.Get(nameof(SayEnraged));
    public static string SayVictory => Loc.Get(nameof(SayVictory));
    public static string SayDefeat => Loc.Get(nameof(SayDefeat));
    public static string SayPowerUp => Loc.Get(nameof(SayPowerUp));
    public static string SayFullPower => Loc.Get(nameof(SayFullPower));
    public static string SayBombRefill => Loc.Get(nameof(SayBombRefill));
    public static string SayLifeRefill => Loc.Get(nameof(SayLifeRefill));


    // Cards
    public static readonly CardText CardHomingWisp = new CardText(nameof(CardHomingWisp));
    public static readonly CardText CardTwinFan = new CardText(nameof(CardTwinFan));
    public static readonly CardText CardPiercingLight = new CardText(nameof(CardPiercingLight));
    public static readonly CardText CardFervor = new CardText(nameof(CardFervor));
    public static readonly CardText CardQuickWick = new CardText(nameof(CardQuickWick));
    public static readonly CardText CardBurstingBolts = new CardText(nameof(CardBurstingBolts));
    public static readonly CardText CardSpiritWard = new CardText(nameof(CardSpiritWard));
    public static readonly CardText CardMendingWard = new CardText(nameof(CardMendingWard));
    public static readonly CardText CardSlimWake = new CardText(nameof(CardSlimWake));
    public static readonly CardText CardCalmSails = new CardText(nameof(CardCalmSails));
    public static readonly CardText CardBrighterLight = new CardText(nameof(CardBrighterLight));
    public static readonly CardText CardLongBurn = new CardText(nameof(CardLongBurn));
    public static readonly CardText CardSearingLight = new CardText(nameof(CardSearingLight));
    public static readonly CardText CardSpareLantern = new CardText(nameof(CardSpareLantern));
    public static readonly CardText CardHungryLantern = new CardText(nameof(CardHungryLantern));
    public static readonly CardText CardWideNet = new CardText(nameof(CardWideNet));
    public static readonly CardText CardGoldenSpirits = new CardText(nameof(CardGoldenSpirits));
    public static readonly CardText CardSerpentFlame = new CardText(nameof(CardSerpentFlame));
    public static readonly CardText CardPartyFever = new CardText(nameof(CardPartyFever));
    public static readonly CardText CardBearCloak = new CardText(nameof(CardBearCloak));
    public static readonly CardText CardStormBolt = new CardText(nameof(CardStormBolt));
    public static string ChooseCard => Loc.Get(nameof(ChooseCard));
    public static string ChooseCardStart => Loc.Get(nameof(ChooseCardStart));
    public static string ChooseCardBoss => Loc.Get(nameof(ChooseCardBoss));
    public static string CardsHint => Loc.Get(nameof(CardsHint));
    public static string YourCards => Loc.Get(nameof(YourCards));
    public static string SayCard => Loc.Get(nameof(SayCard));
    public static string SayWard => Loc.Get(nameof(SayWard));


    // Dialogue: the boy and his lantern, at the start of stage 1
    public static string BoyName => Loc.Get(nameof(BoyName));
    // Dialogue: the lake, before the boss, and the end
    public static string DialogueHint => Loc.Get(nameof(DialogueHint));


    // Music titles, shown when a part of the game begins
    public static string StageMusic => Loc.Get(nameof(StageMusic));
    public static string BossMusic => Loc.Get(nameof(BossMusic));
    public static string LockedBoss => Loc.Get(nameof(LockedBoss));

    // Difficulty
    public static string DifficultyTitle => Loc.Get(nameof(DifficultyTitle));
    public static string DifficultyEasy => Loc.Get(nameof(DifficultyEasy));
    public static string DifficultyNormal => Loc.Get(nameof(DifficultyNormal));
    public static string DifficultyHard => Loc.Get(nameof(DifficultyHard));

    // Stage 2: the river of the parties, and Baylo
    public static string Stage3Title => Loc.Get(nameof(Stage3Title));
    public static string Stage4Title => Loc.Get(nameof(Stage4Title));
    public static string Stage5Title => Loc.Get(nameof(Stage5Title));
    public static string KuychuName => Loc.Get(nameof(KuychuName));
    public static string SerpentLeftName => Loc.Get(nameof(SerpentLeftName));
    public static string SerpentRightName => Loc.Get(nameof(SerpentRightName));
    public static string UkhumoName => Loc.Get(nameof(UkhumoName));
    public static string ThaqaiName => Loc.Get(nameof(ThaqaiName));
    public static string Stage2Title => Loc.Get(nameof(Stage2Title));
    public static string BayloName => Loc.Get(nameof(BayloName));
    public static string AttackDoubleStep => Loc.Get(nameof(AttackDoubleStep));
    public static string AttackConfettiRain => Loc.Get(nameof(AttackConfettiRain));
    public static string AttackSambaWheel => Loc.Get(nameof(AttackSambaWheel));
    public static string AttackDolphinLeap => Loc.Get(nameof(AttackDolphinLeap));
    public static string AttackPartyWave => Loc.Get(nameof(AttackPartyWave));

    // Stage
    public static string StageTitle => Loc.Get(nameof(StageTitle));


    /// Everything above, for tests.


    public static List<string> CardTexts => Enum.GetValues<Card>().SelectMany(c => new[] { CardInfo.Title(c), CardInfo.Detail(c) }).ToList();


    /// <summary>Every text, in the current language (for tests): each text property, the cards, the wind strengths and the names of the attacks of every boss.</summary>
    public static List<string> All()
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
        var list = new List<string>();
        foreach (var property in typeof(UIText).GetProperties(flags))
            if (property.PropertyType == typeof(string)) list.Add((string)property.GetValue(null));
        list.AddRange(CardTexts);
        list.AddRange(WindStrengths);
        foreach (var stage in Stages.All)
        {
            list.Add(stage.Title); list.Add(stage.Boss.Name);
            for (int l = 1; l <= stage.Boss.Lives; l++) list.Add(stage.Boss.AttackName(l));
            foreach (var talk in stage.Talks()) foreach (var line in talk) list.Add(line.Text);
        }
        return list;
    }
}
