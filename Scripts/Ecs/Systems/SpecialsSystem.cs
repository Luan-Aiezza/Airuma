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
        var s = new BossSpecials { Slashes = phase.Slashes, Quake = phase.Quake, Roar = phase.Roar, Gravity = phase.Gravity };
        float time = difficulty == Difficulty.Easy ? 1.2f : (difficulty == Difficulty.Hard ? 0.85f : 1);
        if (s.Slashes.IsActive) { s.Slashes.Telegraph *= difficulty == Difficulty.Easy ? 1.25f : (difficulty == Difficulty.Hard ? 0.88f : 1); s.Slashes.Interval *= time; s.SlashClock = s.Slashes.Interval * 0.4f; }
        if (s.Quake.IsActive) { s.Quake.Interval *= time; s.QuakeClock = s.Quake.Interval * 0.4f; }
        if (s.Roar.IsActive) { s.Roar.Interval *= time; s.Roar.Push *= difficulty == Difficulty.Easy ? 0.8f : 1; s.RoarClock = s.Roar.Interval * 0.5f; }
        if (s.Gravity.IsActive)
        {
            s.Gravity.Interval *= time; s.Gravity.Pull *= difficulty == Difficulty.Easy ? 0.75f : (difficulty == Difficulty.Hard ? 1.1f : 1);
            s.Gravity.Warn *= difficulty == Difficulty.Easy ? 1.25f : 1; s.GravityClock = s.Gravity.Interval * 0.4f;
        }
        return s;
    }

    /// <summary>The moon of a gravity attack, as the scene and the hits need it.</summary>
    public static MoonData Moon(BossSpecials s) => !s.Gravity.IsActive || !s.MoonStarted
        ? default
        : new MoonData { Active = true, Position = s.MoonPos, Radius = s.Gravity.Radius, Warning = s.WarnLeft > 0 ? 1 - s.WarnLeft / Math.Max(0.01f, s.Gravity.Warn) : 0, Pull = s.PullLeft > 0 ? Math.Min(1, s.PullLeft / 0.4f) * Math.Min(1, (s.Gravity.Duration - s.PullLeft) / 0.4f + 0.15f) : 0 };

    /// <summary>The moon is a body: touching it hurts.</summary>
    public static bool HitByMoon(BossSpecials s, PlayerEntity player)
    {
        var moon = Moon(s);
        return moon.Active && moon.Position.DistanceTo(player.Transform.Position) < moon.Radius * 0.85f + player.Hitbox.Radius;
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
    public static Vector2 Update(BossEntity boss, PlayerEntity player, List<SlashData> slashes, float delta, List<WorldEvent> events, SeededGenerator rng, BulletData[] pool = null)
    {
        var s = boss.Specials;
        // Ageing: a slash strikes when its warning is over.
        for (int i = slashes.Count - 1; i >= 0; i--)
        {
            var slash = slashes[i];
            float before = slash.Age;
            slash.Age += delta;
            if (slash.Spin != 0) slash.Angle += slash.Spin * delta;                           // a laser that turns
            if (slash.Follow) slash.Top = boss.Transform.Position + slash.Offset;            // one that goes with the boss
            if (slash.Leader && before < slash.Telegraph && slash.Age >= slash.Telegraph) events.Add(WorldEvent.SlashStruck());
            // after the cut, the line lets go of a trail of bullets that spread out
            if (slash.Trail && !slash.Trailed && pool != null && slash.Age >= slash.Telegraph + slash.Strike) { slash.Trailed = true; LeaveTrail(slash, pool, rng); }
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
                if (s.RoarClock >= s.Roar.Interval) { s.RoarClock = 0; s.RoarLeft = s.Roar.Seconds; events.Add(WorldEvent.Roared(boss.Transform.Position)); ShoveBoulders(boss, pool, s.Roar.Push); }
            }
        }
        if (s.Gravity.IsActive)
        {
            var g = s.Gravity;
            if (!s.MoonStarted) { s.MoonStarted = true; s.MoonStart = boss.Transform.Position; s.MoonPos = s.MoonStart; s.MoonAge = 0; }
            s.MoonAge += delta;
            float u = Math.Clamp(s.MoonAge / Math.Max(0.1f, g.Arrive), 0, 1);
            s.MoonPos = s.MoonStart.Lerp(g.Target, u * u * (3 - 2 * u));
            if (u >= 1)
            {
                if (s.PullLeft > 0)
                {
                    s.PullLeft = Math.Max(0, s.PullLeft - delta);
                    var towards = s.MoonPos - player.Transform.Position;
                    float envelope = Math.Min(1, s.PullLeft / 0.4f) * Math.Min(1, (g.Duration - s.PullLeft) / 0.4f + 0.15f);
                    if (towards.LengthSquared() > 1) push += towards.Normalized() * g.Pull * envelope;
                }
                else if (s.WarnLeft > 0)
                {
                    s.WarnLeft = Math.Max(0, s.WarnLeft - delta);
                    if (s.WarnLeft <= 0) s.PullLeft = g.Duration;
                }
                else
                {
                    s.GravityClock += delta;
                    if (s.GravityClock >= g.Interval) { s.GravityClock = 0; s.WarnLeft = g.Warn; }
                }
            }
        }
        return push;
    }

    /// <summary>Whether a line through `point` along `dir` crosses the field (some part of it is on the screen).</summary>
    private static bool CrossesField(Vector2 point, Vector2 dir)
    {
        var field = new Rect2(0, 0, 600, 775);
        for (float d = -1100; d <= 1100; d += 20) if (field.HasPoint(point + dir * d)) return true;
        return false;
    }

    /// <summary>The roar pushes the great boulders too, away from her and harder than it pushes the boat: they go quicker for a moment and settle back to their pace.</summary>
    private static void ShoveBoulders(BossEntity boss, BulletData[] pool, float push)
    {
        if (pool == null) return;
        for (int i = 0; i < pool.Length; i++)
        {
            ref BulletData b = ref pool[i];
            if (!b.IsActive || b.Cruise <= 0) continue;
            var away = b.Position - boss.Transform.Position;
            if (away.LengthSquared() < 1) away = new Vector2(0, 1);
            b.Velocity += away.Normalized() * push * 1.3f;
        }
    }

    /// <summary>The trail of a cut: small bullets all along the line, each going off to one side or the other, one after another, at its own speed.</summary>
    private static void LeaveTrail(SlashData slash, BulletData[] pool, SeededGenerator rng)
    {
        var dir = slash.Direction;
        float heading = Mathx.Degrees(MathF.Atan2(dir.Y, dir.X));
        int k = 0;
        for (float d = 0; d < slash.TotalLength; d += 34)
        {
            var p = slash.Top + dir * d;
            if (p.X < -10 || p.X > 610 || p.Y < -10 || p.Y > 785) continue;
            float side = k++ % 2 == 0 ? 90 : -90;
            BulletSystem.Fire(pool, p, heading + side + rng.Range(-28f, 28f), new Shot(BulletStyle.BeadRed, rng.Range(55f, 130f)).Sized(0.9f));
        }
    }

    /// <summary>A volley: `Count` parallel slashes, the middle one through the boat (or at random, or alternating from side to side), each warned of before it strikes.</summary>
    private static void Volley(BossSpecials s, PlayerEntity player, List<SlashData> slashes, SeededGenerator rng)
    {
        var spec = s.Slashes;
        s.Volleys++;
        float sign = spec.Aim == SlashAim.Alternate && s.Volleys % 2 == 0 ? -1 : 1;
        float angle = spec.Aim == SlashAim.Anywhere ? rng.Range(0f, MathF.Tau) : Mathx.Radians(spec.Angle) * sign;
        var dir = new Vector2(MathF.Sin(angle), MathF.Cos(angle));
        // the point the middle slash goes through
        Vector2 through = spec.Aim switch
        {
            SlashAim.Player => player.Transform.Position + new Vector2(rng.Range(-0.5f, 0.5f) * spec.Spacing, 0),
            SlashAim.Random => new Vector2(rng.Range(160f, 440f), rng.Range(250f, 550f)),
            _ => new Vector2((s.Volleys % 2 == 0 ? 190 : 410) + rng.Range(-30f, 30f), 400),
        };
        if (spec.Aim == SlashAim.Anywhere)
        {
            // from any direction, always through the boat and across the whole field: the line is long enough to leave the field at both ends from wherever the boat is
            var at = new Vector2(Math.Clamp(player.Transform.Position.X, 20, 580), Math.Clamp(player.Transform.Position.Y, 20, 755));
            var across0 = new Vector2(dir.Y, -dir.X);
            for (int k = 0; k < spec.Count; k++)
            {
                float offset = (k - (spec.Count - 1) / 2f) * spec.Spacing;
                // a cut beside the middle one that would pass outside the field (the boat at the edge, the cuts along it) goes to the other side of it
                if (!CrossesField(at + across0 * offset, dir)) offset = -offset;
                if (!CrossesField(at + across0 * offset, dir)) offset = 0;
                slashes.Add(new SlashData { Top = at + across0 * offset - dir * 1100, Angle = angle, Telegraph = spec.Telegraph, Strike = spec.Strike, Width = spec.Width, Leader = k == 0, Len = 2200, Trail = spec.Trail });
            }
            return;
        }
        const float topY = -40;
        // A slash that is nearly level (a wall of water) begins off the side of the field instead of above it.
        var middleTop = MathF.Abs(dir.Y) < 0.35f ? through - dir * 700 : through - dir * ((through.Y - topY) / dir.Y);
        var across = new Vector2(dir.Y, -dir.X);           // along the row of slashes, perpendicular to them
        for (int k = 0; k < spec.Count; k++)
        {
            float offset = (k - (spec.Count - 1) / 2f) * spec.Spacing;
            slashes.Add(new SlashData { Top = middleTop + across * offset, Angle = angle, Telegraph = spec.Telegraph, Strike = spec.Strike, Width = spec.Width, Leader = k == 0, Trail = spec.Trail });
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
