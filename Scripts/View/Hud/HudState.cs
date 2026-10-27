using Godot;

/// <summary>Everything the HUD needs to know about the game, once per frame.</summary>
public struct HudState
{
    public int Lives;
    public int Bombs;
    public int Power;
    public int Score;
    public int HiScore;
    public Vector2 WindDirection;
    public float WindStrength;
    /// <summary>Boss health 0...1, or null when there is no boss to show.</summary>
    public float? BossHealth;
    public int PhasesCleared;
    /// <summary>Progress (0...1) towards the next weapon level.</summary>
    public float PowerProgress;
    /// <summary>The cards taken in this run, in order (the HUD shows their icons).</summary>
    public System.Collections.Generic.IReadOnlyList<Card> Cards;
}
