using System;
using Godot;
using static ViewKit;

/// <summary>What the boss's view needs to know each frame.</summary>
public struct BossFrame
{
    public float Dt;
    public BossEntity Boss;
    public BossPhase Phase;
    /// <summary>The boss part of the stage is on (the boss is on the field).</summary>
    public bool Fighting;
    /// <summary>The run is over (won or lost).</summary>
    public bool Over;
    /// <summary>A scripted moment (the boss coming down, a talk) is going on: the boss does not spin or show its rage.</summary>
    public bool Scripted;
    /// <summary>The boss that is two has fused into one: its fused style is shown (if it has one).</summary>
    public bool Fused;
    /// <summary>This body is not to be seen (it has fused into the other).</summary>
    public bool Hidden;
}

/// <summary>
/// Everything that is seen of the boss: the sprite with its glow and its reflection in the water, and what goes with the way it moves and fights, each
/// as a small component that shows itself when the boss does that: the spin of a dancer, the rage, the dive (its ring and the shadow under the boat),
/// and the great jet. It is given a <see cref="BossStyle"/> (the art and the colours), and knows nothing of any particular boss.
/// </summary>
public partial class BossView : Node2D
{
    private BossStyle _style;
    private readonly BossStyle _baseStyle;
    private bool _showingFused;
    private Texture2D[] _fusedFrames, _fusedGlowFrames, _baseFrames, _baseGlowFrames;
    private readonly Node2D _body = new();
    private readonly Sprite2D _sprite = new() { Centered = false };
    private readonly Sprite2D _glow = new();
    private Texture2D[] _frames, _glowFrames;
    private float _anim, _glowTimer, _glowWait = 0.3f, _flash = -1;
    private int _glowFrame;

    // The reflection.
    private readonly Node2D _mirror = new();
    private readonly Sprite2D _mirrorSprite = new() { Centered = false }, _mirrorGlow = new();
    private readonly Node _mirrorParent;

    // The parts that show themselves when the boss does what they are for.
    private readonly SpinEffect _spinEffect = new();
    private readonly EnrageEffect _enrage = new();
    private readonly DiveVisuals _dive = new();
    private readonly WaterJet _jet = new() { Visible = false };
    private float _spin, _enragedFor, _rageClock;
    private DiveState _lastDive;
    private SweepState _lastSweep;
    private readonly RandomNumberGenerator _rng = new();

    /// <summary>The seconds of the boss's flash when something changes about it (it appears, it grows angry).</summary>
    private const float FlashSeconds = 0.6f;
    private const float SpinSpeed = MathF.Tau * 1.5f;

    /// <summary>How far the rage has taken the scenery (0...1): the river's tint follows it.</summary>
    public float RageAmount => _enragedFor;
    public BossStyle Style => _style;

    /// <summary>`mirrorParent` is the layer of the water that mirrors what is above it.</summary>
    public BossView(BossStyle style, Node mirrorParent)
    {
        _style = style;
        _baseStyle = style;
        _mirrorParent = mirrorParent;
        _rng.Randomize();
        _frames = style.MakeFrames();
        _glowFrames = style.MakeGlowFrames();
        _baseFrames = _frames; _baseGlowFrames = _glowFrames;
        if (style.Fused != null) { _fusedFrames = style.Fused.MakeFrames(); _fusedGlowFrames = style.Fused.MakeGlowFrames(); }
        ZIndex = 0;

        _body.ZIndex = Z(2);                    // sprites draw above bullets, as in the original
        _glow.Material = AddMaterial();
        _glow.ZIndex = Z(-0.2f);
        _body.AddChild(_glow);
        _sprite.Material = NewTintMaterial(Colors.White, 0);
        _sprite.Scale = new Vector2(style.Scale, style.Scale);
        _sprite.Offset = -_frames[0].GetSize() * style.Anchor;
        _sprite.Texture = _frames[0];
        _body.AddChild(_sprite);
        float glowSide = style.GlowSize * style.Scale;
        _glow.Texture = _glowFrames[0];
        FitTo(_glow, new Vector2(glowSide, glowSide));

        // The reflection: mirrored about the water line.
        _mirror.Scale = new Vector2(1, -1);
        _mirror.ZIndex = 0;                     // explicit: equal z would flicker
        _mirrorSprite.Material = NewTintMaterial(MirrorTint, 0.3f);
        _mirrorSprite.Modulate = new Color(1, 1, 1, 0.6f);
        _mirrorGlow.Modulate = new Color(1, 1, 1, 0.7f);
        _mirrorGlow.Material = AddMaterial();
        _mirrorGlow.ZIndex = Z(-0.2f);
        _mirror.AddChild(_mirrorSprite);
        _mirror.AddChild(_mirrorGlow);

        _jet.ZIndex = Z(1.6f);
        AddChild(_body);
        AddChild(_dive);
        AddChild(_jet);
        AddChild(_spinEffect);
        AddChild(_enrage);
    }

    /// <summary>The boss flashes white and fades in: it has just appeared, or has just grown angry.</summary>
    public void Flash()
    {
        _flash = 0;
        _body.Modulate = new Color(1, 1, 1, 0.2f);
        SetBlend(_sprite, 1);
    }

    /// <summary>The rings of the rage begin (the boss's first enraged attack).</summary>
    public void BurstRage() => _enrage.Burst();

    /// <summary>Back to the start (the run begins again).</summary>
    public void Reset()
    {
        if (_showingFused && _baseStyle.Fused != null) ShowFused(false);
        _enrage.Reset();
        _enragedFor = 0;
        _rageClock = 0;
        _spin = 0;
        _flash = -1;
        _body.Modulate = Colors.White;
        SetBlend(_sprite, 0);
    }

    /// <summary>Takes the look of the fused boss, or goes back to the look of the one: its sprite, its size and its glow, with a flash and a roar.</summary>
    private void ShowFused(bool fused)
    {
        _showingFused = fused;
        _style = fused ? _baseStyle.Fused : _baseStyle;
        _frames = fused ? _fusedFrames : _baseFrames;
        _glowFrames = fused ? _fusedGlowFrames : _baseGlowFrames;
        _sprite.Scale = new Vector2(_style.Scale, _style.Scale);
        _sprite.Offset = -_frames[0].GetSize() * _style.Anchor;
        _glow.Texture = _glowFrames[0];
        float glowSide = _style.GlowSize * _style.Scale;
        FitTo(_glow, new Vector2(glowSide, glowSide));
        if (fused) { Flash(); SfxPlayer.Instance?.Play(SfxBank.Sound.Rage); }
    }

    public void Update(in BossFrame f)
    {
        var boss = f.Boss;
        if (_baseStyle.Fused != null && f.Fused != _showingFused) ShowFused(f.Fused);
        _body.Position = Point(boss.Transform.Position);

        // The sprite and its glow.
        _anim += f.Dt;
        _sprite.Texture = _frames[(int)(_anim / 0.18f) % _frames.Length];
        _glowTimer += f.Dt;
        while (_glowTimer >= _glowWait)
        {
            _glowTimer -= _glowWait;
            _glowFrame = 1 - _glowFrame;
            _glowWait = _glowFrame == 1 ? Wait(0.14f, 0.1f) : Wait(0.3f, 0.2f);
            _glow.Texture = _glowFrames[_glowFrame];
        }
        if (_flash >= 0)
        {
            _flash += f.Dt;
            float t = Math.Min(1, _flash / FlashSeconds);
            _body.Modulate = new Color(1, 1, 1, 0.2f + 0.8f * t);
            SetBlend(_sprite, 1 - t);
            if (t >= 1) _flash = -1;
        }

        UpdateSpin(f);
        UpdateRage(f);
        UpdateDive(f);
        UpdateJet(f);
        if (f.Hidden) _body.Visible = false;
        UpdateMirror(f);
    }

    /// <summary>A time to wait with some randomness: duration, give or take half of range.</summary>
    private float Wait(float duration, float range) => duration + _rng.RandfRange(-range / 2, range / 2);

    // MARK: The spin

    /// <summary>A dancer turns on the spot while he roams (the sprite is squeezed to an edge and comes back flipped, like a body turning), and finishes the
    /// turn he is in when he stops, so that he always rests facing us.</summary>
    private void UpdateSpin(in BossFrame f)
    {
        if (!_style.Spins) return;
        var boss = f.Boss;
        bool moving = f.Fighting && !f.Scripted && boss.MovementType == BossMovementType.Roam && boss.Roam.HasTarget && boss.Roam.Wait <= 0;
        float before = _spin;
        if (moving)
        {
            _spin += SpinSpeed * f.Dt;
            // The whoosh at the start of every turn.
            if (before == 0 || (int)(_spin / MathF.Tau) > (int)(before / MathF.Tau)) SfxPlayer.Instance?.Play(SfxBank.Sound.Twirl);
        }
        else if (_spin > 0)
        {
            float next = MathF.Ceiling(_spin / MathF.Tau) * MathF.Tau;
            _spin = MathF.Min(next, _spin + SpinSpeed * f.Dt);
            if (_spin >= next) _spin = 0;
        }
        _spin %= MathF.Tau * 1000;
        float width = MathF.Cos(_spin);
        float flip = (width < 0 ? -1 : 1) * MathF.Max(0.1f, MathF.Abs(width));
        _sprite.Scale = new Vector2(_style.Scale * flip, _style.Scale);
        _spinEffect.Update(f.Dt, _spin > 0, _body.Position, _sprite, boss.Hitbox.Radius);
    }

    // MARK: The rage

    /// <summary>An enraged boss: a glow behind it, sparks and a deep beat, while the scenery turns a little more each moment the rage lasts.</summary>
    private void UpdateRage(in BossFrame f)
    {
        var boss = f.Boss;
        bool enraged = f.Fighting && !f.Over && boss.Health.Lives > 0 && f.Phase.Enraged;
        _enragedFor = Mathf.MoveToward(_enragedFor, enraged ? 1 : 0, f.Dt * 0.7f);
        _enrage.Update(f.Dt, enraged && _body.Visible, _body.Position, boss.Hitbox.Radius, _style.RageMain, _style.RageAccent);
        if (!enraged) return;
        _rageClock -= f.Dt;
        if (_rageClock <= 0) { _rageClock = 1.3f; SfxPlayer.Instance?.Play(SfxBank.Sound.RagePulse); }
    }

    // MARK: The dive and the jet

    private void UpdateDive(in BossFrame f)
    {
        var boss = f.Boss;
        bool diving = boss.MovementType == BossMovementType.Dive && f.Fighting;
        _dive.Update(f.Dt, boss, diving);
        _body.Visible = !(diving && boss.Dive.Submerged);
        _body.Scale = Vector2.One * _dive.BodySize(boss, diving);
        if (diving && boss.Dive.State != _lastDive && (boss.Dive.State == DiveState.Sinking || boss.Dive.State == DiveState.Rising)) SfxPlayer.Instance?.Play(SfxBank.Sound.Splash);
        _lastDive = boss.Dive.State;
    }

    private void UpdateJet(in BossFrame f)
    {
        var boss = f.Boss;
        bool sweeping = boss.MovementType == BossMovementType.Sweep && f.Fighting;
        var mouth = Point(boss.Transform.Position + new Vector2(0, boss.Hitbox.Radius * 0.9f));
        _jet.Update(f.Dt, sweeping && boss.Sweep.BeamWarn, sweeping && boss.Sweep.BeamOn, mouth, Layout.Height, SweepBrain.BeamHalfWidth);
        if (sweeping && boss.Sweep.State != _lastSweep && (boss.Sweep.State == SweepState.Pass || boss.Sweep.State == SweepState.Pass2)) SfxPlayer.Instance?.Play(SfxBank.Sound.Jet);
        _lastSweep = boss.Sweep.State;
    }

    // MARK: The reflection

    /// <summary>The boss is mirrored about the water line at its feet, as long as it is on the field.</summary>
    private void UpdateMirror(in BossFrame f)
    {
        Attach(_mirror, _mirrorParent, f.Fighting && f.Boss.Health.Lives > 0 && _style.Reflects && !f.Hidden && CanReflect(_body.Position));
        _mirror.Position = new Vector2(_body.Position.X, _body.Position.Y + 2 * _style.Waterline);
        _mirrorSprite.Texture = _sprite.Texture;
        _mirrorSprite.Scale = _sprite.Scale;
        _mirrorSprite.Offset = _sprite.Offset;
        _mirrorSprite.Modulate = new Color(1, 1, 1, 0.6f * _body.Modulate.A);
        _mirrorGlow.Texture = _glow.Texture;
        _mirrorGlow.Scale = _glow.Scale;
    }
}
