using System;
using Godot;

/// <summary>The spirits: how they move along their paths and how they fire their patterns.</summary>
public static class EnemySystem
{
    /// <summary>Enemies are removed once this far outside the play area (after they have had time to enter).</summary>
    public const float Margin = 90;

    // Movement

    public static void Move(EnemyEntity e, float delta)
    {
        e.Age += delta;
        e.HitFlash = MathF.Max(0, e.HitFlash - delta);
        Vector2 before = e.Position;

        switch (e.Path.Kind)
        {
            case EnemyPathKind.Straight:
                e.Position += e.Path.Velocity * delta;
                break;

            case EnemyPathKind.Sine:
            {
                float y = e.Origin.Y + e.Path.Speed * e.Age;
                float x = e.Origin.X + e.Path.Amplitude * MathF.Sin(e.Path.Frequency * e.Age + e.Phase);
                e.Position = new Vector2(x, y);
                break;
            }

            case EnemyPathKind.Arc:
                e.Position += e.Path.Velocity.Rotated(Mathx.Radians(e.Path.Turn * e.Age)) * delta;
                break;

            case EnemyPathKind.Dive:
                if (!e.Arrived)
                {
                    e.Position = e.Position.MoveToward(e.Path.Target, e.Path.Speed * delta);
                    if (e.Position == e.Path.Target) e.Arrived = true;
                }
                else if (e.HoldClock < e.Path.Hold)
                {
                    e.HoldClock += delta;
                }
                else
                {
                    e.Position += e.Path.Exit * delta;
                }
                break;
        }
        e.Velocity = delta > 0 ? (e.Position - before) / delta : Vector2.Zero;
    }

    public static bool IsOffScreen(EnemyEntity e, Rect2 bounds)
    {
        if (!(e.Age > 1)) return false;
        return e.Position.X < bounds.Position.X - Margin || e.Position.X > bounds.End.X + Margin
            || e.Position.Y < bounds.Position.Y - Margin || e.Position.Y > bounds.End.Y + Margin;
    }

    // Shooting

    /// <summary>Enemies only shoot while they are inside the play area.</summary>
    public static void Fire(EnemyEntity e, Vector2 target, BulletData[] pool, Rect2 bounds, float delta, Difficulty difficulty = Difficulty.Normal)
    {
        if (e.Patterns.Count == 0 || !(e.Age > 0.25f)
            || !(e.Position.X > bounds.Position.X) || !(e.Position.X < bounds.End.X)
            || !(e.Position.Y > bounds.Position.Y) || !(e.Position.Y < bounds.End.Y))
            return;

        // Move on to the next pattern when this one has run its course.
        e.PatternClock += delta;
        if (e.PatternClock >= e.Patterns[e.PatternIndex].Duration)
        {
            e.PatternClock = 0;
            e.PatternIndex = (e.PatternIndex + 1) % e.Patterns.Count;
            e.ShotClock = MathF.Min(e.ShotClock, 0);
        }

        FirePattern pattern = e.Patterns[e.PatternIndex].Pattern;
        e.ShotClock += delta;
        if (!(e.ShotClock >= pattern.Interval)) return;
        e.ShotClock -= pattern.Interval;

        float aim = Mathx.Degrees(MathF.Atan2(target.Y - e.Position.Y, target.X - e.Position.X));
        switch (pattern.Kind)
        {
            case FirePatternKind.None:
                break;

            case FirePatternKind.Aimed:
                e.Shots++;
                // Easy: every second shot is left out. Hard: the shot becomes a three-way volley.
                if (difficulty == Difficulty.Easy && e.Shots % 2 == 0) break;
                Shoot(pool, e.Position, aim, pattern.Speed, BulletStyle.OrbGreen);
                if (difficulty == Difficulty.Hard)
                {
                    Shoot(pool, e.Position, aim - 14, pattern.Speed, BulletStyle.OrbGreen);
                    Shoot(pool, e.Position, aim + 14, pattern.Speed, BulletStyle.OrbGreen);
                }
                break;

            case FirePatternKind.Ring:
            {
                e.Spin += pattern.Spin;
                int count = difficulty.Scale(pattern.Count, 4);
                for (int i = 0; i < count; i++)
                    Shoot(pool, e.Position, e.Spin + 360 * (float)i / count, pattern.Speed, BulletStyle.OrbRed);
                // Hard: one more bullet in every ring, straight at the boat.
                if (difficulty == Difficulty.Hard) Shoot(pool, e.Position, aim, pattern.Speed * 1.15f, BulletStyle.OrbRed);
                break;
            }

            case FirePatternKind.Fan:
            {
                int count = difficulty.Scale(pattern.Count, 2);
                for (int i = 0; i < count; i++)
                {
                    float t = count > 1 ? (float)i / (count - 1) - 0.5f : 0;
                    Shoot(pool, e.Position, aim + t * pattern.Spread, pattern.Speed, BulletStyle.RiceRed);
                }
                break;
            }

            case FirePatternKind.Spiral:
            {
                e.Spin += pattern.Step;
                // Easy: one arm fewer; Hard: one arm more.
                int arms = difficulty == Difficulty.Easy ? Math.Max(1, pattern.Arms - 1) : (difficulty == Difficulty.Hard ? pattern.Arms + 1 : pattern.Arms);
                for (int i = 0; i < arms; i++)
                    Shoot(pool, e.Position, e.Spin + 360 * (float)i / arms, pattern.Speed, BulletStyle.RiceRed);
                break;
            }
        }
    }

    private static void Shoot(BulletData[] pool, Vector2 position, float angle, float speed, BulletStyle style)
    {
        float a = Mathx.Radians(angle);
        BulletSystem.SpawnEnemyBullet(pool, position, new Vector2(MathF.Cos(a), MathF.Sin(a)) * speed, style);
    }
}
