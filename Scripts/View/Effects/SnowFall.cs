using System;
using Godot;

/// <summary>A light snowfall over the lake at the top of the Andes: small flakes that drift down, swaying, slowly. <see cref="Update"/> says how much of it there is (0: none).</summary>
public partial class SnowFall : Node2D
{
    private struct Flake { public float X, Y, Speed, Sway, Phase, Size; }

    private readonly Flake[] _flakes;
    private readonly Vector2 _size;
    private readonly Random _rng = new(11);
    private float _amount, _time;

    public SnowFall(Vector2 size, int count = 70)
    {
        _size = size;
        _flakes = new Flake[count];
        for (int i = 0; i < count; i++) _flakes[i] = Make(true);
    }

    private Flake Make(bool anywhere) => new Flake
    {
        X = (float)_rng.NextDouble() * _size.X, Y = anywhere ? (float)_rng.NextDouble() * _size.Y : -10,
        Speed = 28 + 34 * (float)_rng.NextDouble(), Sway = 8 + 14 * (float)_rng.NextDouble(), Phase = (float)_rng.NextDouble() * 10, Size = _rng.Next(0, 4) == 0 ? 3 : 2,
    };

    public void Update(float dt, float amount)
    {
        _amount = amount;
        Visible = amount > 0.01f;
        if (!Visible) return;
        _time += dt;
        for (int i = 0; i < _flakes.Length; i++)
        {
            _flakes[i].Y += _flakes[i].Speed * dt;
            if (_flakes[i].Y > _size.Y + 10) _flakes[i] = Make(false);
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        for (int i = 0; i < _flakes.Length; i++)
        {
            var f = _flakes[i];
            float x = MathF.Round((f.X + f.Sway * MathF.Sin(_time * 0.9f + f.Phase)) / 2) * 2, y = MathF.Round(f.Y / 2) * 2;
            DrawRect(new Rect2(x, y, f.Size, f.Size), new Color(0.95f, 0.98f, 1f, 0.8f * _amount));
        }
    }
}
