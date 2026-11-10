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

    // G minor, after "Crimson Tower ~ Eastern Dream..." (127 BPM, G minor with the raised seventh, Gm and D over and over): bass note, then the chord tones (low to high).
    // The A part: Gm Eb Bb F  Gm Eb Cm D; the B part, in the bright side of the key: Eb Bb Cm Gm  Eb F Gm D.
    private static readonly (int bass, int[] tones)[] DawnA =
    {
        (31, new[] { 55, 58, 62 }), (27, new[] { 55, 58, 63 }), (34, new[] { 58, 62, 65 }), (29, new[] { 57, 60, 65 }),
        (31, new[] { 55, 58, 62 }), (27, new[] { 55, 58, 63 }), (36, new[] { 55, 60, 63 }), (38, new[] { 57, 62, 66 }),
    };

    private static readonly (int bass, int[] tones)[] DawnB =
    {
        (27, new[] { 55, 58, 63 }), (34, new[] { 58, 62, 65 }), (36, new[] { 55, 60, 63 }), (31, new[] { 55, 58, 62 }),
        (27, new[] { 55, 58, 63 }), (29, new[] { 57, 60, 65 }), (31, new[] { 58, 62, 67 }), (38, new[] { 57, 62, 66 }),
    };

    // the sentimental tune of the violin (A part), over Gm Eb Bb F Gm Eb Cm D
    private static Tone[][] DawnTuneA() => new[]
    {
        new[] { new Tone(0, 74, 3), new Tone(3, 72, 1) },
        new[] { new Tone(0, 70, 2), new Tone(2, 67, 1), new Tone(3, 70, 1) },
        new[] { new Tone(0, 74, 2.5), new Tone(2.5, 77, 0.5), new Tone(3, 74, 1) },
        new[] { new Tone(0, 72, 2), new Tone(2, 69, 2) },
        new[] { new Tone(0, 74, 2), new Tone(2, 79, 2) },
        new[] { new Tone(0, 77, 1.5), new Tone(1.5, 75, 0.5), new Tone(2, 74, 1), new Tone(3, 70, 1) },
        new[] { new Tone(0, 72, 2), new Tone(2, 75, 1), new Tone(3, 74, 1) },
        new[] { new Tone(0, 69, 2), new Tone(2, 72, 1), new Tone(3, 66, 1) },
    };

    // the great tune of the trumpets (B part), over Eb Bb Cm Gm Eb F Gm D
    private static Tone[][] DawnTuneB() => new[]
    {
        new[] { new Tone(0, 79, 2), new Tone(2, 77, 1), new Tone(3, 75, 1) },
        new[] { new Tone(0, 74, 2), new Tone(2, 77, 2) },
        new[] { new Tone(0, 79, 1.5), new Tone(1.5, 75, 0.5), new Tone(2, 72, 1), new Tone(3, 75, 1) },
        new[] { new Tone(0, 74, 3), new Tone(3, 70, 1) },
        new[] { new Tone(0, 79, 2), new Tone(2, 82, 1), new Tone(3, 79, 1) },
        new[] { new Tone(0, 81, 2), new Tone(2, 84, 1), new Tone(3, 81, 1) },
        new[] { new Tone(0, 82, 1.5), new Tone(1.5, 81, 0.5), new Tone(2, 79, 2) },
        new[] { new Tone(0, 78, 1), new Tone(1, 81, 1), new Tone(2, 84, 2) },
    };

    /// <summary>
    /// "Aurora": the theme of the ending of the normal and hard modes, an epic and tender piece after ZUN's "Crimson Tower ~ Eastern Dream..." (the same 127 BPM and the same G minor, with its Gm and D):
    /// a call of horns and a trumpet opens it over strings; the violin sings the sad tune (twice, the second with a horn under it); the trumpets and the horns carry the great tune with the whole orchestra,
    /// the drums and the choir; a quiet bridge of the violin alone with the piano; the tune again, fuller; the last great tune with everything; and it closes in G major (the dawn), the bell ringing.
    /// </summary>
    public static (float[] left, float[] right) Daybreak()
    {
        const double bpm = 127;
        double spb = 60 / bpm, bar = 4 * spb;
        // the parts, in bars: intro 4, A 8, A 8, B 8, bridge 4, A 8, B 8, close 4
        const int intro = 4, partA = 8, partB = 8, bridge = 4, close = 4;
        int a1 = intro, a2 = a1 + partA, b1 = a2 + partA, br = b1 + partB, a3 = br + bridge, b2 = a3 + partA, end = b2 + partB, bars = end + close;
        var s = new Synth(bars * bar + 9);
        var tuneA = DawnTuneA(); var tuneB = DawnTuneB();

        // the pads of the strings and the bass, the harp-like arpeggio of the piano: bar by bar
        void Ground(int firstBar, (int bass, int[] tones)[] chords, int level, bool arp)
        {
            for (int b = 0; b < 8; b++)
            {
                double t0 = (firstBar + b) * bar;
                var (bass, tones) = chords[b];
                s.Bass(t0, bar * 0.97, bass, 0.1 + 0.025 * level);
                foreach (int m in tones) s.Pad(t0, bar * 1.02, m, 0.02 + 0.006 * level, (m % 5 - 2) * 0.12, 0.6);
                for (int k = 0; k < tones.Length; k++) s.Violin(t0, bar * 1.0, tones[k] + (level >= 2 ? 12 : 0), 0.018 + 0.006 * level, -0.4 + 0.4 * k, 0.5);
                if (arp)
                    for (int i = 0; i < 8; i++)
                        s.Piano(t0 + i * spb / 2, spb * 1.4, tones[i % 3] + (i % 6 >= 3 ? 12 : 0) + (i == 6 ? 12 : 0), 0.034 + 0.008 * Math.Min(level, 2), -0.2 + 0.05 * i, 0.3);
            }
        }

        // INTRO: the call of the horns (the fifth, then the fourth above), the trumpet far off, a pad of strings; the harp
        for (int b = 0; b < intro; b++)
        {
            double t0 = b * bar;
            var (bass, tones) = new[] { DawnA[0], DawnA[1], DawnA[2], DawnA[7] }[b];
            s.Bass(t0, bar * 0.97, bass, 0.09);
            foreach (int m in tones) { s.Pad(t0, bar * 1.02, m, 0.026, (m % 5 - 2) * 0.1, 0.6); s.Violin(t0, bar, m + 12, 0.016 + 0.004 * b, (m % 3 - 1) * 0.3, 0.5); }
            for (int i = 0; i < 4; i++) s.Piano(t0 + i * spb, spb * 1.6, tones[i % 3] + 12, 0.03 + 0.005 * b, 0.15 - 0.1 * i, 0.4);
        }
        s.Horn(0, bar * 1.9, 55, 0.05, -0.2); s.Horn(bar * 2, bar * 1.9, 62, 0.055, 0.2);
        s.Horn(bar * 2, bar * 1.9, 55, 0.04, -0.3);
        s.Trumpet(bar * 3, spb * 1.4, 67, 0.06, 0.15, 0.7); s.Trumpet(bar * 3 + spb * 1.5, spb * 0.5, 74, 0.06, 0.15, 0.7); s.Trumpet(bar * 3 + spb * 2, spb * 2, 79, 0.075, 0.15, 0.8);
        s.Taiko(3 * bar + 3 * spb, 38, 0.1); s.Taiko(3 * bar + 3.5 * spb, 38, 0.12);

        // A, the first time: the violin alone with the piano's arpeggio and the pads
        Ground(a1, DawnA, 0, true);
        for (int b = 0; b < partA; b++)
            foreach (var n in tuneA[b])
                s.Violin((a1 + b) * bar + n.Beat * spb, n.Length * spb * 1.04, n.Midi, 0.11, 0.1, 1.0);
        // A, the second time: the violin (with a second one a third below in places), the horn under, the strings fuller, the first soft drums
        Ground(a2, DawnA, 1, true);
        for (int b = 0; b < partA; b++)
        {
            foreach (var n in tuneA[b])
            {
                double st = (a2 + b) * bar + n.Beat * spb, dur = n.Length * spb * 1.04;
                s.Violin(st, dur, n.Midi, 0.12, 0.1, 1.1);
                s.Violin(st, dur, n.Midi - (b % 2 == 0 ? 4 : 3), 0.05, -0.15, 0.9);
                if (b >= 4) s.Trumpet(st, dur, n.Midi - 12, 0.045, 0.2, 0.55);
            }
            s.Horn((a2 + b) * bar, bar * 0.98, DawnA[b].tones[0] - 12, 0.07, -0.25);
            if (b % 2 == 0) { s.Taiko((a2 + b) * bar, 38, 0.1); }
            if (b == partA - 1) for (int i = 0; i < 8; i++) s.Tom((a2 + b) * bar + (2 + i * 0.25) * spb, 41 - i / 3, 0.05 + 0.025 * i, -0.3 + 0.08 * i);
        }

        // B: the great tune, in trumpets and horns with the violins an octave above, the whole orchestra and the drums
        Ground(b1, DawnB, 2, true);
        for (int b = 0; b < partB; b++)
        {
            double t0 = (b1 + b) * bar;
            foreach (var n in tuneB[b])
            {
                double st = t0 + n.Beat * spb, dur = n.Length * spb * 1.0;
                s.Trumpet(st, dur, n.Midi, 0.12, 0.05, 1.0);
                s.Trumpet(st, dur, n.Midi - 5 + (b % 4 == 3 ? 1 : 0), 0.06, 0.25, 0.8);
                s.Violin(st, dur, n.Midi + 12 > 96 ? n.Midi : n.Midi + 12, 0.07, -0.1, 1.2);
                s.Horn(st, dur, n.Midi - 12, 0.075, -0.3);
            }
            s.Horn(t0, bar * 0.98, DawnB[b].tones[0] - 12, 0.1, 0.2);
            s.Taiko(t0, 38, 0.2); s.Taiko(t0 + 2 * spb, 38, 0.16);
            s.Kick(t0 + 1.5 * spb, 0.06); 
            if (b % 2 == 1) s.Crash(t0, 0.05);
            s.Choir(t0, bar * 0.99, DawnB[b].tones[1], 0.05, -0.2, 0); s.Choir(t0, bar * 0.99, DawnB[b].tones[2] + 12, 0.045, 0.2, 0);
        }
        s.Crash(b1 * bar, 0.09);

        // the bridge: the violin alone, close, with the piano; Cm Gm Eb D
        var bridgeChords = new[] { DawnB[2], DawnB[3], DawnB[0], DawnA[7] };
        var bridgeTune = new[]
        {
            new[] { new Tone(0, 79, 2), new Tone(2, 75, 1), new Tone(3, 72, 1) },
            new[] { new Tone(0, 74, 3), new Tone(3, 70, 1) },
            new[] { new Tone(0, 75, 2), new Tone(2, 79, 2) },
            new[] { new Tone(0, 78, 2), new Tone(2, 74, 2) },
        };
        for (int b = 0; b < bridge; b++)
        {
            double t0 = (br + b) * bar;
            var (bass, tones) = bridgeChords[b];
            s.Bass(t0, bar * 0.97, bass, 0.07);
            foreach (int m in tones) s.Pad(t0, bar * 1.02, m, 0.02, (m % 5 - 2) * 0.1, 0.5);
            for (int i = 0; i < 8; i++) s.Piano(t0 + i * spb / 2, spb * 1.4, tones[i % 3] + (i % 6 >= 3 ? 12 : 0), 0.03, -0.2 + 0.05 * i, 0.4);
            foreach (var n in bridgeTune[b]) s.Violin(t0 + n.Beat * spb, n.Length * spb * 1.05, n.Midi, 0.12 + 0.01 * b, 0.1, 1.5);
        }
        s.Taiko((br + bridge - 1) * bar + 2 * spb, 38, 0.1); s.Taiko((br + bridge - 1) * bar + 3 * spb, 38, 0.14);
        for (int i = 0; i < 8; i++) s.Tom((br + bridge - 1) * bar + (2 + i * 0.25) * spb, 43 - i / 3, 0.05 + 0.03 * i, -0.3 + 0.08 * i);

        // A, the third time: the violins in unison with the trumpet, the horns, everything swelling
        Ground(a3, DawnA, 2, true);
        for (int b = 0; b < partA; b++)
        {
            double t0 = (a3 + b) * bar;
            foreach (var n in tuneA[b])
            {
                double st = t0 + n.Beat * spb, dur = n.Length * spb * 1.04;
                s.Violin(st, dur, n.Midi, 0.12, 0.1, 1.2); s.Violin(st, dur, n.Midi + 12, 0.07, -0.1, 1.2);
                s.Trumpet(st, dur, n.Midi, 0.075, 0.25, 0.65);
                s.Horn(st, dur, n.Midi - 12, 0.07, -0.3);
            }
            s.Horn(t0, bar * 0.98, DawnA[b].tones[0] - 12, 0.08, 0.2);
            s.Taiko(t0, 38, 0.15); if (b % 2 == 1) s.Taiko(t0 + 2 * spb, 38, 0.12);
            s.Choir(t0, bar * 0.99, DawnA[b].tones[1], 0.035, -0.2, 0);
        }

        // B, the last time: the great tune with everything, the choir, the organ, the drums, a fanfare of trumpets at the end
        Ground(b2, DawnB, 3, true);
        for (int b = 0; b < partB; b++)
        {
            double t0 = (b2 + b) * bar;
            foreach (var n in tuneB[b])
            {
                double st = t0 + n.Beat * spb, dur = n.Length * spb * 1.0;
                s.Trumpet(st, dur, n.Midi, 0.14, 0.05, 1.1);
                s.Trumpet(st, dur, n.Midi - 4 - (b % 2), 0.075, 0.25, 0.9);
                s.Trumpet(st, dur, n.Midi - 12, 0.06, -0.25, 0.8);
                s.Violin(st, dur, n.Midi + (n.Midi + 12 > 96 ? 0 : 12), 0.085, -0.1, 1.3);
                s.Horn(st, dur, n.Midi - 12, 0.1, -0.3);
                s.Organ(st, dur, n.Midi - 12, 0.03, 0);
            }
            s.Horn(t0, bar * 0.98, DawnB[b].tones[0] - 12, 0.12, 0.2);
            s.Organ(t0, bar * 0.99, DawnB[b].bass + 12, 0.05, -0.1);
            for (int k = 0; k < 4; k++) s.Taiko(t0 + k * spb, 38 + (k == 3 ? 5 : 0), k % 2 == 0 ? 0.22 : 0.15);
            s.Kick(t0, 0.07); s.Snare(t0 + 2 * spb, 0.04);
            s.Crash(t0, b % 2 == 0 ? 0.08 : 0.04);
            s.Choir(t0, bar * 0.99, DawnB[b].tones[1], 0.06, -0.2, 0); s.Choir(t0, bar * 0.99, DawnB[b].tones[2] + 12, 0.055, 0.2, 0); s.Choir(t0, bar * 0.99, DawnB[b].tones[0] + 12, 0.045, 0, 0);
        }
        // the fanfare in the last bar of B: a run of the trumpets up to the dominant
        double fan = (b2 + partB - 1) * bar;
        int[] run = { 74, 78, 81, 86 };
        for (int i = 0; i < run.Length; i++) s.Trumpet(fan + (2 + i * 0.5) * spb, i == 3 ? spb * 2 : spb * 0.45, run[i], 0.1, 0.1, 1.1);

        // the close: G major, the dawn: G  C  D  G; the tune is long notes, the bell rings
        var closeChords = new (int bass, int[] tones)[] { (31, new[] { 55, 59, 62 }), (36, new[] { 55, 60, 64 }), (38, new[] { 57, 62, 66 }), (31, new[] { 55, 59, 62 }) };
        int[] closeTune = { 86, 84, 81, 91 };
        for (int b = 0; b < close; b++)
        {
            double t0 = (end + b) * bar; bool last = b == close - 1;
            var (bass, tones) = closeChords[b];
            double len = last ? bar * 1.15 : bar;
            s.Bass(t0, len * 0.97, bass, 0.12);
            foreach (int m in tones)
            {
                s.Pad(t0, len, m, 0.035, (m % 5 - 2) * 0.1, 0.8); s.Violin(t0, len, m + 12, 0.04, (m % 3 - 1) * 0.3, 0.8); s.Horn(t0, len, m - 12, 0.07, (m % 3 - 1) * 0.3);
                s.Choir(t0, len * 0.99, m + 12, 0.05, (m % 5 - 2) * 0.1, 0);
            }
            double sd = last ? bar * 1.05 : bar * 0.99;
            s.Trumpet(t0, sd, closeTune[b] - 12 * (b == 3 ? 1 : 0), 0.12, 0.1, 1.0);
            s.Violin(t0, sd, closeTune[b] - (b == 3 ? 12 : 0) + (b == 3 ? 0 : 0), 0.1, 0.0, 1.2);
            s.Organ(t0, len, bass + 12, 0.05);
            s.Taiko(t0, 38, 0.2); if (last) { s.Crash(t0, 0.1); s.Bell(t0 + 0.2, 91, 0.05, 0.3); s.Bell(t0 + 1.5, 95, 0.04, -0.3); }
            for (int i = 0; i < 4; i++) s.Piano(t0 + i * spb, spb * 1.6, tones[i % 3] + 12, 0.03, 0.1, 0.3);
        }
        s.Reverb(0.5, 0.88);
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
