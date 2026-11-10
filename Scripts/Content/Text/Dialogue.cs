using System.Collections.Generic;

/// <summary>Who speaks. `Boss` is the boss of the stage (the name is the stage's boss's); the others are friends and foes who speak in one stage only. `Narrator` is no one: a sound or a happening written in the box, with no name.</summary>
public enum DialogueSpeaker { Atli, Airu, Boss, Ukhumo, LeftSerpent, RightSerpent, Narrator }

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

/// <summary>The scripted conversations. Light and quick, in the manner of Touhou's chatter before a stage. They are generated from `Docs/Dialogos.md`: the words are in the tables of the languages.</summary>
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
    };

    /// <summary>Before the boss, part one: they reach a lake, and the water goes still.</summary>
    public static IReadOnlyList<DialogueLine> BeforeBossOne => new[]
    {
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, "BossA1"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "BossA2"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "BossA3"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "BossA4"),
        new DialogueLine(DialogueSpeaker.Narrator, Emotion.Neutral, "BossA5"),
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
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "EndE1"),
    };

    /// <summary>Stage 2, at the start: the trail comes back, and with it a party.</summary>
    public static IReadOnlyList<DialogueLine> StageTwoIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Happy, "S2Intro01"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S2Intro02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S2Intro03"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S2Intro04"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S2Intro05"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Worried, "S2Intro06"),
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

    /// <summary>After Baylo: he is out of breath, and, for the fun of it, tells them where to go.</summary>
    public static IReadOnlyList<DialogueLine> StageTwoEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2End01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S2End02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S2End03"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S2End04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2End05"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S2End06"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2End07"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Annoyed, "S2End08"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2End09"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S2End10"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S2End11"),
    };

    /// <summary>Stage 3, at the start: the hail begins.</summary>
    public static IReadOnlyList<DialogueLine> StageThreeIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Worried, "S3Intro01", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Annoyed, "S3Intro02", shout: true),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Surprised, "S3Intro03", shout: true),
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

    /// <summary>After Thaqai: she lets slip that her mistress watches through the moon, and Naru works out that the Great Serpent may know.</summary>
    public static IReadOnlyList<DialogueLine> StageThreeEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Worried, "S3End01"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Worried, "S3End02", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S3End03"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Surprised, "S3End04", shout: true),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Worried, "S3End05", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S3End06"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S3End07"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S3End08"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S3End09"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S3End10"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S3End11"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Neutral, "S3End12"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Neutral, "S3End13"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S3End14"),
    };

    /// <summary>Stage 4, at the start: the dark of the cave.</summary>
    public static IReadOnlyList<DialogueLine> StageFourIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Worried, "S4Intro01"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S4Intro02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S4Intro03"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S4Intro04", shout: true),
    };

    /// <summary>Before Ukhumo: she smells meat, and she is hungry.</summary>
    public static IReadOnlyList<DialogueLine> StageFourMeeting => new[]
    {
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Surprised, "S4Meet01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S4Meet02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Worried, "S4Meet03"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S4Meet04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S4Meet05"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Surprised, "S4Meet06"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S4Meet07", shout: true),
    };

    /// <summary>After Ukhumo: Atli gives her his roasted fish, and she will guide them to the Great Serpent.</summary>
    public static IReadOnlyList<DialogueLine> StageFourEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Worried, "S4End01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, "S4End02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Surprised, "S4End03", shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S4End04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S4End05", shout: true),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Neutral, "S4End06"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S4End07"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S4End08"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S4End09"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Surprised, "S4End10"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Neutral, "S4End11"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S4End12", shout: true),
    };

    /// <summary>Stage 5, at the start: Ukhumo leaves them at the foot of the fall, and warns them about the guardians.</summary>
    public static IReadOnlyList<DialogueLine> StageFiveIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Ukhumo, Emotion.Neutral, "S5Intro01"),
        new DialogueLine(DialogueSpeaker.Ukhumo, Emotion.Happy, "S5Intro02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S5Intro03"),
        new DialogueLine(DialogueSpeaker.Ukhumo, Emotion.Happy, "S5Intro04"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, "S5Intro05"),
        new DialogueLine(DialogueSpeaker.Ukhumo, Emotion.Worried, "S5Intro06"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S5Intro07"),
    };

    /// <summary>Before Kuychu: the two serpents speak first, and ask for proof of their worth, which is a fight.</summary>
    public static IReadOnlyList<DialogueLine> StageFiveMeeting => new[]
    {
        new DialogueLine(DialogueSpeaker.LeftSerpent, Emotion.Neutral, "S5Meet01"),
        new DialogueLine(DialogueSpeaker.RightSerpent, Emotion.Annoyed, "S5Meet02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Annoyed, "S5Meet03"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S5Meet04"),
        new DialogueLine(DialogueSpeaker.LeftSerpent, Emotion.Neutral, "S5Meet05"),
        new DialogueLine(DialogueSpeaker.RightSerpent, Emotion.Annoyed, "S5Meet06"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Annoyed, "S5Meet07"),
        new DialogueLine(DialogueSpeaker.LeftSerpent, Emotion.Neutral, "S5Meet08"),
        new DialogueLine(DialogueSpeaker.RightSerpent, Emotion.Annoyed, "S5Meet09", shout: true),
    };

    /// <summary>After Kuychu (normal and hard): he blames a goddess and sends them to the spirit realm with a great light.</summary>
    public static IReadOnlyList<DialogueLine> StageFiveEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5End01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S5End02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End03"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S5End04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5End05"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5End06"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S5End07"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End08"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End09"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S5End10"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End11"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Worried, "S5End12", shout: true),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S5End13", shout: true),
    };

    /// <summary>Easy mode ends at stage 5: Kuychu calls them unworthy and turns them back (the bad ending follows).</summary>
    public static IReadOnlyList<DialogueLine> StageFiveEasyEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5Easy01"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S5Easy02", shout: true),
    };

    /// <summary>Stage 6, at the start: the water of the sky, shining, and the trail goes on.</summary>
    public static IReadOnlyList<DialogueLine> StageSixIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S6Intro01"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S6Intro02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S6Intro03"),
    };

    /// <summary>Before the goddess: they see the great moon, and someone is coming.</summary>
    public static IReadOnlyList<DialogueLine> StageSixArrival => new[]
    {
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Surprised, "S6Arr01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Worried, "S6Arr02"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S6Arr03"),
    };

    /// <summary>The goddess comes down, and the talk that ends with the fight.</summary>
    public static IReadOnlyList<DialogueLine> StageSixMeeting => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S6Meet01"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S6Meet02"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S6Meet03"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S6Meet04"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, "S6Meet05"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Annoyed, "S6Meet06", shout: true),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S6Meet07"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, "S6Meet08"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S6Meet09"),
    };

    /// <summary>After the goddess: she is talked round and gives the day back.</summary>
    public static IReadOnlyList<DialogueLine> StageSixEnding => new[]
    {
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Worried, "S6End01"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, "S6End02"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S6End03", shout: true),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S6End04"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Annoyed, "S6End05"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Worried, "S6End06"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Sheepish, "S6End07"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Happy, "S6End08"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Sheepish, "S6End09"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Happy, "S6End10"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Happy, "S6End11"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, "S6End12"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Sheepish, "S6End13"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, "S6End14"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S6End15"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Neutral, "S6End16"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S6End17"),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Happy, "S6End18"),
        new DialogueLine(DialogueSpeaker.Boss, Emotion.Happy, "S6End19"),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, "S6End20"),
    };

    /// <summary>The story told when the game opens, over a picture of the night: the keys of its paragraphs.</summary>
    public static readonly string[] IntroStory = { "IntroPage1", "IntroPage2" };

    /// <summary>The bad ending (easy mode): the boy and the bear fishing in the endless night, over a picture: the keys of its paragraphs.</summary>
    public static readonly string[] BadEndStory = { "BadEnd1", "BadEnd2", "BadEnd3", "BadEnd4" };

    /// <summary>The story after the last fight, told over a picture: the keys of its paragraphs.</summary>
    public static readonly string[] StageSixEpilogue = { "S6Epi01", "S6Epi02", "S6Epi03" };
}
