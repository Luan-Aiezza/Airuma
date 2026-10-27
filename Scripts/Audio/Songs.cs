using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The two themes of stage 1, in the manner of ZUN's music for Touhou (minor keys, singing melodies, brass trumpets and driving
/// drums for the boss). Both are in D minor and share the same tune, so the boss's theme is the stage's own song grown angry.
///
/// "Quiet Water" (the stage, 96 BPM): the river at night. A plucked harp rocks over a string pad, a flute sings a tune that
/// rises and falls like the current; underneath, something large turns over (the low bass).
/// "Ember Serpent" (the boss, 156 BPM): the same tune, now in trumpets, over a pounding beat; a slithering chromatic line stands
/// for the fire serpent under the river.
/// </summary>
public static class Songs
{
    private readonly struct Chord
    {
        public readonly int Bass;
        public readonly int[] Tones;
        public Chord(int bass, params int[] tones) { Bass = bass; Tones = tones; }
    }

    private static readonly Chord Dm = new(38, 50, 57, 62, 65), Bb = new(46, 53, 58, 62, 65), F = new(41, 53, 57, 60, 65), C = new(48, 52, 55, 60, 64);
    private static readonly Chord Gm = new(43, 55, 58, 62, 67), A = new(45, 52, 57, 61, 64);

    private readonly struct Note
    {
        public readonly double Beat; public readonly int Midi; public readonly double Len;
        public Note(double beat, int midi, double len) { Beat = beat; Midi = midi; Len = len; }
    }

    // The tune, in 8-bar parts; beats are counted from the start of the part.
    private static readonly Note[][] TuneA =
    {
        new[] { new Note(0, 69, 1.5), new Note(1.5, 74, 0.5), new Note(2, 77, 2) },
        new[] { new Note(0, 74, 1), new Note(1, 77, 1), new Note(2, 76, 1), new Note(3, 74, 1) },
        new[] { new Note(0, 72, 1.5), new Note(1.5, 69, 0.5), new Note(2, 72, 1), new Note(3, 74, 1) },
        new[] { new Note(0, 76, 2), new Note(2, 74, 1), new Note(3, 72, 1) },
        new[] { new Note(0, 69, 1), new Note(1, 74, 1), new Note(2, 77, 1), new Note(3, 81, 1) },
        new[] { new Note(0, 79, 1.5), new Note(1.5, 77, 0.5), new Note(2, 74, 2) },
        new[] { new Note(0, 74, 1), new Note(1, 79, 1), new Note(2, 82, 1), new Note(3, 81, 1) },
        new[] { new Note(0, 81, 1), new Note(1, 85, 1), new Note(2, 81, 2) },
    };
    private static readonly Note[][] TuneB =
    {
        new[] { new Note(0, 77, 2), new Note(2, 74, 2) },
        new[] { new Note(0, 76, 2), new Note(2, 79, 2) },
        new[] { new Note(0, 77, 1), new Note(1, 81, 1), new Note(2, 86, 2) },
        new[] { new Note(0, 84, 1.5), new Note(1.5, 81, 0.5), new Note(2, 77, 2) },
        new[] { new Note(0, 86, 1), new Note(1, 84, 1), new Note(2, 82, 1), new Note(3, 81, 1) },
        new[] { new Note(0, 79, 2), new Note(2, 76, 1), new Note(3, 79, 1) },
        new[] { new Note(0, 81, 1.5), new Note(1.5, 79, 0.5), new Note(2, 77, 1), new Note(3, 76, 1) },
        new[] { new Note(0, 74, 4) },
    };
    private static readonly Note[][] TuneBridge =
    {
        new[] { new Note(0, 70, 4) }, new[] { new Note(0, 73, 4) },
        new[] { new Note(0, 74, 2), new Note(2, 77, 2) }, new[] { new Note(0, 76, 2), new Note(2, 73, 2) },
    };

    private static readonly Chord[] ChordsIntro = { Dm, Bb, F, C };
    private static readonly Chord[] ChordsA = { Dm, Bb, F, C, Dm, Bb, Gm, A };
    private static readonly Chord[] ChordsB = { Bb, C, Dm, F, Bb, C, A, A };
    private static readonly Chord[] ChordsBridge = { Gm, A, Bb, A };

    /// <summary>A note a third or so below the tune that belongs to the chord (for brass and flutes in parallel).</summary>
    private static int Below(int midi, Chord chord)
    {
        for (int d = 3; d <= 9; d++)
            if (chord.Tones.Any(t => ((t - (midi - d)) % 12 + 12) % 12 == 0)) return midi - d;
        return midi - 5;
    }

    private static void PlayTune(Action<double, double, int, Chord, int> play, Note[][] tune, Chord[] chords, double barStart, double bar, double spb)
    {
        for (int b = 0; b < tune.Length; b++)
            foreach (var n in tune[b])
                play(barStart + b * bar + n.Beat * spb, n.Len * spb, n.Midi, chords[b], b);
    }

    // MARK: The stage

    public static (float[] left, float[] right) QuietWater()
    {
        const double bpm = 96;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(ChordsIntro); chords.AddRange(ChordsA); chords.AddRange(ChordsB); chords.AddRange(ChordsA); chords.AddRange(ChordsBridge);
        int bars = chords.Count; // 32
        var s = new Synth(bars * bar + 5);

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < 4, bridge = b >= 28;

            // pad
            for (int i = 0; i < ch.Tones.Length; i++) s.Pad(t0, bar * 1.04, ch.Tones[i], intro ? 0.034 : 0.045, -0.5 + i * 0.33, 1.1);

            // harp: eighth-note arpeggio, rocking like water
            int[] order = { 0, 1, 2, 3, 2, 1, 3, 2 };
            for (int i = 0; i < 8; i++)
            {
                int midi = ch.Tones[order[i]] + 12;
                s.Pluck(t0 + i * spb / 2, midi, (i % 4 == 0 ? 0.11 : 0.075) * (intro ? 0.8 : 1), -0.3 + (i % 4) * 0.2, 1.0);
            }

            // droplets: now and then a high, bell-like note
            if (b % 2 == 0)
            {
                int[] drops = { 86, 88, 91, 93, 89 };
                s.Pluck(t0 + 3.25 * spb, drops[(b / 2) % drops.Length], 0.05, 0.5, 1.6);
                if (intro || bridge) s.Pluck(t0 + 1.5 * spb, drops[(b / 2 + 2) % drops.Length], 0.04, -0.6, 1.6);
            }

            // bass: from the third bar; two long notes a bar, the second one lifting
            if (b >= 2)
            {
                if (bridge) s.Bass(t0, bar * 0.98, ch.Bass, 0.24);
                else
                {
                    s.Bass(t0, 1.9 * spb, ch.Bass, 0.26);
                    s.Bass(t0 + 2 * spb, 1.4 * spb, ch.Bass + (b % 2 == 0 ? 7 : 0), 0.2);
                }
            }

            // drums, very soft
            if (b >= 4 && !bridge)
            {
                s.Kick(t0, 0.28); s.Kick(t0 + 2 * spb, 0.2);
                if (b >= 12) { s.Snare(t0 + spb, 0.06); s.Snare(t0 + 3 * spb, 0.07); }
                if (b >= 12) for (int i = 0; i < 8; i++) s.Hat(t0 + i * spb / 2, i % 2 == 0 ? 0.05 : 0.03, 0.3);
            }
        }

        // the tune: flute
        Action<double, double, int, Chord, int> flute = (t, d, m, c, b) => s.Flute(t, d * 0.97, m, 0.17, 0.12);
        Action<double, double, int, Chord, int> fluteHarm = (t, d, m, c, b) => s.Flute(t, d * 0.97, Below(m, c), 0.1, -0.35);
        PlayTune(flute, TuneA, ChordsA, 4 * bar, bar, spb);
        PlayTune(flute, TuneB, ChordsB, 12 * bar, bar, spb);
        PlayTune(flute, TuneA, ChordsA, 20 * bar, bar, spb);
        PlayTune(fluteHarm, TuneA, ChordsA, 20 * bar, bar, spb);
        PlayTune((t, d, m, c, b) => s.Flute(t, d * 0.99, m, 0.14, 0.05), TuneBridge, ChordsBridge, 28 * bar, bar, spb);

        s.Reverb(0.30, 0.82);
        return s.FinishLoop(bars * bar, 0.12);
    }

    // MARK: The boss

    public static (float[] left, float[] right) EmberSerpent()
    {
        const double bpm = 156;
        double spb = 60 / bpm, bar = 4 * spb;
        var chordsIntro = new[] { Dm, Dm, Bb, A };
        var chordsC = new[] { Dm, Dm, Bb, A, Dm, Gm, A, A };
        var chords = new List<Chord>();
        chords.AddRange(chordsIntro); chords.AddRange(ChordsA); chords.AddRange(ChordsB); chords.AddRange(chordsC); chords.AddRange(ChordsA); chords.AddRange(ChordsB);
        int bars = chords.Count; // 44
        var s = new Synth(bars * bar + 5);

        // section starts (bars)
        int A1 = 4, B1 = 12, C1 = 20, A2 = 28, B2 = 36;

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < 4, serpent = b >= C1 && b < A2, last = b >= A2;
            bool chorus = (b >= B1 && b < C1) || b >= B2;

            // strings
            double padAmp = intro ? 0.03 : (chorus ? 0.065 : 0.05);
            for (int i = 0; i < ch.Tones.Length; i++) s.Pad(t0, bar * 1.03, ch.Tones[i] + (chorus ? 0 : 0), padAmp, -0.55 + i * 0.37, 1.6);

            // bass: a driving eighth-note pulse (half-time in the serpent's part)
            if (b >= 2)
            {
                if (serpent)
                {
                    s.Bass(t0, 1.9 * spb, ch.Bass - 12 + 12, 0.3, true);
                    s.Bass(t0 + 2 * spb, 1.9 * spb, ch.Bass, 0.26, true);
                }
                else if (b >= 4)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        int note = ch.Bass + (i == 3 || i == 7 ? 12 : 0);
                        s.Bass(t0 + i * spb / 2, spb * 0.45, note, i % 2 == 0 ? 0.26 : 0.2, true);
                    }
                }
                else s.Bass(t0, bar, ch.Bass - 12 + 12, 0.3, true);
            }

            // drums
            if (b == 2 || b == 3)
            {
                // a snare roll that grows
                int steps = 16;
                for (int i = 0; i < steps; i++) s.Snare(t0 + i * spb / 4, 0.05 + 0.1 * ((b - 2) * steps + i) / (2.0 * steps), 0.1);
                if (b == 2) s.Kick(t0, 0.35);
            }
            else if (b >= 4)
            {
                bool section = b == A1 || b == B1 || b == C1 || b == A2 || b == B2;
                if (section) s.Crash(t0, 0.18);
                if (serpent)
                {
                    s.Kick(t0, 0.4); s.Snare(t0 + 2 * spb, 0.2);
                    for (int i = 0; i < 8; i++) s.Hat(t0 + i * spb / 2, 0.04, 0.3);
                    s.Tom(t0 + 3 * spb, 45, 0.18, -0.3); s.Tom(t0 + 3.5 * spb, 43, 0.18, 0.3);
                }
                else
                {
                    s.Kick(t0, 0.42); s.Kick(t0 + 1.5 * spb, 0.3); s.Kick(t0 + 2.5 * spb, 0.36); if (b % 2 == 1) s.Kick(t0 + 3.5 * spb, 0.3);
                    s.Snare(t0 + spb, 0.24); s.Snare(t0 + 3 * spb, 0.26);
                    for (int i = 0; i < 8; i++) s.Hat(t0 + i * spb / 2, i % 2 == 0 ? 0.07 : 0.045, 0.3, i == 7 && b % 2 == 1);
                    if (chorus || last) for (int i = 0; i < 16; i++) if (i % 2 == 1) s.Hat(t0 + i * spb / 4, 0.025, -0.3);
                }
                // a tom fill on the last beat before a new part
                if (b + 1 == B1 || b + 1 == C1 || b + 1 == A2 || b + 1 == B2)
                {
                    s.Tom(t0 + 3 * spb, 50, 0.28, -0.4); s.Tom(t0 + 3.25 * spb, 47, 0.28, -0.1);
                    s.Tom(t0 + 3.5 * spb, 45, 0.3, 0.2); s.Tom(t0 + 3.75 * spb, 40, 0.32, 0.4);
                }
            }

            // arpeggio sparks in the last parts
            if (last)
                for (int i = 0; i < 16; i++) s.Pluck(t0 + i * spb / 4, ch.Tones[(i * 3) % 4] + 24, 0.045, 0.6 - (i % 4) * 0.4, 0.5);
        }

        // the fanfare in the introduction: the first notes of the tune, rising
        s.Tom(0, 40, 0.3, -0.2); s.Tom(1 * spb, 40, 0.25, 0.2); s.Tom(2 * spb, 38, 0.3, 0); s.Tom(3 * spb, 38, 0.3, 0);
        s.Crash(0, 0.2);
        s.Bass(0, 2 * bar, 26, 0.34, true);
        foreach (var (beat, midi, len) in new[] { (0.0, 69, 1.0), (1.0, 74, 1.0), (2.0, 77, 1.0), (3.0, 81, 1.0) })
            s.Trumpet(2 * bar + beat * spb, len * spb * 0.9, midi - 12, 0.12, -0.2, 0.8);
        foreach (var (beat, midi, len) in new[] { (0.0, 77, 1.0), (1.0, 82, 1.0), (2.0, 86, 2.0) })
            s.Trumpet(2 * bar + (4 + beat) * spb, len * spb * 0.9, midi - 12, 0.2, 0.15);
        s.Trumpet(3 * bar + 0, 2 * spb, 85 - 12, 0.2, 0.1); s.Trumpet(3 * bar + 2 * spb, spb, 81 - 12, 0.2, 0.1); s.Trumpet(3 * bar + 3 * spb, spb, 85 - 12, 0.22, 0.1);

        // the tune, now in brass: lead one octave down, a parallel voice, and (in the choruses) the high octave too
        Action<double, double, int, Chord, int> lead = (t, d, m, c, b) => s.Trumpet(t, d * 0.82, m - 12, 0.2, 0.15);
        Action<double, double, int, Chord, int> harm = (t, d, m, c, b) => s.Trumpet(t, d * 0.82, Below(m - 12, c), 0.13, -0.4, 0.9);
        Action<double, double, int, Chord, int> high = (t, d, m, c, b) => s.Trumpet(t, d * 0.85, m, 0.1, 0.35, 1.1);
        Action<double, double, int, Chord, int> deep = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, m - 24, 0.1, -0.1, 0.7);

        PlayTune(lead, TuneA, ChordsA, A1 * bar, bar, spb);
        PlayTune(harm, TuneA, ChordsA, A1 * bar, bar, spb);

        foreach (int start in new[] { B1, B2 })
        {
            PlayTune(lead, TuneB, ChordsB, start * bar, bar, spb);
            PlayTune(harm, TuneB, ChordsB, start * bar, bar, spb);
            PlayTune(high, TuneB, ChordsB, start * bar, bar, spb);
            PlayTune(deep, TuneB, ChordsB, start * bar, bar, spb);
        }

        PlayTune(lead, TuneA, ChordsA, A2 * bar, bar, spb);
        PlayTune(harm, TuneA, ChordsA, A2 * bar, bar, spb);
        PlayTune(high, TuneA, ChordsA, A2 * bar, bar, spb);
        PlayTune(deep, TuneA, ChordsA, A2 * bar, bar, spb);

        // the serpent: a chromatic line that slithers up and back, over stabs of brass on the offbeats
        int[] slither = { 0, 2, 3, 4, 5, 4, 3, 2 };   // D E F F# G F# F E, relative to the root
        for (int b = 0; b < 8; b++)
        {
            var ch = chordsC[b];
            double t0 = (C1 + b) * bar;
            int root = ch.Bass + 24;
            for (int i = 0; i < 8; i++)
            {
                int m = root + slither[i];
                s.Trumpet(t0 + i * spb / 2, spb * 0.42, m, 0.1, i % 2 == 0 ? -0.35 : 0.35, 0.6);
                s.Pad(t0 + i * spb / 2, spb * 0.5, m - 12, 0.02, 0.0, 2.0);
            }
            s.Trumpet(t0 + 1.5 * spb, spb * 0.4, ch.Tones[2] + 0, 0.17, 0.1, 1.2);
            s.Trumpet(t0 + 3.5 * spb, spb * 0.4, ch.Tones[2] + 0, 0.17, 0.1, 1.2);
        }

        s.Reverb(0.2, 0.78);
        return s.FinishLoop(bars * bar, 0.17);
    }
    // MARK: The boss, in the manner of Rumia's theme

    // G# minor, as in the theme that inspired it. The tune below is new; what it borrows from "Rumia's Theme" is the character:
    // a fast pulse of eighth notes in a 3 + 3 + 2 pattern in the bass, sixteenth-note arpeggios in the middle, hammered chords
    // under a bright, jumping brass melody, and a snare that stresses the weak beats.
    private static readonly Chord Gsm = new(44, 56, 59, 63, 68), E = new(40, 56, 59, 64, 68), Fs = new(42, 58, 61, 66, 70);
    private static readonly Chord Dsm = new(39, 54, 58, 63, 66), Dsn = new(39, 55, 58, 63, 67), Bn = new(47, 54, 59, 63, 66), Csm = new(37, 56, 61, 64, 68);

    private static readonly Note[][] RumiaA =
    {
        new[] { new Note(0, 80, 1.5), new Note(1.5, 78, 0.5), new Note(2, 75, 1), new Note(3, 71, 1) },
        new[] { new Note(0, 76, 1.5), new Note(1.5, 80, 0.5), new Note(2, 83, 2) },
        new[] { new Note(0, 82, 1.5), new Note(1.5, 80, 0.5), new Note(2, 78, 1), new Note(3, 73, 1) },
        new[] { new Note(0, 75, 2), new Note(2, 68, 1), new Note(3, 71, 1) },
        new[] { new Note(0, 83, 1), new Note(1, 80, 0.5), new Note(1.5, 76, 0.5), new Note(2, 80, 2) },
        new[] { new Note(0, 82, 1.5), new Note(1.5, 78, 0.5), new Note(2, 73, 1), new Note(3, 78, 1) },
        new[] { new Note(0, 75, 1), new Note(1, 78, 1), new Note(2, 83, 2) },
        new[] { new Note(0, 82, 1), new Note(1, 79, 1), new Note(2, 75, 2) },
    };
    private static readonly Note[][] RumiaB =
    {
        new[] { new Note(0, 83, 2), new Note(2, 80, 1), new Note(3, 83, 1) },
        new[] { new Note(0, 85, 2), new Note(2, 82, 2) },
        new[] { new Note(0, 87, 1.5), new Note(1.5, 83, 0.5), new Note(2, 80, 2) },
        new[] { new Note(0, 78, 1), new Note(1, 82, 1), new Note(2, 85, 2) },
        new[] { new Note(0, 83, 1.5), new Note(1.5, 80, 0.5), new Note(2, 76, 2) },
        new[] { new Note(0, 78, 1), new Note(1, 82, 1), new Note(2, 85, 1), new Note(3, 82, 1) },
        new[] { new Note(0, 87, 2), new Note(2, 85, 1), new Note(3, 82, 1) },
        new[] { new Note(0, 79, 1), new Note(1, 82, 1), new Note(2, 87, 2) },
    };
    private static readonly Chord[] RumiaChordsA = { Gsm, E, Fs, Gsm, E, Fs, Bn, Dsn };
    private static readonly Chord[] RumiaChordsB = { E, Fs, Gsm, Dsm, E, Fs, Dsn, Dsn };
    private static readonly Chord[] RumiaChordsC = { Gsm, Gsm, E, Fs, Gsm, Csm, Dsn, Dsn };
    private static readonly Chord[] RumiaChordsIntro = { Gsm, Gsm, E, Dsn };

    /// <summary>"Inferno Serpent": the boss's theme. 150 BPM, G# minor; a tresillo bass, sixteenth arpeggios, stacked trumpets and
    /// hard drums, with a slithering break in the middle. Inspired by (not copied from) Touhou 6's theme for Rumia.</summary>
    public static (float[] left, float[] right) InfernoSerpent()
    {
        const double bpm = 150;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(RumiaChordsIntro); chords.AddRange(RumiaChordsA); chords.AddRange(RumiaChordsB); chords.AddRange(RumiaChordsC); chords.AddRange(RumiaChordsA); chords.AddRange(RumiaChordsB);
        int bars = chords.Count; // 44
        var s = new Synth(bars * bar + 5);
        int A1 = 4, B1 = 12, C1 = 20, A2 = 28, B2 = 36;
        int[] tresillo = { 0, 1, 3, 4, 6, 7 };   // which eighths of the bar the bass hits (the others rest)

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < 4, serpent = b >= C1 && b < A2, chorus = (b >= B1 && b < C1) || b >= B2, finale = b >= A2;

            // strings and sustained brass
            for (int i = 0; i < ch.Tones.Length; i++)
            {
                s.Pad(t0, bar * 1.03, ch.Tones[i], intro ? 0.035 : (chorus ? 0.075 : 0.055), -0.55 + i * 0.37, 1.8);
                if (chorus) s.Trumpet(t0, bar * 0.95, ch.Tones[i] - 12, 0.045, -0.3 + i * 0.2, 0.55);
            }

            // bass: the tresillo pulse, with the octave leaping on the last hit
            if (b >= 0)
            {
                if (serpent)
                {
                    s.Bass(t0, 1.9 * spb, ch.Bass, 0.34, true); s.Bass(t0 + 2 * spb, 1.9 * spb, ch.Bass, 0.3, true);
                }
                else
                {
                    for (int k = 0; k < tresillo.Length; k++)
                    {
                        int i = tresillo[k];
                        int note = ch.Bass + (k == 5 ? 12 : 0);
                        s.Bass(t0 + i * spb / 2, spb * 0.42, note, i % 2 == 0 ? 0.31 : 0.23, true);
                    }
                }
            }

            // sixteenth arpeggio in the middle, like the glittering line in the original's inner voice
            if (!serpent && b >= 2)
                for (int i = 0; i < 16; i++)
                {
                    int[] order = { 0, 2, 1, 3, 1, 2, 3, 2 };
                    s.Pluck(t0 + i * spb / 4, ch.Tones[order[i % 8]] + 12, (i % 4 == 0 ? 0.075 : 0.05) * (chorus || finale ? 1.2 : 1), -0.4 + (i % 4) * 0.27, 0.35);
                }

            // drums
            if (b == 2 || b == 3)
            {
                for (int i = 0; i < 24; i++) s.Snare(t0 + i * spb / 6, 0.06 + 0.16 * ((b - 2) * 24 + i) / 48.0, 0.1);
                if (b == 3) { s.Tom(t0 + 3 * spb, 50, 0.3, -0.4); s.Tom(t0 + 3.25 * spb, 47, 0.3, -0.1); s.Tom(t0 + 3.5 * spb, 45, 0.32, 0.2); s.Tom(t0 + 3.75 * spb, 40, 0.34, 0.4); }
            }
            else if (b < 2)
            {
                for (int i = 0; i < 16; i++) s.Hat(t0 + i * spb / 4, i % 4 == 0 ? 0.06 : 0.035, 0.3);
                s.Kick(t0, 0.4); s.Kick(t0 + 2 * spb, 0.34);
            }
            else if (b >= 4)
            {
                bool section = b == A1 || b == B1 || b == C1 || b == A2 || b == B2;
                if (section) s.Crash(t0, 0.22);
                if (serpent)
                {
                    s.Kick(t0, 0.46); s.Kick(t0 + 1.5 * spb, 0.36); s.Snare(t0 + 2 * spb, 0.3);
                    for (int i = 0; i < 8; i++) s.Hat(t0 + i * spb / 2, 0.05, 0.3);
                    s.Tom(t0 + 3 * spb, 45, 0.2, -0.3); s.Tom(t0 + 3.5 * spb, 43, 0.2, 0.3);
                }
                else
                {
                    // kicks that stress the "a" of beats 1 and 3, a snare on the offbeats, hats in sixteenths
                    double[] kicks = { 0, 0.75, 2, 2.75, 1.5, 3.5 };
                    foreach (var k in kicks) s.Kick(t0 + k * spb, k % 1 == 0 ? 0.46 : 0.36);
                    s.Snare(t0 + spb, 0.3); s.Snare(t0 + 3 * spb, 0.32);
                    if (b % 2 == 1) { s.Snare(t0 + 3.5 * spb, 0.16); s.Snare(t0 + 3.75 * spb, 0.2); }
                    for (int i = 0; i < 16; i++) s.Hat(t0 + i * spb / 4, i % 4 == 0 ? 0.07 : (i % 2 == 0 ? 0.045 : 0.03), 0.3, i == 14 && b % 2 == 0);
                    if (chorus || finale) s.Crash(t0 + 2 * spb, 0.12);
                }
                bool intoPart = b + 1 == B1 || b + 1 == C1 || b + 1 == A2 || b + 1 == B2;
                if (intoPart)
                {
                    for (int i = 0; i < 8; i++) s.Snare(t0 + (2 + i * 0.25) * spb, 0.16 + 0.03 * i, 0.1);
                    s.Tom(t0 + 3 * spb, 50, 0.3, -0.4); s.Tom(t0 + 3.25 * spb, 47, 0.3, -0.1); s.Tom(t0 + 3.5 * spb, 45, 0.32, 0.2); s.Tom(t0 + 3.75 * spb, 40, 0.34, 0.4);
                }
                else if ((b - 4) % 4 == 3)
                {
                    s.Tom(t0 + 3 * spb, 48, 0.25, -0.3); s.Tom(t0 + 3.5 * spb, 45, 0.25, 0.3); s.Snare(t0 + 3.75 * spb, 0.25);
                }
            }
        }

        // the opening: a drone and a brass call that climbs the chord
        s.Crash(0, 0.24); s.Bass(0, 2 * bar, 32, 0.34, true);
        foreach (var (beat, midi, len) in new[] { (0.0, 68, 1.0), (1.0, 71, 1.0), (2.0, 75, 1.0), (3.0, 80, 1.0) })
        {
            s.Trumpet(2 * bar + beat * spb, len * spb * 0.9, midi - 12, 0.15, -0.2, 0.9);
            s.Trumpet(2 * bar + beat * spb, len * spb * 0.9, midi, 0.1, 0.25, 1.1);
        }
        foreach (var (beat, midi, len) in new[] { (0.0, 76, 1.0), (1.0, 80, 1.0), (2.0, 83, 2.0) })
        {
            s.Trumpet(2 * bar + (4 + beat) * spb, len * spb * 0.9, midi - 12, 0.22, 0.1);
            s.Trumpet(2 * bar + (4 + beat) * spb, len * spb * 0.9, midi - 24, 0.14, -0.2, 0.8);
        }

        Action<double, double, int, Chord, int> lead = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, m - 12, 0.2, 0.15);
        Action<double, double, int, Chord, int> harm = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, Below(m - 12, c), 0.14, -0.4, 0.95);
        Action<double, double, int, Chord, int> high = (t, d, m, c, b) => s.Trumpet(t, d * 0.85, m, 0.12, 0.35, 1.2);
        Action<double, double, int, Chord, int> deep = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, m - 24, 0.12, -0.1, 0.8);

        PlayTune(lead, RumiaA, RumiaChordsA, A1 * bar, bar, spb);
        PlayTune(harm, RumiaA, RumiaChordsA, A1 * bar, bar, spb);
        PlayTune(deep, RumiaA, RumiaChordsA, A1 * bar, bar, spb);
        foreach (int start in new[] { B1, B2 })
        {
            PlayTune(lead, RumiaB, RumiaChordsB, start * bar, bar, spb);
            PlayTune(harm, RumiaB, RumiaChordsB, start * bar, bar, spb);
            PlayTune(high, RumiaB, RumiaChordsB, start * bar, bar, spb);
            PlayTune(deep, RumiaB, RumiaChordsB, start * bar, bar, spb);
        }
        PlayTune(lead, RumiaA, RumiaChordsA, A2 * bar, bar, spb);
        PlayTune(harm, RumiaA, RumiaChordsA, A2 * bar, bar, spb);
        PlayTune(high, RumiaA, RumiaChordsA, A2 * bar, bar, spb);
        PlayTune(deep, RumiaA, RumiaChordsA, A2 * bar, bar, spb);

        // brass stabs on the offbeats in the verses and the finale
        for (int b = 0; b < bars; b++)
        {
            bool stabs = (b >= A1 && b < B1) || (b >= A2 && b < B2);
            if (!stabs) continue;
            var ch = chords[b];
            foreach (double beat in new[] { 1.5, 3.5 })
                for (int i = 1; i < 4; i++) s.Trumpet(b * bar + beat * spb, spb * 0.3, ch.Tones[i], 0.05, -0.4 + i * 0.3, 1.3);
        }

        // the serpent: a chromatic line in sixteenths over stabs
        int[] slither = { 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 2, 1 };
        for (int b = 0; b < 8; b++)
        {
            var ch = RumiaChordsC[b];
            double t0 = (C1 + b) * bar;
            int root = ch.Bass + 24;
            for (int i = 0; i < 16; i++)
                s.Trumpet(t0 + i * spb / 4, spb * 0.2, root + slither[i], 0.085, i % 2 == 0 ? -0.4 : 0.4, 0.6);
            s.Trumpet(t0 + 1.5 * spb, spb * 0.4, ch.Tones[2], 0.2, 0.1, 1.3);
            s.Trumpet(t0 + 3.5 * spb, spb * 0.4, ch.Tones[2], 0.2, 0.1, 1.3);
            s.Trumpet(t0, spb * 0.5, ch.Tones[1] - 12, 0.18, -0.1, 1.0);
        }

        s.Reverb(0.18, 0.76);
        return s.FinishLoop(bars * bar, 0.25);
    }

    // MARK: The Great Serpent

    /// <summary>"Serpent Ablaze": the boss's second theme, the Inferno Serpent grown heavy. The same tune and the same key (G# minor), but
    /// faster (176 BPM), with a galloping bass in sixteenths over a sub-bass, a double-kick beat with a snare on every backbeat and a crash
    /// on every bar, brass stacked in fifths and octaves, the chromatic serpent running under the second half, and a last
    /// chorus that goes on a little longer.</summary>
    public static (float[] left, float[] right) SerpentAblaze()
    {
        const double bpm = 176;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(RumiaChordsIntro); chords.AddRange(RumiaChordsA); chords.AddRange(RumiaChordsB); chords.AddRange(RumiaChordsC);
        chords.AddRange(RumiaChordsA); chords.AddRange(RumiaChordsB); chords.AddRange(RumiaChordsB);
        int bars = chords.Count; // 52
        var s = new Synth(bars * bar + 5);
        int A1 = 4, B1 = 12, C1 = 20, A2 = 28, B2 = 36, B3 = 44;
        // which sixteenths of the bar the bass accents: 3 + 3 + 2 eighths
        bool[] accent = new bool[16];
        foreach (int i in new[] { 0, 6, 12 }) accent[i] = true;

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < 4, serpent = b >= C1 && b < A2, chorus = (b >= B1 && b < C1) || b >= B2, finale = b >= A2;
            bool last = b >= B3;

            // a wall of strings and brass, low and wide
            for (int i = 0; i < ch.Tones.Length; i++)
            {
                s.Pad(t0, bar * 1.03, ch.Tones[i], intro ? 0.04 : 0.07, -0.6 + i * 0.3, 2.2);
                if (!intro) s.Trumpet(t0, bar * 0.95, ch.Tones[i] - 12, chorus ? 0.06 : 0.04, -0.3 + i * 0.2, 0.7);
            }
            if (!intro) s.Pad(t0, bar * 1.03, ch.Bass + 12, 0.07, 0, 3.0);

            // bass: a gallop in sixteenths (heavier on the accents), an octave below it, and a long sub-bass note
            if (b >= 2)
            {
                s.Bass(t0, bar * 0.98, ch.Bass - 12, 0.32, true);
                if (!serpent)
                    for (int i = 0; i < 16; i++)
                        s.Bass(t0 + i * spb / 4, spb * 0.2, ch.Bass + (i == 15 && b % 2 == 1 ? 12 : 0), accent[i] ? 0.34 : 0.22, true);
                else
                    for (int i = 0; i < 8; i++)
                        s.Bass(t0 + i * spb / 2, spb * 0.4, ch.Bass + (i == 7 ? 7 : 0), i % 2 == 0 ? 0.34 : 0.26, true);
            }

            // sparkling arpeggio over it
            if (b >= 4 && !serpent)
                for (int i = 0; i < 16; i++)
                {
                    int[] order = { 0, 2, 1, 3, 1, 2, 3, 2 };
                    s.Pluck(t0 + i * spb / 4, ch.Tones[order[i % 8]] + 12, (i % 4 == 0 ? 0.07 : 0.045) * (chorus || finale ? 1.2 : 1), -0.4 + (i % 4) * 0.27, 0.3);
                }

            // drums
            if (b == 2 || b == 3)
            {
                for (int i = 0; i < 32; i++) s.Snare(t0 + i * spb / 8, 0.08 + 0.2 * ((b - 2) * 32 + i) / 64.0, 0.1);
                s.Kick(t0, 0.5); s.Kick(t0 + 2 * spb, 0.5);
                if (b == 3) { s.Tom(t0 + 3 * spb, 52, 0.34, -0.4); s.Tom(t0 + 3.25 * spb, 48, 0.34, -0.1); s.Tom(t0 + 3.5 * spb, 45, 0.36, 0.2); s.Tom(t0 + 3.75 * spb, 40, 0.4, 0.4); }
            }
            else if (b < 2)
            {
                s.Kick(t0, 0.5); s.Kick(t0 + 2 * spb, 0.46); s.Tom(t0 + 3 * spb, 40, 0.34, -0.2); s.Tom(t0 + 3.5 * spb, 38, 0.34, 0.2);
                s.Crash(t0, 0.2);
            }
            else
            {
                bool section = b == A1 || b == B1 || b == C1 || b == A2 || b == B2 || b == B3;
                s.Crash(t0, section ? 0.3 : 0.18);
                if (serpent)
                {
                    // half-time and menacing: kicks and a snare, toms, and the gallop of the double kick
                    s.Kick(t0, 0.55); s.Kick(t0 + 0.75 * spb, 0.4); s.Kick(t0 + 1.5 * spb, 0.5); s.Snare(t0 + 2 * spb, 0.38);
                    s.Kick(t0 + 2.75 * spb, 0.4); s.Kick(t0 + 3.5 * spb, 0.46);
                    for (int i = 0; i < 8; i++) s.Hat(t0 + i * spb / 2, 0.06, 0.3, i == 7);
                    s.Tom(t0 + 3 * spb, 47, 0.26, -0.3); s.Tom(t0 + 3.25 * spb, 45, 0.26, 0); s.Tom(t0 + 3.75 * spb, 43, 0.26, 0.3);
                }
                else
                {
                    // double kick in sixteenths, snare on 2 and 4 with a ghost note, hats in sixteenths
                    for (int i = 0; i < 16; i++)
                        if (i % 4 == 0 || i % 4 == 1 || (last && i % 2 == 1)) s.Kick(t0 + i * spb / 4, i % 4 == 0 ? 0.52 : 0.38);
                    s.Snare(t0 + spb, 0.4); s.Snare(t0 + 3 * spb, 0.42);
                    if (b % 2 == 1) { s.Snare(t0 + 3.5 * spb, 0.2); s.Snare(t0 + 3.75 * spb, 0.26); }
                    for (int i = 0; i < 16; i++) s.Hat(t0 + i * spb / 4, i % 4 == 0 ? 0.08 : (i % 2 == 0 ? 0.05 : 0.035), 0.3, i == 14 && b % 2 == 0);
                    if (chorus || finale) s.Crash(t0 + 2 * spb, 0.16);
                }
                bool intoPart = b + 1 == B1 || b + 1 == C1 || b + 1 == A2 || b + 1 == B2 || b + 1 == B3;
                if (intoPart)
                {
                    for (int i = 0; i < 12; i++) s.Snare(t0 + (2 + i / 6.0) * spb, 0.18 + 0.025 * i, 0.1);
                    s.Tom(t0 + 3 * spb, 52, 0.36, -0.4); s.Tom(t0 + 3.25 * spb, 48, 0.36, -0.1); s.Tom(t0 + 3.5 * spb, 45, 0.38, 0.2); s.Tom(t0 + 3.75 * spb, 40, 0.42, 0.4);
                }
                else if ((b - 4) % 4 == 3)
                {
                    s.Tom(t0 + 3 * spb, 50, 0.3, -0.3); s.Tom(t0 + 3.25 * spb, 47, 0.3, 0); s.Tom(t0 + 3.5 * spb, 45, 0.3, 0.3); s.Snare(t0 + 3.75 * spb, 0.3);
                }
            }
        }

        // the opening: a low drone, a siren of brass that climbs the chord, and the first hammer blows
        s.Crash(0, 0.3); s.Bass(0, 2 * bar, 32, 0.4, true); s.Bass(0, 2 * bar, 44, 0.2, true);
        int[] siren = { 56, 59, 63, 68, 71, 75, 80, 83 };
        for (int k = 0; k < 8; k++)
        {
            double t = 2 * bar + k * spb;
            s.Trumpet(t, spb * 0.95, siren[k] - 12, 0.16, -0.2, 1.1);
            s.Trumpet(t, spb * 0.95, siren[k] - 24, 0.14, 0.2, 0.8);
            s.Trumpet(t, spb * 0.95, siren[k] - 5, 0.08, 0.0, 1.2);
        }

        Action<double, double, int, Chord, int> lead = (t, d, m, c, b) => s.Trumpet(t, d * 0.82, m - 12, 0.21, 0.15, 1.15);
        Action<double, double, int, Chord, int> harm = (t, d, m, c, b) => s.Trumpet(t, d * 0.82, Below(m - 12, c), 0.15, -0.4, 1.0);
        Action<double, double, int, Chord, int> fifth = (t, d, m, c, b) => s.Trumpet(t, d * 0.82, m - 17, 0.1, -0.2, 1.2);
        Action<double, double, int, Chord, int> high = (t, d, m, c, b) => s.Trumpet(t, d * 0.85, m, 0.13, 0.35, 1.3);
        Action<double, double, int, Chord, int> deep = (t, d, m, c, b) => s.Trumpet(t, d * 0.82, m - 24, 0.16, -0.1, 0.9);
        Action<double, double, int, Chord, int> abyss = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, m - 36, 0.14, 0.0, 0.7);

        void Tune(Note[][] tune, Chord[] chs, int startBar, params Action<double, double, int, Chord, int>[] voices)
        {
            foreach (var v in voices) PlayTune(v, tune, chs, startBar * bar, bar, spb);
        }
        Tune(RumiaA, RumiaChordsA, A1, lead, harm, deep);
        Tune(RumiaB, RumiaChordsB, B1, lead, harm, fifth, high, deep);
        Tune(RumiaA, RumiaChordsA, A2, lead, harm, fifth, high, deep, abyss);
        Tune(RumiaB, RumiaChordsB, B2, lead, harm, fifth, high, deep, abyss);
        Tune(RumiaB, RumiaChordsB, B3, lead, harm, fifth, high, deep, abyss);

        // brass stabs on the offbeats in the verses and the finale
        for (int b = 0; b < bars; b++)
        {
            bool stabs = (b >= A1 && b < B1) || (b >= A2 && b < B2);
            if (!stabs) continue;
            var ch = chords[b];
            foreach (double beat in new[] { 1.5, 3.5 })
                for (int i = 1; i < 4; i++) s.Trumpet(b * bar + beat * spb, spb * 0.3, ch.Tones[i], 0.06, -0.4 + i * 0.3, 1.4);
        }

        // the serpent: a chromatic line in sixteenths, full and menacing in its own part and now and then under the rest
        int[] slither = { 0, 1, 2, 3, 4, 5, 4, 3, 2, 1, 0, 1, 2, 3, 2, 1 };
        for (int b = C1; b < bars; b++)
        {
            bool full = b < A2;
            if (!full && b % 2 == 1) continue;
            var ch = chords[b];
            double t0 = b * bar;
            int root = ch.Bass + 24;
            for (int i = 0; i < 16; i++)
                s.Trumpet(t0 + i * spb / 4, spb * 0.2, root + slither[i], full ? 0.09 : 0.05, i % 2 == 0 ? -0.4 : 0.4, 0.6);
            if (full)
            {
                s.Trumpet(t0 + 1.5 * spb, spb * 0.4, ch.Tones[2], 0.22, 0.1, 1.3);
                s.Trumpet(t0 + 3.5 * spb, spb * 0.4, ch.Tones[2], 0.22, 0.1, 1.3);
                s.Trumpet(t0, spb * 0.5, ch.Tones[1] - 12, 0.2, -0.1, 1.0);
            }
        }

        s.Reverb(0.16, 0.74);
        return s.FinishLoop(bars * bar, 0.27);
    }
}
