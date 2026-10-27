using System;
using System.Collections.Generic;

/// <summary>
/// A boss as data: a name and a list of attacks, the first one first. The boss has as many lives as it has attacks, and loses one
/// each time its health runs out, which is when the next attack begins.
/// </summary>
public sealed class BossDefinition
{
    public readonly string Name;
    public readonly IReadOnlyList<BossPhase> Phases;

    public BossDefinition(string name, IReadOnlyList<BossPhase> phases) { Name = name; Phases = phases; }

    /// <summary>One life for each attack.</summary>
    public int Lives => Phases.Count;

    /// <summary>The attack that goes with a number of lives left (all of them: the first attack; one: the last).</summary>
    public BossPhase PhaseFor(int lives) => Phases[Math.Clamp(Phases.Count - lives, 0, Phases.Count - 1)];

    /// <summary>The name of the attack for a number of lives left, or null if there is none.</summary>
    public string AttackName(int lives) => lives >= 1 && lives <= Lives ? PhaseFor(lives).AttackName : null;
}
