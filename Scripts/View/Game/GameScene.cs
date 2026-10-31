using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Renders the <see cref="World"/> and feeds it input. Game logic lives in World and the systems.
///
/// The scene is the river (the 600-wide game field plus half a bank of trees on each side) and the HUD panel.
/// Game space and Godot's screen space both have Y pointing down; <see cref="Point"/> adds the field inset.
/// Trees are drawn over the boat and the boss, so they pass under the branches; bullets stay on top to be readable.
/// ZIndex: water -100 ... boat/boss 20, roots 30, branches 32, wind 35, bullets 40, joystick 50, HUD 100, picker 150.
///
/// Test flags (user args): --autoplay (hidden test pilot: shoots, weaves, picks card 0, bombs now and then).
/// </summary>
public partial class GameScene : Node2D
{
    private const double TickRate = 1.0 / 60.0;

    public readonly World World;
    /// <summary>In boss practice, the attack (1...6) being rehearsed; scores are not recorded.</summary>
    public readonly int? PracticePhase;
    /// <summary>Raised when the player chooses to go back to the menu.</summary>
    public event Action Exit;
    private readonly InputController _input = new();
    public bool Autoplay;

    private double _accumulator;
    private PlayerInputComponent _sampledInput;
    private double _clock;      // time since the scene started

    public RiverBackground River { get; private set; }
    private readonly WakeEffect _wake = new();
    private readonly BurstEffect _burst = new();
    private readonly List<Sprite2D> _enemyNodes = new();
    /// <summary>What the water shows of each spirit: a flipped, tinted copy under it.</summary>
    private readonly List<Sprite2D> _enemyMirrors = new();
    private readonly Dictionary<SpiritKind, Texture2D[]> _spiritFrames = new();
    private float _animationClock;
    /// <summary>Seconds since the last frame, for the effects that are drawn from SyncNodes.</summary>
    private float _bombDelta;
    private readonly List<Sprite2D> _lightNodes = new();
    private Texture2D[] _lightFrames, _bombFrames, _lifeFrames;
    private readonly WindEffect _windEffect = new(new Vector2(Layout.RiverWidth, Layout.Height));
    private (Vector2 direction, float strength) _lastWind;

    // What the water reflects of the actors: flipped copies placed under them.
    private readonly Node2D _boatMirror = new(), _bossMirror = new();
    private readonly Sprite2D _boatMirrorSprite = new(), _boatMirrorAura = new(), _bossMirrorSprite = new(), _bossMirrorGlow = new();
    private readonly Node2D _playerNode = new();
    private readonly Sprite2D _playerSprite = new();
    private Sprite2D _playerAura;
    private Texture2D[] _playerFrames, _auraFrames;
    private float _auraTimer; private int _auraFrame; private float _auraWait;
    private readonly Node2D _bossNode = new();
    private readonly Sprite2D _bossSprite = new();
    private readonly Sprite2D _bossGlow = new();
    private readonly SerpentFlame _bossFlame = new();
    private readonly Dictionary<BossLook, Texture2D[]> _flameFrames = new();
    private BossForm? _bossForm;

    private enum BossLook { Woman, Serpent, Charging }
    private readonly Dictionary<BossLook, Texture2D[]> _bossFrames = new();
    private BossLook? _bossLook;
    private float _bossAnim;
    private float _glowTimer; private int _glowFrame; private float _glowWait;
    private float _bossFlash = -1; // seconds into the transformation flash, or <0
    private readonly SerpentBody _serpentBody = new();
    private readonly Dictionary<BossForm, Texture2D[]> _bossGlowFrames = new();
    private readonly BombEffect _bombEffect = new();
    private readonly List<Sprite2D> _bulletNodes = new();
    private readonly Dictionary<BulletStyle, Texture2D[]> _bulletFrames = new();
    private readonly Dictionary<BulletStyle, Vector2> _bulletScale = new();

    private readonly GameHud _hud = new(HudTheme.ForStage(1));   // each stage has its own HUD, after its boss
    private readonly PauseMenu _pause = new();
    private readonly DialogueBox _dialogue = new();
    private readonly TitleCard _title = new();
    private HashSet<Key> _dialogueHeld = new();
    private readonly CardPicker _picker = new(new Vector2(Layout.PanelX, Layout.Height));
    private readonly Sprite2D _wardRing = new();
    private readonly Sprite2D _focusRing = new(), _focusCore = new();
    private float _focusAlpha;
    private readonly Texture2D[] _wardFrames = { CardArt.WardRing(0).ToTexture(), CardArt.WardRing(1).ToTexture() };
    private HashSet<Key> _heldKeys = new();
    /// <summary>Seconds left before the picker accepts input (so a key held in the heat of the fight does not take a card).</summary>
    private float _pickerLock;
    private Disc _joystickBase, _joystickKnob;

    private int _lastLives = 3, _lastBombs = 3, _lastBossLives = 6;
    private bool _exited;
    private readonly RandomNumberGenerator _rng = new();

    private static readonly Color MirrorTint = new Color(0.12f, 0.32f, 0.42f);

    private const string TintShader = @"
shader_type canvas_item;
uniform vec3 target = vec3(1.0);
uniform float amount = 0.0;
void fragment() { COLOR.rgb = mix(COLOR.rgb, target, amount); }
";
    private static Shader _tintShader;

    public readonly Difficulty Difficulty;

    public GameScene(int? practicePhase = null, Difficulty difficulty = Difficulty.Normal)
    {
        PracticePhase = practicePhase;
        Difficulty = difficulty;
        World = new World(practicePhase, difficulty: difficulty);
        _rng.Randomize();
    }

    private static int Z(float z) => (int)Math.Round(z * 10);

    /// <summary>A shader material that blends the sprite's colour towards `target` by `amount`.</summary>
    private static ShaderMaterial NewTintMaterial(Color target, float amount)
    {
        _tintShader ??= new Shader { Code = TintShader };
        var m = new ShaderMaterial { Shader = _tintShader };
        m.SetShaderParameter("target", new Vector3(target.R, target.G, target.B));
        m.SetShaderParameter("amount", amount);
        return m;
    }

    private static void SetBlend(Sprite2D s, float amount) => ((ShaderMaterial)s.Material).SetShaderParameter("amount", amount);

    private static CanvasItemMaterial AddMaterial() => new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };

    /// <summary>Sizes a centred sprite to `size` screen px, whatever its texture.</summary>
    private static void FitTo(Sprite2D s, Vector2 size)
    {
        if (s.Texture == null) return;
        s.Scale = size / s.Texture.GetSize();
    }

    /// <summary>Screen position of a game point.</summary>
    private static Vector2 Point(Vector2 p) => Layout.ScenePoint(p);

    // MARK: Setup

    public override void _Ready()
    {
        _lightFrames = LightArt.MakeFrames();
        _bombFrames = LightArt.MakeBombFrames();
        _lifeFrames = LightArt.MakeLifeFrames();
        BuildWorldNodes();
        AddChild(_hud);
        AddChild(_picker);
        AddChild(_dialogue);
        AddChild(_title);
        AddChild(_pause);
        _pause.Continue += Resume;
        _pause.GiveUp += DoExit;
        BuildJoystick();
        SyncNodes();
        StartRun();
        UpdateHUD(0);
    }

    /// <summary>Airu's opening line, then the stage's opening.</summary>
    private void StartRun()
    {
        _lastLives = World.Player.Health.Lives;
        _lastBombs = World.Player.Bomb.BombsLeft;
        _lastBossLives = World.Boss.Health.Lives;
        _hud.HideEnd();
        _title.Hide2();
        _dialogue.Cancel();
        _windEffect.Reset();
        _lastWind = (World.Wind.Direction, World.Wind.Strength);
        _windEffect.SetWind(World.Wind.Direction, World.Wind.Strength);
        _windEffect.Announce();
        _hud.PulseWind();
        _hud.Say(UIText.SayStart);
        River.CloseWater();
        _pendingAttackName = null;
        _bossSequence = BossSequence.None;
        _morphHoldWoman = false;
        _bossShake = Vector2.Zero;
        _endSequence = EndSequence.None;
        _dialoguePending = false;
        // The boat waits below the screen.
        World.Player.Transform.Position = new Vector2(CombatSystem.PlayerSpawn.X, OffscreenY);
        _stageIntroPending = true;
        _rising = false;
        MusicPlayer.Instance?.Stop(0.3f);
    }

    // MARK: Game loop

    public override void _Process(double delta)
    {
        if (_exited) return;
        if (_pause.IsShown) return; // the game stands still while paused
        double dt = Math.Min(delta, 0.1);
        _clock += delta;
        float fdt = (float)dt;

        River.Update(fdt);
        _wake.Update(fdt, _playerNode.Position, World.Result != GameResult.Defeat);

        // Fixed 60 Hz simulation, like Godot's physics step; clamp to avoid a spiral after a stall.
        if (_dialogue.IsActive) UpdateDialogueKeys();
        bool frozen = UpdateSequences(fdt) || _dialogue.IsOnScreen;
        if (_pendingAttackName != null && !frozen)
        {
            _attackNameDelay -= fdt;
            if (_attackNameDelay <= 0) { _hud.ShowAttackName(_pendingAttackName); _pendingAttackName = null; }
        }
        _accumulator += Math.Min(delta, 0.25);
        _sampledInput = Autoplay ? AutoInput() : _input.Sample();
        while (_accumulator >= TickRate)
        {
            _accumulator -= TickRate;
            if (!frozen) World.Step((float)TickRate, _sampledInput);
        }
        if (!frozen) UpdatePicker(fdt);

        if (World.Result is GameResult result && !_hud.IsEndVisible)
        {
            if (result == GameResult.Victory && PracticePhase == null && _endSequence != EndSequence.Done)
            {
                // The boss is beaten: a last talk before the end screen.
                if (_endSequence == EndSequence.None)
                {
                    _endSequence = EndSequence.Waiting;
                    _sequenceClock = 0;
                    _hud.HideAttackName();
                    MusicPlayer.Instance?.Stop(1.5f);
                }
            }
            else Finish(result);
        }
        if (World.Result != null && _hud.IsEndVisible)
        {
            if (KeyDown(Key.Enter) || KeyDown(Key.KpEnter)) Restart();
            else if (KeyDown(Key.Escape)) { SfxPlayer.Instance?.Play(SfxBank.Sound.MenuBack); DoExit(); return; }
        }

        _bombDelta = fdt;
        SyncNodes();
        _animationClock += fdt;
        UpdateAnimations(fdt);
        SyncEnemies();
        SyncLights();
        _burst.Update(fdt);
        HandleWorldEvents();
        var boss = World.Boss;
        _serpentBody.Update(fdt, _bossNode.Position, boss.MovementType == BossMovementType.Dash && World.Result == null, boss.DashTimer >= 1.5f);
        _windEffect.SetWind(World.Wind.Direction, World.Wind.Strength);
        _windEffect.Update(fdt);
        TrackEvents();
        UpdateHUD(fdt);
    }


    // MARK: Pause

    private void Pause()
    {
        _input.EndAll();
        _pause.Show();
    }

    private void Resume()
    {
        _pause.Hide();
        _accumulator = 0;
        _input.EndAll();
        // A key still held from the pause menu must not take a card.
        if (_picker.IsShown) _pickerLock = 0.3f;
    }

    private void DoExit()
    {
        if (_exited) return;
        _exited = true;
        MusicPlayer.Instance?.Stop();
        _input.EndAll();
        Exit?.Invoke();
    }

    private void Restart()
    {
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuSelect);
        World.Reset();
        _picker.Hide();
        _wake.Reset();
        _burst.Reset();
        _serpentBody.Reset();
        _accumulator = 0;
        _input.EndAll();
        StartRun();
    }
}
