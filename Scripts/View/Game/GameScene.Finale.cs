using System;
using Godot;
using static ViewKit;

/// <summary>
/// The scripted turns of the last two stages: the veil over the picture (white and black), the great light that ends stage 5 and the dark that stage 6 opens from, the sleeping
/// priestess of stage 6 (she lies in the middle of the river, is sent away when the goddess speaks and is brought back, to wake, after the fight), and the end of the story.
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

    private void UpdateVeil(float dt)
    {
        if (_veilT >= 1) return;
        _veilT = Math.Min(1, _veilT + dt / _veilSeconds);
        _veil.Color = _veilFrom.Lerp(_veilTo, _veilT);
        if (_veilT >= 1 && _veilTo.A <= 0) _veil.Visible = false;     // clear: nothing left to draw
    }

    private void BuildFinale()
    {
        _veil.Size = Layout.SceneSize;
        AddChild(_veil);
        if (Stage.Priestess)
        {
            _priestess = new Sprite2D { Texture = IllariArt.MakeLying().ToTexture(), Scale = new Vector2(PriestessScale, PriestessScale), ZIndex = Z(1.8f), Visible = false };
            _priestessStanding = new Sprite2D { Texture = IllariArt.MakeStanding().ToTexture(), Scale = new Vector2(StandingScale, StandingScale), ZIndex = Z(1.8f), Visible = false };
            AddChild(_priestess);
            AddChild(_priestessStanding);
        }
    }

    /// <summary>A new run of the stage: the veil is cleared (a stage that opens from the dark starts black and clears), and the priestess is not there.</summary>
    private void ResetFinale()
    {
        _veilT = 1; _veil.Visible = false; _veil.Color = new Color(0, 0, 0, 0);
        if (Stage.OpensFromDark) { _veil.Color = Colors.Black; FadeVeil(new Color(0, 0, 0, 0), 2.4f); }
        _lightStarted = _lightDark = _lightDone = false; _lightClock = 0;
        _epilogueState = 0; _epilogueDone = false;
        _epilogue?.QueueFree(); _epilogue = null;
        _priestessTween?.Kill();
        if (_priestess != null) { _priestess.Visible = false; _priestessStanding.Visible = false; _priestessReturned = _priestessAwake = false; }
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

    // MARK: The priestess (stage 6)

    private Sprite2D _priestess, _priestessStanding;
    private Tween _priestessTween;
    private bool _priestessReturned, _priestessAwake;
    /// <summary>Screen pixels per art pixel: lying on the water, and standing.</summary>
    private const float PriestessScale = 1.6f, StandingScale = 1.6f;      // the same as Killa's
    /// <summary>She lies on her side along the line of the horizon of the water, a little below it, in the middle (the boss, who stands on the line, is over her).</summary>
    private static readonly Vector2 PriestessPlace = new Vector2(300, Layout.Horizon + 14);
    /// <summary>Where the standing picture goes so that her feet are where the lying one's body was (its centre is that far above).</summary>
    private static float StandingLift => (IllariArt.LyingBaseline - IllariArt.LyingHeight / 2f) * PriestessScale - (IllariArt.StandingFeet - IllariArt.StandingHeight / 2f) * StandingScale;

    /// <summary>She lies asleep on her side on the water, on the line of the horizon (she fades in as the river opens).</summary>
    private void ShowPriestess()
    {
        if (_priestess == null) return;
        _priestessTween?.Kill();
        _priestessStanding.Visible = false;
        _priestess.Position = Point(PriestessPlace); _priestess.Scale = new Vector2(PriestessScale, PriestessScale);
        _priestess.Modulate = new Color(1, 1, 1, 0); _priestess.Visible = true;
        _priestessTween = CreateTween();
        _priestessTween.TweenProperty(_priestess, "modulate:a", 1f, 1.6f);
    }

    /// <summary>The goddess sends her far away: a flash, and she shrinks into light and is gone.</summary>
    private void SendPriestessAway()
    {
        if (_priestess == null || !_priestess.Visible) return;
        _priestessTween?.Kill();
        SfxPlayer.Instance?.Play(SfxBank.Sound.LevelUp);
        _priestessTween = CreateTween();
        _priestessTween.TweenProperty(_priestess, "modulate", new Color(3, 3, 3, 1), 0.3f);
        _priestessTween.TweenProperty(_priestess, "scale", new Vector2(0.25f, 0.25f), 0.7f);
        _priestessTween.Parallel().TweenProperty(_priestess, "modulate:a", 0f, 0.7f);
        _priestessTween.TweenCallback(Callable.From(() => _priestess.Visible = false));
    }

    /// <summary>The goddess brings her back, still asleep, with a flash.</summary>
    private void BringPriestessBack()
    {
        if (_priestess == null) return;
        _priestessTween?.Kill();
        SfxPlayer.Instance?.Play(SfxBank.Sound.LevelUp);
        _priestessStanding.Visible = false;
        _priestess.Position = Point(PriestessPlace); _priestess.Scale = new Vector2(0.25f, 0.25f);
        _priestess.Modulate = new Color(3, 3, 3, 0); _priestess.Visible = true;
        _priestessTween = CreateTween();
        _priestessTween.TweenProperty(_priestess, "scale", new Vector2(PriestessScale, PriestessScale), 0.7f);
        _priestessTween.Parallel().TweenProperty(_priestess, "modulate:a", 1f, 0.5f);
        _priestessTween.Parallel().TweenProperty(_priestess, "modulate", new Color(1, 1, 1, 1), 0.9f);
    }

    /// <summary>She wakes: the picture of her asleep gives way to the one of her standing, who rises a little out of the water, her hands still in prayer.</summary>
    private void WakePriestess()
    {
        if (_priestess == null) return;
        _priestessTween?.Kill();
        var home = Point(PriestessPlace) + new Vector2(0, StandingLift);
        _priestessStanding.Position = home + new Vector2(0, 26);
        _priestessStanding.Modulate = new Color(1, 1, 1, 0);
        _priestessStanding.Visible = true;
        _priestessTween = CreateTween().SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
        _priestessTween.TweenProperty(_priestess, "modulate:a", 0f, 0.7f);
        _priestessTween.Parallel().TweenProperty(_priestessStanding, "modulate:a", 1f, 0.8f);
        _priestessTween.Parallel().TweenProperty(_priestessStanding, "position:y", home.Y, 1.0f);
        _priestessTween.TweenCallback(Callable.From(() => _priestess.Visible = false));
    }

    /// <summary>During the last talk: she returns at her line and wakes at the first line she speaks.</summary>
    private void UpdatePriestessEnding()
    {
        if (_priestess == null) return;
        if (!_priestessReturned && _dialogue.Index >= Dialogues.StageSixReturnLine) { _priestessReturned = true; BringPriestessBack(); }
        if (_priestessReturned && !_priestessAwake && _dialogue.Index >= Dialogues.StageSixWakeLine) { _priestessAwake = true; WakePriestess(); }
    }

    // MARK: The end of the story

    private EpilogueScreen _epilogue;
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
                MusicPlayer.Instance?.Play(MusicPlayer.Theme.Menu);
                break;
            case 1:
                _epilogueClock += dt;
                if (_epilogueClock < 1.4f) break;
                _epilogueState = 2;
                _epilogue = new EpilogueScreen(Stage.Epilogue());
                _epilogue.Finished += () => _epilogueState = 3;
                AddChild(_epilogue);
                break;
            case 3:
                _epilogue.QueueFree(); _epilogue = null;
                _epilogueDone = true;                   // the end screen takes over
                FadeVeil(new Color(0, 0, 0, 0), 0.8f);
                break;
        }
    }
}
