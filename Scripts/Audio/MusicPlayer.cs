using Godot;
using System.Threading.Tasks;

/// <summary>
/// Plays the game's music. The two themes (see <see cref="Songs"/>) are rendered by the synthesiser on background threads when
/// the game starts, so the menu stays responsive; a theme starts as soon as it is ready. Reach it through <see cref="Instance"/>.
/// </summary>
public partial class MusicPlayer : Node
{
    public enum Theme { Menu, Stage, Boss, BossTwo }

    public static MusicPlayer Instance { get; private set; }

    private const float PlayDb = -6f;

    private AudioStreamPlayer _player;
    private readonly Task<(float[], float[])>[] _tasks = new Task<(float[], float[])>[4];
    private readonly AudioStreamWav[] _streams = new AudioStreamWav[4];
    private Theme? _wanted;
    private Theme? _playing;
    private Tween _fade;

    public Theme? Current => _playing;

    public override void _Ready()
    {
        Instance = this;
        _player = new AudioStreamPlayer { VolumeDb = PlayDb };
        AddChild(_player);
        // The menu's theme is the calm one, the stage's the driving one, the boss's the fiercest, and the serpent's heavier still. The menu's goes first.
        _tasks[(int)Theme.Menu] = Task.Run(Songs.QuietWater);
        _tasks[(int)Theme.Stage] = Task.Run(Songs.EmberSerpent);
        _tasks[(int)Theme.Boss] = Task.Run(Songs.InfernoSerpent);
        _tasks[(int)Theme.BossTwo] = Task.Run(Songs.SerpentAblaze);
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    private static AudioStreamWav MakeStream((float[] l, float[] r) song) => new AudioStreamWav
    {
        Format = AudioStreamWav.FormatEnum.Format16Bits,
        MixRate = Synth.Rate,
        Stereo = true,
        Data = Synth.ToPcm16(song.l, song.r),
        LoopMode = AudioStreamWav.LoopModeEnum.Forward,
        LoopBegin = 0,
        LoopEnd = song.l.Length,
    };

    public override void _Process(double delta)
    {
        for (int i = 0; i < _tasks.Length; i++)
            if (_streams[i] == null && _tasks[i].IsCompletedSuccessfully) _streams[i] = MakeStream(_tasks[i].Result);
        if (_wanted is Theme want && _playing != want)
        {
            var stream = _streams[(int)want];
            if (stream == null) return;           // still being composed
            _fade?.Kill();
            _player.Stream = stream;
            _player.VolumeDb = PlayDb;
            _player.Play();
            _playing = want;
        }
    }

    /// <summary>Starts a theme from its beginning (as soon as it is ready), replacing whatever is playing.</summary>
    public void Play(Theme theme)
    {
        _wanted = theme;
        _playing = null;
        _fade?.Kill();
        if (_player != null) _player.Stop();
    }

    /// <summary>Fades the music out and stops it.</summary>
    public void Stop(float seconds = 0.6f)
    {
        _wanted = null;
        _playing = null;
        if (_player == null || !_player.Playing) return;
        _fade?.Kill();
        _fade = CreateTween();
        _fade.TweenProperty(_player, "volume_db", -60f, seconds);
        _fade.TweenCallback(Callable.From(() => { if (_wanted == null) _player.Stop(); }));
    }
}
