using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The theme of the last boss, Mama Killa, made from the theme of the last stage ("O rio que percorre o universo": the same tunes and the same chords, in A minor) but with the whole sky against us:
/// faster (344 beats a minute against 288), the timpani and a drum kit that never stop, the organ in full chords, trumpets in octaves, and the choir much stronger: five parts in the hymn, men
/// and women in octaves with a fifth in the chant, and, in the fury (the last part), the whole choir, a sixteenth-note piano and the hymn sung high.
/// </summary>
public static partial class Songs
{
    private static Chord[] MoonChordsIntro => new[] { AmB, AmB };
    private static Chord[] MoonChordsFury => new[] { AmB, F, C, GB, AmB, F, E, E };

    /// <summary>"O coro da lua devoradora": the theme of the fight with Mama Killa.</summary>
    public static (float[] left, float[] right) MoonOfFury()
    {
        const double bpm = 344;
        double spb = 60 / bpm, bar = 4 * spb;
        var chords = new List<Chord>();
        chords.AddRange(MoonChordsIntro); chords.AddRange(RiverChordsChant); chords.AddRange(RiverChordsHymn); chords.AddRange(RiverChordsRiver); chords.AddRange(MoonChordsFury); chords.AddRange(RiverChordsHymn);
        int bars = chords.Count;                       // 42
        var s = new Synth(bars * bar + 7);
        const int C1 = 2, H1 = 10, R1 = 18, F1 = 26, H2 = 34;

        for (int b = 0; b < bars; b++)
        {
            var ch = chords[b];
            double t0 = b * bar;
            bool intro = b < C1, chant = b >= C1 && b < H1, hymn = (b >= H1 && b < R1) || b >= H2, river = b >= R1 && b < F1, fury = b >= F1 && b < H2;

            // the organ: the pedal, its octave and its fifth, the whole bar; full chords from the hymn on
            s.Organ(t0, bar * 0.99, ch.Bass, 0.13, -0.2);
            s.Organ(t0, bar * 0.99, ch.Bass + 12, 0.1, 0.2);
            s.Organ(t0, bar * 0.99, ch.Bass + 19, 0.06, 0.0);
            if (!intro) for (int i = 1; i < ch.Tones.Length; i++) s.Organ(t0, bar * 0.99, ch.Tones[i], hymn || fury ? 0.06 : 0.04, -0.4 + i * 0.27);

            // the drums never stop: the timpani on every beat of the hymn and the fury, a kit under the rest
            if (intro) { for (int i = 0; i < 8; i++) s.Tom(t0 + i * bar / 8, 45 - (i % 4), 0.1 + 0.03 * (b * 8 + i) / 8.0, -0.3 + (i % 4) * 0.2); }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    bool strong = hymn || fury;
                    if (i % 2 == 0 || strong) s.Kick(t0 + i * spb, strong ? 0.3 : 0.22);
                    if (i % 2 == 1) s.Snare(t0 + i * spb, strong ? 0.2 : 0.15);
                    if (strong) s.Tom(t0 + i * spb, 33, 0.26, -0.2);
                    s.Hat(t0 + i * spb, 0.05); s.Hat(t0 + (i + 0.5) * spb, 0.04);
                    if (fury) { s.Kick(t0 + (i + 0.5) * spb, 0.12); s.Hat(t0 + (i + 0.25) * spb, 0.03); s.Hat(t0 + (i + 0.75) * spb, 0.03); }
                }
                if (b == C1 || b == H1 || b == R1 || b == F1 || b == H2) s.Crash(t0, 0.22);
            }
            // a roll into every part
            if (b + 1 == C1 || b + 1 == H1 || b + 1 == R1 || b + 1 == F1 || b + 1 == H2) for (int i = 0; i < 16; i++) s.Tom(t0 + (2 + i / 8.0) * spb, 45 - (i % 6), 0.08 + 0.015 * i, -0.4 + (i % 6) * 0.16);

            // the piano: the river of arpeggios (eighths; sixteenths in the fury, that never stop), octaves in the hymn
            int[] order = { 0, 1, 2, 3, 2, 1, 3, 2 };
            if (river || fury)
            {
                int steps = fury ? 16 : 8;
                for (int i = 0; i < steps; i++) s.Piano(t0 + i * 4 * spb / steps, spb * (steps == 16 ? 0.7 : 1.2), ch.Tones[order[i % 8]] + 12 + (i % 8 > 4 ? 12 : 0), i % 4 == 0 ? 0.1 : 0.065, -0.5 + (i % 8) * 0.14, 0.15);
            }
            if (hymn) { s.Piano(t0, bar * 0.98, ch.Bass - 12, 0.14, -0.3, 0.7); s.Piano(t0, spb * 1.5, ch.Tones[2] + 12, 0.07, 0.2, 0.3); s.Piano(t0 + 2 * spb, spb * 1.5, ch.Tones[3] + 12, 0.07, 0.2, 0.3); }
            if (chant) for (int i = 0; i < 4; i++) s.Piano(t0 + i * spb, bar * 0.2, ch.Tones[order[i]], 0.07, 0.1, 0.4);

            // the choir, strong: in the introduction a great chord, "ah", all the voices; in the chant men and women in octaves with the fifth; in the hymn five parts; in the river the whole choir holds under the trumpet; in the fury, it never lets go
            if (intro)
            {
                s.Choir(t0, bar, ch.Bass + 12, 0.12, -0.4, 1); s.Choir(t0, bar, ch.Tones[1] + 12, 0.1, -0.15, 0); s.Choir(t0, bar, ch.Tones[2] + 12, 0.1, 0.15, 0); s.Choir(t0, bar, ch.Tones[3] + 12, 0.1, 0.4, 0);
            }
            if (hymn)
            {
                s.Choir(t0, bar * 0.99, ch.Bass, 0.14, -0.6, 0);
                s.Choir(t0, bar * 0.99, ch.Bass + 12, 0.1, -0.4, 1);
                s.Choir(t0, bar * 0.99, ch.Tones[1], 0.1, -0.2, 1);
                s.Choir(t0, bar * 0.99, ch.Tones[2] + 12, 0.1, 0.2, 0);
                s.Choir(t0, bar * 0.99, ch.Tones[3] + 12, 0.09, 0.4, 0);
            }
            if (river)
            {
                s.Choir(t0, bar * 0.99, ch.Tones[2] + 12, 0.1, 0.3, 1); s.Choir(t0, bar * 0.99, ch.Tones[1], 0.1, -0.3, 1);
                s.Choir(t0, bar * 0.99, ch.Bass + 12, 0.09, -0.5, 0); s.Choir(t0, bar * 0.99, ch.Tones[3] + 12, 0.08, 0.5, 0);
            }
            if (fury)
            {
                foreach (var (m, pan, vowel) in new[] { (ch.Bass, -0.6, 0), (ch.Bass + 12, -0.4, 1), (ch.Tones[1], -0.2, 1), (ch.Tones[2], 0.0, 0), (ch.Tones[2] + 12, 0.2, 0), (ch.Tones[3] + 12, 0.4, 0), (ch.Tones[1] + 24, 0.6, 0) })
                    s.Choir(t0, bar * 0.99, m, 0.1, pan, vowel);
            }
        }

        // the opening: the bell tolls and a great trumpet calls over the timpani
        foreach (var (beat, midi) in new[] { (0.0, 69), (4.0, 69), (8.0, 76) }) s.Bell(beat * spb, midi + 12, 0.18, 0.1);
        s.Trumpet(0.5 * bar, 3 * spb, 69, 0.14, 0.3, 0.9, 0.1);
        s.Trumpet(0.5 * bar, 3 * spb, 81, 0.1, -0.3, 0.9, 0.1);

        // the chant: women and men in unison an octave apart, and the fifth, with the organ; a trumpet answers in the empty beats
        Action<double, double, int, Chord, int> women = (t, d, m, c, b) => s.Choir(t, d * 0.97, m + 12, 0.2, 0.2, 0);
        Action<double, double, int, Chord, int> men = (t, d, m, c, b) => s.Choir(t, d * 0.97, m, 0.19, -0.2, 1);
        Action<double, double, int, Chord, int> fifth = (t, d, m, c, b) => s.Choir(t, d * 0.97, m + 7, 0.1, 0.0, 0);
        Action<double, double, int, Chord, int> answer = (t, d, m, c, b) => { if (b % 2 == 1) s.Trumpet(t + 2 * spb, spb * 1.8, m + 12, 0.14, -0.5, 0.9, 0.1); };
        PlayTune(women, RiverChant, RiverChordsChant, C1 * bar, bar, spb);
        PlayTune(men, RiverChant, RiverChordsChant, C1 * bar, bar, spb);
        PlayTune(fifth, RiverChant, RiverChordsChant, C1 * bar, bar, spb);
        PlayTune(answer, RiverChant, RiverChordsChant, C1 * bar, bar, spb);

        // the hymn: the sopranos sing it with the organ in octaves and trumpets holding the chord under it; the second time the trumpets sing it with them
        Action<double, double, int, Chord, int> sopranos = (t, d, m, c, b) => s.Choir(t, d * 0.97, m, 0.22, 0.3, 0);
        Action<double, double, int, Chord, int> altos = (t, d, m, c, b) => s.Choir(t, d * 0.97, Below(m, c), 0.16, -0.3, 1);
        Action<double, double, int, Chord, int> organLead = (t, d, m, c, b) => { s.Organ(t, d * 0.97, m - 12, 0.09, 0.1); s.Organ(t, d * 0.97, m, 0.07, 0.2); };
        Action<double, double, int, Chord, int> trumpetHymn = (t, d, m, c, b) => { s.Trumpet(t, d * 0.9, m, 0.14, 0.2, 0.9, 0.15); s.Trumpet(t, d * 0.9, m - 12, 0.1, -0.2, 0.8, 0.15); };
        foreach (int start in new[] { H1, H2 })
        {
            PlayTune(sopranos, RiverHymn, RiverChordsHymn, start * bar, bar, spb);
            PlayTune(altos, RiverHymn, RiverChordsHymn, start * bar, bar, spb);
            PlayTune(organLead, RiverHymn, RiverChordsHymn, start * bar, bar, spb);
            PlayTune(trumpetHymn, RiverHymn, RiverChordsHymn, start * bar, bar, spb);
        }

        // the river: the trumpets sing the long, soaring tune in octaves, and the sopranos sing it too
        Action<double, double, int, Chord, int> trumpet = (t, d, m, c, b) => s.Trumpet(t, d * 0.95, m, 0.2, 0.25, 1.0, 0.12);
        Action<double, double, int, Chord, int> trumpetLow = (t, d, m, c, b) => s.Trumpet(t, d * 0.95, Below(m, c), 0.12, -0.3, 0.9);
        Action<double, double, int, Chord, int> riverVoices = (t, d, m, c, b) => s.Choir(t, d * 0.97, m, 0.14, 0.1, 0);
        PlayTune(trumpet, RiverSong, RiverChordsRiver, R1 * bar, bar, spb);
        PlayTune(trumpetLow, RiverSong, RiverChordsRiver, R1 * bar, bar, spb);
        PlayTune(riverVoices, RiverSong, RiverChordsRiver, R1 * bar, bar, spb);

        // the fury: the hymn sung high, a fifth over, by the whole choir and by the trumpets in octaves, over the sixteenths of the piano and the drums that gallop
        Action<double, double, int, Chord, int> highVoices = (t, d, m, c, b) => { s.Choir(t, d * 0.97, m + 12, 0.2, 0.3, 0); s.Choir(t, d * 0.97, m, 0.18, -0.1, 0); };
        Action<double, double, int, Chord, int> highTrumpets = (t, d, m, c, b) => { s.Trumpet(t, d * 0.9, m + 12, 0.14, 0.4, 1.0, 0.15); s.Trumpet(t, d * 0.9, m, 0.14, -0.4, 1.0, 0.15); };
        PlayTune(highVoices, RiverSong, MoonChordsFury, F1 * bar, bar, spb);
        PlayTune(highTrumpets, RiverSong, MoonChordsFury, F1 * bar, bar, spb);

        s.Reverb(0.5, 0.89);
        return s.FinishLoop(bars * bar, 0.19);
    }
}
