using System;
using Godot;

/// <summary>
/// The pilot of the demonstration shown when the menu is left alone: it shoots all the time, stays under the foe it is shooting at and steps out of the way of bullets. Each
/// frame it looks at the few bullets near the boat once (into a buffer that is reused, so nothing is allocated), and judges nine moves (still, or one of eight directions) by where the
/// boat and those bullets would be a moment later.
/// </summary>
public sealed class DemoPilot
{
    private const float LookRadius = 230, Margin = 13, Step = 0.16f, BombCooldown = 7;
    private const int MaxThreats = 96;
    private static readonly Vector2[] Moves = BuildMoves();
    private readonly Vector2[] _from = new Vector2[MaxThreats], _speed = new Vector2[MaxThreats];
    private readonly float[] _reach = new float[MaxThreats];
    private float _bombWait = 5;

    private static Vector2[] BuildMoves()
    {
        var moves = new Vector2[9];
        for (int i = 0; i < 8; i++) moves[i] = new Vector2(MathF.Cos(i * MathF.PI / 4), MathF.Sin(i * MathF.PI / 4));
        moves[8] = Vector2.Zero;
        for (int i = 0; i < 8; i++) if (MathF.Abs(moves[i].X) < 0.01f) moves[i].X = 0; else if (MathF.Abs(moves[i].Y) < 0.01f) moves[i].Y = 0;
        return moves;
    }

    public PlayerInputComponent Sample(World world, float dt)
    {
        _bombWait -= dt;
        var player = world.Player;
        var p = player.Transform.Position;
        float radius = player.Hitbox.Radius;

        // the bullets that matter: the foe's, near the boat
        int count = 0;
        var bullets = world.Bullets;
        float look2 = LookRadius * LookRadius;
        for (int i = 0; i < bullets.Length && count < MaxThreats; i++)
        {
            ref var b = ref bullets[i];
            if (!b.IsActive || b.Faction != Faction.Boss) continue;
            if ((b.Position - p).LengthSquared() > look2) continue;
            _from[count] = b.Position; _speed[count] = b.Velocity; _reach[count] = b.HitboxRadius * Math.Max(1, b.Scale * 0.5f) + radius + Margin;
            count++;
        }

        // where it wants to be: under the foe it is shooting at, near the bottom
        float wantX = p.X;
        if (world.Part == StagePart.Boss && world.Boss.Health.Lives > 0) wantX = world.Boss.Transform.Position.X;
        else
        {
            float best = float.MaxValue;
            foreach (var e in world.Enemies) { float d = MathF.Abs(e.Position.X - p.X) + Math.Max(0, p.Y - e.Position.Y) * 0.2f; if (e.Position.Y > 0 && d < best) { best = d; wantX = e.Position.X; } }
        }
        const float wantY = 640;

        float bestScore = float.MaxValue, bestDanger = 0; Vector2 bestMove = Vector2.Zero;
        float travel = player.Movement.Speed * Step;
        var area = World.PlayArea;
        for (int m = 0; m < Moves.Length; m++)
        {
            var q = p + Moves[m] * travel;
            q.X = Math.Clamp(q.X, area.Position.X + 20, area.End.X - 20);
            q.Y = Math.Clamp(q.Y, 380, area.End.Y - 30);
            float danger = 0;
            for (int i = 0; i < count; i++)
                for (int k = 1; k <= 3; k++)
                {
                    float t = k * Step * 0.8f;
                    var at = _from[i] + _speed[i] * t;
                    float d2 = (at - q).LengthSquared(), r = _reach[i];
                    if (d2 < r * r) danger += (r - MathF.Sqrt(d2) + 4) * (4 - k);
                }
            float score = danger * 40 + MathF.Abs(q.X - wantX) * 0.05f + MathF.Abs(q.Y - wantY) * 0.04f
                          + (MathF.Min(q.X - area.Position.X, area.End.X - q.X) < 60 ? 6 : 0)
                          + (m == 8 ? 0 : 0.3f);
            if (score < bestScore) { bestScore = score; bestMove = Moves[m]; bestDanger = danger; }
        }

        var input = new PlayerInputComponent { IsShooting = true, Direction = bestMove };
        if (bestDanger > 14 && _bombWait <= 0 && player.Bomb.BombsLeft > 0) { input.IsBombing = true; _bombWait = BombCooldown; }
        return input;
    }
}
