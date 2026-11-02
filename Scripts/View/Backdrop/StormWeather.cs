using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// The weather of a stormy night over the river: rain that slants in the strong wind and splashes on the water, and now and then a bolt of lightning that
/// falls on the water, with a flash that lights everything for a moment, a ring where it lands, and the thunder that comes a moment later. It is
/// only for the eyes (and ears): it changes nothing in the game, not even the wind. Give it the size of the river, and call <see cref="Update"/>.
/// </summary>
public partial class StormWeather : Node2D
{
    private struct Drop { public Vector2 Position; public float Speed, Length; }
    /// <summary>A pair of eyes that glow among the trees of the black forest: where, and when it opens its eyes.</summary>
    private struct Eyes { public Vector2 Position; public float Phase, Rate; public bool Green; }
    private struct Splash { public Vector2 Position; public float Age, Life, Size; }

    private const int Drops = 150;
    private const float Slant = 0.38f;            // how far the rain leans: sideways per downwards
    private readonly Vector2 _size;
    private readonly Drop[] _drops = new Drop[Drops];
    private readonly List<Splash> _splashes = new();
    private readonly Random _rng = new Random(3);
    private float _splashClock, _nextStrike = 4;
    private const int EyePairs = 7;
    private const float BankScroll = 52;          // px per second: the banks go by
    private readonly Eyes[] _eyes = new Eyes[EyePairs];
    private float _clock, _open;
    /// <summary>The boss fight: the rain has stopped (`_rain` fades from 1 to 0), and the lightning comes twice as often.</summary>
    private bool _fight;
    private float _rain = 1;

    public void SetFight(bool fight) => _fight = fight;

    // The lightning: the bolt (points, drawn for a moment), the flash that lights everything, and the thunder that follows.
    private Vector2[] _bolt = Array.Empty<Vector2>();
    private Vector2[] _branch = Array.Empty<Vector2>();
    private float _boltAge = 99, _flash, _thunderIn = -1;
    private Vector2 _strike;

    private float Rand(float lo, float hi) => lo + (hi - lo) * (float)_rng.NextDouble();

    public StormWeather(Vector2 size)
    {
        _size = size;
        ZIndex = ViewKit.Z(3.6f);        // over the banks and the branches, under the bullets
        for (int i = 0; i < Drops; i++) _drops[i] = NewDrop(true);
        for (int i = 0; i < EyePairs; i++) { _eyes[i] = NewEyes(Rand(0, size.Y)); }
    }

    private Eyes NewEyes(float y)
    {
        bool left = _rng.Next(2) == 0;
        return new Eyes
        {
            Position = new Vector2(left ? Rand(34, 112) : _size.X - Rand(34, 112), y), Phase = Rand(0, MathF.Tau), Rate = Rand(0.5f, 1.1f), Green = _rng.Next(3) == 0,
        };
    }

    private Drop NewDrop(bool anywhere) => new Drop
    {
        Position = new Vector2(Rand(-120, _size.X + 40), anywhere ? Rand(-60, _size.Y) : Rand(-90, -10)),
        Speed = Rand(900, 1300), Length = Rand(14, 26),
    };

    /// <summary>`open` is how far the banks have slid away (0: not at all): the eyes go with them.</summary>
    public void Update(float dt, float open = 0)
    {
        _clock += dt;
        _open = open;
        for (int i = 0; i < EyePairs; i++)
        {
            _eyes[i].Position.Y += BankScroll * dt;
            if (_eyes[i].Position.Y > _size.Y + 20) _eyes[i] = NewEyes(-20);
        }
        _rain = Mathf.MoveToward(_rain, _fight ? 0 : 1, dt / 1.2f);
        // Rain.
        for (int i = 0; i < Drops; i++)
        {
            _drops[i].Position += new Vector2(Slant, 1) * _drops[i].Speed * dt;
            if (_drops[i].Position.Y > _size.Y + 30) _drops[i] = NewDrop(false);
        }
        // Splashes of the rain on the water: small rings that open and fade.
        _splashClock -= dt;
        while (_splashClock <= 0) { _splashClock += 0.03f; if (_rain < 0.05f) continue; _splashes.Add(new Splash { Position = new Vector2(Rand(150, _size.X - 150), Rand(40, _size.Y - 20)), Life = Rand(0.35f, 0.6f), Size = Rand(5, 11) }); }
        for (int i = _splashes.Count - 1; i >= 0; i--)
        {
            var s = _splashes[i]; s.Age += dt;
            if (s.Age >= s.Life) _splashes.RemoveAt(i); else _splashes[i] = s;
        }

        // Lightning: now and then, a bolt falls on the water.
        _nextStrike -= dt;
        if (_nextStrike <= 0) { Strike(); _nextStrike = _fight ? Rand(3, 6) : Rand(6, 12); }
        _boltAge += dt;
        _flash = MathF.Max(0, _flash - dt * 2.2f);
        if (_thunderIn >= 0)
        {
            _thunderIn -= dt;
            if (_thunderIn < 0) SfxPlayer.Instance?.Play(SfxBank.Sound.Thunder);
        }
        QueueRedraw();
    }

    private void Strike()
    {
        float x = Rand(190, _size.X - 190), y = Rand(260, _size.Y - 140);
        _strike = new Vector2(x, y);
        var points = new List<Vector2>();
        float sway = Rand(-60, 60);
        const int steps = 11;
        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            float jag = i == 0 || i == steps ? 0 : Rand(-24, 24) * (1 - t * 0.5f);
            points.Add(new Vector2(x + sway * (1 - t) + jag, -10 + (y + 10) * t));
        }
        _bolt = points.ToArray();
        // a short branch from the middle of the bolt
        var from = _bolt[steps / 2];
        _branch = new[] { from, from + new Vector2(Rand(25, 55) * (_rng.Next(2) * 2 - 1), 40), from + new Vector2(Rand(40, 90) * (_rng.Next(2) * 2 - 1), 95) };
        _boltAge = 0;
        _flash = 1;
        _thunderIn = Rand(0.25f, 0.6f);
    }

    public override void _Draw()
    {
        // The flash lights everything; it flickers once.
        if (_flash > 0)
        {
            float flicker = _boltAge > 0.12f && _boltAge < 0.2f ? 0.35f : 1f;
            DrawRect(new Rect2(Vector2.Zero, _size), new Color(0.75f, 0.85f, 1f, 0.3f * _flash * flicker));
        }
        // The bolt, with a glow, and the ring and the glow where it lands.
        if (_boltAge < 0.28f && _bolt.Length > 1)
        {
            float a = 1 - _boltAge / 0.28f;
            DrawPolyline(_bolt, new Color(0.6f, 0.75f, 1f, 0.4f * a), 14);
            DrawPolyline(_bolt, new Color(1f, 1f, 1f, a), 5);
            DrawPolyline(_branch, new Color(0.95f, 0.98f, 1f, 0.85f * a), 3);
            DrawCircle(_strike, 26 * a + 8, new Color(0.8f, 0.9f, 1f, 0.5f * a));
        }
        if (_boltAge < 0.9f)
        {
            float t = _boltAge / 0.9f;
            DrawEllipse(_strike, 14 + 90 * t, (14 + 90 * t) * 0.35f, new Color(0.85f, 0.93f, 1f, 0.8f * (1 - t)), 2);
        }
        // Eyes among the trees: they open for a moment now and then, and close.
        foreach (var e in _eyes)
        {
            float open = Mathf.Clamp((MathF.Sin(_clock * e.Rate + e.Phase) - 0.55f) * 5, 0, 1) * (1 - _open);
            if (open <= 0) continue;
            var glow = e.Green ? new Color(0.55f, 1f, 0.5f) : new Color(1f, 0.82f, 0.25f);
            DrawCircle(e.Position, 14, new Color(glow.R, glow.G, glow.B, 0.16f * open));
            foreach (float dx in new[] { -6f, 6f })
                DrawRect(new Rect2(e.Position + new Vector2(dx - 2, -2), new Vector2(4, 4 * Mathf.Max(0.35f, open))), new Color(glow.R, glow.G, glow.B, open));
        }
        // The rain.
        var colour = new Color(0.78f, 0.86f, 1f, 0.32f * _rain);
        if (_rain > 0.01f)
            foreach (var d in _drops)
                DrawLine(d.Position, d.Position - new Vector2(Slant, 1).Normalized() * d.Length, colour, 1.5f);
        foreach (var s in _splashes)
        {
            float t = s.Age / s.Life;
            DrawEllipse(s.Position, s.Size * (0.4f + t), s.Size * (0.4f + t) * 0.4f, new Color(0.8f, 0.9f, 1f, 0.5f * (1 - t) * _rain), 1.5f);
        }
    }

    private void DrawEllipse(Vector2 centre, float rx, float ry, Color colour, float width)
    {
        const int n = 18;
        var points = new Vector2[n + 1];
        for (int i = 0; i <= n; i++) points[i] = centre + new Vector2(MathF.Cos(i * MathF.Tau / n) * rx, MathF.Sin(i * MathF.Tau / n) * ry);
        DrawPolyline(points, colour, width);
    }
}
