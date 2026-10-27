using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Visual feedback for the wind: small leaves swept across the river in the wind's direction.
/// A burst of leaves is sent whenever the wind changes (call Announce(); also when a run begins); a few more come
/// every now and then while there is wind. Stronger wind means more leaves, and faster ones. When calm, nothing is shown.
///
/// API
///   new WindEffect(Vector2 field, int capacity = 90)   // field = area to cover (the river), screen px: (Layout.RiverWidth, Layout.Height)
///   SetWind(Vector2 direction, float strength)          // direction = GAME-space wind direction (Y down == screen Y down, axis-aligned unit or zero); strength as in the game (level = round(strength/25) clamped 0..3)
///   Announce()                                          // the wind just changed: burst of leaves
///   Update(float dt)   Reset()
///   Vector2 WindDirection (screen space, Y down), int WindLevel, int ActiveCount, int PendingCount
///   static Vector2 SceneDirection(Vector2 gameDirection)  // identity in Godot (screen Y down, same as game)
///   static LeafSpeed(level), BurstLeaves(level), GustLeaves(level), Level(strength)
/// Node must be at the scene origin (leaves are in river coordinates 0..field). Give it a ZIndex above the water.
/// </summary>
public partial class WindEffect : Node2D
{
    private struct Leaf
    {
        public bool Active;
        public Vector2 Start;
        public Vector2 Along;      // unit vector of travel
        public float Speed;
        public float SwayAmplitude;
        public float SwayRate;
        public float Phase;
        public float Fall;         // leaves sink a little as they go
        public float Age;
        public float Life;
        public int Tone;
        public float TumbleRate;
    }

    private struct Pending { public float Delay; public int Level; }

    public readonly Vector2 Field;
    private readonly Texture2D[][] _leafTextures = WindArt.MakeLeafTextures();
    private readonly Leaf[] _leaves;
    private readonly Sprite2D[] _nodes;
    private readonly List<Pending> _pending = new List<Pending>();
    private readonly Random _rng = new Random();

    public Vector2 WindDirection { get; private set; } // screen space, unit, axis-aligned
    public int WindLevel { get; private set; }         // 0 calm ... 3 strong
    private float _nextAmbient = 6;

    public int ActiveCount { get { int n = 0; foreach (var l in _leaves) if (l.Active) n++; return n; } }
    public int PendingCount => _pending.Count;

    public WindEffect(Vector2 field, int capacity = 90)
    {
        Field = field;
        _leaves = new Leaf[capacity];
        _nodes = new Sprite2D[capacity];
        for (int i = 0; i < capacity; i++)
        {
            var node = new Sprite2D { Visible = false };
            AddChild(node);
            _nodes[i] = node;
        }
    }

    private float Rand(float lo, float hi) => lo + (hi - lo) * (float)_rng.NextDouble();

    // Wind

    /// <summary>Game-space wind direction (Y down) as screen direction (Y down): the same vector.</summary>
    public static Vector2 SceneDirection(Vector2 gameDirection) => gameDirection;

    /// <summary>Base speed in px per second (each leaf varies a little around it).</summary>
    public static float LeafSpeed(int level) => 180 + level * 80;

    /// <summary>How many leaves the burst for a wind change has, and how many an occasional gust has.</summary>
    public static int BurstLeaves(int level) => 8 + level * 8;
    public static int GustLeaves(int level) => 3 + level * 3;

    /// <summary>0 for calm, then 1...3 for weak, average and strong.</summary>
    public static int Level(float strength) => Math.Max(0, Math.Min(3, (int)MathF.Round(strength / 25, MidpointRounding.AwayFromZero)));

    public void SetWind(Vector2 direction, float strength)
    {
        WindLevel = Level(strength);
        WindDirection = direction == Vector2.Zero ? Vector2.Zero : SceneDirection(direction);
    }

    /// <summary>The wind just changed: a burst of leaves, so the player sees where it blows now.</summary>
    public void Announce()
    {
        if (WindLevel <= 0) return;
        Enqueue(BurstLeaves(WindLevel), 1.8f);
        _nextAmbient = Rand(7, 12);
    }

    public void Reset()
    {
        for (int i = 0; i < _leaves.Length; i++)
        {
            _leaves[i].Active = false;
            _nodes[i].Visible = false;
        }
        _pending.Clear();
        _nextAmbient = 6;
    }

    // Update

    public void Update(float dt)
    {
        if (dt <= 0) return; // time never runs backwards
        if (WindLevel > 0)
        {
            _nextAmbient -= dt;
            if (_nextAmbient <= 0)
            {
                Enqueue(GustLeaves(WindLevel), 1.2f);
                _nextAmbient = Rand(7, 13);
            }
        }
        else
        {
            _nextAmbient = Math.Max(_nextAmbient, 3); // a gust is not saved up while it is calm
        }

        for (int i = 0; i < _pending.Count; i++) _pending[i] = new Pending { Delay = _pending[i].Delay - dt, Level = _pending[i].Level };
        var due = _pending.FindAll(p => p.Delay <= 0);
        _pending.RemoveAll(p => p.Delay <= 0);
        foreach (var item in due) Spawn(item.Level);

        for (int i = 0; i < _leaves.Length; i++)
        {
            if (!_leaves[i].Active) continue;
            _leaves[i].Age += dt;
            if (_leaves[i].Age >= _leaves[i].Life)
            {
                _leaves[i].Active = false;
                _nodes[i].Visible = false;
                continue;
            }
            Render(i);
        }
    }

    // Internals

    private void Enqueue(int count, float duration)
    {
        for (int k = 0; k < count; k++) _pending.Add(new Pending { Delay = Rand(0, duration), Level = WindLevel });
    }

    private void Spawn(int level)
    {
        var d = WindDirection;
        if (level <= 0 || d == Vector2.Zero) return;
        int i = Array.FindIndex(_leaves, l => !l.Active);
        if (i < 0) return;

        // Start just outside the upwind edge, at a random point along it.
        bool horizontal = d.X != 0;
        float across = Rand(0, horizontal ? Field.Y : Field.X);
        const float margin = 50;
        Vector2 start;
        if (d.X == 1) start = new Vector2(-margin, across);
        else if (d.X == -1) start = new Vector2(Field.X + margin, across);
        else if (d.Y == 1) start = new Vector2(across, -margin);            // blowing down: starts above the top edge
        else start = new Vector2(across, Field.Y + margin);                 // blowing up: starts below the bottom edge
        float distance = (horizontal ? Field.X : Field.Y) + 2 * margin;

        var leaf = new Leaf { Active = true, Start = start, Along = d };
        leaf.Phase = Rand(0, 2 * MathF.PI);
        leaf.Speed = LeafSpeed(level) * Rand(0.7f, 1.2f);
        leaf.SwayAmplitude = Rand(16, 36);
        leaf.SwayRate = Rand(2.2f, 4);
        leaf.Fall = Rand(18, 40);
        leaf.Tone = _rng.Next(0, WindArt.LeafTones);
        leaf.TumbleRate = Rand(3, 6);
        leaf.Life = distance / leaf.Speed;
        _leaves[i] = leaf;
        Render(i);
    }

    private void Render(int i)
    {
        var leaf = _leaves[i];
        var node = _nodes[i];
        var d = leaf.Along;
        var perp = new Vector2(d.Y, -d.X);
        float sway = leaf.SwayAmplitude * MathF.Sin(leaf.SwayRate * leaf.Age + leaf.Phase);
        // Leaves sink as they travel (towards the bottom of the screen).
        float x = leaf.Start.X + d.X * leaf.Speed * leaf.Age + perp.X * sway;
        float y = leaf.Start.Y + d.Y * leaf.Speed * leaf.Age + perp.Y * sway + leaf.Fall * leaf.Age;
        node.Position = new Vector2(MathF.Round(x / WindArt.Scale) * WindArt.Scale, MathF.Round(y / WindArt.Scale) * WindArt.Scale);

        // Tumbling: the leaf turns edge-on and flips, advancing a quarter turn each time. Quarter turns keep the pixels crisp.
        float t = leaf.Age / leaf.Life;
        int step = Math.Max(0, (int)(leaf.Age * leaf.TumbleRate + leaf.Phase));
        var texture = _leafTextures[leaf.Tone][step % WindArt.LeafFrames];
        node.Texture = texture;
        // The angle runs counter-clockwise, but rotation is clockwise on a screen whose Y axis points down, so the sign flips.
        node.Rotation = -(step % 4) * MathF.PI / 2 + MathF.Atan2(d.Y, d.X);
        float sx = ((step / 2) % 2 == 0 ? 1 : -1) * WindArt.Scale;
        node.Scale = new Vector2(sx, WindArt.Scale);
        node.Modulate = new Color(1, 1, 1, Math.Min(1, t / 0.06f) * Math.Min(1, (1 - t) / 0.12f));
        node.Visible = true;
    }
}
