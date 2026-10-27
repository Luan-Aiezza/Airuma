using System;
using System.Collections.Generic;
using Godot;

/// <summary>The main menu, in the manner of Touhou: the title at the top, the entries stacked on the right with the selected
/// one lit by a flame, over a moonlit view of the river ahead of the boat.
///
/// Start begins a run. Boss Practice lets you rehearse one of the boss's attacks. Scores shows the best runs.
/// Music Room and Config are in place but not built yet.
/// A screen-space node tree for the 1110x775 viewport (Y down). The owner listens to <see cref="StartGame"/> and <see cref="StartPractice"/>.</summary>
public partial class MenuScene : Node2D
{
    public enum Screen { Main, Difficulty, Practice, Scores }

    public enum ActionKind { Start, Practice, Scores, MusicRoom, Config, Rehearse, Back, Choose }

    public struct Entry
    {
        public string Title;
        public ActionKind Action;
        /// <summary>The attack to rehearse (1..6) when Action is Rehearse.</summary>
        public int Phase;
        /// <summary>The difficulty to start with when Action is Choose.</summary>
        public Difficulty Level;
        /// <summary>A line kept for a boss that is not in the game yet: hidden behind a padlock, and it cannot be chosen.</summary>
        public bool Locked;
        public Entry(string title, ActionKind action, int phase = 0, Difficulty level = Difficulty.Normal, bool locked = false) { Title = title; Action = action; Phase = phase; Level = level; Locked = locked; }
    }

    // MARK: Events

    /// <summary>Raised to begin a full run on the chosen difficulty.</summary>
    public event Action<Difficulty> StartGame;
    /// <summary>Raised to begin practising the given attack (1..6).</summary>
    public event Action<int> StartPractice;

    // MARK: State

    public Screen CurrentScreen { get; private set; } = Screen.Main;
    public int Selection { get; private set; }
    public List<Entry> Entries { get; private set; } = new();
    /// <summary>Whether the "coming soon" notice is up.</summary>
    public bool NoticeVisible { get; private set; }

    // MARK: Nodes

    private readonly MenuBackdrop _backdrop = new();
    private readonly Node2D _ui = new();
    private readonly WindEffect _wind = new(Layout.SceneSize);
    private Node2D _extras;
    private readonly List<(PixelLabel normal, PixelLabel selected)> _itemNodes = new();
    private readonly Sprite2D _marker = new();
    private PixelLabel _notice;
    private float _clock;
    private float _uiFade = 1;
    private float _noticeTime;

    // MARK: Layout

    public const int ShadeWidth = 110;

    /// <summary>Screen point for an x and a distance from the top of the screen (the old scene was Y up, so this is the identity).</summary>
    private static Vector2 At(float x, float fromTop) => new Vector2(x, fromTop);

    public override void _Ready()
    {
        AddChild(_backdrop);
        _wind.ZIndex = 68;
        AddChild(_wind);
        _wind.SetWind(new Vector2(-1, 0), 25);
        _wind.Announce();

        // A soft shade over the right bank, so the entries read against the picture.
        var shade = new Sprite2D
        {
            Texture = MakeShade().ToTexture(), Centered = false, Scale = new Vector2(MenuArt.Pixel, Layout.Height / MenuArt.Height),
            ZIndex = 190,
        };
        shade.Position = new Vector2(Layout.SceneSize.X - ShadeWidth * MenuArt.Pixel, 0);
        AddChild(shade);

        _ui.ZIndex = 200;
        AddChild(_ui);

        var title = new PixelLabel(PixelLabel.Style.Title, 9, PixelLabel.Align.Center) { Position = At(330, 78) };
        title.SetText(UIText.Title);
        _ui.AddChild(title);

        var hint = new PixelLabel(PixelLabel.Style.MenuDim, 3, PixelLabel.Align.Right) { Position = At(Layout.SceneSize.X - 28, 730) };
        hint.SetText(UIText.MenuHint);
        _ui.AddChild(hint);

        _marker.Texture = HudArt.FlameIcon(true).ToTexture();
        _marker.Scale = new Vector2(5, 5);
        _ui.AddChild(_marker);

        _notice = new PixelLabel(PixelLabel.Style.MenuHeading, 4, PixelLabel.Align.Center) { Position = At(Layout.SceneSize.X - 330, 660), Modulate = new Color(1, 1, 1, 0) };
        _ui.AddChild(_notice);

        Show(Screen.Main);
    }

    // MARK: Shade

    /// <summary>A dark veil that thickens towards the right edge, dithered like the rest of the art.</summary>
    public static PixelCanvas MakeShade()
    {
        var c = new PixelCanvas(ShadeWidth, MenuArt.Height);
        for (int x = 0; x < ShadeWidth; x++)
        {
            float t = (float)x / (ShadeWidth - 1);
            for (int y = 0; y < MenuArt.Height; y++)
            {
                float level = MathF.Floor(t * t * 5 + (Bayer.Threshold(x, y) - 0.5f) * 0.9f) / 5;
                if (level <= 0) continue;
                c[x, y] = new PixelColor(2, 6, 10, (byte)Math.Min(255, (int)(170 * level)));
            }
        }
        return c;
    }

    // MARK: Screens

    public void Show(Screen newScreen)
    {
        CurrentScreen = newScreen;
        Selection = 0;
        switch (newScreen)
        {
            case Screen.Main:
                Entries = new List<Entry>
                {
                    new(UIText.MenuStart, ActionKind.Start), new(UIText.MenuBossPractice, ActionKind.Practice),
                    new(UIText.MenuScores, ActionKind.Scores), new(UIText.MenuMusicRoom, ActionKind.MusicRoom),
                    new(UIText.MenuConfig, ActionKind.Config),
                };
                break;
            case Screen.Difficulty:
                Entries = new List<Entry>
                {
                    new(UIText.DifficultyEasy, ActionKind.Choose, 0, Difficulty.Easy), new(UIText.DifficultyNormal, ActionKind.Choose, 0, Difficulty.Normal),
                    new(UIText.DifficultyHard, ActionKind.Choose, 0, Difficulty.Hard), new(UIText.Back, ActionKind.Back),
                };
                Selection = 1;   // the game as designed
                break;
            case Screen.Practice:
                Entries = new List<Entry>();
                // The whole fight with the first boss, from the start; the other five bosses are not here yet.
                Entries.Add(new Entry($"1  {Stages.One.Boss.Name}", ActionKind.Rehearse, 1));
                for (int i = 2; i <= 6; i++) Entries.Add(new Entry($"{i}  {UIText.LockedBoss}", ActionKind.Rehearse, 0, Difficulty.Normal, locked: true));
                Entries.Add(new Entry(UIText.Back, ActionKind.Back));
                break;
            case Screen.Scores:
                Entries = new List<Entry> { new(UIText.Back, ActionKind.Back) };
                break;
        }
        Rebuild();
    }

    private Vector2 ItemOrigin => CurrentScreen switch
    {
        Screen.Main => At(780, 318),
        Screen.Difficulty => At(780, 340),
        Screen.Practice => At(560, 262),
        _ => At(700, 650),
    };

    private float ItemPitch => CurrentScreen == Screen.Main || CurrentScreen == Screen.Difficulty ? 66 : (CurrentScreen == Screen.Practice ? 50 : 60);
    private float ItemScale => CurrentScreen == Screen.Main || CurrentScreen == Screen.Difficulty ? 4 : 3;

    private void Rebuild()
    {
        foreach (var pair in _itemNodes) { pair.normal.QueueFree(); pair.selected.QueueFree(); }
        _itemNodes.Clear();
        _extras?.QueueFree();

        _extras = new Node2D();
        _ui.AddChild(_extras);

        var origin = ItemOrigin;
        for (int i = 0; i < Entries.Count; i++)
        {
            var normal = new PixelLabel(PixelLabel.Style.MenuDim, ItemScale);
            var selected = new PixelLabel(PixelLabel.Style.Title, ItemScale);
            foreach (var label in new[] { normal, selected })
            {
                label.Position = new Vector2(origin.X, origin.Y + i * ItemPitch);
                label.SetText(Entries[i].Title);
                _ui.AddChild(label);
            }
            _itemNodes.Add((normal, selected));
            if (Entries[i].Locked)
            {
                var padlock = new Sprite2D { Texture = PadlockTexture(), Centered = false, Scale = new Vector2(ItemScale, ItemScale), Modulate = new Color(1, 1, 1, 0.8f) };
                padlock.Position = new Vector2(origin.X - 52, origin.Y + i * ItemPitch - 2);
                _extras.AddChild(padlock);
            }
        }

        switch (CurrentScreen)
        {
            case Screen.Difficulty:
                var title = new PixelLabel(PixelLabel.Style.MenuHeading, 5) { Position = At(780, 250) };
                title.SetText(UIText.DifficultyTitle);
                _extras.AddChild(title);
                break;
            case Screen.Practice:
                var heading = new PixelLabel(PixelLabel.Style.MenuHeading, 5) { Position = At(560, 182) };
                heading.SetText(UIText.PracticeTitle);
                _extras.AddChild(heading);
                break;
            case Screen.Scores:
                BuildScoreTable(_extras);
                break;
        }

        _uiFade = 0;
        _ui.Modulate = new Color(1, 1, 1, 0);
        RefreshSelection();
    }

    private static ImageTexture _padlock;

    /// <summary>The little padlock of a locked line (the same as in the locked card slots of the panel).</summary>
    private static ImageTexture PadlockTexture()
    {
        if (_padlock != null) return _padlock;
        var c = new PixelCanvas(8, 9);
        string[] rows = { ".####.", "#....#", "#....#", "######", "##..##", "##..##", "######" };
        var color = PixelColor.Hex(0x8FA3B8);
        for (int y = 0; y < rows.Length; y++)
            for (int x = 0; x < rows[y].Length; x++)
                if (rows[y][x] == '#') c[1 + x, 1 + y] = color;
        return _padlock = c.ToTexture();
    }

    private void BuildScoreTable(Node2D node)
    {
        var heading = new PixelLabel(PixelLabel.Style.MenuHeading, 5) { Position = At(560, 182) };
        heading.SetText(UIText.ScoresTitle);
        node.AddChild(heading);

        var table = HighScore.Entries;
        for (int rank = 0; rank < HighScore.Capacity; rank++)
        {
            float y = 262 + rank * 66;
            bool has = rank < table.Count;
            var number = new PixelLabel(PixelLabel.Style.MenuDim, 4) { Position = At(560, y) };
            number.SetText($"{rank + 1}");
            node.AddChild(number);

            var score = new PixelLabel(has ? PixelLabel.Style.Value : PixelLabel.Style.MenuDim, 4) { Position = At(630, y) };
            score.SetText(has ? Math.Min(table[rank].Score, 9_999_999).ToString("D7") : UIText.NoScore);
            node.AddChild(score);

            if (has)
            {
                var level = new PixelLabel(PixelLabel.Style.MenuDim, 3) { Position = At(840, y + 4) };
                level.SetText(table[rank].Difficulty.Title());
                node.AddChild(level);
            }
            if (has && table[rank].Cleared)
            {
                var tag = new PixelLabel(PixelLabel.Style.Gold, 3) { Position = At(980, y + 4) };
                tag.SetText(UIText.Cleared);
                node.AddChild(tag);
            }
        }
    }

    private void RefreshSelection()
    {
        for (int i = 0; i < _itemNodes.Count; i++)
        {
            var pair = _itemNodes[i];
            bool isSelected = i == Selection;
            pair.normal.Visible = !isSelected;
            pair.selected.Visible = isSelected;
            // The chosen entry steps out to the left.
            pair.selected.Position = new Vector2(ItemOrigin.X - 18, pair.selected.Position.Y);
        }
        if (Selection >= _itemNodes.Count) return;
        var label = _itemNodes[Selection].selected;
        _marker.Position = new Vector2(label.Position.X - 36, label.Position.Y + PixelFont.GlyphHeight * ItemScale / 2);
        _marker.Visible = true;
    }

    // MARK: Actions

    public void MoveSelection(int delta)
    {
        if (Entries.Count == 0) return;
        Selection = (Selection + delta + Entries.Count) % Entries.Count;
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuMove);
        RefreshSelection();
    }

    public void Select(int index)
    {
        if (index < 0 || index >= Entries.Count) return;
        Selection = index;
        RefreshSelection();
    }

    public void ActivateSelection()
    {
        if (Selection < 0 || Selection >= Entries.Count) return;
        var entry = Entries[Selection];
        if (entry.Locked) { SfxPlayer.Instance?.Play(SfxBank.Sound.MenuBack); return; }
        SfxPlayer.Instance?.Play(entry.Action == ActionKind.Back ? SfxBank.Sound.MenuBack : SfxBank.Sound.MenuSelect);
        switch (entry.Action)
        {
            case ActionKind.Start: Show(Screen.Difficulty); break;
            case ActionKind.Choose: StartGame?.Invoke(entry.Level); break;
            case ActionKind.Practice: Show(Screen.Practice); break;
            case ActionKind.Scores: Show(Screen.Scores); break;
            case ActionKind.MusicRoom:
            case ActionKind.Config: ShowNotice(UIText.ComingSoon); break;
            case ActionKind.Rehearse: StartPractice?.Invoke(entry.Phase); break;
            case ActionKind.Back: Show(Screen.Main); break;
        }
    }

    /// <summary>Goes back one level (to the main menu); on the main menu it does nothing.</summary>
    public void GoBack()
    {
        if (CurrentScreen == Screen.Main) return;
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuBack);
        Show(Screen.Main);
    }

    private void ShowNotice(string text)
    {
        _notice.SetText(text);
        _noticeTime = 0;
        NoticeVisible = true;
        _notice.Modulate = new Color(1, 1, 1, 1);
    }

    // MARK: Per-frame

    public override void _Process(double delta)
    {
        float dt = (float)Math.Min(delta, 0.1);
        _clock += dt;
        _wind.Update(dt);

        if (_uiFade < 1)
        {
            _uiFade = Math.Min(1, _uiFade + dt / 0.25f);
            _ui.Modulate = new Color(1, 1, 1, _uiFade);
        }

        // The flame pulses: up to 1.12 over 0.25 s, down to 0.94 over 0.3 s.
        float phase = _clock % 0.55f;
        float pulse = phase < 0.25f ? 0.94f + (1.12f - 0.94f) * phase / 0.25f : 1.12f + (0.94f - 1.12f) * (phase - 0.25f) / 0.3f;
        _marker.Scale = new Vector2(5 * pulse, 5 * pulse);

        if (NoticeVisible)
        {
            _noticeTime += dt;
            if (_noticeTime >= 1.7f) { NoticeVisible = false; _notice.Modulate = new Color(1, 1, 1, 0); }
            else if (_noticeTime > 1.2f) _notice.Modulate = new Color(1, 1, 1, 1 - (_noticeTime - 1.2f) / 0.5f);
        }
    }

    // MARK: Input

    /// <summary>Keyboard: arrows (or W/S) move, Z / Return / Space choose, X / Escape go back. Acts when a key goes down.</summary>
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo)
        {
            switch (key.Keycode)
            {
                case Key.Up: case Key.W: MoveSelection(-1); break;
                case Key.Down: case Key.S: MoveSelection(1); break;
                case Key.Z: case Key.Enter: case Key.KpEnter: case Key.Space: ActivateSelection(); break;
                case Key.X: case Key.Escape: GoBack(); break;
                default: return;
            }
            GetViewport().SetInputAsHandled();
        }
        else if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Left)
        {
            Click(_ui.ToLocal(mouse.GlobalPosition));
        }
    }

    /// <summary>A press at a point in menu space (screen pixels): chooses the entry under it, if any.</summary>
    public void Click(Vector2 p)
    {
        for (int i = 0; i < _itemNodes.Count; i++)
        {
            var pair = _itemNodes[i];
            var label = pair.normal.Visible ? pair.normal : pair.selected;
            if (label.Texture == null) continue;
            var rect = new Rect2(label.Position + label.Offset * label.PixelScale, label.Texture.GetSize() * label.PixelScale).Grown(0);
            rect = new Rect2(rect.Position - new Vector2(24, 14), rect.Size + new Vector2(48, 28));
            if (rect.HasPoint(p))
            {
                Select(i);
                ActivateSelection();
                return;
            }
        }
    }
}
