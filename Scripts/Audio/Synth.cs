using System;

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
    public void Trumpet(double start, double dur, double midi, double amp, double pan = 0, double brightness = 1.0)
    {
        double f = Hz(midi), phase = 0;
        Voice(start, dur, t =>
        {
            double vib = 1 + 0.007 * Math.Sin(2 * Math.PI * 5.4 * t) * Math.Min(1, Math.Max(0, (t - 0.12) / 0.3));
            double scoop = t < 0.06 ? Math.Pow(2, -0.5 * (1 - t / 0.06) / 12) : 1;
            double fr = f * vib * scoop;
            phase += 2 * Math.PI * fr / Rate;
            double b = (2.5 + 5.5 * Math.Min(1, t / 0.07)) * brightness;
            int nh = (int)Math.Min(16, Rate / 2.2 / fr);
            double s = 0;
            for (int h = 1; h <= nh; h++) s += Math.Pow(h, -0.85) * Math.Exp(-(h - 1) / b) * Math.Sin(h * phase);
            return s * 0.55 + White() * 0.012 * Math.Min(1, t / 0.05);
        }, amp, pan, attack: 0.03, release: 0.09, sustain: 0.9, decay: 0.2);
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
