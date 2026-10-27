using System;
using Godot;

/// <summary>
/// The bomb as the player sees it: the lantern flares, a shockwave of light rolls out, the light blooms around the
/// boat and rays turn in it, then it dims away as the bomb runs out. Everything is additive and lit from the lantern.
///
/// API
///   new BombEffect()
///   Update(float dt, bool active, float timeLeft, Vector2 centre)  // timeLeft = how long the bomb still burns; centre = the lantern, screen px (Y down)
///   bool IsShowing, float BloomAlpha
///   const FlashDuration, GrowDuration, RingDuration, FadeDuration
/// THIS NODE MUST BE AT THE SCENE ORIGIN (0,0): the full-screen flash (Layout.SceneSize) is drawn from the node's origin.
/// Children ZIndex (relative): flash -10, bloom 0, rays 1, ring 2. Give the BombEffect a high ZIndex itself so it is over the actors.
/// </summary>
public partial class BombEffect : Node2D
{
    public const float FlashDuration = 0.3f;
    public const float GrowDuration = 0.5f;
    public const float RingDuration = 0.45f;
    public const float FadeDuration = 0.8f;

    private readonly Sprite2D _flash = new Sprite2D();
    private readonly Sprite2D _bloom = new Sprite2D();
    private readonly Sprite2D _rays = new Sprite2D();
    private readonly Sprite2D _ring = new Sprite2D();

    private readonly Texture2D[] _bloomGrow;
    private readonly Texture2D[] _bloomSteady;
    private readonly Texture2D[] _rayTextures;
    private readonly Texture2D[] _ringTextures;

    private float _clock;
    private bool _wasActive;

    /// <summary>Whether any part of the effect is showing.</summary>
    public bool IsShowing => _bloom.Visible || _flash.Visible || _ring.Visible;
    /// <summary>How bright the bloom is right now (0 when it is not showing).</summary>
    public float BloomAlpha => _bloom.Visible ? _bloom.Modulate.A : 0;

    public BombEffect()
    {
        _bloomGrow = new Texture2D[BombArt.Growth.Length];
        for (int i = 0; i < _bloomGrow.Length; i++) _bloomGrow[i] = BombArt.MakeBloom(BombArt.Growth[i]).ToTexture();
        _bloomSteady = new Texture2D[BombArt.Steady.Length];
        for (int i = 0; i < _bloomSteady.Length; i++) _bloomSteady[i] = BombArt.MakeBloom(BombArt.Steady[i]).ToTexture();
        _rayTextures = new Texture2D[BombArt.RayFrames];
        for (int i = 0; i < _rayTextures.Length; i++) _rayTextures[i] = BombArt.MakeRays(i).ToTexture();
        _ringTextures = new Texture2D[BombArt.RingFrames];
        for (int i = 0; i < _ringTextures.Length; i++) _ringTextures[i] = BombArt.MakeRing(i).ToTexture();

        float side = BombArt.Size * BombArt.Scale;
        foreach (var node in new[] { _bloom, _rays, _ring })
        {
            node.Scale = new Vector2(BombArt.Scale, BombArt.Scale);
            node.Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
            node.Visible = false;
            AddChild(node);
        }
        _rays.ZIndex = 1;
        _ring.ZIndex = 2;

        var size = Layout.SceneSize;
        _flash.Texture = new PixelCanvas(1, 1, new PixelColor(255, 214, 128, 255)).ToTexture();
        _flash.Centered = false;
        _flash.Scale = size;
        _flash.Material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        _flash.ZIndex = -10;
        _flash.Visible = false;
        AddChild(_flash);
        _ = side;
    }

    public void Update(float dt, bool active, float timeLeft, Vector2 centre)
    {
        if (dt < 0) return;
        if (active && !_wasActive) _clock = 0;
        _wasActive = active;
        if (!active)
        {
            foreach (var node in new[] { _bloom, _rays, _ring, _flash }) node.Visible = false;
            return;
        }
        _clock += dt;
        float t = _clock;
        var snapped = new Vector2(MathF.Round(centre.X / BombArt.Scale) * BombArt.Scale, MathF.Round(centre.Y / BombArt.Scale) * BombArt.Scale);
        foreach (var node in new[] { _bloom, _rays, _ring }) node.Position = snapped;

        // The flash that lights the whole picture for an instant (the flash sits at the scene's corner, not the lantern).
        if (t < FlashDuration)
        {
            _flash.Visible = true;
            _flash.Position = Vector2.Zero;
            _flash.Modulate = new Color(1, 1, 1, 0.32f * (1 - t / FlashDuration));
        }
        else
        {
            _flash.Visible = false;
        }

        // The shockwave.
        if (t < RingDuration)
        {
            _ring.Visible = true;
            _ring.Texture = _ringTextures[Math.Min(BombArt.RingFrames - 1, (int)(t / RingDuration * BombArt.RingFrames))];
        }
        else
        {
            _ring.Visible = false;
        }

        // The bloom swells to full reach, then pulses like a flame, then dims away.
        _bloom.Visible = true;
        if (t < GrowDuration)
            _bloom.Texture = _bloomGrow[Math.Min(_bloomGrow.Length - 1, (int)(t / GrowDuration * _bloomGrow.Length))];
        else
            _bloom.Texture = _bloomSteady[(int)(t * 9) % _bloomSteady.Length];
        float fade = Math.Min(1, Math.Max(0, timeLeft / FadeDuration));
        _bloom.Modulate = new Color(1, 1, 1, (0.88f + 0.12f * MathF.Sin(t * 14)) * fade);

        // The rays come in with the light and turn.
        _rays.Visible = true;
        _rays.Texture = _rayTextures[(int)(t * 11) % _rayTextures.Length];
        _rays.Modulate = new Color(1, 1, 1, Math.Min(1, t / 0.25f) * fade * 0.85f);
    }
}
