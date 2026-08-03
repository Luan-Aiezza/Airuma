using Godot;
using System;

public partial class ECSManager : Node2D
{
    private PlayerEntity _player;
    private BossEntity _boss;
    private Rect2 _screenBounds;
    
    private BulletData[] _bulletPool = new BulletData[1000];
    private Texture2D _bulletTexture;
    private WindState _globalWind;

    // Variáveis da Interface de Usuário
    private Label _livesLabel;
    private Label _powerLabel;
    private Label _windLabel;
    private Label _bossHpLabel;
    private Label _bombsLabel; // NOVO

    // Variáveis da UI de Fim de Jogo
    private Panel _endGamePanel;
    private Label _endGameTitle;
    private Button _restartButton;

    public override void _Ready()
    {
        _screenBounds = new Rect2(0, 0, 600, 775);
        _bulletTexture = GD.Load<Texture2D>("res://Assets/icon.svg"); // Caminho atualizado!

        _player = new PlayerEntity
        {
            Id = 1,
            Transform = new Transform2DComponent { 
                Position = new Vector2(300, 700),
                Scale = new Vector2(0.4f, 0.4f) // <-- ADICIONE O SCALE AQUI!
            },
            Movement = new VelocityComponent { Speed = 400f },
            Input = new PlayerInputComponent(),
            Weapon = new PlayerWeaponComponent { PowerLevel = 3, FireRate = 0.1f, BulletSpeed = 800f, TimeSinceLastFire = 0f },
            Render = new RenderComponent { SpriteNode = GetNode<Sprite2D>("PlayerSprite") },
            Hitbox = new HitboxComponent { Radius = 4f },
            Health = new HealthComponent { CurrentHealth = 1, MaxHealth = 1, Lives = 3, InvincibilityTimer = 0f },
            Bomb = new BombComponent { 
                BombsLeft = 3, 
                IsActive = false, 
                Timer = 0f, 
                Duration = 5.0f, // 5 segundos de luz
                Radius = 250f    // Uma aura grande de proteção (250 pixels)
            },
            WindModifier = new WindModifierComponent { Value = 1.0f }
        };

        _boss = new BossEntity
        {
            Id = 2,
            Transform = new Transform2DComponent { 
                Position = new Vector2(300, 100),
                Scale = new Vector2(1f, 1f) // <-- ADICIONE O SCALE AQUI TAMBÉM!
            },
            Movement = new VelocityComponent { Speed = 200f },
            Render = new RenderComponent { SpriteNode = GetNode<Sprite2D>("BossSprite") },
            Hitbox = new HitboxComponent { Radius = 40f },
            Health = new HealthComponent { CurrentHealth = 100, MaxHealth = 100, Lives = 6, InvincibilityTimer = 0f },
            PatrolDirection = 1f // Estado inicial da patrulha
        };

        // Conecta os nós de interface (verifique se os nomes batem com os que você colocou na árvore)
        _livesLabel = GetNode<Label>("UI_Layer/GameUI/ColorRect/VBoxContainer/LivesLabel");
        _powerLabel = GetNode<Label>("UI_Layer/GameUI/ColorRect/VBoxContainer/PowerLabel");
        _windLabel = GetNode<Label>("UI_Layer/GameUI/ColorRect/VBoxContainer/WindLabel");
        _bossHpLabel = GetNode<Label>("UI_Layer/GameUI/ColorRect/VBoxContainer/BossHpLabel");
        _bombsLabel = GetNode<Label>("UI_Layer/GameUI/ColorRect/VBoxContainer/BombsLabel");

        // Captura o painel de fim de jogo e esconde por precaução
        _endGamePanel = GetNode<Panel>("UI_Layer/EndGamePanel");
        _endGameTitle = _endGamePanel.GetNode<Label>("TitleLabel");
        _restartButton = _endGamePanel.GetNode<Button>("RestartButton");
        _endGamePanel.Visible = false;

        // Conecta o evento de clique do botão à nossa função de reinício
        _restartButton.Pressed += OnRestartButtonPressed;

        WindSystem.RandomizeWind(ref _globalWind);
        PhaseManagerSystem.ChangePhase(_boss, _boss.Health.Lives);
        WindSystem.RandomizeWind(ref _globalWind);
    }

    public override void _PhysicsProcess(double delta)
    {
        float floatDelta = (float)delta;

        // 1. INPUTS E SPAWNERS
        MovementSystem.ProcessPlayerInput(_player);
        BulletSystem.UpdatePlayerWeapons(_player, _bulletPool, floatDelta);
        BulletSystem.UpdateBossWeapons(_boss, _player, _bulletPool, floatDelta);

        // 2. MOVIMENTAÇÃO (Passando o CurrentWindVector)
        MovementSystem.MovePlayer(_player, _screenBounds, floatDelta, _globalWind.CurrentWindVector);
        MovementSystem.MoveBoss(_boss, _screenBounds, floatDelta);
        BulletSystem.MoveBullets(_bulletPool, _screenBounds, floatDelta, _globalWind.CurrentWindVector, _player);

        // 3. PROCESSA A BOMBA DEPOIS DE MOVER
        BombSystem.ProcessBombs(_player, _bulletPool, floatDelta);
        
        // 4. COLISÕES E VIDA (Capturando a mudança de fase)
        CombatSystem.ProcessCollisions(_bulletPool, _player, _boss);
        
        bool bossPhaseChanged = CombatSystem.ProcessHealth(_player, _boss, floatDelta);
        if (bossPhaseChanged) {
            
            // NOVO: Limpa todas as balas inimigas da tela instantaneamente!
            BulletSystem.ClearBossBullets(_bulletPool);
            
            // CORREÇÃO: Removemos o 'ref _bossEmitter' daqui também!
            // Altera padrão de tiro e movimento da Boitatá!
            PhaseManagerSystem.ChangePhase(_boss, _boss.Health.Lives);
            
            // Altera a força da natureza!
            WindSystem.RandomizeWind(ref _globalWind);
        }

        if (_player.Health.Lives <= 0)
        {
            TriggerEndGame(false); // Derrota
        }
        // CORREÇÃO: Removemos o "&& _boss.Health.CurrentHealth <= 0"
        else if (_boss.Health.Lives <= 0)
        {
            TriggerEndGame(true); // Vitória
        }

        // 5. RENDERIZAÇÃO
        RenderSystem.SyncNodes(_player, _boss);
        QueueRedraw(); 

        // 6. Atualiza os textos da interface do usuário todo frame
        UISystem.UpdateUI(_player, _boss, _globalWind, _livesLabel, _powerLabel, _windLabel, _bossHpLabel, _bombsLabel);
    }

    public override void _Draw()
    {
        Vector2 scale = new Vector2(0.2f, 0.2f); 
        for (int i = 0; i < _bulletPool.Length; i++)
        {
            if (_bulletPool[i].IsActive)
            {
                Vector2 drawPos = _bulletPool[i].Position - (_bulletTexture.GetSize() * scale * _bulletPool[i].Scale / 2);
                Color tintColor = _bulletPool[i].Faction == 0 ? Colors.Aqua : Colors.OrangeRed;
                DrawTextureRect(_bulletTexture, new Rect2(drawPos, _bulletTexture.GetSize() * scale * _bulletPool[i].Scale), false, tintColor);
            }
        }
        ///TROCAR POR ASSETS DEPOIS
        if (_player.Bomb.IsActive)
        {
            // Desenha um círculo amarelo neon com um pouco de transparência ao redor do Airumã
            Color auraColor = new Color(1.0f, 1.0f, 0.5f, 0.4f); 
            DrawCircle(_player.Transform.Position, _player.Bomb.Radius, auraColor);
        }
        
    }

    private void TriggerEndGame(bool isVictory)
    {
        // Congela toda a SceneTree (interrompe o _PhysicsProcess instantaneamente)
        GetTree().Paused = true;

        // Define a mensagem e a cor com base no resultado
        if (isVictory)
        {
            _endGameTitle.Text = "VICTORY!!!\nyou win!";
            _endGameTitle.AddThemeColorOverride("font_color", Colors.Gold);
        }
        else
        {
            _endGameTitle.Text = "DEFEAT...\nyou lose!";
            _endGameTitle.AddThemeColorOverride("font_color", Colors.Red);
        }

        // Mostra o painel na tela
        _endGamePanel.Visible = true;
    }

    private void OnRestartButtonPressed()
    {
        // Remove o pause da SceneTree para que o jogo não nasça congelado
        GetTree().Paused = false;
        
        // Recarrega a cena atual inteira do zero
        GetTree().ReloadCurrentScene();
    }
}