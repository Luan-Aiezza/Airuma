using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// The little explosion when a spirit is defeated: a ring spreads from where it was and sparks of its colour fly out.
///
/// API
///   new BurstEffect(int capacity = 14)
///   Burst(Vector2 point, SpiritKind kind)   // point = screen px (Y down)
///   Update(float dt)   Reset()   int ActiveCount
///   const Duration = 0.55, SparksPerBurst = 8
/// Node must be at the scene origin. Give it a ZIndex above the enemies.
/// </summary>
public partial class BurstEffect : Node2D
{
    private struct BurstData
    {
        public bool Active;
        public Vector2 Origin;
        public float Age;
        public SpiritKind Kind;
    }

    public const float Duration = 0.55f;
    public const int SparksPerBurst = 8;

    private readonly BurstData[] _bursts;
    private readonly List<Sprite2D> _rings = new List<Sprite2D>();
    private readonly List<Sprite2D[]> _sparks = new List<Sprite2D[]>();
    private readonly Texture2D[] _ringTextures;
    private readonly Dictionary<SpiritKind, Texture2D> _sparkTextures = new Dictionary<SpiritKind, Texture2D>();

    public int ActiveCount { get { int n = 0; foreach (var b in _bursts) if (b.Active) n++; return n; } }

    public BurstEffect(int capacity = 14)
    {
        _bursts = new BurstData[capacity];
        _ringTextures = new Texture2D[BurstArt.RingFrames];
        for (int i = 0; i < _ringTextures.Length; i++) _ringTextures[i] = BurstArt.MakeRing(i).ToTexture();
        foreach (var kind in Enum.GetValues<SpiritKind>())
            _sparkTextures[kind] = BurstArt.MakeSpark(SpiritArt.PaletteFor(kind).Light).ToTexture();
        for (int i = 0; i < capacity; i++)
        {
            // Textures are art pixels; each art pixel is BossArt.Scale screen px.
            var ring = new Sprite2D { Visible = false, Scale = new Vector2(BossArt.Scale, BossArt.Scale) };
            AddChild(ring);
            _rings.Add(ring);
            var group = new Sprite2D[SparksPerBurst];
            for (int k = 0; k < SparksPerBurst; k++)
            {
                var spark = new Sprite2D { Visible = false, Scale = new Vector2(BossArt.Scale, BossArt.Scale) };
                AddChild(spark);
                group[k] = spark;
            }
            _sparks.Add(group);
        }
    }

    public void Burst(Vector2 point, SpiritKind kind)
    {
        int i = Array.FindIndex(_bursts, b => !b.Active);
        if (i < 0) return;
        _bursts[i] = new BurstData { Active = true, Origin = point, Age = 0, Kind = kind };
        Render(i);
    }

    public void Reset()
    {
        for (int i = 0; i < _bursts.Length; i++) Hide(i);
    }

    public void Update(float dt)
    {
        if (dt <= 0) return;
        for (int i = 0; i < _bursts.Length; i++)
        {
            if (!_bursts[i].Active) continue;
            _bursts[i].Age += dt;
            if (_bursts[i].Age >= Duration) Hide(i); else Render(i);
        }
    }

    private void Hide(int i)
    {
        _bursts[i].Active = false;
        _rings[i].Visible = false;
        foreach (var spark in _sparks[i]) spark.Visible = false;
    }

    private static Vector2 Aligned(Vector2 p) =>
        new Vector2(MathF.Round(p.X / BossArt.Scale) * BossArt.Scale, MathF.Round(p.Y / BossArt.Scale) * BossArt.Scale);

    private void Render(int i)
    {
        var b = _bursts[i];
        float t = b.Age / Duration;

        _rings[i].Texture = _ringTextures[Math.Min(BurstArt.RingFrames - 1, (int)(t * BurstArt.RingFrames))];
        _rings[i].Position = Aligned(b.Origin);
        _rings[i].Visible = true;

        var sparks = _sparks[i];
        for (int k = 0; k < sparks.Length; k++)
        {
            float angle = (float)k / SparksPerBurst * 2 * MathF.PI + 0.4f;
            float distance = 70 * (1 - MathF.Pow(1 - t, 2));
            var spark = sparks[k];
            spark.Texture = _sparkTextures[b.Kind];
            // sin is negated: the angle runs counter-clockwise, and the screen's Y axis points down.
            spark.Position = Aligned(new Vector2(b.Origin.X + MathF.Cos(angle) * distance, b.Origin.Y - MathF.Sin(angle) * distance));
            spark.Modulate = new Color(1, 1, 1, 1 - t);
            spark.Visible = true;
        }
    }
}
