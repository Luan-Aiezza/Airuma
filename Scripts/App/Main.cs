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

    /// <summary>Whether the window is in full screen. The window itself has a fixed size (not resizable); in full screen the
    /// picture grows to the screen keeping its proportions, and what is left at the sides is filled by the pillars of <see cref="SideFrame"/> (stretch mode canvas_items / expand).</summary>
    public static bool IsFullscreen => DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

    /// <summary>The mode last chosen (the window itself may take a frame to show it).</summary>
    public static bool FullscreenChosen { get; private set; }

    public static void SetFullscreen(bool on)
    {
        FullscreenChosen = on;
        DisplayServer.WindowSetMode(on ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        var cfg = new ConfigFile();
        cfg.Load(SettingsPath);
        cfg.SetValue("display", "fullscreen", on);
        cfg.Save(SettingsPath);
    }

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
            bool toggle = key.Keycode == Key.F11
                || ((key.Keycode == Key.Enter || key.Keycode == Key.KpEnter) && key.AltPressed)
                || (key.Keycode == Key.F && key.MetaPressed && key.CtrlPressed);
            if (toggle)
            {
                SetFullscreen(!IsFullscreen);
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _Ready()
    {
        Loc.Load();
        var settings = new ConfigFile();
        if (settings.Load(SettingsPath) == Error.Ok && (bool)settings.GetValue("display", "fullscreen", false))
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
        FullscreenChosen = IsFullscreen;

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

    private void ContinueRun(int stage, int level) => StartGame(null, (Difficulty)level, stage, _carry);

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
        menu.StartGame += level => CallDeferred(nameof(StartGameDeferred), -1, (int)level, 0);
        menu.StartPractice += (stage, phase) => CallDeferred(nameof(StartGameDeferred), phase, (int)Difficulty.Normal, stage);
        Swap(menu);
    }

    private void StartGameDeferred(int phase, int level, int stage) => StartGame(phase <= 0 ? null : phase, (Difficulty)level, stage, null, stagePractice: phase == 0);

    private void StartGame(int? phase, Difficulty difficulty = Difficulty.Normal, int stage = 0, RunCarry carry = null, bool stagePractice = false)
    {
        _inMenu = false;
        _menu = null;
        var definition = Stages.All[System.Math.Clamp(stage, 0, Stages.All.Count - 1)];
        var game = new GameScene(phase, difficulty, definition, carry, stagePractice) { Autoplay = _autoplay };
        game.Exit += () => CallDeferred(nameof(ShowMenu));
        game.NextStage += next =>
        {
            int index = Stages.IndexOf(definition) + 1;
            _carry = next;
            CallDeferred(nameof(ContinueRun), index, (int)difficulty);
        };
        Swap(game);
    }
}
