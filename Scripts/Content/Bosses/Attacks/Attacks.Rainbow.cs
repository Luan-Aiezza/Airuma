using Godot;

/// <summary>
/// The attacks of the waterfall (stage 5, Kuychu, the guardian between the two worlds), made with what was taken from the way Touhou 6 shoots (see Docs/Touhou6.md): layered fans and rings laid out like its
/// bullet instructions (<see cref="Pat.Shoot"/>), bullets that STOP and TURN (rings that stop and then fly at the boat, rings that turn into pinwheels), the burst of speed of a bullet leaving the boss
/// (<see cref="Shot.Popping"/>), curves that last only a while (<see cref="Shot.EasingOutAfter"/>), and LASERS: warned of by a thin line, then a beam, that may turn about the boss (<see cref="Pat.Laser"/>).
/// The boss is two persons who share one life (the one on the left is the boss itself, the one on the right is its twin): in the first half they fire from where they stand and take turns to roam the screen
/// (<see cref="TwinDance"/>); in the second half they are one body that fires from its two halves, and does not move. The rainbow is the colours of the bullets and of the lasers.
/// </summary>
public static partial class Attacks
{
    private static readonly Vector2 LeftSide = new Vector2(170, Layout.BossY), RightSide = new Vector2(430, Layout.BossY);

    /// <summary>The colours of the seven lasers of the rainbow.</summary>
    private static readonly Color[] LaserHues =
    {
        new Color(0.95f, 0.19f, 0.17f), new Color(1f, 0.55f, 0.16f), new Color(1f, 0.89f, 0.24f), new Color(0.24f, 0.88f, 0.42f),
        new Color(0.24f, 0.88f, 0.94f), new Color(0.24f, 0.49f, 1f), new Color(0.69f, 0.3f, 1f),
    };

    private static Shot Arc7(float speed) => new Shot(BulletStyle.OrbRed, speed).Sized(0.95f);
    private static Shot RainDrop(float speed) => new Shot(BulletStyle.BeadBlue, speed).Sized(0.9f);

    /// <summary>A ring of `n` bullets in the colours of `styles` that comes out with a burst, slows to a stop in `stop` seconds and then flies at the boat (each from where it stopped, a little to one side or the other).</summary>
    public static void StopAndAimRing(PatternContext c, Vector2 at, int n, float start, BulletStyle[] styles, float speed, float stop, float after)
    {
        n = c.N(n, 6);
        for (int k = 0; k < n; k++)
            c.Fire(at, start + 360f * k / n, new Shot(styles[k % styles.Length], speed) { Anchor = at }.Sized(0.95f).Popping().Turning(3, stop, 1, 0, after));
    }

    /// <summary>A ring of `n` bullets that slows to a stop and then turns `turn` degrees and goes off again: the ring becomes a pinwheel (a second turn, the other way, makes a flower).</summary>
    public static void PinwheelRing(PatternContext c, Vector2 at, int n, float start, BulletStyle[] styles, float speed, float stop, float turn, int times = 1)
    {
        n = c.N(n, 6);
        for (int k = 0; k < n; k++)
            c.Fire(at, start + 360f * k / n, new Shot(styles[k % styles.Length], speed).Sized(0.95f).Popping().Turning(0, stop, times, turn, speed * 1.15f));
    }

    // MARK: The first form: the two, apart

    /// <summary>The two heads fire in turns, each its own colours: rings that burst out, stop and then fly at the boat, layered fans of rice at it, and a rain of the seven colours from the sky.</summary>
    public static BossPhase TwoHeads() => Attack("AttackTwoHeads", BossMovementType.Stationary, LeftSide, dance: true, patterns: new[]
    {
        // 1) the rings that stop and fly at the boat: the heads take turns   2) layered fans of rice at the boat, also in turns
        Pat.Every(1.6f, c => StopAndAimRing(c, c.Origin, 14, c.Count * 11f, new[] { BulletStyle.OrbRed, BulletStyle.OrbOrange, BulletStyle.OrbYellow }, 170, 0.9f, 185)),
        Pat.Every(1.6f, c => StopAndAimRing(c, c.Origin, 14, c.Count * -11f, new[] { BulletStyle.OrbGreen, BulletStyle.OrbCyan, BulletStyle.OrbBlue }, 170, 0.9f, 185)).Twin().After(0.8f),
        Pat.Every(1.6f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 4, 2, 0, 17, 240, 170, new Shot(BulletStyle.RiceYellow, 0).Popping())).After(0.4f),
        Pat.Every(1.6f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 4, 2, 0, 17, 240, 170, new Shot(BulletStyle.RiceViolet, 0).Popping())).Twin().After(1.2f),
    })
        .Pressured(0.78f);

    /// <summary>
    /// The rainbow: from each of the two, seven streams of bullets in the seven colours turn and bend in great arcs (the bend lasts a while and then they fly straight), turning in opposite senses, with layered rings
    /// of kunai that burst at the boat, and a rain.
    /// </summary>
    public static BossPhase SevenArcs() => Attack("AttackSevenArcs", BossMovementType.Stationary, LeftSide, dance: true, patterns: new[]
    {
        // 1) the seven streams that bend, one head each way   2) layered rings of kunai that burst at the boat, in turns
        Pat.Every(0.5f, c => Pat.Ring(c, c.Origin, 7, c.Count * 3.4f, Arc7(150).Curving(42).EasingOutAfter(1.6f).Popping(), RainbowOrbs)),
        Pat.Every(0.5f, c => Pat.Ring(c, c.Origin, 7, c.Count * -3.4f, Arc7(150).Curving(-42).EasingOutAfter(1.6f).Popping(), RainbowOrbs)).Twin(),
        Pat.Every(3.0f, c => Pat.Shoot(c, c.Origin, Pat.Aim.CircleAimed, 18, 2, 0, 0, 200, 140, new Shot(BulletStyle.KunaiBlue, 0).Popping())).After(0.5f),
        Pat.Every(3.0f, c => Pat.Shoot(c, c.Origin, Pat.Aim.CircleAimed, 18, 2, 10, 0, 200, 140, new Shot(BulletStyle.KunaiGreen, 0).Popping())).Twin().After(2.0f),
    })
        .Pressured(1.0f);

    /// <summary>
    /// The earthquake: the border shakes, the screen with it, and drops of the falls come down from above in a crowd; the two heads send out LASERS, five at a time in a fan at the boat, in turns; and rings burst out and
    /// turn into pinwheels.
    /// </summary>
    public static BossPhase BorderQuake() => Attack("AttackBorderQuake", BossMovementType.Stationary, LeftSide, dance: true,
        quake: new QuakeSpec { IsActive = true, Interval = 3.0f, Duration = 3.6f, Intensity = 1f },
        patterns: new[]
        {
            // 1) the drops of the falls, while the earth shakes   2) fans of five lasers at the boat, in turns
            Pat.Every(0.09f, c => Pat.Rain(c, Edge.Top, 3, 5, RainDrop(300))).OnQuake(),
            Pat.Every(4.6f, c => { for (int k = -2; k <= 2; k++) Pat.Laser(c, c.Origin, c.Aim + k * 24, 32, 1.1f, 0.55f, LaserHues[(k + 2) * 5 / 4 % 7]); }).After(0.8f),
            Pat.Every(4.6f, c => { for (int k = -2; k <= 2; k++) Pat.Laser(c, c.Origin, c.Aim + k * 24 + 12, 32, 1.1f, 0.55f, LaserHues[(k + 5) % 7]); }).Twin().After(3.1f),
        })
        .Pressured(2.0f);

    /// <summary>
    /// The tide: walls of water cross the field from the side, level, one above the other, each warned of by a line; the boat has to go up or down between them. From the sides, rows of bullets come in, stop, and fall from
    /// where they stopped like a curtain; and rings turn. The two stand still.
    /// </summary>
    public static BossPhase Tide() => Attack("AttackTide", BossMovementType.Stationary, LeftSide, dance: false, twinTarget: RightSide,
        slashes: new SlashSpec { IsActive = true, Interval = 2.7f, Telegraph = 1.1f, Strike = 0.45f, Width = 44, Spacing = 200, Count = 2, Angle = 84, Aim = SlashAim.Player },
        patterns: new[]
        {
            // 1) the walls of water (above)   2) walls of drops that fall from the top at random points (a gap of at least 200 px between two of them)
            Pat.Every(1.7f, c =>
            {
                int walls = 2;
                float first = PatternContext.Rand(c.Left + 70, c.Right - 70);
                for (int w = 0; w < walls; w++)
                {
                    float x = w == 0 ? first : (first < c.MiddleX ? PatternContext.Rand(first + 220, c.Right - 60) : PatternContext.Rand(c.Left + 60, first - 220));
                    int drops = 6;
                    for (int k = 0; k < drops; k++)
                        c.Fire(new Vector2(x + (k - (drops - 1) / 2f) * 38, -14), 90, new Shot(RainbowOrbs[(k + w * 3) % 7], c.Difficulty.Pick(150f, 165f, 185f)).Sized(0.95f).Steady());
                }
            }).After(0.6f),
            // 3) (normal and hard) pinwheels of the seven colours from the body that is on the left, every few seconds, besides the walls: 8 bullets in normal, 12 in hard
            Pat.Every(3.4f, c => PinwheelRing(c, c.Origin, c.Difficulty.Pick(8, 8, 12), c.Count * 8f, RainbowOrbs, 90, 0.8f, c.Count % 2 == 0 ? 60 : -60)).After(1.6f).OnlyOnHard(),
        })
        .Pressured(4.0f);

    /// <summary>
    /// The trial of worth: the two fire together, face to face: great rings of the seven colours that burst out and turn into pinwheels, one turning each way, with layered fans of kunai at the boat, and rings that stop and
    /// fly at it.
    /// </summary>
    public static BossPhase Trial() => Attack("AttackTrial", BossMovementType.Stationary, LeftSide, dance: true, patterns: new[]
    {
        // 1) great rings that burst and turn into pinwheels, one turning each way   2) layered fans of kunai at the boat
        Pat.Every(1.4f, c => PinwheelRing(c, c.Origin, 21, c.Count * 8f, RainbowOrbs, 150, 0.7f, 90, times: 2)),
        Pat.Every(1.4f, c => PinwheelRing(c, c.Origin, 21, c.Count * -8f, RainbowOrbs, 150, 0.7f, -90, times: 2)).Twin().After(0.7f),
        Pat.Every(2.8f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 5, 2, 0, 16, 250, 170, new Shot(BulletStyle.KunaiBlue, 0).Popping())).After(1.4f),
    })
        .Pressured(1.0f);

    // From here the two are ONE (the second half of the fight): the second glides to the first and they fuse, and the one body that is left fires from the two halves of itself. It does not move.
    private static readonly Vector2 Fusion = new Vector2(300, Layout.BossY + 50);   // the fused body is big: lower, so that its head is clear of the boss bar
    private static Vector2 LeftHalf(PatternContext c) => c.Origin + new Vector2(-34, 0);
    private static Vector2 RightHalf(PatternContext c) => c.Origin + new Vector2(34, 0);

    /// <summary>
    /// The broken rainbow (enraged, fused): seven LASERS, one of each colour, spin slowly about the body, the way the spokes of a wheel do, and leave a gap between them for the boat; layered rings burst and turn into
    /// pinwheels, a galaxy of the seven colours turns out of each half, and a rain falls.
    /// </summary>
    public static BossPhase BrokenRainbow() => Attack("AttackBrokenRainbow", BossMovementType.Stationary, Fusion, fused: true, patterns: new[]
    {
        // 1) the seven lasers that spin   2) the galaxy of the seven colours from each half   3) pinwheels
        Pat.Every(9.5f, c => { float spin = c.Count % 2 == 0 ? 11 : -11; for (int k = 0; k < 7; k++) Pat.Laser(c, c.Origin, c.Count * 17f + 360f * k / 7, 30, 1.6f, 6.4f, LaserHues[k], spin, follow: true, blink: 2); }).After(0.8f),
        Pat.Every(0.95f, c => Pat.PolarRing(c, LeftHalf(c), 7, c.Count * 13f, 12, 34, 20, new Shot(BulletStyle.OrbRed, 0).Sized(0.95f), RainbowOrbs)),
        Pat.Every(0.95f, c => Pat.PolarRing(c, RightHalf(c), 7, c.Count * -13f, 12, 34, -20, new Shot(BulletStyle.OrbRed, 0).Sized(0.95f), RainbowOrbs)).After(0.48f),
        Pat.Every(2.0f, c => PinwheelRing(c, c.Origin, 24, c.Count * 7f, RainbowOrbs, 40, 0.8f, c.Count % 2 == 0 ? 75 : -75)).After(0.6f),
    })
        .Pressured(0.6f);

    /// <summary>
    /// The tremor between two worlds (enraged, fused): the one body stamps, the screen shakes almost without pause while the drops of the falls pour down; columns of bullets come in from the top (the sky) and from the
    /// bottom (the water), stop in the middle and turn on the boat; layered fans of the seven colours. (No roar: it threw the boat about and made the attack too hard.)
    /// </summary>
    public static BossPhase TwoWorlds() => Attack("AttackTwoWorlds", BossMovementType.Stationary, Fusion, fused: true,
        quake: new QuakeSpec { IsActive = true, Interval = 1.0f, Duration = 4.2f, Intensity = 1.4f },
        patterns: new[]
        {
            // 1) the drops, while the earth shakes   2) columns from the sky and from the water that stop in the middle and turn on the boat (not so often: the boat needs time to read them)   3) fans of the seven colours
            Pat.Every(0.24f, c => Pat.Rain(c, Edge.Top, 2, 5, RainDrop(300))).OnQuake(),
            Pat.Every(3.8f, c =>
            {
                bool sky = c.Count % 2 == 0;
                for (int k = 0; k < 8; k++)
                {
                    float x = c.Left + c.Width * (k + 0.5f) / 8 + PatternContext.Rand(-14, 14);
                    if (sky) c.Fire(new Vector2(x, -14), 90, new Shot(RainbowOrbs[k % 7], 2 * 230 / 1.1f) { Anchor = new Vector2(c.MiddleX, -14) }.Sized(0.95f).Steady().Turning(3, 1.1f, 1, 0, 210));
                    else c.Fire(new Vector2(x, c.Bottom + 14), -90, new Shot(RainbowOrbs[(k + 3) % 7], 2 * 330 / 1.3f) { Anchor = new Vector2(c.MiddleX, c.Bottom + 14) }.Sized(0.95f).Steady().Turning(3, 1.3f, 1, 0, 190));
                }
            }).After(0.5f),
            Pat.Every(2.6f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 5, 2, 0, 16, 240, 170, new Shot(BulletStyle.OrbRed, 0).Sized(0.95f).Popping())).After(1.2f),
        })
        .Pressured(2.5f);

    /// <summary>
    /// The guardian (enraged, fused): everything of the border at once, from the one body that has two halves. Lasers turn out of each half, one way and the other; layered rings burst and turn into pinwheels and flowers;
    /// fans of the seven colours go out from each half in turns; the galaxies turn; the tremor. (The roar is Ukhumo's alone.)
    /// </summary>
    public static BossPhase Guardian() => Attack("AttackGuardian", BossMovementType.Stationary, Fusion, fused: true,
        quake: new QuakeSpec { IsActive = true, Interval = 5.0f, Duration = 2.2f, Intensity = 1.1f },
        patterns: new[]
        {
            // 1) lasers turning out of each half, one way and the other   2) the galaxies   3) pinwheels and flowers
            Pat.Every(7.2f, c =>
            {
                // transparent (harmless) for the last 0.8 s of every 2 s, so the boat may cross
                for (int k = 0; k < 5; k++) Pat.Laser(c, LeftHalf(c), 72f * k + c.Count * 20f, 30, 1.2f, 4.4f, LaserHues[k], 12, follow: true, blink: 2);
                for (int k = 0; k < 5; k++) Pat.Laser(c, RightHalf(c), 72f * k + 36 - c.Count * 20f, 30, 1.2f, 4.4f, LaserHues[(k + 2) % 7], -12, follow: true, blink: 2);
            }).After(0.8f),
            Pat.Every(1.0f, c => Pat.PolarRing(c, LeftHalf(c), 7, c.Count * 11f, 12, 48, 27, new Shot(BulletStyle.OrbRed, 0).Sized(0.95f), RainbowOrbs)),
            Pat.Every(1.0f, c => Pat.PolarRing(c, RightHalf(c), 7, c.Count * -11f, 12, 48, -27, new Shot(BulletStyle.OrbRed, 0).Sized(0.95f), RainbowOrbs)).After(0.5f),
            Pat.Every(3.2f, c => PinwheelRing(c, LeftHalf(c), 16, c.Count * 9f, RainbowOrbs, 57, 0.8f, 80, times: 2)).After(0.5f),
            Pat.Every(3.2f, c => PinwheelRing(c, RightHalf(c), 16, c.Count * -9f, RainbowOrbs, 57, 0.8f, -80, times: 2)).After(2.1f),
        })
        .Pressured(0.6f)
        .Faster(1.5f);
}
