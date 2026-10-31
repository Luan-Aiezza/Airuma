using System;
using Godot;

/// <summary>
/// What the game was asked to do from the command line (the user args, after `--`):
///   --selftest             run the logic tests and quit with the number of failures
///   --game                 skip the menu and start a run (--difficulty=easy|normal|hard, --stage=N)
///   --practice=N           start practice of attack N of the boss of --stage=N directly
///   --autoplay             a test pilot plays (shoots, weaves, picks card 0, bombs now and then)
///   --shot=file.png        save the picture after a few frames (--shot-frames=N, 90 by default) and quit
///   --export-...=path      the tools of <see cref="DevTools"/> (art, sounds and music rendered to files)
/// </summary>
public sealed class LaunchOptions
{
    public readonly string[] Args;
    public bool SelfTest, Game, Autoplay;
    public int? Practice;
    public int Stage;                           // 0 for the first stage
    public Difficulty Difficulty = Difficulty.Normal;
    public string Shot;
    public int ShotFrames = 90;

    public LaunchOptions(string[] args)
    {
        Args = args;
        SelfTest = Has("--selftest");
        Game = Has("--game");
        Autoplay = Has("--autoplay");
        if (Value("--practice") is string practice && int.TryParse(practice, out int p)) Practice = p;
        if (Value("--stage") is string stage && int.TryParse(stage, out int n)) Stage = Math.Max(0, n - 1);
        if (Value("--difficulty") is string level && Enum.TryParse(level, true, out Difficulty parsed)) Difficulty = parsed;
        Shot = Value("--shot");
        if (Value("--shot-frames") is string frames && int.TryParse(frames, out int f)) ShotFrames = f;
    }

    public static LaunchOptions FromCommandLine() => new LaunchOptions(OS.GetCmdlineUserArgs());

    private bool Has(string flag) => Array.IndexOf(Args, flag) >= 0;

    /// <summary>The value of `--name=value`, or null.</summary>
    public string Value(string name)
    {
        foreach (var a in Args) if (a.StartsWith(name + "=")) return a.Substring(name.Length + 1);
        return null;
    }
}
