using System.Linq;
using Godot;

/// <summary>
/// The attacks of fire and the serpent (stage 1, Iryssh), made as the later bosses' are: a few patterns at once (see Docs/Danmaku.md), gaps for the boat (<see cref="Pat.MinGap"/>), and the ways of shooting
/// of Touhou 6 (Docs/Touhou6.md): rings that burst out and stop and then fly at the boat, layered fans, petals that brake and turn, serpents that bend. The colours are those of fire: red, orange and yellow.
/// </summary>
public static partial class Attacks
{
    // The health of the attacks of the Iryssh, chosen so that a boat at the most that stage 1 allows (the straight shot at level 2, focused: 20 x 1.4 = 28 damage a second, every shot a hit) still sees
    // each attack go round at least once or twice before it ends: the ring every 3.4 s (two rounds: 8 s), the lotus every 2.6 s (about two and a half: 7 s), a great fireball that flies, pulses two
    // seconds and breaks (two lives of it: 9 s), a lunge and its curtains (one and a half rounds of 5.3 s: 6.8 s) and the tail (two rounds of its shots across the middle: 14 s).
    private const float RingHealth = 1.6f, LotusHealth = 1.39f, FireballHealth = 0.87f, LungeHealth = 1.2f, TailHealth = 1.25f;
    private static readonly BulletStyle[] FireOrbs = { BulletStyle.OrbRed, BulletStyle.OrbOrange, BulletStyle.OrbYellow };

    /// <summary>Three arms of fire spiral out of her as she goes from station to station, and, now and then, a slow ring of orbs bursts out, stops and flies, all together, at the boat.</summary>
    public static BossPhase RingOfEmbers() => Attack("AttackRingOfEmbers", BossMovementType.Patrol, Centre, patterns: new[]
    {
        Pat.Every(0.3f, c => Pat.Ring(c, c.Origin, 3, c.Count * 8.5f, new Shot(BulletStyle.Fireball, 135).Sized(0.9f))),
        Pat.Every(3.4f, c => StopAndAimRing(c, c.Origin, 12, c.Count * 15f, FireOrbs, 150, 0.9f, 165)).After(1.2f),
    })
        .Pressured(0.51f).Tougher(RingHealth);

    /// <summary>A lotus: the petals burst out in a ring, brake to a stop and then fly, all together, at the boat; between the rings, a layered fan of embers.</summary>
    public static BossPhase BlazingBloom() => Attack("AttackBlazingBloom", BossMovementType.Patrol, Centre, patterns: new[]
    {
        Pat.Every(2.6f, c => Pat.Shoot(c, c.Origin, Pat.Aim.Circle, 14, 1, c.Count * 13f, 0, 250, 250, new Shot(BulletStyle.LotusPetal, 0) { Anchor = c.Origin }.Sized(1.1f).Popping().Turning(3, 0.9f, 1, 0, 190))),
        Pat.Every(2.6f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 3, 2, 0, 20, 220, 150, new Shot(BulletStyle.Fireball, 0).Sized(0.9f).Popping())).After(1.3f),
    })
        .Pressured(1.37f).Tougher(LotusHealth);

    /// <summary>The serpent: great magma orbs at the boat, in a fan that opens out (the first of the three pairs in layers), and a thin spiral of serpents of fire that bend as they go.</summary>
    public static BossPhase FuryOfTheRiver() => Attack("AttackFuryOfTheRiver", BossMovementType.Stationary, new Vector2(300, 150), patterns: new[]
    {
        Pat.Every(2.4f, c => Pat.Arc(c, c.Origin, 3, c.Aim, 40, new Shot(BulletStyle.MagmaOrb, 190).Sized(2.2f).Hitting(16))),
        Pat.Every(0.34f, c => Pat.Ring(c, c.Origin, 4, c.Count * 11f, new Shot(BulletStyle.FireSerpent, 140).Sized(0.9f).Curving(c.Count % 2 == 0 ? 30 : -30).EasingOutAfter(1.5f))),
    })
        .Pressured(1.2f);

    /// <summary>The places where the great fireballs break, as a share of the width of the field (x) and px of height (y): the four corners, the two sides and the middle of the bottom.</summary>
    private static readonly (float X0, float X1, float Y0, float Y1)[] FireballZones =
    {
        (0.00f, 0.18f, 180, 330), (0.82f, 1.00f, 180, 330),     // the top corners
        (0.00f, 0.18f, 360, 520), (0.82f, 1.00f, 360, 520),     // the sides, lower
        (0.05f, 0.30f, 540, 640), (0.70f, 0.95f, 540, 640),     // the bottom corners
        (0.38f, 0.62f, 520, 620),                               // the middle of the bottom
    };
    private static int _lastFireballZone = -1;

    /// <summary>How long a great fireball waits, stopped and pulsing, before it breaks.</summary>
    public const float PulseSeconds = 2f;

    /// <summary>
    /// Great fireballs thrown at random places of the scene: each flies to a point of the field that is chosen by chance, stops there for two seconds, pulsing (it throbs, with a sound, quicker as it goes) and then breaks into a ring of smaller fireballs. The boss stands still at the top
    /// and the fireballs make no figure: the boat reads them one by one, by where they are going to break.
    /// </summary>
    public static BossPhase ShatteringFireballs() => Attack("AttackShatteringFireballs", BossMovementType.Stationary, new Vector2(300, 150), patterns: new[]
    {
        Pat.Every(0.85f, c =>
        {
            // where it will break: one of the corners and edges of the scene, never the same twice in a row, anywhere inside the field (the sides that open in the fight count)
            int zone;
            do zone = (int)PatternContext.Rand(0, FireballZones.Length - 0.001f); while (zone == _lastFireballZone);
            _lastFireballZone = zone;
            var (x0, x1, y0, y1) = FireballZones[zone];
            var target = new Vector2(c.Left + 40 + (c.Width - 80) * PatternContext.Rand(x0, x1), PatternContext.Rand(y0, y1));
            const float speed = 150;
            float fly = System.Math.Max(0.5f, c.Origin.DistanceTo(target) / speed);        // how long it takes to get there; then it stops and pulses for two seconds before it breaks
            float heading = Mathx.Degrees(System.MathF.Atan2(target.Y - c.Origin.Y, target.X - c.Origin.X));
            c.Fire(c.Origin, heading, new Shot(BulletStyle.MagmaOrb, speed).Zoomed(2.0f).Hitting(22).Steady().Bursting(48).StoppingToPulse(fly).Splitting(fly + PulseSeconds, c.N(6, 5), BulletStyle.Fireball, 105, childScale: 0.8f));
        }),
    })
        .Pressured(1.0f).Tougher(FireballHealth);

    /// <summary>
    /// The serpent lunges down the screen from one of three lanes, as before, and leaves TWO CURTAINS of great fireballs, one to the right of her way and one to the left, only while she lunges. Each curtain is a wall
    /// hinged at the bottom of the field that FALLS to its own side: the top of it, where the attack began, goes first, down and sideways along a great arc, and the lower the fireball the less far it goes (the foot of the curtain falls too, a little later), so that
    /// the fireballs draw apart from each other more and more as it falls (the boat can go through the gaps), and the curtains of the lunges that follow gather on the scene with those that are still there.
    /// </summary>
    public static BossPhase TideOfEmbers() => Attack("AttackTideOfEmbers", BossMovementType.Dash, new Vector2(300, -50), startsAtTarget: true, patterns: new[]
    {
        Pat.Every(0.03f, c =>
        {
            var dash = c.Boss.Dash;
            if (c.LungeTime <= 0) { dash.TrailY = float.NaN; return; }          // (nothing while she comes down to her place and waits)
            if (float.IsNaN(dash.TrailY)) dash.TrailY = c.Origin.Y - TrailSpacing;
            if (c.Origin.Y - dash.TrailY < TrailSpacing || c.Origin.Y > CurtainHingeY - 50) return;       // (the curtain is made until she is out of the screen)
            dash.TrailY = c.Origin.Y;
            if (c.Difficulty == Difficulty.Easy && c.Count % 3 == 2) return;    // (easy: a curtain with more room between its fireballs)
            var fall = new Shot(BulletStyle.Fireball, 0).Sized(1.7f).Hitting(16).Steady();
            FireCurtainPair(c, 20, 1f, fall);
            // on hard, a second, flatter curtain inside the first, of embers
            if (c.Difficulty == Difficulty.Hard && c.Count % 2 == 0) FireCurtainPair(c, 40, 0.7f, new Shot(BulletStyle.Ember, 0).Sized(1.2f).Steady());
        }),
    })
        .Pressured(1.0f).Tougher(LungeHealth);

    /// <summary>Where the curtains of the lunge are hinged: a little below the bottom of the field (she leaves the screen at about 800).</summary>
    private const float CurtainHingeY = 850;

    /// <summary>
    /// A fireball at each side of her way, each on its own arc round the hinge of its curtain (the foot of her way): the farther it is from the hinge, the quicker it turns, so that the curtain bends as it falls.
    /// `pace` makes the whole fall quicker or slower.
    /// </summary>
    private static void FireCurtainPair(PatternContext c, float offset, float pace, in Shot shot)
    {
        var hinge = new Vector2(c.Origin.X, CurtainHingeY);
        float h = CurtainHingeY - c.Origin.Y;                                  // how high above the hinge it is let go
        float omega = pace * (12f + 10f * (h / 900f) * (h / 900f));             // degrees a second: 12 at the foot (it falls too, though later and not far), 22 at the very top
        foreach (int side in new[] { 1, -1 })
        {
            var at = new Vector2(c.Origin.X + side * offset, c.Origin.Y);
            float radius = at.DistanceTo(hinge), theta = Mathx.Degrees(System.MathF.Atan2(at.Y - hinge.Y, at.X - hinge.X));
            c.FirePolar(hinge, radius, 0, theta, side * omega, shot);          // to the right it turns one way, to the left the other
        }
    }

    /// <summary>Px that she moves between one pair of the curtain and the next: with the size of the fireballs it leaves 16 px between two of them (the gaps rule).</summary>
    private const float TrailSpacing = 48;

    /// <summary>
    /// The tail of the serpent of hell: she goes back to the middle of the field, her skirt on the line of the horizon of the water (as the others of her kind stand), and calls down two GIANT lasers of fire at the sides of the field, that cover most of it; from their inner edges, bullets like those of firearms fly
    /// across the narrow space between them at heights that seem random but that, in turn, take in every height of the field, those of the left laser to the right, those of the right laser to the left. The boat has to hold in the middle, dodging the fireballs while it shoots her.
    /// </summary>
    public static BossPhase SerpentTail() => Attack("AttackSerpentTail", BossMovementType.Stationary, new Vector2(300, Layout.Horizon - BossStyles.Iryssh.Waterline), patterns: new[]
    {
        // 1) the two lasers (once; they last the whole attack)
        Pat.Every(1000, c =>
        {
            // each covers from the edge of the scene (beyond the sides that open in the fight) to the edge of the space in the middle
            float outer = World.FieldExtraMax + 40;
            Pat.Laser(c, new Vector2((-outer + LaserInnerLeft) / 2, -20), 90, LaserInnerLeft + outer, 1.8f, 900, LaserFire);
            Pat.Laser(c, new Vector2((LaserInnerRight + 600 + outer) / 2, -20), 90, 600 + outer - LaserInnerRight, 1.8f, 900, LaserFire);
        }).After(0.5f),
        // 2) fireballs from the left laser to the right, and from the right to the left, at random heights
        Pat.Every(0.52f, c => LaserFireball(c, LaserInnerLeft + 8, 0)).After(2.2f),
        Pat.Every(0.52f, c => LaserFireball(c, LaserInnerRight - 8, 180)).After(2.5f),
    })
        .Pressured(1.0f)
        .Tougher(TailHealth);

    /// <summary>Where the two lasers end towards the middle: the space between them stays 280 px wide.</summary>
    private const float LaserInnerLeft = 160, LaserInnerRight = 440;
    private static readonly Color LaserFire = new Color(1f, 0.5f, 0.14f);

    /// <summary>The heights (px) between which the shots of the lasers cross the space in the middle: from under the boss to the bottom of the field, where the boat may stand.</summary>
    private const float LaserShotTop = 190, LaserShotBottom = 765;
    private const int LaserShotSlots = 12;
    private static readonly System.Collections.Generic.Queue<int>[] LaserBags = { new(), new() };

    /// <summary>
    /// A bullet that leaves the inner edge of a laser across the space in the middle (the way of a firearm's: see <see cref="BulletStyle.FireSlug"/>). Its height seems random, but it is taken from a bag of slots,
    /// that is shuffled again each time it is empty: every height of the field gets its turn, so that nowhere (not the bottom either) is safe for long.
    /// </summary>
    private static void LaserFireball(PatternContext c, float x, float heading)
    {
        var bag = LaserBags[heading == 0 ? 0 : 1];
        int n = c.N(1, 1);
        for (int k = 0; k < n; k++)
        {
            if (bag.Count == 0)
            {
                var order = System.Linq.Enumerable.Range(0, LaserShotSlots).OrderBy(_ => PatternContext.Rand(0, 1)).ToList();
                foreach (int slot in order) bag.Enqueue(slot);
            }
            int slotNow = bag.Dequeue();
            float slotHeight = (LaserShotBottom - LaserShotTop) / LaserShotSlots;
            float y = LaserShotTop + slotHeight * (slotNow + PatternContext.Rand(0.15f, 0.85f));
            c.Fire(new Vector2(x, y), heading, new Shot(BulletStyle.FireSlug, 140).Sized(1.3f).Hitting(8));
        }
    }
}
