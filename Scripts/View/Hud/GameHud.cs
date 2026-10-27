using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

/// <summary>
/// The Touhou-style HUD: a side panel (title, the strip of cards the player has taken, score, lives, lantern charges, power,
/// wind, Airu's dialogue and an ornament), a frame around the river, the boss bar and the end screen. The panel's look comes from a
/// <see cref="HudTheme"/>: each stage has its own, after its boss.
/// Everything lives in screen space (Y down, 1110x775); add it to the scene at the origin, above the river (ZIndex 100 is set here).
/// </summary>
public partial class GameHud : Node2D
{
    public enum Control { Restart, Menu }

    public static readonly Vector2 SceneSize = Layout.SceneSize;
    public const float PanelX = Layout.PanelX;
    /// <summary>The panel was drawn for a river 600 wide; everything on it shifts by the extra width.</summary>
    private const float PanelShift = Layout.PanelX - 598;
    public const int MaxLives = 3, MaxBombs = 3, MaxPower = 3;
    public const int TotalPhases = 6;

    /// <summary>Characters per line in the dialogue box.</summary>
    public const int DialogueWidth = 14;
    public const int DialogueLineCount = 3;

    private const float Art = HudArt.Scale;

    // Textures, built once.
    private readonly ImageTexture _boatFull = HudArt.BoatIcon(true).ToTexture();
    private readonly ImageTexture _boatEmpty = HudArt.BoatIcon(false).ToTexture();
    private readonly ImageTexture _lanternFull = HudArt.LanternIcon(true).ToTexture();
    private readonly ImageTexture _lanternEmpty = HudArt.LanternIcon(false).ToTexture();
    private readonly ImageTexture _flameFull = HudArt.FlameIcon(true).ToTexture();
    private readonly ImageTexture _flameEmpty = HudArt.FlameIcon(false).ToTexture();
    private readonly ImageTexture[] _arrowTextures = Enumerable.Range(1, 3).Select(i => HudArt.WindArrow(i).ToTexture()).ToArray();
    private readonly ImageTexture[] _windPulseTextures = Enumerable.Range(0, HudArt.WindPulseFrames).Select(i => HudArt.WindPulse(i).ToTexture()).ToArray();

    private readonly List<Sprite2D> _lifeIcons = new(), _bombIcons = new(), _powerIcons = new();
    private readonly HudTheme _theme;
    private readonly List<Sprite2D> _cardSlots = new();
    private readonly Dictionary<Card, ImageTexture> _cardTextures = new();
    private readonly ImageTexture _lockedSlot = HudArt.CardSlot(null).ToTexture();
    private int _shownCards = -1;
    private readonly Sprite2D _compassArrow = new();
    private readonly ColorRect _powerBarFill = NewRect(HudArt.Colors.Gold.ToColor());
    private const float PowerBarWidth = 92;
    private readonly Sprite2D _windPulse = new();
    private readonly ColorRect _calmGem = NewRect(HudArt.Colors.Gold.ToColor());

    private readonly PixelLabel _hiScoreLabel = new(PixelLabel.Style.Gold, 3, PixelLabel.Align.Right);
    private readonly PixelLabel _scoreLabel = new(PixelLabel.Style.Value, 3, PixelLabel.Align.Right);
    private readonly PixelLabel _windDirectionLabel = new(PixelLabel.Style.Value);
    private readonly PixelLabel _windStrengthLabel = new(PixelLabel.Style.Dim);

    private readonly Sprite2D _portrait = new() { Centered = false };
    private readonly ImageTexture[] _portraitFrames = { HudArt.FairyPortrait(0).ToTexture(), HudArt.FairyPortrait(1).ToTexture() };
    private float _portraitTime;
    private readonly List<PixelLabel> _dialogueLabels = new();
    private List<string> _dialogueLines = new();
    private float _revealed;
    private int _dialogueLength;

    private readonly Node2D _bossGroup = new();
    private readonly ColorRect[] _bossFillRows = { NewRect(Colors.Transparent), NewRect(Colors.Transparent), NewRect(Colors.Transparent) };
    private const float BossBarInnerWidth = 392;
    private float _shownBossHealth = 1;

    private readonly Node2D _banner = new();
    private readonly PixelLabel _bannerLabel = new(PixelLabel.Style.Value, 3, PixelLabel.Align.Right);
    private bool _bannerActive;
    private float _bannerTime;

    private readonly Node2D _endPanel = new();
    private readonly PixelLabel _victoryTitle = new(PixelLabel.Style.Victory, 5, PixelLabel.Align.Center);
    private readonly PixelLabel _defeatTitle = new(PixelLabel.Style.Defeat, 5, PixelLabel.Align.Center);
    private readonly PixelLabel _endSubtitle = new(PixelLabel.Style.Label, 3, PixelLabel.Align.Center);
    private readonly PixelLabel _endPoints = new(PixelLabel.Style.Value, 3, PixelLabel.Align.Center);
    private readonly PixelLabel _endRecord = new(PixelLabel.Style.Gold, 3, PixelLabel.Align.Center);
    private readonly Sprite2D _restartPlaque = new() { Texture = HudArt.Plaque(36, 9).ToTexture() };
    private readonly Sprite2D _menuPlaque = new() { Texture = HudArt.Plaque(36, 9).ToTexture() };

    // Wind pulse animation state.
    private bool _pulsingWind;
    private float _pulseTime;
    private const float PulseDuration = 0.1f * HudArt.WindPulseFrames * 3;
    private const float BlinkDuration = 0.6f;
    private const float GlowDuration = 1.2f;
    private float _blinkTime = BlinkDuration;
    private float _glowTime = GlowDuration;

    public bool IsEndVisible => _endPanel.Visible;

    private static ColorRect NewRect(Color color) => new ColorRect { Color = color, MouseFilter = Godot.Control.MouseFilterEnum.Ignore };

    private static int Z(float z) => (int)Math.Round(z * 10);

    public GameHud(HudTheme theme = null)
    {
        _theme = theme ?? HudTheme.Original();
        ZIndex = Z(10);
        BuildPanel();
        BuildFrame();
        BuildStats();
        BuildDialogue();
        BuildBossBar();
        BuildEndPanel();
    }

    // MARK: Layout helpers

    /// <summary>Screen point for an x and a distance from the top of the screen.</summary>
    private static Vector2 AtScene(float x, float top) => new Vector2(x, top);

    /// <summary>A point on the side panel, with x as drawn for the original 600-wide river.</summary>
    private static Vector2 At(float x, float top) => AtScene(x + PanelShift, top);

    /// <summary>A point in the game field, with x measured from the field's left edge (inside the trees' inset).</summary>
    private static Vector2 AtField(float x, float top) => AtScene(x + Layout.FieldInset, top);

    /// <summary>A sprite whose position is where the <paramref name="anchor"/> of the texture (default top-left) sits.</summary>
    private Sprite2D MakeSprite(Texture2D texture, Vector2 at, float z = 0, Vector2? anchor = null, float? scale = null, Node parent = null)
    {
        var sp = new Sprite2D { Texture = texture, Centered = false };
        PlaceSprite(sp, anchor ?? new Vector2(0, 1), scale ?? Art);
        sp.Position = at;
        sp.ZIndex = Z(z);
        (parent ?? this).AddChild(sp);
        return sp;
    }

    private static void PlaceSprite(Sprite2D sp, Vector2 anchor, float scale)
    {
        var size = sp.Texture?.GetSize() ?? Vector2.Zero;
        sp.Centered = false;
        sp.Scale = new Vector2(scale, scale);
        sp.Offset = new Vector2(-anchor.X * size.X, -(1 - anchor.Y) * size.Y);
    }

    private enum Space { Panel, Field, Scene }

    private void Label(PixelLabel label, string text, float x, float top, Node parent = null, Space space = Space.Panel)
    {
        label.Position = space switch { Space.Panel => At(x, top), Space.Field => AtField(x, top), _ => AtScene(x, top) };
        label.ZIndex = Z(2);
        label.SetText(text);
        (parent ?? this).AddChild(label);
    }

    // MARK: Build

    private void BuildPanel()
    {
        // Plaques in art pixels, relative to the panel's top-left.
        var plaques = new[] { new Rect2(11, 19, 74, 29), new Rect2(11, 49, 74, 17), new Rect2(11, 67, 74, 54), new Rect2(11, 122, 74, 46) };
        MakeSprite(HudArt.MakePanel(plaques, _theme).ToTexture(), AtScene(PanelX, 0));

        var title = new PixelLabel(PixelLabel.Style.Title, 6, PixelLabel.Align.Center);
        Label(title, UIText.Title, 790, 32);

        // The strip of cards, in the first plaque: twelve places, locked until the cards are taken (a whole run gathers twelve).
        const float slotScale = 3, pitchX = 49, pitchY = 50;
        for (int i = 0; i < RunState.MaxCards; i++)
        {
            var slot = new Sprite2D { Texture = _lockedSlot, Centered = false, Scale = new Vector2(slotScale, slotScale), ZIndex = Z(2) };
            slot.Position = AtScene(PanelX + 44 + 5 + (i % 6) * pitchX, 76 + 9 + (i / 6) * pitchY);
            AddChild(slot);
            _cardSlots.Add(slot);
        }

        // The theme's ornament, in the free space at the bottom.
        if (_theme.Ornament != null)
            MakeSprite(_theme.Ornament().ToTexture(), At(646, 680), 1);
    }

    private void ShowCards(IReadOnlyList<Card> cards)
    {
        int count = cards?.Count ?? 0;
        if (count == _shownCards) return;
        _shownCards = count;
        for (int i = 0; i < _cardSlots.Count; i++)
        {
            if (i >= count) { _cardSlots[i].Texture = _lockedSlot; continue; }
            var card = cards[i];
            if (!_cardTextures.TryGetValue(card, out var texture)) _cardTextures[card] = texture = HudArt.CardSlot(card).ToTexture();
            _cardSlots[i].Texture = texture;
        }
    }

    private void BuildFrame()
    {
        int length = (int)Math.Ceiling(PanelX / Art);
        var horizontal = HudArt.MakeFrameStrip(length, true, _theme).ToTexture();
        MakeSprite(horizontal, AtScene(0, 0), 1); // top
        MakeSprite(horizontal, AtScene(0, Layout.Height), 1, Vector2.Zero); // bottom
        MakeSprite(HudArt.MakeFrameStrip(194, false, _theme).ToTexture(), AtScene(0, 0), 1); // left
    }

    private void BuildStats()
    {
        Label(new PixelLabel(PixelLabel.Style.Label), UIText.HiScore, 654, 204);
        Label(new PixelLabel(PixelLabel.Style.Label), UIText.Score, 654, 232);
        Label(_hiScoreLabel, "0000000", 926, 204);
        Label(_scoreLabel, "0000000", 926, 232);

        Label(new PixelLabel(PixelLabel.Style.Label), UIText.Lives, 654, 282);
        Label(new PixelLabel(PixelLabel.Style.Label), UIText.Lantern, 654, 320);
        Label(new PixelLabel(PixelLabel.Style.Label), UIText.Power, 654, 358);
        Label(new PixelLabel(PixelLabel.Style.Label), UIText.Wind, 654, 398);

        static float SlotX(int i) => 826 + i * 32;
        for (int i = 0; i < MaxLives; i++)
        {
            _lifeIcons.Add(MakeSprite(_boatEmpty, At(SlotX(i) - 2, 276), 2));
            _bombIcons.Add(MakeSprite(_lanternEmpty, At(SlotX(i), 317), 2));
            _powerIcons.Add(MakeSprite(_flameEmpty, At(SlotX(i) + 4, 355), 2));
        }

        // How close the lantern is to the next level of light.
        var barBack = NewRect(HudArt.Colors.WoodLight.ToColor());
        barBack.Size = new Vector2(PowerBarWidth, 4);
        barBack.Position = At(SlotX(0) + 2, 388);
        barBack.ZIndex = Z(2);
        AddChild(barBack);
        _powerBarFill.Size = new Vector2(0, 4);
        _powerBarFill.Position = barBack.Position;
        _powerBarFill.ZIndex = Z(3);
        AddChild(_powerBarFill);

        var compass = MakeSprite(HudArt.Compass().ToTexture(), At(888, 438), 2, new Vector2(0.5f, 0.5f));
        _windPulse.Scale = new Vector2(Art, Art);
        _windPulse.Texture = _windPulseTextures[0];
        _windPulse.Position = compass.Position;
        _windPulse.ZIndex = Z(4);
        _windPulse.Visible = false;
        AddChild(_windPulse);
        _compassArrow.Scale = new Vector2(Art, Art);
        _compassArrow.Position = compass.Position;
        _compassArrow.ZIndex = Z(3);
        AddChild(_compassArrow);
        _calmGem.Size = new Vector2(12, 12);
        _calmGem.Position = compass.Position - new Vector2(6, 6);
        _calmGem.ZIndex = Z(3);
        AddChild(_calmGem);
        Label(_windDirectionLabel, "", 654, 424);
        Label(_windStrengthLabel, "", 654, 450);
    }

    private void BuildDialogue()
    {
        _portrait.Position = At(654, 496);
        _portrait.Scale = new Vector2(Art, Art);
        _portrait.ZIndex = Z(2);
        _portrait.Texture = _portraitFrames[0];
        AddChild(_portrait);

        Label(new PixelLabel(PixelLabel.Style.Gold), UIText.FairyName, 738, 504);
        Label(new PixelLabel(PixelLabel.Style.Dim), UIText.FairyRole, 738, 532);

        MakeSprite(HudArt.Box(73, 24).ToTexture(), At(648, 574), 1);
        for (int i = 0; i < DialogueLineCount; i++)
        {
            var line = new PixelLabel(PixelLabel.Style.Speech);
            Label(line, "", 668, 590 + i * 24);
            _dialogueLabels.Add(line);
        }
    }

    private readonly PixelLabel _bossName = new(PixelLabel.Style.Label);
    private readonly PixelLabel _bossNameTop = new(PixelLabel.Style.Label, 2), _bossNameBottom = new(PixelLabel.Style.Label, 2);

    /// <summary>The name beside the boss's life bar; a long name is split over two smaller lines so as not to run into the bar.</summary>
    public void SetBossName(string name)
    {
        int space = name.IndexOf(' ');
        bool split = name.Length > 7 && space > 0;
        _bossName.SetText(split ? "" : name);
        _bossNameTop.SetText(split ? name.Substring(0, space) : "");
        _bossNameBottom.SetText(split ? name.Substring(space + 1) : "");
    }

    private void BuildBossBar()
    {
        _bossGroup.ZIndex = Z(2);
        AddChild(_bossGroup);
        Label(_bossName, Stages.One.Boss.Name, 24, 36, _bossGroup, Space.Field);
        Label(_bossNameTop, "", 24, 28, _bossGroup, Space.Field);
        Label(_bossNameBottom, "", 24, 46, _bossGroup, Space.Field);
        MakeSprite(HudArt.BossBarFrame(106).ToTexture(), AtField(160, 28), 0, null, null, _bossGroup);

        for (int i = 0; i < _bossFillRows.Length; i++)
        {
            var row = _bossFillRows[i];
            row.Color = FirePalette.Ramp[i + 1].ToColor();
            row.Position = AtField(176, 36 + i * 4);
            row.Size = new Vector2(BossBarInnerWidth, 4);
            row.ZIndex = Z(1);
            _bossGroup.AddChild(row);
        }

        _banner.ZIndex = Z(3);
        _banner.Visible = false;
        AddChild(_banner);
        MakeSprite(HudArt.Ribbon(140).ToTexture(), AtScene(PanelX, 66), 0, new Vector2(1, 1), null, _banner);
        Label(_bannerLabel, "", PanelX - 16, 74, _banner, Space.Scene);
    }

    private void BuildEndPanel()
    {
        _endPanel.ZIndex = Z(20);
        _endPanel.Visible = false;
        _endPanel.Position = AtField(300, 288); // top-centre of the game field
        AddChild(_endPanel);
        MakeSprite(HudArt.Box(100, 50).ToTexture(), Vector2.Zero, 0, new Vector2(0.5f, 1), null, _endPanel);
        _victoryTitle.SetText(UIText.Victory);
        _defeatTitle.SetText(UIText.Defeat);
        var rows = new[] { (_victoryTitle, 24f), (_defeatTitle, 24f), (_endSubtitle, 76f), (_endPoints, 84f), (_endRecord, 116f) };
        foreach (var (l, top) in rows)
        {
            l.Position = new Vector2(0, top);
            l.ZIndex = Z(1);
            _endPanel.AddChild(l);
        }
        var plaques = new[] { (_restartPlaque, UIText.Restart, -84f), (_menuPlaque, UIText.Menu, 84f) };
        foreach (var (plaque, label, x) in plaques)
        {
            PlaceSprite(plaque, new Vector2(0.5f, 1), Art);
            plaque.Position = new Vector2(x, 156);
            plaque.ZIndex = Z(1);
            _endPanel.AddChild(plaque);
            var text = new PixelLabel(PixelLabel.Style.Value, 3, PixelLabel.Align.Center);
            text.Position = new Vector2(x, 164);
            text.ZIndex = Z(2);
            text.SetText(label);
            _endPanel.AddChild(text);
        }
    }

    // MARK: Update

    public void Update(HudState state, float dt)
    {
        for (int i = 0; i < MaxLives; i++)
        {
            _lifeIcons[i].Texture = i < state.Lives ? _boatFull : _boatEmpty;
            _bombIcons[i].Texture = i < state.Bombs ? _lanternFull : _lanternEmpty;
            _powerIcons[i].Texture = i < state.Power ? _flameFull : _flameEmpty;
        }
        _powerBarFill.Size = new Vector2(MathF.Round(PowerBarWidth * state.PowerProgress / 4, MidpointRounding.AwayFromZero) * 4, 4);
        _scoreLabel.SetText(Math.Min(state.Score, 9_999_999).ToString("D7", CultureInfo.InvariantCulture));
        _hiScoreLabel.SetText(Math.Min(Math.Max(state.HiScore, state.Score), 9_999_999).ToString("D7", CultureInfo.InvariantCulture));
        ShowCards(state.Cards);
        UpdateWind(state.WindDirection, state.WindStrength);
        UpdateBossBar(state.BossHealth, dt);
        UpdateDialogue(dt);
        UpdateAnimations(dt);
    }

    /// <summary>Portrait frames, wind pulse, arrow blink and wind text glow, banner slide.</summary>
    private void UpdateAnimations(float dt)
    {
        _portraitTime += dt;
        _portrait.Texture = _portraitFrames[(int)(_portraitTime / 0.4f) % 2];

        if (_pulsingWind)
        {
            _pulseTime += dt;
            if (_pulseTime >= PulseDuration) { _pulsingWind = false; _windPulse.Visible = false; }
            else _windPulse.Texture = _windPulseTextures[(int)(_pulseTime / 0.1f) % HudArt.WindPulseFrames];
        }

        if (_blinkTime < BlinkDuration)
        {
            _blinkTime = Math.Min(BlinkDuration, _blinkTime + dt);
            float t = _blinkTime % 0.2f;
            float a = _blinkTime >= BlinkDuration ? 1 : (t < 0.1f ? 1 - 0.8f * (t / 0.1f) : 0.2f + 0.8f * ((t - 0.1f) / 0.1f));
            _compassArrow.Modulate = new Color(1, 1, 1, a);
        }

        if (_glowTime < GlowDuration)
        {
            _glowTime = Math.Min(GlowDuration, _glowTime + dt);
            float amount = 0.75f * (1 - _glowTime / GlowDuration);
            _windDirectionLabel.SetTint(HudArt.Colors.Gold.ToColor(), amount);
            _windStrengthLabel.SetTint(HudArt.Colors.Gold.ToColor(), amount);
        }

        if (_bannerActive)
        {
            _bannerTime += dt;
            const float In = 0.35f, Wait = 3.2f, Out = 0.4f;
            if (_bannerTime < In)
            {
                float u = _bannerTime / In;
                _banner.Position = new Vector2(560 * (1 - (1 - (1 - u) * (1 - u))), 0); // ease out
            }
            else if (_bannerTime < In + Wait) _banner.Position = Vector2.Zero;
            else if (_bannerTime < In + Wait + Out)
            {
                float u = (_bannerTime - In - Wait) / Out;
                _banner.Position = new Vector2(560 * u * u, 0); // ease in
                _banner.Modulate = new Color(1, 1, 1, 1 - u);
            }
            else { _bannerActive = false; _banner.Visible = false; }
        }
    }

    /// <summary>The wind changed: a golden ring spreads from the compass, the arrow blinks and the wind's text glows, so the
    /// player's eye is drawn to the new direction and strength.</summary>
    public void PulseWind()
    {
        _windPulse.Visible = true;
        _windPulse.Texture = _windPulseTextures[0];
        _pulsingWind = true;
        _pulseTime = 0;

        _compassArrow.Modulate = Colors.White;
        _blinkTime = 0;

        _glowTime = 0;
        _windDirectionLabel.SetTint(HudArt.Colors.Gold.ToColor(), 0.75f);
        _windStrengthLabel.SetTint(HudArt.Colors.Gold.ToColor(), 0.75f);
    }

    private void UpdateWind(Vector2 direction, float strength)
    {
        int level = (int)MathF.Round(strength / 25, MidpointRounding.AwayFromZero);
        bool calm = level == 0 || direction == Vector2.Zero;
        _compassArrow.Visible = !calm;
        _calmGem.Visible = calm;
        if (!calm)
        {
            _compassArrow.Texture = _arrowTextures[Math.Min(3, level) - 1];
            _compassArrow.Rotation = BulletArt.GodotRotation(direction);
        }
        var names = new (Vector2, string)[] { (new Vector2(0, -1), UIText.North), (new Vector2(0, 1), UIText.South), (new Vector2(1, 0), UIText.East), (new Vector2(-1, 0), UIText.West) };
        string dirName = "?";
        foreach (var (v, n) in names) if (v == direction) { dirName = n; break; }
        _windDirectionLabel.SetText(calm ? UIText.WindCalm : dirName);
        _windStrengthLabel.SetText(calm ? UIText.WindNone : UIText.WindStrengths[Math.Min(3, level) - 1]);
    }

    private void UpdateBossBar(float? health, float dt)
    {
        _bossGroup.Visible = health != null;
        if (health == null) return;
        float target = Math.Max(0, Math.Min(1, health.Value));
        _shownBossHealth += (target - _shownBossHealth) * Math.Min(1, dt * 8);
        float width = MathF.Round(BossBarInnerWidth * _shownBossHealth / Art, MidpointRounding.AwayFromZero) * Art;
        int first = target < 0.25f ? 3 : 1;
        for (int i = 0; i < _bossFillRows.Length; i++)
        {
            _bossFillRows[i].Size = new Vector2(width, 4);
            _bossFillRows[i].Color = FirePalette.Ramp[first + i].ToColor();
        }
    }

    // MARK: Dialogue and banner

    private static int Len(string s) => new StringInfo(s).LengthInTextElements;

    private static string Prefix(string s, int n)
    {
        var info = new StringInfo(s);
        int len = info.LengthInTextElements;
        return n >= len ? s : (n <= 0 ? "" : info.SubstringByTextElements(0, n));
    }

    /// <summary>Airu says something, one letter at a time.</summary>
    public void Say(string message)
    {
        _dialogueLines = Wrap(message, DialogueWidth, DialogueLineCount);
        _dialogueLength = _dialogueLines.Sum(Len);
        _revealed = 0;
        UpdateDialogue(0);
    }

    private void UpdateDialogue(float dt)
    {
        _revealed = Math.Min(_dialogueLength, _revealed + dt * 32);
        int remaining = (int)_revealed;
        for (int i = 0; i < _dialogueLabels.Count; i++)
        {
            string full = i < _dialogueLines.Count ? _dialogueLines[i] : "";
            _dialogueLabels[i].SetText(Prefix(full, Math.Max(0, remaining)));
            remaining -= Len(full);
        }
    }

    /// <summary>Greedy word wrap into at most <paramref name="maxLines"/> lines of <paramref name="width"/> characters.</summary>
    public static List<string> Wrap(string text, int width, int maxLines)
    {
        var lines = new List<string>();
        string current = "";
        foreach (var word in text.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.Length == 0) current = word;
            else if (Len(current) + 1 + Len(word) <= width) current += " " + word;
            else { lines.Add(current); current = word; }
        }
        if (current.Length > 0) lines.Add(current);
        return lines.Take(maxLines).ToList();
    }

    /// <summary>Shows the name of the boss's current attack on a ribbon that slides in and out, like a spell card.</summary>
    public void ShowAttackName(string name)
    {
        _bannerLabel.SetText(name);
        _banner.Visible = true;
        _banner.Position = new Vector2(560, 0);
        _banner.Modulate = Colors.White;
        _bannerActive = true;
        _bannerTime = 0;
    }

    public void HideAttackName()
    {
        _bannerActive = false;
        _banner.Visible = false;
    }

    // MARK: Controls

    private static Rect2 SpriteRect(Sprite2D s, Vector2 anchor)
    {
        var size = s.Texture.GetSize() * s.Scale;
        return new Rect2(s.Position - new Vector2(anchor.X * size.X, (1 - anchor.Y) * size.Y), size);
    }

    /// <summary>The control under <paramref name="point"/> (in the HUD's own coordinates, i.e. screen pixels), if any.</summary>
    public Control? ControlAt(Vector2 point)
    {
        if (IsEndVisible)
        {
            var local = point - _endPanel.Position;
            if (SpriteRect(_restartPlaque, new Vector2(0.5f, 1)).HasPoint(local)) return Control.Restart;
            return SpriteRect(_menuPlaque, new Vector2(0.5f, 1)).HasPoint(local) ? Control.Menu : (Control?)null;
        }
        return null;
    }

    // MARK: End screen

    public void ShowEnd(GameResult result, int score, bool newRecord)
    {
        _victoryTitle.Visible = result == GameResult.Victory;
        _defeatTitle.Visible = result == GameResult.Defeat;
        _endSubtitle.SetText("");
        string points = score.ToString("D7", CultureInfo.InvariantCulture);
        _endPoints.SetText(UIText.Score + " " + points);
        _endRecord.SetText(newRecord ? UIText.NewRecord : "");
        _endPanel.Visible = true;
    }

    public void HideEnd() => _endPanel.Visible = false;
}
