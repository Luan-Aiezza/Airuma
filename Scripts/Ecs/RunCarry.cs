using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// What the boat takes from one stage into the next: the score, lives, bombs and lantern, the cards taken so far, the ward it still has, and the spare lantern and spare boat that were
/// still falling when the stage ended (the prize was earned: it is given at the start of the next stage).
/// </summary>
public class RunCarry
{
    public int Score, Lives, Bombs, Shield, PendingBombs, PendingLives;
    public float Energy;
    public List<Card> Cards = new List<Card>();

    public static RunCarry From(World world) => new RunCarry
    {
        Score = world.Score, Lives = world.Player.Health.Lives, Bombs = world.Player.Bomb.BombsLeft,
        Energy = world.Player.Energy, Cards = world.Run.History.ToList(),
        Shield = world.Player.Shield,
        PendingBombs = world.Lights.Count(l => l.IsActive && l.Kind == LightKind.Bomb), PendingLives = world.Lights.Count(l => l.IsActive && l.Kind == LightKind.Life),
    };
}
