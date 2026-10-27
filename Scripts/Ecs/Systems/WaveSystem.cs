using System.Collections.Generic;
using System.Linq;

/// <summary>Brings the spirits of a stage in, wave by wave, as the script says.</summary>
public static class WaveSystem
{
    public const float FirstPause = 2.0f;
    public const float PauseBetweenWaves = 1.6f;
    public const float PauseBeforeBoss = 2.8f;

    /// <summary>Every wave has been cleared.</summary>
    public static bool IsDone(IReadOnlyList<Wave> waves, WaveProgress progress) => progress.Index >= waves.Count;

    /// <summary>
    /// Runs the waves for delta seconds, adding the spirits that are due to `enemies`. Returns true once every wave is cleared and the
    /// breather before the boss is over.
    /// </summary>
    public static bool Update(IReadOnlyList<Wave> waves, WaveProgress progress, List<EnemyEntity> enemies, float delta, List<WorldEvent> events)
    {
        // Every wave done: a breather, then the boss.
        if (IsDone(waves, progress))
        {
            if (enemies.Count != 0) return false;
            progress.Pause -= delta;
            return progress.Pause <= 0;
        }

        if (progress.Pending.Count == 0 && progress.Clock == 0)
        {
            // Between waves.
            progress.Pause -= delta;
            if (!(progress.Pause <= 0)) return false;
            progress.Pending = waves[progress.Index].Spawns.Select((s, i) => (s, i)).OrderBy(t => t.s.Time).ThenBy(t => t.i).Select(t => t.s).ToList();
            events.Add(WorldEvent.WaveStarted(progress.Index));
        }

        progress.Clock += delta;
        while (progress.Pending.Count > 0 && progress.Pending[0].Time <= progress.Clock)
        {
            EnemySpec next = progress.Pending[0];
            progress.Pending.RemoveAt(0);
            enemies.Add(new EnemyEntity(progress.NextEnemyId, next.Kind, next.Position, next.Path, next.Phase, next.Patterns, next.FirstShotDelay));
            progress.NextEnemyId += 1;
        }

        if (progress.Pending.Count == 0 && enemies.Count == 0)
        {
            // The wave is cleared.
            progress.Index += 1;
            progress.Clock = 0;
            progress.Pause = IsDone(waves, progress) ? PauseBeforeBoss : PauseBetweenWaves;
        }
        return false;
    }

    /// <summary>Jumps to the start of a wave, with no spirits about (to rehearse it, or to test it).</summary>
    public static void JumpTo(WaveProgress progress, List<EnemyEntity> enemies, int index)
    {
        enemies.Clear();
        progress.Pending.Clear();
        progress.Index = index;
        progress.Clock = 0;
        progress.Pause = 0;
    }
}
