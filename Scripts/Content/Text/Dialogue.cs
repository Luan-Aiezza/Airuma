using System.Collections.Generic;

/// <summary>Who speaks. `Boss` is the boss of the stage (the name is the stage's boss's).</summary>
public enum DialogueSpeaker { Atli, Airu, Boss }

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
}
