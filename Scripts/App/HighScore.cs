using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>One finished run in the score table.</summary>
public struct ScoreEntry
{
    public int Score;
    /// <summary>Whether the run ended with the boss defeated.</summary>
    public bool Cleared;
    /// <summary>The difficulty the run was played on.</summary>
    public Difficulty Difficulty;
    public ScoreEntry(int score, bool cleared, Difficulty difficulty = Difficulty.Normal) { Score = score; Cleared = cleared; Difficulty = difficulty; }
}

/// <summary>The best runs on this device: a top five, best first.</summary>
public static class HighScore
{
    public const int Capacity = 5;
    private const string Path = "user://scores.cfg";
    private const string Section = "airuma";

    /// <summary>
    /// The version of the table. The first one (no version in the file) was written when the difficulties were one step easier: its Normal is today's Easy and its Hard today's Normal. A table of that version is
    /// brought up to date when it is read (the old Easy, which no longer exists, is today's Easy as well), and saved again.
    /// </summary>
    private const int Version = 2;

    public static List<ScoreEntry> Entries
    {
        get
        {
            var cfg = new ConfigFile();
            var result = new List<ScoreEntry>();
            if (cfg.Load(Path) != Error.Ok) return result;
            bool legacy = !cfg.HasSectionKey(Section, "version");
            if (cfg.HasSectionKey(Section, "scores"))
            {
                int[] scores = cfg.GetValue(Section, "scores").AsInt32Array();
                int[] cleared = cfg.HasSectionKey(Section, "cleared") ? cfg.GetValue(Section, "cleared").AsInt32Array() : Array.Empty<int>();
                int[] levels = cfg.HasSectionKey(Section, "difficulty") ? cfg.GetValue(Section, "difficulty").AsInt32Array() : Array.Empty<int>();
                for (int i = 0; i < scores.Length; i++)
                {
                    int level = i < levels.Length ? Math.Clamp(levels[i], 0, 2) : 1;
                    if (legacy) level = Math.Max(0, level - 1);                   // old Easy and old Normal are Easy now, old Hard is Normal
                    result.Add(new ScoreEntry(scores[i], i < cleared.Length && cleared[i] != 0, (Difficulty)level));
                }
                if (legacy) Entries = result;                                      // saved again, in the new version
                return result;
            }
            // The table that predates it only kept the best score.
            int old = cfg.HasSectionKey(Section, "highscore") ? (int)cfg.GetValue(Section, "highscore") : 0;
            if (old > 0) result.Add(new ScoreEntry(old, false, Difficulty.Easy));          // (it was a Normal of the first scheme)
            return result;
        }
        set
        {
            var table = value.Take(Capacity).ToList();
            var cfg = new ConfigFile();
            cfg.SetValue(Section, "version", Version);
            cfg.SetValue(Section, "scores", table.Select(e => e.Score).ToArray());
            cfg.SetValue(Section, "cleared", table.Select(e => e.Cleared ? 1 : 0).ToArray());
            cfg.SetValue(Section, "difficulty", table.Select(e => (int)e.Difficulty).ToArray());
            cfg.Save(Path);
            _best = table.Count > 0 ? table.Max(e => e.Score) : 0;
        }
    }

    /// <summary>The best score, kept in memory (the HUD asks for it at every frame: the file is read once, and again only if it is written).</summary>
    private static int? _best;

    /// <summary>The best score.</summary>
    public static int Current
    {
        get
        {
            if (_best is int cached) return cached;
            var e = Entries;
            _best = e.Count > 0 ? e.Max(x => x.Score) : 0;
            return _best.Value;
        }
        set { Entries = value > 0 ? new List<ScoreEntry> { new ScoreEntry(value, false) } : new List<ScoreEntry>(); }
    }

    /// <summary>Adds a run to the table if it makes the top five. Returns whether it is the new best score.</summary>
    public static bool Submit(int score, bool cleared = false, Difficulty difficulty = Difficulty.Normal)
    {
        if (score <= 0) return false;
        bool isBest = score > Current;
        var table = Entries;
        table.Add(new ScoreEntry(score, cleared, difficulty));
        // Stable sort, best first.
        Entries = table.Select((e, i) => (e, i)).OrderByDescending(t => t.e.Score).ThenBy(t => t.i).Select(t => t.e).Take(Capacity).ToList();
        return isBest;
    }
}
