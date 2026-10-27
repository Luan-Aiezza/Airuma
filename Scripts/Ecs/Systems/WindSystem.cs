using System;
using Godot;

/// <summary>The wind: which way it blows and how hard, drawn at random whenever a new attack begins.</summary>
public struct WindState
{
    public Vector2 Direction;
    public float Strength;

    public Vector2 CurrentWindVector => Direction * Strength;
}

public static class WindSystem
{
    public static readonly float[] Strengths = { 0, 25, 50, 75 };

    public static void Randomize(ref WindState wind)
    {
        Vector2[] directions = { Vector2.Up, Vector2.Down, Vector2.Right, Vector2.Left };
        wind.Direction = directions[Random.Shared.Next(directions.Length)];
        wind.Strength = Strengths[Random.Shared.Next(Strengths.Length)];
    }
}
