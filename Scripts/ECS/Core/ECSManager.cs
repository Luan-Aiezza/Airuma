using Godot;
using System;

public partial class ECSManager : Node2D
{
    private PlayerEntity _player;
    private BossEntity _boss;
    private Rect2 _screenBounds;
    
    private BulletData[] _bulletPool = new BulletData[1000];
    private Texture2D _bulletTexture;
    private BulletEmitterComponent _bossEmitter;

    public override void _Ready()
    {
        _screenBounds = new Rect2(0, 0, 600, 800);
        _bulletTexture = GD.Load<Texture2D>("res://Assets/icon.svg"); // Caminho atualizado!

        _player = new PlayerEntity
        {
            Id = 1,
            Transform = new Transform2DComponent { Position = new Vector2(300, 700) },
            Movement = new VelocityComponent { Speed = 400f },
            Input = new PlayerInputComponent(),
            Weapon = new PlayerWeaponComponent { PowerLevel = 3, FireRate = 0.1f, BulletSpeed = 800f, TimeSinceLastFire = 0f },
            Render = new RenderComponent { SpriteNode = GetNode<Sprite2D>("PlayerSprite") },
            Hitbox = new HitboxComponent { Radius = 4f },
            Health = new HealthComponent { CurrentHealth = 1, MaxHealth = 1, Lives = 3, InvincibilityTimer = 0f }
        };

        _boss = new BossEntity
        {
            Id = 2,
            Transform = new Transform2DComponent { Position = new Vector2(300, 100) },
            Movement = new VelocityComponent { Speed = 200f },
            Render = new RenderComponent { SpriteNode = GetNode<Sprite2D>("BossSprite") },
            Hitbox = new HitboxComponent { Radius = 40f },
            Health = new HealthComponent { CurrentHealth = 50, MaxHealth = 300, Lives = 1, InvincibilityTimer = 0f },
            PatrolDirection = 1f // Estado inicial da patrulha
        };

        _bossEmitter = new BulletEmitterComponent
        {
            FireRate = 1.0f, TimeSinceLastFire = 0f,
            BulletsPerVolley = 36, SpreadAngle = 360f, BaseAngle = 0f, BulletSpeed = 150f
        };
    }

    public override void _PhysicsProcess(double delta)
    {
        float floatDelta = (float)delta;

        // 1. INPUTS E SPAWNERS
        MovementSystem.ProcessPlayerInput(_player);
        BulletSystem.UpdatePlayerWeapons(_player, _bulletPool, floatDelta);
        BulletSystem.UpdateBossWeapons(_boss, ref _bossEmitter, _bulletPool, floatDelta);

        // 2. MOVIMENTAÇÃO
        MovementSystem.MovePlayer(_player, _screenBounds, floatDelta);
        MovementSystem.MoveBoss(_boss, _screenBounds, floatDelta);
        BulletSystem.MoveBullets(_bulletPool, _screenBounds, floatDelta);
        
        // 3. COLISÕES E VIDA
        CombatSystem.ProcessCollisions(_bulletPool, _player, _boss);
        CombatSystem.ProcessHealth(_player, _boss, floatDelta);

        // 4. RENDERIZAÇÃO
        RenderSystem.SyncNodes(_player, _boss);
        QueueRedraw(); 
    }

    public override void _Draw()
    {
        Vector2 scale = new Vector2(0.2f, 0.2f); 
        for (int i = 0; i < _bulletPool.Length; i++)
        {
            if (_bulletPool[i].IsActive)
            {
                Vector2 drawPos = _bulletPool[i].Position - (_bulletTexture.GetSize() * scale / 2);
                Color tintColor = _bulletPool[i].Faction == 0 ? Colors.Aqua : Colors.OrangeRed;
                DrawTextureRect(_bulletTexture, new Rect2(drawPos, _bulletTexture.GetSize() * scale), false, tintColor);
            }
        }
    }
}