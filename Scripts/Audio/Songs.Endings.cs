using System;

/// <summary>
/// The two songs of the endings. "Noite sem fim" (the bad ending, easy mode): a sad piano alone, slow, in A minor, the left hand falling in broken chords and a single voice above it, a pad
/// far under it like the dark of the river. "Aurora" (the ending of the normal and hard modes): a tender piano in C major, slow, the tune three times, each a little fuller.
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

    // C major, tender: C  G/B  Am  Em  F  C/E  Dm  G (bass, then the tones the left hand breaks into arpeggios)
    private static readonly (int bass, int[] tones)[] GladBars =
    {
        (36, new[] { 48, 52, 55, 60, 64 }), (35, new[] { 47, 50, 55, 59, 62 }), (33, new[] { 45, 52, 57, 60, 64 }), (40, new[] { 40, 47, 52, 55, 59 }),
        (41, new[] { 41, 48, 53, 57, 60 }), (40, new[] { 43, 48, 52, 55, 60 }), (38, new[] { 41, 50, 53, 57, 62 }), (43, new[] { 43, 50, 55, 59, 62 }),
    };

    private static Tone[][] GladTune() => new[]
    {
        new[] { new Tone(0, 72, 1), new Tone(1, 76, 1), new Tone(2, 79, 2) },
        new[] { new Tone(0, 79, 1.5), new Tone(1.5, 77, 0.5), new Tone(2, 74, 2) },
        new[] { new Tone(0, 72, 1), new Tone(1, 76, 1), new Tone(2, 81, 2) },
        new[] { new Tone(0, 79, 1.5), new Tone(1.5, 76, 0.5), new Tone(2, 71, 2) },
        new[] { new Tone(0, 77, 1), new Tone(1, 81, 1), new Tone(2, 84, 2) },
        new[] { new Tone(0, 83, 1.5), new Tone(1.5, 81, 0.5), new Tone(2, 79, 2) },
        new[] { new Tone(0, 77, 1), new Tone(1, 74, 1), new Tone(2, 69, 1), new Tone(3, 74, 1) },
        new[] { new Tone(0, 71, 2), new Tone(2, 74, 2) },
    };

    /// <summary>"Aurora": the theme of the ending of the normal and hard modes: slow and tender, a piano alone at first (the left hand in broken chords, the right singing the tune), then a second time with the tune doubled an octave above and a soft pad under it, and a third with a faint choir and the bells of the dawn. No trumpets, no drums.</summary>
    public static (float[] left, float[] right) Daybreak()
    {
        const double bpm = 62;
        double spb = 60 / bpm, bar = 4 * spb;
        var tune = GladTune();
        const int passes = 3, bars = passes * 8;
        var s = new Synth(bars * bar + 8);
        int[] arp = { 0, 2, 3, 4, 3, 2, 3, 1 };
        for (int p = 0; p < passes; p++)
            for (int b = 0; b < 8; b++)
            {
                double t0 = (p * 8 + b) * bar;
                var (bass, tones) = GladBars[b];
                // the left hand: the bass, held, and the chord broken in eighths, rising and falling
                s.Piano(t0, bar * 0.98, bass, p == 0 ? 0.12 : 0.14, -0.3, 0.5);
                for (int i = 1; i < 8; i++) s.Piano(t0 + i * spb / 2, spb * 1.5, tones[arp[i]], p == 0 ? 0.04 : 0.05, -0.2 + 0.05 * i, 0.4);
                // the right hand: the tune, warm, with a little more weight the later it is
                foreach (var n in tune[b])
                {
                    double st = t0 + n.Beat * spb, dur = n.Length * spb * 1.08;
                    s.Piano(st, dur, n.Midi, p == 0 ? 0.13 : 0.15, 0.15, p == 0 ? 0.25 : 0.1);
                    if (p >= 1) s.Piano(st, dur, n.Midi + 12, 0.05, 0.3, 0.0);
                    if (p == 2) s.Piano(st, dur, n.Midi - 12 + (n.Midi - 12 < tones[1] ? 12 : 0), 0.05, -0.1, 0.4);
                }
                if (p >= 1) { s.Pad(t0, bar * 1.02, tones[1], 0.035, 0.0, 0.5); s.Pad(t0, bar * 1.02, tones[2], 0.03, 0.2, 0.5); }
                if (p == 2)
                {
                    s.Choir(t0, bar * 0.99, tones[2], 0.05, -0.2, 2); s.Choir(t0, bar * 0.99, tones[3] + 12, 0.045, 0.2, 2);
                    if (b % 2 == 0) s.Bell(t0 + spb * 2, 91 - (b % 4 == 2 ? 2 : 0), 0.04, 0.4);
                }
            }
        s.Bell(0, 96, 0.05, 0.3);
        s.Reverb(0.6, 0.9);
        return s.FinishLoop(bars * bar, 0.12);
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
