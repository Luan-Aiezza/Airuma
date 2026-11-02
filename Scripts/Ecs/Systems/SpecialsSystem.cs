using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Runs what a boss attack does besides firing bullets (see <see cref="BossSpecials"/>): the slashes of its claws, the quake it stamps and the roar that pushes
/// the boat. The world calls <see cref="Update"/> every step while the boss fights.
/// </summary>
public static class SpecialsSystem
{
    /// <summary>Starts the specials of an attack: the first of each comes a little after the attack begins. Easy gives more time to react, hard less.</summary>
    public static BossSpecials Begin(BossPhase phase, Difficulty difficulty)
    {
        var s = new BossSpecials { Slashes = phase.Slashes, Quake = phase.Quake, Roar = phase.Roar };
        float time = difficulty == Difficulty.Easy ? 1.2f : (difficulty == Difficulty.Hard ? 0.85f : 1);
        if (s.Slashes.IsActive) { s.Slashes.Telegraph *= difficulty == Difficulty.Easy ? 1.25f : (difficulty == Difficulty.Hard ? 0.88f : 1); s.Slashes.Interval *= time; s.SlashClock = s.Slashes.Interval * 0.4f; }
        if (s.Quake.IsActive) { s.Quake.Interval *= time; s.QuakeClock = s.Quake.Interval * 0.4f; }
        if (s.Roar.IsActive) { s.Roar.Interval *= time; s.Roar.Push *= difficulty == Difficulty.Easy ? 0.8f : 1; s.RoarClock = s.Roar.Interval * 0.5f; }
        return s;
    }

    /// <summary>How hard the screen shakes now (0: not at all).</summary>
    public static float Shake(BossSpecials s)
    {
        float shake = s.QuakeLeft > 0 ? s.Quake.Intensity * Math.Min(1, s.QuakeLeft / 0.5f + 0.35f) : 0;
        if (s.RoarLeft > 0 && s.Roar.Seconds > 0) shake = Math.Max(shake, 0.8f * s.RoarLeft / s.Roar.Seconds);
        return shake;
    }

    /// <summary>
    /// Moves the clocks on, begins a volley of slashes, a quake or a roar when it is time, ages the slashes and drops those that have faded.
    /// Returns the push (px/s) that the roar gives the boat at this moment.
    /// </summary>
    public static Vector2 Update(BossEntity boss, PlayerEntity player, List<SlashData> slashes, float delta, List<WorldEvent> events, SeededGenerator rng)
    {
        var s = boss.Specials;
        // Ageing: a slash strikes when its warning is over.
        for (int i = slashes.Count - 1; i >= 0; i--)
        {
            var slash = slashes[i];
            float before = slash.Age;
            slash.Age += delta;
            if (slash.Leader && before < slash.Telegraph && slash.Age >= slash.Telegraph) events.Add(WorldEvent.SlashStruck());
            if (slash.Gone) slashes.RemoveAt(i); else slashes[i] = slash;
        }

        if (s.Slashes.IsActive)
        {
            s.SlashClock += delta;
            if (s.SlashClock >= s.Slashes.Interval) { s.SlashClock = 0; Volley(s, player, slashes, rng); }
        }

        if (s.Quake.IsActive)
        {
            if (s.QuakeLeft > 0) s.QuakeLeft = Math.Max(0, s.QuakeLeft - delta);
            else
            {
                s.QuakeClock += delta;
                if (s.QuakeClock >= s.Quake.Interval) { s.QuakeClock = 0; s.QuakeLeft = s.Quake.Duration; events.Add(WorldEvent.QuakeStarted()); }
            }
        }

        var push = Vector2.Zero;
        if (s.Roar.IsActive)
        {
            if (s.RoarLeft > 0)
            {
                s.RoarLeft = Math.Max(0, s.RoarLeft - delta);
                var away = player.Transform.Position - boss.Transform.Position;
                if (away.LengthSquared() < 1) away = new Vector2(0, 1);
                push = away.Normalized() * s.Roar.Push * (s.RoarLeft / Math.Max(0.01f, s.Roar.Seconds));
            }
            else
            {
                s.RoarClock += delta;
                if (s.RoarClock >= s.Roar.Interval) { s.RoarClock = 0; s.RoarLeft = s.Roar.Seconds; events.Add(WorldEvent.Roared(boss.Transform.Position)); }
            }
        }
        return push;
    }

    /// <summary>A volley: `Count` parallel slashes, the middle one through the boat (or at random, or alternating from side to side), each warned of before it strikes.</summary>
    private static void Volley(BossSpecials s, PlayerEntity player, List<SlashData> slashes, SeededGenerator rng)
    {
        var spec = s.Slashes;
        s.Volleys++;
        float sign = spec.Aim == SlashAim.Alternate && s.Volleys % 2 == 0 ? -1 : 1;
        float angle = Mathx.Radians(spec.Angle) * sign;
        var dir = new Vector2(MathF.Sin(angle), MathF.Cos(angle));
        // the point the middle slash goes through
        Vector2 through = spec.Aim switch
        {
            SlashAim.Player => player.Transform.Position + new Vector2(rng.Range(-0.5f, 0.5f) * spec.Spacing, 0),
            SlashAim.Random => new Vector2(rng.Range(160f, 440f), rng.Range(250f, 550f)),
            _ => new Vector2((s.Volleys % 2 == 0 ? 190 : 410) + rng.Range(-30f, 30f), 400),
        };
        const float topY = -40;
        // A slash that is nearly level (a wall of water) begins off the side of the field instead of above it.
        var middleTop = MathF.Abs(dir.Y) < 0.35f ? through - dir * 700 : through - dir * ((through.Y - topY) / dir.Y);
        var across = new Vector2(dir.Y, -dir.X);           // along the row of slashes, perpendicular to them
        for (int k = 0; k < spec.Count; k++)
        {
            float offset = (k - (spec.Count - 1) / 2f) * spec.Spacing;
            slashes.Add(new SlashData { Top = middleTop + across * offset, Angle = angle, Telegraph = spec.Telegraph, Strike = spec.Strike, Width = spec.Width, Leader = k == 0 });
        }
    }

    /// <summary>Whether the boat is in the way of a slash that is striking.</summary>
    public static bool HitBySlash(IReadOnlyList<SlashData> slashes, PlayerEntity player)
    {
        foreach (var slash in slashes)
            if (slash.Striking && Mathx.DistanceToSegment(player.Transform.Position, slash.Top, slash.Bottom) < slash.Width / 2 + player.Hitbox.Radius) return true;
        return false;
    }
}
