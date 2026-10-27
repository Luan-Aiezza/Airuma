using System.Collections.Generic;

/// <summary>How far the run has got through the waves of the stage.</summary>
public sealed class WaveProgress
{
    /// <summary>Index of the wave in progress, or the number of waves once every wave is done.</summary>
    public int Index;
    /// <summary>Seconds since the wave began.</summary>
    public float Clock;
    /// <summary>Seconds to wait before the next wave (or the boss) comes in.</summary>
    public float Pause = WaveSystem.FirstPause;
    /// <summary>The spirits of the wave that have not come in yet, in the order they do.</summary>
    public List<EnemySpec> Pending = new List<EnemySpec>();
    public int NextEnemyId = 100;
}
