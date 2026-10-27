using System;
using System.Linq;
using Godot;

/// <summary>Building the nodes of the scene, and putting the simulation on them every frame.</summary>
public partial class GameScene : Node2D
{
    private void BuildWorldNodes()
    {
        River = new RiverBackground(new Vector2(Layout.RiverWidth, Layout.Height));
        AddChild(River);
        BuildMirrors();

        foreach (var style in Enum.GetValues<BulletStyle>())
        {
            _bulletFrames[style] = BulletArt.MakeFrames(style);
            _bulletScale[style] = BulletArt.SpriteSize(style) / _bulletFrames[style][0].GetSize();
        }
        for (int i = 0; i < BulletSystem.PoolSize; i++)
        {
            var node = new Sprite2D { Visible = false, ZIndex = Z(4), Name = "bullet" + i }; // above the trees, so bullets stay readable
            AddChild(node);
            _bulletNodes.Add(node);
        }

        foreach (var kind in Enum.GetValues<SpiritKind>()) _spiritFrames[kind] = SpiritArt.MakeFrames(kind);
        var spiritSize = new Vector2(SpiritArt.Width * SpiritArt.Scale, SpiritArt.Height * SpiritArt.Scale);
        for (int i = 0; i < 48; i++)
        {
            var node = new Sprite2D { Texture = _spiritFrames[SpiritKind.Swarm][0], Visible = false, ZIndex = Z(1.5f), Material = NewTintMaterial(Colors.White, 0) }; // above the water, below the boat and the trees
            FitTo(node, spiritSize);
            AddChild(node);
            _enemyNodes.Add(node);

            var mirror = new Sprite2D
            {
                Texture = node.Texture, Visible = false, ZIndex = Z(0.1f), // in the reflection: under the boat's (0.2) and the boss's (0)
                Modulate = new Color(1, 1, 1, 0.55f), Material = NewTintMaterial(MirrorTint, 0.3f),
            };
            mirror.Scale = new Vector2(node.Scale.X, -node.Scale.Y);
            River.MirrorLayer.AddChild(mirror);
            _enemyMirrors.Add(mirror);
        }
        var lightSize = new Vector2(LightArt.Size * LightArt.Scale, LightArt.Size * LightArt.Scale);
        for (int i = 0; i < LightSystem.Capacity; i++)
        {
            var node = new Sprite2D { Texture = _lightFrames[0], Material = AddMaterial(), ZIndex = Z(1.8f), Visible = false }; // over the spirits, under the boat
            FitTo(node, lightSize);
            AddChild(node);
            _lightNodes.Add(node);
        }
        _burst.ZIndex = Z(2.6f);
        AddChild(_burst);

        _wake.ZIndex = Z(-5); // on the water: above the river, below bullets and boats
        AddChild(_wake);

        _bombEffect.ZIndex = Z(1.7f); // the light is under the boat (so it stays in sight) and over the spirits
        AddChild(_bombEffect);

        _bossFrames[BossLook.Woman] = BossArt.MakeFrames(BossForm.Human);
        _bossFrames[BossLook.Serpent] = BossArt.MakeFrames(BossForm.Serpent);
        _bossFrames[BossLook.Charging] = BossArt.MakeFrames(BossForm.Serpent, true);
        _flameFrames[BossLook.Serpent] = BossArt.MakeFlameFrames(false);
        _flameFrames[BossLook.Charging] = BossArt.MakeFlameFrames(true);
        foreach (var form in new[] { BossForm.Human, BossForm.Serpent }) _bossGlowFrames[form] = BossArt.MakeGlowFrames(form);

        _windEffect.ZIndex = Z(3.5f); // over the trees, under the bullets
        AddChild(_windEffect);

        _serpentBody.ZIndex = Z(1.8f); // under the head, over the water, under the trees
        AddChild(_serpentBody);

        // Boss: a container at the boss's position; the sprite is anchored on it, the glow sits under the sprite.
        _bossGlow.Material = AddMaterial();
        _bossGlow.ZIndex = Z(-0.2f);
        _bossNode.AddChild(_bossGlow);
        _bossSprite.Centered = false;
        _bossSprite.Scale = new Vector2(BossArt.Scale, BossArt.Scale);
        _bossSprite.Material = NewTintMaterial(Colors.White, 0);
        _bossNode.AddChild(_bossSprite);
        _bossSprite.AddChild(_bossFlame);
        _bossFlame.Tune(44, 1.3f, 10); // the head is a pyre

        _playerFrames = PlayerArt.MakeFrames();
        _playerSprite.Texture = _playerFrames[0];
        FitTo(_playerSprite, PlayerArt.SpriteSize);
        _playerNode.AddChild(_playerSprite);
        foreach (var node in new[] { _playerNode, _bossNode })
        {
            node.ZIndex = Z(2); // sprites draw above bullets, as in the original
            AddChild(node);
        }
        BuildPlayerLight();
        _wardRing.Texture = _wardFrames[0];
        _wardRing.Scale = new Vector2(3, 3);
        _wardRing.ZIndex = Z(0.3f);
        _wardRing.Modulate = new Color(1, 1, 1, 0.85f);
        _wardRing.Visible = false;
        _playerNode.AddChild(_wardRing);
    }

    /// <summary>Flipped copies of the boat and the boss, with their lights, drawn into the water's reflection. Each is
    /// mirrored about a line under the actor (the stern; the waterline of the woman; below the serpent's head).</summary>
    private void BuildMirrors()
    {
        var pairs = new[] { (_boatMirror, _boatMirrorSprite, _boatMirrorAura), (_bossMirror, _bossMirrorSprite, _bossMirrorGlow) };
        foreach (var (container, sprite, light) in pairs)
        {
            container.Scale = new Vector2(1, -1);
            container.ZIndex = container == _boatMirror ? Z(0.2f) : 0; // explicit: equal z would flicker
            sprite.ZIndex = 0;
            sprite.Material = NewTintMaterial(MirrorTint, 0.3f);
            sprite.Modulate = new Color(1, 1, 1, 0.6f);
            light.Modulate = new Color(1, 1, 1, 0.7f);
            light.Material = AddMaterial();
            light.ZIndex = Z(-0.2f);
            container.AddChild(sprite);
            container.AddChild(light);
            River.MirrorLayer.AddChild(container);
        }
        _boatMirrorSprite.Texture = _playerSprite.Texture ?? PlayerArt.MakeFrames()[0];
        FitTo(_boatMirrorSprite, PlayerArt.SpriteSize);
        _boatMirrorAura.Position = PlayerArt.LanternOffset;
    }

    /// <summary>Whether a mirrored actor may be part of the reflection (the defeated boss is parked at -9999).</summary>
    private static bool CanReflect(Vector2 point)
    {
        const float slack = 400;
        return point.X > -slack && point.X < Layout.RiverWidth + slack && point.Y > -slack && point.Y < Layout.Height + slack;
    }

    private static void Attach(Node2D mirror, Node parent, bool wanted)
    {
        if (wanted) { if (mirror.GetParent() == null) parent.AddChild(mirror); }
        else if (mirror.GetParent() != null) mirror.GetParent().RemoveChild(mirror);
    }

    private void SyncMirrors()
    {
        var boat = _playerNode.Position;
        Attach(_boatMirror, River.MirrorLayer, CanReflect(boat));
        _boatMirror.Position = new Vector2(boat.X, boat.Y + PlayerArt.SpriteSize.Y);
        _boatMirror.Visible = _playerNode.Visible;
        _boatMirrorSprite.Texture = _playerSprite.Texture;
        _boatMirrorAura.Texture = _playerAura.Texture;
        FitTo(_boatMirrorAura, new Vector2(PlayerArt.AuraSize * PlayerArt.Scale, PlayerArt.AuraSize * PlayerArt.Scale));

        // The woman is mirrored about the water line at her toes; the serpent casts no reflection.
        float waterline = BossArt.Waterline(_bossForm ?? BossForm.Human);
        Attach(_bossMirror, River.MirrorLayer,
            World.Part == StagePart.Boss && _bossForm == BossForm.Human && World.Boss.Health.Lives > 0 && CanReflect(_bossNode.Position));
        _bossMirror.Position = new Vector2(_bossNode.Position.X, _bossNode.Position.Y + 2 * waterline);
        _bossMirrorSprite.Texture = _bossSprite.Texture;
        _bossMirrorSprite.Centered = false;
        _bossMirrorSprite.Scale = _bossSprite.Scale;
        _bossMirrorSprite.Offset = _bossSprite.Offset;
        _bossMirrorSprite.Modulate = new Color(1, 1, 1, 0.6f * _bossNode.Modulate.A);
        _bossMirrorGlow.Texture = _bossGlow.Texture;
        _bossMirrorGlow.Scale = _bossGlow.Scale;
    }

    /// <summary>The lantern's glow is a child of the boat, so it follows it and blinks with it. It is drawn additively
    /// just under the boat sprite and flickers between two sizes.</summary>
    private void BuildPlayerLight()
    {
        _auraFrames = PlayerArt.MakeAuraFrames();
        _playerAura = new Sprite2D { Texture = _auraFrames[0], Position = PlayerArt.LanternOffset, ZIndex = Z(-0.2f), Material = AddMaterial() };
        FitTo(_playerAura, new Vector2(PlayerArt.AuraSize * PlayerArt.Scale, PlayerArt.AuraSize * PlayerArt.Scale));
        _playerNode.AddChild(_playerAura);
        _auraFrame = 0; _auraWait = 0.3f + _rng.RandfRange(-0.1f, 0.1f);
    }

    /// <summary>A time to wait with some randomness: duration, give or take half of range.</summary>
    private float Wait(float duration, float range) => duration + _rng.RandfRange(-range / 2, range / 2);

    private void UpdateAnimations(float dt)
    {
        // Aura: frame 1 for 0.12 (+-0.05), then frame 0 for 0.3 (+-0.1).
        _auraTimer += dt;
        while (_auraTimer >= _auraWait)
        {
            _auraTimer -= _auraWait;
            _auraFrame = 1 - _auraFrame;
            _auraWait = _auraFrame == 1 ? Wait(0.12f, 0.1f) : Wait(0.3f, 0.2f);
            _playerAura.Texture = _auraFrames[_auraFrame];
        }
        _playerSprite.Texture = _playerFrames[(int)(_clock / 0.35) % _playerFrames.Length];

        if (_bossLook is BossLook look && _bossForm is BossForm form)
        {
            _bossAnim += dt;
            var frames = _bossFrames[look];
            int frameIndex = (int)(_bossAnim / 0.18f) % frames.Length;
            _bossSprite.Texture = frames[frameIndex];
            if (_flameFrames.TryGetValue(look, out var flames)) _bossFlame.Texture = flames[frameIndex];
            var glow = _bossGlowFrames[form];
            _glowTimer += dt;
            while (_glowTimer >= _glowWait)
            {
                _glowTimer -= _glowWait;
                _glowFrame = 1 - _glowFrame;
                _glowWait = _glowFrame == 1 ? Wait(0.14f, 0.1f) : Wait(0.3f, 0.2f);
                _bossGlow.Texture = glow[_glowFrame];
            }
        }

        if (_bossFlash >= 0)
        {
            _bossFlash += dt;
            float t = Math.Min(1, _bossFlash / 0.6f);
            _bossNode.Modulate = new Color(1, 1, 1, 0.2f + 0.8f * t);
            SetBlend(_bossSprite, 1 - t);
            if (t >= 1) _bossFlash = -1;
        }
    }

    /// <summary>Switches the boss between the woman and the serpent: new art, size and glow, with a white flash
    /// and fade-in when it changes mid-fight.</summary>
    private void ApplyBossLook(BossLook look)
    {
        var form = look == BossLook.Woman ? BossForm.Human : BossForm.Serpent;
        bool transforming = _bossForm != null && _bossForm != form;
        _bossForm = form;
        _bossLook = look;
        _hud?.SetBossName(World.Boss.Definition.Phases.First(p => p.Form == form).Name);

        var frames = _bossFrames[look];
        _bossAnim = 0;
        _bossSprite.Texture = frames[0];
        _bossSprite.Scale = new Vector2(BossArt.ScaleFor(form), BossArt.ScaleFor(form));
        _bossSprite.Offset = -frames[0].GetSize() * BossArt.Anchor(form);
        _bossFlame.Visible = _flameFrames.ContainsKey(look);
        if (_bossFlame.Visible) { _bossFlame.Texture = _flameFrames[look][0]; _bossFlame.Align(_bossSprite.Offset); }

        var glowFrames = _bossGlowFrames[form];
        float glowSide = (form == BossForm.Human ? BossArt.HumanGlowSize : BossArt.SerpentGlowSize) * BossArt.ScaleFor(form);
        _glowFrame = 0; _glowTimer = 0; _glowWait = Wait(0.3f, 0.2f);
        _bossGlow.Texture = glowFrames[0];
        FitTo(_bossGlow, new Vector2(glowSide, glowSide));
        _bossGlow.Position = Vector2.Zero;
        _bossGlow.Visible = form == BossForm.Human; // the serpent is lit by its own flames

        if (transforming)
        {
            _bossFlash = 0;
            _bossNode.Modulate = new Color(1, 1, 1, 0.2f);
            SetBlend(_bossSprite, 1);
        }
    }

    private void BuildJoystick()
    {
        _joystickBase = new Disc(50) { ZIndex = Z(5), Visible = false };
        _joystickKnob = new Disc(20) { ZIndex = Z(5), Visible = false };
        AddChild(_joystickBase);
        AddChild(_joystickKnob);
    }

    /// <summary>The virtual joystick's circles (white outline at 0.35, fill at 0.12).</summary>
    private partial class Disc : Node2D
    {
        private readonly float _radius;
        public Disc(float radius) { _radius = radius; }
        public override void _Draw()
        {
            DrawCircle(Vector2.Zero, _radius, new Color(1, 1, 1, 0.12f));
            DrawArc(Vector2.Zero, _radius, 0, Mathf.Tau, 48, new Color(1, 1, 1, 0.35f), 2, true);
        }
    }

    // MARK: Rendering

    private void SyncNodes()
    {
        var player = World.Player; var boss = World.Boss;

        _playerNode.Position = Point(player.Transform.Position);
        _playerNode.Scale = player.Transform.Scale;
        _playerNode.Visible = player.IsVisible;
        _wardRing.Visible = player.Shield > 0;
        if (player.Shield > 0)
        {
            _wardRing.Texture = _wardFrames[(int)(_animationClock * 6) % 2];
            float s = 3 * (1 + 0.12f * (Math.Min(player.Shield, 3) - 1));
            _wardRing.Scale = new Vector2(s, s);
        }

        _bossNode.Position = Point(boss.Transform.Position) + _bossShake;
        var look = boss.Form == BossForm.Human || _morphHoldWoman ? BossLook.Woman : (CurrentPhase.Movement == BossMovementType.Dash ? BossLook.Charging : BossLook.Serpent);
        if (_bossLook != look) ApplyBossLook(look);

        SyncMirrors();

        var bomb = player.Bomb;
        // The bomb is the lantern's light, so it is centred on the lantern, not on the middle of the boat.
        var lantern = _playerNode.Position + PlayerArt.LanternOffset;
        _bombEffect.Update(_bombDelta, bomb.IsActive, bomb.Timer, lantern);

        int bulletFrame = (int)(_clock * 8);
        var bullets = World.Bullets;
        for (int i = 0; i < bullets.Length; i++)
        {
            var node = _bulletNodes[i];
            ref var bullet = ref bullets[i];
            if (!bullet.IsActive) { node.Visible = false; continue; }
            node.Visible = true;
            node.Position = Point(bullet.Position);
            var frames = _bulletFrames[bullet.Style];
            node.Texture = frames[bulletFrame % frames.Length];
            node.Scale = _bulletScale[bullet.Style];
            if (BulletArt.Rotates(bullet.Style))
            {
                // Keep the last heading while a bullet is (almost) stopped, e.g. braking lotus petals.
                if (bullet.Velocity.Length() > 1) node.Rotation = BulletArt.GodotRotation(bullet.Velocity);
            }
            else node.Rotation = 0;
        }

        if (_input.JoystickOrigin is Vector2 origin)
        {
            _joystickBase.Visible = true;
            _joystickKnob.Visible = true;
            _joystickBase.Position = origin;
            _joystickKnob.Position = _input.JoystickKnob;
        }
        else
        {
            _joystickBase.Visible = false;
            _joystickKnob.Visible = false;
        }
    }

    private void UpdateHUD(float dt)
    {
        var player = World.Player; var boss = World.Boss; var wind = World.Wind;
        _hud.Update(new HudState
        {
            Lives = player.Health.Lives,
            Bombs = player.Bomb.BombsLeft,
            Power = player.Weapon.PowerLevel,
            Score = World.Score,
            HiScore = HighScore.Current,
            WindDirection = wind.Direction,
            WindStrength = wind.Strength,
            BossHealth = World.Part == StagePart.Boss && _bossSequence == BossSequence.None && boss.Health.Lives > 0
                ? (float)Math.Max(0, boss.Health.CurrentHealth) / boss.Health.MaxHealth : (float?)null,
            PhasesCleared = GameHud.TotalPhases - Math.Max(0, boss.Health.Lives),
            PowerProgress = PowerCurve.Progress(player.Energy),
            Cards = World.Run.History,
        }, dt);
    }

    /// <summary>Draws the spirits: same sprite for all, bobbing tails, a white flash when hit.</summary>
    private void SyncEnemies()
    {
        int frame = (int)(_animationClock * 10);
        for (int i = 0; i < _enemyNodes.Count; i++)
        {
            var node = _enemyNodes[i]; var mirror = _enemyMirrors[i];
            if (i >= World.Enemies.Count) { node.Visible = false; mirror.Visible = false; continue; }
            var enemy = World.Enemies[i];
            var frames = _spiritFrames[enemy.Kind];
            node.Texture = frames[(frame + enemy.Id) % frames.Length];
            node.Position = Point(enemy.Position);
            SetBlend(node, enemy.HitFlash > 0 ? 0.7f : 0);
            node.Visible = true;

            // The spirit hovers a little above the water; its reflection is as far under the surface as it is above it.
            const float hover = 16;
            float height = SpiritArt.Height * SpiritArt.Scale;
            mirror.Texture = node.Texture;
            bool reflect = CanReflect(node.Position);
            mirror.Visible = reflect;
            if (reflect) mirror.Position = new Vector2(node.Position.X, node.Position.Y + height + 2 * hover);
        }
    }

    /// <summary>Draws the little lights falling through the picture.</summary>
    private void SyncLights()
    {
        int frame = (int)(_animationClock * 8);
        for (int i = 0; i < _lightNodes.Count; i++)
        {
            var node = _lightNodes[i]; var item = World.Lights[i];
            if (!item.IsActive) { node.Visible = false; continue; }
            if (item.Kind == LightKind.Bomb || item.Kind == LightKind.Life)
            {
                node.Texture = (item.Kind == LightKind.Bomb ? _bombFrames : _lifeFrames)[(frame / 2 + i) % _bombFrames.Length];
                FitTo(node, new Vector2(LightArt.BombSize * LightArt.Scale, LightArt.BombSize * LightArt.Scale));
            }
            else
            {
                node.Texture = _lightFrames[(frame + i) % _lightFrames.Length];
                FitTo(node, new Vector2(LightArt.Size * LightArt.Scale, LightArt.Size * LightArt.Scale));
            }
            node.Position = Point(item.Position);
            node.Visible = true;
        }
    }
}
