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
    // Everything that belongs to a bank and slides away when the river opens into the sea: the node, its place, and which side.
    private readonly List<(Node2D node, float baseX, int side)> _banks = new List<(Node2D, float, int)>();
    private Sprite2D _sky, _horizonReflection, _deepReflection, _horizonSky, _haze;
    private ColorRect _horizonLine;
    private readonly List<CanvasItem> _crownNodes = new List<CanvasItem>();
    private float _open;             // 0 = the river, 1 = open water
    private double _openTime = -1, _openDuration = 3;
    private readonly List<ScrollingStrip> _ripples = new List<ScrollingStrip>();
    private readonly ShaderMaterial _water;
    private double _rippleClock;
    private double _time;

    public RiverBackground() : this(new Vector2(Layout.RiverWidth, Layout.Height)) { }

    public RiverBackground(Vector2 size)
    {
        Size = size;
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

        var skyCanvas = SkyArt.MakeCanvas();
        var sky = new Sprite2D
        {
            Texture = skyCanvas.ToTexture(), Centered = false,
            Scale = size / new Vector2(skyCanvas.Width, skyCanvas.Height), ZIndex = ReflectionLayer.Sky,
        };
        WaterViewport.AddChild(sky);
        _sky = sky;

        // For the open water: the horizon sky, flipped, is what the sea reflects (its first row, the horizon, at the top).
        var horizonTexture = HorizonSkyArt.MakeCanvas().ToTexture();
        _horizonReflection = new Sprite2D
        {
            Texture = horizonTexture, Centered = false, Scale = new Vector2(pixel, -pixel * 2), ZIndex = ReflectionLayer.Sky + 1, Visible = false,
        };
        WaterViewport.AddChild(_horizonReflection);
        // ... and below it, the high sky, so that the far water goes on being a surface that reflects the stars.
        _deepReflection = new Sprite2D
        {
            Texture = HorizonSkyArt.MakeDeepCanvas().ToTexture(), Centered = false, Scale = new Vector2(pixel, pixel * 2),
            ZIndex = ReflectionLayer.Sky + 1, Visible = false,
        };
        WaterViewport.AddChild(_deepReflection);

        // Tree crowns, mirrored into the water: they start a little inside the river.
        var sides = new[] { BankArt.Side.Left, BankArt.Side.Right };
        for (int i = 0; i < 2; i++)
        {
            var texture = BankArt.MakeCrowns(sides[i]).ToTexture();
            float crownWidth = texture.GetWidth() * pixel;
            float x = i == 0 ? 20 : size.X - crownWidth - 20;
            var strip = new ScrollingStrip(new Texture2D[] { texture }, BankArt.Height, rows, pixel, speed * 0.8, new Vector2(x, 0))
            { ZIndex = ReflectionLayer.Crowns };
            WaterViewport.AddChild(strip);
            _strips.Add(strip);
            _banks.Add((strip, x, i == 0 ? -1 : 1));
            _crownNodes.Add(strip);
        }
        MirrorLayer.ZIndex = ReflectionLayer.Actors;
        WaterViewport.AddChild(MirrorLayer);

        _water = new ShaderMaterial { Shader = GD.Load<Shader>("res://Shaders/water.gdshader") };
        _water.SetShaderParameter("bed", BedArt.MakeCanvas().ToTexture());
        _water.SetShaderParameter("moon", MoonGridPosition);
        _water.SetShaderParameter("grid", new Vector2(GridWidth, GridHeight));
        _water.SetShaderParameter("bed_size", new Vector2(BedArt.Width, BedArt.Height));
        _water.SetShaderParameter("scroll_rows", ScrollRowsPerSecond * 0.65f);
        var water = new Sprite2D
        {
            Texture = WaterViewport.GetTexture(), Centered = false, ZIndex = -100,
            Material = _water, TextureFilter = TextureFilterEnum.Nearest,
        };
        AddChild(water);

        // Foreground (roots, branches, vines)
        float bankWidth = BankArt.Width * pixel;
        foreach (var side in sides)
        {
            float x = side == BankArt.Side.Left ? 0 : size.X - bankWidth;
            var origin = new Vector2(x, 0);

            var rippleCanvases = BankArt.MakeRipples(side);
            var rippleTex = new Texture2D[rippleCanvases.Length];
            for (int k = 0; k < rippleTex.Length; k++) rippleTex[k] = rippleCanvases[k].ToTexture();
            var ripple = new ScrollingStrip(rippleTex, BankArt.Height, rows, pixel, speed, origin) { ZIndex = -47 };
            AddChild(ripple);
            _ripples.Add(ripple);
            _banks.Add((ripple, x, side == BankArt.Side.Left ? -1 : 1));

            var roots = new ScrollingStrip(new Texture2D[] { BankArt.MakeRoots(side).ToTexture() }, BankArt.Height, rows, pixel, speed, origin)
            { ZIndex = 30 }; // over the boat and the boss: they pass under the trees
            AddChild(roots);
            _strips.Add(roots);
            _banks.Add((roots, x, side == BankArt.Side.Left ? -1 : 1));

            // Branches hang closer to the camera, so they move a little faster.
            var branches = new ScrollingStrip(new Texture2D[] { BankArt.MakeBranches(side).ToTexture() }, BankArt.Height, rows, pixel, speed * 1.35, origin)
            { ZIndex = 32 };
            AddChild(branches);
            _strips.Add(branches);
            _banks.Add((branches, x, side == BankArt.Side.Left ? -1 : 1));
        }

        // The real sky over the open water (above the horizon), a haze and a thin bright line on the horizon.
        _horizonSky = new Sprite2D
        {
            Texture = horizonTexture, Centered = false, Scale = new Vector2(pixel, pixel), ZIndex = -95, Visible = false,
            TextureFilter = TextureFilterEnum.Nearest,
        };
        AddChild(_horizonSky);
        var hazeCanvas = new PixelCanvas(1, 24);
        for (int y = 0; y < 24; y++) hazeCanvas[0, y] = PixelColor.Hex(0x86B4CC, (byte)(150 * System.Math.Pow(1 - y / 24f, 1.6)));
        _haze = new Sprite2D
        {
            Texture = hazeCanvas.ToTexture(), Centered = false, Scale = new Vector2(size.X, 5), ZIndex = -94, Visible = false,
            TextureFilter = TextureFilterEnum.Linear,
        };
        AddChild(_haze);
        _horizonLine = new ColorRect { Color = new Color(0.72f, 0.88f, 0.95f, 0.55f), Size = new Vector2(size.X, 2), ZIndex = -93, Visible = false };
        AddChild(_horizonLine);
    }

    /// <summary>Opens the river into the sea: the banks slide out, and the horizon comes down from above the screen with the moon on it.</summary>
    public void BeginOpenWater(float seconds = 3f)
    {
        _openTime = 0;
        _openDuration = seconds;
    }

    /// <summary>Back to the river at once.</summary>
    public void CloseWater()
    {
        _openTime = -1;
        _open = 0;
        ApplyOpen();
    }

    private void ApplyOpen()
    {
        float e = _open;
        bool open = e > 0.001f;
        float span = BankArt.Width * 4 + 80;
        foreach (var (node, baseX, side) in _banks) node.Position = new Vector2(baseX + side * e * span, node.Position.Y);
        _sky.Modulate = new Color(1, 1, 1, 1 - e);
        foreach (var c in _crownNodes) c.Modulate = new Color(1, 1, 1, 1 - e);
        _horizonSky.Visible = _haze.Visible = _horizonLine.Visible = _horizonReflection.Visible = _deepReflection.Visible = open;
        if (!open)
        {
            _water.SetShaderParameter("moon", MoonGridPosition);
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
        _horizonReflection.Position = new Vector2(0, horizon + band * 2);
        _horizonReflection.Modulate = new Color(1, 1, 1, e);
        _deepReflection.Position = new Vector2(0, horizon + band * 2);
        _deepReflection.Modulate = new Color(1, 1, 1, e);
        // the water's glitter path starts at the moon, on the horizon
        _water.SetShaderParameter("moon", new Vector2(HorizonSkyArt.Moon.x, horizon / 4f));
    }

    public void Update(float dt)
    {
        if (_openTime >= 0)
        {
            _openTime += dt;
            float u = Mathf.Clamp((float)(_openTime / _openDuration), 0, 1);
            _open = u * u * (3 - 2 * u);
            ApplyOpen();
            if (u >= 1) _openTime = -1;
        }
        foreach (var strip in _strips) strip.Advance(dt);
        _rippleClock += dt;
        foreach (var ripple in _ripples)
        {
            ripple.Variant = (int)(_rippleClock / 0.5) % 2;
            ripple.Advance(dt);
        }
        _time += dt;
        _water.SetShaderParameter("time", (float)_time);
    }
}
