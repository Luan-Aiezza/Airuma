using Godot;
using System;

/// <summary>
/// Every sound effect of the game, synthesised at start-up (no audio files). They are modelled on the sounds of Touhou's
/// danmaku: a thin "tick" for the player's shots, a rising "pyu" or a soft "tun" for the spirits' orbs, a hissing "tsu" for rice
/// bullets, and for the boss's magic a fiery rush, a crackle, a wobbling gloop, a bell, a heavy thud. Each kind of bullet has its own.
/// </summary>
public static class SfxBank
{
    public const int Rate = 32000;

    public enum Sound
    {
        PlayerShot, PlayerWisp, PlayerHit, WardBroke,
        DefeatSwarm, DefeatGunner, DefeatHeavy, BossBroken,
        OrbGreen, OrbRed, RiceRed, Fireball, FlameDart, Spark, VenomOrb, LotusPetal, MagmaOrb, FireSerpent, Ember,
        Bomb, Pickup, LevelUp, MenuMove, MenuSelect, MenuBack, DialogueNext,
        HitGunner, HitHeavy, HitBoss,
    }

    /// <summary>Volume in dB, and the least time in seconds between two plays (so volleys do not pile up into noise).</summary>
    public static (float db, float gap) Settings(Sound s) => s switch
    {
        Sound.PlayerShot => (-20f, 0.05f), Sound.PlayerWisp => (-15f, 0.1f),
        Sound.PlayerHit => (-5f, 0.2f), Sound.WardBroke => (-7f, 0.2f),
        Sound.DefeatSwarm => (-13f, 0.03f), Sound.DefeatGunner => (-11f, 0.04f), Sound.DefeatHeavy => (-7f, 0.1f), Sound.BossBroken => (-3f, 0.5f),
        Sound.OrbGreen => (-16f, 0.06f), Sound.OrbRed => (-16f, 0.08f), Sound.RiceRed => (-22f, 0.05f),
        Sound.Fireball => (-13f, 0.12f), Sound.FlameDart => (-15f, 0.12f), Sound.Spark => (-21f, 0.06f), Sound.VenomOrb => (-19f, 0.06f),
        Sound.LotusPetal => (-14f, 0.15f), Sound.MagmaOrb => (-9f, 0.2f), Sound.FireSerpent => (-25f, 0.05f), Sound.Ember => (-24f, 0.05f),
        Sound.Bomb => (-5f, 0.5f), Sound.Pickup => (-19f, 0.035f), Sound.LevelUp => (-8f, 0.3f),
        Sound.MenuMove => (-15f, 0.03f), Sound.MenuSelect => (-11f, 0.1f), Sound.MenuBack => (-13f, 0.1f), Sound.DialogueNext => (-16f, 0.05f),
        Sound.HitGunner => (-18f, 0.05f), Sound.HitHeavy => (-16f, 0.05f), Sound.HitBoss => (-14f, 0.06f),
        _ => (-12f, 0.05f),
    };


    /// <summary>The sound of a bullet that has just been fired.</summary>
    public static Sound ForBullet(BulletStyle style) => style switch
    {
        BulletStyle.LightBolt => Sound.PlayerShot,
        BulletStyle.Wisp => Sound.PlayerWisp,
        BulletStyle.OrbGreen => Sound.OrbGreen,
        BulletStyle.OrbRed => Sound.OrbRed,
        BulletStyle.RiceRed => Sound.RiceRed,
        BulletStyle.Fireball => Sound.Fireball,
        BulletStyle.FlameDart => Sound.FlameDart,
        BulletStyle.Spark => Sound.Spark,
        BulletStyle.VenomOrb => Sound.VenomOrb,
        BulletStyle.LotusPetal => Sound.LotusPetal,
        BulletStyle.MagmaOrb => Sound.MagmaOrb,
        BulletStyle.FireSerpent => Sound.FireSerpent,
        _ => Sound.Ember,
    };

    public static Sound ForDefeat(SpiritKind kind) => kind switch { SpiritKind.Swarm => Sound.DefeatSwarm, SpiritKind.Gunner => Sound.DefeatGunner, _ => Sound.DefeatHeavy };

    // MARK: Building blocks

    private sealed class Noise
    {
        private readonly Random _r;
        public Noise(int seed) { _r = new Random(seed); }
        public double Next() => _r.NextDouble() * 2 - 1;
    }

    private static float[] Render(double seconds, Func<double, double> gen)
    {
        int n = (int)(seconds * Rate);
        var data = new float[n];
        for (int i = 0; i < n; i++)
        {
            double t = i / (double)Rate;
            // a short fade at the very end avoids a click
            double fade = Math.Min(1, (seconds - t) / 0.004);
            data[i] = (float)(gen(t) * fade);
        }
        return data;
    }

    /// <summary>A tone whose frequency follows `freq(t)`; the phase is integrated so sweeps are smooth.</summary>
    private static Func<double, double> Sweep(Func<double, double> freq, Func<double, double> shape = null)
    {
        double phase = 0, last = 0;
        return t =>
        {
            phase += 2 * Math.PI * freq(t) * (t - last);
            last = t;
            return shape != null ? shape(phase) : Math.Sin(phase);
        };
    }

    private static double Env(double t, double attack, double decay) => Math.Min(1, t / Math.Max(attack, 1e-6)) * Math.Exp(-t * decay);

    private static Func<double, double> LowNoise(int seed, Func<double, double> cutoff)
    {
        var n = new Noise(seed);
        double y = 0, last = 0;
        return t =>
        {
            double a = Math.Clamp(cutoff(t), 0.001, 0.99);
            y += (n.Next() - y) * a;
            last = t;
            return y;
        };
    }

    private static Func<double, double> HighNoise(int seed, double amount = 1.0)
    {
        var n = new Noise(seed);
        double prev = 0;
        return t => { double w = n.Next(); double hp = w - prev; prev = w * amount; return hp * 0.6; };
    }

    // MARK: The sounds

    public static float[] Make(Sound sound)
    {
        switch (sound)
        {
            case Sound.PlayerShot: // a bright, thin tick that falls a little
            {
                var tone = Sweep(t => 1900 - 9000 * t);
                return Render(0.055, t => tone(t) * Env(t, 0.001, 55) * 0.8 + HighNoise(1)(t) * Math.Exp(-t * 200) * 0.25);
            }
            case Sound.PlayerWisp: // a soft chime
            {
                double p1 = 0, p2 = 0;
                return Render(0.16, t =>
                {
                    p1 += 2 * Math.PI * 1318.5 * (1 + 0.004 * Math.Sin(t * 60)) / Rate; p2 += 2 * Math.PI * 1975.5 / Rate;
                    return (Math.Sin(p1) * 0.8 + Math.Sin(p2) * 0.4) * Env(t, 0.003, 20);
                });
            }
            case Sound.PlayerHit: // the fall of a bright note, and a crack
            {
                var tone = Sweep(t => 2200 * Math.Exp(-t * 5.5) + 160);
                var tone2 = Sweep(t => 1100 * Math.Exp(-t * 4.5) + 120);
                var crack = LowNoise(2, t => 0.7 * Math.Exp(-t * 14) + 0.03);
                return Render(0.6, t => (tone(t) * 0.6 + tone2(t) * 0.35) * Env(t, 0.002, 4.5) + crack(t) * Math.Exp(-t * 16) * 0.9);
            }
            case Sound.WardBroke: // glass
            {
                var hi = HighNoise(3, 0.4);
                double[] partials = { 2637, 3520, 4186, 5274 };
                var phases = new double[partials.Length];
                return Render(0.45, t =>
                {
                    double s = hi(t) * Math.Exp(-t * 22) * 0.6;
                    for (int i = 0; i < partials.Length; i++)
                    {
                        phases[i] += 2 * Math.PI * partials[i] / Rate;
                        s += Math.Sin(phases[i]) * Math.Exp(-t * (8 + i * 4)) * 0.22;
                    }
                    return s;
                });
            }
            case Sound.DefeatSwarm: // a small pop
            {
                var tone = Sweep(t => 720 * Math.Exp(-t * 11) + 140);
                var puff = LowNoise(4, t => 0.5 * Math.Exp(-t * 20) + 0.04);
                return Render(0.2, t => tone(t) * Env(t, 0.001, 18) * 0.9 + puff(t) * Math.Exp(-t * 22) * 0.7);
            }
            case Sound.DefeatGunner: // a rounder burst
            {
                var tone = Sweep(t => 560 * Math.Exp(-t * 8) + 90);
                var puff = LowNoise(5, t => 0.45 * Math.Exp(-t * 12) + 0.03);
                return Render(0.3, t => tone(t) * Env(t, 0.001, 12) * 0.9 + puff(t) * Math.Exp(-t * 13) * 0.9);
            }
            case Sound.DefeatHeavy: // a deep burst with sparks
            {
                var tone = Sweep(t => 330 * Math.Exp(-t * 6) + 55);
                var boom = LowNoise(6, t => 0.35 * Math.Exp(-t * 7) + 0.02);
                var sparks = HighNoise(7, 0.5);
                return Render(0.6, t => tone(t) * Env(t, 0.001, 7) * 1.0 + boom(t) * Math.Exp(-t * 7) * 1.2 + sparks(t) * Math.Exp(-t * 14) * 0.25);
            }
            case Sound.BossBroken: // a great explosion
            {
                var tone = Sweep(t => 260 * Math.Exp(-t * 3) + 38);
                var boom = LowNoise(8, t => 0.3 * Math.Exp(-t * 3.2) + 0.012);
                var sparks = HighNoise(9, 0.5);
                var chime = Sweep(t => 880 + 440 * t);
                return Render(1.4, t => tone(t) * Env(t, 0.002, 2.6) * 1.1 + boom(t) * Math.Exp(-t * 2.6) * 1.5 + sparks(t) * Math.Exp(-t * 6) * 0.35 + chime(t) * Math.Exp(-t * 4) * 0.15);
            }
            case Sound.OrbGreen: // "tun": a soft round tick
            {
                var tone = Sweep(t => 560 + 400 * Math.Exp(-t * 40));
                return Render(0.12, t => tone(t) * Env(t, 0.002, 30));
            }
            case Sound.OrbRed: // "pyu": a small bubble that rises
            {
                var tone = Sweep(t => 480 + 5200 * t);
                return Render(0.13, t => tone(t) * Math.Sin(Math.PI * Math.Min(1, t / 0.13)) * 0.95);
            }
            case Sound.RiceRed: // "tsu": a thin hiss
            {
                var hi = HighNoise(10, 0.7);
                var tone = Sweep(t => 3000 - 12000 * t);
                return Render(0.05, t => (hi(t) * 0.8 + tone(t) * 0.4) * Env(t, 0.001, 70));
            }
            case Sound.Fireball: // a rush of fire
            {
                var rush = LowNoise(11, t => 0.05 + 0.5 * Math.Min(1, t / 0.12) * Math.Exp(-t * 6));
                var tone = Sweep(t => 150 + 200 * Math.Min(1, t / 0.1));
                return Render(0.3, t => rush(t) * Env(t, 0.03, 9) * 2.2 + tone(t) * Env(t, 0.02, 12) * 0.25);
            }
            case Sound.FlameDart: // a quick sharp whoosh that climbs
            {
                var rush = LowNoise(12, t => 0.12 + 5 * t);
                var tone = Sweep(t => 500 + 3000 * t);
                return Render(0.16, t => rush(t) * Env(t, 0.01, 12) * 1.8 + tone(t) * Env(t, 0.005, 20) * 0.3);
            }
            case Sound.Spark: // a zap
            {
                var tone = Sweep(t => 1900 + 9000 * t, p => Math.Sign(Math.Sin(p)) * 0.5 + Math.Sin(p) * 0.5);
                return Render(0.09, t => tone(t) * Env(t, 0.001, 25) * (0.7 + 0.3 * Math.Sin(t * 2 * Math.PI * 200)));
            }
            case Sound.VenomOrb: // a wobbling gloop
            {
                var tone = Sweep(t => 330 + 90 * Math.Sin(2 * Math.PI * 34 * t) - 400 * t);
                return Render(0.16, t => tone(t) * Env(t, 0.004, 14));
            }
            case Sound.LotusPetal: // a little bell
            {
                double[] f = { 1174.7, 1760, 2349.3 };
                var ph = new double[3];
                return Render(0.45, t =>
                {
                    double s = 0;
                    for (int i = 0; i < 3; i++) { ph[i] += 2 * Math.PI * f[i] / Rate; s += Math.Sin(ph[i]) * Math.Exp(-t * (7 + 5 * i)) * (i == 0 ? 0.8 : 0.35); }
                    return s * Math.Min(1, t / 0.002);
                });
            }
            case Sound.MagmaOrb: // a heavy thump of molten rock
            {
                var tone = Sweep(t => 110 * Math.Exp(-t * 7) + 38);
                var rumble = LowNoise(13, t => 0.06);
                return Render(0.45, t => tone(t) * Env(t, 0.003, 7) * 1.2 + rumble(t) * Env(t, 0.01, 8) * 2.5);
            }
            case Sound.FireSerpent: // a tiny hiss
            {
                var hi = HighNoise(14, 0.8);
                return Render(0.09, t => hi(t) * Math.Sin(Math.PI * Math.Min(1, t / 0.09)) * (0.7 + 0.3 * Math.Sin(t * 2 * Math.PI * 90)));
            }
            case Sound.Bomb: // the lantern flares: a deep swell, a rush of light and a rising peal of bells
            {
                var tone = Sweep(t => 150 * Math.Exp(-t * 2.2) + 48);
                var swell = LowNoise(15, t => 0.02 + 0.28 * Math.Min(1, t / 0.5) * Math.Exp(-t * 1.3));
                var shine = HighNoise(16, 0.5);
                double[] bells = { 587.3, 880, 1174.7, 1568, 2093 };
                var ph = new double[bells.Length];
                return Render(1.6, t =>
                {
                    double s = tone(t) * Env(t, 0.004, 2.8) * 0.9 + swell(t) * Math.Min(1, t / 0.12) * Math.Exp(-t * 1.5) * 1.8 + shine(t) * Math.Exp(-t * 5) * 0.2;
                    for (int i = 0; i < bells.Length; i++)
                    {
                        double start = 0.04 + i * 0.07;
                        ph[i] += 2 * Math.PI * bells[i] / Rate;
                        if (t > start) s += Math.Sin(ph[i]) * Math.Exp(-(t - start) * 3.2) * Math.Min(1, (t - start) / 0.003) * (0.28 - i * 0.03);
                    }
                    return s;
                });
            }
            case Sound.Pickup: // a tiny sparkle of two notes
            {
                double p = 0;
                return Render(0.11, t =>
                {
                    double f = t < 0.045 ? 1568 : 2093;
                    p += 2 * Math.PI * f / Rate;
                    return Math.Sin(p) * Env(t < 0.045 ? t : t - 0.045, 0.002, 30) * 0.8;
                });
            }
            case Sound.LevelUp: // a bright rising arpeggio with a shimmer
            {
                double[] notes = { 1046.5, 1318.5, 1568, 2093 };
                var ph = new double[notes.Length];
                var sh = HighNoise(17, 0.5);
                return Render(0.75, t =>
                {
                    double s = 0;
                    for (int i = 0; i < notes.Length; i++)
                    {
                        double start = i * 0.07;
                        ph[i] += 2 * Math.PI * notes[i] / Rate;
                        if (t > start) s += (Math.Sin(ph[i]) + 0.3 * Math.Sin(2 * ph[i])) * Math.Exp(-(t - start) * 5.5) * Math.Min(1, (t - start) / 0.003) * 0.35;
                    }
                    return s + sh(t) * Math.Exp(-t * 9) * 0.1;
                });
            }
            case Sound.MenuMove: // the cursor: a small "pi"
            {
                var tone = Sweep(t => 1250 - 700 * t);
                return Render(0.05, t => tone(t) * Env(t, 0.001, 55));
            }
            case Sound.MenuSelect: // a confirming "pin-pon"
            {
                double p = 0;
                return Render(0.2, t =>
                {
                    double f = t < 0.07 ? 1175 : 1760;
                    p += 2 * Math.PI * f / Rate;
                    double local = t < 0.07 ? t : t - 0.07;
                    return (Math.Sin(p) + 0.25 * Math.Sin(2 * p)) * Env(local, 0.002, 16) * 0.7;
                });
            }
            case Sound.MenuBack: // a lower, falling "po"
            {
                var tone = Sweep(t => 880 - 2200 * t);
                return Render(0.12, t => tone(t) * Env(t, 0.002, 28) * 0.9);
            }
            case Sound.DialogueNext: // a soft blip for the next line
            {
                var tone = Sweep(t => 1000 + 500 * Math.Min(1, t / 0.03));
                return Render(0.075, t => (tone(t) * 0.7 + HighNoise(18, 0.4)(t) * 0.05) * Env(t, 0.002, 38));
            }
            case Sound.HitGunner: // a light "tk" on a sturdy spirit
            {
                var tone = Sweep(t => 950 * Math.Exp(-t * 30) + 520);
                var tick = HighNoise(19, 0.5);
                return Render(0.07, t => tone(t) * Env(t, 0.001, 55) * 0.8 + tick(t) * Math.Exp(-t * 130) * 0.35);
            }
            case Sound.HitHeavy: // a duller, meatier "thok"
            {
                var tone = Sweep(t => 420 * Math.Exp(-t * 22) + 160);
                var thud = LowNoise(20, t => 0.3 * Math.Exp(-t * 30) + 0.02);
                return Render(0.1, t => tone(t) * Env(t, 0.001, 38) * 0.9 + thud(t) * Math.Exp(-t * 40) * 1.2);
            }
            case Sound.HitBoss: // the boss is hurt: a ringing, fiery impact
            {
                var tone = Sweep(t => 300 * Math.Exp(-t * 18) + 130);
                double ph = 0;
                var crack = HighNoise(21, 0.5);
                return Render(0.16, t =>
                {
                    ph += 2 * Math.PI * 1960.0 / Rate;
                    return tone(t) * Env(t, 0.001, 24) * 1.0 + Math.Sin(ph) * Math.Exp(-t * 34) * 0.35 + crack(t) * Math.Exp(-t * 90) * 0.4;
                });
            }
            default: // Ember: a small tick
            {
                var tone = Sweep(t => 1500 - 7000 * t);
                return Render(0.045, t => tone(t) * Env(t, 0.001, 60));
            }
        }
    }

    public static AudioStreamWav ToStream(float[] data)
    {
        var bytes = new byte[data.Length * 2];
        for (int i = 0; i < data.Length; i++)
        {
            short v = (short)Math.Clamp(Math.Tanh(data[i]) * 32000, -32768, 32767);
            bytes[i * 2] = (byte)(v & 0xFF); bytes[i * 2 + 1] = (byte)((v >> 8) & 0xFF);
        }
        return new AudioStreamWav { Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = Rate, Stereo = false, Data = bytes };
    }
}
