using System;
using System.Collections.Generic;
using Godot;
using static ViewKit;

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
    /// <summary>The demonstration shown when the menu is left alone: a pilot plays a stage by itself, and any key, click, touch or move of the mouse goes back to the menu.</summary>
    public bool Demo;
    private DemoPilot _demoPilot;
    private PixelLabel _demoLabel;
    private double _demoClock;
    /// <summary>The demonstration ends after this many seconds if it has not ended by itself.</summary>
    private const double DemoSeconds = 80;
    private bool Scripted => Autoplay || Demo;

    private double _accumulator;
    private PlayerInputComponent _sampledInput;
    private double _clock;      // time since the scene started

    public RiverBackground River { get; private set; }
    private readonly WakeEffect _wake = new();
    private BurstEffect _burst;
    private readonly ConstellationLines _constellations = new();
    private readonly SlashEffect _slashes = new();
    private readonly MoonEffect _moon = new();
    private readonly List<Sprite2D> _enemyNodes = new();
    /// <summary>What the water shows of each spirit: a flipped, tinted copy under it.</summary>
    private readonly List<Sprite2D> _enemyMirrors = new();
    private readonly Dictionary<SpiritKind, Texture2D[]> _spiritFrames = new();
    private float _animationClock;
    /// <summary>Seconds since the last frame, for the effects that are drawn from SyncNodes.</summary>
    private float _bombDelta;
    private readonly List<Sprite2D> _lightNodes = new();
    private Texture2D[] _lightFrames, _bombFrames, _lifeFrames;
    private readonly Texture2D[][] _powerFrames = new Texture2D[3][];
    private readonly CanvasItemMaterial _lightAdd = ViewKit.AddMaterial();
    private readonly WindEffect _windEffect = new(new Vector2(Layout.RiverWidth, Layout.Height));
    private (Vector2 direction, float strength) _lastWind;

    // What the water reflects of the actors: flipped copies placed under them.
    private readonly Node2D _boatMirror = new();
    private readonly Sprite2D _boatMirrorSprite = new(), _boatMirrorAura = new();
    private readonly Node2D _playerNode = new();
    private readonly Sprite2D _playerSprite = new();
    private Sprite2D _playerAura;
    private Texture2D[] _playerFrames, _auraFrames;
    private float _auraTimer; private int _auraFrame; private float _auraWait;
    /// <summary>The boss: its sprite, its reflection and everything that goes with how it moves and fights.</summary>
    private BossView _boss;
    /// <summary>The second body of a boss that is two (null for the others).</summary>
    private BossView _twin;
    private readonly BombEffect _bombEffect = new();
    private readonly List<Sprite2D> _bulletNodes = new();
    private readonly Dictionary<BulletStyle, Texture2D[]> _bulletFrames = new();
    private readonly Dictionary<BulletStyle, Vector2> _bulletScale = new();
    private readonly int[] _bulletKey = new int[BulletSystem.PoolSize];
    private readonly bool[] _bulletRotates = new bool[Enum.GetValues<BulletStyle>().Length];

    private readonly GameHud _hud;   // each stage has its own HUD, after its boss
    private readonly PauseMenu _pause;
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
    private bool _afterBossOffered;
    private readonly RandomNumberGenerator _rng = new();

    public readonly Difficulty Difficulty;
    private bool _progressSaved;

    /// <summary>The stage being played.</summary>
    public readonly StageDefinition Stage;
    /// <summary>Practising a whole stage from its start: it does not go on to the next stage and is not recorded.</summary>
    public readonly bool StagePractice;
    /// <summary>Asked for when the stage is won in a normal run and another follows: the carry for it (the scene is replaced by Main).</summary>
    public event Action<RunCarry> NextStage;

    public GameScene(int? practicePhase = null, Difficulty difficulty = Difficulty.Normal, StageDefinition stage = null, RunCarry carry = null, bool stagePractice = false)
    {
        StagePractice = stagePractice;
        Stage = stage ?? Stages.One;
        _hud = new GameHud(Stage.Hud());
        _pause = new PauseMenu(Stage.Hud());
        _burst = new BurstEffect(Stage.Spirits);
        PracticePhase = practicePhase;
        Difficulty = difficulty;
        World = new World(practicePhase, difficulty: difficulty, stage: Stage, carry: carry, stagePractice: stagePractice);
        _rng.Randomize();
    }

    // MARK: Setup

    public override void _Ready()
    {
        // Everything of the game is cut at the edges of the picture of the game: the boat that reaches the left edge (in the open scenes, where the banks have gone), and the bullets
        // that fly out of the field, go no further than the edge; they no longer pass in front of the pillars that fill the sides of a wide screen (see SideFrame).
        RenderingServer.CanvasItemSetClip(GetCanvasItem(), true);
        RenderingServer.CanvasItemSetCustomRect(GetCanvasItem(), true, new Rect2(Vector2.Zero, Layout.SceneSize));
        _lightFrames = LightArt.MakeFrames();
        for (int size = 0; size < 3; size++) _powerFrames[size] = LightArt.MakePowerFrames(size);
        _bombFrames = LightArt.MakeBombFrames();
        _lifeFrames = LightArt.MakeLifeFrames();
        BuildWorldNodes();
        AddChild(_hud);
        _hud.SetBossName(Stage.Boss.Name);
        _dialogue.BossName = Stage.Boss.Name;
        _dialogue.BossStage = System.Linq.Enumerable.ToList(Stages.All).IndexOf(Stage) + 1;
        AddChild(_picker);
        AddChild(_dialogue);
        AddChild(_title);
        AddChild(_pause);
        _pause.Continue += Resume;
        _pause.GiveUp += DoExit;
        BuildJoystick();
        BuildFinale();
        if (Demo)
        {
            _demoPilot = new DemoPilot();
            _demoLabel = new PixelLabel(PixelLabel.Style.Gold, 4, PixelLabel.Align.Center) { Position = new Vector2(Layout.FieldInset + 300, 40), ZIndex = 160 };
            _demoLabel.SetText(UIText.DemoLabel);
            AddChild(_demoLabel);
        }
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
        // the songs of this stage are asked for now, so that they are being rendered long before the title and the fight need them
        MusicPlayer.Instance?.Prepare(Stage.Theme);
        foreach (var phase in Stage.Boss.Phases) MusicPlayer.Instance?.Prepare(phase.Theme);
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
        _boss.Reset();
        _twin?.Reset();
        _endSequence = EndSequence.None;
        ResetFinale();
        _afterBossOffered = false;
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
        if (Demo)
        {
            _demoClock += delta;
            // it ends when the boat is lost, the stage is won or the time is up; the label blinks
            if (World.Result != null || _demoClock > DemoSeconds) { DoExit(); return; }
            _demoLabel.Modulate = new Color(1, 1, 1, (float)_demoClock % 1.2f < 0.8f ? 1 : 0.25f);
        }
        double dt = Math.Min(delta, 0.1);
        _clock += delta;
        float fdt = (float)dt;
        UpdateVeil(fdt);

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
        _sampledInput = Demo ? _demoPilot.Sample(World, fdt) : (Autoplay ? AutoInput() : _input.Sample());
        while (_accumulator >= TickRate)
        {
            _accumulator -= TickRate;
            if (!frozen) World.Step((float)TickRate, _sampledInput);
        }
        if (!frozen) UpdatePicker(fdt);

        if (_gameOver != null) UpdateGameOver(fdt);
        if (World.Result is GameResult result && !_hud.IsEndVisible && _gameOver == null)
        {
            if (result == GameResult.Victory && !_progressSaved && PracticePhase == null && !StagePractice && !Demo) { _progressSaved = true; Progress.MarkCleared(Stages.IndexOf(Stage)); }
            if (result == GameResult.Victory && PracticePhase == null && !StagePractice && _endSequence == EndSequence.Done && Stages.After(Stage) != null && !EasyEnds)
            {
                // The stage is won; a stage that ends in a great light goes white and then dark first; the boss leaves a gift (one choice of cards), and then the river goes on.
                if (Stage.LightTransition && !_lightDone) _lightDone = RunLightTransition(fdt);
                else
                {
                    if (!_afterBossOffered) { _afterBossOffered = true; World.OfferAfterBoss(); }
                    if (!_exited && World.Draft == null && !_picker.IsShown) { _exited = true; _input.EndAll(); NextStage?.Invoke(RunCarry.From(World)); }
                }
            }
            else if (result == GameResult.Victory && _endSequence != EndSequence.Done)
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
            else if (result == GameResult.Victory && Stage.LightTransition && !_lightDone && !EasyEnds)
            {
                // Practice: the great light and the dark play too, and then the screen clears for the end screen (the next stage is not entered).
                if (RunLightTransition(fdt)) { _lightDone = true; FadeVeil(new Color(0, 0, 0, 0), 1.0f); }
            }
            else if (result == GameResult.Victory && (Stage.Epilogue != null || EasyEnds) && !_epilogueDone) RunEpilogue(fdt);
            else Finish(result);
        }
        if (World.Result != null && _hud.IsEndVisible)
        {
            if ((KeyDown(Key.Enter) || KeyDown(Key.KpEnter)) && CanRestart) Restart();
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
        if (!CanRestart) return;
        if (CountsRestarts) RunRestarts.Used++;
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuSelect);
        World.Reset();
        _picker.Hide();
        _wake.Reset();
        _burst.Reset();
        _accumulator = 0;
        _input.EndAll();
        StartRun();
    }
}
