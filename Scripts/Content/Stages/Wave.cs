using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>One enemy to bring in, and when (seconds from the start of its wave).</summary>
public struct EnemySpec
{
    public float Time;
    public SpiritKind Kind;
    public Vector2 Position;
    public EnemyPath Path;
    public float Phase;
    public List<ScheduledPattern> Patterns;
    public float FirstShotDelay;

    public EnemySpec() { Time = 0; Kind = SpiritKind.Swarm; Position = Vector2.Zero; Path = default; Phase = 0; Patterns = new List<ScheduledPattern>(); FirstShotDelay = 0.6f; }
}

public class Wave
{
    public List<EnemySpec> Spawns;
    public Wave(List<EnemySpec> spawns) { Spawns = spawns; }
    /// <summary>When the last enemy comes in.</summary>
    public float LastSpawnTime => Spawns.Count == 0 ? 0 : Spawns.Max(s => s.Time);
}
