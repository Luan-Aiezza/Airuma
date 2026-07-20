using Godot;
using System;

// Guarda as informações globais do vento
public struct WindState
{
    public Vector2 Direction;
    public float Strength;
    public Vector2 CurrentWindVector => Direction * Strength; // Calcula o vetor final da força
}

public static class WindSystem
{
    private static Random _random = new Random();

    public static void RandomizeWind(ref WindState wind)
    {
        // 1. Randomiza a Direção (Norte, Sul, Leste, Oeste)
        int dir = _random.Next(0, 4);
        wind.Direction = dir switch {
            0 => new Vector2(0, -1), // Norte (Para cima)
            1 => new Vector2(0, 1),  // Sul (Para baixo)
            2 => new Vector2(1, 0),  // Leste (Para a direita)
            3 => new Vector2(-1, 0), // Oeste (Para a esquerda)
            _ => Vector2.Zero
        };

        // 2. Randomiza a Força (Fraco, Médio, Forte, ou sem vento)
		// Agora pedimos um número entre 0 e 3 (o 4 é exclusivo)
		int str = _random.Next(0, 4); 

		wind.Strength = str switch {
			0 => 0f,   // Sem vento (Calmaria)
			1 => 25f,  // Fraco
			2 => 50f,  // Médio
			3 => 75f,  // Forte (pode empurrar as balas drasticamente!)
			_ => 0f    // Segurança padrão
		};

        // Imprime no console para sabermos o que o vento está fazendo
        GD.Print($"[EVENTO DE VENTO] Direção: {wind.Direction} | Força: {wind.Strength}");
    }
}