using System;

/// <summary>
/// The two songs of the endings. "Noite sem fim" (the bad ending, easy mode): a sad piano alone, slow, in A minor, the left hand falling in broken chords and a single voice above it, a pad
/// far under it like the dark of the river. "Aurora" (the ending of the normal and hard modes): glad trumpets in C major, a march-like beat, the tune sung twice and then once more a step higher.
/// Both are written with their own notes (not the shared chords of the other songs).
/// </summary>
public static partial class Songs
{
    // MARK: The bad ending

    /// <summary>One note of a tune: the beat in the bar, the pitch, how many beats it lasts.</summary>
    private readonly record struct Tone(double Beat, int Midi, double Length);

    // roots (pitch class), minor or not, for A minor: Am F C G / Am F E Am
    private static readonly (int bass, int[] triad)[] SadBars =
    {
        (45, new[] { 0, 3, 7 }), (41, new[] { 0, 4, 7 }), (48, new[] { 0, 4, 7 }), (43, new[] { 0, 4, 7 }),
        (45, new[] { 0, 3, 7 }), (41, new[] { 0, 4, 7 }), (40, new[] { 0, 4, 7 }), (45, new[] { 0, 3, 7 }),
    };

    private static Tone[][] SadTune() => new[]
    {
        new[] { new Tone(0, 76, 2), new Tone(2, 72, 1), new Tone(3, 71, 1) },
        new[] { new Tone(0, 69, 2), new Tone(2, 72, 2) },
        new[] { new Tone(0, 67, 2), new Tone(2, 72, 1), new Tone(3, 71, 1) },
        new[] { new Tone(0, 74, 3), new Tone(3, 71, 1) },
        new[] { new Tone(0, 76, 2), new Tone(2, 77, 1), new Tone(3, 76, 1) },
        new[] { new Tone(0, 72, 2), new Tone(2, 69, 2) },
        new[] { new Tone(0, 68, 2), new Tone(2, 71, 1), new Tone(3, 68, 1) },
        new[] { new Tone(0, 69, 4) },
    };

    /// <summary>"Noite sem fim": the theme of the bad ending.</summary>
    public static (float[] left, float[] right) EndlessNight()
    {
        const double bpm = 60;
        double spb = 60 / bpm, bar = 4 * spb;
        var tune = SadTune();
        const int bars = 16;
        var s = new Synth(bars * bar + 8);
        for (int b = 0; b < bars; b++)
        {
            var (bass, triad) = SadBars[b % 8];
            double t0 = b * bar;
            bool second = b >= 8;
            // the left hand: the bass, then the chord broken upwards and falling back, slow
            s.Piano(t0, bar * 0.98, bass - 12 + 12, 0.11, -0.3, 0.6);
            int[] order = { 0, 1, 2, 1, 2, 1, 0, 1 };
            for (int i = 0; i < 8; i++)
            {
                if (i == 0) continue;
                s.Piano(t0 + i * spb / 2, spb * 1.6, bass + 12 + triad[order[i]] + (order[i] == 0 ? 12 : 0), 0.045, -0.15 + 0.03 * i, 0.5);
            }
            // the dark under it: a pad, very soft
            s.Pad(t0, bar * 1.02, bass + 12, 0.03, 0.0, 0.5);
            // the voice: the tune, the second time one octave lower for the first half then with a sigh (a note added at the end of the bar)
            foreach (var n in tune[b % 8])
            {
                int midi = n.Midi + (second && b >= 12 ? 12 : 0);
                s.Piano(t0 + n.Beat * spb, n.Length * spb * 1.05, midi, second ? 0.15 : 0.13, 0.15, second ? 0.0 : 0.2);
            }
        }
        // a bell far away, now and then, as if from the other shore
        foreach (int bar0 in new[] { 0, 4, 8, 12 }) s.Bell(bar0 * bar + 2 * spb, 93, 0.045, 0.45);
        s.Reverb(0.55, 0.9);
        return s.FinishLoop(bars * bar, 0.12);
    }

    // MARK: The good ending

    /// <summary>The tune of "Aurora", in C; each bar is a chord (C G Am F / C G F C).</summary>
    private static Tone[][] GladTune() => new[]
    {
        new[] { new Tone(0, 76, 1), new Tone(1, 79, 1), new Tone(2, 84, 2) },
        new[] { new Tone(0, 83, 1.5), new Tone(1.5, 81, 0.5), new Tone(2, 79, 2) },
        new[] { new Tone(0, 81, 1), new Tone(1, 84, 1), new Tone(2, 88, 2) },
        new[] { new Tone(0, 84, 1.5), new Tone(1.5, 81, 0.5), new Tone(2, 77, 2) },
        new[] { new Tone(0, 79, 0.5), new Tone(0.5, 81, 0.5), new Tone(1, 84, 1), new Tone(2, 83, 1), new Tone(3, 79, 1) },
        new[] { new Tone(0, 74, 0.5), new Tone(0.5, 76, 0.5), new Tone(1, 79, 1), new Tone(2, 83, 2) },
        new[] { new Tone(0, 81, 1), new Tone(1, 84, 1), new Tone(2, 81, 1), new Tone(3, 77, 1) },
        new[] { new Tone(0, 79, 0.5), new Tone(0.5, 77, 0.5), new Tone(1, 76, 1), new Tone(2, 72, 2) },
    };

    // (pitch class of the root, minor)
    private static readonly (int root, bool minor)[] GladBars = { (0, false), (7, false), (9, true), (5, false), (0, false), (7, false), (5, false), (0, false) };

    /// <summary>"Aurora": the theme of the good ending.</summary>
    public static (float[] left, float[] right) Daybreak()
    {
        const double bpm = 132;
        double spb = 60 / bpm, bar = 4 * spb;
        var tune = GladTune();
        const int intro = 2, passes = 3, bars = intro + passes * 8;
        var s = new Synth(bars * bar + 4);

        // the call: a fanfare of the trumpets on the notes of the chord, over a roll of the drum
        double[] call = { 0, 0.5, 1, 2 };
        int[] callNotes = { 72, 76, 79, 84 };
        for (int i = 0; i < 4; i++) s.Trumpet(call[i] * spb, i == 3 ? 4 * spb : spb * 0.5, callNotes[i], 0.16, 0.0, 1.0, 0.05);
        for (int i = 0; i < 8; i++) s.Snare(4 * spb + i * spb / 2, 0.07 + 0.012 * i);
        s.Crash(bar * 1.0, 0.16);
        s.Trumpet(bar, 2 * spb, 79, 0.14, 0.2, 1.0);

        for (int p = 0; p < passes; p++)
        {
            int shift = p == 2 ? 2 : 0;                // the last time, a step higher
            for (int b = 0; b < 8; b++)
            {
                int bi = intro + p * 8 + b;
                double t0 = bi * bar;
                var (root, minor) = GladBars[b];
                int[] triad = minor ? new[] { 0, 3, 7 } : new[] { 0, 4, 7 };
                int bass = 36 + root + shift; if (root >= 7) bass -= 12;

                // the rhythm: a kick on the beat, snare on the back-beat, hats in eighths
                for (int i = 0; i < 4; i++)
                {
                    if (i % 2 == 0) s.Kick(t0 + i * spb, 0.22); else s.Snare(t0 + i * spb, 0.14);
                    s.Hat(t0 + i * spb, 0.05); s.Hat(t0 + (i + 0.5) * spb, 0.035);
                }
                // the bass walks root, fifth, root, fifth
                for (int i = 0; i < 4; i++) s.Bass(t0 + i * spb, spb * 0.9, bass + (i % 2 == 1 ? 7 : 0), 0.22);
                // the piano plays the chord on the off-beats (oom-pah)
                for (int i = 0; i < 4; i++)
                    foreach (int iv in triad) s.Piano(t0 + (i + 0.5) * spb, spb * 0.4, 60 + root % 12 + shift + iv, 0.032, -0.2 + 0.2 * (iv % 3));
                // bells over the first beat of every second bar
                if (b % 2 == 0) s.Bell(t0, 96 + shift, 0.035, 0.3);

                // the tune on the trumpets; in the repeat a second trumpet a third below; in the last time the horns hold the chord
                foreach (var n in tune[b])
                {
                    double st = t0 + n.Beat * spb, dur = n.Length * spb * 0.95;
                    s.Trumpet(st, dur, n.Midi + shift, p == 0 ? 0.16 : 0.18, 0.15, 1.0, 0.04);
                    if (p >= 1) s.Trumpet(st, dur, n.Midi + shift - (minor ? 3 : 4), 0.1, -0.3, 0.85);
                }
                if (p == 2) foreach (int iv in triad) s.Trumpet(t0, bar * 0.97, 55 + root % 12 + shift + iv, 0.06, -0.5 + 0.5 * (iv % 3), 0.7);
            }
        }
        s.Crash((intro + 16) * bar, 0.18);
        s.Reverb(0.28, 0.82);
        return s.FinishLoop(bars * bar, 0.17);
    }
}

public static partial class Songs
{
    // MARK: Game over

    // D minor: Dm Bb Gm A, twice (bass, triad intervals)
    private static readonly (int bass, int[] triad)[] OverBars =
    {
        (38, new[] { 0, 3, 7 }), (34, new[] { 0, 4, 7 }), (43, new[] { 0, 3, 7 }), (45, new[] { 0, 4, 7 }),
    };

    private static Tone[][] OverTune() => new[]
    {
        new[] { new Tone(0, 77, 2), new Tone(2, 74, 2) },
        new[] { new Tone(0, 74, 2), new Tone(2, 70, 2) },
        new[] { new Tone(0, 70, 2), new Tone(2, 67, 1), new Tone(3, 65, 1) },
        new[] { new Tone(0, 64, 3), new Tone(3, 61, 1) },
    };

    /// <summary>"Luz que se apaga": the theme of the Game Over screen, slow and low, a piano falling through D minor with a tolling bell.</summary>
    public static (float[] left, float[] right) LightGoingOut()
    {
        const double bpm = 52;
        double spb = 60 / bpm, bar = 4 * spb;
        var tune = OverTune();
        const int bars = 8;
        var s = new Synth(bars * bar + 9);
        for (int b = 0; b < bars; b++)
        {
            var (bass, triad) = OverBars[b % 4];
            double t0 = b * bar;
            s.Piano(t0, bar * 0.98, bass - 12, 0.15, -0.25, 0.8);
            s.Piano(t0 + 2 * spb, spb * 1.8, bass + triad[2], 0.05, -0.1, 0.6);
            for (int i = 0; i < 3; i++) s.Piano(t0 + spb * (1 + i), spb * 1.4, bass + 12 + triad[i], 0.035, 0.1 + 0.1 * i, 0.5);
            s.Pad(t0, bar * 1.02, bass, 0.04, 0.0, 0.3);
            foreach (var n in tune[b % 4])
                s.Piano(t0 + n.Beat * spb, n.Length * spb * 1.05, n.Midi - 12, b >= 4 ? 0.13 : 0.15, 0.1, b >= 4 ? 0.8 : 0.6);
            if (b % 2 == 0) s.Bell(t0, 50, 0.08, -0.3);
        }
        s.Reverb(0.6, 0.9);
        return s.FinishLoop(bars * bar, 0.11);
    }
}
