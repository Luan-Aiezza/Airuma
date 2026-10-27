using System.Collections.Generic;
using Godot;

// Hooks that let the tests set up the world without playing it.
public partial class World
{
    /// <summary>Stills the air (for tests that cannot have the boat drifting).</summary>
    public void CalmWindForTesting() { Wind = new WindState(); }

    /// <summary>Sets the wind.</summary>
    public void SetWindForTesting(Vector2 direction, float strength) { Wind = new WindState { Direction = direction, Strength = strength }; }

    /// <summary>Holds the next wave back for a long while (for tests that want the stage quiet).</summary>
    public void HoldWavesForTesting() { _waves.Pause = 10_000; }

    /// <summary>Clears every wave, so the next thing is the boss.</summary>
    public void FinishWavesForTesting() { WaveSystem.JumpTo(_waves, Enemies, Definition.Waves.Count); }

    /// <summary>Takes a card without a draft.</summary>
    public void GiveForTesting(Card card)
    {
        Draft = new Draft(DraftMoment.StageStart, new List<Card> { card });
        Choose(0);
    }

    /// <summary>Replaces the lights.</summary>
    public void SetLightsForTesting(LightItem[] items) { Lights = items; }

    /// <summary>Puts a spirit in the world directly.</summary>
    public void AddForTesting(EnemyEntity enemy) { Enemies.Add(enemy); }

    /// <summary>Jumps to the start of a wave (to rehearse it, or to test it).</summary>
    public void StartWave(int index)
    {
        if (Part != StagePart.Waves || index < 0 || index >= Definition.Waves.Count) return;
        WaveSystem.JumpTo(_waves, Enemies, index);
    }

    /// <summary>Adds points as if they had been earned (for tests of the prizes).</summary>
    public void AddScoreForTesting(int points) => AddScore(points);

    /// <summary>Ends the spirits' part and brings the boss in.</summary>
    public void SkipToBoss()
    {
        if (Part != StagePart.Waves) return;
        WaveSystem.JumpTo(_waves, Enemies, Definition.Waves.Count);
        Draft = null;
        StartBoss();
    }
}
