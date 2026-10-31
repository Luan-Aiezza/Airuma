using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>What the boat takes from one stage into the next: the score, lives, bombs and lantern, and the cards taken so far.</summary>
public class RunCarry
{
    public int Score, Lives, Bombs;
    public float Energy;
    public List<Card> Cards = new List<Card>();

    public static RunCarry From(World world) => new RunCarry
    {
        Score = world.Score, Lives = world.Player.Health.Lives, Bombs = world.Player.Bomb.BombsLeft,
        Energy = world.Player.Energy, Cards = world.Run.History.ToList(),
    };
}
