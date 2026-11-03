using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The music of stage 5, the waterfall of the seven colours and its guardian, with a divine air.
///
/// "A grande serpe sob a montanha" (148 BPM, B minor) is the theme of the stage: the great two-headed serpent. WIND instruments lead, flutes, trumpets and saxophones (a bass and a few drums and cymbals only
/// support them): a riff of saxophones and trumpets that slithers by half steps, hard drums, trills and runs of flutes, a divine chorus of trumpets and flutes that climbs to the top, and a break where a
/// sax solo twists among trilling flutes.
/// "Hino ao guardião dos dois reinos" (the boss, 144 BPM, D major and D Dorian) is made of TRUMPETS and CHOIRS, in the manner of the music of the Catholic church: a tubular bell and an organ open it, men and
/// women sing a plainchant in unison (D Dorian) with trumpets answering from the other side, then a hymn in four parts with the trumpets above the choir, then a Gloria of heavy held blocks of brass over high held
/// chords of the choir, the chant again with everything, the hymn, and an Amen (a plagal cadence) before it begins again. Timpani and a cymbal mark the great beats.
/// </summary>
public static partial class Songs
{
	private static readonly Chord DK = new(38, 50, 54, 57, 62), FS7K = new(42, 54, 58, 61, 64);
	private const double Tr = 1.0 / 3;      // a third of a beat: (from the old Gloria of trumpet triplets; no longer used)

	// MARK: The boss

	private static readonly Note[][] SerpentA =
	{
		new[] { new Note(0, 71, 0.5), new Note(0.5, 73, 0.5), new Note(1, 74, 0.5), new Note(1.5, 73, 0.5), new Note(2, 71, 0.5), new Note(2.5, 70, 0.5), new Note(3, 71, 1) },
		new[] { new Note(0, 71, 0.5), new Note(0.5, 74, 0.5), new Note(1, 78, 1), new Note(2, 77, 0.5), new Note(2.5, 78, 0.5), new Note(3, 74, 1) },
		new[] { new Note(0, 79, 0.5), new Note(0.5, 78, 0.5), new Note(1, 74, 0.5), new Note(1.5, 71, 0.5), new Note(2, 67, 1), new Note(3, 71, 1) },
		new[] { new Note(0, 73, 0.5), new Note(0.5, 76, 0.5), new Note(1, 81, 1), new Note(2, 80, 0.5), new Note(2.5, 81, 0.5), new Note(3, 76, 1) },
		new[] { new Note(0, 71, 0.5), new Note(0.5, 73, 0.5), new Note(1, 74, 0.5), new Note(1.5, 73, 0.5), new Note(2, 71, 0.5), new Note(2.5, 70, 0.5), new Note(3, 78, 1) },
		new[] { new Note(0, 79, 0.5), new Note(0.5, 83, 0.5), new Note(1, 86, 1), new Note(2, 83, 0.5), new Note(2.5, 79, 0.5), new Note(3, 74, 1) },
		new[] { new Note(0, 78, 0.5), new Note(0.5, 77, 0.5), new Note(1, 78, 0.5), new Note(1.5, 82, 0.5), new Note(2, 85, 1), new Note(3, 82, 1) },
		new[] { new Note(0, 85, 1), new Note(1, 82, 0.5), new Note(1.5, 78, 0.5), new Note(2, 76, 1), new Note(3, 70, 1) },
	};
	private static readonly Note[][] SerpentB =
	{
		new[] { new Note(0, 86, 2), new Note(2, 83, 1), new Note(3, 86, 1) },
		new[] { new Note(0, 90, 2), new Note(2, 86, 1), new Note(3, 81, 1) },
		new[] { new Note(0, 85, 2), new Note(2, 88, 1), new Note(3, 85, 1) },
		new[] { new Note(0, 83, 2), new Note(2, 86, 1), new Note(3, 83, 1) },
		new[] { new Note(0, 86, 1), new Note(1, 83, 1), new Note(2, 79, 2) },
		new[] { new Note(0, 81, 1), new Note(1, 86, 1), new Note(2, 90, 2) },
		new[] { new Note(0, 90, 1), new Note(1, 88, 1), new Note(2, 85, 1), new Note(3, 82, 1) },
		new[] { new Note(0, 85, 2), new Note(2, 82, 1), new Note(3, 85, 1) },
	};

	private static Chord[] SerpentChordsIntro => new[] { BmN, BmN, BmN, FS7K };
	private static Chord[] SerpentChordsA => new[] { BmN, BmN, GB, A, BmN, GB, FS7K, FS7K };
	private static Chord[] SerpentChordsB => new[] { GB, DK, A, BmN, GB, DK, FS7K, FS7K };
	private static Chord[] SerpentChordsBreak => new[] { BmN, BmN, BmN, BmN, GB, A, FS7K, FS7K };

	/// <summary>"A grande serpe sob a montanha": the theme of stage 5 (it was the boss's, and the stage's own song was taken away).</summary>
	public static (float[] left, float[] right) SerpentUnderTheMountain()
	{
		const double bpm = 148;
		double spb = 60 / bpm, bar = 4 * spb;
		var chords = new List<Chord>();
		chords.AddRange(SerpentChordsIntro); chords.AddRange(SerpentChordsA); chords.AddRange(SerpentChordsB); chords.AddRange(SerpentChordsBreak); chords.AddRange(SerpentChordsA); chords.AddRange(SerpentChordsB);
		int bars = chords.Count; // 44
		var s = new Synth(bars * bar + 5);
		const int A1 = 4, B1 = 12, K1 = 20, A2 = 28, B2 = 36;
		int[] tresillo = { 0, 1, 3, 4, 6, 7 };

		for (int b = 0; b < bars; b++)
		{
			var ch = chords[b];
			double t0 = b * bar;
			bool intro = b < A1, chorus = (b >= B1 && b < K1) || b >= B2, brk = b >= K1 && b < A2;

			// the foundation: a bass in the 3 + 3 + 2 pattern (held in the break), and the drums, hard but behind the winds
			if (b >= 2)
			{
				if (brk) { s.Bass(t0, 1.9 * spb, ch.Bass, 0.3, true); s.Bass(t0 + 2 * spb, 1.9 * spb, ch.Bass + (b % 2 == 0 ? 7 : 0), 0.26, true); }
				else foreach (int i in tresillo) s.Bass(t0 + i * spb / 2, spb * 0.42, ch.Bass + (i == 7 ? 12 : 0), i % 2 == 0 ? 0.28 : 0.2, true);
			}
			if (b < 2) { s.Tom(t0, 38, 0.3, -0.2); s.Tom(t0 + 2 * spb, 38, 0.26, 0.2); }
			else if (b == 2 || b == 3)
			{
				for (int i = 0; i < 24; i++) s.Snare(t0 + i * spb / 6, 0.05 + 0.13 * ((b - 2) * 24 + i) / 48.0, 0.1);
				if (b == 3) { s.Tom(t0 + 3 * spb, 50, 0.3, -0.4); s.Tom(t0 + 3.25 * spb, 47, 0.3, -0.1); s.Tom(t0 + 3.5 * spb, 45, 0.32, 0.2); s.Tom(t0 + 3.75 * spb, 40, 0.34, 0.4); }
			}
			else
			{
				bool section = b == A1 || b == B1 || b == K1 || b == A2 || b == B2;
				if (section) s.Crash(t0, 0.24);
				if (brk)
				{
					s.Kick(t0, 0.42); s.Kick(t0 + 1.5 * spb, 0.32); s.Snare(t0 + 2 * spb, 0.26);
					for (int i = 0; i < 8; i++) s.Hat(t0 + i * spb / 2, 0.045, 0.3);
					s.Tom(t0 + 3 * spb, 45, 0.18, -0.3); s.Tom(t0 + 3.5 * spb, 43, 0.18, 0.3);
				}
				else
				{
					foreach (var k in new[] { 0.0, 0.75, 1.5, 2.0, 2.75, 3.5 }) s.Kick(t0 + k * spb, k % 1 == 0 ? 0.44 : 0.34);
					s.Snare(t0 + spb, 0.26); s.Snare(t0 + 3 * spb, 0.28);
					for (int i = 0; i < 16; i++) s.Hat(t0 + i * spb / 4, i % 4 == 0 ? 0.07 : (i % 2 == 0 ? 0.045 : 0.03), 0.3, i == 14 && b % 2 == 0);
					if (chorus) { s.Crash(t0 + 2 * spb, 0.12); s.Tom(t0, 36, 0.2, -0.2); }
				}
				bool intoPart = b + 1 == B1 || b + 1 == K1 || b + 1 == A2 || b + 1 == B2;
				if (intoPart)
				{
					for (int i = 0; i < 8; i++) s.Snare(t0 + (2 + i * 0.25) * spb, 0.16 + 0.03 * i, 0.1);
					s.Tom(t0 + 3 * spb, 50, 0.3, -0.4); s.Tom(t0 + 3.25 * spb, 47, 0.3, -0.1); s.Tom(t0 + 3.5 * spb, 45, 0.32, 0.2); s.Tom(t0 + 3.75 * spb, 40, 0.34, 0.4);
				}
			}

			// the flutes: sixteenth-note arpeggios that rise like sparks, in the chorus and the break (trills in the break)
			if (chorus)
				for (int i = 0; i < 16; i++) s.Flute(t0 + i * spb / 4, spb * 0.24, ch.Tones[(i * 3) % 4] + 24, 0.055 + (i % 4 == 0 ? 0.015 : 0), 0.7 - (i % 4) * 0.45);
			if (brk)
				for (int i = 0; i < 16; i++) s.Flute(t0 + i * spb / 4, spb * 0.22, ch.Tones[1 + (i % 2)] + 24, 0.05, 0.6 - (i % 2) * 1.2);       // the trill
			// saxes and trumpets hold the chord under the chorus
			if (chorus) for (int i = 1; i < ch.Tones.Length; i++) { s.Sax(t0, bar * 0.97, ch.Tones[i], 0.05, -0.4 + i * 0.25, 0.1, 0.8); s.Trumpet(t0, bar * 0.97, ch.Tones[i], 0.035, 0.3 - i * 0.15, 0.6); }
		}

		// the opening: a roll of drums, and the serpent's call in the saxophone, rising, with trumpets
		s.Crash(0, 0.26);
		foreach (var (beat, midi, len) in new[] { (0.0, 59, 1.0), (1.0, 62, 1.0), (2.0, 66, 1.0), (3.0, 71, 1.0) })
		{
			s.Sax(2 * bar + beat * spb, len * spb * 0.9, midi, 0.17, -0.2, 0.6, 1.1);
			s.Trumpet(2 * bar + beat * spb, len * spb * 0.9, midi + 12, 0.08, 0.2, 1.0);
		}
		foreach (var (beat, midi, len) in new[] { (0.0, 73, 0.5), (0.5, 74, 0.5), (1.0, 78, 0.5), (1.5, 81, 0.5), (2.0, 83, 2.0) })
		{
			s.Trumpet(3 * bar + beat * spb, len * spb * 0.9, midi, 0.17, 0.1, 1.2);
			s.Sax(3 * bar + beat * spb, len * spb * 0.9, midi - 12, 0.15, -0.1, 0.6);
		}

		// the riff: saxophones and trumpets in unison (the sax an octave down, growling), a flute above it in the repeat; in the chorus the trumpets soar an octave below the flutes
		Action<double, double, int, Chord, int> sax = (t, d, m, c, b) => s.Sax(t, d * 0.84, m - 12, 0.21, -0.2, 0.6, 1.15);
		Action<double, double, int, Chord, int> trumpet = (t, d, m, c, b) => s.Trumpet(t, d * 0.82, m, 0.17, 0.2, 1.25);
		Action<double, double, int, Chord, int> fluteUp = (t, d, m, c, b) => s.Flute(t, d * 0.9, m + 12, 0.09, 0.4);
		Action<double, double, int, Chord, int> harm = (t, d, m, c, b) => s.Trumpet(t, d * 0.8, Below(m, c), 0.11, 0.4, 1.0);
		Action<double, double, int, Chord, int> trumpetLead = (t, d, m, c, b) => s.Trumpet(t, d * 0.9, m - 12, 0.21, 0.2, 1.1);
		Action<double, double, int, Chord, int> fluteLead = (t, d, m, c, b) => s.Flute(t, d * 0.97, m, 0.17, 0.0);
		Action<double, double, int, Chord, int> saxChorus = (t, d, m, c, b) => s.Sax(t, d * 0.95, Below(m - 12, c), 0.12, -0.4, 0.3, 1.0);
		PlayTune(sax, SerpentA, SerpentChordsA, A1 * bar, bar, spb);
		PlayTune(trumpet, SerpentA, SerpentChordsA, A1 * bar, bar, spb);
		PlayTune(sax, SerpentA, SerpentChordsA, A2 * bar, bar, spb);
		PlayTune(trumpet, SerpentA, SerpentChordsA, A2 * bar, bar, spb);
		PlayTune(fluteUp, SerpentA, SerpentChordsA, A2 * bar, bar, spb);
		PlayTune(harm, SerpentA, SerpentChordsA, A2 * bar, bar, spb);
		foreach (int start in new[] { B1, B2 })
		{
			PlayTune(trumpetLead, SerpentB, SerpentChordsB, start * bar, bar, spb);
			PlayTune(fluteLead, SerpentB, SerpentChordsB, start * bar, bar, spb);
			PlayTune(saxChorus, SerpentB, SerpentChordsB, start * bar, bar, spb);
			if (start == B2) PlayTune(harm, SerpentB, SerpentChordsB, start * bar, bar, spb);
		}

		// the break: a tenor sax twists through B harmonic minor, the shapes growing wilder; the trumpets answer every second bar
		int[] scale = { 0, 2, 3, 5, 7, 8, 11 };                                     // B C# D E F# G A#
		int Degree(int index) => 59 + scale[((index % 7) + 7) % 7] + 12 * (int)Math.Floor(index / 7.0);
		int[][] shapes =
		{
			new[] { 0, 2, 4, 3, 5, 4, 2, 1 }, new[] { 5, 4, 6, 5, 7, 6, 4, 5 }, new[] { 7, 6, 8, 7, 9, 8, 6, 7 }, new[] { 9, 8, 7, 6, 5, 4, 3, 2 },
			new[] { 4, 5, 7, 6, 8, 7, 9, 8 }, new[] { 9, 10, 8, 9, 7, 8, 6, 7 }, new[] { 6, 8, 10, 9, 11, 10, 8, 9 }, new[] { 11, 9, 10, 8, 9, 7, 8, 6 },
		};
		for (int b = 0; b < 8; b++)
			for (int i = 0; i < 8; i++)
				s.Sax((K1 + b) * bar + i * spb / 2, spb * 0.46, Degree(shapes[b][i] + 3), 0.2 + (i % 4 == 0 ? 0.02 : 0), 0.1, 0.5 + b * 0.07, 1.25);
		for (int b = 1; b < 8; b += 2)
			foreach (var t in new[] { 2.0, 2.5, 3.0, 3.5 }) s.Trumpet((K1 + b) * bar + t * spb, spb * 0.4, chords[K1 + b].Tones[2] + 12, 0.12, 0.3, 1.2);

		s.Reverb(0.22, 0.78);
		return s.FinishLoop(bars * bar, 0.22);
	}

	// MARK: The boss

	// the plainchant (D Dorian), sung in unison; the hymn in D major; the Gloria in held blocks
	private static readonly Note[][] ChantA =
	{
		new[] { new Note(0, 62, 2), new Note(2, 65, 1), new Note(3, 64, 1) },
		new[] { new Note(0, 60, 2), new Note(2, 64, 2) },
		new[] { new Note(0, 65, 1), new Note(1, 67, 1), new Note(2, 69, 2) },
		new[] { new Note(0, 67, 2), new Note(2, 62, 2) },
		new[] { new Note(0, 62, 1), new Note(1, 65, 1), new Note(2, 69, 2) },
		new[] { new Note(0, 72, 2), new Note(2, 69, 1), new Note(3, 65, 1) },
		new[] { new Note(0, 67, 2), new Note(2, 64, 1), new Note(3, 67, 1) },
		new[] { new Note(0, 62, 4) },
	};
	private static readonly Note[][] HymnB =
	{
		new[] { new Note(0, 74, 2), new Note(2, 78, 1), new Note(3, 81, 1) },
		new[] { new Note(0, 81, 2), new Note(2, 76, 1), new Note(3, 73, 1) },
		new[] { new Note(0, 78, 2), new Note(2, 74, 1), new Note(3, 74, 1) },
		new[] { new Note(0, 79, 2), new Note(2, 83, 1), new Note(3, 79, 1) },
		new[] { new Note(0, 81, 2), new Note(2, 78, 1), new Note(3, 81, 1) },
		new[] { new Note(0, 85, 2), new Note(2, 81, 1), new Note(3, 76, 1) },
		new[] { new Note(0, 79, 1), new Note(1, 83, 1), new Note(2, 86, 2) },
		new[] { new Note(0, 86, 4) },
	};
	private static readonly Note[][] GloriaC =
	{
		new[] { new Note(0, 74, Tr), new Note(Tr, 78, Tr), new Note(2 * Tr, 81, Tr), new Note(1, 83, 2), new Note(3, 81, 1) },
		new[] { new Note(0, 78, Tr), new Note(Tr, 81, Tr), new Note(2 * Tr, 83, Tr), new Note(1, 86, 2), new Note(3, 83, 1) },
		new[] { new Note(0, 79, Tr), new Note(Tr, 83, Tr), new Note(2 * Tr, 86, Tr), new Note(1, 86, 2), new Note(3, 83, 1) },
		new[] { new Note(0, 81, Tr), new Note(Tr, 85, Tr), new Note(2 * Tr, 81, Tr), new Note(1, 85, 1), new Note(2, 85, 2) },
		new[] { new Note(0, 74, Tr), new Note(Tr, 79, Tr), new Note(2 * Tr, 83, Tr), new Note(1, 86, 2), new Note(3, 83, 1) },
		new[] { new Note(0, 78, Tr), new Note(Tr, 81, Tr), new Note(2 * Tr, 86, Tr), new Note(1, 86, 2), new Note(3, 81, 1) },
		new[] { new Note(0, 85, 1), new Note(1, 81, 1), new Note(2, 76, 1), new Note(3, 81, 1) },
		new[] { new Note(0, 85, 4) },
	};

	private static Chord[] HymnChordsIntro => new[] { DK, DK, A, DK };
	private static Chord[] HymnChordsChant => new[] { Dm, C, Dm, GB, Dm, F, C, Dm };
	private static Chord[] HymnChordsHymn => new[] { DK, A, BmN, GB, DK, A, GB, DK };
	private static Chord[] HymnChordsGloria => new[] { GB, DK, EmN, A, GB, DK, A, A };
	private static Chord[] HymnChordsAmen => new[] { GB, DK };

	/// <summary>Plays one long note a bar: the first note of each bar of a tune, held for the whole bar (the heavy, slow way: blocks of brass instead of a quick line).</summary>
	private static void HoldBars(Action<double, double, int, Chord, int> play, Note[][] tune, Chord[] chords, double barStart, double bar)
	{
		for (int b = 0; b < tune.Length; b++) play(barStart + b * bar, bar * 0.98, tune[b][0].Midi, chords[b], b);
	}

	/// <summary>"Hino ao guardião dos dois reinos": the boss's theme of stage 5, trumpets and choirs.</summary>
	public static (float[] left, float[] right) HymnToTheGuardian()
	{
		const double bpm = 288;
		double spb = 60 / bpm, bar = 4 * spb;
		var chords = new List<Chord>();
		chords.AddRange(HymnChordsIntro); chords.AddRange(HymnChordsChant); chords.AddRange(HymnChordsHymn); chords.AddRange(HymnChordsGloria); chords.AddRange(HymnChordsChant);
		chords.AddRange(HymnChordsHymn); chords.AddRange(HymnChordsAmen);
		int bars = chords.Count; // 46
		var s = new Synth(bars * bar + 7);
		const int C1 = 4, H1 = 12, G1 = 20, C2 = 28, H2 = 36, AM = 44;

		for (int b = 0; b < bars; b++)
		{
			var ch = chords[b];
			double t0 = b * bar;
			bool intro = b < C1, chant = (b >= C1 && b < H1) || (b >= C2 && b < H2), hymn = (b >= H1 && b < G1) || (b >= H2 && b < AM), gloria = b >= G1 && b < C2, amen = b >= AM;

			// the organ: the root and the fifth of the chord, held all the bar, as the floor of the church (a drone in the chant)
			s.Organ(t0, bar * 0.99, ch.Bass + 12, intro ? 0.07 : 0.085, -0.2);
			s.Organ(t0, bar * 0.99, ch.Bass + 19, intro ? 0.04 : 0.05, 0.2);
			if (!chant) s.Organ(t0, bar * 0.99, ch.Tones[1], 0.04, 0.0);

			// the choir in four parts, long notes on a vowel: in the hymn, the Gloria and the Amen; in the introduction a soft "oo"
			if (intro) for (int v = 1; v < 3; v++) s.Choir(t0, bar * 1.0, ch.Tones[v] + 12, 0.04 + 0.02 * (b - 1 < 0 ? 0 : 1), v == 1 ? -0.3 : 0.3, 2);
			if (hymn || amen)
			{
				s.Choir(t0, bar * 0.99, ch.Bass, 0.09, -0.5, 0);
				s.Choir(t0, bar * 0.99, ch.Tones[1], 0.07, -0.25, 1);
				s.Choir(t0, bar * 0.99, ch.Tones[2] + 12, 0.07, 0.25, 0);
			}
			if (gloria)
			{
				s.Choir(t0, bar * 0.99, ch.Bass, 0.1, -0.5, 0);
				s.Choir(t0, bar * 0.99, ch.Tones[1] + 12, 0.07, -0.25, 0);
				s.Choir(t0, bar * 0.99, ch.Tones[2] + 12, 0.07, 0.25, 0);
				s.Choir(t0, bar * 0.99, ch.Tones[3] + 24, 0.05, 0.5, 0);               // the sopranos, high and held
			}

			// the drums: timpani on the great beats, a cymbal at each part; rolls into the next part; the Gloria is driven
			if (b >= C1)
			{
				s.Tom(t0, 36, hymn || gloria ? 0.3 : 0.22, -0.2);
				if (hymn || gloria) s.Tom(t0 + 2 * spb, 38, 0.24, 0.2);
				if (gloria)
				{
					// heavy: the great drum on the first and the third, the kick under them; no hats, no quick notes
					for (int k = 0; k < 4; k += 2) s.Taiko(t0 + k * spb, 33, 0.3, k == 0 ? -0.2 : 0.2);        // (a great drum rings for a second: at this speed, one on the first and the third is heavy enough)
					s.Kick(t0, 0.3); s.Kick(t0 + 2 * spb, 0.26); s.Snare(t0 + spb, 0.1); s.Snare(t0 + 3 * spb, 0.12);
				}
				if (b == C1 || b == H1 || b == G1 || b == C2 || b == H2) s.Crash(t0, 0.22);
				bool into = b + 1 == H1 || b + 1 == G1 || b + 1 == C2 || b + 1 == H2 || b + 1 == AM;
				if (into) { for (int i = 0; i < 12; i++) s.Tom(t0 + (2 + i / 6.0) * spb, 45 - (i % 6), 0.12 + 0.025 * i, -0.4 + (i % 6) * 0.16); }
			}
		}

		// the opening: the bell tolls, four times, and the trumpets call from far away
		foreach (var (beat, midi) in new[] { (0.0, 62), (4.0, 62), (8.0, 62), (12.0, 69) }) s.Bell(beat * spb, midi + 12, 0.18, 0.1);
		s.Crash(0, 0.2);
		// (the calls are long, slow blocks of brass, far away and then close, not quick notes)
		s.Trumpet(2 * bar, 4 * spb * 0.97, 62 + 12, 0.15, 0.2, 0.8, 0.2);
		s.Trumpet(3 * bar, 4 * spb * 0.97, 69, 0.18, -0.2, 0.9, 0.3); s.Trumpet(3 * bar, 4 * spb * 0.97, 74, 0.1, 0.2, 0.9);

		// the plainchant: men and women in unison an octave apart ("ah"), with the trumpets answering in the empty beats from the other side of the church (antiphonal)
		Action<double, double, int, Chord, int> women = (t, d, m, c, b) => s.Choir(t, d * 0.97, m + 12, 0.15, 0.2, 0);
		Action<double, double, int, Chord, int> men = (t, d, m, c, b) => s.Choir(t, d * 0.97, m, 0.14, -0.2, 1);
		Action<double, double, int, Chord, int> trumpetAnswer = (t, d, m, c, b) => { if (b % 2 == 1) s.Trumpet(t + 2 * spb, spb * 2.2, m, 0.14, -0.5, 0.7, 0.2); };
		foreach (int start in new[] { C1, C2 })
		{
			PlayTune(women, ChantA, HymnChordsChant, start * bar, bar, spb);
			PlayTune(men, ChantA, HymnChordsChant, start * bar, bar, spb);
			PlayTune(trumpetAnswer, ChantA, HymnChordsChant, start * bar, bar, spb);
		}

		// the hymn: the sopranos sing the tune, the trumpets play it an octave lower and a second trumpet in thirds under them; the choir (above) harmonises
		Action<double, double, int, Chord, int> sopranos = (t, d, m, c, b) => s.Choir(t, d * 0.97, m, 0.16, 0.3, 0);
		Action<double, double, int, Chord, int> trumpet = (t, d, m, c, b) => s.Trumpet(t, d * 0.92, m - 12, 0.18, 0.15, 0.95);
		Action<double, double, int, Chord, int> trumpetHarm = (t, d, m, c, b) => s.Trumpet(t, d * 0.92, Below(m - 12, c), 0.11, -0.3, 0.9);
		Action<double, double, int, Chord, int> trumpetHigh = (t, d, m, c, b) => s.Trumpet(t, d, m, 0.09, 0.4, 0.9, 0.2);
		foreach (int start in new[] { H1, H2 })
		{
			PlayTune(sopranos, HymnB, HymnChordsHymn, start * bar, bar, spb);
			PlayTune(trumpet, HymnB, HymnChordsHymn, start * bar, bar, spb);
			PlayTune(trumpetHarm, HymnB, HymnChordsHymn, start * bar, bar, spb);
			if (start == H2) HoldBars(trumpetHigh, HymnB, HymnChordsHymn, start * bar, bar);
		}

		// the Gloria: no quick notes, but heavy blocks of brass, one to a bar, from the low and the high trumpets together, with the sopranos on the held notes; the great drums carry the weight
		Action<double, double, int, Chord, int> gloriaTrumpet = (t, d, m, c, b) => s.Trumpet(t, d, m - 12, 0.12, 0.15, 1.0, 0.4);
		Action<double, double, int, Chord, int> gloriaSecond = (t, d, m, c, b) => s.Trumpet(t, d, Below(m - 12, c), 0.075, -0.35, 0.9, 0.3);
		Action<double, double, int, Chord, int> gloriaDeep = (t, d, m, c, b) => s.Trumpet(t, d, c.Bass + 12, 0.07, 0.0, 0.8, 0.7);
		Action<double, double, int, Chord, int> gloriaChoir = (t, d, m, c, b) => s.Choir(t, d * 0.99, m, 0.08, 0.4, 0);
		HoldBars(gloriaTrumpet, GloriaC, HymnChordsGloria, G1 * bar, bar);
		HoldBars(gloriaSecond, GloriaC, HymnChordsGloria, G1 * bar, bar);
		HoldBars(gloriaDeep, GloriaC, HymnChordsGloria, G1 * bar, bar);
		HoldBars(gloriaChoir, GloriaC, HymnChordsGloria, G1 * bar, bar);

		// the Amen: a plagal cadence (G to D), trumpets and everything, and the bell
		foreach (var (barIndex, midi) in new[] { (0, 79), (1, 74) })
		{
			s.Trumpet((AM + barIndex) * bar, bar * 0.98, midi, 0.17, 0.2, 1.0);
			s.Trumpet((AM + barIndex) * bar, bar * 0.98, midi - 12, 0.1, -0.2, 0.9);
		}
		s.Bell((AM + 1) * bar, 74, 0.18, 0.1);

		s.Reverb(0.46, 0.88);
		return s.FinishLoop(bars * bar, 0.27);      // a boss's song is louder than a stage's (as the boss songs of stage 1)
	}
}
