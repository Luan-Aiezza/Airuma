using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The two themes of stage 4, the river inside the mountain: drums and trumpets (the drums are the heart of it).
///
/// "Hibernação em meio a morcegos e musgo" (the stage, 66 BPM, D minor): calm and slow. The big drums beat slowly, like the heart of an animal asleep for the winter; round them, small drums
/// murmur like bats and a wooden knock drips like water; a lone trumpet, low and in echo, sings a slow tune, and in the middle the drums alone go on while the cave breathes.
/// "A grande fera Deidane" (the boss, 172 BPM, A minor with the flat second): the animal wakes. The DRUMS rule, in a tribal gallop of triplets with the kick under the accents and the floor toms and the snare
/// on the backbeat; the sax and the trumpet are secondary, they sing the tune and answer the drums with roars; the break puts five great drums against four beats.
/// </summary>
public static partial class Songs
{
    // MARK: The stage

    private static readonly Note[][] CaveA =
    {
        new[] { new Note(0, 62, 2), new Note(2, 65, 1), new Note(3, 64, 1) },
        new[] { new Note(0, 62, 1), new Note(1, 57, 1), new Note(2, 62, 2) },
        new[] { new Note(0, 65, 2), new Note(2, 62, 1), new Note(3, 58, 1) },
        new[] { new Note(0, 64, 2), new Note(2, 60, 1), new Note(3, 64, 1) },
        new[] { new Note(0, 65, 1.5), new Note(1.5, 67, 0.5), new Note(2, 69, 2) },
        new[] { new Note(0, 67, 2), new Note(2, 62, 1), new Note(3, 67, 1) },
        new[] { new Note(0, 64, 1), new Note(1, 61, 1), new Note(2, 57, 2) },
        new[] { new Note(0, 62, 4) },
    };
    private static readonly Note[][] CaveB =
    {
        new[] { new Note(0, 70, 2), new Note(2, 65, 1), new Note(3, 70, 1) },
        new[] { new Note(0, 72, 2), new Note(2, 67, 2) },
        new[] { new Note(0, 74, 2), new Note(2, 69, 1), new Note(3, 74, 1) },
        new[] { new Note(0, 72, 1.5), new Note(1.5, 69, 0.5), new Note(2, 65, 2) },
        new[] { new Note(0, 70, 2), new Note(2, 74, 1), new Note(3, 72, 1) },
        new[] { new Note(0, 72, 2), new Note(2, 76, 1), new Note(3, 72, 1) },
        new[] { new Note(0, 69, 1), new Note(1, 73, 1), new Note(2, 76, 2) },
        new[] { new Note(0, 74, 4) },
    };
    private static Chord[] CaveChordsIntro => new[] { Dm, Dm };
    private static Chord[] CaveChordsA => new[] { Dm, Dm, Bb, C, Dm, Gm, A, Dm };
    private static Chord[] CaveChordsB => new[] { Bb, C, Dm, F, Bb, C, A, A };
    private static Chord[] CaveChordsBridge => new[] { Dm, A };

    /// <summary>"Hibernação em meio a morcegos e musgo": the theme of stage 4.</summary>
    public static (float[] left, float[] right) CaveHibernation()
    {
        const double bpm = 66;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(CaveChordsIntro); chords.AddRange(CaveChordsA); chords.AddRange(CaveChordsB); chords.AddRange(CaveChordsA); chords.AddRange(CaveChordsBridge);
        int bars = chords.Count; // 28
        var s = new Synth(bars * bar + 7);
        const int A1 = 2, B1 = 10, A2 = 18, BR = 26;

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < A1, chorus = b >= B1 && b < A2, bridge = b >= BR;

            // the heart: the big drums beat slowly (a deep stroke on one, a softer answer on three; in the chorus the beat is heavier, with a second voice)
            s.Taiko(t0, 31, intro ? 0.34 : 0.4, -0.15);
            s.Taiko(t0 + 2 * spb, 34, 0.26, 0.15);
            if (b >= 2 && b % 2 == 1) s.Taiko(t0 + 3.25 * spb, 38, 0.16, 0.3);
            if (chorus) { s.Taiko(t0 + spb, 36, 0.2, -0.3); s.Taiko(t0 + 2.75 * spb, 38, 0.22, 0.2); }

            // the bats: small drums murmuring in the dark, a few soft strokes that never keep one pattern for long
            if (b >= 2 && !bridge)
            {
                double[] murmur = (b % 4) switch { 0 => new[] { 0.5, 1.25, 2.5, 3.75 }, 1 => new[] { 0.75, 1.5, 3.0, 3.5 }, 2 => new[] { 0.25, 1.0, 2.25, 3.25 }, _ => new[] { 0.5, 1.75, 2.75, 3.5 } };
                for (int i = 0; i < murmur.Length; i++) s.Conga(t0 + murmur[i] * spb, i % 2 == 0 ? 64 : 69, chorus ? 0.075 : 0.05, i % 2 == 0 ? -0.5 : 0.5);
            }
            // the water: a wooden knock that drips, now here, now there
            if (b >= 4 && b % 2 == 0) s.Clave(t0 + (b % 4 == 0 ? 1.5 : 3.25) * spb, 0.06, b % 4 == 0 ? -0.5 : 0.5);

            // a swell of drums into the next part
            if (b + 1 == A1 + 8 || b + 1 == B1 || b + 1 == A2 || b + 1 == BR || b == bars - 1)
                for (int i = 0; i < 12; i++) s.Tom(t0 + (1.5 + i * 0.2) * spb, 40 - i / 4, 0.05 + 0.025 * i, -0.4 + i * 0.07);
            if (b == B1 || b == A2) s.Crash(t0, 0.1);

            // the drone: trumpets, very low and soft, on the root and the fifth, so long that they seem to be the mountain
            if (b >= 2)
            {
                s.Trumpet(t0, bar * 0.98, ch.Bass + 12, chorus ? 0.05 : 0.035, -0.2, 0.4);
                s.Trumpet(t0, bar * 0.98, ch.Bass + 19, chorus ? 0.035 : 0.022, 0.2, 0.4);
            }
        }

        // the opening: a lone trumpet call, far away
        s.Trumpet(0.5 * bar, 3 * spb, 62, 0.12, 0.1, 0.7);
        s.Trumpet(0.5 * bar + 4 * spb, 3 * spb, 57, 0.1, 0.1, 0.7);

        // the tune: one trumpet, low, with its echo a beat later and softer, as in a cave; in the chorus a second voice a third below
        Action<double, double, int, Chord, int> lead = (t, d, m, c, b) => s.Trumpet(t, d * 0.96, m, 0.16, 0.1, 0.6);
        Action<double, double, int, Chord, int> echo = (t, d, m, c, b) => s.Trumpet(t + spb, d * 0.9, m, 0.05, -0.4, 0.4);
        Action<double, double, int, Chord, int> harm = (t, d, m, c, b) => s.Trumpet(t, d * 0.96, Below(m, c), 0.09, -0.3, 0.6);
        Action<double, double, int, Chord, int> high = (t, d, m, c, b) => s.Trumpet(t, d * 0.9, m + 12, 0.04, 0.4, 0.9);
        PlayTune(lead, CaveA, CaveChordsA, A1 * bar, bar, spb);
        PlayTune(echo, CaveA, CaveChordsA, A1 * bar, bar, spb);
        PlayTune(lead, CaveB, CaveChordsB, B1 * bar, bar, spb);
        PlayTune(harm, CaveB, CaveChordsB, B1 * bar, bar, spb);
        PlayTune(echo, CaveB, CaveChordsB, B1 * bar, bar, spb);
        PlayTune(lead, CaveA, CaveChordsA, A2 * bar, bar, spb);
        PlayTune(harm, CaveA, CaveChordsA, A2 * bar, bar, spb);
        PlayTune(high, CaveA, CaveChordsA, A2 * bar, bar, spb);
        PlayTune(echo, CaveA, CaveChordsA, A2 * bar, bar, spb);
        // the last long note, falling asleep again
        s.Trumpet(BR * bar, 4 * spb, 62, 0.12, 0.1, 0.6); s.Trumpet((BR + 1) * bar, 4 * spb, 57, 0.1, 0.1, 0.5, 0, 2);

        s.Reverb(0.42, 0.87);
        return s.FinishLoop(bars * bar, 0.15);
    }

    // MARK: The boss

    private static readonly Chord AmB = new(45, 57, 60, 64, 69), GB = new(43, 55, 59, 62, 67), E7B = new(40, 56, 59, 62, 64);
    private const double T = 1.0 / 3;      // a third of a beat: the gallop is in triplets

    private static readonly Note[][] BeastA =
    {
        new[] { new Note(0, 69, T), new Note(2 * T, 72, T), new Note(1, 69, T), new Note(1 + 2 * T, 67, T), new Note(2, 69, 2 * T), new Note(3, 64, 1) },
        new[] { new Note(0, 69, T), new Note(2 * T, 72, T), new Note(1, 76, T), new Note(1 + 2 * T, 72, T), new Note(2, 76, 2 * T), new Note(3, 72, 1) },
        new[] { new Note(0, 70, T), new Note(2 * T, 70, T), new Note(1, 74, T), new Note(1 + 2 * T, 70, T), new Note(2, 77, 1), new Note(3, 74, 1) },
        new[] { new Note(0, 72, T), new Note(2 * T, 69, T), new Note(1, 64, T), new Note(1 + 2 * T, 69, T), new Note(2, 72, 2) },
        new[] { new Note(0, 69, T), new Note(2 * T, 72, T), new Note(1, 69, T), new Note(1 + 2 * T, 67, T), new Note(2, 69, 2 * T), new Note(3, 76, 1) },
        new[] { new Note(0, 74, T), new Note(2 * T, 71, T), new Note(1, 67, T), new Note(1 + 2 * T, 71, T), new Note(2, 74, 1), new Note(3, 79, 1) },
        new[] { new Note(0, 77, T), new Note(2 * T, 72, T), new Note(1, 69, T), new Note(1 + 2 * T, 72, T), new Note(2, 77, 2) },
        new[] { new Note(0, 76, T), new Note(2 * T, 80, T), new Note(1, 76, T), new Note(1 + 2 * T, 71, T), new Note(2, 68, 1), new Note(3, 71, 1) },
    };
    private static readonly Note[][] BeastB =
    {
        new[] { new Note(0, 77, 1), new Note(1, 81, 1), new Note(2, 84, 2) },
        new[] { new Note(0, 83, 1), new Note(1, 79, 1), new Note(2, 74, 2) },
        new[] { new Note(0, 81, 1), new Note(1, 76, 1), new Note(2, 81, 2 * T), new Note(2 + 2 * T, 84, T), new Note(3, 83, 1) },
        new[] { new Note(0, 81, 2), new Note(2, 76, 1), new Note(3, 72, 1) },
        new[] { new Note(0, 77, 1), new Note(1, 81, 1), new Note(2, 84, 2) },
        new[] { new Note(0, 86, 1), new Note(1, 83, 1), new Note(2, 79, 2) },
        new[] { new Note(0, 82, 1), new Note(1, 86, 1), new Note(2, 89, 2) },
        new[] { new Note(0, 88, 1), new Note(1, 83, 1), new Note(2, 80, 1), new Note(3, 83, 1) },
    };
    private static Chord[] BeastChordsIntro => new[] { AmB, AmB, AmB, E7B };
    private static Chord[] BeastChordsA => new[] { AmB, AmB, Bb, AmB, AmB, GB, F, E7B };
    private static Chord[] BeastChordsB => new[] { F, GB, AmB, AmB, F, GB, Bb, E7B };
    private static Chord[] BeastChordsBreak => new[] { AmB, AmB, AmB, AmB, Bb, Bb, E7B, E7B };

    /// <summary>
    /// "A grande fera Deidane": the boss's theme of stage 4, a new one. A tribal GALLOP in triplets (128 BPM, A minor with the flat second): the great drums fall on the strong and the weak thirds of
    /// every beat (deep, high, deep, high), the kick under the accents, the floor toms and the snare on the backbeat; the sax and the trumpet are secondary, they sing the tune in unison and in octaves over the drums and
    /// answer them with roars. The break puts five great drums against four beats, and three floor toms against two.
    /// </summary>
    public static (float[] left, float[] right) GreatBeast()
    {
        const double bpm = 172;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(BeastChordsIntro); chords.AddRange(BeastChordsA); chords.AddRange(BeastChordsB); chords.AddRange(BeastChordsBreak); chords.AddRange(BeastChordsA); chords.AddRange(BeastChordsB);
        int bars = chords.Count; // 44
        var s = new Synth(bars * bar + 5);
        const int A1 = 4, B1 = 12, K1 = 20, A2 = 28, B2 = 36;
        int[] gallop = { 0, 2, 3, 5, 6, 8, 9, 11 };             // thirds of a beat (12 in the bar) where a great drum falls: strong and weak thirds of every beat

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool chorus = (b >= B1 && b < K1) || b >= B2, brk = b >= K1 && b < A2;
            double third = spb / 3;

            if (b < 2)
            {
                // the first steps, far away and growing: one great drum a beat
                for (int i = 0; i < 4; i++) s.Taiko(t0 + i * spb, i % 2 == 0 ? 35 : 40, 0.28 + 0.06 * i + 0.1 * b, i % 2 == 0 ? -0.25 : 0.25);
            }
            else if (b == 2 || b == 3)
            {
                // the gallop begins and swells; a roll of the toms into the first part
                foreach (int i in gallop) s.Taiko(t0 + i * third, i % 3 == 0 ? 35 : 40, 0.24 + 0.2 * (b - 2) + (i % 3 == 0 ? 0.1 : 0), i < 6 ? -0.25 : 0.25);
                if (b == 3) for (int i = 0; i < 12; i++) s.Tom(t0 + (2 + i / 6.0) * spb, 47 - (i % 6), 0.12 + 0.03 * i, -0.4 + (i % 6) * 0.16);
                s.Kick(t0, 0.5);
            }
            else
            {
                bool section = b == A1 || b == B1 || b == K1 || b == A2 || b == B2;
                if (section) { s.Crash(t0, 0.3); s.Taiko(t0, 33, 0.6, 0); }
                if (brk)
                {
                    // five great drums against the four beats, three floor toms against two, a kick on the one: the pattern leans over, then falls back
                    foreach (var k in Enumerable.Range(0, 5)) s.Taiko(t0 + k * bar / 5, k == 0 ? 33 : (k % 2 == 0 ? 35 : 40), k == 0 ? 0.6 : 0.46, -0.3 + k * 0.15);
                    foreach (var k in Enumerable.Range(0, 3)) s.Tom(t0 + k * (2 * spb) / 3 + 2 * spb * ((b - K1) % 2) , 45 + k * 2, 0.36, -0.3 + k * 0.3);
                    s.Kick(t0, 0.55); if ((b - K1) % 2 == 1) s.Kick(t0 + 2 * spb, 0.5);
                    for (int i = 0; i < 8; i++) s.Conga(t0 + (i + 0.5) * spb / 2, new[] { 62, 65, 59, 65 }[i % 4], 0.1, -0.5 + i * 0.14);
                }
                else
                {
                    // the gallop: the great drums on the strong and the weak thirds of every beat, deep and high in turns, the accents on one and three
                    for (int k = 0; k < gallop.Length; k++)
                    {
                        int i = gallop[k];
                        bool accent = i == 0 || i == 6;
                        s.Taiko(t0 + i * third, k % 2 == 0 ? 35 : 40, accent ? 0.62 : (k % 2 == 0 ? 0.46 : 0.36), k < 4 ? -0.25 : 0.25);
                        if (accent) s.Kick(t0 + i * third, 0.5);
                    }
                    // the drive: sixteenth hats over the gallop and a second kick on the way into the accents
                    for (int i = 0; i < 16; i++) s.Hat(t0 + i * spb / 4, i % 4 == 0 ? 0.07 : (i % 2 == 0 ? 0.045 : 0.03), 0.3, chorus && i == 14);
                    s.Kick(t0 + 5 * third, 0.3); s.Kick(t0 + 11 * third, 0.3);
                    // the backbeat: the floor toms and a deep snare on two and four
                    s.Tom(t0 + spb, 45, 0.4, -0.2); s.Tom(t0 + 3 * spb, 45, 0.42, 0.2);
                    s.Snare(t0 + spb, 0.2); s.Snare(t0 + 3 * spb, 0.22);
                    for (int i = 1; i < 12; i += 3) s.Conga(t0 + i * third, 62, 0.08, -0.4 + (i % 6) * 0.15);
                    if (chorus) { s.Crash(t0 + spb, 0.14); s.Crash(t0 + 3 * spb, 0.14); for (int k = 0; k < 4; k++) s.Tom(t0 + k * spb + 2 * third, 50, 0.14, k % 2 == 0 ? -0.4 : 0.4); }
                }
                bool intoPart = b + 1 == B1 || b + 1 == K1 || b + 1 == A2 || b + 1 == B2;
                if (intoPart)
                {
                    for (int i = 0; i < 12; i++) s.Taiko(t0 + (2 + i / 6.0) * spb, 40 - (i % 6), 0.3 + 0.04 * i, -0.4 + (i % 6) * 0.16);
                    s.Taiko(t0 + 3.5 * spb, 31, 0.7, 0);
                }
            }

            // under everything, a baritone sax that growls on the root, long (the weight of the beast's breath)
            if (b >= 4 && !brk) s.Sax(t0, bar * 0.95, ch.Bass + 24, chorus ? 0.07 : 0.05, -0.2, 0.9, 0.7);
        }

        // the opening: a low roar in the trumpet, falling, and the sax answering with the first notes of the tune
        s.Crash(0, 0.3);
        s.Trumpet(2 * bar, 2 * bar - 0.1, 64, 0.15, 0.0, 1.0, 0.9, 6);
        foreach (var (beat, midi, len) in new[] { (0.0, 64, 1.0), (1.0, 67, 1.0), (2.0, 71, 1.0), (3.0, 76, 1.0) })
            s.Sax(3 * bar + beat * spb, len * spb * 0.9, midi, 0.16, 0.1, 0.8, 1.1);

        // the tune is secondary to the drums: the sax sings it (growling), the trumpet doubles it an octave up and, in the chorus, takes the lead with the sax under it
        Action<double, double, int, Chord, int> sax = (t, d, m, c, b) => s.Sax(t, d * 0.85, m, 0.17, -0.2, 0.55, 1.15);
        Action<double, double, int, Chord, int> trumpetUp = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, m + 12, 0.08, 0.3, 1.1, 0.3, b % 4 == 3 && d >= spb ? 3 : 0);
        Action<double, double, int, Chord, int> trumpet = (t, d, m, c, b) => s.Trumpet(t, d * 0.82, m, 0.16, 0.25, 1.2, 0.35);
        Action<double, double, int, Chord, int> trumpetDouble = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, m, 0.09, 0.35, 1.2, 0.3);
        Action<double, double, int, Chord, int> saxHarm = (t, d, m, c, b) => s.Sax(t, d * 0.85, Below(m, c), 0.1, -0.45, 0.4, 1.0);
        PlayTune(sax, BeastA, BeastChordsA, A1 * bar, bar, spb);
        PlayTune(trumpetDouble, BeastA, BeastChordsA, A1 * bar, bar, spb);
        PlayTune(sax, BeastA, BeastChordsA, A2 * bar, bar, spb);
        PlayTune(trumpetDouble, BeastA, BeastChordsA, A2 * bar, bar, spb);
        PlayTune(trumpetUp, BeastA, BeastChordsA, A2 * bar, bar, spb);
        foreach (int start in new[] { B1, B2 })
        {
            PlayTune(trumpet, BeastB, BeastChordsB, start * bar, bar, spb);
            PlayTune(sax, BeastB, BeastChordsB, start * bar, bar, spb);
            PlayTune(saxHarm, BeastB, BeastChordsB, start * bar, bar, spb);
                        if (start == B2) PlayTune(trumpetUp, BeastB, BeastChordsB, start * bar, bar, spb);
        }

        // the break: the drums are alone, and now and then the beast roars, a long falling note in the trumpet and the sax together
        foreach (var (barIndex, midi) in new[] { (1, 64), (3, 69), (5, 65), (7, 71) })
        {
            s.Trumpet((K1 + barIndex) * bar, 2.5 * spb, midi + 12, 0.14, 0.2, 1.1, 0.9, 7);
            s.Sax((K1 + barIndex) * bar, 2.5 * spb, midi, 0.16, -0.2, 1.0, 1.1);
        }

        s.Reverb(0.2, 0.8);
        return s.FinishLoop(bars * bar, 0.28);      // louder than the others' 0.17: a drum-heavy mix has high peaks, and the soft limiter keeps the average low unless it is asked for more
    }
}
