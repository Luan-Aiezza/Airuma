using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// The choice of how the boat attacks, after the difficulty: two boxes side by side, each with a picture of the attack being made (it moves), its name under it and a line about it.
/// Left and Right (or A and D) pick, Z / Enter / Space confirm, X / Escape go back; a click on a box picks and confirms it.
/// </summary>
public partial class WeaponPicker : Node2D
{
    public event Action<WeaponStyle> Chosen;
    public event Action Cancelled;

    private const float CardWidth = 470, CardHeight = 470, Gap = 36, PreviewHeight = 250, Top = 130;
    private readonly WeaponPreview[] _previews = new WeaponPreview[2];
    private readonly Node2D[] _cards = new Node2D[2];
    private readonly ColorRect[] _borders = new ColorRect[2];
    private int _selected;
    private float _clock;

    public WeaponStyle Selected => _selected == 1 ? WeaponStyle.Homing : WeaponStyle.Stream;

    public WeaponPicker(WeaponStyle current)
    {
        ZIndex = 260;
        _selected = current == WeaponStyle.Homing ? 1 : 0;
    }

    public override void _Ready()
    {
        var size = Layout.SceneSize;
        AddChild(new ColorRect { Color = new Color(0.02f, 0.03f, 0.06f, 0.82f), Size = size, MouseFilter = Control.MouseFilterEnum.Ignore });
        var title = new PixelLabel(PixelLabel.Style.MenuHeading, 5, PixelLabel.Align.Center) { Position = new Vector2(size.X / 2, 56) };
        title.SetText(UIText.WeaponTitle);
        AddChild(title);

        string[] names = { UIText.WeaponStreamName, UIText.WeaponHomingName };
        string[] details = { UIText.WeaponStreamDesc, UIText.WeaponHomingDesc };
        float x0 = (size.X - (2 * CardWidth + Gap)) / 2;
        for (int i = 0; i < 2; i++)
        {
            var card = new Node2D { Position = new Vector2(x0 + i * (CardWidth + Gap), Top) };
            _cards[i] = card;
            _borders[i] = new ColorRect { Size = new Vector2(CardWidth, CardHeight), MouseFilter = Control.MouseFilterEnum.Ignore };
            card.AddChild(_borders[i]);
            card.AddChild(new ColorRect { Color = new Color(0.05f, 0.07f, 0.11f), Size = new Vector2(CardWidth - 8, CardHeight - 8), Position = new Vector2(4, 4), MouseFilter = Control.MouseFilterEnum.Ignore });
            var preview = new WeaponPreview(i == 1 ? WeaponStyle.Homing : WeaponStyle.Stream, new Vector2(CardWidth - 40, PreviewHeight)) { Position = new Vector2(20, 20) };
            _previews[i] = preview;
            card.AddChild(preview);
            var name = new PixelLabel(PixelLabel.Style.Gold, 3, PixelLabel.Align.Center) { Position = new Vector2(CardWidth / 2, 20 + PreviewHeight + 30) };
            name.SetText(names[i]);
            card.AddChild(name);
            var lines = GameHud.Wrap(details[i], (int)((CardWidth - 48) / 12), 3);
            for (int k = 0; k < lines.Count; k++)
            {
                var line = new PixelLabel(PixelLabel.Style.Value, 2, PixelLabel.Align.Center) { Position = new Vector2(CardWidth / 2, 20 + PreviewHeight + 90 + k * 30) };
                line.SetText(lines[k]);
                card.AddChild(line);
            }
            AddChild(card);
        }
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < 2; i++)
        {
            bool on = i == _selected;
            _borders[i].Color = on ? new Color(0.95f, 0.76f, 0.31f) : new Color(0.25f, 0.30f, 0.36f);
            _cards[i].Modulate = on ? Colors.White : new Color(0.62f, 0.62f, 0.62f);
        }
    }

    public override void _Process(double delta)
    {
        _clock += (float)delta;
        // the picked box breathes a little
        float s = 1 + 0.008f * MathF.Sin(_clock * 4);
        for (int i = 0; i < 2; i++) _cards[i].Scale = i == _selected ? new Vector2(s, s) : Vector2.One;
    }

    private void Move(int step)
    {
        _selected = (_selected + step + 2) % 2;
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuMove);
        Refresh();
    }

    private void Confirm()
    {
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuSelect);
        Chosen?.Invoke(Selected);
    }

    private void Cancel()
    {
        SfxPlayer.Instance?.Play(SfxBank.Sound.MenuBack);
        Cancelled?.Invoke();
    }

    /// <summary>The keys and the mouse, forwarded by the menu. True when the event was used.</summary>
    public bool Handle(InputEvent @event)
    {
        if (@event is InputEventKey { Pressed: true, Echo: false } key)
        {
            switch (key.Keycode)
            {
                case Key.Left: case Key.A: case Key.Up: case Key.W: Move(-1); return true;
                case Key.Right: case Key.D: case Key.Down: case Key.S: Move(1); return true;
                case Key.Z: case Key.Enter: case Key.KpEnter: case Key.Space: Confirm(); return true;
                case Key.X: case Key.Escape: Cancel(); return true;
            }
        }
        else if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } click)
        {
            var p = ToLocal(click.GlobalPosition);
            for (int i = 0; i < 2; i++)
                if (new Rect2(_cards[i].Position, new Vector2(CardWidth, CardHeight)).HasPoint(p)) { _selected = i; Refresh(); Confirm(); return true; }
        }
        return false;
    }
}

/// <summary>
/// The picture of an attack in a box: the boat at the bottom, three spirits above, and the attack being made over and over. The stream is the bolts going straight up in three columns; the other attack is
/// round lights that leave to the sides and turn to the nearest spirit, which flashes when it is hit.
/// </summary>
public partial class WeaponPreview : Node2D
{
    private sealed class Shot { public Sprite2D Sprite; public Vector2 Velocity; public bool Active; public float Age; }

    private readonly WeaponStyle _style;
    private readonly Vector2 _size;
    private readonly List<Shot> _shots = new();
    private readonly List<Sprite2D> _spirits = new();
    private readonly float[] _flash = new float[3];
    private Texture2D[] _bolt, _orb;
    private Sprite2D _boat;
    private float _clock, _fire;
    private static readonly Vector2[] SpiritPlaces = { new Vector2(0.2f, 0.2f), new Vector2(0.5f, 0.12f), new Vector2(0.8f, 0.26f) };

    public WeaponPreview(WeaponStyle style, Vector2 size) { _style = style; _size = size; }

    public override void _Ready()
    {
        AddChild(new ColorRect { Color = new Color(0.03f, 0.08f, 0.13f), Size = _size, MouseFilter = Control.MouseFilterEnum.Ignore });
        AddChild(new ColorRect { Color = new Color(0.05f, 0.14f, 0.20f), Size = new Vector2(_size.X, _size.Y * 0.45f), Position = new Vector2(0, _size.Y * 0.55f), MouseFilter = Control.MouseFilterEnum.Ignore });
        _bolt = BulletArt.MakeFrames(BulletStyle.LightBolt);
        _orb = BulletArt.MakeFrames(BulletStyle.HomingOrb);
        var spirit = SpiritStyle.Forest.MakeFrames(SpiritKind.Gunner);
        foreach (var place in SpiritPlaces)
            _spirits.Add(Add(new Sprite2D { Texture = spirit[0], Scale = new Vector2(SpiritArt.Scale, SpiritArt.Scale) * 1.3f, Position = place * _size }));
        _boat = Add(new Sprite2D { Texture = PlayerArt.MakeFrames()[0], Scale = new Vector2(PlayerArt.Scale, PlayerArt.Scale), Position = new Vector2(_size.X / 2, _size.Y - 52) });
        for (int i = 0; i < 40; i++)
            _shots.Add(new Shot { Sprite = Add(new Sprite2D { Visible = false }) });
    }

    private Sprite2D Add(Sprite2D s) { AddChild(s); return s; }

    private Vector2 Muzzle => _boat.Position + new Vector2(0, -30);

    public override void _Process(double delta)
    {
        float dt = (float)Math.Min(delta, 0.05);
        _clock += dt; _fire += dt;
        int frame = (int)(_clock * 8) % 2;
        _boat.Position = new Vector2(_size.X / 2 + 6 * MathF.Sin(_clock * 1.3f), _size.Y - 52 + 2 * MathF.Sin(_clock * 3));
        for (int i = 0; i < _spirits.Count; i++)
        {
            _flash[i] = Math.Max(0, _flash[i] - dt);
            _spirits[i].Position = SpiritPlaces[i] * _size + new Vector2(0, 5 * MathF.Sin(_clock * 2 + i));
            _spirits[i].Modulate = _flash[i] > 0 ? new Color(2.2f, 2.2f, 2.2f) : Colors.White;
        }
        float interval = _style == WeaponStyle.Homing ? 0.2f : 0.09f;          // (the lights go out at half the cadence of the bolts)
        while (_fire >= interval) { _fire -= interval; Fire(); }
        foreach (var s in _shots)
        {
            if (!s.Active) continue;
            s.Age += dt;
            if (_style == WeaponStyle.Homing)
            {
                // as in the game: for two thirds of a second a push of a quarter of the speed towards the spirit, the speed held to the cruising one; then it flies straight
                int near = 0; float best = float.MaxValue;
                for (int i = 0; i < _spirits.Count; i++) { float d = _spirits[i].Position.DistanceTo(s.Sprite.Position); if (d < best) { best = d; near = i; } }
                if (s.Age < BulletSystem.AmuletSteerSeconds)
                {
                    float speed = s.Velocity.Length();
                    var v = s.Velocity + (_spirits[near].Position - s.Sprite.Position).Normalized() * (speed / 4f) * (dt * 60f);
                    s.Velocity = v.Normalized() * Math.Clamp(v.Length(), PreviewSpeed * 0.1f, PreviewSpeed);
                }
                if (best < 26) { _flash[near] = 0.12f; s.Active = false; s.Sprite.Visible = false; continue; }
                s.Sprite.Texture = _orb[frame];
            }
            else s.Sprite.Texture = _bolt[frame];
            s.Sprite.Position += s.Velocity * dt;
            if (s.Sprite.Position.Y < -20 || s.Sprite.Position.X < -20 || s.Sprite.Position.X > _size.X + 20 || s.Age > 3)
            {
                s.Active = false; s.Sprite.Visible = false;
                if (_style == WeaponStyle.Stream && s.Sprite.Position.Y < 80) for (int i = 0; i < _spirits.Count; i++) if (MathF.Abs(_spirits[i].Position.X - s.Sprite.Position.X) < 26) _flash[i] = 0.12f;
            }
        }
        // (in the stream the bolts are lost above the spirits; the ones that pass over a spirit make it flash)
        if (_style == WeaponStyle.Stream)
            foreach (var s in _shots)
                if (s.Active) for (int i = 0; i < _spirits.Count; i++) if (MathF.Abs(_spirits[i].Position.X - s.Sprite.Position.X) < 24 && MathF.Abs(_spirits[i].Position.Y - s.Sprite.Position.Y) < 24) _flash[i] = 0.1f;
    }

    private const float PreviewSpeed = 330;
    private int _volley;

    private void Fire()
    {
        if (_style == WeaponStyle.Homing)
        {
            // two lights, one along each diagonal (as in the game at level 2)
            foreach (float side in new[] { -1f, 1f })
            {
                var shot = _shots.Find(s => !s.Active);
                if (shot == null) return;
                shot.Active = true; shot.Age = 0; shot.Sprite.Visible = true;
                shot.Sprite.Scale = new Vector2(BulletArt.Scale, BulletArt.Scale) * 1.4f * 0.7f;
                shot.Sprite.Position = Muzzle + new Vector2(side * 10, 0);
                float rad = Mathx.Radians(-90 + side * BulletSystem.HomingAngle);
                shot.Velocity = new Vector2(MathF.Cos(rad), MathF.Sin(rad)) * PreviewSpeed;
            }
            _volley++;
            return;
        }
        float[] offsets = { -22, 0, 22 };
        for (int k = 0; k < 3; k++)
        {
            var shot = _shots.Find(s => !s.Active);
            if (shot == null) return;
            shot.Active = true; shot.Age = 0; shot.Sprite.Visible = true;
            shot.Sprite.Position = Muzzle + new Vector2(offsets[k], 0);
            shot.Sprite.Scale = new Vector2(BulletArt.Scale, BulletArt.Scale) * 1.7f * 0.5f;
            shot.Velocity = new Vector2(0, -430);
        }
    }
}
