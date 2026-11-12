using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The two themes of stage 3, the lake of the hail and the storm: trumpets and pianos (with the rumble of the toms as thunder, and nothing else).
///
/// "A tempestade sob o rio agitado" (the stage, 114 BPM, F minor): dark and sombre. The water churns under a piano that rolls in triplets while the left hand tolls low octaves; a trumpet,
/// muted and lonely, sings a slow tune above; thunder rumbles in the toms, and now and then a lightning of high notes flashes. The chorus climbs, the trumpets in two voices.
/// "A gata arisca brinca na tempestade" (the boss, 200 BPM, F minor): the same storm, and a cat playing in it. A much lower, heavier piano hammers a prowling pulse and chords on the offbeats,
/// and the PIANO is the lead: its right hand sings the tune in octaves (in chords in the chorus) while the trumpets double and answer it, sharp and fierce; in the middle the cat plays (a staccato figure in the piano, answered by the trumpets).
/// </summary>
public static partial class Songs
{
    private static readonly Chord FmS = new(41, 53, 56, 60, 65), DbS = new(37, 56, 61, 65, 68), BbmS = new(46, 53, 58, 61, 65), C7S = new(48, 52, 55, 58, 60);
    private static readonly Chord EbS = new(39, 55, 58, 63, 67), AbS = new(44, 56, 60, 63, 68);

    // MARK: The stage

    private static readonly Note[][] StormA =
    {
        new[] { new Note(0, 72, 2), new Note(2, 68, 1), new Note(3, 72, 1) },
        new[] { new Note(0, 73, 2), new Note(2, 68, 2) },
        new[] { new Note(0, 75, 1.5), new Note(1.5, 72, 0.5), new Note(2, 70, 2) },
        new[] { new Note(0, 67, 2), new Note(2, 64, 1), new Note(3, 67, 1) },
        new[] { new Note(0, 72, 2), new Note(2, 77, 1), new Note(3, 75, 1) },
        new[] { new Note(0, 73, 1.5), new Note(1.5, 70, 0.5), new Note(2, 65, 2) },
        new[] { new Note(0, 67, 1), new Note(1, 70, 1), new Note(2, 72, 1), new Note(3, 67, 1) },
        new[] { new Note(0, 65, 4) },
    };
    private static readonly Note[][] StormB =
    {
        new[] { new Note(0, 77, 2), new Note(2, 73, 1), new Note(3, 77, 1) },
        new[] { new Note(0, 80, 2), new Note(2, 75, 2) },
        new[] { new Note(0, 79, 1.5), new Note(1.5, 75, 0.5), new Note(2, 82, 2) },
        new[] { new Note(0, 80, 1), new Note(1, 77, 1), new Note(2, 73, 2) },
        new[] { new Note(0, 77, 1), new Note(1, 80, 1), new Note(2, 85, 2) },
        new[] { new Note(0, 84, 2), new Note(2, 80, 1), new Note(3, 75, 1) },
        new[] { new Note(0, 82, 1.5), new Note(1.5, 79, 0.5), new Note(2, 76, 2) },
        new[] { new Note(0, 79, 2), new Note(2, 76, 1), new Note(3, 72, 1) },
    };
    private static readonly Note[][] StormBridge =
    {
        new[] { new Note(0, 70, 4) }, new[] { new Note(0, 67, 2), new Note(2, 70, 2) }, new[] { new Note(0, 72, 4) }, new[] { new Note(0, 67, 3) },
    };

    private static Chord[] StormChordsIntro => new[] { FmS, FmS, DbS, C7S };
    private static Chord[] StormChordsA => new[] { FmS, DbS, EbS, C7S, FmS, BbmS, C7S, FmS };
    private static Chord[] StormChordsB => new[] { DbS, AbS, EbS, BbmS, DbS, AbS, C7S, C7S };
    private static Chord[] StormChordsBridge => new[] { BbmS, C7S, FmS, C7S };

    /// <summary>"A tempestade sob o rio agitado": the theme of stage 3.</summary>
    public static (float[] left, float[] right) StormUnderTheRiver()
    {
        const double bpm = 114;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(StormChordsIntro); chords.AddRange(StormChordsA); chords.AddRange(StormChordsB); chords.AddRange(StormChordsA); chords.AddRange(StormChordsBridge);
        int bars = chords.Count; // 32
        var s = new Synth(bars * bar + 6);
        const int A1 = 4, B1 = 12, A2 = 20, BR = 28;
        int[] order = { 0, 1, 2, 3, 2, 1 };

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < A1, chorus = b >= B1 && b < A2, bridge = b >= BR;

            // the left hand tolls: a low octave held the whole bar, and a fifth that answers in the middle
            s.Piano(t0, bar * 0.98, ch.Bass - 12, intro ? 0.2 : 0.26, -0.3, 0.7);
            s.Piano(t0, bar * 0.98, ch.Bass, 0.16, -0.2, 0.6);
            if (!intro && !bridge) s.Piano(t0 + 2 * spb, 2 * spb * 0.95, ch.Bass + 7, 0.13, -0.15, 0.6);

            // the right hand: the water churning, triplets rolling up and down the chord (higher and fuller in the chorus)
            for (int i = 0; i < 12; i++)
            {
                int midi = ch.Tones[order[i % 6]] + (chorus ? 12 : (i % 6 == 3 ? 12 : 0));
                double amp = (i % 3 == 0 ? 0.085 : 0.055) * (intro ? 0.7 : (bridge ? 0.8 : (chorus ? 1.25 : 1.0)));
                s.Piano(t0 + i * spb / 3, spb * 0.5, midi, amp, 0.15 + (i % 6) * 0.06, 0.2);
            }

            // trumpets underneath, long and soft, like the wind
            if (!intro)
                for (int i = 1; i < ch.Tones.Length; i++) s.Trumpet(t0, bar * 0.97, ch.Tones[i] - 12, chorus ? 0.04 : 0.025, -0.3 + i * 0.2, 0.45);

            // thunder: the toms, low and uneven, every few bars; louder and closer in the chorus
            if (b >= 2 && b % 4 == 2) { s.Tom(t0 + 3 * spb, 31, chorus ? 0.3 : 0.2, -0.3); s.Tom(t0 + 3.5 * spb, 29, chorus ? 0.34 : 0.22, 0.2); }
            if (chorus && b % 2 == 0) s.Tom(t0, 33, 0.22, 0.1);
            if (b == B1 - 1) { for (int i = 0; i < 8; i++) s.Tom(t0 + (2 + i * 0.25) * spb, 34 - i / 3, 0.12 + 0.03 * i, -0.4 + i * 0.1); }
            if (b == bars - 1) { s.Tom(t0 + 2 * spb, 31, 0.3); s.Tom(t0 + 3 * spb, 29, 0.34); }

            // lightning: a flash of high notes, now and then
            if (b >= 6 && b % 4 == 1 && !bridge)
            {
                int[] flash = { 96, 93, 91, 89 };
                for (int i = 0; i < flash.Length; i++) s.Piano(t0 + 2.5 * spb + i * 0.06, 0.2, flash[i], 0.035, 0.6 - i * 0.3, 0);
            }
        }

        // the opening: a call of one trumpet over the rolling piano
        foreach (var (beat, midi, len) in new[] { (0.0, 65, 2.0), (2.0, 68, 2.0) })
            s.Trumpet(2 * bar + beat * spb, len * spb * 0.95, midi + 12, 0.13, 0.1, 0.7);
        s.Trumpet(3 * bar, 4 * spb, 67 + 12, 0.14, 0.1, 0.7);

        // the tune: a lone, muted trumpet; in the second verse a second trumpet in thirds under it; in the chorus both, with a high octave
        Action<double, double, int, Chord, int> lead = (t, d, m, c, b) => s.Trumpet(t, d * 0.95, m, 0.17, 0.1, 0.75);
        Action<double, double, int, Chord, int> harm = (t, d, m, c, b) => s.Trumpet(t, d * 0.95, Below(m, c), 0.1, -0.35, 0.7);
        Action<double, double, int, Chord, int> high = (t, d, m, c, b) => s.Trumpet(t, d * 0.9, m + 12, 0.055, 0.4, 1.0);
        Action<double, double, int, Chord, int> piano = (t, d, m, c, b) => s.Piano(t, d * 1.1, m + 12, 0.06, 0.3, 0.1);
        PlayTune(lead, StormA, StormChordsA, A1 * bar, bar, spb);
        PlayTune(lead, StormB, StormChordsB, B1 * bar, bar, spb);
        PlayTune(harm, StormB, StormChordsB, B1 * bar, bar, spb);
        PlayTune(high, StormB, StormChordsB, B1 * bar, bar, spb);
        PlayTune(lead, StormA, StormChordsA, A2 * bar, bar, spb);
        PlayTune(harm, StormA, StormChordsA, A2 * bar, bar, spb);
        PlayTune(piano, StormA, StormChordsA, A2 * bar, bar, spb);
        PlayTune(lead, StormBridge, StormChordsBridge, BR * bar, bar, spb);

        s.Reverb(0.34, 0.84);
        return s.FinishLoop(bars * bar, 0.15);
    }

    // MARK: The boss

    private static readonly Note[][] CatA =
    {
        new[] { new Note(0, 65, 0.5), new Note(0.5, 68, 0.5), new Note(1, 72, 1), new Note(2.5, 70, 0.5), new Note(3, 68, 0.5), new Note(3.5, 65, 0.5) },
        new[] { new Note(0, 73, 1), new Note(1, 72, 0.5), new Note(1.5, 73, 0.5), new Note(2, 77, 1), new Note(3, 73, 1) },
        new[] { new Note(0, 70, 0.5), new Note(0.5, 73, 0.5), new Note(1, 77, 1), new Note(2.5, 76, 0.5), new Note(3, 77, 1) },
        new[] { new Note(0, 76, 1), new Note(1, 72, 0.5), new Note(1.5, 70, 0.5), new Note(2, 67, 1), new Note(3, 70, 1) },
        new[] { new Note(0, 65, 0.5), new Note(0.5, 68, 0.5), new Note(1, 72, 1), new Note(2.5, 75, 0.5), new Note(3, 77, 1) },
        new[] { new Note(0, 80, 1), new Note(1, 77, 0.5), new Note(1.5, 73, 0.5), new Note(2, 77, 1), new Note(3, 80, 1) },
        new[] { new Note(0, 79, 0.5), new Note(0.5, 76, 0.5), new Note(1, 72, 0.5), new Note(1.5, 76, 0.5), new Note(2, 79, 0.5), new Note(2.5, 82, 1.5) },
        new[] { new Note(0, 82, 1), new Note(1, 79, 1), new Note(2, 76, 1), new Note(3, 72, 1) },
    };
    private static readonly Note[][] CatB =
    {
        new[] { new Note(0, 80, 2), new Note(2, 85, 1), new Note(3, 84, 1) },
        new[] { new Note(0, 82, 2), new Note(2, 87, 1), new Note(3, 84, 1) },
        new[] { new Note(0, 84, 1), new Note(1, 80, 1), new Note(2, 77, 2) },
        new[] { new Note(0, 80, 1), new Note(1, 77, 1), new Note(2, 72, 1), new Note(3, 77, 1) },
        new[] { new Note(0, 85, 2), new Note(2, 80, 1), new Note(3, 77, 1) },
        new[] { new Note(0, 82, 1), new Note(1, 87, 1), new Note(2, 89, 2) },
        new[] { new Note(0, 88, 1), new Note(1, 84, 1), new Note(2, 79, 1), new Note(3, 82, 1) },
        new[] { new Note(0, 84, 2), new Note(2, 76, 1), new Note(3, 79, 1) },
    };
    // the cat's play: a little staccato figure that is passed round
    private static readonly Note[] CatPlay =
    {
        new Note(0, 77, 0.25), new Note(0.5, 75, 0.25), new Note(1, 77, 0.25), new Note(1.5, 80, 0.5), new Note(2.5, 77, 0.25), new Note(3, 75, 0.5), new Note(3.5, 72, 0.5),
    };

    private static Chord[] CatChordsIntro => new[] { FmS, FmS, FmS, C7S };
    private static Chord[] CatChordsA => new[] { FmS, DbS, BbmS, C7S, FmS, DbS, C7S, C7S };
    private static Chord[] CatChordsB => new[] { DbS, EbS, FmS, FmS, DbS, EbS, C7S, C7S };
    private static Chord[] CatChordsPlay => new[] { FmS, FmS, DbS, DbS, BbmS, BbmS, C7S, C7S };

    /// <summary>"A gata arisca brinca na tempestade": the boss's theme of stage 3.</summary>
    public static (float[] left, float[] right) CatInTheStorm()
    {
        const double bpm = 200;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(CatChordsIntro); chords.AddRange(CatChordsA); chords.AddRange(CatChordsB); chords.AddRange(CatChordsPlay); chords.AddRange(CatChordsA); chords.AddRange(CatChordsB);
        int bars = chords.Count; // 44
        var s = new Synth(bars * bar + 5);
        const int A1 = 4, B1 = 12, P1 = 20, A2 = 28, B2 = 36;
        int[] tresillo = { 0, 1, 3, 4, 6, 7 };
        int[] order = { 0, 2, 1, 3, 2, 1, 3, 2 };

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < A1, chorus = (b >= B1 && b < P1) || b >= B2, play = b >= P1 && b < A2;

            // the left hand, very low and heavy: a prowling pulse in the 3 + 3 + 2 pattern, two octaves doubled, the last hit leaping; underneath, a soft bass for weight
            if (b >= 2)
            {
                for (int k = 0; k < tresillo.Length; k++)
                {
                    int i = tresillo[k];
                    double t = t0 + i * spb / 2;
                    int note = ch.Bass - 12 + (k == 5 ? 12 : 0);
                    s.Piano(t, spb * 0.4, note, i % 2 == 0 ? 0.36 : 0.27, -0.2, 0.75);
                    s.Piano(t, spb * 0.4, note + 12, i % 2 == 0 ? 0.2 : 0.15, -0.1, 0.6);
                }
                s.Bass(t0, bar, ch.Bass - 12, 0.13);
            }
            else s.Bass(t0, bar, ch.Bass - 12, 0.1 + 0.03 * b);       // the opening is never silent: the low pedal from the very first beat


            // the right hand: chords hammered on the offbeats; in the cat's play, a staccato figure; in the chorus, fuller and higher
            if (b >= 4 && !play)
                foreach (var beat in new[] { 0.5, 1.5, 2.5, 3.5 })
                    for (int i = 1; i < ch.Tones.Length; i++) s.Piano(t0 + beat * spb, spb * 0.3, ch.Tones[i] + (chorus ? 12 : 0), chorus ? 0.075 : 0.055, 0.1 + i * 0.08, 0.15);
            if (play)
                for (int i = 0; i < 8; i++) s.Piano(t0 + i * spb / 2, spb * 0.3, ch.Tones[order[i]], (i % 4 == 0 ? 0.07 : 0.045), -0.3 + i * 0.09, 0.2);

            // the toms and the cymbals: a timpani on one and three, the thunder in the fills
            if (b <= 3)
            {
                // the roll of the toms fills the whole opening (it used to begin on the third bar, and the first three seconds of the fight were silence), growing to the climb of the piano
                for (int i = 0; i < 16; i++) s.Tom(t0 + i * spb / 4, 36 + (i % 4), 0.06 + 0.12 * (b * 16 + i) / 64.0, -0.4 + (i % 4) * 0.25);
                if (b == 2) s.Crash(t0, 0.2);
            }
            else if (b >= 4)
            {
                bool section = b == A1 || b == B1 || b == P1 || b == A2 || b == B2;
                if (section) s.Crash(t0, 0.24);
                s.Tom(t0, 33, 0.34, -0.2); s.Tom(t0 + 2 * spb, 33, 0.3, 0.2);
                if (chorus) { s.Tom(t0 + spb, 38, 0.2, -0.3); s.Tom(t0 + 3 * spb, 38, 0.22, 0.3); }
                if (b % 4 == 3) { s.Tom(t0 + 3 * spb, 41, 0.28, -0.4); s.Tom(t0 + 3.25 * spb, 38, 0.28, -0.1); s.Tom(t0 + 3.5 * spb, 36, 0.3, 0.2); s.Tom(t0 + 3.75 * spb, 33, 0.32, 0.4); }
            }

            // trumpets underneath in the chorus; the lightning in the high piano
            if (chorus) for (int i = 1; i < ch.Tones.Length; i++) s.Trumpet(t0, bar * 0.97, ch.Tones[i] - 12, 0.04, -0.3 + i * 0.2, 0.6);
            if (b >= 4 && b % 2 == 1 && !intro)
            {
                int[] flash = { 96, 93, 91, 89, 86 };
                for (int i = 0; i < flash.Length; i++) s.Piano(t0 + 3 * spb + i * 0.05, 0.25, flash[i], 0.03, 0.6 - i * 0.3, 0);
            }
        }

        // the opening: a roll of the toms, then the piano climbs the chord in octaves and the trumpets call over it
        s.Crash(0, 0.24);
        foreach (var (beat, midi, len) in new[] { (0.0, 53, 1.0), (1.0, 56, 1.0), (2.0, 60, 1.0), (3.0, 65, 1.0) })
        {
            s.Piano(2 * bar + beat * spb, len * spb * 0.95, midi, 0.3, -0.2, 0.2);
            s.Piano(2 * bar + beat * spb, len * spb * 0.95, midi - 12, 0.2, -0.1, 0.4);
            s.Trumpet(2 * bar + beat * spb, len * spb * 0.9, midi, 0.07, -0.2, 0.9);
        }
        foreach (var (beat, midi, len) in new[] { (0.0, 68, 0.5), (0.5, 72, 0.5), (1.0, 77, 1.0), (2.0, 80, 2.0) })
        {
            s.Piano(3 * bar + beat * spb, len * spb * 0.95, midi, 0.34, 0.1, 0.1);
            s.Piano(3 * bar + beat * spb, len * spb * 0.95, midi - 12, 0.22, -0.1, 0.3);
            s.Trumpet(3 * bar + beat * spb, len * spb * 0.9, midi, 0.1, 0.1, 1.1);
        }

        // the tune is the PIANO's: the right hand sings it in octaves (and in the chorus in chords), the trumpets double and answer it, sharp and staccato, a step behind in volume
        Action<double, double, int, Chord, int> lead = (t, d, m, c, b) => { s.Piano(t, d * 0.93, m, 0.34, 0.12, 0.1); s.Piano(t, d * 0.93, m - 12, 0.2, -0.12, 0.3); };
        Action<double, double, int, Chord, int> leadChords = (t, d, m, c, b) =>
        {
            s.Piano(t, d * 0.93, m, 0.34, 0.12, 0.1); s.Piano(t, d * 0.93, m - 12, 0.22, -0.12, 0.3);
            s.Piano(t, d * 0.93, Below(m, c), 0.2, 0.0, 0.15); s.Piano(t, d * 0.93, Below(Below(m, c), c), 0.14, -0.05, 0.2);
        };
        Action<double, double, int, Chord, int> octave = (t, d, m, c, b) => s.Piano(t, d * 0.9, m + 12, 0.12, 0.3, 0.0);
        Action<double, double, int, Chord, int> trumpet = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, m, 0.1, 0.25, 1.15);
        Action<double, double, int, Chord, int> harm = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, Below(m, c), 0.09, -0.4, 1.0);
        Action<double, double, int, Chord, int> high = (t, d, m, c, b) => s.Trumpet(t, d * 0.85, m + 12, 0.07, 0.4, 1.4);
        Action<double, double, int, Chord, int> deep = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, m - 24, 0.08, -0.1, 0.8);
        PlayTune(lead, CatA, CatChordsA, A1 * bar, bar, spb);
        PlayTune(trumpet, CatA, CatChordsA, A1 * bar, bar, spb);
        foreach (int start in new[] { B1, B2 })
        {
            PlayTune(leadChords, CatB, CatChordsB, start * bar, bar, spb);
            PlayTune(octave, CatB, CatChordsB, start * bar, bar, spb);
            PlayTune(trumpet, CatB, CatChordsB, start * bar, bar, spb);
            PlayTune(harm, CatB, CatChordsB, start * bar, bar, spb);
            PlayTune(deep, CatB, CatChordsB, start * bar, bar, spb);
            if (start == B2) PlayTune(high, CatB, CatChordsB, start * bar, bar, spb);
        }
        PlayTune(leadChords, CatA, CatChordsA, A2 * bar, bar, spb);
        PlayTune(octave, CatA, CatChordsA, A2 * bar, bar, spb);
        PlayTune(trumpet, CatA, CatChordsA, A2 * bar, bar, spb);
        PlayTune(harm, CatA, CatChordsA, A2 * bar, bar, spb);
        PlayTune(high, CatA, CatChordsA, A2 * bar, bar, spb);

        // the cat's play: the staccato figure is the piano's, in octaves, and the trumpets answer it every second bar
        int[] shift = { 0, 0, -4, -4, -7, -7, -5, -5 };
        for (int b = 0; b < 8; b++)
            foreach (var n in CatPlay)
            {
                double t = (P1 + b) * bar + n.Beat * spb;
                int midi = n.Midi + shift[b];
                s.Piano(t, n.Len * spb * 0.9, midi, 0.3, b % 2 == 0 ? -0.3 : 0.3, 0.15);
                s.Piano(t, n.Len * spb * 0.9, midi - 12, 0.2, 0.0, 0.3);
                if (b % 2 == 1) s.Trumpet(t, n.Len * spb * 0.8, midi + 12, 0.1, 0.4, 1.2);
            }

        s.Reverb(0.2, 0.76);
        return s.FinishLoop(bars * bar, 0.18);
    }
}
