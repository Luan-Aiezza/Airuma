using Godot;

/// <summary>
/// Shows the menu; starts a run (or practice of a boss or of a stage) from it; a won stage goes on to the next one, carrying what the boat has;
/// a finished run returns to a new menu. Also the window: full screen, and where the picture sits. What can be asked from the command line is in
/// <see cref="LaunchOptions"/>; the tools that render art, sounds and music to files are in <see cref="DevTools"/>.
/// </summary>
public partial class Main : Node2D
{
    private Node _current;
    private bool _autoplay;
    private LaunchOptions _options;
    private readonly Node2D _stage = new();
    private readonly SideFrame _sides = new();

    /// <summary>Where the game's picture sits in the view: when the screen is wider than the game (full screen on a 16:9 monitor) the
    /// view grows to the right and below, and the picture is centred by this offset. Mouse and touch positions subtract it.</summary>
    public static Vector2 StageOffset { get; private set; }

    /// <summary>Size of the picture the game is drawn for.</summary>
    public static readonly Vector2 GameSize = Layout.SceneSize;

    private string _shot;
    private int _shotFrames, _shotAt = 90;

    /// <summary>For looking at the picture: `--shot=file.png` saves the view after a few frames (`--shot-frames=N`, 90 by default) and quits.</summary>
    public override void _Process(double delta)
    {
        WatchScreenMode((float)delta);
        // the menu left alone for a while turns into a demonstration of the game
        if (_menu == null || _menu.CurrentScreen != MenuScene.Screen.Main) _idle = 0;     // only the main menu, left alone, turns into the demonstration: not a sub-menu
        if (_inMenu && !_starting && (_idle += (float)delta) >= IdleSeconds) StartDemo();
        if (_shot == null || ++_shotFrames < _shotAt) return;
        GetViewport().GetTexture().GetImage().SavePng(_shot);
        GetTree().Quit();
    }

    /// <summary>Seconds without a key, click, touch or move of the mouse in the menu before the demonstration begins.</summary>
    private const float IdleSeconds = 10;
    private float _idle;
    private bool _inMenu, _starting;
    private MenuScene _menu;
    private static int _demoStage = System.Random.Shared.Next(4);

    /// <summary>A black veil over everything, for the soft change between the menu and the demonstration.</summary>
    private readonly ColorRect _veil = new() { Color = new Color(0, 0, 0, 0), MouseFilter = Control.MouseFilterEnum.Ignore };
    private Tween _veilTween;
    private const float FadeSeconds = 0.7f;

    private void BuildVeil()
    {
        var layer = new CanvasLayer { Layer = 100 };
        AddChild(layer);
        _veil.AnchorRight = 1; _veil.AnchorBottom = 1;
        _veil.Visible = false;
        layer.AddChild(_veil);
    }

    /// <summary>Fades the veil to `alpha` (1: black); `then` runs when it gets there. At 0 the veil is hidden, so it costs nothing.</summary>
    private void FadeTo(float alpha, System.Action then = null)
    {
        _veilTween?.Kill();
        _veil.Visible = true;
        _veilTween = CreateTween();
        _veilTween.TweenProperty(_veil, "color:a", alpha, FadeSeconds);
        _veilTween.TweenCallback(Callable.From(() => { if (alpha <= 0) _veil.Visible = false; then?.Invoke(); }));
    }

    private void StartDemo()
    {
        _starting = true;
        var definition = Stages.All[_demoStage++ % 4];     // stages 1 to 4, in turns
        // the menu fades to black, the demonstration is built behind the veil and fades in; on leaving, the same the other way round
        FadeTo(1, () =>
        {
            _starting = false; _inMenu = false; _menu = null;
            var game = new GameScene(null, Difficulty.Normal, definition, null, stagePractice: true) { Demo = true };
            game.Exit += () => FadeTo(1, () => { MusicPlayer.Instance.Silent = false; ShowMenu(); FadeTo(0); });
            MusicPlayer.Instance.Stop(0.2f);
            MusicPlayer.Instance.Silent = true;     // the demonstration has no music: it would give the songs away
            Swap(game);
            FadeTo(0);
        });
    }

    private void Fit()
    {
        var view = GetViewport().GetVisibleRect().Size;
        float extraX = Mathf.Max(0, (view.X - GameSize.X) / 2), extraY = Mathf.Max(0, (view.Y - GameSize.Y) / 2);
        StageOffset = new Vector2(extraX, extraY);
        _stage.Position = StageOffset;
        _sides.Fit(extraX, GameSize.X, view.Y);
    }

    private const string SettingsPath = "user://settings.cfg";

    /// <summary>Whether the window is in full screen. The window itself has a fixed size (not resizable, except while in full screen); in full screen the
    /// picture grows to the screen keeping its proportions, and what is left at the sides is filled by the pillars of <see cref="SideFrame"/> (stretch mode canvas_items / expand).</summary>
    public static bool IsFullscreen => DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

    /// <summary>The mode last chosen (the window itself may take a frame to show it).</summary>
    public static bool FullscreenChosen { get; private set; }

    /// <summary>Seconds since the last change of mode was asked for (the window takes a while to get there on macOS: an animation), and how long it has been in a window with the resize still free.</summary>
    private static float _sinceRequest = 99, _windowedFor;
    private static bool _lockPending;

    /// <summary>
    /// The window is not resizable, and on macOS a window that cannot be resized goes to full screen without growing (the picture stays at the size of the window, in the middle of the screen). So the window
    /// is made resizable while it is in full screen, and fixed again some moments after it is back in a window (not at once: the way out of full screen is an animation, and the flag must not change in the middle of it).
    /// </summary>
    private static void ApplyScreenMode(bool fullscreen)
    {
        _sinceRequest = 0; _windowedFor = 0;
        if (fullscreen)
        {
            _lockPending = false;
            DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.ResizeDisabled, false);
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        }
        else
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
            _lockPending = true;
        }
    }

    /// <summary>
    /// Every frame: fixes the window's size again once it has been in a window for a while, and keeps <see cref="FullscreenChosen"/> true to the window itself when it was changed from outside
    /// (the green button, or Esc, of macOS), long after the last request, saving the new mode.
    /// </summary>
    private static void WatchScreenMode(float delta)
    {
        _sinceRequest += delta;
        if (_lockPending)
        {
            _windowedFor = IsFullscreen ? 0 : _windowedFor + delta;
            if (_windowedFor >= 0.8f) { DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.ResizeDisabled, true); _lockPending = false; }
        }
        if (_sinceRequest > 2.0f && FullscreenChosen != IsFullscreen)
        {
            FullscreenChosen = IsFullscreen;
            if (!IsFullscreen) _lockPending = true; else DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.ResizeDisabled, false);
            SaveFullscreen(FullscreenChosen);
        }
    }

    private static void SaveFullscreen(bool on)
    {
        var cfg = new ConfigFile();
        cfg.Load(SettingsPath);
        cfg.SetValue("display", "fullscreen", on);
        cfg.Save(SettingsPath);
    }

    public static void SetFullscreen(bool on)
    {
        FullscreenChosen = on;
        ApplyScreenMode(on);
        SaveFullscreen(on);
    }

    /// <summary>The keys that toggle full screen from anywhere: F11, Alt+Enter, Cmd+Ctrl+F. The scenes and screens leave them alone, so that they reach <see cref="_Input"/> of this node.</summary>
    public static bool IsFullscreenKey(InputEvent @event) =>
        @event is InputEventKey { Pressed: true } key
        && (key.Keycode == Key.F11 || ((key.Keycode == Key.Enter || key.Keycode == Key.KpEnter) && key.AltPressed) || (key.Keycode == Key.F && key.MetaPressed && key.CtrlPressed));

    /// <summary>F11, Alt+Enter or Cmd+Ctrl+F toggle full screen from anywhere (menu or game).</summary>
    public override void _Input(InputEvent @event)
    {
        if (_inMenu && @event is InputEventKey or InputEventMouseButton or InputEventMouseMotion or InputEventScreenTouch or InputEventScreenDrag or InputEventJoypadButton)
        {
            _idle = 0;
            if (_starting) { _starting = false; FadeTo(0); }     // a key during the fade to black: the menu stays
        }
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            if (IsFullscreenKey(@event))
            {
                SetFullscreen(!FullscreenChosen);
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _Ready()
    {
        Loc.Load();
        GetWindow().Title = UIText.Title;                // the project's name is plain ASCII (the macOS export cannot open an app whose executable has an accent); the window shows the real one
        var settings = new ConfigFile();
        if (settings.Load(SettingsPath) == Error.Ok && (bool)settings.GetValue("display", "fullscreen", false))
        {
            ApplyScreenMode(true);
            FullscreenChosen = true;
        }

        _options = LaunchOptions.FromCommandLine();
        if (DevTools.Run(_options)) { GetTree().Quit(); return; }
        if (_options.SelfTest) { GetTree().Quit(SelfTest.Run()); return; }
        Settings.Load();
        _shot = _options.Shot;
        _shotAt = _options.ShotFrames;
        _autoplay = _options.Autoplay;

        AddChild(_sides);
        AddChild(_stage);
        BuildVeil();
        Fit();
        GetTree().Root.SizeChanged += Fit;
        AddChild(new MusicPlayer());
        AddChild(new SfxPlayer());
        _stage.AddChild(new AchievementToast());
        if (_options.Practice is int phase) StartGame(phase, Difficulty.Normal, _options.Stage);
        else if (_options.Game) StartGame(null, _options.Difficulty, _options.Stage);
        else if (_shot != null) ShowMenu();
        else ShowIntro();
    }

    private void Swap(Node next)
    {
        if (_current != null) { _stage.RemoveChild(_current); _current.QueueFree(); }
        _current = next;
        _stage.AddChild(next);
    }

    private RunCarry _carry;

    private void ContinueRun(int stage, int level, int weapon) => StartGame(null, (Difficulty)level, stage, _carry, weapon: (WeaponStyle)weapon);

    private bool _menuThemeOn;

    /// <summary>When the game opens, before the menu: the story so far, over a picture of the night (Z turns the page, X skips it).</summary>
    private void ShowIntro()
    {
        MusicPlayer.Instance?.Play(MusicPlayer.Theme.Menu);
        _menuThemeOn = true;
        var story = new IntroScreen(Dialogues.IntroStory);
        story.Finished += () => CallDeferred(nameof(ShowMenu));
        Swap(story);
    }

    private void ShowMenu()
    {
        if (!_menuThemeOn) MusicPlayer.Instance?.Play(MusicPlayer.Theme.Menu);
        _menuThemeOn = false;
        _idle = 0;
        _inMenu = true;
        var menu = _menu = new MenuScene();
        menu.Quit += () => GetTree().Quit();
        menu.StartGame += (level, weapon) => CallDeferred(nameof(StartGameDeferred), -1, (int)level, 0, (int)weapon);
        menu.StartPractice += (stage, phase) => CallDeferred(nameof(StartGameDeferred), phase, (int)Difficulty.Normal, stage, (int)Settings.Weapon);
        Swap(menu);
    }

    private void StartGameDeferred(int phase, int level, int stage, int weapon)
    {
        if (phase < 0) RunRestarts.Used = 0;     // a new game: the restarts are given again
        StartGame(phase <= 0 ? null : phase, (Difficulty)level, stage, null, stagePractice: phase == 0, weapon: (WeaponStyle)weapon);
    }


    private void StartGame(int? phase, Difficulty difficulty = Difficulty.Normal, int stage = 0, RunCarry carry = null, bool stagePractice = false, WeaponStyle? weapon = null)
    {
        _inMenu = false;
        _menu = null;
        var definition = Stages.All[System.Math.Clamp(stage, 0, Stages.All.Count - 1)];
        var attack = weapon ?? Settings.Weapon;
        var game = new GameScene(phase, difficulty, definition, carry, stagePractice, attack) { Autoplay = _autoplay };
        game.Exit += () => CallDeferred(nameof(ShowMenu));
        game.NextStage += next =>
        {
            int index = Stages.IndexOf(definition) + 1;
            _carry = next;
            CallDeferred(nameof(ContinueRun), index, (int)difficulty, (int)attack);
        };
        Swap(game);
    }
}
