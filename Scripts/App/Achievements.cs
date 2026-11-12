using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Where the achievements are kept. The game uses a file in the player's folder; the tests use the memory.
/// </summary>
public interface IAchievementStore
{
    HashSet<string> Load();
    void Save(HashSet<string> unlocked);
}

/// <summary>The achievements in a file of the player's folder (user://achievements.cfg).</summary>
public sealed class FileAchievementStore : IAchievementStore
{
    private const string Path = "user://achievements.cfg", Section = "airuma";

    public HashSet<string> Load()
    {
        var cfg = new ConfigFile();
        if (cfg.Load(Path) != Error.Ok || !cfg.HasSectionKey(Section, "unlocked")) return new HashSet<string>();
        return cfg.GetValue(Section, "unlocked").AsStringArray().ToHashSet();
    }

    public void Save(HashSet<string> unlocked)
    {
        var cfg = new ConfigFile();
        cfg.SetValue(Section, "unlocked", unlocked.OrderBy(s => s).ToArray());
        cfg.Save(Path);
    }
}

/// <summary>
/// Whatever receives the achievements besides the game's own file: Steam, when it is there (see the notes in <see cref="Achievements"/>). It is told every achievement that is unlocked,
/// and, when it is attached, those that were unlocked before (so that nothing earned without Steam is lost).
/// </summary>
public interface IAchievementBackend
{
    void Unlock(string id);
}

/// <summary>
/// The achievements of the game: 20 of them. One for each boss beaten in each difficulty (`BOSS{1..6}_{EASY|NORMAL|HARD}`; the easy mode ends at the fifth boss, and its sixth is the joke
/// `EASY_JOKE`, unlocked with the bad ending), and one for each ending (`ENDING_BAD`, `ENDING_TRUE`). Practice and the demonstration never unlock any.
///
/// The ids are the API names to register in the Steamworks site (Stats &amp; Achievements). To use Steam: add the Steamworks.NET package, write an <see cref="IAchievementBackend"/> whose Unlock
/// calls `SteamUserStats.SetAchievement(id)` and `StoreStats()`, and hand it to <see cref="Attach"/> once Steam has started (and call `SteamAPI.RunCallbacks()` every frame).
/// </summary>
public static class Achievements
{
    public const int Count = 20;

    public enum Kind { Boss, EasyJoke, EndingBad, EndingTrue }

    public sealed class Info
    {
        public string Id;
        public Kind Kind;
        /// <summary>The stage (0 for the first) of a boss achievement.</summary>
        public int Stage;
        public Difficulty Difficulty;
        /// <summary>Hidden until it is unlocked (the joke).</summary>
        public bool Hidden;
    }

    private static readonly string[] Levels = { "EASY", "NORMAL", "HARD" };

    /// <summary>The id of the achievement for the boss of a stage (0 for the first) beaten on a difficulty; null if there is none (the easy mode has no sixth boss).</summary>
    public static string BossId(int stage, Difficulty difficulty) =>
        stage < 0 || stage > 5 || (stage == 5 && difficulty == Difficulty.Easy) ? null : $"BOSS{stage + 1}_{Levels[(int)difficulty]}";

    public const string EasyJoke = "EASY_JOKE", EndingBad = "ENDING_BAD", EndingTrue = "ENDING_TRUE";

    /// <summary>All of them, in the order of the list.</summary>
    public static readonly IReadOnlyList<Info> All = Build();

    private static List<Info> Build()
    {
        var list = new List<Info>();
        for (int stage = 0; stage < 6; stage++)
            foreach (var difficulty in new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard })
            {
                if (BossId(stage, difficulty) is string id) list.Add(new Info { Id = id, Kind = Kind.Boss, Stage = stage, Difficulty = difficulty });
                else list.Add(new Info { Id = EasyJoke, Kind = Kind.EasyJoke, Stage = stage, Difficulty = difficulty, Hidden = true });
            }
        list.Add(new Info { Id = EndingBad, Kind = Kind.EndingBad });
        list.Add(new Info { Id = EndingTrue, Kind = Kind.EndingTrue });
        return list;
    }

    private static readonly Dictionary<string, Info> ById = All.ToDictionary(a => a.Id);

    public static Info Get(string id) => ById.TryGetValue(id, out var info) ? info : null;

    // MARK: State

    /// <summary>Where the unlocked ones are kept. (The tests put a memory here.)</summary>
    public static IAchievementStore Store = new FileAchievementStore();

    private static HashSet<string> _unlocked;
    private static IAchievementBackend _backend;

    /// <summary>One has just been unlocked (the first time only): the game shows a notice.</summary>
    public static event Action<Info> Unlocked;

    private static HashSet<string> Set => _unlocked ??= Store.Load();

    public static bool IsUnlocked(string id) => Set.Contains(id);

    public static int UnlockedCount => Set.Count;

    /// <summary>Forgets what was read (the next question reads the store again).</summary>
    public static void Reload() { _unlocked = null; }

    /// <summary>Hands the achievements over to Steam (or another service): all that were unlocked so far are sent, and the ones to come as they happen.</summary>
    public static void Attach(IAchievementBackend backend)
    {
        _backend = backend;
        if (backend == null) return;
        foreach (var id in Set.OrderBy(s => s)) backend.Unlock(id);
    }

    /// <summary>Unlocks an achievement (once only; an unknown id is ignored). True when it was new.</summary>
    public static bool Unlock(string id)
    {
        if (id == null || !ById.TryGetValue(id, out var info) || !Set.Add(id)) return false;
        Store.Save(Set);
        _backend?.Unlock(id);
        Unlocked?.Invoke(info);
        return true;
    }

    // MARK: The moments of the game

    /// <summary>The boss of a stage (0 for the first) was beaten, in a real run, on a difficulty.</summary>
    public static void BossDefeated(int stage, Difficulty difficulty) => Unlock(BossId(stage, difficulty));

    /// <summary>The bad ending (the easy mode) was seen to its end: the ending, and the joke about the sixth boss that the easy mode does not have.</summary>
    public static void BadEndingSeen() { Unlock(EndingBad); Unlock(EasyJoke); }

    /// <summary>The ending of the story (normal and hard) was seen to its end.</summary>
    public static void TrueEndingSeen() => Unlock(EndingTrue);
}
