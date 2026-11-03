using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A small offline synthesiser and mixer: it renders notes of a few instruments (brass, flute, plucked harp, string pad, bass,
/// drums) into a stereo buffer, adds a reverb and turns the result into 16-bit audio. The game's music is built with it, so
/// there are no audio files either.
/// </summary>
public sealed class Synth
{
    public const int Rate = 32000;

    public readonly float[] L, R;
    public readonly double Length;

    public Synth(double seconds)
    {
        Length = seconds;
        L = new float[(int)(seconds * Rate)];
        R = new float[(int)(seconds * Rate)];
    }

    public static double Hz(double midi) => 440.0 * Math.Pow(2.0, (midi - 69.0) / 12.0);

    private readonly Random _noise = new Random(1234);
    private double White() => _noise.NextDouble() * 2 - 1;

    // MARK: Generic voice

    /// <summary>Adds a note: `gen(t)` makes the raw tone t seconds after the onset; the envelope and the pan are applied here.</summary>
    public void Voice(double start, double dur, Func<double, double> gen, double amp, double pan = 0, double attack = 0.01, double release = 0.08, double sustain = 1.0, double decay = 0.1)
    {
        int s0 = (int)(start * Rate);
        int n = (int)((dur + release) * Rate);
        double gl = Math.Cos((pan + 1) * Math.PI / 4), gr = Math.Sin((pan + 1) * Math.PI / 4);
        for (int i = 0; i < n; i++)
        {
            int k = s0 + i;
            if (k < 0 || k >= L.Length) continue;
            double t = i / (double)Rate;
            double env;
            if (t < attack) env = t / attack;
            else if (t < dur) env = sustain + (1 - sustain) * Math.Exp(-(t - attack) / decay);
            else env = (attack < dur ? (sustain + (1 - sustain) * Math.Exp(-(dur - attack) / decay)) : t / attack) * Math.Exp(-(t - dur) / (release * 0.35));
            double v = gen(t) * amp * env;
            L[k] += (float)(v * gl);
            R[k] += (float)(v * gr);
        }
    }

    // MARK: Instruments

    /// <summary>A trumpet in the manner of ZUN's: a bright, buzzy brass tone that swells in, scoops up to the pitch and has a late vibrato.</summary>
    public void Trumpet(double start, double dur, double midi, double amp, double pan = 0, double brightness = 1.0, double growl = 0, double bend = 0)
    {
        double f = Hz(midi), phase = 0;
        Voice(start, dur, t =>
        {
            double vib = 1 + 0.007 * Math.Sin(2 * Math.PI * 5.4 * t) * Math.Min(1, Math.Max(0, (t - 0.12) / 0.3));
            double scoop = t < 0.06 ? Math.Pow(2, -0.5 * (1 - t / 0.06) / 12) : 1;
            // a roar: the note falls away by `bend` semitones over its last part, and `growl` buzzes it
            double fall = bend != 0 && t > dur * 0.55 ? Math.Pow(2, -bend * Math.Min(1, (t - dur * 0.55) / (dur * 0.45 + 0.04)) / 12) : 1;
            double fr = f * vib * scoop * fall;
            phase += 2 * Math.PI * fr / Rate;
            double b = (2.5 + 5.5 * Math.Min(1, t / 0.07)) * brightness;
            int nh = (int)Math.Min(16, Rate / 2.2 / fr);
            double s = 0;
            for (int h = 1; h <= nh; h++) s += Math.Pow(h, -0.85) * Math.Exp(-(h - 1) / b) * Math.Sin(h * phase);
            return (s * 0.55 + White() * 0.012 * Math.Min(1, t / 0.05)) * (growl > 0 ? 1 + growl * 0.5 * Math.Sin(2 * Math.PI * 36 * t) : 1);
        }, amp, pan, attack: 0.03, release: 0.09, sustain: 0.9, decay: 0.2);
    }

    /// <summary>
    /// A saxophone (alto or tenor, by the pitch): a reedy, singing tone, rich in harmonics with a bump in the spectrum round 1.5 kHz where a sax is loudest, a breath of noise, a late vibrato, and
    /// (with `growl`) the rough, buzzing edge of a hard-blown note.
    /// </summary>
    public void Sax(double start, double dur, double midi, double amp, double pan = 0, double growl = 0, double brightness = 1.0)
    {
        double f = Hz(midi), phase = 0, lp = 0;
        // the spectrum is worked out once for the note (the vibrato is too small to move it): each partial's weight, with the bump of the sax's bell round 1.5 kHz
        int nh = (int)Math.Min(18, Rate / 2.2 / f);
        var weight = new double[nh + 1];
        for (int h = 1; h <= nh; h++)
        {
            double hz = f * h;
            double formant = 1 + 1.7 * Math.Exp(-Math.Pow((hz - 1500) / 1000, 2));
            weight[h] = Math.Pow(h, -0.72) * Math.Exp(-(h - 1) / (9 * brightness)) * formant * (h % 2 == 1 ? 1 : 0.72);
        }
        Voice(start, dur, t =>
        {
            double vib = 1 + 0.0065 * Math.Sin(2 * Math.PI * 5.6 * t) * Math.Min(1, Math.Max(0, (t - 0.16) / 0.3));
            double fr = f * vib * (t < 0.04 ? Math.Pow(2, -0.3 * (1 - t / 0.04) / 12) : 1);
            phase += 2 * Math.PI * fr / Rate;
            double s = 0;
            for (int h = 1; h <= nh; h++) s += weight[h] * Math.Sin(h * phase);
            lp += (White() - lp) * 0.3;
            double rough = growl > 0 ? 1 + growl * 0.55 * Math.Sin(2 * Math.PI * 41 * t) : 1;
            return (s * 0.34 + lp * 0.05 * Math.Min(1, t / 0.05)) * rough;
        }, amp, pan, attack: 0.028, release: 0.07, sustain: 0.93, decay: 0.25);
    }

    /// <summary>
    /// A piano: a struck string, every partial ringing at its own rate (the high ones die first, the low notes ring longest), slightly stretched in pitch like a real string, with the
    /// knock of the hammer at the start. `dark` (0...1) takes the brightness away for a heavier, lower sound. `dur` is how long the key is held; the damper then stops the string.
    /// </summary>
    public void Piano(double start, double dur, double midi, double amp, double pan = 0, double dark = 0)
    {
        double f = Hz(midi);
        double ring = Math.Max(0.45, 3.4 - (midi - 28) * 0.045);          // seconds the fundamental takes to fade (long in the bass)
        int nh = (int)Math.Min(12, Rate / 2.4 / f);
        // every partial is a little rotating phasor that fades by a fixed factor each sample (no sines or exponentials in the loop)
        var sn = new double[nh + 1]; var cs = new double[nh + 1]; var rs = new double[nh + 1]; var rc = new double[nh + 1]; var level = new double[nh + 1]; var fade = new double[nh + 1];
        for (int h = 1; h <= nh; h++)
        {
            double w = 2 * Math.PI * f * h * Math.Sqrt(1 + 0.00035 * h * h) / Rate;
            rs[h] = Math.Sin(w); rc[h] = Math.Cos(w); cs[h] = 1;
            level[h] = Math.Pow(h, -1.05) * Math.Exp(-dark * (h - 1) * 0.3);
            fade[h] = Math.Exp(-1.0 / (Rate * (ring / (1 + 0.6 * (h - 1)))));
        }
        double knock = (1 - dark * 0.5) * 0.045, knockFade = Math.Exp(-70.0 / Rate);
        Voice(start, dur, t =>
        {
            double s = 0;
            for (int h = 1; h <= nh; h++)
            {
                s += level[h] * sn[h];
                double ns = sn[h] * rc[h] + cs[h] * rs[h];
                cs[h] = cs[h] * rc[h] - sn[h] * rs[h];
                sn[h] = ns;
                level[h] *= fade[h];
            }
            double hit = White() * knock;
            knock *= knockFade;
            return s * 0.55 + hit;
        }, amp, pan, attack: 0.002, release: 0.55, sustain: 1.0, decay: 1.0);
    }

    /// <summary>
    /// A choir voice singing a vowel (0: "ah", 1: "oh", 2: "oo", 3: "eh"): a bright, buzzing source shaped by the formants of that vowel, two voices slightly out of tune with each other (so that one voice
    /// sounds like a few singers), a slow swell, a touch of breath. Every partial is a rotating phasor, and only the strongest are kept, so many voices can be sung at little cost.
    /// </summary>
    public void Choir(double start, double dur, double midi, double amp, double pan = 0, int vowel = 0)
    {
        double f = Hz(midi);
        double[] f1 = { 800, 500, 325, 530 }, f2 = { 1150, 900, 700, 1840 }, f3 = { 2900, 2800, 2700, 2480 };
        int nh = (int)Math.Min(40, Rate / 2.4 / f);
        var picks = new List<(int h, double w)>();
        for (int h = 1; h <= nh; h++)
        {
            double hz = f * h;
            double w = Math.Pow(h, -0.9) * (Math.Exp(-Math.Pow((hz - f1[vowel]) / 140, 2)) + 0.55 * Math.Exp(-Math.Pow((hz - f2[vowel]) / 170, 2)) + 0.22 * Math.Exp(-Math.Pow((hz - f3[vowel]) / 300, 2)) + 0.03);
            picks.Add((h, w));
        }
        var top = picks.OrderByDescending(p => p.w).Take(10).ToArray();
        int n = top.Length;
        // two voices, a few cents apart: each partial has a phasor in each
        var sn = new double[2, n]; var cs = new double[2, n]; var rs = new double[2, n]; var rc = new double[2, n];
        double[] detune = { 0.9972, 1.0028 };
        for (int v = 0; v < 2; v++)
            for (int k = 0; k < n; k++)
            {
                double w = 2 * Math.PI * f * top[k].h * detune[v] / Rate;
                rs[v, k] = Math.Sin(w); rc[v, k] = Math.Cos(w); cs[v, k] = 1; sn[v, k] = 0;
            }
        double lp = 0;
        Voice(start, dur, t =>
        {
            double s = 0;
            for (int v = 0; v < 2; v++)
                for (int k = 0; k < n; k++)
                {
                    s += top[k].w * sn[v, k];
                    double ns = sn[v, k] * rc[v, k] + cs[v, k] * rs[v, k];
                    cs[v, k] = cs[v, k] * rc[v, k] - sn[v, k] * rs[v, k];
                    sn[v, k] = ns;
                }
            lp += (White() - lp) * 0.25;
            double trem = 1 + 0.05 * Math.Sin(2 * Math.PI * 5.0 * t);
            return (s * 0.5 + lp * 0.03 * Math.Min(1, t / 0.2)) * trem;
        }, amp, pan, attack: 0.28, release: 0.4, sustain: 1.0, decay: 1.0);
    }

    /// <summary>A church organ: stops of 16', 8', 4', 2 2/3' and 2' (sines, steady), a gentle chiff at the start.</summary>
    public void Organ(double start, double dur, double midi, double amp, double pan = 0)
    {
        double f = Hz(midi), phase = 0;
        double[] ratio = { 1, 2, 3, 4, 6 }, level = { 1.0, 0.6, 0.35, 0.3, 0.14 };
        Voice(start, dur, t =>
        {
            phase += 2 * Math.PI * f / Rate;
            double s = 0;
            for (int k = 0; k < ratio.Length; k++) s += level[k] * Math.Sin(ratio[k] * phase);
            return (s + White() * 0.03 * Math.Exp(-t * 30)) * 0.4;
        }, amp, pan, attack: 0.05, release: 0.25, sustain: 1.0, decay: 1.0);
    }

    /// <summary>A tubular bell, struck: inharmonic partials that ring a long time.</summary>
    public void Bell(double start, double midi, double amp, double pan = 0)
    {
        double f = Hz(midi);
        double[] ratio = { 1, 2.76, 5.4, 8.93 }, level = { 1.0, 0.55, 0.3, 0.15 }, decay = { 1.0, 1.6, 2.6, 4.0 };
        var ph = new double[ratio.Length];
        Voice(start, 0.05, t =>
        {
            double s = 0;
            for (int k = 0; k < ratio.Length; k++) { ph[k] += 2 * Math.PI * f * ratio[k] / Rate; s += level[k] * Math.Sin(ph[k]) * Math.Exp(-t * decay[k] / 2.2); }
            return s * 0.5;
        }, amp, pan, attack: 0.001, release: 4.5);
    }

    /// <summary>A breathy flute.</summary>
    public void Flute(double start, double dur, double midi, double amp, double pan = 0)
    {
        double f = Hz(midi), phase = 0, lp = 0;
        Voice(start, dur, t =>
        {
            double vib = 1 + 0.004 * Math.Sin(2 * Math.PI * 5.0 * t) * Math.Min(1, Math.Max(0, (t - 0.2) / 0.4));
            phase += 2 * Math.PI * f * vib / Rate;
            lp += (White() - lp) * 0.2;
            return Math.Sin(phase) + 0.28 * Math.Sin(2 * phase) + 0.07 * Math.Sin(3 * phase) + lp * 0.05;
        }, amp, pan, attack: 0.06, release: 0.12, sustain: 0.95, decay: 0.3);
    }

    /// <summary>A plucked harp-like string.</summary>
    public void Pluck(double start, double midi, double amp, double pan = 0, double ring = 1.0)
    {
        double f = Hz(midi), phase = 0;
        Voice(start, 0.02, t =>
        {
            phase += 2 * Math.PI * f / Rate;
            double s = 0;
            for (int h = 1; h <= 6; h++) s += Math.Sin(h * phase) / (h * h * 0.5 + 0.5) * Math.Exp(-t * (1.6 + 1.9 * h) / ring);
            return s;
        }, amp, pan, attack: 0.002, release: 1.4 * ring);
    }

    /// <summary>A soft string pad: two slightly detuned oscillators with a few harmonics, a slow swell.</summary>
    public void Pad(double start, double dur, double midi, double amp, double pan = 0, double warmth = 1.0)
    {
        double f = Hz(midi), p1 = 0, p2 = 0;
        Voice(start, dur, t =>
        {
            p1 += 2 * Math.PI * f * 0.9985 / Rate; p2 += 2 * Math.PI * f * 1.0015 / Rate;
            double s = 0;
            for (int h = 1; h <= 5; h++) s += (Math.Sin(h * p1) + Math.Sin(h * p2)) / (h * (1 + 0.5 * h / warmth));
            return s * 0.5;
        }, amp, pan, attack: 0.35, release: 0.5);
    }

    /// <summary>A bass: a sine with a few harmonics, or a thicker sawtooth-like tone when `gritty`.</summary>
    public void Bass(double start, double dur, double midi, double amp, bool gritty = false)
    {
        double f = Hz(midi), phase = 0;
        Voice(start, dur, t =>
        {
            phase += 2 * Math.PI * f / Rate;
            double s = Math.Sin(phase) + 0.35 * Math.Sin(2 * phase);
            if (gritty) for (int h = 3; h <= 7; h++) s += Math.Sin(h * phase) / (h * 1.4) * Math.Exp(-t * 3);
            return s * 0.7;
        }, amp, 0, attack: 0.008, release: 0.06, sustain: 0.85, decay: 0.3);
    }

    // MARK: Drums (rendered straight into the buffer)

    private void Burst(double start, double length, Func<double, double> gen, double amp, double pan)
    {
        int s0 = (int)(start * Rate), n = (int)(length * Rate);
        double gl = Math.Cos((pan + 1) * Math.PI / 4), gr = Math.Sin((pan + 1) * Math.PI / 4);
        for (int i = 0; i < n; i++)
        {
            int k = s0 + i;
            if (k < 0 || k >= L.Length) continue;
            double v = gen(i / (double)Rate) * amp;
            L[k] += (float)(v * gl); R[k] += (float)(v * gr);
        }
    }

    /// <summary>A big drum (a taiko, a war drum): deep, with a long boom and a skin that slaps at the start.</summary>
    public void Taiko(double start, double midi, double amp, double pan = 0)
    {
        double phase = 0, f0 = Hz(midi);
        Burst(start, 1.2, t =>
        {
            phase += 2 * Math.PI * (f0 * (1 + 0.55 * Math.Exp(-t * 16))) / Rate;
            return Math.Sin(phase) * Math.Exp(-t * 3.4) + 0.35 * Math.Sin(2 * phase) * Math.Exp(-t * 7) + White() * 0.1 * Math.Exp(-t * 45);
        }, amp, pan);
    }

    public void Kick(double start, double amp)
    {
        double phase = 0;
        Burst(start, 0.35, t =>
        {
            double f = 46 + 90 * Math.Exp(-t * 28);
            phase += 2 * Math.PI * f / Rate;
            return Math.Sin(phase) * Math.Exp(-t * 11) + White() * 0.12 * Math.Exp(-t * 90);
        }, amp, 0);
    }

    public void Snare(double start, double amp, double pan = 0.05)
    {
        double phase = 0, lp = 0;
        Burst(start, 0.28, t =>
        {
            phase += 2 * Math.PI * 190 / Rate;
            lp += (White() - lp) * 0.75;
            return (lp * Math.Exp(-t * 17) * 0.9 + Math.Sin(phase) * Math.Exp(-t * 24) * 0.5);
        }, amp, pan);
    }

    public void Hat(double start, double amp, double pan = 0.25, bool open = false)
    {
        double prev = 0;
        Burst(start, open ? 0.3 : 0.06, t =>
        {
            double w = White(); double hp = w - prev; prev = w;
            return hp * Math.Exp(-t * (open ? 14 : 70));
        }, amp, pan);
    }

    /// <summary>A cowbell: two metallic tones that ring a moment.</summary>
    public void Cowbell(double start, double amp, double pan = 0.3)
    {
        double p1 = 0, p2 = 0;
        Burst(start, 0.22, t =>
        {
            p1 += 2 * Math.PI * 562 / Rate; p2 += 2 * Math.PI * 845 / Rate;
            double sq = Math.Sign(Math.Sin(p1)) * 0.6 + Math.Sign(Math.Sin(p2)) * 0.4;
            return sq * Math.Exp(-t * 22) * Math.Min(1, t / 0.0015);
        }, amp, pan);
    }

    /// <summary>A shaker: a short burst of high noise.</summary>
    public void Shaker(double start, double amp, double pan = -0.35)
    {
        double prev = 0;
        Burst(start, 0.07, t =>
        {
            double w = White(); double hp = w - prev; prev = w;
            return hp * Math.Exp(-t * 55) * Math.Min(1, t / 0.006);
        }, amp, pan);
    }

    /// <summary>The clave: two sticks, a short bright knock.</summary>
    public void Clave(double start, double amp, double pan = 0.4)
    {
        double phase = 0;
        Burst(start, 0.09, t =>
        {
            phase += 2 * Math.PI * 2450 / Rate;
            return Math.Sin(phase) * Math.Exp(-t * 55);
        }, amp, pan);
    }

    /// <summary>A ride cymbal: a bright wash that rings a little.</summary>
    public void Ride(double start, double amp, double pan = 0.25)
    {
        double prev = 0, p1 = 0, p2 = 0;
        Burst(start, 0.5, t =>
        {
            double w = White(); double hp = w - prev; prev = w;
            p1 += 2 * Math.PI * 3300 / Rate; p2 += 2 * Math.PI * 4710 / Rate;
            return (hp * 0.5 + Math.Sin(p1) * 0.2 + Math.Sin(p2) * 0.15) * Math.Exp(-t * 7) * Math.Min(1, t / 0.002);
        }, amp, pan);
    }

    /// <summary>A conga: a round skin drum, higher and shorter than a tom, with a slap at the start.</summary>
    public void Conga(double start, double midi, double amp, double pan = 0.2)
    {
        double phase = 0, f0 = Hz(midi);
        Burst(start, 0.22, t =>
        {
            phase += 2 * Math.PI * (f0 * (1 + 0.25 * Math.Exp(-t * 35))) / Rate;
            return Math.Sin(phase) * Math.Exp(-t * 15) + White() * 0.18 * Math.Exp(-t * 90);
        }, amp, pan);
    }

    public void Crash(double start, double amp)
    {
        double prev = 0;
        Burst(start, 2.4, t =>
        {
            double w = White(); double hp = w - prev; prev = w * 0.6;
            return hp * Math.Exp(-t * 2.2) * Math.Min(1, t / 0.004);
        }, amp, -0.1);
    }

    public void Tom(double start, double midi, double amp, double pan = 0)
    {
        double phase = 0, f0 = Hz(midi);
        Burst(start, 0.45, t =>
        {
            phase += 2 * Math.PI * (f0 * (1 + 0.5 * Math.Exp(-t * 20))) / Rate;
            return Math.Sin(phase) * Math.Exp(-t * 8) + White() * 0.05 * Math.Exp(-t * 60);
        }, amp, pan);
    }

    // MARK: Master

    /// <summary>A small Schroeder reverb: four combs and two all-pass filters per channel.</summary>
    public void Reverb(double wet, double room = 0.8)
    {
        ReverbChannel(L, wet, room, 0);
        ReverbChannel(R, wet, room, 1);
    }

    private static void ReverbChannel(float[] x, double wet, double room, int side)
    {
        double[] combTimes = { 0.0297, 0.0371, 0.0411, 0.0437 };
        var dry = (float[])x.Clone();
        var acc = new float[x.Length];
        foreach (double ct in combTimes)
        {
            int d = (int)((ct + side * 0.0007) * Rate);
            var buf = new float[d];
            int p = 0;
            for (int i = 0; i < x.Length; i++)
            {
                float y = buf[p];
                buf[p] = (float)(dry[i] + y * room);
                if (++p >= d) p = 0;
                acc[i] += y * 0.25f;
            }
        }
        foreach (double at in new[] { 0.005, 0.0017 })
        {
            int d = (int)(at * Rate);
            var buf = new float[d];
            int p = 0;
            for (int i = 0; i < acc.Length; i++)
            {
                float bufOut = buf[p];
                float input = acc[i];
                float outv = -0.6f * input + bufOut;
                buf[p] = input + 0.6f * outv;
                if (++p >= d) p = 0;
                acc[i] = outv;
            }
        }
        for (int i = 0; i < x.Length; i++) x[i] = (float)(dry[i] + acc[i] * wet);
    }

    /// <summary>Folds the part after `loopLength` seconds onto the start (so the reverb tail wraps round), trims, and sets the loudness
    /// (`rms` is the average level; peaks are softly limited).</summary>
    public (float[] left, float[] right) FinishLoop(double loopLength, double rms = 0.17)
    {
        int n = (int)(loopLength * Rate);
        var l = new float[n]; var r = new float[n];
        for (int i = 0; i < L.Length; i++)
        {
            l[i % n] += L[i]; r[i % n] += R[i];
        }
        double sum = 0;
        for (int i = 0; i < n; i += 3) sum += l[i] * (double)l[i];
        double current = Math.Sqrt(sum / (n / 3.0));
        float g = (float)(rms / Math.Max(current, 1e-9));
        for (int i = 0; i < n; i++)
        {
            l[i] = (float)Math.Tanh(l[i] * g) * 0.97f; r[i] = (float)Math.Tanh(r[i] * g) * 0.97f;
        }
        return (l, r);
    }

    public static byte[] ToPcm16(float[] l, float[] r)
    {
        var bytes = new byte[l.Length * 4];
        for (int i = 0; i < l.Length; i++)
        {
            short a = (short)Math.Clamp(l[i] * 32767f, -32768, 32767), b = (short)Math.Clamp(r[i] * 32767f, -32768, 32767);
            bytes[i * 4] = (byte)(a & 0xFF); bytes[i * 4 + 1] = (byte)((a >> 8) & 0xFF);
            bytes[i * 4 + 2] = (byte)(b & 0xFF); bytes[i * 4 + 3] = (byte)((b >> 8) & 0xFF);
        }
        return bytes;
    }
}
