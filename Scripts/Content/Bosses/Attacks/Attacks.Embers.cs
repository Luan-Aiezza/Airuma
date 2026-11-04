using Godot;

/// <summary>
/// The attacks of fire and the serpent (stage 1, Iryssh), made as the later bosses' are: a few patterns at once (see Docs/Danmaku.md), gaps for the boat (<see cref="Pat.MinGap"/>), and the ways of shooting
/// of Touhou 6 (Docs/Touhou6.md): rings that burst out and stop and then fly at the boat, layered fans, petals that brake and turn, serpents that bend. The colours are those of fire: red, orange and yellow.
/// </summary>
public static partial class Attacks
{
    private static readonly BulletStyle[] FireOrbs = { BulletStyle.OrbRed, BulletStyle.OrbOrange, BulletStyle.OrbYellow };

    /// <summary>Three arms of fire spiral out of her as she goes from station to station, and, now and then, a slow ring of orbs bursts out, stops and flies, all together, at the boat.</summary>
    public static BossPhase RingOfEmbers() => Attack("AttackRingOfEmbers", BossMovementType.Patrol, Centre, patterns: new[]
    {
        Pat.Every(0.3f, c => Pat.Ring(c, c.Origin, 3, c.Count * 8.5f, new Shot(BulletStyle.Fireball, 135).Sized(0.9f))),
        Pat.Every(3.4f, c => StopAndAimRing(c, c.Origin, 12, c.Count * 15f, FireOrbs, 150, 0.9f, 165)).After(1.2f),
    })
        .Pressured(0.51f);

    /// <summary>A lotus: the petals burst out in a ring, brake to a stop and then fly, all together, at the boat; between the rings, a layered fan of embers.</summary>
    public static BossPhase BlazingBloom() => Attack("AttackBlazingBloom", BossMovementType.Patrol, Centre, patterns: new[]
    {
        Pat.Every(2.6f, c => Pat.Shoot(c, c.Origin, Pat.Aim.Circle, 14, 1, c.Count * 13f, 0, 250, 250, new Shot(BulletStyle.LotusPetal, 0) { Anchor = c.Origin }.Sized(1.1f).Popping().Turning(3, 0.9f, 1, 0, 190))),
        Pat.Every(2.6f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 3, 2, 0, 20, 220, 150, new Shot(BulletStyle.Fireball, 0).Sized(0.9f).Popping())).After(1.3f),
    })
        .Pressured(1.37f);

    /// <summary>The serpent: great magma orbs at the boat, in a fan that opens out (the first of the three pairs in layers), and a thin spiral of serpents of fire that bend as they go.</summary>
    public static BossPhase FuryOfTheRiver() => Attack("AttackFuryOfTheRiver", BossMovementType.Stationary, new Vector2(300, 150), patterns: new[]
    {
        Pat.Every(2.4f, c => Pat.Arc(c, c.Origin, 3, c.Aim, 40, new Shot(BulletStyle.MagmaOrb, 190).Sized(2.2f).Hitting(16))),
        Pat.Every(0.34f, c => Pat.Ring(c, c.Origin, 4, c.Count * 11f, new Shot(BulletStyle.FireSerpent, 140).Sized(0.9f).Curving(c.Count % 2 == 0 ? 30 : -30).EasingOutAfter(1.5f))),
    })
        .Pressured(1.2f);

    /// <summary>The serpent lunges down the screen from one of three lanes, leaving embers in its wake (none on easy; see EmbersOneStepDown); at the boat, slow rings of embers that stop and fly.</summary>
    public static BossPhase TideOfEmbers()
    {
        var attack = Attack("AttackTideOfEmbers", BossMovementType.Dash, new Vector2(300, -50), startsAtTarget: true,
            main: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 0.26f, BulletsPerVolley = 8,
                SpreadAngle = 360, BulletSpeed = 100, SpinRate = 15, BulletScale = 0.5f, BulletStyle = BulletStyle.Ember,
            }, embersOneStepDown: true, patterns: new[]
            {
                Pat.Every(3.6f, c => StopAndAimRing(c, new Vector2(c.MiddleX + PatternContext.Rand(-120, 120), 150), 10, c.Count * 17f, FireOrbs, 40, 0.9f, 160)).After(1.5f),
            })
            .Pressured(0.5f);
        return attack;
    }
}
