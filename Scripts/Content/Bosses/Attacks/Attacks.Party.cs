using Godot;

/// <summary>The attacks of the party (stage 2, Baylo): his bullets dance, sway on a half-second beat, and he dives and fires a great jet.</summary>
public static partial class Attacks
{
    /// <summary>Two rings that turn in opposite senses with neighbours swaying against each other, while the dancer roams the field; between the beats, a fan at the boat.</summary>
    public static BossPhase PartyTwoStep() => Attack("AttackDoubleStep", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat, BulletsPerVolley = 12, SpreadAngle = 360, BulletSpeed = 135, SpinRate = 14,
            CurveRate = 100, AlternateCurve = true, BulletScale = 0.9f, BulletBehavior = BulletBehavior.Dance, BulletStyle = BulletStyle.VenomOrb,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat * 3, BulletsPerVolley = 5, SpreadAngle = 50, BulletSpeed = 210,
            IsAimed = true, BulletScale = 0.9f, BulletStyle = BulletStyle.OrbGreen,
        });

    /// <summary>
    /// Confetti: aimed bursts of streamers that sway as they come, and, from the dancer wherever he goes, pieces of lines of confetti sent out to many places at once: short streaks that point the way they go,
    /// in a wide fan, and short combs across it, at the boat; more pieces of line fall from the top at random places, over a rain of confetti.
    /// </summary>
    public static BossPhase ConfettiRain() => Attack("AttackConfettiRain", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat * 2f, BulletsPerVolley = 11, SpreadAngle = 90, BulletSpeed = 200,
            IsAimed = true, CurveRate = 80, AlternateCurve = true, BulletScale = 1.0f, BulletBehavior = BulletBehavior.Dance, BulletStyle = BulletStyle.RiceRed,
        },
        patterns: new[]
        {
            // pieces of line to many places: six streaks in a fan of 160 degrees, each of four bullets in a row, the fan turned a little every time
            Pat.Every(Pat.Beat * 2.5f, c =>
            {
                int n = c.N(6, 4);
                float turn = (c.Count % 2) * 9;
                for (int k = 0; k < n; k++)
                {
                    float heading = 90 - 80 + turn + 160f * k / (n - 1);
                    Pat.Line(c, c.Origin, c.Origin + Pat.Dir(heading) * 48, 4, heading, new Shot(Beads[(k + c.Count) % Beads.Length], 150).Sized(0.9f));
                }
            }),
            // short combs across the way, three of them at the boat
            Pat.Every(Pat.Beat * 3, c =>
            {
                for (int k = -1; k <= 1; k++)
                {
                    float heading = c.Aim + k * 24;
                    Pat.Comb(c, c.Origin + Pat.Dir(heading) * 30, heading, 5, 16, new Shot(Rices[(c.Count + k + 1) % Rices.Length], 215).Sized(0.9f));
                }
            }).After(Pat.Beat * 0.5f),
            // pieces of line from the top, at random places, slanting
            Pat.Every(Pat.Beat * 4, c =>
            {
                for (int k = 0; k < c.N(3, 2); k++)
                {
                    var at = Pat.EdgePoint(c, Edge.Top, PatternContext.Rand(0.05f, 0.95f));
                    float heading = 90 + PatternContext.Rand(-28, 28);
                    Pat.Line(c, at, at + Pat.Dir(heading) * 70, 5, heading, new Shot(Beads[(c.Count + k) % Beads.Length], 135).Sized(0.9f));
                }
            }).After(Pat.Beat * 0.25f),
            // the rain of confetti
            Pat.Every(0.4f, c => Pat.Rain(c, Edge.Top, 2, 16, new Shot(Beads[c.Count % Beads.Length], 125).Sized(0.8f).Dancing(60))),
        });

    /// <summary>A turning wheel of swaying petals, one volley a beat, and claps: two thin arms of orbs that spin the other way.</summary>
    public static BossPhase SambaWheel() => Attack("AttackSambaWheel", BossMovementType.Roam, Centre,
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat, BulletsPerVolley = 16, SpreadAngle = 360, BulletSpeed = 120, SpinRate = 11,
            CurveRate = 70, AlternateCurve = true, BulletScale = 1.0f, BulletBehavior = BulletBehavior.Dance, BulletStyle = BulletStyle.LotusPetal,
        },
        sub: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 0.1f, BulletsPerVolley = 2, SpreadAngle = 180, BulletSpeed = 150, SpinRate = -17,
            BulletScale = 0.7f, BulletStyle = BulletStyle.OrbGreen,
        });

    /// <summary>
    /// The dolphin dives and a shadow lies under the boat, following it a while and then holding still; he comes out of the water at the
    /// shadow (the dive backwards), with a fan of spray. Ringed bubbles that sway while he is on the surface. Hard adds a three-way at the boat.
    /// </summary>
    public static BossPhase GreatLeap() => Attack("AttackDolphinLeap", BossMovementType.Dive, new Vector2(300, MovementSystem.DiveTopY),
        main: new BulletEmitterComponent
        {
            IsActive = true, FireRate = BulletSystem.DanceBeat, BulletsPerVolley = 10, SpreadAngle = 360, BulletSpeed = 130, SpinRate = 9,
            CurveRate = 90, AlternateCurve = true, BulletScale = 1.0f, BulletBehavior = BulletBehavior.Dance, BulletStyle = BulletStyle.VenomOrb,
        },
        hardExtra: new BulletEmitterComponent
        {
            IsActive = true, FireRate = 1.0f, BulletsPerVolley = 3, SpreadAngle = 30, BulletSpeed = 260,
            IsAimed = true, BulletScale = 0.8f, BulletStyle = BulletStyle.OrbGreen,
        });

    /// <summary>
    /// A great, wide jet of water. The boss goes to the left edge and charges, then sweeps the jet to the right, stopping short of the right
    /// edge so that the boat has a place to stand; crosses to the right edge, charges again and sweeps back to the left, stopping short of the
    /// left edge; then returns to the middle and rests a few seconds, for the boat to hit it. Every time the jet is let go it opens a circular attack of great spheres of water; there are no small bullets in the attack.
    /// </summary>
    public static BossPhase WaterJet() => Attack("AttackPartyWave", BossMovementType.Sweep, new Vector2(300, MovementSystem.SweepY),
        main: new BulletEmitterComponent(), patterns: new[]
        {
            // every time the jet is let go, a circular attack of great spheres of water: a ring of them opens out of the boss, and a second ring, turned between the first one's spheres, a moment after.
            // There are no small bullets in this attack: the jet and the spheres are all of it.
            Pat.Every(1, c => GreatSpheres(c, 0)).WithTheJet(0),
            Pat.Every(1, c => GreatSpheres(c, 30)).WithTheJet(0.9f),
        });

    /// <summary>A ring of great spheres of water (white and blue in turns) opening out of the boss, `turn` degrees off the first ring.</summary>
    private static void GreatSpheres(PatternContext c, float turn)
    {
        int n = c.N(6, 4);
        for (int k = 0; k < n; k++)
            c.Fire(c.Origin, turn + 360f * k / n + 90f / n, new Shot(k % 2 == 0 ? BulletStyle.GiantBlue : BulletStyle.GiantWhite, 115).Zoomed(1.5f).Hitting(54).Steady());
    }
}
