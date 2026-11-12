using System;
using System.Collections.Generic;
using System.Linq;

public static partial class SelfTest
{
    private sealed class MemoryAchievementStore : IAchievementStore
    {
        public HashSet<string> Saved = new HashSet<string>();
        public HashSet<string> Load() => new HashSet<string>(Saved);
        public void Save(HashSet<string> unlocked) { Saved = new HashSet<string>(unlocked); }
    }

    private sealed class RecordingBackend : IAchievementBackend
    {
        public List<string> Sent = new List<string>();
        public void Unlock(string id) { Sent.Add(id); }
    }

    private static void RegisterAchievements(Action<string, Action> T)
    {
        T("TheTwentyAchievementsAreOneForEachBossOnEachDifficultyAndTheEndings", TheTwentyAchievementsAreOneForEachBossOnEachDifficultyAndTheEndings);
        T("AnAchievementIsUnlockedOnceAndTold", AnAchievementIsUnlockedOnceAndTold);
        T("TheBadEndingBringsTheJokeAndTheTrueEndingDoesNot", TheBadEndingBringsTheJokeAndTheTrueEndingDoesNot);
        T("WhatWasUnlockedBeforeSteamIsSentWhenItIsAttached", WhatWasUnlockedBeforeSteamIsSentWhenItIsAttached);
    }

    /// <summary>Puts a clean memory where the achievements are kept, and takes everything off at the end.</summary>
    private static void WithCleanAchievements(Action<MemoryAchievementStore> body)
    {
        var before = Achievements.Store;
        var store = new MemoryAchievementStore();
        Achievements.Store = store; Achievements.Reload(); Achievements.Attach(null);
        try { body(store); }
        finally { Achievements.Store = before; Achievements.Reload(); Achievements.Attach(null); }
    }

    private static void TheTwentyAchievementsAreOneForEachBossOnEachDifficultyAndTheEndings()
    {
        Eq(Achievements.All.Count, Achievements.Count);
        Eq(Achievements.All.Select(a => a.Id).Distinct().Count(), 20, "twenty different ids");
        Eq(Achievements.All.Count(a => a.Kind == Achievements.Kind.Boss), 17, "17 bosses beaten: 6 on normal, 6 on hard, 5 on easy");
        Eq(Achievements.BossId(0, Difficulty.Easy), "BOSS1_EASY");
        Eq(Achievements.BossId(4, Difficulty.Hard), "BOSS5_HARD");
        Eq(Achievements.BossId(5, Difficulty.Normal), "BOSS6_NORMAL");
        True(Achievements.BossId(5, Difficulty.Easy) == null, "the easy mode has no sixth boss");
        True(Achievements.BossId(6, Difficulty.Normal) == null && Achievements.BossId(-1, Difficulty.Normal) == null, "nor a seventh");
        True(Achievements.Get(Achievements.EasyJoke).Hidden, "the joke is hidden");
        Eq(Achievements.All.Count(a => a.Hidden), 1);
    }

    private static void AnAchievementIsUnlockedOnceAndTold()
    {
        WithCleanAchievements(store =>
        {
            int told = 0; Achievements.Info last = null;
            Action<Achievements.Info> hear = a => { told++; last = a; };
            Achievements.Unlocked += hear;
            try
            {
                True(Achievements.Unlock("BOSS2_HARD"), "new");
                False(Achievements.Unlock("BOSS2_HARD"), "not twice");
                False(Achievements.Unlock("NOT_AN_ACHIEVEMENT"), "unknown ones are ignored");
                False(Achievements.Unlock(null), "and null");
                Eq(told, 1); Eq(last.Id, "BOSS2_HARD"); Eq(last.Stage, 1); Eq(last.Difficulty, Difficulty.Hard);
                True(store.Saved.Contains("BOSS2_HARD"), "kept in the store");
                Achievements.Reload();
                True(Achievements.IsUnlocked("BOSS2_HARD"), "and read again from it");
                Eq(Achievements.UnlockedCount, 1);
            }
            finally { Achievements.Unlocked -= hear; }
        });
    }

    private static void TheBadEndingBringsTheJokeAndTheTrueEndingDoesNot()
    {
        WithCleanAchievements(store =>
        {
            Achievements.TrueEndingSeen();
            True(Achievements.IsUnlocked("ENDING_TRUE") && !Achievements.IsUnlocked("ENDING_BAD") && !Achievements.IsUnlocked("EASY_JOKE"), "the true ending alone");
            Achievements.BadEndingSeen();
            True(Achievements.IsUnlocked("ENDING_BAD") && Achievements.IsUnlocked("EASY_JOKE"), "the bad ending and the joke");
            Eq(Achievements.UnlockedCount, 3);
        });
    }

    private static void WhatWasUnlockedBeforeSteamIsSentWhenItIsAttached()
    {
        WithCleanAchievements(store =>
        {
            Achievements.BossDefeated(0, Difficulty.Easy);
            Achievements.BossDefeated(0, Difficulty.Normal);
            var steam = new RecordingBackend();
            Achievements.Attach(steam);
            Eq(string.Join(",", steam.Sent), "BOSS1_EASY,BOSS1_NORMAL", "what was earned before, in order");
            Achievements.BossDefeated(1, Difficulty.Easy);
            Achievements.BossDefeated(1, Difficulty.Easy);
            Eq(steam.Sent.Count, 3, "and each new one once");
            Eq(steam.Sent[2], "BOSS2_EASY");
        });
    }
}
