using System;
using System.Collections.Generic;
using Godot;

public static partial class BulletSystem
{
    private static readonly PatternContext Context = new PatternContext();

    /// <summary>Runs the patterns of the boss's attack: each has its own clock, begins after its delay, rests between bursts and fires every interval.</summary>
    private static void RunPatterns(BossEntity boss, PlayerEntity player, BulletData[] pool, float delta, Difficulty difficulty, Rect2 field, BossEntity twin, List<SlashData> slashes = null)
    {
        var c = Context;
        c.Slashes = slashes;
        c.Pool = pool; c.Player = player.Transform.Position; c.Field = field; c.Difficulty = difficulty; c.Pressure = boss.PatternPressure; c.Speed = boss.PatternSpeed;
        float rate = MathF.Sqrt(MathF.Max(0.1f, boss.PatternPressure));
        var runs = boss.Patterns;
        for (int i = 0; i < runs.Length; i++)
        {
            ref PatternRun run = ref runs[i];
            var spec = run.Spec;
            run.Clock += delta;
            if (spec.OnBeam)
            {
                // a figure that goes with the jet: once, a moment after the beam goes on, for each jet
                if (!boss.Sweep.BeamOn) { run.BeamClock = 0; run.BeamDone = false; continue; }
                run.BeamClock += delta;
                if (run.BeamDone || run.BeamClock < spec.BeamDelay) continue;
                run.BeamDone = true;
                c.Origin = (spec.FromTwin && twin != null ? twin : boss).Transform.Position;
                c.Time = run.Clock; c.Count = run.Shots++; c.Step = c.Count; c.Burst = 0;
                spec.Fire(c);
                continue;
            }
            if (spec.QuakeOnly && boss.Specials.QuakeLeft <= 0) continue;
            float t = run.Clock - spec.Delay;
            if (t < 0) continue;
            // inside a burst or in the rest after it
            float cycle = spec.OnFor + spec.OffFor;
            int burst = 0;
            float inBurst = t;
            if (!float.IsInfinity(spec.OnFor))
            {
                burst = (int)MathF.Floor(t / cycle);
                inBurst = t - burst * cycle;
                if (inBurst >= spec.OnFor) continue;
                if (burst != run.Burst) { run.Burst = burst; run.BurstShots = 0; run.NextAt = burst * cycle; }
            }
            if (t < run.NextAt) continue;
            float interval = spec.Interval / rate;
            run.NextAt = MathF.Max(run.NextAt, t - interval) + interval;        // (at most one volley a step: a stall does not make a flood)
            var source = spec.FromTwin && twin != null ? twin : boss;
            c.Origin = source.Transform.Position;
            c.Time = t; c.Count = run.Shots++; c.Step = run.BurstShots++; c.Burst = burst;
            spec.Fire(c);
        }
    }
}
