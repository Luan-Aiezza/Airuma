using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>The scripted parts of a run: the stage opening, the talks before and after the boss, and the boss changing shape.</summary>
public partial class GameScene : Node2D
{
    // The sequences. The stage opens with the boat rising from below the screen under the stage's title and music; then, after a
    // moment, the talk comes in, and after it the choice of a card. The boss fight opens with the river turning into the sea (the
    // banks slide out); after a moment the talk comes in; the boss comes down from above and says her piece; her title and music
    // begin and the fight starts. After her defeat there is a last talk, and then the end screen. Nothing appears out of the blue.
    private bool _stageIntroPending;
    private bool _rising;
    private float _riseClock;
    private bool _dialoguePending;
    private float _dialogueDelay;
    private string _pendingAttackName;
    private float _attackNameDelay;
    private const float RiseStart = 0.5f, RiseEnd = 2.3f, RiseTotal = 2.9f, OffscreenY = 900;
    private const float TalkDelayAfterRise = 0.6f, TalkDelayAfterTrees = 1.8f, DescendTime = 2.4f, EndingDelay = 1.5f;

    private enum BossSequence { None, Opening, TalkOne, Descend, TalkTwo, MorphPause, MorphTalk, MorphReveal }
    private BossSequence _bossSequence;
    /// <summary>While she gathers herself and speaks, the boss is still the woman, though the world has already counted her second form.</summary>
    private bool _morphHoldWoman;
    private Vector2 _bossShake;
    private const float MorphPauseTime = 1.4f, MorphRevealTime = 2.0f;
    private float _sequenceClock;
    private enum EndSequence { None, Waiting, Talking, Done }
    private EndSequence _endSequence;

    /// <summary>Runs the openings and the talks; returns true while the world must stay still.</summary>
    private bool UpdateSequences(float dt)
    {
        if (_stageIntroPending)
        {
            _stageIntroPending = false;
            _rising = true;
            _riseClock = 0;
            if (World.Part == StagePart.Boss) AnnounceBossPhase();          // boss practice opens with the boss
            else
            {
                MusicPlayer.Instance?.Play(World.Definition.Theme);
                _title.Show(World.Definition.Title, World.Definition.MusicTitle, fire: false, hold: 3.0f, difficulty: Difficulty);
            }
        }
        if (_rising)
        {
            _riseClock += dt;
            float u = Mathf.Clamp((_riseClock - RiseStart) / (RiseEnd - RiseStart), 0, 1);
            float ease = 1 - (1 - u) * (1 - u) * (1 - u);
            World.Player.Transform.Position = new Vector2(CombatSystem.PlayerSpawn.X, Mathf.Lerp(OffscreenY, CombatSystem.PlayerSpawn.Y, ease));
            if (_riseClock >= RiseTotal)
            {
                _rising = false;
                World.Player.Transform.Position = CombatSystem.PlayerSpawn;
                if (World.Part == StagePart.Boss) BeginBossArrival(false);
                else if (World.UsesCards) { _dialoguePending = true; _dialogueDelay = TalkDelayAfterRise; }
            }
            return true;
        }
        if (_dialoguePending)
        {
            _dialogueDelay -= dt;
            if (_dialogueDelay <= 0) { _dialoguePending = false; _dialogue.Start(Dialogues.StageOneIntro); }
            return true;
        }

        switch (_bossSequence)
        {
            case BossSequence.Opening:
                _sequenceClock += dt;
                if (_sequenceClock >= TalkDelayAfterTrees) { _dialogue.Start(Dialogues.BeforeBossOne); _bossSequence = BossSequence.TalkOne; }
                return true;
            case BossSequence.TalkOne:
                if (!_dialogue.IsOnScreen) { _bossSequence = BossSequence.Descend; _sequenceClock = 0; }
                return true;
            case BossSequence.Descend:
            {
                _sequenceClock += dt;
                float u = Mathf.Clamp(_sequenceClock / DescendTime, 0, 1);
                float ease = 1 - (1 - u) * (1 - u) * (1 - u);
                World.Boss.Transform.Position = new Vector2(300, Mathf.Lerp(-170, Layout.BossY, ease));
                if (_sequenceClock >= DescendTime + 0.5f) { _dialogue.Start(Dialogues.BeforeBossTwo); _bossSequence = BossSequence.TalkTwo; }
                return true;
            }
            case BossSequence.TalkTwo:
                if (!_dialogue.IsOnScreen)
                {
                    // The talk is over: her title and her music, and the fight begins a moment later.
                    _bossSequence = BossSequence.None;
                    World.Boss.Transform.Position = new Vector2(300, Layout.BossY);
                    World.Boss.TransitionTimer = 1.0f;
                    World.Boss.TransitionSpeed = 0;
                    AnnounceBossPhase(1.6f);
                }
                return true;
        }

        switch (_bossSequence)
        {
            case BossSequence.MorphPause:
                // She has had enough: the music dies away and she trembles.
                _sequenceClock += dt;
                _bossShake = ShakeOffset(2 + 4 * Mathf.Clamp(_sequenceClock / MorphPauseTime, 0, 1));
                if (_sequenceClock >= MorphPauseTime) { _dialogue.Start(CurrentPhase.TransformationDialogue); _bossSequence = BossSequence.MorphTalk; }
                return true;
            case BossSequence.MorphTalk:
                _bossShake = ShakeOffset(5);
                if (!_dialogue.IsOnScreen)
                {
                    // The woman bursts into the serpent.
                    _bossSequence = BossSequence.MorphReveal;
                    _sequenceClock = 0;
                    _morphHoldWoman = false;
                    World.Boss.Transform.Position = new Vector2(300, Layout.BossY);
                    SfxPlayer.Instance?.Play(SfxBank.Sound.Bomb);
                    SfxPlayer.Instance?.Play(SfxBank.Sound.BossBroken);
                }
                return true;
            case BossSequence.MorphReveal:
                _sequenceClock += dt;
                _bossShake = ShakeOffset(6 * (1 - Mathf.Clamp(_sequenceClock / MorphRevealTime, 0, 1)));
                if (_sequenceClock >= MorphRevealTime)
                {
                    // Her new name and her new music, and the fight begins a moment later.
                    _bossSequence = BossSequence.None;
                    _bossShake = Vector2.Zero;
                    World.Boss.TransitionTimer = 1.0f;
                    World.Boss.TransitionSpeed = 0;
                    AnnounceBossPhase(1.6f);
                }
                return true;
        }

        switch (_endSequence)
        {
            case EndSequence.Waiting:
                _sequenceClock += dt;
                if (_sequenceClock >= EndingDelay) { _dialogue.Start(Dialogues.AfterBoss); _endSequence = EndSequence.Talking; }
                return true;
            case EndSequence.Talking:
                if (!_dialogue.IsOnScreen) _endSequence = EndSequence.Done;
                return true;
        }
        return false;
    }

    private Vector2 ShakeOffset(float amount) =>
        new Vector2(_rng.RandfRange(-amount, amount), _rng.RandfRange(-amount, amount) * 0.5f);

    /// <summary>The woman breaks into her second form: a new fight. The attack is cleared, the music stops, she speaks, and only then does she change.</summary>
    private void BeginMorph()
    {
        _bossSequence = BossSequence.MorphPause;
        _sequenceClock = 0;
        _morphHoldWoman = true;
        _pendingAttackName = null;
        _hud.HideAttackName();
        MusicPlayer.Instance?.Stop(1.0f);
    }

    /// <summary>The boss is coming (a normal run): the banks slide out, the horizon and the moon appear, and, after a moment, the talk.</summary>
    private void BeginBossCutscene()
    {
        _bossSequence = BossSequence.Opening;
        _sequenceClock = 0;
        World.Boss.Transform.Position = new Vector2(300, -170);
        River.BeginOpenWater(3.2f);
        MusicPlayer.Instance?.Stop(1.2f);
    }

    /// <summary>Boss practice: the river opens and the boss comes down at once, with her title and music.</summary>
    private void BeginBossArrival(bool announce)
    {
        River.BeginOpenWater(3.2f);
        AnnounceBossPhase(PhaseManagerSystem.EntranceSeconds, announce);
    }

    private BossPhase CurrentPhase => World.Boss.Definition.PhaseFor(World.Boss.Health.Lives);

    /// <summary>
    /// The boss's attack is about to begin: her name and her music (unless `withTitle` is false), then, after attackNameDelay seconds, the
    /// name of the attack.
    /// </summary>
    private void AnnounceBossPhase(float attackNameDelay = 0, bool withTitle = true)
    {
        var phase = CurrentPhase;
        if (withTitle)
        {
            MusicPlayer.Instance?.Play(phase.Theme);
            _title.Show(phase.Name, phase.MusicTitle, fire: true, hold: 3.0f, difficulty: Difficulty);
        }
        if (attackNameDelay <= 0) return;
        _pendingAttackName = phase.AttackName;
        _attackNameDelay = attackNameDelay;
    }
}
