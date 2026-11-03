using Godot;

/// <summary>What the player has done across runs: which stages were cleared (their boss beaten in a real run). The Music Room unlocks the music of a stage and of its boss by it.</summary>
public static class Progress
{
    private const string Path = "user://progress.cfg";
    private const string Section = "airuma";

    private static int Mask
    {
        get
        {
            var cfg = new ConfigFile();
            return cfg.Load(Path) == Error.Ok && cfg.HasSectionKey(Section, "cleared") ? (int)cfg.GetValue(Section, "cleared") : 0;
        }
    }

    /// <summary>Whether the stage (0 for the first) was cleared in some run.</summary>
    public static bool IsCleared(int stage) => stage >= 0 && (Mask & (1 << stage)) != 0;

    public static void MarkCleared(int stage)
    {
        if (stage < 0 || IsCleared(stage)) return;
        var cfg = new ConfigFile();
        cfg.Load(Path);
        cfg.SetValue(Section, "cleared", Mask | (1 << stage));
        cfg.Save(Path);
    }
}
