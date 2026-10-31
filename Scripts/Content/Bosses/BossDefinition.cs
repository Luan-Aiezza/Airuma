using System;
using System.Collections.Generic;

/// <summary>
/// A boss as data: a name and a list of attacks, the first one first. The boss has as many lives as it has attacks, and loses one
/// each time its health runs out, which is when the next attack begins.
/// </summary>
public sealed class BossDefinition
{
    private readonly string _nameKey;
    public string Name => Loc.Get(_nameKey);
    public readonly IReadOnlyList<BossPhase> Phases;
    /// <summary>How the boss looks (the art, the glow, the colours of its rage).</summary>
    public BossStyle Style = BossStyles.Iryssh;

    public BossDefinition(string nameKey, IReadOnlyList<BossPhase> phases) { _nameKey = nameKey; Phases = phases; }

    /// <summary>One life for each attack.</summary>
    public int Lives => Phases.Count;

    /// <summary>The attack that goes with a number of lives left (all of them: the first attack; one: the last).</summary>
    public BossPhase PhaseFor(int lives) => Phases[Math.Clamp(Phases.Count - lives, 0, Phases.Count - 1)];

    /// <summary>The name of the attack for a number of lives left, or null if there is none.</summary>
    public string AttackName(int lives) => lives >= 1 && lives <= Lives ? PhaseFor(lives).AttackName : null;
}
