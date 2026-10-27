using Godot;

/// <summary>
/// Shows the menu; starts a run (or boss practice) from it; a finished run returns to a new menu.
/// User args (after `--`): --selftest (run the simulation tests and quit with the failure count), --game (skip the menu and
/// start a run), --practice=N (start boss practice N directly), --autoplay (test pilot in the game).
/// </summary>
public partial class Main : Node2D
{
    private Node _current;
    private bool _autoplay;
    private readonly Node2D _stage = new();
    private readonly SideFrame _sides = new();

    /// <summary>Where the game's picture sits in the view: when the screen is wider than the game (full screen on a 16:9 monitor) the
    /// view grows to the right and below, and the picture is centred by this offset. Mouse and touch positions subtract it.</summary>
    public static Vector2 StageOffset { get; private set; }

    /// <summary>Size of the picture the game is drawn for.</summary>
    public static readonly Vector2 GameSize = Layout.SceneSize;

    private string _shot;
    private int _shotFrames;

    /// <summary>For looking at the picture: `--shot=file.png` saves the view after a few frames and quits.</summary>
    public override void _Process(double delta)
    {
        if (_shot == null || ++_shotFrames < 90) return;
        GetViewport().GetTexture().GetImage().SavePng(_shot);
        GetTree().Quit();
    }

    private static PixelCanvas Shift(PixelCanvas c, int dx, int w, int h)
    {
        var r = new PixelCanvas(w, h);
        for (int y = 0; y < c.Height; y++) for (int x = 0; x < c.Width; x++) if (c.Contains(x, y) && x + dx >= 0 && x + dx < w) r[x + dx, y] = c[x, y];
        return r;
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

    public static void SetFullscreen(bool on)
    {
        DisplayServer.WindowSetMode(on ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        var cfg = new ConfigFile();
        cfg.Load(SettingsPath);
        cfg.SetValue("display", "fullscreen", on);
        cfg.Save(SettingsPath);
    }

    /// <summary>F11, Alt+Enter or Cmd+Ctrl+F toggle full screen from anywhere (menu or game).</summary>
    public override void _Input(InputEvent @event)
    {
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
        var settings = new ConfigFile();
        if (settings.Load(SettingsPath) == Error.Ok && (bool)settings.GetValue("display", "fullscreen", false))
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);

        var args = OS.GetCmdlineUserArgs();
        foreach (var a in args)
        {
            if (a.StartsWith("--export-sfx="))
            {
                ExportSfx(a.Substring("--export-sfx=".Length));
                GetTree().Quit();
                return;
            }
            if (a.StartsWith("--export-icon="))
            {
                IconArt.MakeImage(16).SavePng(a.Substring("--export-icon=".Length));
                GetTree().Quit();
                return;
            }
            if (a.StartsWith("--export-pillar="))
            {
                // the pillar on its wall, three times side by side, at 3x, to look at
                var pil = SidePillarArt.MakePillar(); var wall = SidePillarArt.MakeWall();
                var sheet = new PixelCanvas(wall.Width * 3 + pil.Width + 4, SidePillarArt.Height);
                for (int ox = 0; ox < wall.Width * 3; ox += wall.Width) for (int y = 0; y < wall.Height; y++) for (int x = 0; x < wall.Width; x++) sheet[ox + x, y] = wall[x, y];
                PixelPaint.Composite(sheet, Shift(pil, wall.Width * 3 - 16, sheet.Width, sheet.Height));
                var img = sheet.ToImage(); img.Resize(img.GetWidth() * 4, img.GetHeight() * 4, Image.Interpolation.Nearest);
                img.SavePng(a.Substring("--export-pillar=".Length));
                GetTree().Quit();
                return;
            }
            if (a.StartsWith("--export-boss="))
            {
                ExportBoss(a.Substring("--export-boss=".Length));
                GetTree().Quit();
                return;
            }
            if (a.StartsWith("--export-music="))
            {
                ExportMusic(a.Substring("--export-music=".Length));
                GetTree().Quit();
                return;
            }
        }
        foreach (var a in args) if (a.StartsWith("--shot=")) _shot = a.Substring(7);
        _autoplay = System.Array.IndexOf(args, "--autoplay") >= 0;
        if (System.Array.IndexOf(args, "--selftest") >= 0)
        {
            GetTree().Quit(SelfTest.Run());
            return;
        }
        AddChild(_sides);
        AddChild(_stage);
        Fit();
        GetTree().Root.SizeChanged += Fit;
        AddChild(new MusicPlayer());
        AddChild(new SfxPlayer());
        foreach (var a in args)
        {
            if (a.StartsWith("--practice=") && int.TryParse(a.Substring(11), out int phase)) { StartGame(phase); return; }
        }
        if (System.Array.IndexOf(args, "--game") >= 0)
        {
            var level = Difficulty.Normal;
            foreach (var a in args) if (a.StartsWith("--difficulty=") && System.Enum.TryParse(a.Substring(13), true, out Difficulty parsed)) level = parsed;
            StartGame(null, level);
            return;
        }
        ShowMenu();
    }

    /// <summary>Renders every sound effect to a WAV file in a folder (for listening outside the game).</summary>
    /// <summary>Saves the serpent form (head frames, charging head, body) at 4x on a dark-water background, to look at the art.</summary>
    private static void ExportBoss(string folder)
    {
        System.IO.Directory.CreateDirectory(folder);
        const int k = 4;
        var sheet = Image.CreateEmpty(SerpentBossArt.Width * k / 2 * 5 + 40, SerpentBossArt.Height * k / 2 + SerpentBossArt.BodyLength * k / 4 + 30, false, Image.Format.Rgba8);
        sheet.Fill(new Color(0.05f, 0.18f, 0.22f));
        void Blit(PixelCanvas c, int x, int y, bool half)
        {
            var img = c.ToImage();
            img.Resize(c.Width * k / (half ? 4 : 2), c.Height * k / (half ? 4 : 2), Image.Interpolation.Nearest);
            sheet.BlendRect(img, new Rect2I(0, 0, img.GetWidth(), img.GetHeight()), new Vector2I(x, y));
        }
        for (int f = 0; f < 4; f++) Blit(SerpentBossArt.MakeHead(f, false), 5 + f * (SerpentBossArt.Width * k / 2 + 5), 5, false);
        Blit(SerpentBossArt.MakeHead(0, true), 5 + 4 * (SerpentBossArt.Width * k / 2 + 5), 5, false);
        Blit(SerpentBossArt.MakeBody(), 5, SerpentBossArt.Height * k / 2 + 10, true);
        sheet.SavePng(System.IO.Path.Combine(folder, "serpent.png"));
        var big = SerpentBossArt.MakeHead(2, false).ToImage();
        var bg = Image.CreateEmpty(big.GetWidth() * 5, big.GetHeight() * 5, false, Image.Format.Rgba8);
        bg.Fill(new Color(0.05f, 0.18f, 0.22f));
        big.Resize(big.GetWidth() * 5, big.GetHeight() * 5, Image.Interpolation.Nearest);
        bg.BlendRect(big, new Rect2I(0, 0, big.GetWidth(), big.GetHeight()), Vector2I.Zero);
        bg.SavePng(System.IO.Path.Combine(folder, "head.png"));
    }

    private static void ExportSfx(string folder)
    {
        System.IO.Directory.CreateDirectory(folder);
        foreach (SfxBank.Sound sound in System.Enum.GetValues<SfxBank.Sound>())
        {
            var data = SfxBank.Make(sound);
            var pcm = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                short v = (short)System.Math.Clamp(System.Math.Tanh(data[i]) * 32000, -32768, 32767);
                pcm[i * 2] = (byte)(v & 0xFF); pcm[i * 2 + 1] = (byte)((v >> 8) & 0xFF);
            }
            using var f = System.IO.File.Create(System.IO.Path.Combine(folder, sound + ".wav"));
            using var w = new System.IO.BinaryWriter(f);
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + pcm.Length); w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            w.Write(16); w.Write((short)1); w.Write((short)1); w.Write(SfxBank.Rate); w.Write(SfxBank.Rate * 2); w.Write((short)2); w.Write((short)16);
            w.Write(System.Text.Encoding.ASCII.GetBytes("data")); w.Write(pcm.Length); w.Write(pcm);
        }
    }

    /// <summary>Renders both themes to WAV files in a folder (for listening outside the game).</summary>
    private static void ExportMusic(string folder)
    {
        System.IO.Directory.CreateDirectory(folder);
        foreach (var (name, song) in new[] { ("menu_quiet_water", Songs.QuietWater()), ("stage_ember_serpent", Songs.EmberSerpent()), ("boss_inferno_serpent", Songs.InfernoSerpent()), ("boss_serpent_ablaze", Songs.SerpentAblaze()) })
        {
            var pcm = Synth.ToPcm16(song.Item1, song.Item2);
            using var f = System.IO.File.Create(System.IO.Path.Combine(folder, name + ".wav"));
            using var w = new System.IO.BinaryWriter(f);
            w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + pcm.Length); w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));
            w.Write(16); w.Write((short)1); w.Write((short)2); w.Write(Synth.Rate); w.Write(Synth.Rate * 4); w.Write((short)4); w.Write((short)16);
            w.Write(System.Text.Encoding.ASCII.GetBytes("data")); w.Write(pcm.Length); w.Write(pcm);
            GD.Print($"{name}: {song.Item1.Length / (double)Synth.Rate:F1} s");
        }
    }

    private void Swap(Node next)
    {
        if (_current != null) { _stage.RemoveChild(_current); _current.QueueFree(); }
        _current = next;
        _stage.AddChild(next);
    }

    private void ShowMenu()
    {
        MusicPlayer.Instance?.Play(MusicPlayer.Theme.Menu);
        var menu = new MenuScene();
        menu.StartGame += level => CallDeferred(nameof(StartGameDeferred), -1, (int)level);
        menu.StartPractice += phase => CallDeferred(nameof(StartGameDeferred), phase, (int)Difficulty.Normal);
        Swap(menu);
    }

    private void StartGameDeferred(int phase, int level) => StartGame(phase < 0 ? null : phase, (Difficulty)level);

    private void StartGame(int? phase, Difficulty difficulty = Difficulty.Normal)
    {
        var game = new GameScene(phase, difficulty) { Autoplay = _autoplay };
        game.Exit += () => CallDeferred(nameof(ShowMenu));
        Swap(game);
    }
}
