using System.Collections.Generic;

/// <summary>Who speaks. `Boss` is the boss of the stage (the name is the stage's boss's); the others are friends and foes who speak in one stage only.</summary>
public enum DialogueSpeaker { Atli, Airu, Boss, Ukhumo, LeftSerpent, RightSerpent, Illari }

/// <summary>How a speaker looks while saying a line (named Emotion because Godot already has a class called Expression).</summary>
public enum Emotion { Neutral, Annoyed, Sheepish, Surprised, Happy, Worried }

/// <summary>
/// A line of a talk. It holds the key of its text (see <see cref="Loc"/>), so a talk is written once, in the code, and its words live only in the
/// tables of the languages; the text is looked up when it is shown, in the language of the moment. (A text that is not in the tables is shown as
/// it is, which is how tests write their lines.)
/// </summary>
public struct DialogueLine
{
    public DialogueSpeaker Speaker;
    public Emotion Emotion;
    public string Key;
    /// <summary>The line is shouted: the box shakes as it appears.</summary>
    public bool Shout;

    public DialogueLine(DialogueSpeaker speaker, Emotion emotion, string key, bool shout = false)
    {
        Speaker = speaker; Emotion = emotion; Key = key; Shout = shout;
    }

    public string Text => Loc.Get(Key);
}

/// <summary>The scripted conversations. Light and quick, in the manner of Touhou's chatter before a stage.</summary>
public static class Dialogues
{
    /// <summary>Stage 1, at the start: the boy and his lantern are lost.</summary>
    public static IReadOnlyList<DialogueLine> StageOneIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "Intro01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "Intro02"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "Intro03", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "Intro04"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "Intro05"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "Intro06"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "Intro07", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "Intro08"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, "Intro09", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "Intro10"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "Intro11"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "Intro12", shout: true),
    };

    /// <summary>Before the boss, part one: they reach a lake, and the water goes still.</summary>
    public static IReadOnlyList<DialogueLine> BeforeBossOne => new[]
    {
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, "BossA1"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "BossA2"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "BossA3"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "BossA4"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "BossA5"),
    };

    /// <summary>Before the boss, part two: she rises, and is not pleased.</summary>
    public static IReadOnlyList<DialogueLine> BeforeBossTwo => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "BossB1", shout: true),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "BossB2"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "BossB3"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "BossB4", shout: true),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "BossB5"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "BossB6", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "BossB7"),
    };

    /// <summary>After the boss: it is better that she sleeps forever.</summary>
    public static IReadOnlyList<DialogueLine> AfterBoss => new[]
    {
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "EndE1"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "EndE2"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "EndE3", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "EndE4"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, "EndE5"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "EndE6"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "EndE7"),
    };

    // MARK: Stage 2, the river of the parties

    /// <summary>Stage 2, at the start: the trail comes back, and with it a party.</summary>
    public static IReadOnlyList<DialogueLine> StageTwoIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Happy, "S2Intro01"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S2Intro02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S2Intro03"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S2Intro04"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S2Intro05"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S2Intro06", shout: true),
    };

    /// <summary>Before Baylo: he comes dancing and will not take no for an answer.</summary>
    public static IReadOnlyList<DialogueLine> StageTwoMeeting => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2Meet01"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2Meet02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S2Meet03"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2Meet04"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S2Meet05"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2Meet06", shout: true),
    };

    /// <summary>After Baylo: he is out of breath, and, for the fun of it, tells them where the boat went.</summary>
    public static IReadOnlyList<DialogueLine> StageTwoEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2End01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S2End02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S2End03"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S2End04"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Surprised, "S2End05"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2End06"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Annoyed, "S2End07"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2End08"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2End09"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S2End10"),
    };

    // MARK: Stage 3, the lake of the hail

    /// <summary>Stage 3, at the start: the hail begins, and Atli has no hat.</summary>
    public static IReadOnlyList<DialogueLine> StageThreeIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Worried, "S3Intro01"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Surprised, "S3Intro02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S3Intro03", shout: true),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S3Intro04"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S3Intro05"),
    };

    /// <summary>Before Thaqai: she bars the way, on orders she will not tell.</summary>
    public static IReadOnlyList<DialogueLine> StageThreeMeeting => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S3Meet01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S3Meet02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S3Meet03"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S3Meet04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S3Meet05", shout: true),
    };

    /// <summary>After Thaqai: she lets "she" slip, and Airu works out that it must be a god, and that the great serpent may know.</summary>
    public static IReadOnlyList<DialogueLine> StageThreeEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Worried, "S3End01"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Worried, "S3End02", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S3End03"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Surprised, "S3End04", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S3End05"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S3End06"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S3End07"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S3End08"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S3End09"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Neutral, "S3End10"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S3End11"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Neutral, "S3End12"),
    };

    // MARK: Stage 4, the river of the cave

    /// <summary>Stage 4, at the start: the dark of the cave, and Atli's stomach.</summary>
    public static IReadOnlyList<DialogueLine> StageFourIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Worried, "S4Intro01"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S4Intro02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S4Intro03"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S4Intro04"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Happy, "S4Intro05"),
    };

    /// <summary>Before Ukhumo: she smells the toasted corn, and she is hungry.</summary>
    public static IReadOnlyList<DialogueLine> StageFourMeeting => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S4Meet01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S4Meet02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S4Meet03"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S4Meet04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S4Meet05", shout: true),
    };

    /// <summary>After Ukhumo: Atli gives her his snack, and she will guide them through the tunnels to the great serpent.</summary>
    public static IReadOnlyList<DialogueLine> StageFourEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Worried, "S4End01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S4End02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Surprised, "S4End03"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S4End04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S4End05"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S4End06"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S4End07"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Neutral, "S4End08"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Surprised, "S4End09"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S4End10"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S4End11"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S4End12"),
    };

    // MARK: Stage 5, the waterfall of the seven colours

    /// <summary>Stage 5, at the start: Ukhumo leaves them at the foot of the fall, and warns them about the two heads.</summary>
    public static IReadOnlyList<DialogueLine> StageFiveIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Ukhumo, Emotion.Neutral, "S5Intro01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Happy, "S5Intro02"),
        new DialogueLine(DialogueSpeaker.Ukhumo, Emotion.Happy, "S5Intro03"),
        new DialogueLine(DialogueSpeaker.Ukhumo, Emotion.Worried, "S5Intro04"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S5Intro05"),
    };

    /// <summary>Before Kuychu: the two serpents that he wears speak first, and he asks for proof of their worth, which is a fight.</summary>
    public static IReadOnlyList<DialogueLine> StageFiveMeeting => new[]
    {
        new DialogueLine(DialogueSpeaker.LeftSerpent, Emotion.Neutral, "S5Meet01"),
        new DialogueLine(DialogueSpeaker.RightSerpent, Emotion.Annoyed, "S5Meet02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5Meet03"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S5Meet04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5Meet05"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S5Meet06"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5Meet07", shout: true),
    };

    /// <summary>After Kuychu: he knows who took Illari and will not say, but he will carry them to her, up the fall, by a bridge of water.</summary>
    public static IReadOnlyList<DialogueLine> StageFiveEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5End01"),
        new DialogueLine(DialogueSpeaker.LeftSerpent, Emotion.Happy, "S5End02"),
        new DialogueLine(DialogueSpeaker.RightSerpent, Emotion.Annoyed, "S5End03"),
        new DialogueLine(DialogueSpeaker.LeftSerpent, Emotion.Annoyed, "S5End04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5End05"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S5End06"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End07"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5End08"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S5End09"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End10"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S5End11"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End12"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End13"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End14"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Worried, "S5End15"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Happy, "S5End16"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S5End17"),
    };

    // MARK: Stage 6, the river of stars

    private static DialogueLine L(DialogueSpeaker who, Emotion e, string key, bool shout = false) => new DialogueLine(who, e, key, shout);

    /// <summary>Stage 6, at the start: the water of the sky, shining, and the trail goes on.</summary>
    public static IReadOnlyList<DialogueLine> StageSixIntro => new[]
    {
        L(DialogueSpeaker.Atli, Emotion.Surprised, "S6Intro01"), L(DialogueSpeaker.Airu, Emotion.Worried, "S6Intro02"),
        L(DialogueSpeaker.Atli, Emotion.Happy, "S6Intro03"), L(DialogueSpeaker.Airu, Emotion.Neutral, "S6Intro04"),
    };

    /// <summary>Before the goddess: they find Illari asleep on the ground, in the middle of the river, and guess whose cat it was.</summary>
    public static IReadOnlyList<DialogueLine> StageSixArrival => new[]
    {
        L(DialogueSpeaker.Atli, Emotion.Surprised, "S6Arr01"), L(DialogueSpeaker.Airu, Emotion.Worried, "S6Arr02"), L(DialogueSpeaker.Atli, Emotion.Worried, "S6Arr03"),
    };

    /// <summary>The goddess comes down and sends the sleeping Illari away (at the first line), and the longer talk that ends with the fight.</summary>
    public static IReadOnlyList<DialogueLine> StageSixMeeting => new[]
    {
        L(DialogueSpeaker.Boss, Emotion.Neutral, "S6Meet01"), L(DialogueSpeaker.Boss, Emotion.Annoyed, "S6Meet02"), L(DialogueSpeaker.Atli, Emotion.Surprised, "S6Meet03"),
        L(DialogueSpeaker.Boss, Emotion.Neutral, "S6Meet04"), L(DialogueSpeaker.Airu, Emotion.Annoyed, "S6Meet05"), L(DialogueSpeaker.Boss, Emotion.Annoyed, "S6Meet06"),
        L(DialogueSpeaker.Atli, Emotion.Neutral, "S6Meet07"), L(DialogueSpeaker.Boss, Emotion.Neutral, "S6Meet08"), L(DialogueSpeaker.Airu, Emotion.Worried, "S6Meet09"),
        L(DialogueSpeaker.Boss, Emotion.Annoyed, "S6Meet10", shout: true),
    };

    /// <summary>The line (of <see cref="StageSixEnding"/>) at which the goddess brings Illari back, asleep, and the line at which she wakes.</summary>
    public const int StageSixReturnLine = 13, StageSixWakeLine = 15;

    /// <summary>After the goddess: tired, she is talked round; she brings Illari back, who wakes and speaks with the three.</summary>
    public static IReadOnlyList<DialogueLine> StageSixEnding => new[]
    {
        L(DialogueSpeaker.Boss, Emotion.Worried, "S6End01"), L(DialogueSpeaker.Atli, Emotion.Neutral, "S6End02"), L(DialogueSpeaker.Boss, Emotion.Neutral, "S6End03"),
        L(DialogueSpeaker.Atli, Emotion.Neutral, "S6End04"), L(DialogueSpeaker.Boss, Emotion.Happy, "S6End05"), L(DialogueSpeaker.Atli, Emotion.Neutral, "S6End06"),
        L(DialogueSpeaker.Airu, Emotion.Annoyed, "S6End07"), L(DialogueSpeaker.Atli, Emotion.Neutral, "S6End08"), L(DialogueSpeaker.Boss, Emotion.Sheepish, "S6End09"),
        L(DialogueSpeaker.Atli, Emotion.Happy, "S6End10"), L(DialogueSpeaker.Boss, Emotion.Sheepish, "S6End11"), L(DialogueSpeaker.Atli, Emotion.Happy, "S6End12"),
        L(DialogueSpeaker.Boss, Emotion.Sheepish, "S6End13"), L(DialogueSpeaker.Atli, Emotion.Surprised, "S6End14"), L(DialogueSpeaker.Boss, Emotion.Neutral, "S6End15"),
        L(DialogueSpeaker.Illari, Emotion.Surprised, "S6End16"), L(DialogueSpeaker.Atli, Emotion.Happy, "S6End17"), L(DialogueSpeaker.Illari, Emotion.Happy, "S6End18"),
        L(DialogueSpeaker.Illari, Emotion.Worried, "S6End19"), L(DialogueSpeaker.Boss, Emotion.Sheepish, "S6End20"), L(DialogueSpeaker.Atli, Emotion.Happy, "S6End21"),
        L(DialogueSpeaker.Airu, Emotion.Happy, "S6End22"),
    };

    /// <summary>The story told when the game opens, over a picture of the night: the keys of its paragraphs.</summary>
    public static readonly string[] IntroStory = { "IntroPage1", "IntroPage2", "IntroPage3", "IntroPage4" };

    /// <summary>The story after the last fight, told over a picture: the keys of its paragraphs.</summary>
    public static readonly string[] StageSixEpilogue = { "S6Epi01", "S6Epi02", "S6Epi03", "S6Epi04" };
}
