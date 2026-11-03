using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The theme of stage 6, the realm of the Moon, with the end of the cosmos and of the divine for its theme: a mighty ORGAN and a piano, choirs singing plainchant, and a few trumpets (the tubular bell tolls now and then).
///
/// "O rio que percorre o universo" (the stage, 288 BPM, A minor and A Dorian): slow and vast. A drone of the organ under everything, the piano pouring stars and a river of arpeggios, the choir chanting in unison
/// (men and women an octave apart), then a hymn in four parts with the organ in full chords, a trumpet answering from afar, and a river of trumpet melody over the flowing piano.
/// </summary>
public static partial class Songs
{
	// MARK: The stage

	private static readonly Note[][] RiverChant =
	{
		new[] { new Note(0, 69, 2), new Note(2, 72, 1), new Note(3, 71, 1) },
		new[] { new Note(0, 69, 1), new Note(1, 67, 1), new Note(2, 69, 2) },
		new[] { new Note(0, 72, 2), new Note(2, 69, 1), new Note(3, 65, 1) },
		new[] { new Note(0, 67, 2), new Note(2, 71, 1), new Note(3, 74, 1) },
		new[] { new Note(0, 72, 2), new Note(2, 76, 1), new Note(3, 74, 1) },
		new[] { new Note(0, 72, 1), new Note(1, 67, 1), new Note(2, 64, 2) },
		new[] { new Note(0, 67, 1), new Note(1, 71, 1), new Note(2, 74, 2) },
		new[] { new Note(0, 69, 4) },
	};
	private static readonly Note[][] RiverHymn =
	{
		new[] { new Note(0, 76, 2), new Note(2, 72, 1), new Note(3, 76, 1) },
		new[] { new Note(0, 77, 2), new Note(2, 72, 1), new Note(3, 69, 1) },
		new[] { new Note(0, 79, 2), new Note(2, 76, 1), new Note(3, 72, 1) },
		new[] { new Note(0, 74, 2), new Note(2, 71, 1), new Note(3, 74, 1) },
		new[] { new Note(0, 77, 2), new Note(2, 74, 1), new Note(3, 77, 1) },
		new[] { new Note(0, 81, 2), new Note(2, 76, 1), new Note(3, 72, 1) },
		new[] { new Note(0, 80, 1), new Note(1, 83, 1), new Note(2, 80, 2) },
		new[] { new Note(0, 81, 4) },
	};
	private static readonly Note[][] RiverSong =
	{
		new[] { new Note(0, 81, 3), new Note(3, 79, 1) },
		new[] { new Note(0, 77, 2), new Note(2, 81, 2) },
		new[] { new Note(0, 79, 2), new Note(2, 84, 2) },
		new[] { new Note(0, 83, 2), new Note(2, 79, 2) },
		new[] { new Note(0, 81, 2), new Note(2, 76, 1), new Note(3, 81, 1) },
		new[] { new Note(0, 84, 2), new Note(2, 81, 2) },
		new[] { new Note(0, 83, 1), new Note(1, 80, 1), new Note(2, 76, 2) },
		new[] { new Note(0, 81, 4) },
	};
	private static Chord[] RiverChordsIntro => new[] { AmB, AmB };
	private static Chord[] RiverChordsChant => new[] { AmB, AmB, F, GB, AmB, C, GB, AmB };
	private static Chord[] RiverChordsHymn => new[] { AmB, F, C, GB, Dm, AmB, E, AmB };
	private static Chord[] RiverChordsRiver => new[] { AmB, F, C, GB, AmB, F, E, AmB };

	/// <summary>"O rio que percorre o universo": the theme of stage 6.</summary>
	public static (float[] left, float[] right) RiverThroughTheUniverse()
	{
		const double bpm = 288;
		double spb = 60 / bpm, bar = 4 * spb;
		var chords = new List<Chord>();
		chords.AddRange(RiverChordsIntro); chords.AddRange(RiverChordsChant); chords.AddRange(RiverChordsHymn); chords.AddRange(RiverChordsRiver); chords.AddRange(RiverChordsHymn);
		int bars = chords.Count; // 34
		var s = new Synth(bars * bar + 7);
		const int C1 = 2, H1 = 10, R1 = 18, H2 = 26;
		int[] order = { 0, 1, 2, 3, 2, 1, 3, 2 };

		for (int b = 0; b < bars; b++)
		{
			var ch = chords[b];
			double t0 = b * bar;
			bool intro = b < C1, chant = b >= C1 && b < H1, hymn = (b >= H1 && b < R1) || b >= H2, river = b >= R1 && b < H2;

			// the organ: a pedal of the root and the fifth, very low, under the whole bar; in the hymn and the river the full chord as well
			s.Organ(t0, bar * 0.99, ch.Bass, intro ? 0.1 : 0.13, -0.2);
			s.Organ(t0, bar * 0.99, ch.Bass + 12, 0.09, 0.2);
			s.Organ(t0, bar * 0.99, ch.Bass + 19, 0.05, 0.0);
			if (hymn || river)
				for (int i = 1; i < ch.Tones.Length; i++) s.Organ(t0, bar * 0.99, ch.Tones[i], 0.045, -0.4 + i * 0.27);

			// the piano: stars (a few high notes, here and there) in the introduction and the chant; in the river, the water itself, arpeggios of eighth notes (sixteenths in the second half) that never stop
			if (intro || chant)
			{
				int[] stars = { 93, 88, 91, 96, 86 };
				s.Piano(t0 + 1.0 * spb, spb * 1.5, stars[b % stars.Length], 0.05, 0.5 - (b % 3) * 0.4, 0);
				s.Piano(t0 + 2.75 * spb, spb * 1.5, stars[(b + 2) % stars.Length], 0.04, -0.5 + (b % 3) * 0.4, 0);
			}
			if (chant) for (int i = 0; i < 4; i++) s.Piano(t0 + i * spb, bar * 0.24, ch.Tones[order[i]], 0.06, 0.1, 0.4);
			if (river)
			{
				int steps = b >= R1 + 4 ? 16 : 8;
				for (int i = 0; i < steps; i++) s.Piano(t0 + i * 4 * spb / steps, spb * (steps == 16 ? 0.7 : 1.3), ch.Tones[order[i % 8]] + 12 + (i % 8 > 4 ? 12 : 0), (i % 4 == 0 ? 0.09 : 0.06), -0.5 + (i % 8) * 0.14, 0.15);
			}
			if (hymn) s.Piano(t0, bar * 0.98, ch.Bass - 12, 0.12, -0.3, 0.7);

			// the choir: "oo" in the introduction; the chant carries it in the chant; the hymn in four parts; the river an "oh" held all the bar
			if (intro) { s.Choir(t0, bar, ch.Tones[1] + 12, 0.05, -0.3, 2); s.Choir(t0, bar, ch.Tones[2] + 12, 0.04, 0.3, 2); }
			if (hymn)
			{
				s.Choir(t0, bar * 0.99, ch.Bass, 0.09, -0.5, 0);
				s.Choir(t0, bar * 0.99, ch.Tones[1], 0.07, -0.25, 1);
				s.Choir(t0, bar * 0.99, ch.Tones[2] + 12, 0.07, 0.25, 0);
			}
			if (river) { s.Choir(t0, bar * 0.99, ch.Tones[2] + 12, 0.06, 0.3, 1); s.Choir(t0, bar * 0.99, ch.Tones[1], 0.06, -0.3, 1); }

			// the timpani: a stroke on the first beat of the hymn and the river, and a roll into each part
			if (hymn || river) s.Tom(t0, 33, hymn ? 0.24 : 0.18, -0.2);
			if (b == H1 || b == R1 || b == H2) s.Crash(t0, 0.18);
			if (b + 1 == H1 || b + 1 == R1 || b + 1 == H2) for (int i = 0; i < 12; i++) s.Tom(t0 + (2 + i / 6.0) * spb, 45 - (i % 6), 0.08 + 0.02 * i, -0.4 + (i % 6) * 0.16);
		}

		// the opening: the bell tolls, slowly, and the first trumpet calls, one long note, from the far side of the cosmos
		foreach (var (beat, midi) in new[] { (0.0, 69), (4.0, 69), (8.0, 76) }) s.Bell(beat * spb, midi + 12, 0.16, 0.1);
		s.Trumpet(0.5 * bar, 3 * spb, 69, 0.11, 0.3, 0.7, 0.1);

		// the plainchant: women and men in unison an octave apart; a trumpet answers in the empty beats, from afar (antiphonal)
		Action<double, double, int, Chord, int> women = (t, d, m, c, b) => s.Choir(t, d * 0.97, m + 12, 0.15, 0.2, 0);
		Action<double, double, int, Chord, int> men = (t, d, m, c, b) => s.Choir(t, d * 0.97, m, 0.14, -0.2, 1);
		Action<double, double, int, Chord, int> answer = (t, d, m, c, b) => { if (b % 2 == 1) s.Trumpet(t + 2 * spb, spb * 1.8, m + 12, 0.1, -0.5, 0.7, 0.1); };
		PlayTune(women, RiverChant, RiverChordsChant, C1 * bar, bar, spb);
		PlayTune(men, RiverChant, RiverChordsChant, C1 * bar, bar, spb);
		PlayTune(answer, RiverChant, RiverChordsChant, C1 * bar, bar, spb);

		// the hymn: the sopranos sing it, the organ doubles it in octaves (the organ is the lead), the trumpets hold the chord under it
		Action<double, double, int, Chord, int> sopranos = (t, d, m, c, b) => s.Choir(t, d * 0.97, m, 0.15, 0.3, 0);
		Action<double, double, int, Chord, int> organLead = (t, d, m, c, b) => { s.Organ(t, d * 0.97, m - 12, 0.085, 0.1); s.Organ(t, d * 0.97, m, 0.06, 0.2); };
		Action<double, double, int, Chord, int> trumpetHymn = (t, d, m, c, b) => s.Trumpet(t, d * 0.9, m - 12, 0.1, 0.2, 0.8, 0.15);
		foreach (int start in new[] { H1, H2 })
		{
			PlayTune(sopranos, RiverHymn, RiverChordsHymn, start * bar, bar, spb);
			PlayTune(organLead, RiverHymn, RiverChordsHymn, start * bar, bar, spb);
			if (start == H2) PlayTune(trumpetHymn, RiverHymn, RiverChordsHymn, start * bar, bar, spb);
		}

		// the river: a trumpet sings a long, soaring melody above the flowing piano, the organ and the choir holding
		Action<double, double, int, Chord, int> trumpet = (t, d, m, c, b) => s.Trumpet(t, d * 0.95, m, 0.17, 0.25, 0.9, 0.1);
		Action<double, double, int, Chord, int> trumpetLow = (t, d, m, c, b) => s.Trumpet(t, d * 0.95, Below(m, c), 0.09, -0.3, 0.8);
		PlayTune(trumpet, RiverSong, RiverChordsRiver, R1 * bar, bar, spb);
		PlayTune(trumpetLow, RiverSong, RiverChordsRiver, R1 * bar, bar, spb);

		s.Reverb(0.5, 0.89);
		return s.FinishLoop(bars * bar, 0.15);
	}
}
