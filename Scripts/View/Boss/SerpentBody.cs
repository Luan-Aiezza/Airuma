using System;
using Godot;

/// <summary>
/// The serpent's body during its charge, made of just two sprites of one long image.
///
/// One sprite is fixed to the head and trails behind it (the body extends UP the screen from the head position).
/// When the head wraps around and reappears at the top, that sprite goes with it, and the other takes over the old
/// pose: the body stays lying on the water where the serpent passed, drifting with the current, until the next
/// charge begins and it dissolves. Scenery only: it does not hurt the player.
///
/// API
///   new SerpentBody()
///   Update(float dt, Vector2 head, bool active, bool lunging)   // head = serpent's centre, screen px (Y down); active = in its charging attack; lunging = lunging right now
///   Reset()
///   Sprite2D AttachedNode, Sprite2D LeftBehindNode
///   const WrapDistance = 250, DissolveDuration = 0.4
/// Node must be at the scene origin. Put it under the boss sprite (ZIndex of this node below the boss); the left-behind
/// sprite is one step (-1) below the attached one.
/// </summary>
public partial class SerpentBody : Node2D
{
    /// <summary>A jump larger than this means the head wrapped around to the top of the screen.</summary>
    public const float WrapDistance = 250;
    public const float DissolveDuration = 0.4f;

    private class Layer
    {
        public readonly Sprite2D Node = new Sprite2D();
        public bool Visible;
        /// <summary>Seconds since it started to dissolve, or null while it is fully there.</summary>
        public float? Dissolving;
    }

    private readonly Layer _attached = new Layer();
    private readonly Layer _leftBehind = new Layer();
    private readonly Texture2D[] _frames;
    private readonly Texture2D[] _flameFrames;
    private readonly SerpentFlame[] _flames = { new SerpentFlame(), new SerpentFlame() };
    private float _clock;
    private Vector2? _lastHead;
    private bool _wasLunging;
    private float _flip = 1;
    private readonly Random _rng = new Random();

    public Sprite2D AttachedNode => _attached.Node;
    public Sprite2D LeftBehindNode => _leftBehind.Node;

    public SerpentBody()
    {
        _frames = new Texture2D[2];
        _flameFrames = new Texture2D[2];
        // The body is one image, and so are the two frames of the sprite (they only differ in the flames, which the shader makes).
        var art = SerpentBossArt.MakeBody();
        var texture = art.ToTexture();
        var flameTexture = SerpentBossArt.Padded(art, continueBottom: true).ToTexture();
        for (int i = 0; i < 2; i++) { _frames[i] = texture; _flameFrames[i] = flameTexture; }
        int n = 0;
        foreach (var node in new[] { _attached.Node, _leftBehind.Node })
        {
            // The head end is at the bottom of the image and sits on the node's position.
            node.Centered = false;
            node.Offset = new Vector2(-SerpentBossArt.BodyWidth / 2f, -SerpentBossArt.BodyLength);
            node.Scale = new Vector2(BossArt.SerpentScale, BossArt.SerpentScale);
            node.Texture = _frames[0];
            node.Visible = false;
            var flame = _flames[n++];
            flame.Texture = _flameFrames[0];
            flame.Align(node.Offset);
            node.AddChild(flame);
            AddChild(node);
        }
        _leftBehind.Node.ZIndex = -1;
    }

    public void Reset()
    {
        _attached.Visible = false;
        _leftBehind.Visible = false;
        _attached.Dissolving = null;
        _leftBehind.Dissolving = null;
        _attached.Node.Visible = false;
        _leftBehind.Node.Visible = false;
        _lastHead = null;
        _wasLunging = false;
    }

    public void Update(float dt, Vector2 head, bool active, bool lunging)
    {
        _clock += dt;
        var texture = _frames[(int)(_clock / 0.2f) % _frames.Length];
        _attached.Node.Texture = texture;
        _leftBehind.Node.Texture = texture;
        var flameTexture = _flameFrames[(int)(_clock / 0.2f) % _frames.Length];
        foreach (var flame in _flames) flame.Texture = flameTexture;

        if (active)
        {
            if (_lastHead is Vector2 last && last.DistanceTo(head) > WrapDistance) HandOverToLeftBehind();
            if (lunging && !_wasLunging) _leftBehind.Dissolving ??= 0; // the next charge: the old body goes
            _wasLunging = lunging;
            _lastHead = head;

            if (!_attached.Visible)
            {
                _attached.Visible = true;
                _attached.Dissolving = null;
                _flip = _rng.Next(2) == 0 ? 1 : -1;
            }
            SetFlip(_attached.Node, _flip);
            _attached.Node.Position = Snapped(head);
        }
        else
        {
            if (_attached.Visible && _attached.Dissolving == null) _attached.Dissolving = 0;
            if (_leftBehind.Visible && _leftBehind.Dissolving == null) _leftBehind.Dissolving = 0;
            _lastHead = null;
            _wasLunging = false;
        }

        // Lying on the water, the body that was left behind drifts downstream with the current.
        if (_leftBehind.Visible) _leftBehind.Node.Position += new Vector2(0, WakeEffect.CurrentSpeed * dt);
        Step(_attached, dt);
        Step(_leftBehind, dt);
    }

    private static void SetFlip(Sprite2D node, float flip) => node.Scale = new Vector2(flip * BossArt.SerpentScale, BossArt.SerpentScale);

    private void HandOverToLeftBehind()
    {
        _leftBehind.Node.Position = _attached.Node.Position;
        _leftBehind.Node.Scale = _attached.Node.Scale;
        _leftBehind.Visible = true;
        _leftBehind.Dissolving = null;
        _flip = -_flip; // the next lane's body curves the other way
        SetFlip(_attached.Node, _flip);
    }

    private static void Step(Layer layer, float dt)
    {
        if (!layer.Visible)
        {
            layer.Node.Visible = false;
            return;
        }
        if (layer.Dissolving is float d)
        {
            float next = d + dt;
            layer.Dissolving = next;
            if (next >= DissolveDuration)
            {
                layer.Visible = false;
                layer.Dissolving = null;
                layer.Node.Visible = false;
                return;
            }
            layer.Node.Modulate = new Color(1, 1, 1, 1 - next / DissolveDuration);
        }
        else
        {
            layer.Node.Modulate = new Color(1, 1, 1, 1);
        }
        layer.Node.Visible = true;
    }

    private static Vector2 Snapped(Vector2 p) =>
        new Vector2(MathF.Round(p.X / BossArt.SerpentScale) * BossArt.SerpentScale, MathF.Round(p.Y / BossArt.SerpentScale) * BossArt.SerpentScale);
}
