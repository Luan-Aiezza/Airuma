using System;
using Godot;

/// <summary>
/// What an enraged boss gives off: a glow that beats behind it, sparks that fly up from it, and a ring that spreads when the rage begins.
/// Call <see cref="Update"/> every frame (the effect fades away by itself when the boss is not enraged) and <see cref="Burst"/> when it begins.
/// </summary>
public partial class EnrageEffect : Node2D
{
    private const int Sparks = 48, Rings = 3;

    private struct Spark { public bool Alive; public Vector2 Position, Velocity; public float Age, Life; }

    private readonly Sprite2D _aura = new() { Visible = false };
    private readonly Spark[] _sparks = new Spark[Sparks];
    private readonly Sprite2D[] _sparkNodes = new Sprite2D[Sparks];
    private readonly Sprite2D[] _rings = new Sprite2D[Rings];
    private readonly float[] _ringAge = new float[Rings];
    private readonly Random _rng = new Random(5);
    private float _strength, _clock, _spawn;
    private Color _main = Colors.OrangeRed, _accent = Colors.Gold;

    public EnrageEffect()
    {
        var add = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        _aura.Texture = MakeGlow(96);
        _aura.Material = add;
        _aura.ZIndex = 19;
        AddChild(_aura);
        var sparkTexture = new PixelCanvas(3, 3, PixelColor.Hex(0xFFFFFF)).ToTexture();
        for (int i = 0; i < Sparks; i++)
        {
            _sparkNodes[i] = new Sprite2D { Texture = sparkTexture, Visible = false, Material = add, ZIndex = 21, Scale = new Vector2(3, 3) };
            AddChild(_sparkNodes[i]);
        }
        var ringTexture = MakeRing(128);
        for (int i = 0; i < Rings; i++)
        {
            _rings[i] = new Sprite2D { Texture = ringTexture, Visible = false, Material = add, ZIndex = 21 };
            _ringAge[i] = 99;
            AddChild(_rings[i]);
        }
    }

    private static ImageTexture MakeGlow(int size)
    {
        var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = MathF.Sqrt(MathF.Pow(x + 0.5f - size / 2f, 2) + MathF.Pow(y + 0.5f - size / 2f, 2)) / (size / 2f);
                float a = d >= 1 ? 0 : MathF.Pow(1 - d, 1.8f);
                // flat bands, like the other glows
                a = MathF.Floor(a * 5 + 0.5f) / 5;
                img.SetPixel(x, y, new Color(1, 1, 1, a * 0.8f));
            }
        return ImageTexture.CreateFromImage(img);
    }

    private static ImageTexture MakeRing(int size)
    {
        var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = MathF.Sqrt(MathF.Pow(x + 0.5f - size / 2f, 2) + MathF.Pow(y + 0.5f - size / 2f, 2)) / (size / 2f);
                float ring = 1 - MathF.Abs(d - 0.9f) / 0.08f;
                if (ring > 0) img.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp(ring, 0, 1)));
            }
        return ImageTexture.CreateFromImage(img);
    }

    /// <summary>The rage begins: rings spread from the boss.</summary>
    public void Burst()
    {
        for (int i = 0; i < Rings; i++) _ringAge[i] = -i * 0.18f;
    }

    public void Reset()
    {
        _strength = 0;
        for (int i = 0; i < Sparks; i++) { _sparks[i].Alive = false; _sparkNodes[i].Visible = false; }
        for (int i = 0; i < Rings; i++) { _ringAge[i] = 99; _rings[i].Visible = false; }
        _aura.Visible = false;
    }

    public void Update(float dt, bool enraged, Vector2 at, float radius, Color main, Color accent)
    {
        _main = main; _accent = accent;
        _clock += dt;
        _strength = Mathf.MoveToward(_strength, enraged ? 1 : 0, dt * (enraged ? 1.5f : 1.2f));
        float beat = 0.5f + 0.5f * MathF.Sin(_clock * 7);

        _aura.Visible = _strength > 0.01f;
        _aura.Position = at;
        float s = (radius * 2 + 70 + 24 * beat) / 96f * 1.6f;
        _aura.Scale = new Vector2(s, s);
        _aura.Modulate = new Color(main.R, main.G, main.B, _strength * (0.45f + 0.3f * beat));

        // Sparks fly up from the boss and drift out.
        if (_strength > 0.3f)
        {
            _spawn -= dt;
            while (_spawn <= 0)
            {
                _spawn += 0.022f;
                int i = Array.FindIndex(_sparks, p => !p.Alive);
                if (i < 0) break;
                float a = (float)_rng.NextDouble() * MathF.Tau, r = radius * (0.5f + (float)_rng.NextDouble() * 0.9f);
                _sparks[i] = new Spark
                {
                    Alive = true, Position = at + new Vector2(MathF.Cos(a) * r, MathF.Sin(a) * r * 0.7f),
                    Velocity = new Vector2((float)(_rng.NextDouble() - 0.5) * 40, -50 - (float)_rng.NextDouble() * 80), Life = 0.7f + (float)_rng.NextDouble() * 0.6f,
                };
            }
        }
        for (int i = 0; i < Sparks; i++)
        {
            if (!_sparks[i].Alive) { _sparkNodes[i].Visible = false; continue; }
            _sparks[i].Age += dt;
            if (_sparks[i].Age >= _sparks[i].Life) { _sparks[i].Alive = false; _sparkNodes[i].Visible = false; continue; }
            _sparks[i].Position += _sparks[i].Velocity * dt;
            float t = _sparks[i].Age / _sparks[i].Life;
            _sparkNodes[i].Visible = true;
            _sparkNodes[i].Position = new Vector2(MathF.Round(_sparks[i].Position.X / 3) * 3, MathF.Round(_sparks[i].Position.Y / 3) * 3);
            var c = i % 3 == 0 ? accent : main;
            _sparkNodes[i].Modulate = new Color(c.R, c.G, c.B, 1 - t);
        }

        // The rings.
        for (int i = 0; i < Rings; i++)
        {
            _ringAge[i] += dt;
            float t = _ringAge[i] / 0.9f;
            _rings[i].Visible = t >= 0 && t < 1;
            if (!_rings[i].Visible) continue;
            _rings[i].Position = at;
            float k = (0.4f + 3.2f * t) * (radius / 40f + 0.5f);
            _rings[i].Scale = new Vector2(k, k);
            _rings[i].Modulate = new Color(accent.R, accent.G, accent.B, 0.9f * (1 - t));
        }
    }
}
