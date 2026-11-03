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
	public enum Screen { Main, Difficulty, PracticeKind, PracticeStages, Practice, Scores, Config, Controls, MusicRoom }

	public enum ActionKind { Start, Practice, PracticeStages, PracticeBosses, Scores, MusicRoom, Config, Quit, Rehearse, RehearseStage, Back, Choose, Setting, OpenControls, RestoreSettings, KeyBind, RestoreKeys, Track }

	/// <summary>The settings of the Config screen (the Phase of an entry of kind Setting).</summary>
	public enum SettingId { Lives, Bombs, Music, Sfx, Mode }

	public struct Entry
	{
		public string Title;
		public ActionKind Action;
		/// <summary>The attack to rehearse (1..6) when Action is Rehearse.</summary>
		public int Phase;
		/// <summary>The stage (0...) of the boss when Action is Rehearse.</summary>
		public int Stage;
		/// <summary>The difficulty to start with when Action is Choose.</summary>
		public Difficulty Level;
		/// <summary>A line kept for a boss that is not in the game yet: hidden behind a padlock, and it cannot be chosen.</summary>
		public bool Locked;
		public Entry(string title, ActionKind action, int phase = 0, Difficulty level = Difficulty.Normal, bool locked = false, int stage = 0) { Stage = stage; Title = title; Action = action; Phase = phase; Level = level; Locked = locked; }
	}

	// MARK: Events

	/// <summary>Raised to begin a full run on the chosen difficulty.</summary>
	public event Action<Difficulty> StartGame;
	/// <summary>Raised when the player chooses to close the game.</summary>
	public event Action Quit;
	/// <summary>Raised to begin practising: the stage (0...) and the attack (1..6) of its boss, or 0 for the whole stage from the start.</summary>
	public event Action<int, int> StartPractice;

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
	private readonly List<PixelLabel> _valueNodes = new();
	private PixelLabel _hint;
	/// <summary>The key of an action is being chosen: the next key pressed is the one (Escape gives up).</summary>
	private bool _capturing;
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

		_title = new PixelLabel(PixelLabel.Style.Title, 9, PixelLabel.Align.Center) { Position = At(330, 78) };
		_title.SetText(UIText.Title);
		_ui.AddChild(_title);

		_hint = new PixelLabel(PixelLabel.Style.MenuDim, 3, PixelLabel.Align.Right) { Position = At(Layout.SceneSize.X - 28, 730) };
		_hint.SetText(UIText.MenuHint);
		_ui.AddChild(_hint);

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
					new(UIText.MenuConfig, ActionKind.Config), new(UIText.MenuQuit, ActionKind.Quit),
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
			case Screen.PracticeKind:
				Entries = new List<Entry>
				{
					new(UIText.PracticeStages, ActionKind.PracticeStages), new(UIText.PracticeBosses, ActionKind.PracticeBosses), new(UIText.Back, ActionKind.Back),
				};
				break;
			case Screen.PracticeStages:
				Entries = new List<Entry>();
				for (int i = 0; i < Stages.All.Count; i++) Entries.Add(new Entry(Stages.All[i].Title, ActionKind.RehearseStage, 0, stage: i));
				Entries.Add(new Entry(UIText.Back, ActionKind.Back));
				break;
			case Screen.Practice:
				Entries = new List<Entry>();
				// The whole fight with the first boss, from the start; the other five bosses are not here yet.
				for (int i = 0; i < Stages.All.Count; i++) Entries.Add(new Entry($"{i + 1}  {Stages.All[i].Boss.Name}", ActionKind.Rehearse, 1, stage: i));
				for (int i = Stages.All.Count + 1; i <= 6; i++) Entries.Add(new Entry($"{i}  {UIText.LockedBoss}", ActionKind.Rehearse, 0, Difficulty.Normal, locked: true));
				Entries.Add(new Entry(UIText.Back, ActionKind.Back));
				break;
			case Screen.Scores:
				Entries = new List<Entry> { new(UIText.Back, ActionKind.Back) };
				break;
			case Screen.Config:
				Entries = new List<Entry>
				{
					new(UIText.CfgLives, ActionKind.Setting, (int)SettingId.Lives), new(UIText.CfgBombs, ActionKind.Setting, (int)SettingId.Bombs),
					new(UIText.CfgMusic, ActionKind.Setting, (int)SettingId.Music), new(UIText.CfgSfx, ActionKind.Setting, (int)SettingId.Sfx),
					new(UIText.CfgMode, ActionKind.Setting, (int)SettingId.Mode), new(UIText.CfgControls, ActionKind.OpenControls),
					new(UIText.CfgDefaults, ActionKind.RestoreSettings), new(UIText.CfgExit, ActionKind.Back),
				};
				break;
			case Screen.MusicRoom:
				// every song in the order a player meets them; those of a stage and its boss stay behind a padlock until the stage is cleared
				Entries = new List<Entry>();
				for (int i = 0; i < MusicRoom.Tracks.Count; i++)
				{
					var track = MusicRoom.Tracks[i];
					Entries.Add(new Entry(track.Unlocked ? Loc.Get(track.TitleKey) : UIText.LockedBoss, ActionKind.Track, i, locked: !track.Unlocked));
				}
				Entries.Add(new Entry(UIText.Back, ActionKind.Back));
				_playingTrack = -1;
				break;
			case Screen.Controls:
				Entries = new List<Entry>();
				foreach (ControlAction action in Enum.GetValues<ControlAction>()) Entries.Add(new Entry(ControlName(action), ActionKind.KeyBind, (int)action));
				Entries.Add(new Entry(UIText.CfgDefaults, ActionKind.RestoreKeys));
				Entries.Add(new Entry(UIText.CfgExit, ActionKind.Back));
				break;
		}
		_capturing = false;
		_hint.SetText(newScreen is Screen.Config or Screen.Controls ? UIText.CfgHint : UIText.MenuHint);
		Rebuild();
	}

	private Vector2 ItemOrigin => CurrentScreen switch
	{
		Screen.Main => At(780, 318),
		Screen.Difficulty => At(780, 340),
		Screen.Practice or Screen.PracticeStages => At(560, 262),
		Screen.PracticeKind => At(560, 300),
		Screen.Config => At(300, 250),
		Screen.Controls => At(300, 236),
		_ => At(700, 650),
	};

	/// <summary>The track now playing in the Music Room (its index), or -1.</summary>
	private int _playingTrack = -1;
	/// <summary>The Music Room is one list that scrolls: the chosen song in the middle, larger, and the others fading above and below it (these many each way).</summary>
	private const int RoomReach = 3;
	private const float RoomRight = 1080, RoomCentreY = 430;
	private readonly Dictionary<int, Sprite2D> _roomPadlocks = new();
	private PixelLabel _title;

	private Vector2 ItemAt(int i)
	{
		var origin = ItemOrigin;
		return new Vector2(origin.X, origin.Y + i * ItemPitch);
	}

	private float ItemPitch => CurrentScreen == Screen.Config ? 52 : CurrentScreen == Screen.Controls ? 46 : CurrentScreen == Screen.Main || CurrentScreen == Screen.Difficulty ? 66 : (CurrentScreen is Screen.Practice or Screen.PracticeStages or Screen.PracticeKind ? 50 : 60);
	private float ItemScale => CurrentScreen == Screen.Main || CurrentScreen == Screen.Difficulty ? 4 : 3;

	private void Rebuild()
	{
		foreach (var pair in _itemNodes) { pair.normal.QueueFree(); pair.selected.QueueFree(); }
		_itemNodes.Clear();
		foreach (var padlock in _roomPadlocks.Values) padlock.QueueFree();
		_roomPadlocks.Clear();
		foreach (var value in _valueNodes) value?.QueueFree();
		_valueNodes.Clear();
		_extras?.QueueFree();

		_extras = new Node2D();
		_ui.AddChild(_extras);

		var origin = ItemOrigin;
		for (int i = 0; i < Entries.Count; i++)
		{
			var room = CurrentScreen == Screen.MusicRoom;
			var align = room ? PixelLabel.Align.Right : PixelLabel.Align.Left;
			var normal = new PixelLabel(PixelLabel.Style.MenuDim, ItemScale, align);
			var selected = new PixelLabel(PixelLabel.Style.Title, room ? ItemScale + 1 : ItemScale, align);
			foreach (var label in new[] { normal, selected })
			{
				label.Position = ItemAt(i);
				label.SetText(Entries[i].Title);
				_ui.AddChild(label);
			}
			_itemNodes.Add((normal, selected));
			if (Entries[i].Action is ActionKind.Setting or ActionKind.KeyBind)
			{
				var value = new PixelLabel(PixelLabel.Style.Value, ItemScale) { Position = new Vector2(origin.X + 400, origin.Y + i * ItemPitch) };
				_ui.AddChild(value);
				_valueNodes.Add(value);
			}
			else _valueNodes.Add(null);
			if (Entries[i].Locked)
			{
				var padlock = new Sprite2D { Texture = PadlockTexture(), Centered = false, Scale = new Vector2(ItemScale, ItemScale), Modulate = new Color(1, 1, 1, 0.8f) };
				padlock.Position = ItemAt(i) + new Vector2(-40, -2);
				_extras.AddChild(padlock);
				if (CurrentScreen == Screen.MusicRoom) _roomPadlocks[i] = padlock;
			}
		}

		switch (CurrentScreen)
		{
			case Screen.Difficulty:
				var title = new PixelLabel(PixelLabel.Style.MenuHeading, 5) { Position = At(730, 250) };
				title.SetText(UIText.DifficultyTitle);
				_extras.AddChild(title);
				break;
			case Screen.PracticeKind:
			case Screen.PracticeStages:
			case Screen.Practice:
				var heading = new PixelLabel(PixelLabel.Style.MenuHeading, 5) { Position = At(560, 182) };
				heading.SetText(CurrentScreen == Screen.PracticeStages ? UIText.PracticeStageTitle : (CurrentScreen == Screen.Practice ? UIText.PracticeTitle : UIText.MenuBossPractice));
				_extras.AddChild(heading);
				break;
			case Screen.MusicRoom:
				var roomTitle = new PixelLabel(PixelLabel.Style.MenuHeading, 5) { Position = At(300, 160) };
				roomTitle.SetText(UIText.MusicRoomTitle);
				_extras.AddChild(roomTitle);
				break;
			case Screen.Scores:
				BuildScoreTable(_extras);
				break;
			case Screen.Config:
			case Screen.Controls:
				var cfgTitle = new PixelLabel(PixelLabel.Style.MenuHeading, 5) { Position = At(300, 160) };
				cfgTitle.SetText(CurrentScreen == Screen.Config ? UIText.CfgTitle : UIText.CfgControls);
				_extras.AddChild(cfgTitle);
				break;
		}
		RefreshValues();

		RefreshTracks();
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

	/// <summary>The song that is playing shows in gold.</summary>
	private void RefreshTracks()
	{
		if (CurrentScreen != Screen.MusicRoom) return;
		for (int i = 0; i < _itemNodes.Count && i < Entries.Count; i++)
		{
			float tint = Entries[i].Action == ActionKind.Track && Entries[i].Phase == _playingTrack ? 0.9f : 0f;
			_itemNodes[i].normal.SetTint(new Color(1f, 0.82f, 0.3f), tint);
		}
	}

	private void RefreshSelection()
	{
		if (CurrentScreen == Screen.MusicRoom) { RefreshRoom(); return; }
		for (int i = 0; i < _itemNodes.Count; i++)
		{
			var pair = _itemNodes[i];
			bool isSelected = i == Selection;
			pair.normal.Visible = !isSelected;
			pair.selected.Visible = isSelected;
			// The chosen entry steps out to the left.
			pair.selected.Position = ItemAt(i) + new Vector2(-18, 0);
		}
		if (Selection >= _itemNodes.Count) return;
		var label = _itemNodes[Selection].selected;
		_marker.Position = new Vector2(label.Position.X - 36, label.Position.Y + PixelFont.GlyphHeight * ItemScale / 2);
		_marker.Visible = true;
	}

	/// <summary>The Music Room's list: the chosen song in the middle, larger and lit; the ones near it above and below, smaller and fainter; the rest hidden. Moving the choice scrolls the list.</summary>
	private void RefreshRoom()
	{
		_marker.Visible = false;
		for (int i = 0; i < _itemNodes.Count; i++)
		{
			var (normal, selected) = _itemNodes[i];
			int d = i - Selection;
			bool shown = Math.Abs(d) <= RoomReach;
			normal.Visible = shown && d != 0;
			selected.Visible = d == 0;
			var label = d == 0 ? selected : normal;
			float height = PixelFont.GlyphHeight * label.PixelScale;
			float y = RoomCentreY + d * 58 + Math.Sign(d) * 10 - height / 2;
			float alpha = d == 0 ? 1 : 1 - 0.27f * (Math.Abs(d) - 0.4f);
			normal.Position = new Vector2(RoomRight, y);
			selected.Position = new Vector2(RoomRight, y);
			normal.Modulate = new Color(1, 1, 1, alpha);
			if (_roomPadlocks.TryGetValue(i, out var padlock))
			{
				padlock.Visible = shown;
				padlock.Scale = new Vector2(label.PixelScale, label.PixelScale);
				padlock.Modulate = new Color(1, 1, 1, 0.8f * alpha);
				float width = label.Texture == null ? 0 : label.Texture.GetSize().X * label.PixelScale;
				padlock.Position = new Vector2(RoomRight - width - 12 * label.PixelScale, y - 2);
			}
		}
	}

	// MARK: Settings

	private static string ControlName(ControlAction action) => action switch
	{
		ControlAction.Up => UIText.CfgKeyUp, ControlAction.Down => UIText.CfgKeyDown, ControlAction.Left => UIText.CfgKeyLeft, ControlAction.Right => UIText.CfgKeyRight,
		ControlAction.Shoot => UIText.CfgKeyShoot, ControlAction.Bomb => UIText.CfgKeyBomb, _ => UIText.CfgKeyFocus,
	};

	private static string KeyName(Key key) => OS.GetKeycodeString(key).ToUpperInvariant();

	private static string ValueOf(Entry entry) => entry.Action == ActionKind.KeyBind
		? KeyName(Settings.KeyFor((ControlAction)entry.Phase))
		: (SettingId)entry.Phase switch
		{
			SettingId.Lives => $"< {Settings.Lives} >",
			SettingId.Bombs => $"< {Settings.Bombs} >",
			SettingId.Music => $"< {Settings.Music * 10}% >",
			SettingId.Sfx => $"< {Settings.Sfx * 10}% >",
			_ => Main.FullscreenChosen ? $"< {UIText.CfgFullscreen} >" : $"< {UIText.CfgWindowed} >",
		};

	/// <summary>Writes the current value beside each line that has one (the key being chosen shows as dots).</summary>
	private void RefreshValues()
	{
		for (int i = 0; i < _valueNodes.Count && i < Entries.Count; i++)
		{
			if (_valueNodes[i] == null) continue;
			_valueNodes[i].SetText(_capturing && i == Selection ? "..." : ValueOf(Entries[i]));
		}
	}

	/// <summary>Left or right on a setting: one step down or up (a step of the volume is a tenth; the lives, the bombs and the mode stop at their ends, the mode toggles).</summary>
	public void AdjustSelection(int delta)
	{
		if (Selection < 0 || Selection >= Entries.Count || Entries[Selection].Action != ActionKind.Setting) return;
		switch ((SettingId)Entries[Selection].Phase)
		{
			case SettingId.Lives: Settings.SetLives(Settings.Lives + delta); break;
			case SettingId.Bombs: Settings.SetBombs(Settings.Bombs + delta); break;
			case SettingId.Music: Settings.SetMusic(Settings.Music + delta); break;
			case SettingId.Sfx: Settings.SetSfx(Settings.Sfx + delta); SfxPlayer.Instance?.Play(SfxBank.Sound.MenuMove); break;
			case SettingId.Mode: Main.SetFullscreen(!Main.FullscreenChosen); break;
		}
		if ((SettingId)Entries[Selection].Phase is SettingId.Lives or SettingId.Bombs or SettingId.Music) SfxPlayer.Instance?.Play(SfxBank.Sound.MenuMove);
		RefreshValues();
	}

	/// <summary>Choosing a setting with Z or a click steps it up, going round to the start after the last value.</summary>
	private void StepSelection()
	{
		var entry = Entries[Selection];
		int step = 1;
		switch ((SettingId)entry.Phase)
		{
			case SettingId.Lives when Settings.Lives >= Settings.MaxLivesChoice: step = -(Settings.MaxLivesChoice - Settings.MinLives); break;
			case SettingId.Bombs when Settings.Bombs >= Settings.MaxBombsChoice: step = -(Settings.MaxBombsChoice - Settings.MinBombs); break;
			case SettingId.Music when Settings.Music >= Settings.MaxVolume: step = -Settings.MaxVolume; break;
			case SettingId.Sfx when Settings.Sfx >= Settings.MaxVolume: step = -Settings.MaxVolume; break;
		}
		AdjustSelection(step);
	}

	/// <summary>The key pressed while a line of the controls is waiting for one: Escape gives up; F11 is the window's and cannot be used.</summary>
	private void Capture(Key key)
	{
		if (key == Key.F11) return;
		_capturing = false;
		if (key != Key.Escape)
		{
			Settings.SetKey((ControlAction)Entries[Selection].Phase, key);
			SfxPlayer.Instance?.Play(SfxBank.Sound.MenuSelect);
		}
		else SfxPlayer.Instance?.Play(SfxBank.Sound.MenuBack);
		RefreshValues();
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
			case ActionKind.Practice: Show(Screen.PracticeKind); break;
			case ActionKind.PracticeStages: Show(Screen.PracticeStages); break;
			case ActionKind.PracticeBosses: Show(Screen.Practice); break;
			case ActionKind.Scores: Show(Screen.Scores); break;
			case ActionKind.MusicRoom: Show(Screen.MusicRoom); break;
				case ActionKind.Track: PlayTrack(entry.Phase); break;
			case ActionKind.Config: Show(Screen.Config); break;
			case ActionKind.Setting: StepSelection(); break;
			case ActionKind.OpenControls: Show(Screen.Controls); break;
			case ActionKind.RestoreSettings: Settings.RestoreDefaults(); RefreshValues(); break;
			case ActionKind.RestoreKeys: Settings.RestoreKeys(); RefreshValues(); break;
			case ActionKind.KeyBind: _capturing = true; ShowNotice(UIText.CfgPressKey); RefreshValues(); break;
			case ActionKind.Quit: Quit?.Invoke(); break;
			case ActionKind.Rehearse:
			case ActionKind.RehearseStage: StartPractice?.Invoke(entry.Stage, entry.Phase); break;
			case ActionKind.Back: Back(); break;
		}
	}

	/// <summary>Goes back one level (to the main menu); on the main menu it does nothing.</summary>
	public void GoBack()
	{
		if (CurrentScreen == Screen.Main) return;
		SfxPlayer.Instance?.Play(SfxBank.Sound.MenuBack);
		Back();
	}

	/// <summary>One level up: the lists of stages and of bosses go back to the choice between them, the rest to the main menu.</summary>
	/// <summary>Plays a song of the Music Room, or, if it is the one playing, goes back to the menu's own.</summary>
	private void PlayTrack(int index)
	{
		if (_playingTrack == index) { _playingTrack = -1; MusicPlayer.Instance?.Play(MusicPlayer.Theme.Menu); }
		else { _playingTrack = index; MusicPlayer.Instance?.Play(MusicRoom.Tracks[index].Theme); }
		RefreshTracks();
	}

	private void Back()
	{
		if (CurrentScreen == Screen.MusicRoom) MusicPlayer.Instance?.Play(MusicPlayer.Theme.Menu);
		BackTo();
	}

	private void BackTo() => Show(CurrentScreen is Screen.PracticeStages or Screen.Practice ? Screen.PracticeKind : (CurrentScreen == Screen.Controls ? Screen.Config : Screen.Main));

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
		if (_capturing)
		{
			if (@event is InputEventKey { Pressed: true, Echo: false } chosen) { Capture(chosen.Keycode); GetViewport().SetInputAsHandled(); }
			return;
		}
		if (@event is InputEventKey key && key.Pressed && !key.Echo)
		{
			switch (key.Keycode)
			{
				case Key.Left: case Key.A: AdjustSelection(-1); break;
				case Key.Right: case Key.D: AdjustSelection(1); break;
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
			if (label.Texture == null || !label.Visible) continue;
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
