using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Plays the sound effects of <see cref="SfxBank"/>. Each sound is rendered once, at start-up; a handful of players take turns so
/// that sounds can overlap, and a short gap per sound keeps a ring of thirty bullets from becoming a wall of noise. Reach it through
/// <see cref="Instance"/>; it does nothing when absent (as in the self test).
/// </summary>
public partial class SfxPlayer : Node
{
    public static SfxPlayer Instance { get; private set; }

    private const int Voices = 20;
    private readonly Dictionary<SfxBank.Sound, AudioStreamWav> _streams = new();
    private readonly Dictionary<SfxBank.Sound, double> _lastPlayed = new();
    private readonly List<AudioStreamPlayer> _players = new();
    private int _next;
    private readonly Random _random = new Random(77);

    public override void _Ready()
    {
        Instance = this;
        foreach (SfxBank.Sound s in Enum.GetValues<SfxBank.Sound>()) _streams[s] = SfxBank.ToStream(SfxBank.Make(s));
        for (int i = 0; i < Voices; i++)
        {
            var p = new AudioStreamPlayer();
            AddChild(p);
            _players.Add(p);
        }
    }

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public void Play(SfxBank.Sound sound)
    {
        var (db, gap) = SfxBank.Settings(sound);
        double now = Time.GetTicksMsec() / 1000.0;
        if (_lastPlayed.TryGetValue(sound, out double last) && now - last < gap) return;
        _lastPlayed[sound] = now;
        var player = _players[_next];
        _next = (_next + 1) % _players.Count;
        player.Stream = _streams[sound];
        player.VolumeDb = db;
        player.PitchScale = 1f + (float)(_random.NextDouble() - 0.5) * 0.07f;   // a touch of variety
        player.Play();
    }

    public void PlayBullet(BulletStyle style) => Play(SfxBank.ForBullet(style));
}
