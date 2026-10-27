using System;
using System.Collections.Generic;
using Godot;

/// <summary>The menu's living backdrop: the layers of <see cref="MenuArt"/> stacked up, with a rippling mirror for the water, twinkling
/// stars, drifting mist, fireflies, the lantern's flickering light.
/// Coordinates are screen pixels (Y down).</summary>
public partial class MenuBackdrop : Node2D
{
    private const float Pixel = MenuArt.Pixel;
    private const float SceneHeight = MenuArt.Height * Pixel;

    private class Star { public ColorRect Node; public float Delay, WaitUp, WaitDown; }
    private class Mist { public Node2D Node; public float Width, Duration; }
    private class Firefly { public Sprite2D Node; public Vector2 Base, Radius, Speed; public float Phase; }

    private readonly List<Star> _stars = new();
    private readonly List<Mist> _mists = new();
    private readonly List<Firefly> _fireflies = new();
    private readonly Texture2D[] _fireflyTextures = { MenuArt.MakeFirefly(false).ToTexture(), MenuArt.MakeFirefly(true).ToTexture() };

    private readonly Sprite2D _lantern = new() { Centered = true };
    private readonly Sprite2D _lanternGlow = new() { Centered = true };
    private Texture2D[] _lanternFrames, _glowFrames;
    private readonly CanvasItemMaterial _additive = new() { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
    private readonly SeededGenerator _flickerRng = new(0xF11C);
    private float _glowTimer, _glowDuration = 0.3f;
    private bool _glowLow;
    private float _clock;

    /// <summary>Where the lantern hangs, in screen pixels.</summary>
    public Vector2 LanternPosition => Point(MenuArt.Lantern.x, MenuArt.Lantern.y);

    public override void _Ready() { Build(); }

    private static Vector2 Point(float column, float row) => new Vector2(column * Pixel, row * Pixel);

    private static int Z(float z) => (int)MathF.Round(z * 10);

    private Sprite2D Layer(PixelCanvas canvas, float z, int row = 0)
    {
        var node = new Sprite2D
        {
            Texture = canvas.ToTexture(), Centered = false, Scale = new Vector2(Pixel, Pixel),
            Position = new Vector2(0, row * Pixel), ZIndex = Z(z),
        };
        AddChild(node);
        return node;
    }

    private void Build()
    {
        var sky = MenuArt.MakeSky();
        var far = MenuArt.MakeTreeline(1, 14, 4, 7, 0.5f, false);
        var mid = MenuArt.MakeTreeline(2, 20, 5, 9, 0.28f, true);
        var mist = MenuArt.MakeHorizonMist();

        Layer(sky, 0);
        Layer(far, 1);
        Layer(mid, 2);
        Layer(mist, 3);
        AddStars(sky);

        // The water: a mirror of the sky and the far trees, rippled by a shader.
        var water = Layer(MenuArt.MakeWaterReflection(sky, new[] { far, mid }, mist), 4, MenuArt.Horizon);
        var mat = new ShaderMaterial { Shader = GD.Load<Shader>("res://Shaders/MenuWater.gdshader") };
        mat.SetShaderParameter("grid_w", (float)MenuArt.Width);
        mat.SetShaderParameter("grid_h", (float)(MenuArt.Height - MenuArt.Horizon));
        mat.SetShaderParameter("moon_x", (float)MenuArt.Moon.x);
        water.Material = mat;

        Layer(MenuArt.MakeBanks(), 6);
        AddMist(98, 22, 0.55f, 110, 5, 3);
        AddMist(150, 24, 0.4f, 80, 6.5f, 4);
        AddFireflies();

        Layer(MenuArt.MakeBow(), 8);
        AddLantern();
    }

    // MARK: Stars

    private void AddStars(PixelCanvas sky)
    {
        var rng = new SeededGenerator(0x57A5);
        var candidates = new List<(int x, int y)>();
        for (int y = 0; y < MenuArt.Horizon - 20; y++)
            for (int x = 0; x < sky.Width; x++)
            {
                var c = sky[x, y];
                if (c.R + c.G + c.B > 560 && MathF.Sqrt((x - MenuArt.Moon.x) * (x - MenuArt.Moon.x) + (y - MenuArt.Moon.y) * (y - MenuArt.Moon.y)) > 20)
                    candidates.Add((x, y));
            }
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = rng.Range(0, i);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }
        for (int i = 0; i < Math.Min(36, candidates.Count); i++)
        {
            var (x, y) = candidates[i];
            var node = new ColorRect
            {
                Color = new Color(0.85f, 0.92f, 1f, 1f), Size = new Vector2(Pixel, Pixel), Position = Point(x, y),
                ZIndex = Z(0.5f), Modulate = new Color(1, 1, 1, 0), MouseFilter = Control.MouseFilterEnum.Ignore,
            };
            AddChild(node);
            // Each star blinks on its own rhythm.
            _stars.Add(new Star { Node = node, Delay = rng.Range(0f, 4f), WaitUp = rng.Range(0.2f, 1.4f), WaitDown = rng.Range(1.5f, 5f) });
        }
    }

    private static float StarAlpha(Star s, float clock)
    {
        float t = clock - s.Delay;
        if (t < 0) return 0;
        float cycle = 0.5f + s.WaitUp + 0.7f + s.WaitDown;
        t %= cycle;
        if (t < 0.5f) return 0.9f * t / 0.5f;
        t -= 0.5f;
        if (t < s.WaitUp) return 0.9f;
        t -= s.WaitUp;
        if (t < 0.7f) return 0.9f * (1 - t / 0.7f);
        return 0;
    }

    // MARK: Mist

    private void AddMist(int row, int rows, float alpha, float duration, float z, ulong seed)
    {
        var rng = new SeededGenerator(seed);
        var noise = new PeriodicNoise(9, 3, rng);
        var c = new PixelCanvas(MenuArt.Width, rows);
        var mist = PixelColor.Hex(0x9CC8DA);
        for (int y = 0; y < rows; y++)
        {
            // Thickest in the middle rows, fading to nothing at the edges.
            float edge = 1 - MathF.Abs((float)y / rows * 2 - 1);
            for (int x = 0; x < MenuArt.Width; x++)
            {
                float n = noise.Sample((float)x / MenuArt.Width, (float)y / rows);
                float level = MathF.Floor((n * edge * 1.6f - 0.35f) * 4 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / 4;
                if (level <= 0) continue;
                c[x, y] = mist.WithAlpha((byte)Math.Min(255, (int)(110 * level * 2)));
            }
        }
        var texture = c.ToTexture();
        float width = MenuArt.Width * Pixel;
        var band = new Node2D { ZIndex = Z(z), Modulate = new Color(1, 1, 1, alpha), Position = new Vector2(0, row * Pixel) };
        for (int i = 0; i < 2; i++)
            band.AddChild(new Sprite2D { Texture = texture, Centered = false, Scale = new Vector2(Pixel, Pixel), Position = new Vector2(i * width, 0) });
        AddChild(band);
        _mists.Add(new Mist { Node = band, Width = width, Duration = duration });
    }

    // MARK: Fireflies

    private void AddFireflies()
    {
        var rng = new SeededGenerator(0xF1F1);
        for (int i = 0; i < 26; i++)
        {
            var node = new Sprite2D { Texture = _fireflyTextures[0], Scale = new Vector2(Pixel, Pixel), Material = _additive, ZIndex = Z(7) };
            AddChild(node);
            _fireflies.Add(new Firefly
            {
                Node = node,
                Base = new Vector2(rng.Range(80f, 1040f), rng.Range(110f, 470f)),
                Radius = new Vector2(rng.Range(20f, 80f), rng.Range(12f, 40f)),
                Speed = new Vector2(rng.Range(0.25f, 0.7f), rng.Range(0.3f, 0.8f)),
                Phase = rng.Range(0f, 2 * MathF.PI),
            });
        }
    }

    // MARK: Lantern

    private void AddLantern()
    {
        _lanternFrames = new[] { MenuArt.MakeLantern(0).ToTexture(), MenuArt.MakeLantern(1).ToTexture() };
        var position = LanternPosition;

        _lantern.Texture = _lanternFrames[0];
        _lantern.Scale = new Vector2(Pixel, Pixel);
        _lantern.Position = position;
        _lantern.ZIndex = Z(9);
        AddChild(_lantern);

        _glowFrames = new[]
        {
            GlowArt.MakeCanvas(91, 44, PixelColor.Hex(0xFFD27A), PixelColor.Hex(0xFF8A2A), new GlowArt.Tiers(120, 80, 40)).ToTexture(),
            GlowArt.MakeCanvas(91, 39, PixelColor.Hex(0xFFD27A), PixelColor.Hex(0xFF8A2A), new GlowArt.Tiers(120, 80, 40)).ToTexture(),
        };
        _lanternGlow.Texture = _glowFrames[0];
        _lanternGlow.Scale = new Vector2(Pixel, Pixel);
        _lanternGlow.Position = position;
        _lanternGlow.Material = _additive;
        _lanternGlow.ZIndex = Z(9.5f);
        AddChild(_lanternGlow);

        Update(0);
    }

    // MARK: Animation

    public override void _Process(double delta) { Update((float)Math.Min(delta, 0.1)); }

    public void Update(float dt)
    {
        _clock += dt;
        foreach (var s in _stars) s.Node.Modulate = new Color(1, 1, 1, StarAlpha(s, _clock));
        foreach (var m in _mists)
            m.Node.Position = new Vector2(-m.Width * ((_clock / m.Duration) % 1f), m.Node.Position.Y);
        foreach (var f in _fireflies)
        {
            // Computed in the old Y-up numbers, then flipped to screen space.
            float x = f.Base.X + MathF.Sin(_clock * f.Speed.X + f.Phase) * f.Radius.X;
            float y = f.Base.Y + MathF.Cos(_clock * f.Speed.Y + f.Phase * 1.7f) * f.Radius.Y;
            f.Node.Position = new Vector2(x, SceneHeight - y);
            float blink = MathF.Sin(_clock * 1.6f + f.Phase * 3);
            f.Node.Modulate = new Color(1, 1, 1, MathF.Max(0, 0.25f + 0.75f * blink));
            f.Node.Texture = _fireflyTextures[blink > 0.55f ? 1 : 0];
        }

        // Lantern frames swap every 0.2 s; the glow flickers with a random rhythm.
        _lantern.Texture = _lanternFrames[(int)(_clock / 0.2f) % 2];
        _glowTimer += dt;
        while (_glowTimer >= _glowDuration)
        {
            _glowTimer -= _glowDuration;
            _glowLow = !_glowLow;
            _glowDuration = _glowLow ? 0.13f + _flickerRng.Range(-0.05f, 0.05f) : 0.3f + _flickerRng.Range(-0.1f, 0.1f);
        }
        _lanternGlow.Texture = _glowFrames[_glowLow ? 1 : 0];
    }
}
