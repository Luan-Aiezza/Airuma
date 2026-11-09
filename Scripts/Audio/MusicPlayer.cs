using Godot;
using System;
using System.Threading.Tasks;

/// <summary>
/// Plays the game's music. The themes (see <see cref="Songs"/>) are rendered by the synthesiser on a background thread when
/// the game starts, one after the other, so the menu stays responsive; a theme starts as soon as it is ready. Reach it through <see cref="Instance"/>.
/// </summary>
public partial class MusicPlayer : Node
{
	public enum Theme { Menu, Stage, Boss, NightStage, DancerBoss, StormStage, CatBoss, CaveStage, BeastBoss, DivineStage, SerpentBoss, CosmicStage, SadEnding, HappyEnding, GameOver, CosmicBoss }

	public static MusicPlayer Instance { get; private set; }

	private const float BaseDb = -6f;
	/// <summary>The volume of the music: its own level less what the player took off in Config.</summary>
	private static float PlayDb => BaseDb + Settings.ToDb(Settings.Music);

	private AudioStreamPlayer _player;
	private static readonly int ThemeCount = Enum.GetValues<Theme>().Length;
	/// <summary>Each theme is rendered (by <see cref="Songs"/>, and turned into 16-bit audio) on a background thread; the result is its audio and its length in frames.</summary>
	private readonly Task<(byte[] pcm, int frames)>[] _tasks = new Task<(byte[], int)>[ThemeCount];
	private readonly AudioStreamWav[] _streams = new AudioStreamWav[ThemeCount];
	private readonly object _lock = new();
	/// <summary>How each theme is composed, in the order of <see cref="Theme"/>.</summary>
	private static readonly Func<(float[], float[])>[] Composers =
	{
		Songs.QuietWater, Songs.EmberSerpent, Songs.InfernoSerpent, Songs.AnimatedNight, Songs.StarDancer, Songs.StormUnderTheRiver, Songs.CatInTheStorm, Songs.CaveHibernation, Songs.GreatBeast, Songs.SerpentUnderTheMountain, Songs.HymnToTheGuardian, Songs.SilverMoonOverTheLake, Songs.EndlessNight, Songs.Daybreak, Songs.LightGoingOut, Songs.RiverThroughTheUniverse,
	};

	/// <summary>Starts rendering a theme, if it has not been started (a theme that is wanted now goes first, whatever the order of the background chain).</summary>
	public void Prepare(Theme theme)
	{
		int i = (int)theme;
		lock (_lock)
		{
			if (_tasks[i] != null) return;
			_tasks[i] = Task.Run(() =>
			{
				var (l, r) = Composers[i]();
				return (Synth.ToPcm16(l, r), l.Length);
			});
		}
	}
	private Theme? _wanted;
	private Theme? _playing;
	private Tween _fade;

	public Theme? Current => _playing;

	public override void _Ready()
	{
		Instance = this;
		_player = new AudioStreamPlayer { VolumeDb = PlayDb };
		AddChild(_player);
		// Every song is rendered at the start, one after the other, on a background thread (the menu's first, then stage 1's...), so that each is ready long before its stage; one that is
		// wanted sooner is started at once by Prepare. (They are all rendered here, and not as each is wanted, because a song takes some seconds to render.)
		// (a few at a time, in the order they are wanted: the first songs are ready at once, and a long song does not hold up the ones that come after it)
		Task.Run(() =>
		{
			int degree = Math.Clamp(System.Environment.ProcessorCount / 2, 1, 3);
			try { Parallel.For(0, ThemeCount, new ParallelOptions { MaxDegreeOfParallelism = degree }, i => { Prepare((Theme)i); try { _tasks[i].Wait(); } catch (Exception) { /* reported when it is wanted */ } }); }
			catch (Exception) { /* reported when it is wanted */ }
		});
	}

	/// <summary>The volume was changed in Config: it takes effect on what is playing now.</summary>
	public void ApplyVolume()
	{
		if (_player == null) return;
		if (_fade != null && _fade.IsValid()) return;      // it is fading out: it has its own way down
		_player.VolumeDb = PlayDb;
	}

	public override void _ExitTree()
	{
		if (Instance == this) Instance = null;
	}

	private static AudioStreamWav MakeStream((byte[] pcm, int frames) song) => new AudioStreamWav
	{
		Format = AudioStreamWav.FormatEnum.Format16Bits,
		MixRate = Synth.Rate,
		Stereo = true,
		Data = song.pcm,
		LoopMode = AudioStreamWav.LoopModeEnum.Forward,
		LoopBegin = 0,
		LoopEnd = song.frames,
	};

	public override void _Process(double delta)
	{
		if (_wanted is Theme want && _playing != want)
		{
			int i = (int)want;
			var task = _tasks[i];
			if (_streams[i] == null && task != null)
			{
				if (task.IsFaulted) { GD.PrintErr($"The music {want} could not be composed: {task.Exception?.GetBaseException().Message}"); _wanted = null; return; }
				if (task.IsCompletedSuccessfully) { _streams[i] = MakeStream(task.Result); _tasks[i] = Task.FromResult<(byte[], int)>((Array.Empty<byte>(), 0)); }   // (the audio is now in the stream: the copy is let go)
			}
			var stream = _streams[i];
			if (stream == null) return;           // still being composed
			_fade?.Kill();
			_player.Stream = stream;
			_player.VolumeDb = PlayDb;
			_player.Play();
			_playing = want;
		}
	}

	/// <summary>Starts a theme from its beginning (as soon as it is ready), replacing whatever is playing.</summary>
	/// <summary>While true no song starts: the demonstration of the game plays without music, so as not to give the songs away.</summary>
	public bool Silent { get; set; }

	public void Play(Theme theme)
	{
		if (Silent) return;
		if (_wanted == theme) return;          // already wanted (a song that goes on from one attack to the next does not start over)
		Prepare(theme);
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
