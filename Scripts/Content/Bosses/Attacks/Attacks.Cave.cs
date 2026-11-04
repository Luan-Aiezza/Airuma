using Godot;

/// <summary>
/// The attacks of the cave (stage 4, Ukhumo, the hungry bear girl, who never leaves the rock platform where she is trapped: she cannot swim). They are the attacks
/// of an animal, of claws and of brute force, and each is different from the others in what it asks of the player, not only in its bullets:
///   the claws        three red slashes cut down the field, warned of by a thin line (<see cref="SlashSpec"/>);
///   the roar         she roars and the boat is pushed away from her, into the bullets (<see cref="RoarSpec"/>);
///   the quake        she stamps, the whole screen shakes and stalactites fall from the ceiling (<see cref="QuakeSpec"/>);
///   the boulder      a great stone is thrown that breaks in the air into a ring of small ones (<see cref="BulletBehavior.Burst"/>).
/// </summary>
public static partial class Attacks
{
    private static readonly BulletStyle[] ClawStyles = { BulletStyle.Claw };

    /// <summary>
    /// Three red slashes cut down the field, each warned of by a thin line; and, all round her, rings of claw marks go out of her, turning from one volley to the next (nothing else: no fans at the boat).
    /// </summary>
    public static BossPhase ClawRake() => Attack("AttackClawRake", BossMovementType.Stationary, Centre,
        slashes: new SlashSpec { IsActive = true, Interval = 2.6f, Telegraph = 0.95f, Strike = 0.22f, Width = 34, Spacing = 155, Count = 3, Angle = 0, Aim = SlashAim.Player },
        patterns: new[]
        {
            Pat.Every(0.9f, c => Pat.Ring(c, c.Origin, 14, c.Count * 9f * (c.Count % 2 == 0 ? 1 : -1), new Shot(BulletStyle.Claw, c.Count % 2 == 0 ? 140 : 105).Sized(1.0f))),
        })
        .Pressured(1.2f);

    /// <summary>
    /// The roar: every few seconds she roars and the boat is thrown back from her, into the rings of claw marks that spread from her; and now and then she hurls three great boulders covered with moss at the boat,
    /// in an arc: they are three times as big as the boat and they draw apart on the way, so that there is room to pass between them.
    /// </summary>
    public static BossPhase RoarOfTheMountain() => Attack("AttackRoar", BossMovementType.Stationary, Centre,
        roar: new RoarSpec { IsActive = true, Interval = 3.6f, Push = 240, Seconds = 0.55f },
        patterns: new[]
        {
            Pat.Every(1.6f, c => Pat.Ring(c, c.Origin, 16, c.Count * 11f, new Shot(BulletStyle.Claw, 120).Sized(1.0f).Popping(160, 0.2f))),
            // three great boulders, one at the boat and one at each side, 42 degrees apart: by the time they reach the boat they are 330 px apart (the gap between two of them is 130 px wider than the boat)
            Pat.Every(4.2f, c =>
            {
                foreach (int k in new[] { -1, 0, 1 })
                    c.Fire(c.Origin, c.Aim + k * 42, new Shot(BulletStyle.MossBoulder, 95).Hitting(100).Steady().Heavy());
            }).After(1.0f),
        })
        .Pressured(1.8f);

    /// <summary>
    /// The stamp: she strikes the ground, the whole screen shakes for a few seconds and GIANT stalactites fall from the ceiling, slowly (each a spike of 64 x 160 px, drawn with care); between the quakes, green
    /// bullets at the boat that break, in the air, into rings of smaller green ones.
    /// </summary>
    public static BossPhase Stomp() => Attack("AttackStomp", BossMovementType.Stationary, Centre,
        quake: new QuakeSpec { IsActive = true, Interval = 3.2f, Duration = 3.4f, Intensity = 1f },
        patterns: new[]
        {
            Pat.Every(0.42f, c => Pat.Rain(c, Edge.Top, 2, 4, new Shot(BulletStyle.GiantStalactite, 170).Hitting(26), scale: false)).OnQuake(),
            Pat.Every(1.6f, c => c.Fire(c.Origin, c.Aim + PatternContext.Rand(-14, 14), new Shot(BulletStyle.OrbGreen, 160).Zoomed(2.2f).Hitting(18).Splitting(0.9f, 8, BulletStyle.BeadGreen, 120, childScale: 0.9f))),
        })
        .Pressured(2.0f);

    /// <summary>Brute force: single heavy cuts, one after the other, from any direction: from above, from the sides, on the slant, each warned of by a thin line before it strikes. There are no stones in this attack. After each cut, the line where it cut lets go of a trail of small red bullets that spread out; and, far from the cuts, a slow ring of claw marks that stops and flies at the boat.</summary>
    public static BossPhase BruteForce() => Attack("AttackBruteForce", BossMovementType.Stationary, Centre,
        slashes: new SlashSpec { IsActive = true, Interval = 2.8f, Telegraph = 0.9f, Strike = 0.25f, Width = 46, Spacing = 0, Count = 1, Angle = 0, Aim = SlashAim.Anywhere, Trail = true },
        patterns: new[]
        {
            Pat.Every(3.6f, c => StopAndAimRing(c, c.Origin, 10, c.Count * 17f, ClawStyles, 130, 0.9f, 150)).After(1.4f),
        })
        .Pressured(2.0f);

    /// <summary>Fury, the claws crossed: triple cuts, three parallel cuts at a time, come from every direction, one volley after another, over layered fans and rings of claw marks.</summary>
    public static BossPhase CrossingClaws() => Attack("AttackCrossingClaws", BossMovementType.Stationary, Centre,
        slashes: new SlashSpec { IsActive = true, Interval = 2.4f, Telegraph = 0.9f, Strike = 0.2f, Width = 30, Spacing = 150, Count = 3, Angle = 30, Aim = SlashAim.Anywhere },
        patterns: new[]
        {
            Pat.Every(2.0f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 4, 2, 0, 17, 230, 165, new Shot(BulletStyle.Claw, 0).Popping())),
            Pat.Every(1.3f, c => Pat.Ring(c, c.Origin, 14, c.Count * 8f * (c.Count % 2 == 0 ? 1 : -1), new Shot(BulletStyle.Claw, 115).Sized(1.0f))).After(0.6f),
        })
        .Pressured(1.8f);

    /// <summary>Fury, the cave comes down: the screen shakes almost without a pause, stalactites fall in a crowd and great stones fall that break in a ring when they have fallen a while; now and then a roar throws the boat back.</summary>
    public static BossPhase CaveIn() => Attack("AttackCaveIn", BossMovementType.Stationary, Centre,
        quake: new QuakeSpec { IsActive = true, Interval = 1.0f, Duration = 4.4f, Intensity = 1.4f },
        roar: new RoarSpec { IsActive = true, Interval = 5.0f, Push = 210, Seconds = 0.5f },
        patterns: new[]
        {
            Pat.Every(0.16f, c => Pat.Rain(c, Edge.Top, 1, 4, new Shot(BulletStyle.Stalactite, 300).Sized(1.3f), scale: false)).OnQuake(),
            Pat.Every(1.1f, c => Pat.Rain(c, Edge.Top, 1, 0, new Shot(BulletStyle.Rock, 180).Zoomed(2.0f).Hitting(16).Splitting(0.9f, 6, BulletStyle.Rock, 110, childScale: 0.9f), scale: false)).OnQuake(),
        })
        .Pressured(3.0f);

    /// <summary>
    /// Fury, the feast: everything at once. Great bites that go for the boat, thick rings of red claw marks, layered fans of red claws, four slanting cuts at a time, short quakes and a roar. No stones.
    /// </summary>
    public static BossPhase Feast() => Attack("AttackFeast", BossMovementType.Stationary, Centre,
        slashes: new SlashSpec { IsActive = true, Interval = 2.3f, Telegraph = 0.85f, Strike = 0.22f, Width = 30, Spacing = 140, Count = 4, Angle = 12, Aim = SlashAim.Alternate },
        quake: new QuakeSpec { IsActive = true, Interval = 4.5f, Duration = 2.2f, Intensity = 1.1f },
        roar: new RoarSpec { IsActive = true, Interval = 7.0f, Push = 220, Seconds = 0.5f },
        patterns: new[]
        {
            // the great bites: three at a time, at the boat, in a fan; red like the claw marks
            Pat.Every(2.4f, c => Pat.Arc(c, c.Origin, 3, c.Aim, 36, new Shot(BulletStyle.Bite, 175).Hitting(30), scale: false)).After(0.6f),
            Pat.Every(1.4f, c => Pat.Ring(c, c.Origin, 14, c.Count * 9f * (c.Count % 2 == 0 ? 1 : -1), new Shot(BulletStyle.Claw, 115).Sized(1.0f))),
            Pat.Every(2.8f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 4, 2, 0, 17, 225, 160, new Shot(BulletStyle.Claw, 0).Popping())).After(1.6f),
        })
        .Pressured(2.0f);
}
