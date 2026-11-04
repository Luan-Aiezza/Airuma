using Godot;

/// <summary>
/// The attacks of the river of stars (stage 6, Mama Killa, the Moon): the phases of the moon, from the new moon to the eternal night. Ten attacks (six calm, four enraged) made of what was learnt in the
/// stages before: the gaps rule (see <see cref="Pat.MinGap"/>), few patterns at once (1 to 2 in the first, 2 in the middle, 2 to 3 in the last, 4 in the very last), slow bullets in generous figures, and the
/// mechanics taken from Touhou 6 (see Docs/Touhou6.md): rings that stop and fly at the boat, pinwheels, galaxies, lasers that turn and go transparent. The colours are those of the night: silver, pale blue,
/// violet and the gold of the stars. She stands on the horizon (see <see cref="KillaBoss"/>): where it says "path", she does not move.
/// </summary>
public static partial class Attacks
{
    private static readonly BulletStyle[] MoonOrbs = { BulletStyle.OrbWhite, BulletStyle.OrbBlue, BulletStyle.OrbViolet, BulletStyle.OrbCyan };
    private static readonly BulletStyle[] SilverOrbs = { BulletStyle.OrbWhite, BulletStyle.OrbCyan };
    private static readonly BulletStyle[] VioletOrbs = { BulletStyle.OrbViolet, BulletStyle.OrbBlue };
    private static readonly BulletStyle[] NightStars = { BulletStyle.StarBlue, BulletStyle.StarGold, BulletStyle.StarViolet };
    private static readonly Color[] MoonHues = { new Color(0.88f, 0.94f, 1f), new Color(0.58f, 0.74f, 1f), new Color(0.76f, 0.6f, 1f) };

    private static Vector2 LeftHand(PatternContext c) => c.Origin + new Vector2(-48, -34);
    private static Vector2 RightHand(PatternContext c) => c.Origin + new Vector2(48, -34);

    /// <summary>A texture layer of the night: slow stars that fall at random places.</summary>
    private static PatternSpec FallingStars(float interval, int n = 1, float speed = 105) =>
        Pat.Every(interval, c => Pat.Rain(c, Edge.Top, n, 8, new Shot(NightStars[c.Count % 3], speed).Sized(0.95f)));

    // MARK: The first form: the moon grows

    /// <summary>The new moon: no moon in the sky, only slow stars that fall, and rings of silver that stop and then fly, all together, at the boat.</summary>
    public static BossPhase KillaNewMoon() => Attack("AttackKillaNewMoon", BossMovementType.Roam, Centre, patterns: new[]
    {
        FallingStars(0.4f),
        Pat.Every(2.4f, c => StopAndAimRing(c, c.Origin, 16, c.Count * 9f, c.Count % 2 == 0 ? SilverOrbs : VioletOrbs, 150, 0.9f, 165)),
    })
        .Pressured(2.0f);

    /// <summary>The crescent: horns of silver, two arcs of beads that fly at the boat as one figure, from one side and the other, and layered fans of kunai.</summary>
    public static BossPhase KillaCrescent() => Attack("AttackKillaCrescent", BossMovementType.Roam, Centre, patterns: new[]
    {
        Pat.Every(1.8f, c => Pat.Crescent(c, c.Origin, c.Aim + (c.Count % 2 == 0 ? 14 : -14), 120, 6, new Shot(BulletStyle.BeadWhite, 150).Sized(0.85f))),
        Pat.Every(2.6f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 4, 2, 0, 17, 230, 165, new Shot(BulletStyle.KunaiBlue, 0).Popping())).After(0.9f),
    })
        .Pressured(1.6f);

    /// <summary>The lunar tide (path): rows of hail with a wide gap that swings slowly, like the tide, and a few beads at the boat. She stands still.</summary>
    public static BossPhase KillaTide() => Attack("AttackKillaTide", BossMovementType.Stationary, new Vector2(300, 140), patterns: new[]
    {
        Pat.Every(0.9f, c => Pat.Wall(c, Edge.Top, 24, 0.5f + 0.13f * Pat.Wave(c.Time, 8), 170, new Shot(BulletStyle.Hail, 100).Sized(1.9f))),
        Pat.Every(2.2f, c => Pat.Aimed(c, c.Origin, 3, 24, new Shot(BulletStyle.BeadCyan, 170).Sized(0.95f))).After(0.5f),
    })
        .Pressured(1.0f);

    /// <summary>The full moon: great silver orbs, slow, in rings that turn, and rings of little silver rays.</summary>
    public static BossPhase KillaFullMoon() => Attack("AttackKillaFullMoon", BossMovementType.Roam, Centre, patterns: new[]
    {
        Pat.Every(3.4f, c => Pat.Ring(c, c.Origin, 6, c.Count * 19f, new Shot(BulletStyle.GiantWhite, 85).Sized(1.0f), scale: false)),
        Pat.Every(1.8f, c => Pat.Ring(c, c.Origin, 18, c.Count * -7f, new Shot(BulletStyle.BeadWhite, 125).Sized(0.9f))).After(0.9f),
    })
        .Pressured(3.0f);

    /// <summary>
    /// The constellations: stars that fly out, are linked by thin lines and hang for a while; and a MOON that comes down from her to the middle of the field and, every few seconds, swells, warns and pulls the boat
    /// towards it (the boat has to pull away at full speed; the moon hurts if touched). Pinwheels of blue and violet turn about it.
    /// </summary>
    public static BossPhase KillaConstellations()
    {
        var attack = Attack("AttackKillaConstellations", BossMovementType.Roam, Centre, patterns: new[]
        {
            Pat.Every(4.6f, c => Pat.Constellation(c, c.Origin, 5, c.Count * 23f, 2, new Shot(c.Count % 2 == 0 ? BulletStyle.StarBlue : BulletStyle.StarGold, 175).Constellation(250))),
            Pat.Every(3.0f, c => PinwheelRing(c, c.Origin, 14, c.Count * 11f, MoonOrbs, 130, 0.8f, c.Count % 2 == 0 ? 80 : -80)).After(1.5f),
        })
            .Pressured(2.0f);
        attack.Gravity = new GravitySpec { IsActive = true, Target = new Vector2(300, 420), Arrive = 5f, Interval = 5.5f, Warn = 1.2f, Duration = 2.6f, Pull = 230, Radius = 30 };
        return attack;
    }

    /// <summary>The silver rain (path): fans of five lasers of moonlight at the boat, which go transparent for a moment every two seconds, and a rain of slow stars. She stands still.</summary>
    public static BossPhase KillaSilverRain() => Attack("AttackKillaSilverRain", BossMovementType.Stationary, new Vector2(300, 140), patterns: new[]
    {
        Pat.Every(6.4f, c => { for (int k = -2; k <= 2; k++) Pat.Laser(c, c.Origin, c.Aim + k * 26, 30, 1.2f, 3.4f, MoonHues[(k + 2) % 3], blink: 2); }).After(0.6f),
        FallingStars(0.3f),
    })
        .Pressured(3.0f);

    // MARK: The second form: the moon wanes

    /// <summary>The waning moon: columns of orbs come in from the sides, stop and fall as one curtain from where they stopped; pinwheels of silver; stars.</summary>
    public static BossPhase KillaWaning() => Attack("AttackKillaWaning", BossMovementType.Stationary, Centre, patterns: new[]
    {
        Pat.Every(2.2f, c =>
        {
            bool left = c.Count % 2 == 0;
            float stopAt = PatternContext.Rand(c.Left + 120, c.Right - 120);
            int rows = c.N(6, 4);
            for (int k = 0; k < rows; k++)
            {
                var from = new Vector2(left ? c.Left - 14 : c.Right + 14, 230 + k * 70);
                float speed = 2 * System.Math.Abs(stopAt - from.X) / 1.2f;
                c.Fire(from, left ? 0 : 180, new Shot(MoonOrbs[k % 4], speed).Sized(0.95f).Steady().Turning(2, 1.2f, 1, 90, 150));
            }
        }),
        Pat.Every(2.8f, c => PinwheelRing(c, c.Origin, 16, c.Count * 9f, SilverOrbs, 120, 0.8f, c.Count % 2 == 0 ? 75 : -75)).After(1.0f),
        FallingStars(0.5f),
    })
        .Pressured(3.0f);

    /// <summary>The eclipse (path): seven lasers of moonlight turn slowly about her like the rays of a corona, and go transparent every two seconds; a galaxy of silver and violet; rings that stop and fly at the boat.</summary>
    public static BossPhase KillaEclipse() => Attack("AttackKillaEclipse", BossMovementType.Stationary, Centre, patterns: new[]
    {
        Pat.Every(7.2f, c => { float spin = c.Count % 2 == 0 ? 10 : -10; for (int k = 0; k < 7; k++) Pat.Laser(c, c.Origin, c.Count * 15f + 360f * k / 7, 28, 1.2f, 4.4f, MoonHues[k % 3], spin, follow: true, blink: 2); }).After(0.8f),
        Pat.Every(1.0f, c => Pat.PolarRing(c, c.Origin, 6, c.Count * 12f, 12, 42, 22, new Shot(BulletStyle.OrbWhite, 0).Sized(0.95f), MoonOrbs)),
        Pat.Every(3.2f, c => StopAndAimRing(c, c.Origin, 12, c.Count * 15f, VioletOrbs, 140, 0.8f, 160)).After(1.2f),
    })
        .Pressured(2.6f);

    /// <summary>The lunar pentagram: great pentagrams of stars that fly at the boat as one figure and burst, constellations, and a slow galaxy.</summary>
    public static BossPhase KillaPentagram() => Attack("AttackKillaPentagram", BossMovementType.Roam, Centre, patterns: new[]
    {
        Pat.Every(4.6f, c => AdvancingPentagram(c, c.Aim + (c.Count % 2 == 0 ? 0 : 24), 110, c.Count * 17f, c.Count % 2 == 0 ? BulletStyle.StarBlue : BulletStyle.StarViolet, BulletStyle.StarGold, 2)),
        Pat.Every(4.6f, c => Pat.Constellation(c, c.Origin, 5, c.Count * 29f, 2, new Shot(BulletStyle.Star, 175).Constellation(250))).After(1.9f).NotOnEasy(),
        Pat.Every(1.6f, c => Pat.PolarRing(c, c.Origin, 5, c.Count * 14f, 12, 46, c.Count % 2 == 0 ? 20 : -20, new Shot(BulletStyle.OrbCyan, 0).Sized(0.95f), MoonOrbs)),
    })
        .Pressured(3.0f);

    /// <summary>
    /// The eternal night (path, the last): curtains of hail with two gaps, one the mirror of the other, that sway slowly; a galaxy that turns out of each of her hands, one each way; pinwheels from her heart;
    /// and the slow stars that fall. She stands still.
    /// </summary>
    public static BossPhase KillaEternalNight() => Attack("AttackKillaEternalNight", BossMovementType.Stationary, Centre, patterns: new[]
    {
        Pat.Every(1.0f, c => Pat.Wall(c, Edge.Top, 24, 0.27f + 0.08f * Pat.Wave(c.Time, 7), 150, new Shot(BulletStyle.Hail, 105).Sized(1.9f), mirror: true)),
        Pat.Every(1.1f, c => Pat.PolarRing(c, LeftHand(c), 5, c.Count * 13f, 12, 40, 20, new Shot(BulletStyle.OrbWhite, 0).Sized(0.95f), SilverOrbs)),
        Pat.Every(1.1f, c => Pat.PolarRing(c, RightHand(c), 5, c.Count * -13f, 12, 40, -20, new Shot(BulletStyle.OrbViolet, 0).Sized(0.95f), VioletOrbs)).After(0.55f),
        Pat.Every(3.4f, c => PinwheelRing(c, c.Origin, 16, c.Count * 8f, MoonOrbs, 110, 0.8f, c.Count % 2 == 0 ? 80 : -80)).After(1.3f),
        FallingStars(0.6f, 1, 95),
    })
        .Pressured(1.1f);
}
