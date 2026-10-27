using System.Collections.Generic;
using System.Linq;
using Godot;

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
                case WorldEventKind.BombFired: SfxPlayer.Instance?.Play(SfxBank.Sound.Bomb); break;
                case WorldEventKind.LightGathered: SfxPlayer.Instance?.Play(SfxBank.Sound.Pickup); break;
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
        if (boss.Health.Lives < _lastBossLives && boss.Health.Lives > 0)
        {
            // A new shape (with something to say about it) is a new fight; otherwise just a new attack.
            bool changedShape = boss.Definition.PhaseFor(_lastBossLives).Form != boss.Form;
            _hud.Say(changedShape ? UIText.SaySerpent : UIText.SayEnraged);
            if (changedShape && CurrentPhase.TransformationDialogue != null && World.Part == StagePart.Boss) BeginMorph();
            else _hud.ShowAttackName(CurrentPhase.AttackName ?? "");
        }
        _lastLives = player.Health.Lives;
        _lastBombs = player.Bomb.BombsLeft;
        _lastBossLives = boss.Health.Lives;
    }

    private void Finish(GameResult result)
    {
        // Practice runs are not recorded.
        bool newRecord = PracticePhase == null && HighScore.Submit(World.Score, result == GameResult.Victory, Difficulty);
        _hud.HideAttackName();
        MusicPlayer.Instance?.Stop(2.0f);
        _hud.ShowEnd(result, World.Score, newRecord);
        _hud.Say(result == GameResult.Victory ? UIText.SayVictory : UIText.SayDefeat);
        _input.EndAll();
    }
}
