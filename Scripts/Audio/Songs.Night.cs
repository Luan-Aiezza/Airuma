using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The two themes of stage 2, the river of the parties (brass, saxophones and a drum kit, with a bass and a little percussion; no strings, no harp):
///
/// "Uma noite animada em meio à escuridão" (the stage, 168 BPM, G minor with a bright chorus in B flat major): a swinging street party at night. The alto sax sings a syncopated tune over a walking bass
/// and a swing beat, trumpets chop the offbeats, a cowbell and a shaker keep the carnival going, and in the middle a saxophone solo runs over the changes.
/// "O dançarino das estrelas" (the boss, 178 BPM, E minor): the same night grown fierce, a dancer who never stops. A unison riff of trumpets and saxophones on the Andalusian cadence (E minor, D, C, B7),
/// double-time drums, a soaring chorus of trumpets, a breakdown for a wild sax solo with star-bright sparks over it, and a Latin section of hammered brass stabs and congas.
/// </summary>
public static partial class Songs
{
    private static readonly Chord Cm = new(36, 55, 60, 63, 67), Eb = new(39, 55, 58, 63, 67), D7 = new(38, 54, 57, 60, 62);

    /// <summary>Beats off the beat are pushed late a little, for a swing.</summary>
    private static double Sw(double beat, double swing)
    {
        double f = beat - Math.Floor(beat);
        return Math.Abs(f - 0.5) < 1e-6 ? beat + swing : beat;
    }

    private static void PlayTuneSwing(Action<double, double, int, Chord, int> play, Note[][] tune, Chord[] chords, double barStart, double bar, double spb, double swing)
    {
        for (int b = 0; b < tune.Length; b++)
            foreach (var n in tune[b])
                play(barStart + b * bar + Sw(n.Beat, swing) * spb, n.Len * spb, n.Midi, chords[b], b);
    }

    /// <summary>A chord hit by the brass: three trumpets above and a sax underneath.</summary>
    private static void Stab(Synth s, double t, double dur, Chord ch, double amp, double pan = 0)
    {
        s.Trumpet(t, dur, ch.Tones[1] + 12, amp, pan - 0.2, 1.1);
        s.Trumpet(t, dur, ch.Tones[2] + 12, amp, pan, 1.1);
        s.Trumpet(t, dur, ch.Tones[3] + 12, amp * 0.9, pan + 0.2, 1.2);
        s.Sax(t, dur, ch.Tones[2], amp * 0.9, pan - 0.35, 0.3);
    }

    // MARK: The stage

    private static readonly Note[][] NightA =
    {
        new[] { new Note(0, 67, 0.5), new Note(0.5, 70, 0.5), new Note(1, 74, 1), new Note(2.5, 72, 0.5), new Note(3, 70, 0.5), new Note(3.5, 67, 0.5) },
        new[] { new Note(0, 70, 1), new Note(1.5, 67, 0.5), new Note(2, 69, 0.5), new Note(2.5, 70, 0.5), new Note(3, 74, 1) },
        new[] { new Note(0, 72, 0.5), new Note(0.5, 75, 0.5), new Note(1, 79, 1), new Note(2.5, 77, 0.5), new Note(3, 75, 0.5), new Note(3.5, 72, 0.5) },
        new[] { new Note(0, 74, 1), new Note(1, 72, 0.5), new Note(1.5, 69, 0.5), new Note(2, 66, 1), new Note(3, 69, 0.5), new Note(3.5, 72, 0.5) },
        new[] { new Note(0, 67, 0.5), new Note(0.5, 70, 0.5), new Note(1, 74, 1), new Note(2.5, 77, 0.5), new Note(3, 79, 1) },
        new[] { new Note(0, 77, 0.5), new Note(0.5, 75, 0.5), new Note(1, 72, 1), new Note(2, 70, 0.5), new Note(2.5, 67, 0.5), new Note(3, 70, 1) },
        new[] { new Note(0, 69, 0.5), new Note(0.5, 72, 0.5), new Note(1, 74, 0.5), new Note(1.5, 78, 0.5), new Note(2, 81, 2) },
        new[] { new Note(0, 79, 1.5), new Note(1.5, 74, 0.5), new Note(2, 70, 1), new Note(3, 67, 0.5) },
    };
    private static readonly Note[][] NightB =
    {
        new[] { new Note(0, 77, 1), new Note(1, 74, 0.5), new Note(1.5, 77, 0.5), new Note(2, 82, 2) },
        new[] { new Note(0, 81, 1), new Note(1, 77, 0.5), new Note(1.5, 81, 0.5), new Note(2, 84, 2) },
        new[] { new Note(0, 79, 1), new Note(1, 75, 0.5), new Note(1.5, 79, 0.5), new Note(2, 84, 1), new Note(3, 82, 1) },
        new[] { new Note(0, 81, 1), new Note(1, 78, 1), new Note(2, 74, 2) },
        new[] { new Note(0, 82, 1), new Note(1, 79, 0.5), new Note(1.5, 82, 0.5), new Note(2, 87, 2) },
        new[] { new Note(0, 86, 1), new Note(1, 82, 0.5), new Note(1.5, 86, 0.5), new Note(2, 82, 2) },
        new[] { new Note(0, 84, 1), new Note(1, 81, 1), new Note(2, 78, 1), new Note(3, 81, 1) },
        new[] { new Note(0, 81, 0.5), new Note(0.5, 78, 0.5), new Note(1, 74, 1), new Note(2, 78, 0.5), new Note(2.5, 81, 0.5), new Note(3, 86, 1) },
    };

    private static Chord[] NightChordsIntro => new[] { Gm, Gm, D7, D7 };
    private static Chord[] NightChordsA => new[] { Gm, Gm, Cm, D7, Gm, Eb, D7, Gm };
    private static Chord[] NightChordsB => new[] { Bb, F, Cm, D7, Eb, Bb, Cm, D7 };
    private static Chord[] NightChordsSolo => new[] { Gm, Gm, Cm, Cm, D7, D7, Gm, D7 };
    private static Chord[] NightChordsOutro => new[] { Gm, Cm, D7, Gm };

    /// <summary>"Uma noite animada em meio à escuridão": the theme of stage 2.</summary>
    public static (float[] left, float[] right) AnimatedNight()
    {
        const double bpm = 168, swing = 0.11;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(NightChordsIntro); chords.AddRange(NightChordsA); chords.AddRange(NightChordsB); chords.AddRange(NightChordsA);
        chords.AddRange(NightChordsSolo); chords.AddRange(NightChordsB); chords.AddRange(NightChordsOutro);
        int bars = chords.Count; // 48
        var s = new Synth(bars * bar + 5);
        const int A1 = 4, B1 = 12, A2 = 20, S1 = 28, B2 = 36, O1 = 44;

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < A1, chorus = (b >= B1 && b < A2) || (b >= B2 && b < O1), solo = b >= S1 && b < B2, outro = b >= O1;
            bool lively = !intro && !outro;
            int next = chords[(b + 1) % bars].Bass;

            // the walking bass: root, fifth, octave, fifth, with a chromatic step into the next chord
            if (b >= 2)
            {
                int[] walk = { 0, 7, 12, 7 };
                for (int k = 0; k < 4; k++)
                {
                    int note = ch.Bass + walk[k];
                    if (k == 3 && next != ch.Bass) note = next - 1;
                    s.Bass(t0 + k * spb, spb * 0.88, note, k == 0 ? 0.33 : 0.27);
                }
            }

            // drums: a swing beat (kick on one and three, the snare on two and four, a ride in triplet feel); four on the floor in the chorus
            if (b == 1) { for (int i = 0; i < 8; i++) s.Snare(t0 + (2 + i * 0.25) * spb, 0.06 + 0.04 * i, 0.1); }
            else if (b >= 2)
            {
                if (b == A1 || b == B1 || b == A2 || b == S1 || b == B2 || b == O1) s.Crash(t0, 0.2);
                if (chorus) for (int k = 0; k < 4; k++) s.Kick(t0 + k * spb, k % 2 == 0 ? 0.42 : 0.3);
                else { s.Kick(t0, 0.4); s.Kick(t0 + 2 * spb, 0.36); if (b % 2 == 1) s.Kick(t0 + Sw(2.5, swing) * spb, 0.24); }
                s.Snare(t0 + spb, 0.22); s.Snare(t0 + 3 * spb, 0.24);
                if (b % 2 == 1) s.Snare(t0 + (3.67) * spb, 0.07);                          // a ghost note
                for (int k = 0; k < 4; k++)
                {
                    s.Ride(t0 + k * spb, k % 2 == 0 ? 0.07 : 0.05);
                    s.Ride(t0 + Sw(k + 0.5, swing) * spb, 0.04);
                }
                if (lively)
                    for (int i = 0; i < 8; i++) s.Shaker(t0 + Sw(i * 0.5, swing) * spb, i % 2 == 0 ? 0.045 : 0.028);
                if (chorus || solo) { s.Cowbell(t0, 0.07); s.Cowbell(t0 + Sw(1.5, swing) * spb, 0.055); s.Cowbell(t0 + 3 * spb, 0.06); }
                if (solo) { s.Conga(t0 + 0.5 * spb, 62, 0.12, -0.3); s.Conga(t0 + 1.5 * spb, 66, 0.1, 0.3); s.Conga(t0 + 2.5 * spb, 62, 0.1, -0.3); s.Conga(t0 + 3.5 * spb, 66, 0.12, 0.3); }
                bool fillBar = b + 1 == B1 || b + 1 == A2 || b + 1 == S1 || b + 1 == B2 || b + 1 == O1;
                if (fillBar)
                {
                    s.Tom(t0 + 2.5 * spb, 52, 0.26, -0.4); s.Tom(t0 + 3 * spb, 48, 0.28, -0.1); s.Tom(t0 + 3.33 * spb, 45, 0.3, 0.2); s.Tom(t0 + 3.67 * spb, 41, 0.32, 0.4);
                }
            }

            // the brass chops the offbeats (in the verses and the solo)
            if (!chorus && !intro && !outro && b != S1 + 7)
            {
                Stab(s, t0 + Sw(1.5, swing) * spb, spb * 0.22, ch, solo ? 0.045 : 0.06, -0.1);
                Stab(s, t0 + Sw(3.5, swing) * spb, spb * 0.22, ch, solo ? 0.045 : 0.06, 0.1);
            }
            else if (chorus)
            {
                Stab(s, t0, spb * 0.4, ch, 0.05); Stab(s, t0 + Sw(2.5, swing) * spb, spb * 0.3, ch, 0.05);
            }
        }

        // the introduction: a fanfare of trumpets and saxes, climbing the chord
        s.Crash(0, 0.24);
        foreach (var (beat, midi, len) in new[] { (0.0, 67, 0.5), (0.5, 70, 0.5), (1.0, 74, 0.5), (1.5, 79, 0.5), (2.0, 82, 1.0), (3.0, 79, 1.0) })
        {
            s.Trumpet(2 * bar + beat * spb, len * spb * 0.88, midi, 0.15, 0.1, 1.1);
            s.Sax(2 * bar + beat * spb, len * spb * 0.9, midi - 12, 0.15, -0.15, 0.2);
        }
        foreach (var (beat, midi, len) in new[] { (0.0, 81, 0.5), (0.5, 78, 0.5), (1.0, 74, 0.5), (1.5, 69, 0.5), (2.0, 74, 2.0) })
        {
            s.Trumpet(3 * bar + beat * spb, len * spb * 0.9, midi + 0, 0.15, 0.1, 1.1);
            s.Sax(3 * bar + beat * spb, len * spb * 0.9, midi - 12, 0.15, -0.15, 0.2);
        }

        // the tune, first by the alto sax alone (growling a little on the long notes), then with the trumpets
        Action<double, double, int, Chord, int> sax = (t, d, m, c, b) => s.Sax(t, d * 0.9, m, 0.22, -0.1, d > 0.9 * spb ? 0.5 : 0.15);
        Action<double, double, int, Chord, int> saxLow = (t, d, m, c, b) => s.Sax(t, d * 0.9, m - 12, 0.13, -0.3, 0.2);
        Action<double, double, int, Chord, int> trumpet = (t, d, m, c, b) => s.Trumpet(t, d * 0.88, m, 0.2, 0.2, 1.1);
        Action<double, double, int, Chord, int> trumpetHarm = (t, d, m, c, b) => s.Trumpet(t, d * 0.85, Below(m, c), 0.12, 0.45, 1.0);
        Action<double, double, int, Chord, int> trumpetHigh = (t, d, m, c, b) => s.Trumpet(t, d * 0.85, m + 12, 0.07, 0.35, 1.3);
        Action<double, double, int, Chord, int> saxHarm = (t, d, m, c, b) => s.Sax(t, d * 0.9, Below(m, c), 0.15, -0.4, 0.2);

        PlayTuneSwing(sax, NightA, NightChordsA, A1 * bar, bar, spb, swing);
        PlayTuneSwing(saxLow, NightA, NightChordsA, A1 * bar, bar, spb, swing);
        PlayTuneSwing(trumpet, NightB, NightChordsB, B1 * bar, bar, spb, swing);
        PlayTuneSwing(trumpetHarm, NightB, NightChordsB, B1 * bar, bar, spb, swing);
        PlayTuneSwing(saxHarm, NightB, NightChordsB, B1 * bar, bar, spb, swing);
        PlayTuneSwing(sax, NightA, NightChordsA, A2 * bar, bar, spb, swing);
        PlayTuneSwing(trumpet, NightA, NightChordsA, A2 * bar, bar, spb, swing);
        PlayTuneSwing(trumpetHarm, NightA, NightChordsA, A2 * bar, bar, spb, swing);
        PlayTuneSwing(saxLow, NightA, NightChordsA, A2 * bar, bar, spb, swing);
        PlayTuneSwing(trumpet, NightB, NightChordsB, B2 * bar, bar, spb, swing);
        PlayTuneSwing(trumpetHarm, NightB, NightChordsB, B2 * bar, bar, spb, swing);
        PlayTuneSwing(trumpetHigh, NightB, NightChordsB, B2 * bar, bar, spb, swing);
        PlayTuneSwing(sax, NightB, NightChordsB, B2 * bar, bar, spb, swing);
        PlayTuneSwing(saxHarm, NightB, NightChordsB, B2 * bar, bar, spb, swing);

        // the solo: a tenor sax runs up and down G harmonic minor, a different shape each bar, and ends on a held note
        int[] scale = { 0, 2, 3, 5, 7, 8, 11 };                                      // G A Bb C D Eb F#
        int Degree(int index) => 67 + scale[((index % 7) + 7) % 7] + 12 * (int)Math.Floor(index / 7.0);
        int[][] shapes =
        {
            new[] { 0, 1, 2, 4, 3, 2, 4, 5 }, new[] { 5, 4, 2, 3, 1, 2, 0, 1 }, new[] { 2, 4, 6, 5, 4, 6, 7, 6 }, new[] { 7, 6, 4, 5, 3, 4, 2, 3 },
            new[] { 4, 5, 6, 8, 7, 6, 5, 4 }, new[] { 8, 7, 5, 6, 4, 5, 3, 4 }, new[] { 6, 8, 9, 7, 8, 6, 7, 5 },
        };
        int[] starts = { 7, 7, 7, 7, 5, 6, 7 };
        for (int b = 0; b < 7; b++)
            for (int i = 0; i < 8; i++)
            {
                int idx = starts[b] + shapes[b][i] - 2;
                s.Sax(S1 * bar + b * bar + Sw(i * 0.5, swing) * spb, spb * 0.46, Degree(idx), 0.2 + 0.02 * (i % 4 == 0 ? 1 : 0), 0.1, b >= 4 ? 0.7 : 0.3, 1.2);
            }
        s.Sax((S1 + 7) * bar, 2 * spb, Degree(14), 0.22, 0.1, 0.8, 1.2);                    // the held note into the chorus
        s.Sax((S1 + 7) * bar + 2 * spb, 2 * spb, Degree(13), 0.2, 0.1, 0.8, 1.2);

        // the ending: the tune's tag, then back to the start of the loop
        foreach (var (bar0, beat, midi, len) in new[] { (0, 0.0, 74, 1.0), (0, 1.0, 70, 1.0), (0, 2.0, 67, 2.0), (1, 0.0, 75, 1.0), (1, 1.0, 72, 1.0), (1, 2.0, 67, 2.0), (2, 0.0, 78, 1.0), (2, 1.0, 74, 1.0), (2, 2.0, 69, 2.0), (3, 0.0, 74, 3.0) })
        {
            s.Trumpet((O1 + bar0) * bar + beat * spb, len * spb * 0.9, midi + 12, 0.14, 0.2, 1.1);
            s.Sax((O1 + bar0) * bar + beat * spb, len * spb * 0.92, midi, 0.16, -0.2, 0.3);
        }

        s.Reverb(0.16, 0.72);
        return s.FinishLoop(bars * bar, 0.17);
    }

    // MARK: The boss

    private static readonly Chord EmN = new(40, 55, 59, 64, 67), DN = new(38, 54, 57, 62, 66), CN = new(48, 55, 60, 64, 67), B7N = new(47, 54, 57, 63, 66);
    private static readonly Chord AmN = new(45, 57, 60, 64, 69), BmN = new(47, 54, 59, 62, 66);

    private static readonly Note[][] DancerA =
    {
        new[] { new Note(0, 76, 0.5), new Note(0.5, 76, 0.5), new Note(1, 79, 0.5), new Note(1.5, 78, 0.5), new Note(2, 76, 1), new Note(3, 71, 1) },
        new[] { new Note(0, 74, 0.5), new Note(0.5, 74, 0.5), new Note(1, 78, 0.5), new Note(1.5, 76, 0.5), new Note(2, 74, 1), new Note(3, 69, 1) },
        new[] { new Note(0, 72, 0.5), new Note(0.5, 72, 0.5), new Note(1, 76, 0.5), new Note(1.5, 74, 0.5), new Note(2, 72, 1), new Note(3, 67, 1) },
        new[] { new Note(0, 71, 0.5), new Note(0.5, 75, 0.5), new Note(1, 78, 0.5), new Note(1.5, 81, 0.5), new Note(2, 78, 1), new Note(3, 75, 1) },
        new[] { new Note(0, 76, 0.5), new Note(0.5, 76, 0.5), new Note(1, 79, 0.5), new Note(1.5, 78, 0.5), new Note(2, 76, 1), new Note(3, 83, 1) },
        new[] { new Note(0, 81, 0.5), new Note(0.5, 78, 0.5), new Note(1, 74, 0.5), new Note(1.5, 78, 0.5), new Note(2, 81, 1), new Note(3, 86, 1) },
        new[] { new Note(0, 84, 0.5), new Note(0.5, 79, 0.5), new Note(1, 76, 0.5), new Note(1.5, 79, 0.5), new Note(2, 84, 2) },
        new[] { new Note(0, 83, 1), new Note(1, 81, 0.5), new Note(1.5, 78, 0.5), new Note(2, 75, 1), new Note(3, 71, 1) },
    };
    private static readonly Note[][] DancerB =
    {
        new[] { new Note(0, 79, 2), new Note(2, 84, 1), new Note(3, 83, 1) },
        new[] { new Note(0, 81, 2), new Note(2, 86, 1), new Note(3, 81, 1) },
        new[] { new Note(0, 83, 1), new Note(1, 79, 1), new Note(2, 76, 2) },
        new[] { new Note(0, 78, 1), new Note(1, 74, 1), new Note(2, 78, 1), new Note(3, 83, 1) },
        new[] { new Note(0, 84, 2), new Note(2, 79, 1), new Note(3, 76, 1) },
        new[] { new Note(0, 81, 1), new Note(1, 86, 1), new Note(2, 88, 1), new Note(3, 86, 1) },
        new[] { new Note(0, 87, 1), new Note(1, 83, 1), new Note(2, 78, 1), new Note(3, 81, 1) },
        new[] { new Note(0, 83, 2), new Note(2, 75, 1), new Note(3, 78, 1) },
    };
    private static readonly Note[][] DancerDance =
    {
        new[] { new Note(0, 81, 0.5), new Note(1.5, 84, 0.5), new Note(3, 81, 0.5), new Note(3.5, 79, 0.5) },
        new[] { new Note(0, 83, 0.5), new Note(1.5, 87, 0.5), new Note(3, 83, 0.5), new Note(3.5, 81, 0.5) },
        new[] { new Note(0, 79, 0.5), new Note(1.5, 83, 0.5), new Note(3, 79, 0.5), new Note(3.5, 76, 0.5) },
        new[] { new Note(0, 76, 0.5), new Note(0.5, 79, 0.5), new Note(1, 83, 0.5), new Note(1.5, 88, 1), new Note(3, 86, 0.5), new Note(3.5, 83, 0.5) },
        new[] { new Note(0, 81, 0.5), new Note(1.5, 84, 0.5), new Note(3, 88, 0.5), new Note(3.5, 84, 0.5) },
        new[] { new Note(0, 83, 0.5), new Note(1.5, 87, 0.5), new Note(3, 90, 0.5), new Note(3.5, 87, 0.5) },
        new[] { new Note(0, 88, 0.5), new Note(1.5, 83, 0.5), new Note(3, 79, 0.5), new Note(3.5, 83, 0.5) },
        new[] { new Note(0, 87, 1), new Note(1, 83, 1), new Note(2, 78, 1), new Note(3, 75, 1) },
    };

    private static Chord[] DancerChordsIntro => new[] { EmN, EmN, EmN, B7N };
    private static Chord[] DancerChordsA => new[] { EmN, DN, CN, B7N, EmN, DN, CN, B7N };
    private static Chord[] DancerChordsB => new[] { CN, DN, EmN, BmN, CN, DN, B7N, B7N };
    private static Chord[] DancerChordsBreak => new[] { EmN, EmN, EmN, EmN, CN, CN, B7N, B7N };
    private static Chord[] DancerChordsDance => new[] { AmN, B7N, EmN, EmN, AmN, B7N, EmN, B7N };

    /// <summary>"O dançarino das estrelas": the boss's theme of stage 2.</summary>
    public static (float[] left, float[] right) StarDancer()
    {
        const double bpm = 178;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(DancerChordsIntro); chords.AddRange(DancerChordsA); chords.AddRange(DancerChordsB); chords.AddRange(DancerChordsBreak); chords.AddRange(DancerChordsDance);
        chords.AddRange(DancerChordsA); chords.AddRange(DancerChordsB);
        int bars = chords.Count; // 52
        var s = new Synth(bars * bar + 5);
        const int A1 = 4, B1 = 12, K1 = 20, D1 = 28, A2 = 36, B2 = 44;
        int[] tresillo = { 0, 1, 3, 4, 6, 7 };

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < A1, chorus = (b >= B1 && b < K1) || b >= B2, brk = b >= K1 && b < D1, dance = b >= D1 && b < A2, verse = (b >= A1 && b < B1) || (b >= A2 && b < B2);

            // the bass: a driving eighth pulse in the 3 + 3 + 2 pattern, an octave leaping on the last hit; held in the breakdown
            if (b >= 2)
            {
                if (brk) { s.Bass(t0, 1.9 * spb, ch.Bass, 0.34, true); s.Bass(t0 + 2 * spb, 1.9 * spb, ch.Bass + (b % 2 == 0 ? 7 : 0), 0.3, true); }
                else
                    for (int k = 0; k < tresillo.Length; k++)
                    {
                        int i = tresillo[k];
                        s.Bass(t0 + i * spb / 2, spb * 0.42, ch.Bass + (k == 5 ? 12 : 0), i % 2 == 0 ? 0.32 : 0.24, true);
                    }
            }

            // the drums, hard: double-time hats, a snare on the weak beats, and kicks that push the tresillo
            if (b == 2 || b == 3)
            {
                for (int i = 0; i < 24; i++) s.Snare(t0 + i * spb / 6, 0.06 + 0.16 * ((b - 2) * 24 + i) / 48.0, 0.1);
                if (b == 3) { s.Tom(t0 + 3 * spb, 50, 0.3, -0.4); s.Tom(t0 + 3.25 * spb, 47, 0.3, -0.1); s.Tom(t0 + 3.5 * spb, 45, 0.32, 0.2); s.Tom(t0 + 3.75 * spb, 40, 0.34, 0.4); }
            }
            else if (b < 2) { s.Kick(t0, 0.4); s.Kick(t0 + 2 * spb, 0.36); for (int i = 0; i < 8; i++) s.Tom(t0 + i * spb / 2, i % 2 == 0 ? 40 : 43, 0.1 + 0.015 * i, -0.3 + i * 0.08); }
            else
            {
                bool section = b == A1 || b == B1 || b == K1 || b == D1 || b == A2 || b == B2;
                if (section) s.Crash(t0, 0.24);
                if (brk)
                {
                    s.Kick(t0, 0.46); s.Kick(t0 + 1.5 * spb, 0.36); s.Snare(t0 + 2 * spb, 0.3);
                    for (int i = 0; i < 8; i++) s.Hat(t0 + i * spb / 2, 0.05, 0.3);
                    s.Tom(t0 + 3 * spb, 45, 0.2, -0.3); s.Tom(t0 + 3.5 * spb, 43, 0.2, 0.3);
                }
                else if (dance)
                {
                    // a Latin beat: kicks on the tresillo, rim clicks on the clave, congas, a cowbell
                    foreach (var k in new[] { 0.0, 1.5, 3.0 }) s.Kick(t0 + k * spb, 0.44);
                    s.Snare(t0 + spb, 0.16); s.Snare(t0 + 3 * spb, 0.2);
                    foreach (var k in new[] { 0.0, 0.75, 1.5, 2.5, 3.0 }) s.Clave(t0 + k * spb, 0.07);
                    for (int i = 0; i < 8; i++) s.Conga(t0 + (i * 0.5 + 0.25) * spb, i % 2 == 0 ? 66 : 60, 0.09, i % 2 == 0 ? 0.3 : -0.3);
                    s.Cowbell(t0, 0.07); s.Cowbell(t0 + 1.5 * spb, 0.06); s.Cowbell(t0 + 3 * spb, 0.07);
                    for (int i = 0; i < 16; i++) s.Shaker(t0 + i * spb / 4, i % 4 == 0 ? 0.05 : 0.03);
                }
                else
                {
                    double[] kicks = { 0, 0.75, 1.5, 2, 2.75, 3.5 };
                    foreach (var k in kicks) s.Kick(t0 + k * spb, k % 1 == 0 ? 0.48 : 0.38);
                    s.Snare(t0 + spb, 0.32); s.Snare(t0 + 3 * spb, 0.34);
                    if (b % 2 == 1) { s.Snare(t0 + 3.5 * spb, 0.16); s.Snare(t0 + 3.75 * spb, 0.2); }
                    for (int i = 0; i < 16; i++) s.Hat(t0 + i * spb / 4, i % 4 == 0 ? 0.08 : (i % 2 == 0 ? 0.05 : 0.032), 0.3, i == 14 && b % 2 == 0);
                    if (chorus) s.Crash(t0 + 2 * spb, 0.13);
                }
                bool intoPart = b + 1 == B1 || b + 1 == K1 || b + 1 == D1 || b + 1 == A2 || b + 1 == B2;
                if (intoPart)
                {
                    for (int i = 0; i < 8; i++) s.Snare(t0 + (2 + i * 0.25) * spb, 0.16 + 0.03 * i, 0.1);
                    s.Tom(t0 + 3 * spb, 50, 0.3, -0.4); s.Tom(t0 + 3.25 * spb, 47, 0.3, -0.1); s.Tom(t0 + 3.5 * spb, 45, 0.32, 0.2); s.Tom(t0 + 3.75 * spb, 40, 0.34, 0.4);
                }
            }

            // long saxes underneath the chorus for body; sparks of stars, far up, in the chorus and the breakdown
            if (chorus || brk) for (int i = 0; i < 3; i++) s.Sax(t0, bar * 0.97, ch.Tones[i] - 12, 0.06, -0.4 + i * 0.4, 0.1, 0.8);
            if (chorus || brk)
                for (int i = 0; i < 16; i++) s.Pluck(t0 + i * spb / 4, ch.Tones[(i * 3) % 4] + 36, 0.032 + (brk ? 0.012 : 0), 0.7 - (i % 4) * 0.45, 0.4);
            // brass hits on the tresillo behind the verses
            if (verse) foreach (var k in new[] { 0.0, 1.5, 3.0 }) Stab(s, t0 + k * spb, spb * 0.3, ch, 0.05, k == 1.5 ? 0.15 : -0.1);
        }

        // the opening: a drum call, then the first notes of the riff climbing the chord, trumpets over saxes
        s.Crash(0, 0.26);
        foreach (var (beat, midi, len) in new[] { (0.0, 64, 1.0), (1.0, 67, 1.0), (2.0, 71, 1.0), (3.0, 76, 1.0) })
        {
            s.Trumpet(2 * bar + beat * spb, len * spb * 0.9, midi, 0.14, -0.2, 0.9);
            s.Sax(2 * bar + beat * spb, len * spb * 0.9, midi - 12, 0.14, 0.2, 0.3);
        }
        foreach (var (beat, midi, len) in new[] { (0.0, 78, 0.5), (0.5, 79, 0.5), (1.0, 83, 0.5), (1.5, 86, 0.5), (2.0, 88, 2.0) })
        {
            s.Trumpet(3 * bar + beat * spb, len * spb * 0.9, midi, 0.18, 0.1, 1.2);
            s.Sax(3 * bar + beat * spb, len * spb * 0.9, midi - 12, 0.16, -0.1, 0.5);
        }

        // the riff: trumpets and saxes in unison (the saxes an octave down, growling), the trumpets doubled on the repeat
        Action<double, double, int, Chord, int> trumpet = (t, d, m, c, b) => s.Trumpet(t, d * 0.82, m, 0.2, 0.2, 1.2);
        Action<double, double, int, Chord, int> sax = (t, d, m, c, b) => s.Sax(t, d * 0.86, m - 12, 0.2, -0.2, 0.6);
        Action<double, double, int, Chord, int> saxUp = (t, d, m, c, b) => s.Sax(t, d * 0.86, m, 0.14, -0.35, 0.5);
        Action<double, double, int, Chord, int> harm = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, Below(m, c), 0.12, 0.4, 1.0);
        Action<double, double, int, Chord, int> high = (t, d, m, c, b) => s.Trumpet(t, d * 0.85, m + 12, 0.08, 0.35, 1.4);
        PlayTune(trumpet, DancerA, DancerChordsA, A1 * bar, bar, spb);
        PlayTune(sax, DancerA, DancerChordsA, A1 * bar, bar, spb);
        PlayTune(trumpet, DancerA, DancerChordsA, A2 * bar, bar, spb);
        PlayTune(sax, DancerA, DancerChordsA, A2 * bar, bar, spb);
        PlayTune(saxUp, DancerA, DancerChordsA, A2 * bar, bar, spb);
        PlayTune(harm, DancerA, DancerChordsA, A2 * bar, bar, spb);

        // the chorus: the trumpets soar, the saxes sing underneath, a high octave in the second chorus
        foreach (int start in new[] { B1, B2 })
        {
            PlayTune(trumpet, DancerB, DancerChordsB, start * bar, bar, spb);
            PlayTune(harm, DancerB, DancerChordsB, start * bar, bar, spb);
            PlayTune(sax, DancerB, DancerChordsB, start * bar, bar, spb);
            if (start == B2) PlayTune(high, DancerB, DancerChordsB, start * bar, bar, spb);
        }

        // the breakdown: a tenor sax runs wild over the pedal in E harmonic minor, the shapes growing wilder, with the trumpets' hits every second bar
        int[] scale = { 0, 2, 3, 5, 7, 8, 11 };                                     // E F# G A B C D#
        int Degree(int index) => 64 + scale[((index % 7) + 7) % 7] + 12 * (int)Math.Floor(index / 7.0);
        int[][] shapes =
        {
            new[] { 0, 2, 4, 3, 5, 4, 2, 1 }, new[] { 5, 4, 6, 5, 7, 6, 4, 5 }, new[] { 7, 6, 8, 7, 9, 8, 6, 7 }, new[] { 9, 8, 7, 6, 5, 4, 3, 2 },
            new[] { 4, 5, 7, 6, 8, 7, 9, 8 }, new[] { 9, 10, 8, 9, 7, 8, 6, 7 }, new[] { 6, 8, 10, 9, 11, 10, 8, 9 }, new[] { 11, 9, 10, 8, 9, 7, 8, 6 },
        };
        for (int b = 0; b < 8; b++)
            for (int i = 0; i < 8; i++)
                s.Sax((K1 + b) * bar + i * spb / 2, spb * 0.46, Degree(shapes[b][i] + 2), 0.2 + (i % 4 == 0 ? 0.02 : 0), 0.1, 0.5 + b * 0.07, 1.25);
        for (int b = 1; b < 8; b += 2) Stab(s, (K1 + b) * bar + 2 * spb, spb * 0.5, chords[K1 + b], 0.08);

        // the dance: brass hits on the tresillo, trumpets and saxes together, the run answering on the last beat
        Action<double, double, int, Chord, int> danceT = (t, d, m, c, b) => s.Trumpet(t, Math.Min(d, spb * 0.42) * 0.9, m, 0.2, 0.2, 1.3);
        Action<double, double, int, Chord, int> danceS = (t, d, m, c, b) => s.Sax(t, Math.Min(d, spb * 0.42) * 0.9, m - 12, 0.2, -0.2, 0.7);
        Action<double, double, int, Chord, int> danceH = (t, d, m, c, b) => s.Trumpet(t, Math.Min(d, spb * 0.42) * 0.9, Below(m, c), 0.12, 0.4, 1.2);
        PlayTune(danceT, DancerDance, DancerChordsDance, D1 * bar, bar, spb);
        PlayTune(danceS, DancerDance, DancerChordsDance, D1 * bar, bar, spb);
        PlayTune(danceH, DancerDance, DancerChordsDance, D1 * bar, bar, spb);
        for (int b = 0; b < 8; b++)
            foreach (var k in new[] { 0.0, 1.5, 3.0 }) Stab(s, (D1 + b) * bar + k * spb, spb * 0.28, chords[D1 + b], 0.06, k == 1.5 ? 0.15 : -0.1);

        s.Reverb(0.18, 0.74);
        return s.FinishLoop(bars * bar, 0.18);
    }
}
