using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The theme of the last stage (the river of stars, the realm of Killa, the Moon), made first for the last boss: it keeps the name of the slot of the boss in the texts ("A lua prateada reflete sob o lago" is the title of the fight with Killa) while it plays in the stage, whose title is "O rio que percorre o universo". It takes its character from the theme that inspired it ("Pure Furies ~ Whereabouts of the Heart"): the same key (G# minor), the
/// tempo (132 beats a minute, slower than the 176 of that theme), the same wall of sound that never lets go, a driving beat (kick on the first and third beats, snare on the second and fourth, eighth-note hats, a kick on the last sixteenth to
/// push the bar), a galloping pulse of sixteenths underneath, and the pattern of the arrangement: the verse, the verse again higher, a hymn that soars, a bridge that builds, the hymn again at full strength. But it is
/// played by the instruments of the last stage, in the divine manner: a CHOIR (pads that hold the chords, then the hymn in five parts), the ORGAN (the pedal, the hammered chords and the gallop of its pipes) and the SAXOPHONE
/// (the tune of the verse and the cries over the hymn); the drums are the only others.
///
/// Sixty bars: the introduction (4), the verse (8), the verse again (8), the hymn (8), the bridge (8), the hymn in fury (8), the verse again with the whole choir (8), the last hymn (8).
/// </summary>
public static partial class Songs
{
	private static Chord[] SilverIntro => new[] { Gsm, Fs, E, Fs };
	private static Chord[] SilverVerse => new[] { Gsm, Gsm, Fs, Fs, E, E, Fs, Dsm };
	private static Chord[] SilverVerseTwo => new[] { Gsm, Gsm, Fs, Fs, E, Fs, Bn, Dsm };
	private static Chord[] SilverHymnChords => new[] { E, Fs, Gsm, Gsm, E, Fs, Bn, Dsm };
	private static Chord[] SilverBridgeChords => new[] { Csm, Gsm, E, Bn, Csm, Gsm, Fs, Fs };

	// The tune of the verse, for the saxophone (G# minor: G#5 = 80, F#5 = 78, E5 = 76, D#5 = 75, C#5 = 73, B4 = 71, G#4 = 68).
	private static readonly Note[][] SilverTune =
	{
		new[] { new Note(0, 80, 1.5), new Note(1.5, 78, 0.5), new Note(2, 76, 1), new Note(3, 75, 1) },
		new[] { new Note(0, 76, 1.5), new Note(1.5, 75, 0.5), new Note(2, 71, 2) },
		new[] { new Note(0, 78, 1.5), new Note(1.5, 76, 0.5), new Note(2, 73, 1), new Note(3, 76, 1) },
		new[] { new Note(0, 78, 2), new Note(2, 82, 1), new Note(3, 80, 1) },
		new[] { new Note(0, 76, 1.5), new Note(1.5, 80, 0.5), new Note(2, 83, 1), new Note(3, 80, 1) },
		new[] { new Note(0, 76, 1), new Note(1, 80, 1), new Note(2, 76, 1), new Note(3, 71, 1) },
		new[] { new Note(0, 78, 1.5), new Note(1.5, 73, 0.5), new Note(2, 78, 1), new Note(3, 82, 1) },
		new[] { new Note(0, 80, 2), new Note(2, 78, 1), new Note(3, 75, 1) },
	};
	// The verse again: the same tune with the notes of the second half higher and busier.
	private static readonly Note[][] SilverTuneTwo =
	{
		new[] { new Note(0, 80, 1.5), new Note(1.5, 83, 0.5), new Note(2, 80, 1), new Note(3, 78, 1) },
		new[] { new Note(0, 76, 1), new Note(1, 80, 0.5), new Note(1.5, 78, 0.5), new Note(2, 75, 2) },
		new[] { new Note(0, 78, 1.5), new Note(1.5, 81, 0.5), new Note(2, 85, 1), new Note(3, 82, 1) },
		new[] { new Note(0, 78, 1), new Note(1, 82, 1), new Note(2, 85, 2) },
		new[] { new Note(0, 83, 1.5), new Note(1.5, 80, 0.5), new Note(2, 76, 1), new Note(3, 80, 1) },
		new[] { new Note(0, 78, 1), new Note(1, 82, 1), new Note(2, 85, 1), new Note(3, 82, 1) },
		new[] { new Note(0, 83, 1.5), new Note(1.5, 87, 0.5), new Note(2, 83, 1), new Note(3, 78, 1) },
		new[] { new Note(0, 87, 2), new Note(2, 83, 1), new Note(3, 80, 1) },
	};
	// The hymn, for the choir (and the saxophone an octave above it in fury).
	private static readonly Note[][] SilverHymn =
	{
		new[] { new Note(0, 71, 2), new Note(2, 68, 2) },
		new[] { new Note(0, 70, 2), new Note(2, 66, 1), new Note(3, 70, 1) },
		new[] { new Note(0, 68, 2), new Note(2, 71, 1), new Note(3, 73, 1) },
		new[] { new Note(0, 75, 2), new Note(2, 73, 1), new Note(3, 71, 1) },
		new[] { new Note(0, 71, 1.5), new Note(1.5, 68, 0.5), new Note(2, 64, 2) },
		new[] { new Note(0, 70, 1.5), new Note(1.5, 66, 0.5), new Note(2, 70, 1), new Note(3, 73, 1) },
		new[] { new Note(0, 71, 1), new Note(1, 75, 1), new Note(2, 71, 2) },
		new[] { new Note(0, 75, 4) },
	};
	// The bridge: a chant that climbs, for the choir.
	private static readonly Note[][] SilverChant =
	{
		new[] { new Note(0, 73, 1), new Note(1, 71, 1), new Note(2, 68, 2) },
		new[] { new Note(0, 71, 2), new Note(2, 75, 2) },
		new[] { new Note(0, 71, 1), new Note(1, 68, 1), new Note(2, 76, 2) },
		new[] { new Note(0, 75, 2), new Note(2, 78, 2) },
		new[] { new Note(0, 76, 1), new Note(1, 73, 1), new Note(2, 80, 2) },
		new[] { new Note(0, 78, 2), new Note(2, 75, 2) },
		new[] { new Note(0, 78, 1), new Note(1, 82, 1), new Note(2, 85, 2) },
		new[] { new Note(0, 85, 4) },
	};

	/// <summary>The theme of stage 6 (see above for the titles).</summary>
	public static (float[] left, float[] right) SilverMoonOverTheLake()
	{
		const double bpm = 165;
		double spb = 60 / bpm, bar = 4 * spb, st = spb / 4;
		const int V1 = 4, V2 = 12, H1 = 20, BR = 28, F1 = 36, V3 = 44, H2 = 52, bars = 60;
		var chords = new List<Chord>();
		chords.AddRange(SilverIntro); chords.AddRange(SilverVerse); chords.AddRange(SilverVerseTwo); chords.AddRange(SilverHymnChords); chords.AddRange(SilverBridgeChords);
		chords.AddRange(SilverHymnChords); chords.AddRange(SilverVerse); chords.AddRange(SilverHymnChords);
		var s = new Synth(bars * bar + 8);

		for (int b = 0; b < bars; b++)
		{
			var ch = chords[b];
			double t0 = b * bar;
			bool intro = b < V1, verse = (b >= V1 && b < H1) || (b >= V3 && b < H2), hymn = (b >= H1 && b < BR) || b >= H2, bridge = b >= BR && b < F1, fury = b >= F1 && b < V3;
			bool lastOfPart = b + 1 == V1 || b + 1 == V2 || b + 1 == H1 || b + 1 == BR || b + 1 == F1 || b + 1 == V3 || b + 1 == H2 || b + 1 == bars;
			bool full = hymn || fury || bridge || (b >= V3 && b < H2);

			// the organ: the pedal under everything, its octave, and the fifth
			s.Organ(t0, bar * 0.99, ch.Bass, 0.13, -0.2);
			s.Organ(t0, bar * 0.99, ch.Bass + 12, 0.1, 0.2);
			if (!intro) s.Organ(t0, bar * 0.99, ch.Bass + 19, 0.05, 0.0);

			// the gallop of the pipes: sixteenths in the pattern of a run of horses, an octave up on the offbeats, hammered chords on the beats of the hymn and the fury
			if (verse || fury || hymn)
			{
				int[] gallop = { 0, 1, 3, 4, 6, 8, 9, 11, 12, 14 };
				foreach (int k in gallop)
				{
					bool accent = k % 4 == 0;
					s.OrganStab(t0 + k * st, st * 0.8, ch.Bass + 12 + (k == 6 || k == 14 ? 12 : 0), accent ? 0.09 : 0.06, -0.3 + (k % 4) * 0.2);
				}
				if (hymn || fury)
					for (int i = 0; i < 4; i++) for (int j = 1; j < ch.Tones.Length; j++) s.OrganStab(t0 + i * spb, spb * 0.45, ch.Tones[j], 0.035, -0.4 + j * 0.25);
			}
			if (bridge) for (int i = 0; i < 8; i++) s.OrganStab(t0 + i * spb / 2, spb * 0.3, ch.Bass + 12, i % 2 == 0 ? 0.08 : 0.055, -0.2 + (i % 4) * 0.13);
			if (intro) for (int i = 0; i < 4; i++) s.OrganStab(t0 + i * spb, spb * 0.5, ch.Bass + 12, 0.07, 0);

			// the drums: the kick on the first and third beats and on the last sixteenth of the bar, the snare on the second and fourth, eighth-note hats, the timpani under the hymn and the fury; a roll into every part
			if (intro)
			{
				for (int i = 0; i < 8; i++) s.Taiko(t0 + i * bar / 8, 45 - (i % 4), 0.1 + 0.03 * (b * 8 + i) / 8.0, -0.3 + (i % 4) * 0.2);
			}
			else
			{
				for (int i = 0; i < 4; i++)
				{
					if (i % 2 == 0) s.Kick(t0 + i * spb, full ? 0.3 : 0.25);
					else s.Snare(t0 + i * spb, full ? 0.22 : 0.18);
					s.Hat(t0 + i * spb, 0.05); s.Hat(t0 + (i + 0.5) * spb, 0.04, 0.25, i == 3);
					if (fury || hymn) s.Taiko(t0 + i * spb, 33, 0.2, -0.2);
				}
				s.Kick(t0 + 3.75 * spb, 0.16);                                          // the push of the bar
				if (verse && b % 2 == 1) s.Kick(t0 + 1.5 * spb, 0.12);
				if (fury) { s.Kick(t0 + 1.5 * spb, 0.14); s.Kick(t0 + 2.5 * spb, 0.12); for (int i = 0; i < 4; i++) { s.Hat(t0 + (i + 0.25) * spb, 0.03); s.Hat(t0 + (i + 0.75) * spb, 0.03); } }
				if (b == V1 || b == V2 || b == H1 || b == BR || b == F1 || b == V3 || b == H2) s.Crash(t0, 0.24);
			}
			if (lastOfPart && !intro) for (int i = 0; i < 16; i++) s.Tom(t0 + (2 + i / 8.0) * spb, 45 - (i % 6), 0.08 + 0.016 * i, -0.4 + (i % 6) * 0.16);

			// the choir: in the introduction a great "ah" that swells; in the verse the chord held in three voices; in the bridge and the fury, the whole choir
			if (intro)
			{
				s.Choir(t0, bar, ch.Bass + 12, 0.12, -0.4, 1); s.Choir(t0, bar, ch.Tones[1] + 12, 0.1, -0.15, 0); s.Choir(t0, bar, ch.Tones[2] + 12, 0.1, 0.15, 0); s.Choir(t0, bar, ch.Tones[3] + 12, 0.1, 0.4, 0);
			}
			if (verse)
			{
				s.Choir(t0, bar * 0.99, ch.Tones[1], 0.08, -0.4, 1); s.Choir(t0, bar * 0.99, ch.Tones[2] + 12, 0.07, 0.3, 0); s.Choir(t0, bar * 0.99, ch.Tones[3] + 12, 0.06, 0.5, 0);
				if (b >= V3) { s.Choir(t0, bar * 0.99, ch.Bass + 12, 0.1, -0.6, 1); s.Choir(t0, bar * 0.99, ch.Tones[2], 0.08, 0.1, 0); }
			}
			if (hymn)
			{
				s.Choir(t0, bar * 0.99, ch.Bass, 0.14, -0.6, 0);
				s.Choir(t0, bar * 0.99, ch.Bass + 12, 0.1, -0.4, 1);
				s.Choir(t0, bar * 0.99, ch.Tones[1], 0.09, -0.2, 1);
			}
			if (bridge)
			{
				s.Choir(t0, bar * 0.99, ch.Tones[1], 0.1, -0.3, 1); s.Choir(t0, bar * 0.99, ch.Tones[2] + 12, 0.09, 0.3, 0); s.Choir(t0, bar * 0.99, ch.Bass + 12, 0.1, -0.5, 0);
			}
			if (fury)
				foreach (var (m, pan, vowel) in new[] { (ch.Bass, -0.6, 0), (ch.Bass + 12, -0.4, 1), (ch.Tones[1], -0.2, 1), (ch.Tones[2], 0.0, 0), (ch.Tones[2] + 12, 0.2, 0), (ch.Tones[3] + 12, 0.4, 0) })
					s.Choir(t0, bar * 0.99, m, 0.08, pan, vowel);
		}

		// the opening: a bell tolls over the timpani and the saxophone calls from afar
		foreach (var (beat, midi) in new[] { (0.0, 68), (8.0, 68), (12.0, 75) }) s.Bell(beat * spb, midi + 12, 0.16, 0.1);
		s.Sax(2 * bar, 1.8 * spb, 80, 0.17, 0.2, 0.05); s.Sax(2 * bar + 2 * spb, 1.8 * spb, 78, 0.17, 0.2, 0.05);
		s.Sax(3 * bar, 4 * spb, 76, 0.18, 0.2, 0.08);

		// the verse: the saxophone sings the tune, the first time with a low shadow under it, the second time higher; the choir answers it on the open beats
		Action<double, double, int, Chord, int> sax = (t, d, m, c, b) => s.Sax(t, d * 0.93, m, 0.22, 0.25, d > 1.2 * spb ? 0.1 : 0.0);
		Action<double, double, int, Chord, int> shadow = (t, d, m, c, b) => s.Sax(t, d * 0.9, Below(m, c) - 5, 0.11, -0.25, 0, 0.8);
		Action<double, double, int, Chord, int> echo = (t, d, m, c, b) => { if (b % 2 == 1 && d >= spb) s.Choir(t + d * 0.5, d * 1.4, m - 12, 0.1, 0.4, 0); };
		PlayTune(sax, SilverTune, SilverVerse, V1 * bar, bar, spb);
		PlayTune(shadow, SilverTune, SilverVerse, V1 * bar, bar, spb);
		PlayTune(echo, SilverTune, SilverVerse, V1 * bar, bar, spb);
		PlayTune(sax, SilverTuneTwo, SilverVerseTwo, V2 * bar, bar, spb);
		PlayTune(shadow, SilverTuneTwo, SilverVerseTwo, V2 * bar, bar, spb);
		PlayTune(echo, SilverTuneTwo, SilverVerseTwo, V2 * bar, bar, spb);

		// the hymn: the sopranos sing it with the organ in octaves, the altos a third below, the saxophone an octave under the sopranos; the last time the whole choir sings it and the saxophone cries over it
		Action<double, double, int, Chord, int> sopranos = (t, d, m, c, b) => s.Choir(t, d * 0.97, m, 0.24, 0.3, 0);
		Action<double, double, int, Chord, int> sopranosHigh = (t, d, m, c, b) => s.Choir(t, d * 0.97, m + 12, 0.09, 0.1, 0);
		Action<double, double, int, Chord, int> altos = (t, d, m, c, b) => s.Choir(t, d * 0.97, Below(m, c), 0.17, -0.3, 1);
		Action<double, double, int, Chord, int> organLead = (t, d, m, c, b) => { s.Organ(t, d * 0.97, m - 12, 0.09, 0.1); s.Organ(t, d * 0.97, m, 0.07, 0.2); };
		Action<double, double, int, Chord, int> saxHymn = (t, d, m, c, b) => s.Sax(t, d * 0.95, m, 0.17, 0.35, 0.05);
		Action<double, double, int, Chord, int> saxCry = (t, d, m, c, b) => { if (d >= spb) s.Sax(t, d * 0.95, m + 12, 0.15, -0.35, 0.18, 1.1); };
		foreach (int start in new[] { H1, F1, H2 })
		{
			var hymnChords = SilverHymnChords;
			PlayTune(sopranos, SilverHymn, hymnChords, start * bar, bar, spb);
			PlayTune(altos, SilverHymn, hymnChords, start * bar, bar, spb);
			PlayTune(organLead, SilverHymn, hymnChords, start * bar, bar, spb);
			PlayTune(saxHymn, SilverHymn, hymnChords, start * bar, bar, spb);
			if (start != H1) { PlayTune(sopranosHigh, SilverHymn, hymnChords, start * bar, bar, spb); PlayTune(saxCry, SilverHymn, hymnChords, start * bar, bar, spb); }
		}

		// the bridge: the chant climbs in the choir (women and men an octave apart), over the saxophone's held notes
		Action<double, double, int, Chord, int> women = (t, d, m, c, b) => s.Choir(t, d * 0.97, m + 12, 0.17, 0.2, 0);
		Action<double, double, int, Chord, int> men = (t, d, m, c, b) => s.Choir(t, d * 0.97, m, 0.16, -0.2, 1);
		Action<double, double, int, Chord, int> held = (t, d, m, c, b) => { if (d >= 2 * spb) s.Sax(t, d * 0.95, m, 0.16, 0.3, 0.1, 0.9); };
		PlayTune(women, SilverChant, SilverBridgeChords, BR * bar, bar, spb);
		PlayTune(men, SilverChant, SilverBridgeChords, BR * bar, bar, spb);
		PlayTune(held, SilverChant, SilverBridgeChords, BR * bar, bar, spb);

		// the verse in its last form: the tune in the saxophone and, over it, the hymn in the choir
		PlayTune(sax, SilverTuneTwo, SilverVerse, V3 * bar, bar, spb);
		PlayTune(shadow, SilverTuneTwo, SilverVerse, V3 * bar, bar, spb);
		Action<double, double, int, Chord, int> voices = (t, d, m, c, b) => { if (d >= 1.5 * spb) s.Choir(t, d * 0.95, m - 12, 0.12, -0.4, 1); };
		PlayTune(voices, SilverTuneTwo, SilverVerse, V3 * bar, bar, spb);

		s.Reverb(0.5, 0.89);
		return s.FinishLoop(bars * bar, 0.19);
	}
}
