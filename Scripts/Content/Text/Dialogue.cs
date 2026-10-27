using System.Collections.Generic;

public enum DialogueSpeaker { Atli, Airu, Iryssh }

/// <summary>How a speaker looks while saying a line (named Emotion because Godot already has a class called Expression).</summary>
public enum Emotion { Neutral, Annoyed, Sheepish, Surprised, Happy, Worried }

public struct DialogueLine
{
    public DialogueSpeaker Speaker;
    public Emotion Emotion;
    public string Text;
    /// <summary>The line is shouted: the box shakes as it appears.</summary>
    public bool Shout;

    public DialogueLine(DialogueSpeaker speaker, Emotion emotion, string text, bool shout = false)
    {
        Speaker = speaker; Emotion = emotion; Text = text; Shout = shout;
    }
}

/// <summary>The scripted conversations. Light and quick, in the manner of Touhou's chatter before a stage.</summary>
public static class Dialogues
{
    public static string NameOf(DialogueSpeaker speaker) => speaker switch
    {
        DialogueSpeaker.Atli => UIText.BoyName,
        DialogueSpeaker.Airu => UIText.FairyName,
        _ => IrysshBoss.Name,
    };

    /// <summary>Stage 1, at the start: the boy and his lantern are lost.</summary>
    public static IReadOnlyList<DialogueLine> StageOneIntro => new[]
    {
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, UIText.Intro01),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, UIText.Intro02),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, UIText.Intro03, shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, UIText.Intro04),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, UIText.Intro05),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, UIText.Intro06),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, UIText.Intro07, shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, UIText.Intro08),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, UIText.Intro09, shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, UIText.Intro10),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, UIText.Intro11),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, UIText.Intro12, shout: true),
    };

    /// <summary>Before the boss, part one: they reach a lake, and the water goes still.</summary>
    public static IReadOnlyList<DialogueLine> BeforeBossOne => new[]
    {
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, UIText.BossA1),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Surprised, UIText.BossA2),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, UIText.BossA3),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, UIText.BossA4),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, UIText.BossA5),
    };

    /// <summary>Before the boss, part two: she rises, and is not pleased.</summary>
    public static IReadOnlyList<DialogueLine> BeforeBossTwo => new[]
    {
        new DialogueLine(DialogueSpeaker.Iryssh, Emotion.Annoyed, UIText.BossB1, shout: true),
        new DialogueLine(DialogueSpeaker.Iryssh, Emotion.Neutral, UIText.BossB2),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, UIText.BossB3),
        new DialogueLine(DialogueSpeaker.Iryssh, Emotion.Annoyed, UIText.BossB4, shout: true),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, UIText.BossB5),
        new DialogueLine(DialogueSpeaker.Iryssh, Emotion.Annoyed, UIText.BossB6, shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, UIText.BossB7),
    };

    /// <summary>The boss has had enough: she throws off her human shape.</summary>
    public static IReadOnlyList<DialogueLine> Transformation => new[]
    {
        new DialogueLine(DialogueSpeaker.Iryssh, Emotion.Annoyed, UIText.Morph1, shout: true),
        new DialogueLine(DialogueSpeaker.Iryssh, Emotion.Neutral, UIText.Morph2, shout: true),
    };

    /// <summary>After the boss: it is better that she sleeps forever.</summary>
    public static IReadOnlyList<DialogueLine> AfterBoss => new[]
    {
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Worried, UIText.EndE1),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, UIText.EndE2),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, UIText.EndE3, shout: true),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Sheepish, UIText.EndE4),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Happy, UIText.EndE5),
        new DialogueLine(DialogueSpeaker.Atli, Emotion.Neutral, UIText.EndE6),
        new DialogueLine(DialogueSpeaker.Airu, Emotion.Annoyed, UIText.EndE7),
    };
}
