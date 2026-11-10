using System;
using System.Linq;
using Godot;
using static ViewKit;

/// <summary>Building the nodes of the scene, and putting the simulation on them every frame.</summary>
public partial class GameScene : Node2D
{
    private void BuildWorldNodes()
    {
        River = new RiverBackground(new Vector2(Layout.RiverWidth, Layout.Height), Stage.Scenery);
        _windEffect.Style = Stage.Scenery.StarWind ? WindEffect.Kind.Stars : Stage.Scenery.BirdWind ? WindEffect.Kind.Birds : (Stage.Scenery.LanternWind ? WindEffect.Kind.Lanterns : WindEffect.Kind.Leaves);
        AddChild(River);
        BuildMirrors();

        foreach (var style in Enum.GetValues<BulletStyle>())
        {
            _bulletFrames[style] = BulletArt.MakeFrames(style);
            _bulletScale[style] = BulletArt.SpriteSize(style) / _bulletFrames[style][0].GetSize();
            _bulletRotates[(int)style] = BulletArt.Rotates(style);
        }
        for (int i = 0; i < BulletSystem.PoolSize; i++)
        {
            var node = new Sprite2D { Visible = false, ZIndex = Z(4), Name = "bullet" + i }; // above the trees, so bullets stay readable
            AddChild(node);
            _bulletNodes.Add(node);
        }

        foreach (var kind in Enum.GetValues<SpiritKind>()) _spiritFrames[kind] = Stage.Spirits.MakeFrames(kind);
        var spiritSize = Stage.Spirits.SpriteSize;
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
        _constellations.ZIndex = Z(3.9f);   // under the bullets, over everything else
        AddChild(_constellations);
        _slashes.ZIndex = Z(4.3f);          // the claws cut over the bullets
        AddChild(_slashes);
        _moon.ZIndex = Z(4.2f);             // the moon that pulls: over the bullets
        AddChild(_moon);
        _burst.ZIndex = Z(2.6f);
        AddChild(_burst);

        _wake.ZIndex = Z(-5); // on the water: above the river, below bullets and boats
        AddChild(_wake);

        _bombEffect.ZIndex = Z(1.7f); // the light is under the boat (so it stays in sight) and over the spirits
        AddChild(_bombEffect);

        _windEffect.ZIndex = Z(3.5f); // over the trees, under the bullets
        AddChild(_windEffect);

        _playerFrames = PlayerArt.MakeFrames();
        _playerSprite.Texture = _playerFrames[0];
        FitTo(_playerSprite, PlayerArt.SpriteSize);
        _playerNode.AddChild(_playerSprite);
        _playerNode.ZIndex = Z(2); // sprites draw above bullets, as in the original
        AddChild(_playerNode);
        _boss = new BossView(Stage.Boss.Style, River.MirrorLayer);
        AddChild(_boss);
        if (Stage.Boss.Style.Twin != null) { _twin = new BossView(Stage.Boss.Style.Twin, River.MirrorLayer); AddChild(_twin); }
        BuildPlayerLight();
        _wardRing.Texture = _wardFrames[0];
        _wardRing.Scale = new Vector2(3, 3);
        _wardRing.ZIndex = Z(0.3f);
        _wardRing.Modulate = new Color(1, 1, 1, 0.85f);
        _wardRing.Visible = false;
        _playerNode.AddChild(_wardRing);
        // The hitbox, shown while focusing: a turning ring of dashes and the small heart that can be hit.
        foreach (var (sprite, canvas, z) in new[] { (_focusRing, PlayerArt.MakeFocusRing(), 2.4f), (_focusCore, PlayerArt.MakeFocusCore(), 2.5f) })
        {
            sprite.Texture = canvas.ToTexture();
            sprite.Scale = new Vector2(PlayerArt.FocusScale, PlayerArt.FocusScale);
            sprite.ZIndex = Z(z);            // above the bullets, so that the heart is never hidden
            sprite.Modulate = new Color(1, 1, 1, 0);
            sprite.Visible = false;
            _playerNode.AddChild(sprite);
        }
    }

    /// <summary>A flipped copy of the boat, with its light, drawn into the water's reflection, mirrored about a line under the stern.
    /// (The boss's reflection is made by its own view.)</summary>
    private void BuildMirrors()
    {
        var pairs = new[] { (_boatMirror, _boatMirrorSprite, _boatMirrorAura) };
        foreach (var (container, sprite, light) in pairs)
        {
            container.Scale = new Vector2(1, -1);
            container.ZIndex = Z(0.2f);
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

    private void SyncMirrors()
    {
        var boat = _playerNode.Position;
        Attach(_boatMirror, River.MirrorLayer, CanReflect(boat));
        _boatMirror.Position = new Vector2(boat.X, boat.Y + PlayerArt.SpriteSize.Y);
        _boatMirror.Visible = _playerNode.Visible;
        _boatMirrorSprite.Texture = _playerSprite.Texture;
        _boatMirrorAura.Texture = _playerAura.Texture;
        FitTo(_boatMirrorAura, new Vector2(PlayerArt.AuraSize * PlayerArt.Scale, PlayerArt.AuraSize * PlayerArt.Scale));

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

    /// <summary>The boss's view is told what the boss is doing; the river takes the tint of its rage.</summary>
    private void SyncBoss()
    {
        _boss.Update(new BossFrame
        {
            Dt = _bombDelta, Boss = World.Boss, Phase = CurrentPhase, Fighting = World.Part == StagePart.Boss, Over = World.Result != null,
            Scripted = _bossSequence != BossSequence.None, Fused = World.Twin != null && World.Twin.Merged,
        });
        _twin?.Update(new BossFrame
        {
            Dt = _bombDelta, Boss = World.Twin, Phase = CurrentPhase, Fighting = World.Part == StagePart.Boss, Over = World.Result != null,
            Scripted = _bossSequence != BossSequence.None, Hidden = World.Twin.Merged,
        });
        River.Modulate = Colors.White.Lerp(_boss.Style.SceneTint, _boss.RageAmount);
        World.HorizonOpen = River.HorizonAmount;
        World.Rock = River.RockNow;
        World.FieldOpen = River.OpenAmount;       // the banks have gone: the boat has their room
    }

    private void SyncNodes()
    {
        var player = World.Player; var boss = World.Boss;

        _playerNode.Position = Point(player.Transform.Position);
        _playerNode.Scale = player.Transform.Scale;
        _playerNode.Visible = player.IsVisible;
        _wardRing.Visible = player.Shield > 0;
        _focusAlpha = Mathf.MoveToward(_focusAlpha, _sampledInput.IsFocused && player.IsVisible ? 1 : 0, _bombDelta * 8);
        _focusRing.Visible = _focusCore.Visible = _focusAlpha > 0;
        _focusRing.Modulate = _focusCore.Modulate = new Color(1, 1, 1, _focusAlpha);
        _focusRing.Rotation += _bombDelta * 1.6f;
        _focusRing.Scale = new Vector2(PlayerArt.FocusScale, PlayerArt.FocusScale) * (1.25f - 0.25f * _focusAlpha);   // it closes in as it appears
        if (player.Shield > 0)
        {
            _wardRing.Texture = _wardFrames[(int)(_animationClock * 6) % 2];
            float s = 3 * (1 + 0.12f * (Math.Min(player.Shield, 3) - 1));
            _wardRing.Scale = new Vector2(s, s);
        }

        SyncBoss();
        SyncMirrors();
        _constellations.Update(_bombDelta, BulletSystem.Links(World.Bullets));
        _slashes.Show(World.Slashes, _bombDelta);
        _moon.Show(World.Moon, World.Player.Transform.Position, _bombDelta);
        // The quake: the whole picture shakes, more and less.
        float shake = Math.Max(World.Shake, Math.Min(1, _tremor));
        Position = shake > 0 ? new Vector2((_rng.Randf() - 0.5f) * 10 * shake, (_rng.Randf() - 0.5f) * 10 * shake) : Vector2.Zero;

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
            if (!bullet.IsActive)
            {
                if (_bulletKey[i] != 0) { node.Visible = false; _bulletKey[i] = 0; }
                continue;
            }
            node.Position = Point(bullet.Position);
            // (a thousand bullets or more are on the screen in the late fights: what has not changed is not set again)
            var frames = _bulletFrames[bullet.Style];
            int frame = bulletFrame % frames.Length;
            int key = 1 + (((int)bullet.Style * frames.Length + frame) << 8) + (int)(bullet.Zoom * 10);
            if (_bulletKey[i] != key)
            {
                _bulletKey[i] = key;
                node.Visible = true;
                node.Texture = frames[frame];
                node.Scale = bullet.Zoom == 1 ? _bulletScale[bullet.Style] : _bulletScale[bullet.Style] * bullet.Zoom;
                if (!_bulletRotates[(int)bullet.Style]) node.Rotation = 0;
            }
            // Keep the last heading while a bullet is (almost) stopped, e.g. braking lotus petals.
            if (_bulletRotates[(int)bullet.Style] && bullet.Velocity.LengthSquared() > 1) node.Rotation = BulletArt.GodotRotation(bullet.Velocity);
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
            PhasesCleared = boss.Definition.Lives - Math.Max(0, boss.Health.Lives),
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
            float height = Stage.Spirits.SpriteSize.Y;
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
            node.Material = item.Kind == LightKind.Energy ? null : _lightAdd;               // the power squares are drawn plain; the spare lantern and boat glow
            if (item.Kind == LightKind.Bomb || item.Kind == LightKind.Life)
            {
                node.Texture = (item.Kind == LightKind.Bomb ? _bombFrames : _lifeFrames)[(frame / 2 + i) % _bombFrames.Length];
                FitTo(node, new Vector2(LightArt.BombSize * LightArt.Scale, LightArt.BombSize * LightArt.Scale));
            }
            else
            {
                int size = LightSystem.Size(item.Value);
                node.Texture = _powerFrames[size][(frame / 2 + i) % _powerFrames[size].Length];
                float side = LightArt.PowerPixels(size);
                FitTo(node, new Vector2(side, side));
            }
            node.Position = Point(item.Position);
            node.Visible = true;
        }
    }
}
