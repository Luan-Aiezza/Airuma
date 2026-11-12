using System;
using Godot;
using static ViewKit;

/// <summary>
/// The scripted turns of the last two stages: the veil over the picture (white and black), the great light that ends stage 5 and the dark that stage 6 opens from, and the end of the story.
/// </summary>
public partial class GameScene : Node2D
{
    // MARK: The veil

    /// <summary>A plain colour over the whole picture (over the boat, the boss and the HUD, under the card picker), faded from one colour to another.</summary>
    private readonly ColorRect _veil = new() { Color = new Color(0, 0, 0, 0), MouseFilter = Control.MouseFilterEnum.Ignore, ZIndex = 145, Visible = false };
    private Color _veilFrom, _veilTo;
    private float _veilT = 1, _veilSeconds = 1;

    private void FadeVeil(Color to, float seconds)
    {
        _veilFrom = _veil.Color; _veilTo = to; _veilT = 0; _veilSeconds = Math.Max(0.01f, seconds);
        _veil.Visible = true;
    }

    // MARK: Effects timed by the lines

    private float _tremor;

    /// <summary>A line of a talk has appeared. A line spoken by no one is a happening, and it is shown: the tremor before the first boss rises shakes the whole picture.</summary>
    private void OnDialogueLine(DialogueLine line)
    {
        if (line.Speaker != DialogueSpeaker.Narrator) return;
        _tremor = 2.8f;
        SfxPlayer.Instance?.Play(SfxBank.Sound.Quake);
    }

    private void UpdateVeil(float dt)
    {
        _tremor = Math.Max(0, _tremor - dt);
        if (_veilT >= 1) return;
        _veilT = Math.Min(1, _veilT + dt / _veilSeconds);
        _veil.Color = _veilFrom.Lerp(_veilTo, _veilT);
        if (_veilT >= 1 && _veilTo.A <= 0) _veil.Visible = false;     // clear: nothing left to draw
    }

    private void BuildFinale()
    {
        _veil.Size = Layout.SceneSize;
        AddChild(_veil);
    }

    /// <summary>A new run of the stage: the veil is cleared (a stage that opens from the dark starts black and clears).</summary>
    private void ResetFinale()
    {
        _veilT = 1; _veil.Visible = false; _veil.Color = new Color(0, 0, 0, 0);
        if (Stage.OpensFromDark) { _veil.Color = Colors.Black; FadeVeil(new Color(0, 0, 0, 0), 2.4f); }
        _lightStarted = _lightDark = _lightDone = false; _lightClock = 0;
        _epilogueState = 0; _epilogueDone = false;
        _epilogue?.QueueFree(); _epilogue = null;
    }

    // MARK: The great light (the end of stage 5)

    private bool _lightStarted, _lightDark, _lightDone;
    private float _lightClock;

    /// <summary>The boss opens the way: a great light whitens the whole picture, and then everything goes dark and stays so (the card of the end is chosen in the dark). True when it is dark.</summary>
    private bool RunLightTransition(float dt)
    {
        if (!_lightStarted)
        {
            _lightStarted = true;
            _hud.HideAttackName();
            _veil.Color = new Color(1, 1, 1, 0);
            FadeVeil(Colors.White, 1.6f);
            SfxPlayer.Instance?.Play(SfxBank.Sound.Bomb);
            MusicPlayer.Instance?.Stop(2.0f);
        }
        _lightClock += dt;
        if (!_lightDark && _lightClock >= 2.4f) { _lightDark = true; FadeVeil(Colors.Black, 1.6f); }
        return _lightClock >= 4.2f;
    }

    // MARK: The end of the story

    /// <summary>
    /// Easy mode goes no further than stage 5: the boss calls the player unworthy and the game ends there, with the bad ending (the boy and the bear fishing in the endless night, to a sad piano).
    /// </summary>
    private bool EasyEnds => Difficulty == Difficulty.Easy && Stage == Stages.Five && PracticePhase == null && !StagePractice;

    private Node _epilogue;
    private int _epilogueState;
    private bool _epilogueDone;
    private float _epilogueClock;

    /// <summary>After the last talk: the picture fades to black, the story is told over a picture, and then the end screen. True while it goes on.</summary>
    private void RunEpilogue(float dt)
    {
        switch (_epilogueState)
        {
            case 0:
                _epilogueState = 1; _epilogueClock = 0;
                _hud.HideAttackName();
                FadeVeil(Colors.Black, 1.2f);
                MusicPlayer.Instance?.Play(EasyEnds ? MusicPlayer.Theme.SadEnding : MusicPlayer.Theme.HappyEnding);
                break;
            case 1:
                _epilogueClock += dt;
                if (_epilogueClock < 1.4f) break;
                _epilogueState = 2;
                if (EasyEnds)
                {
                    var bad = new IntroScreen(Dialogues.BadEndStory, bad: true);
                    bad.Finished += () => _epilogueState = 3;
                    _epilogue = bad;
                }
                else
                {
                    var good = new EpilogueScreen(Stage.Epilogue());
                    good.Finished += () => _epilogueState = 3;
                    _epilogue = good;
                }
                AddChild(_epilogue);
                break;
            case 3:
                _epilogue.QueueFree(); _epilogue = null;
                _epilogueDone = true;                   // the end screen takes over
                if (!Demo && PracticePhase == null && !StagePractice) { if (EasyEnds) Achievements.BadEndingSeen(); else Achievements.TrueEndingSeen(); }
                FadeVeil(new Color(0, 0, 0, 0), 0.8f);
                break;
        }
    }
}
