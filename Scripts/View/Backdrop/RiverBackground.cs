using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// The river. The water has no art of its own: it is a clear mirror (a SubViewport shown through water.gdshader) that
/// shows what lies under it, flipped and rippled: the night sky with its moon and stars, the crowns of the trees on
/// both banks, and whatever the scene adds to <see cref="MirrorLayer"/> (the boat, the boss, their lights).
/// In front of the water, only roots, branches and vines of the banks are seen, drawn over the actors.
///
/// API
///   new RiverBackground(Vector2 size)   // size = the river in screen px (Layout.RiverWidth x Layout.Height); parameterless = that size
///   Node2D MirrorLayer                  // add already-flipped mirror sprites here, in screen coordinates of the river
///                                       // (river top-left = (0,0), Y down: a reflection of a thing at p is drawn at your own flipped position).
///                                       // Children's ZIndex is local to the SubViewport. Use ReflectionLayer.* for any extra layers.
///   Update(float dt)                    // scrolls banks, ripples; advances the shader clock
///   Vector2 MoonGridPosition            // the moon in sky grid pixels (column, row from top); moon in screen px = MoonScreenPosition
///   Vector2 MoonScreenPosition
///
/// Z (ZIndex, relative to this node): water -100, ripples -47, roots 30, branches 32.
/// This node is expected at the scene origin (0,0). The SubViewport children: bounds -10, sky 0, crowns 10, MirrorLayer 20.
/// </summary>
public partial class RiverBackground : Node2D
{
    public const int GridWidth = SkyArt.Width;
    public const int GridHeight = SkyArt.Height;

    /// <summary>Rows per second the water surface scrolls (the boat sails "up", so the scenery moves down).</summary>
    public const float ScrollRowsPerSecond = 14;

    /// <summary>Things added here are drawn into the water's reflection (already mirrored by the caller).</summary>
    public readonly Node2D MirrorLayer = new Node2D();

    /// <summary>Draw order inside the reflection (ZIndex values inside the SubViewport).</summary>
    public static class ReflectionLayer
    {
        public const int Bounds = -10;
        public const int Sky = 0;
        public const int Crowns = 10;
        public const int Actors = 20;
    }

    public Vector2 MoonGridPosition => new Vector2(SkyArt.Moon.x, SkyArt.Moon.y);
    public Vector2 MoonScreenPosition => new Vector2(SkyArt.Moon.x, SkyArt.Moon.y) * (Size / new Vector2(GridWidth, GridHeight));

    public readonly Vector2 Size;
    public SubViewport WaterViewport { get; }

    private readonly List<ScrollingStrip> _strips = new List<ScrollingStrip>();
    /// <summary>The speed of each strip when the water does not rush (the waterfall makes the rocks pass faster).</summary>
    private readonly Dictionary<ScrollingStrip, double> _baseSpeed = new Dictionary<ScrollingStrip, double>();
    private double _bedOffset;
    private FlowStreaks _streaks;
    private SnowFall _snow;
    // Everything that belongs to a bank and slides away when the river opens into the sea: the node, its place, and which side.
    private readonly List<(Node2D node, float baseX, int side, float sway)> _banks = new List<(Node2D, float, int, float)>();
    private Sprite2D _sky, _horizonReflection, _deepReflection, _horizonSky, _haze;
    private ColorRect _horizonLine;
    private readonly List<CanvasItem> _crownNodes = new List<CanvasItem>();
    private float _open;             // 0 = the river, 1 = open water
    private double _openTime = -1, _openDuration = 3;
    private readonly List<ScrollingStrip> _ripples = new List<ScrollingStrip>();
    private readonly ShaderMaterial _water;
    private double _rippleClock;
    private double _time;

    // The lanterns afloat on the water during the boss fight at the festival: four, each kept in its place, bobbing a little.
    private readonly List<(Sprite2D sprite, Vector2 home)> _floaters = new List<(Sprite2D, Vector2)>();
    private bool _floatersWanted;
    private float _floatersShown;
    private static readonly Vector2[] FloaterSpots = { new Vector2(190, 330), new Vector2(440, 250), new Vector2(300, 540), new Vector2(460, 650) };

    private void BuildFloaters()
    {
        for (int i = 0; i < FloaterSpots.Length; i++)
        {
            var sprite = new Sprite2D { Texture = FestivalArt.MakeFloatingLantern(i * 2 + 1).ToTexture(), Scale = new Vector2(4, 4), ZIndex = -90, Visible = false, TextureFilter = TextureFilterEnum.Nearest };
            AddChild(sprite);
            _floaters.Add((sprite, FloaterSpots[i]));
        }
    }

    private void UpdateFloaters(float dt)
    {
        if (_floaters.Count == 0) return;
        _floatersShown = Mathf.MoveToward(_floatersShown, _floatersWanted ? 1 : 0, dt / 1.5f);
        for (int i = 0; i < _floaters.Count; i++)
        {
            var (sprite, home) = _floaters[i];
            sprite.Visible = _floatersShown > 0;
            sprite.Position = home + new Vector2(2.5f * Mathf.Sin((float)_time * 0.9f + i * 1.7f), 2f * Mathf.Sin((float)_time * 1.3f + i));
            sprite.Modulate = new Color(1, 1, 1, _floatersShown);
        }
    }

    // The cave: fish and fireflies, and the platform of the boss with its shaft of moonlight (which come in with the opening of the cave).
    private CaveLife _life;
    private Sprite2D _platform, _beam;
    private static readonly Vector2 PlatformCentre = new Vector2(Layout.FieldInset + Layout.PlayWidth / 2, Layout.Horizon);

    private void BuildPlatform()
    {
        const float pixel = 4;
        _platform = new Sprite2D { Texture = CaveArt.MakePlatform().ToTexture(), Centered = false, Scale = new Vector2(pixel, pixel), ZIndex = -90, TextureFilter = TextureFilterEnum.Nearest, Visible = false };
        var beam = CaveArt.MakeMoonbeam();
        _beam = new Sprite2D
        {
            Texture = beam.ToTexture(), Centered = false, Scale = new Vector2(pixel, pixel), ZIndex = -89, TextureFilter = TextureFilterEnum.Nearest, Visible = false,
            Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add },
        };
        AddChild(_platform);
        AddChild(_beam);
    }

    /// <summary>The platform comes down from above the picture as the cave opens; its light fades in once it is in place.</summary>
    private void PlacePlatform(float e)
    {
        if (_platform == null) return;
        const float pixel = 4;
        float top = PlatformCentre.Y - CaveArt.PlatformTopCentreY * pixel;
        _platform.Visible = e > 0.001f;
        _platform.Position = new Vector2(PlatformCentre.X - CaveArt.PlatformWidth * pixel / 2, Mathf.Lerp(-CaveArt.PlatformHeight * pixel - 20, top, e));
        _beam.Visible = e > 0.5f;
        _beam.Modulate = new Color(1, 1, 1, Mathf.Clamp((e - 0.5f) * 2, 0, 1));
        _beam.Position = new Vector2(PlatformCentre.X - 64 * pixel / 2, PlatformCentre.Y - 50 * pixel + 16);
    }

    private readonly Scenery _look;
    private StormWeather _weather;
    /// <summary>The sky of the boss fight (the clouds parted, with the moon), faded in over the stormy one; and how far it has faded in (0...1).</summary>
    private Sprite2D _clearSky;
    private float _clear;
    /// <summary>How agitated the water is now (it moves towards what the scenery says for the moment: the boss fight is rougher).</summary>
    private float _agitation;
    /// <summary>How far the river of stars has opened into the sea of the fight (0...1): its banks are always gone (<see cref="_open"/> is 1 from the start), but the horizon comes down only when the boss comes.</summary>
    private float _sea;
    private bool _bossFight;
    /// <summary>The sky of the boss's second form (the Moon gone red), faded in over the sky of the fight, and how far it has faded in (0...1).</summary>
    private Sprite2D _horizonSkyRage, _horizonReflectionRage;
    private float _rage, _rageTarget;

    /// <summary>The boss is in her second form (or not): the sky of the fight turns red (or back).</summary>
    public void SetRage(bool on) => _rageTarget = on ? 1 : 0;

    private void PlaceRage()
    {
        if (_horizonSkyRage == null) return;
        float e = _look.Cosmic ? _sea : _open;
        bool visible = e > 0.001f && _rage > 0.001f;
        _horizonSkyRage.Visible = _horizonReflectionRage.Visible = visible;
        if (!visible) return;
        _horizonSkyRage.Position = _horizonSky.Position;
        _horizonSkyRage.Modulate = new Color(1, 1, 1, _rage * Mathf.Clamp(e * 3, 0, 1));
        _horizonReflectionRage.Position = _horizonReflection.Position;
        _horizonReflectionRage.Modulate = new Color(1, 1, 1, _rage * e);
    }
    /// <summary>Where the water's glitter path starts: at the moon, or nowhere (far off the picture) when there is no moon.</summary>
    private Vector2 MoonSpot => _look.OpensToSea && !_look.Moonless ? MoonGridPosition : new Vector2(-1000, -1000);

    public RiverBackground() : this(new Vector2(Layout.RiverWidth, Layout.Height)) { }

    public RiverBackground(Vector2 size, Scenery scenery = null)
    {
        Size = size;
        _look = scenery ?? Scenery.Jungle;
        // The banks slide out of the river when the boss comes: everything is cut at the edges of the river, so that what slides out
        // does not show over the walls and pillars beside the game.
        RenderingServer.CanvasItemSetClip(GetCanvasItem(), true);
        RenderingServer.CanvasItemSetCustomRect(GetCanvasItem(), true, new Rect2(Vector2.Zero, size));
        const float pixel = 4;
        int rows = GridHeight;
        double speed = ScrollRowsPerSecond;

        // Reflection (the water)
        WaterViewport = new SubViewport
        {
            Size = new Vector2I((int)size.X, (int)size.Y),
            TransparentBg = true,
            Disable3D = true,
            CanvasItemDefaultTextureFilter = Godot.Viewport.DefaultCanvasItemTextureFilter.Nearest,
            RenderTargetUpdateMode = SubViewport.UpdateMode.Always,
        };
        AddChild(WaterViewport);

        var bounds = new Sprite2D
        {
            Texture = new PixelCanvas(1, 1, new PixelColor(0, 0, 0, 1)).ToTexture(),
            Centered = false, Scale = size, ZIndex = ReflectionLayer.Bounds,
        };
        WaterViewport.AddChild(bounds);

        var skyCanvas = _look.MakeSky();
        var sky = new Sprite2D
        {
            Texture = skyCanvas.ToTexture(), Centered = false,
            Scale = size / new Vector2(skyCanvas.Width, skyCanvas.Height), ZIndex = ReflectionLayer.Sky,
        };
        WaterViewport.AddChild(sky);
        _sky = sky;
        if (_look.ClearsForBoss)
        {
            var clear = StormArt.MakeClearSky();
            _clearSky = new Sprite2D
            {
                Texture = clear.ToTexture(), Centered = false, Scale = size / new Vector2(clear.Width, clear.Height),
                ZIndex = ReflectionLayer.Sky + 1, Modulate = new Color(1, 1, 1, 0),
            };
            WaterViewport.AddChild(_clearSky);
        }

        // For the open water: the horizon sky, flipped, is what the sea reflects (its first row, the horizon, at the top).
        var horizonTexture = (_look.MakeHorizon ?? HorizonSkyArt.MakeCanvas)().ToTexture();
        _horizonReflection = new Sprite2D
        {
            Texture = horizonTexture, Centered = false, Scale = new Vector2(pixel, -pixel * (_look.MirrorLake ? 1 : 2)), ZIndex = ReflectionLayer.Sky + 1, Visible = false,
        };
        WaterViewport.AddChild(_horizonReflection);
        if (_look.MakeHorizonRage != null)
        {
            var redTexture = _look.MakeHorizonRage().ToTexture();
            _horizonReflectionRage = new Sprite2D
            {
                Texture = redTexture, Centered = false, Scale = new Vector2(pixel, -pixel * (_look.MirrorLake ? 1 : 2)), ZIndex = ReflectionLayer.Sky + 2, Visible = false,
            };
            WaterViewport.AddChild(_horizonReflectionRage);
            _horizonSkyRage = new Sprite2D { Texture = redTexture, Centered = false, Scale = new Vector2(pixel, pixel), ZIndex = -94, Visible = false, TextureFilter = TextureFilterEnum.Nearest };
            AddChild(_horizonSkyRage);
        }
        // ... and below it, the high sky, so that the far water goes on being a surface that reflects the stars.
        _deepReflection = new Sprite2D
        {
            Texture = (_look.MakeDeep ?? (() => HorizonSkyArt.MakeDeepCanvas()))().ToTexture(), Centered = false, Scale = new Vector2(pixel, pixel * 2),
            ZIndex = ReflectionLayer.Sky + 1, Visible = false,
        };
        WaterViewport.AddChild(_deepReflection);

        // Tree crowns, mirrored into the water: they start a little inside the river (the river of stars has no banks).
        var sides = new[] { BankArt.Side.Left, BankArt.Side.Right };
        for (int i = 0; i < (_look.Cosmic ? 0 : 2); i++)
        {
            var texture = _look.MakeReflection(sides[i]).ToTexture();
            float crownWidth = texture.GetWidth() * pixel;
            float x = i == 0 ? 20 : size.X - crownWidth - 20;
            var strip = new ScrollingStrip(new Texture2D[] { texture }, BankArt.Height, rows, pixel, speed * 0.8, new Vector2(x, 0))
            { ZIndex = ReflectionLayer.Crowns };
            WaterViewport.AddChild(strip);
            _strips.Add(strip); _baseSpeed[strip] = speed * 0.8;
            _banks.Add((strip, x, i == 0 ? -1 : 1, 0.5f));
            _crownNodes.Add(strip);
        }
        MirrorLayer.ZIndex = ReflectionLayer.Actors;
        WaterViewport.AddChild(MirrorLayer);

        _water = new ShaderMaterial { Shader = GD.Load<Shader>("res://Shaders/water.gdshader") };
        var bedCanvas = (_look.MakeBed ?? BedArt.MakeCanvas)();
        _water.SetShaderParameter("bed", bedCanvas.ToTexture());
        _water.SetShaderParameter("moon", MoonSpot);
        _water.SetShaderParameter("grid", new Vector2(GridWidth, GridHeight));
        _water.SetShaderParameter("bed_size", new Vector2(bedCanvas.Width, bedCanvas.Height));
        _water.SetShaderParameter("scroll_rows", ScrollRowsPerSecond * 0.65f);
        _water.SetShaderParameter("refl_strength", _look.RiverReflection);
        var water = new Sprite2D
        {
            Texture = WaterViewport.GetTexture(), Centered = false, ZIndex = -100,
            Material = _water, TextureFilter = TextureFilterEnum.Nearest,
        };
        AddChild(water);

        // Foreground (roots, branches, vines)
        float bankWidth = BankArt.Width * pixel;
        foreach (var side in _look.Cosmic ? System.Array.Empty<BankArt.Side>() : sides)
        {
            float x = side == BankArt.Side.Left ? 0 : size.X - bankWidth;
            var origin = new Vector2(x, 0);

            var rippleCanvases = _look.MakeRipples(side);
            var rippleTex = new Texture2D[rippleCanvases.Length];
            for (int k = 0; k < rippleTex.Length; k++) rippleTex[k] = rippleCanvases[k].ToTexture();
            var ripple = new ScrollingStrip(rippleTex, BankArt.Height, rows, pixel, speed, origin) { ZIndex = -47 };
            AddChild(ripple);
            _ripples.Add(ripple); _baseSpeed[ripple] = speed;
            _banks.Add((ripple, x, side == BankArt.Side.Left ? -1 : 1, 0.25f));

            var roots = new ScrollingStrip(new Texture2D[] { _look.MakeGround(side).ToTexture() }, BankArt.Height, rows, pixel, speed, origin)
            { ZIndex = 30 }; // over the boat and the boss: they pass under the trees
            AddChild(roots);
            _strips.Add(roots); _baseSpeed[roots] = speed;
            _banks.Add((roots, x, side == BankArt.Side.Left ? -1 : 1, 0.25f));

            // Branches hang closer to the camera, so they move a little faster.
            if (_look.MakeOverhead != null)
            {
                var branches = new ScrollingStrip(new Texture2D[] { _look.MakeOverhead(side).ToTexture() }, BankArt.Height, rows, pixel, speed * 1.35, origin)
                { ZIndex = 32 };
                AddChild(branches);
                _strips.Add(branches); _baseSpeed[branches] = speed * 1.35;
                _banks.Add((branches, x, side == BankArt.Side.Left ? -1 : 1, 1f));
            }
        }

        // The real sky over the open water (above the horizon), a haze and a thin bright line on the horizon.
        _horizonSky = new Sprite2D
        {
            Texture = horizonTexture, Centered = false, Scale = new Vector2(pixel, pixel), ZIndex = -95, Visible = false,
            TextureFilter = TextureFilterEnum.Nearest,
        };
        AddChild(_horizonSky);
        var hazeCanvas = new PixelCanvas(1, 24);
        for (int y = 0; y < 24; y++) hazeCanvas[0, y] = PixelColor.Hex(_look.Cosmic ? 0xB89CE0u : 0x86B4CCu, (byte)(150 * System.Math.Pow(1 - y / 24f, 1.6)));
        _haze = new Sprite2D
        {
            Texture = hazeCanvas.ToTexture(), Centered = false, Scale = new Vector2(size.X, 5), ZIndex = -94, Visible = false,
            TextureFilter = TextureFilterEnum.Linear,
        };
        AddChild(_haze);
        if (_look.FloatingLanterns) BuildFloaters();
        if (_look.Platform) BuildPlatform();
        if (_look.CaveLife) { _life = new CaveLife(size); AddChild(_life); }
        if (_look.WaterfallRush)
        {
            // fish swept up the fall, and the streaks of the water rushing up
            _life = new CaveLife(size, fireflies: false, fishSpeed: 5.5f, upward: true); AddChild(_life);
            _streaks = new FlowStreaks(size) { ZIndex = -97 }; AddChild(_streaks);
        }
        if (_look.Snow) { _snow = new SnowFall(size) { ZIndex = 18 }; AddChild(_snow); }
        if (_look.Rains) { _weather = new StormWeather(size, _look.RainContinues); AddChild(_weather); }
        _water.SetShaderParameter("grade", new Vector3(_look.WaterGrade.R, _look.WaterGrade.G, _look.WaterGrade.B));
        _agitation = _look.Agitation;
        foreach (var (node, _, _, _) in _banks) node.Modulate = _look.BankTint;
        _horizonLine = new ColorRect { Color = _look.Cosmic ? new Color(0.82f, 0.74f, 0.96f, 0.55f) : new Color(0.72f, 0.88f, 0.95f, 0.55f), Size = new Vector2(size.X, 2), ZIndex = -93, Visible = false };
        AddChild(_horizonLine);
        if (_look.Cosmic) _open = 1;     // no banks: the sides of the field are open from the start
    }

    /// <summary>Opens the river into the sea: the banks slide out, and the horizon comes down from above the screen with the moon on it.</summary>
    public void BeginOpenWater(float seconds = 3f)
    {
        // At the festival the banks still slide away, but there is no horizon: the river stays as it is, with a few lanterns afloat on the water.
        if (_look.FloatingLanterns) _floatersWanted = true;
        _bossFight = true;

        _weather?.SetFight(true);
        _openTime = 0;
        _openDuration = seconds;
    }

    /// <summary>Back to the river at once.</summary>
    public void CloseWater()
    {
        _floatersWanted = false;
        _bossFight = false;
        _weather?.SetFight(false);
        _openTime = -1;
        _sea = 0;
        _rage = _rageTarget = 0;
        _open = _look.Cosmic ? 1 : 0;
        ApplyOpen();
    }

    private void ApplyOpen()
    {
        float e = _look.Cosmic ? _sea : _open;
        bool open = e > 0.001f;
        PlaceBanks();
        PlacePlatform(e);
        _sky.Modulate = new Color(1, 1, 1, _look.OpensToSea ? 1 - e : 1);
        foreach (var c in _crownNodes) c.Modulate = new Color(_look.BankTint.R, _look.BankTint.G, _look.BankTint.B, 1 - e);
        _horizonSky.Visible = _haze.Visible = _horizonLine.Visible = _horizonReflection.Visible = _deepReflection.Visible = open && _look.OpensToSea;
        if (!_look.OpensToSea) return;
        if (!open)
        {
            _water.SetShaderParameter("moon", MoonSpot);
            return;
        }
        // The horizon comes down into place; the sky is above it and the reflection below.
        float horizon = Mathf.Lerp(-HorizonSkyArt.Height * 4 - 60, Layout.Horizon, e);
        float band = HorizonSkyArt.Height * 4;
        _horizonSky.Position = new Vector2(0, horizon - band);
        _horizonSky.Modulate = new Color(1, 1, 1, Mathf.Clamp(e * 3, 0, 1));
        _haze.Position = new Vector2(0, horizon);
        _haze.Modulate = new Color(1, 1, 1, e);
        _horizonLine.Position = new Vector2(0, horizon - 1);
        _horizonLine.Modulate = new Color(1, 1, 1, e);
        float reach = _look.MirrorLake ? 1 : 2;            // the sea stretches the sky in its reflection; a lake mirrors it as it is
        _horizonReflection.Position = new Vector2(0, horizon + band * reach);
        _horizonReflection.Modulate = new Color(1, 1, 1, e);
        _deepReflection.Position = new Vector2(0, horizon + band * reach);
        _deepReflection.Modulate = new Color(1, 1, 1, e);
        // the water's glitter path starts at the moon, on the horizon
        // (a lake has no path of light under the moon: the moon is simply mirrored in it)
        _water.SetShaderParameter("moon", _look.MirrorLake ? new Vector2(-1000, -1000) : new Vector2(HorizonSkyArt.Moon.x, horizon / 4f));
    }

    /// <summary>Puts the banks where they are: slid out as far as the river is open, and bent by the wind (the branches more than the roots).</summary>
    private void PlaceBanks()
    {
        float span = BankArt.Width * 4 + 80;
        float t = (float)_time;
        // The wind comes in gusts: the trees bend a good way and spring back, a little more every few seconds.
        float gust = 0.65f + 0.35f * Mathf.Sin(t * 0.45f);
        float bend = _look.BankSway * gust * (0.6f + 0.4f * Mathf.Sin(t * 1.7f) + 0.25f * Mathf.Sin(t * 3.1f + 1)) + _look.BankSway * 0.35f;
        foreach (var (node, baseX, side, sway) in _banks)
            node.Position = new Vector2(baseX + side * _open * span + bend * sway, node.Position.Y);
    }

    /// <summary>How far the banks have slid out of the river (0: not at all, 1: gone): the boat has that much more room.</summary>
    public float OpenAmount => _open;

    public void Update(float dt)
    {
        // The bed of the river scrolls with the boat, down the picture; in the waterfall the water rushes up, until the lake calms it.
        double bedSpeed = ScrollRowsPerSecond * 0.65;
        float rush = _look.WaterfallRush ? 1 - _open : 0;
        bedSpeed = bedSpeed * (1 - rush) - rush * ScrollRowsPerSecond * 2.4;
        _bedOffset += bedSpeed * dt;
        _water.SetShaderParameter("bed_offset", (float)_bedOffset);
        if (_look.WaterfallRush)
        {
            // the rocks pass quickly, and slowly again once the fight begins
            float factor = Mathf.Lerp(_look.RockSpeedFactor, 1f, _open);
            foreach (var pair in _baseSpeed) pair.Key.RowsPerSecond = pair.Value * factor;
            _water.SetShaderParameter("refl_strength", Mathf.Lerp(_look.RiverReflection, _look.LakeReflection, _open));
            _streaks?.Update(dt, rush);
        }
        if (_horizonSkyRage != null) { _rage = Mathf.MoveToward(_rage, _rageTarget, dt / 2.5f); PlaceRage(); }
        _snow?.Update(dt, _open);
        UpdateFloaters(dt);
        _life?.Update(dt, _open);
        _weather?.Update(dt, _open);
        _agitation = Mathf.MoveToward(_agitation, _bossFight ? _look.BossAgitation : _look.Agitation, dt * 0.7f);
        _water.SetShaderParameter("agitation", _agitation);
        if (_clearSky != null)
        {
            // The clouds part: the moon fades in, in the middle of the water, and its glitter goes up as well as down.
            _clear = Mathf.MoveToward(_clear, _bossFight ? 1 : 0, dt / 2.2f);
            _clearSky.Modulate = new Color(1, 1, 1, _clear);
            _water.SetShaderParameter("moon", new Vector2(StormArt.Moon.x, StormArt.Moon.y));
            _water.SetShaderParameter("moon_strength", _clear);
            _water.SetShaderParameter("glitter_both", 1f);
        }
        if (_look.BankSway > 0) PlaceBanks();
        if (_openTime >= 0)
        {
            _openTime += dt;
            float u = Mathf.Clamp((float)(_openTime / _openDuration), 0, 1);
            if (_look.Cosmic) _sea = u * u * (3 - 2 * u); else _open = u * u * (3 - 2 * u);     // (the river of stars has no banks to slide, only the horizon to bring)
            ApplyOpen();
            if (u >= 1) _openTime = -1;
        }
        foreach (var strip in _strips) strip.Advance(dt);
        _rippleClock += dt;
        foreach (var ripple in _ripples)
        {
            ripple.Variant = (int)(_rippleClock / _look.RipplePeriod) % Math.Max(1, ripple.VariantCount);
            ripple.Advance(dt);
        }
        _time += dt;
        _water.SetShaderParameter("time", (float)_time);
    }
}
