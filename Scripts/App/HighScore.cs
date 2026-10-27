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

    public static List<ScoreEntry> Entries
    {
        get
        {
            var cfg = new ConfigFile();
            var result = new List<ScoreEntry>();
            if (cfg.Load(Path) != Error.Ok) return result;
            if (cfg.HasSectionKey(Section, "scores"))
            {
                int[] scores = cfg.GetValue(Section, "scores").AsInt32Array();
                int[] cleared = cfg.HasSectionKey(Section, "cleared") ? cfg.GetValue(Section, "cleared").AsInt32Array() : Array.Empty<int>();
                int[] levels = cfg.HasSectionKey(Section, "difficulty") ? cfg.GetValue(Section, "difficulty").AsInt32Array() : Array.Empty<int>();
                for (int i = 0; i < scores.Length; i++)
                    result.Add(new ScoreEntry(scores[i], i < cleared.Length && cleared[i] != 0, i < levels.Length ? (Difficulty)Math.Clamp(levels[i], 0, 2) : Difficulty.Normal));
                return result;
            }
            // The table that predates it only kept the best score.
            int old = cfg.HasSectionKey(Section, "highscore") ? (int)cfg.GetValue(Section, "highscore") : 0;
            if (old > 0) result.Add(new ScoreEntry(old, false));
            return result;
        }
        set
        {
            var table = value.Take(Capacity).ToList();
            var cfg = new ConfigFile();
            cfg.SetValue(Section, "scores", table.Select(e => e.Score).ToArray());
            cfg.SetValue(Section, "cleared", table.Select(e => e.Cleared ? 1 : 0).ToArray());
            cfg.SetValue(Section, "difficulty", table.Select(e => (int)e.Difficulty).ToArray());
            cfg.Save(Path);
        }
    }

    /// <summary>The best score.</summary>
    public static int Current
    {
        get { var e = Entries; return e.Count > 0 ? e[0].Score : 0; }
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
