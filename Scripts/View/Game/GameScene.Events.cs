using System.Collections.Generic;
using System.Linq;
using Godot;
using static ViewKit;

/// <summary>What the simulation reports, and the scene's reactions: sounds, speech, the end of the run.</summary>
public partial class GameScene : Node2D
{
    /// <summary>Things the world reports: spirits defeated, the boss arriving.</summary>
    private void HandleWorldEvents()
    {
        foreach (var e in World.TakeEvents())
        {
            switch (e.Kind)
            {
                case WorldEventKind.EnemyDefeated:
                    _burst.Burst(Point(e.Position), e.Spirit);
                    SfxPlayer.Instance?.Play(SfxBank.ForDefeat(e.Spirit));
                    break;
                case WorldEventKind.BulletsFired: SfxPlayer.Instance?.PlayBullet(e.Style); break;
                case WorldEventKind.PlayerHit: SfxPlayer.Instance?.Play(SfxBank.Sound.PlayerHit); break;
                case WorldEventKind.BossBroken: SfxPlayer.Instance?.Play(SfxBank.Sound.BossBroken); break;
                case WorldEventKind.BossEntered:
                    BeginBossCutscene();
                    break;
                case WorldEventKind.PowerUp: SfxPlayer.Instance?.Play(SfxBank.Sound.LevelUp); _hud.Say(e.Int >= PowerCurve.MaxLevel ? UIText.SayFullPower : UIText.SayPowerUp); break;
                case WorldEventKind.CardTaken: _hud.Say(UIText.SayCard); break;
                case WorldEventKind.WardBroke: SfxPlayer.Instance?.Play(SfxBank.Sound.WardBroke); _hud.Say(UIText.SayWard); break;
                case WorldEventKind.LifeRefilled: SfxPlayer.Instance?.Play(SfxBank.Sound.LevelUp); _hud.Say(UIText.SayLifeRefill); break;
                case WorldEventKind.BombRefilled: SfxPlayer.Instance?.Play(SfxBank.Sound.LevelUp); _hud.Say(UIText.SayBombRefill); break;
                case WorldEventKind.EnemyHit:
                    SfxPlayer.Instance?.Play(e.Spirit == SpiritKind.Heavy ? SfxBank.Sound.HitHeavy : SfxBank.Sound.HitGunner);
                    break;
                case WorldEventKind.BossHit: SfxPlayer.Instance?.Play(SfxBank.Sound.HitBoss); break;
                case WorldEventKind.SlashStruck: SfxPlayer.Instance?.Play(SfxBank.Sound.Slash); break;
                case WorldEventKind.GravityPulled: SfxPlayer.Instance?.Play(SfxBank.Sound.Gravity); break;
                case WorldEventKind.QuakeStarted: SfxPlayer.Instance?.Play(SfxBank.Sound.Quake); break;
                case WorldEventKind.Roared:
                    SfxPlayer.Instance?.Play(SfxBank.Sound.Roar);
                    AddChild(new RoarWave { Position = Point(e.Position), ZIndex = Z(4.2f) });
                    break;
                case WorldEventKind.BombFired: SfxPlayer.Instance?.Play(SfxBank.Sound.Bomb); break;
                case WorldEventKind.LightGathered: SfxPlayer.Instance?.Play(SfxBank.Sound.Pickup); break;
                case WorldEventKind.TimerTick: SfxPlayer.Instance?.Play(SfxBank.Sound.Tick); break;
                case WorldEventKind.AttackBonus: _hud.Flash(UIText.AttackBonus + " +" + e.Int.ToString(System.Globalization.CultureInfo.InvariantCulture)); break;
                case WorldEventKind.TimeUp: break;
                case WorldEventKind.WaveStarted: break;
            }
        }
    }

    /// <summary>Airu reacts to what happens, and the boss's attack names appear as it changes phase.</summary>
    private void TrackEvents()
    {
        var player = World.Player; var boss = World.Boss;
        if (player.Health.Lives < _lastLives && player.Health.Lives > 0) _hud.Say(UIText.SayHit);
        if (player.Bomb.BombsLeft < _lastBombs) _hud.Say(UIText.SayBomb);
        if (World.Wind.Direction != _lastWind.direction || World.Wind.Strength != _lastWind.strength)
        {
            _lastWind = (World.Wind.Direction, World.Wind.Strength);
            _windEffect.Announce(); // the wind changed: show where it blows now
            _hud.PulseWind();
        }
        if (boss.Health.Lives < _lastBossLives && boss.Health.Lives > 0 && World.Result == null)      // (not when a rehearsal of one attack has just ended: it would start the next one over the end screen)
        {
            // The first enraged attack is a bigger moment (other music, rings, a flash); any other is just a new attack.
            bool becomesEnraged = CurrentPhase.Enraged && !boss.Definition.PhaseFor(_lastBossLives).Enraged && World.Part == StagePart.Boss;
            _hud.Say(becomesEnraged ? UIText.SayRage : UIText.SayEnraged);
            if (becomesEnraged) BeginEnrage();
            else _hud.ShowAttackName(CurrentPhase.AttackName ?? "");
        }
        _lastLives = player.Health.Lives;
        _lastBombs = player.Bomb.BombsLeft;
        _lastBossLives = boss.Health.Lives;
    }

    /// <summary>A real game (not a practice, not the demonstration) has only three restarts in all, and only in the easy mode.</summary>
    private bool CountsRestarts => PracticePhase == null && !StagePractice && !Demo;
    /// <summary>The normal and the hard modes have no restart at all: losing goes straight to Game Over.</summary>
    private bool NoRestart => Difficulty != Difficulty.Easy && CountsRestarts;
    private bool CanRestart => !CountsRestarts || (!NoRestart && RunRestarts.Left > 0);

    /// <summary>When the end screen appeared, and how long it ignores the keys and clicks: a player who is mashing Z or X when the boat goes down must not skip it, or burn a restart, by accident.</summary>
    private double _endShownAt;
    private const double EndInputLock = 0.7;
    private bool EndInputReady => _clock - _endShownAt >= EndInputLock;

    /// <summary>The score of a defeat that may still be followed by a restart: it is recorded only if the player leaves for the menu instead (see <see cref="DoExit"/>).</summary>
    private int? _pendingScore;

    private ColorRect _gameOver;
    private bool _gameOverKeyWasDown;
    private float _gameOverClock;

    private void BeginGameOver()
    {
        _gameOver = new ColorRect { Color = new Color(0, 0, 0, 0), Size = Layout.SceneSize, ZIndex = 300, MouseFilter = Control.MouseFilterEnum.Ignore };
        var label = new PixelLabel(PixelLabel.Style.Defeat, 10, PixelLabel.Align.Center) { Position = new Vector2(Layout.SceneSize.X / 2, 330) };
        label.SetText(UIText.GameOver);
        _gameOver.AddChild(label);
        AddChild(_gameOver);
        _gameOverClock = 0;
        _gameOverKeyWasDown = true;
        MusicPlayer.Instance?.Play(MusicPlayer.Theme.GameOver);
    }

    private void UpdateGameOver(float dt)
    {
        _gameOverClock += dt;
        _gameOver.Color = new Color(0, 0, 0, System.Math.Min(1, _gameOverClock / 1.2f));
        // (a fresh press: a key still held from the fight does not skip it)
        bool down = KeyDown(Key.Enter) || KeyDown(Key.KpEnter) || KeyDown(Key.Escape) || KeyDown(Key.Z) || KeyDown(Key.Space);
        bool skip = _gameOverClock > 2.0f && down && !_gameOverKeyWasDown;
        _gameOverKeyWasDown = down;
        if (_gameOverClock >= 5.5f || skip) DoExit();
    }

    private void Finish(GameResult result)
    {
        // Practice runs (of a boss or of a stage) are not recorded. A run is recorded when it really ends: a victory, a defeat with no restart left, or a defeat from which the player goes to the menu
        // (a defeat that is followed by a restart is not: the attempt is thrown away, so that the table does not fill with the attempts of one run).
        bool recorded = PracticePhase == null && !StagePractice;
        bool endOfRun = result == GameResult.Victory || !CanRestart;
        bool newRecord = false;
        if (recorded && endOfRun) { newRecord = HighScore.Submit(World.Score, result == GameResult.Victory, Difficulty); _pendingScore = null; }
        else if (recorded) { _pendingScore = World.Score; newRecord = World.Score > HighScore.Current; }
        _hud.HideAttackName();
        MusicPlayer.Instance?.Stop(2.0f);
        if (result == GameResult.Defeat && CountsRestarts && (NoRestart || RunRestarts.Left == 0))
        {
            // lost with no restart left: the screen goes black, Game Over, and then the menu
            _input.EndAll();
            BeginGameOver();
            return;
        }
        _hud.ShowEnd(result, World.Score, newRecord, CountsRestarts ? RunRestarts.Left : -1);
        _hud.SelectEnd(CanRestart ? GameHud.Control.Restart : GameHud.Control.Menu);
        _endShownAt = _clock;
        _hud.Say(result == GameResult.Victory ? UIText.SayVictory : UIText.SayDefeat);
        _input.EndAll();
    }
}
