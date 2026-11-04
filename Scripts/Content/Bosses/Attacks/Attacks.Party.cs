using Godot;

/// <summary>
/// The attacks of the party (stage 2, Baylo, the dancer): his bullets dance, sway on a half-second beat, and now there are NOTES in the air; he dives and fires a great jet. Made as the later bosses' are: few
/// patterns at once, gaps for the boat (<see cref="Pat.MinGap"/>), and the ways of shooting of Touhou 6 (Docs/Touhou6.md): layered fans, rings that stop and fly at the boat, galaxies that turn.
/// </summary>
public static partial class Attacks
{
    private static readonly BulletStyle[] Notes = { BulletStyle.NoteGold, BulletStyle.NotePink, BulletStyle.NoteCyan };
    private static readonly BulletStyle[] PartyOrbs = { BulletStyle.OrbGreen, BulletStyle.OrbYellow, BulletStyle.OrbPink, BulletStyle.OrbCyan };

    /// <summary>Two rings of orbs that turn in opposite senses with neighbours swaying against each other, while the dancer roams the field; between the beats, a layered fan of notes at the boat.</summary>
    public static BossPhase PartyTwoStep() => Attack("AttackDoubleStep", BossMovementType.Roam, Centre, patterns: new[]
    {
        Pat.Every(Pat.Beat * 2, c => Pat.Ring(c, c.Origin, 10, c.Count * 14f * (c.Count % 2 == 0 ? 1 : -1), new Shot(BulletStyle.VenomOrb, 130).Sized(0.9f).Dancing(100), alternate: true)),
        Pat.Every(Pat.Beat * 5, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 4, 2, 0, 17, 220, 160, new Shot(Notes[c.Count % 3], 0).Popping())).After(Pat.Beat * 2),
    })
        .Pressured(1.00f);

    /// <summary>Confetti and music: pieces of line (streamers) thrown out of the dancer wherever he goes, in a fan, turned a little each time; and notes that fall from the top, swaying, at random places.</summary>
    public static BossPhase ConfettiRain() => Attack("AttackConfettiRain", BossMovementType.Roam, Centre, patterns: new[]
    {
        Pat.Every(Pat.Beat * 4, c =>
        {
            int n = c.N(6, 4);
            float turn = (c.Count % 2) * 9;
            for (int k = 0; k < n; k++)
            {
                float heading = 90 - 80 + turn + 160f * k / (n - 1);
                Pat.Line(c, c.Origin, c.Origin + Pat.Dir(heading) * 72, 3, heading, new Shot(Beads[(k + c.Count) % Beads.Length], 140).Sized(0.9f));
            }
        }),
        Pat.Every(0.5f, c => Pat.Rain(c, Edge.Top, 1, 16, new Shot(Notes[c.Count % 3], 115).Sized(1.0f).Dancing(50))),
    })
        .Pressured(1.24f);

    /// <summary>A turning wheel of swaying petals, one galaxy a beat, and rings of notes that burst out, stop and then fly at the boat.</summary>
    public static BossPhase SambaWheel() => Attack("AttackSambaWheel", BossMovementType.Roam, Centre, patterns: new[]
    {
        Pat.Every(Pat.Beat * 2, c => Pat.PolarRing(c, c.Origin, 7, c.Count * 12f, 12, 42, c.Count % 8 < 4 ? 24 : -24, new Shot(BulletStyle.LotusPetal, 0).Sized(1.0f), PartyPetals)),
        Pat.Every(3.4f, c => StopAndAimRing(c, c.Origin, 10, c.Count * 17f, Notes, 150, 0.9f, 170)).After(1.2f),
    })
        .Pressured(1.00f);

    private static readonly BulletStyle[] PartyPetals = { BulletStyle.LotusPetal };

    /// <summary>
    /// The dolphin dives and a shadow lies under the boat, following it a while and then holding still; he comes out of the water at the shadow (the dive backwards), with a fan of spray. Ringed bubbles that
    /// sway while he is on the surface, and, on hard, a fan of notes at the boat.
    /// </summary>
    public static BossPhase GreatLeap() => Attack("AttackDolphinLeap", BossMovementType.Dive, new Vector2(300, MovementSystem.DiveTopY), patterns: new[]
    {
        Pat.Every(Pat.Beat * 2, c => Pat.Ring(c, c.Origin, 9, c.Count * 13f, new Shot(BulletStyle.VenomOrb, 120).Sized(1.0f).Dancing(90), alternate: true)),
        Pat.Every(2.4f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 3, 2, 0, 18, 230, 160, new Shot(BulletStyle.NoteCyan, 0).Popping())).After(1.0f).OnlyOnHard(),
    })
        .Pressured(2.51f);

    /// <summary>
    /// A great, wide jet of water. The boss goes to the left edge and charges, then sweeps the jet to the right, stopping short of the right edge so that the boat has a place to stand; crosses to the right edge,
    /// charges again and sweeps back to the left, stopping short of the left edge; then returns to the middle and rests a few seconds, for the boat to hit it. Every time the jet is let go it opens a circular
    /// attack of great spheres of water; there are no small bullets in the attack.
    /// </summary>
    public static BossPhase WaterJet() => Attack("AttackPartyWave", BossMovementType.Sweep, new Vector2(300, MovementSystem.SweepY),
        main: new BulletEmitterComponent(), patterns: new[]
        {
            Pat.Every(1, c => GreatSpheres(c, 0)).WithTheJet(0),
            Pat.Every(1, c => GreatSpheres(c, 30)).WithTheJet(0.9f),
        })
        .Pressured(1.0f);

    /// <summary>A ring of great spheres of water (white and blue in turns) opening out of the boss, `turn` degrees off the first ring.</summary>
    private static void GreatSpheres(PatternContext c, float turn)
    {
        int n = 6;
        for (int k = 0; k < n; k++)
            c.Fire(c.Origin, turn + 360f * k / n + 90f / n, new Shot(k % 2 == 0 ? BulletStyle.GiantBlue : BulletStyle.GiantWhite, 105).Zoomed(1.5f).Hitting(54).Steady());
    }
}
