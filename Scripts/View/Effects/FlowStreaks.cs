using System;
using Godot;

/// <summary>
/// The water of the waterfall rushing up: thin streaks of foam and light that race up the picture, quickly, and fade in and out as they go. They are drawn over the water and
/// under everything else. <see cref="Update"/> says how much of the rush is left (1: full; 0: none, the water has become a calm lake).
/// </summary>
public partial class FlowStreaks : Node2D
{
    private struct Streak { public float X, Y, Length, Speed, Width; public float Alpha; }

    private readonly Streak[] _streaks;
    private readonly Vector2 _size;
    private readonly Random _rng = new(5);
    private float _strength = 1;

    public FlowStreaks(Vector2 size, int count = 60)
    {
        _size = size;
        _streaks = new Streak[count];
        for (int i = 0; i < count; i++) _streaks[i] = Make(true);
    }

    private Streak Make(bool anywhere) => new Streak
    {
        X = (float)_rng.NextDouble() * _size.X, Y = anywhere ? (float)_rng.NextDouble() * _size.Y : _size.Y + 60 * (float)_rng.NextDouble(),
        Length = 40 + 110 * (float)_rng.NextDouble(), Speed = 170 + 260 * (float)_rng.NextDouble(), Width = _rng.Next(0, 2) == 0 ? 8 : 4, Alpha = 0.3f + 0.35f * (float)_rng.NextDouble(),
    };

    public void Update(float dt, float strength)
    {
        _strength = strength;
        Visible = strength > 0.01f;
        if (!Visible) return;
        for (int i = 0; i < _streaks.Length; i++)
        {
            _streaks[i].Y -= _streaks[i].Speed * dt * strength;
            if (_streaks[i].Y + _streaks[i].Length < -10) _streaks[i] = Make(false);
        }
        QueueRedraw();
    }

    public override void _Draw()
    {
        foreach (var s in _streaks)
        {
            // each streak is brightest in the middle of its length
            var mid = new Color(0.86f, 0.95f, 1f, s.Alpha * _strength);
            var end = new Color(0.86f, 0.95f, 1f, 0);
            DrawLine(new Vector2(s.X, s.Y), new Vector2(s.X, s.Y + s.Length * 0.5f), mid, s.Width);
            DrawLine(new Vector2(s.X, s.Y + s.Length * 0.5f), new Vector2(s.X, s.Y + s.Length), new Color(mid.R, mid.G, mid.B, mid.A * 0.4f), s.Width);
        }
    }
}
