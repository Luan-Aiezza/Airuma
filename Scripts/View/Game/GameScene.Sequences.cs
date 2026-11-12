using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using static ViewKit;

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

    private enum BossSequence { None, Opening, TalkOne, Descend, TalkTwo }
    private BossSequence _bossSequence;
    /// <summary>While she gathers herself and speaks, the boss is still the woman, though the world has already counted her second form.</summary>
    private float _sequenceClock;
    /// <summary>Seconds left in which the boss rides the platform down, in boss practice.</summary>
    private float _rideClock;
    private enum EndSequence { None, Waiting, Talking, Done }
    private EndSequence _endSequence;

    /// <summary>Runs the openings and the talks; returns true while the world must stay still.</summary>
    private bool UpdateSequences(float dt)
    {
        // The boss rides the platform down: they come together (stage 4)
        if (River.HasPlatform && (_bossSequence is BossSequence.Opening or BossSequence.TalkOne or BossSequence.Descend || _rideClock > 0))
        {
            _rideClock = Math.Max(0, _rideClock - dt);
            if (_bossSequence != BossSequence.None || World.Boss.MovementType == BossMovementType.Transition)
                World.PlaceBoss(new Vector2(300, World.Definition.Boss.StandY - River.PlatformRise));
        }
        if (_stageIntroPending)
        {
            _stageIntroPending = false;
            _rising = true;
            _riseClock = 0;
            // boss practice has no opening: the boss's own scene follows the rise. The music of the stage (and its title) begins when the talk is over.
            if (World.Part != StagePart.Boss)
            {
                if (World.UsesCards && Stage.Intro != null) _musicAfterIntro = true;
                else StartStageMusic();
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
                if (World.Part == StagePart.Boss) BeginBossCutscene();           // boss practice has the same scene as a run: the river opens, the talks, the boss comes down
                else if (World.UsesCards) { _dialoguePending = true; _dialogueDelay = TalkDelayAfterRise; }
            }
            return true;
        }
        if (_dialoguePending)
        {
            _dialogueDelay -= dt;
            if (_dialogueDelay <= 0) { _dialoguePending = false; if (!StartTalk(Stage.Intro)) { _musicAfterIntro = false; StartStageMusic(); } }
            return true;
        }
        if (_musicAfterIntro && !_dialogue.IsOnScreen) { _musicAfterIntro = false; StartStageMusic(); }

        switch (_bossSequence)
        {
            case BossSequence.Opening:
                _sequenceClock += dt;
                if (_sequenceClock >= TalkDelayAfterTrees) { _bossSequence = BossSequence.TalkOne; StartTalk(Stage.BossArrival); }
                return true;
            case BossSequence.TalkOne:
                if (!_dialogue.IsOnScreen) { _bossSequence = BossSequence.Descend; _sequenceClock = 0; }
                return true;
            case BossSequence.Descend:
            {
                _sequenceClock += dt;
                float u = Mathf.Clamp(_sequenceClock / DescendTime, 0, 1);
                float ease = 1 - (1 - u) * (1 - u) * (1 - u);
                if (!River.HasPlatform) World.PlaceBoss(new Vector2(300, Mathf.Lerp(-170, World.Definition.Boss.StandY, ease)));
                if (_sequenceClock >= DescendTime + 0.5f) { _bossSequence = BossSequence.TalkTwo; StartTalk(Stage.BossEncounter); }
                return true;
            }
            case BossSequence.TalkTwo:
                if (!_dialogue.IsOnScreen)
                {
                    // The talk is over: her title and her music, and the fight begins a moment later.
                    _bossSequence = BossSequence.None;
                    World.PlaceBoss(new Vector2(300, World.Definition.Boss.StandY));
                    World.Boss.TransitionTimer = 1.0f;
                    World.Boss.TransitionSpeed = 0;
                    if (World.Twin != null) { World.Twin.TransitionTimer = 1.0f; World.Twin.TransitionSpeed = 0; }
                    AnnounceBossPhase(1.6f);
                }
                return true;
        }

        switch (_endSequence)
        {
            case EndSequence.Waiting:
                _sequenceClock += dt;
                if (_sequenceClock >= EndingDelay) { _endSequence = EndSequence.Talking; StartTalk(EasyEnds ? () => Dialogues.StageFiveEasyEnding : Stage.Ending); }
                return true;
            case EndSequence.Talking:
                if (!_dialogue.IsOnScreen) _endSequence = EndSequence.Done;
                return true;
        }
        return false;
    }

    /// <summary>The music of the stage and its title: they begin together, once the talk before the stage is over.</summary>
    private bool _musicAfterIntro;

    private void StartStageMusic()
    {
        MusicPlayer.Instance?.Play(World.Definition.Theme);
        _title.Show(World.Definition.Title, World.Definition.MusicTitle, fire: false, hold: 3.0f, difficulty: Difficulty);
    }

    /// <summary>Starts a talk of the stage, if it has one; false when it has none.</summary>
    private bool StartTalk(Func<IReadOnlyList<DialogueLine>> talk)
    {
        if (talk == null) return false;
        _dialogue.Start(talk());
        return true;
    }

    /// <summary>The boss's first enraged attack: it flashes, rings spread from it, other music and its title begin, and the attack's name follows.</summary>
    private void BeginEnrage()
    {
        _boss.Flash();
        _twin?.Flash();
        AnnounceBossPhase(1.6f, banner: false);
    }

    /// <summary>The boss is coming (a normal run): the banks slide out, the horizon and the moon appear, and, after a moment, the talk.</summary>
    private void BeginBossCutscene()
    {
        _bossSequence = BossSequence.Opening;
        _sequenceClock = 0;
        World.PlaceBoss(new Vector2(300, -170));
        River.BeginOpenWater(3.2f);
        MusicPlayer.Instance?.Stop(1.2f);
    }

    /// <summary>Boss practice: the river opens and the boss comes down at once, with her title and music.</summary>
    private void BeginBossArrival(bool announce)
    {
        River.BeginOpenWater(3.2f);
        _rideClock = 3.4f;
        AnnounceBossPhase(PhaseManagerSystem.EntranceSeconds, announce);
    }

    private BossPhase CurrentPhase => World.Boss.Definition.PhaseFor(World.Boss.Health.Lives);

    /// <summary>
    /// The boss's attack is about to begin: her name and her music (unless `withTitle` is false), then, after attackNameDelay seconds, the
    /// name of the attack.
    /// </summary>
    private void AnnounceBossPhase(float attackNameDelay = 0, bool withTitle = true, bool banner = true)
    {
        var phase = CurrentPhase;
        River.SetRage(phase.Enraged);               // the second form of the boss: the sky of the river of stars goes red
        if (withTitle)
        {
            MusicPlayer.Instance?.Play(phase.Theme);
            // the banner with the boss's name and its song is for the meeting only (also when the rehearsal begins at an enraged attack: that is its meeting); the second form, in the middle of the fight, just bursts into rage
            if (banner) _title.Show(phase.Name, phase.MusicTitle, fire: true, hold: 3.0f, difficulty: Difficulty);
            if (phase.Enraged)
            {
                _boss.BurstRage();
                _twin?.BurstRage();
                SfxPlayer.Instance?.Play(SfxBank.Sound.Rage);
                SfxPlayer.Instance?.Play(SfxBank.Sound.BossBroken);
            }
        }
        if (attackNameDelay <= 0) return;
        _pendingAttackName = phase.AttackName;
        _attackNameDelay = attackNameDelay;
    }
}
