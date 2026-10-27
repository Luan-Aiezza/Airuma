using System;
using Godot;

/// <summary>
/// Foam churned up by the boat: a bow wave, foam along the hull and a V-shaped trail behind the stern.
/// Foam belongs to the water, so once it is left behind it drifts with the river's current (downwards on screen).
/// Particles come from a fixed pool.
///
/// API
///   new WakeEffect(int capacity = 180)
///   Update(float dt, Vector2 boat, bool emitting)   // boat = the boat's centre, screen space (Y down). While emitting, foam is born around it.
///   Reset()
///   int ActiveCount
///   const float CurrentSpeed                         // px/s the river scrolls down (also used by SerpentBody)
/// Put it behind the boat (child ZIndex 0; give this node a ZIndex below the boat). Node must be at the scene origin.
/// </summary>
public partial class WakeEffect : Node2D
{
    private struct Particle
    {
        public bool Active;
        public Vector2 Position;
        public Vector2 Velocity;
        public float Age;
        public float Life;
        public bool Large;
    }

    /// <summary>Where foam is born, relative to the boat's centre (screen px, Y DOWN: the prow is at negative y).</summary>
    private struct Source
    {
        public Vector2 Offset;
        public float Spread;      // sideways speed, outwards from the boat's axis
        public float LifeMin, LifeMax;
        public float Interval;
        public bool Large;
        public Source(Vector2 offset, float spread, float lifeMin, float lifeMax, float interval, bool large)
        { Offset = offset; Spread = spread; LifeMin = lifeMin; LifeMax = lifeMax; Interval = interval; Large = large; }
    }

    private static readonly Source[] Sources =
    {
        // Bow wave, peeling off the prow.
        new Source(new Vector2(4, -34), 38, 0.4f, 0.6f, 0.1f, false),
        // Foam sliding along the hull.
        new Source(new Vector2(13, -6), 16, 0.5f, 0.8f, 0.16f, false),
        new Source(new Vector2(13, 14), 20, 0.6f, 0.9f, 0.16f, false),
        // The trail: churn at the stern spreading into a V.
        new Source(new Vector2(7, 38), 30, 1.3f, 2.0f, 0.09f, true),
        new Source(new Vector2(0, 40), 5, 0.8f, 1.2f, 0.14f, false),
    };

    /// <summary>The river scrolls down at this many px per second (see RiverBackground).</summary>
    public const float CurrentSpeed = RiverBackground.ScrollRowsPerSecond * FoamArt.Scale;

    private const float Snap = FoamArt.Scale;
    private readonly Texture2D[] _smallFrames = FoamArt.MakeStages(false);
    private readonly Texture2D[] _largeFrames = FoamArt.MakeStages(true);
    private readonly Particle[] _particles;
    private readonly Sprite2D[] _nodes;
    private readonly float[] _timers;
    private readonly Random _rng = new Random();

    public int ActiveCount { get { int n = 0; foreach (var p in _particles) if (p.Active) n++; return n; } }

    public WakeEffect(int capacity = 180)
    {
        _particles = new Particle[capacity];
        _nodes = new Sprite2D[capacity];
        _timers = new float[Sources.Length];
        for (int i = 0; i < capacity; i++)
        {
            var node = new Sprite2D { Visible = false };
            AddChild(node);
            _nodes[i] = node;
        }
    }

    private float Rand(float lo, float hi) => lo + (hi - lo) * (float)_rng.NextDouble();

    public void Reset()
    {
        for (int i = 0; i < _particles.Length; i++)
        {
            _particles[i].Active = false;
            _nodes[i].Visible = false;
        }
    }

    public void Update(float dt, Vector2 boat, bool emitting)
    {
        if (emitting)
        {
            for (int i = 0; i < Sources.Length; i++)
            {
                var source = Sources[i];
                _timers[i] += dt;
                while (_timers[i] >= source.Interval)
                {
                    _timers[i] -= source.Interval;
                    // Mirrored on both sides of the boat (the centre churn has no spread, so it just jitters).
                    Spawn(source, boat, 1);
                    if (source.Offset.X > 0) Spawn(source, boat, -1);
                }
            }
        }

        for (int i = 0; i < _particles.Length; i++)
        {
            if (!_particles[i].Active) continue;
            _particles[i].Age += dt;
            var p = _particles[i];
            if (p.Age >= p.Life)
            {
                _particles[i].Active = false;
                _nodes[i].Visible = false;
                continue;
            }

            // Sideways motion eases out; the current carries the foam downstream.
            _particles[i].Velocity.X *= Math.Max(0, 1 - 1.6f * dt);
            _particles[i].Position.X += _particles[i].Velocity.X * dt;
            _particles[i].Position.Y += _particles[i].Velocity.Y * dt;

            float t = p.Age / p.Life;
            int stage = Math.Min(FoamArt.StageCount - 1, (int)(t * FoamArt.StageCount));
            var node = _nodes[i];
            node.Visible = true;
            var texture = (p.Large ? _largeFrames : _smallFrames)[stage];
            node.Texture = texture;
            node.Scale = FoamArt.Size(p.Large) / new Vector2(texture.GetWidth(), texture.GetHeight());
            // Stay on the pixel grid, like the rest of the art.
            node.Position = new Vector2(MathF.Round(_particles[i].Position.X / Snap) * Snap, MathF.Round(_particles[i].Position.Y / Snap) * Snap);
            node.Modulate = new Color(1, 1, 1, t < 0.5f ? 0.8f : 0.8f * (1 - (t - 0.5f) / 0.5f));
        }
    }

    private void Spawn(Source source, Vector2 boat, float side)
    {
        int i = Array.FindIndex(_particles, p => !p.Active);
        if (i < 0) return;
        var jitter = new Vector2(Rand(-3, 3), Rand(-3, 3));
        float outward = source.Offset.X == 0 ? Rand(-1, 1) : side;
        _particles[i] = new Particle
        {
            Active = true,
            Position = new Vector2(boat.X + source.Offset.X * side + jitter.X, boat.Y + source.Offset.Y + jitter.Y),
            Velocity = new Vector2(outward * source.Spread * Rand(0.7f, 1.2f), CurrentSpeed),
            Age = 0,
            Life = Rand(source.LifeMin, source.LifeMax),
            Large = source.Large,
        };
    }
}
