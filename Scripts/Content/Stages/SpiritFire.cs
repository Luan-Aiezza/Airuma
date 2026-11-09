using System.Collections.Generic;
using Godot;

/// <summary>
/// What the spirits fire, stage after stage: the same figures as the bosses (<see cref="Pat"/>), in the colours of each stage and a little more elaborate with every one, but SOFT (see Docs/Danmaku.md):
/// the gunners (type 2) throw one small figure at a time (a fan, a ring, a rose) for as long as they are there; the red ones (type 3) work through a series of figures, one at a time, with a rest between, and
/// they never fire two at once. The gaps between the bullets leave room for the boat (<see cref="Pat.MinGap"/>), the counts are low, and the figures use the ways of shooting of Touhou 6 (Docs/Touhou6.md):
/// layered fans that burst out, rings that stop and fly together at the boat. Spirits only fire in the upper part of the field (see EnemySystem.FireLimitY).
/// </summary>
public static class SpiritFire
{
    private static List<ScheduledPattern> L(params ScheduledPattern[] p) => new List<ScheduledPattern>(p);
    private static ScheduledPattern P(FirePattern f, float duration) => new ScheduledPattern(f, duration);
    private static FirePattern F(float interval, System.Action<PatternContext> figure) => FirePattern.Custom(interval, figure);
    private static Shot S(BulletStyle style, float speed, float scale = 0.9f) => new Shot(style, speed).Sized(scale);
    private static ScheduledPattern Rest(float seconds) => P(FirePattern.None, seconds);
    private const float Always = 1000;

    /// <summary>A layered fan at the boat that bursts out of the spirit: `count` bullets a layer, `layers` layers, `step` degrees between neighbours.</summary>
    private static FirePattern Fan(float interval, int count, int layers, float step, float speed1, float speed2, BulletStyle style, float scale = 0.9f) =>
        F(interval, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, count, layers, 0, step, speed1, speed2, S(style, 0, scale).Popping(220, 0.25f)));

    /// <summary>A small ring that turns a little each time, in `styles`.</summary>
    private static FirePattern Ring(float interval, int n, float turn, float speed, BulletStyle[] styles, bool dance = false) =>
        F(interval, c => Pat.Ring(c, c.Origin, n, c.Count * turn, dance ? S(styles[0], speed).Dancing(60) : S(styles[0], speed), styles, dance));

    /// <summary>A ring that bursts out, stops and then flies, all together, at the boat.</summary>
    private static FirePattern StopRing(float interval, int n, BulletStyle[] styles, float speed = 150, float stop = 0.9f, float after = 160) =>
        F(interval, c => Attacks.StopAndAimRing(c, c.Origin, n, c.Count * 17f, styles, speed, stop, after));

    /// <summary>Little bolts that zigzag at the boat, a few at a time.</summary>
    private static FirePattern Zigzag(float interval, int n, float spread, BulletStyle style, float speed) =>
        F(interval, c => Pat.Aimed(c, c.Origin, n, spread, S(style, speed).Zigzagging(60)));

    // MARK: Stage 2, the party

    public static List<ScheduledPattern> Confetti => L(P(Fan(2.0f, 3, 1, 20, 190, 190, BulletStyle.NotePink, 1.0f), Always));
    public static List<ScheduledPattern> Balloons => L(P(Ring(2.8f, 8, 14, 85, new[] { BulletStyle.OrbBlue }, dance: true), Always));
    public static List<ScheduledPattern> Streamers => L(P(F(0.32f, c => Pat.Ring(c, c.Origin, 2, c.Count * 23f, S(BulletStyle.BeadYellow, 120, 0.8f), scale: false)), 2.0f), Rest(2.6f));
    public static List<ScheduledPattern> Pinwheel => L(P(F(2.0f, c => Pat.Rose(c, c.Origin, 4, 16, c.Count * 8f, 0.5f, S(BulletStyle.NoteGold, 95).Dancing(50))), Always));

    // MARK: Stage 3, the storm

    public static List<ScheduledPattern> HailFan => L(P(Fan(2.0f, 3, 1, 20, 185, 185, BulletStyle.Hail), Always));
    public static List<ScheduledPattern> HailRing => L(P(Ring(2.6f, 8, 17, 85, new[] { BulletStyle.BeadWhite }, dance: true), Always));
    public static List<ScheduledPattern> Bolts => L(P(Zigzag(0.9f, 3, 36, BulletStyle.Spark, 190), 2.4f), Rest(2.4f));
    public static List<ScheduledPattern> HailSpiral => L(P(F(0.32f, c => Pat.Ring(c, c.Origin, 2, c.Count * 21f, S(BulletStyle.Hail, 120), scale: false)), 2.0f), Rest(2.6f));
    public static List<ScheduledPattern> StormStars => L(P(F(3.6f, c => Pat.Constellation(c, c.Origin, 5, c.Count * 23f, 2, new Shot(BulletStyle.Star, 150).Constellation(210))), Always));

    public static List<ScheduledPattern> FirstStormHeavy => L(
        P(Ring(0.9f, 10, 13, 100, new[] { BulletStyle.Hail }, dance: true), 3.6f), Rest(1.6f),
        P(Fan(1.2f, 5, 2, 17, 220, 160, BulletStyle.Hail), 3.6f), Rest(1.6f),
        P(StopRing(1.7f, 12, new[] { BulletStyle.BeadWhite, BulletStyle.Hail }, 140, 0.9f, 150), 3.4f), Rest(2.2f));
    public static List<ScheduledPattern> SecondStormHeavy => L(
        P(F(0.3f, c => Pat.Ring(c, c.Origin, 3, c.Count * 17f, S(BulletStyle.Hail, 130), scale: false)), 3.0f), Rest(1.6f),
        P(Zigzag(0.9f, 5, 50, BulletStyle.Spark, 200), 3.0f), Rest(1.6f),
        P(F(2.4f, c => Pat.Polygon(c, c.Origin, 6, 70, c.Count * 17f, 2, S(BulletStyle.Spark, 1).LaunchedOut(0.5f, 170))), 4.8f), Rest(1.6f),
        P(Ring(0.9f, 14, -11, 105, new[] { BulletStyle.Hail }, dance: true), 3.6f), Rest(2.0f));

    // MARK: Stage 4, the cave

    public static List<ScheduledPattern> ShardFan => L(P(Fan(2.0f, 3, 1, 20, 190, 190, BulletStyle.Shard), Always));
    public static List<ScheduledPattern> ShardRing => L(P(Ring(2.6f, 8, 15, 85, new[] { BulletStyle.BeadCyan }, dance: true), Always));
    public static List<ScheduledPattern> ShardBolts => L(P(Zigzag(0.9f, 3, 36, BulletStyle.Spark, 195), 2.4f), Rest(2.2f));
    public static List<ScheduledPattern> ShardSpiral => L(P(F(0.32f, c => Pat.Ring(c, c.Origin, 2, c.Count * 21f, S(BulletStyle.Shard, 125), scale: false)), 2.0f), Rest(2.4f));
    public static List<ScheduledPattern> CaveClaws => L(P(F(2.2f, c => Pat.Comb(c, c.Origin + new Vector2(0, 10), c.Aim, 4, 34, new Shot(BulletStyle.Claw, 185))), Always));

    private static List<ScheduledPattern> IceSeries(float turn) => L(
        P(Ring(0.9f, 12, 12 * turn, 100, new[] { BulletStyle.Hail }, dance: true), 3.6f), Rest(1.6f),
        P(Fan(1.2f, 5, 2, 17, 220, 160, BulletStyle.Shard), 3.6f), Rest(1.6f),
        P(F(1.4f, c => Pat.Spokes(c, c.Origin, 5, 3, c.Count * 13f * turn, 24, S(BulletStyle.BeadCyan, 95))), 3.4f), Rest(2.2f));
    private static List<ScheduledPattern> StoneSeries(float turn) => L(
        P(F(0.32f, c => Pat.Ring(c, c.Origin, 3, c.Count * 18f * turn, S(BulletStyle.Shard, 130), scale: false)), 3.0f), Rest(1.6f),
        P(Zigzag(0.9f, 5, 50, BulletStyle.Spark, 200), 3.0f), Rest(1.6f),
        P(F(1.5f, c => Pat.Comb(c, c.Origin, c.Aim, 6, 34, new Shot(BulletStyle.Claw, 190))), 3.0f), Rest(2.0f));
    public static List<ScheduledPattern> IceHeavy => IceSeries(1);
    public static List<ScheduledPattern> IcePairLeft => IceSeries(1);
    public static List<ScheduledPattern> IcePairRight => IceSeries(-1);
    public static List<ScheduledPattern> StonePairLeft => StoneSeries(1);
    public static List<ScheduledPattern> StonePairRight => StoneSeries(-1);
    public static List<ScheduledPattern> StoneHeavy => StoneSeries(1);
    public static List<ScheduledPattern> DeepHeavy => L(
        P(Ring(0.8f, 14, 9, 105, new[] { BulletStyle.Hail }, dance: true), 3.2f), Rest(1.4f),
        P(F(0.3f, c => Pat.Ring(c, c.Origin, 3, c.Count * 15f, S(BulletStyle.Shard, 135), scale: false)), 3.0f), Rest(1.4f),
        P(F(2.2f, c => Pat.Polygon(c, c.Origin, 6, 70, c.Count * 15f, 2, S(BulletStyle.BeadCyan, 1).LaunchedOut(0.5f, 170))), 4.4f), Rest(2.0f));

    // MARK: Stage 5, the waterfall of the seven colours

    private static readonly BulletStyle[] Seven = { BulletStyle.OrbRed, BulletStyle.OrbOrange, BulletStyle.OrbYellow, BulletStyle.OrbGreen, BulletStyle.OrbCyan, BulletStyle.OrbBlue, BulletStyle.OrbViolet };

    public static List<ScheduledPattern> RainbowFan => L(P(F(2.0f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 3, 1, 0, 20, 190, 190, S(BulletStyle.OrbRed, 0).Popping(220, 0.25f))), Always));
    public static List<ScheduledPattern> BlueRing => L(P(Ring(2.6f, 9, 13, 85, new[] { BulletStyle.Wisp }, dance: true), Always));
    public static List<ScheduledPattern> VioletBolts => L(P(Zigzag(0.9f, 3, 36, BulletStyle.VenomOrb, 195), 2.4f), Rest(2.2f));
    public static List<ScheduledPattern> GreenSpiral => L(P(F(0.34f, c => Pat.Ring(c, c.Origin, 2, c.Count * 25f, S(BulletStyle.OrbGreen, 125), scale: false)), 2.0f), Rest(2.4f));
    public static List<ScheduledPattern> Rainbow => L(P(F(1.3f, c => Pat.PolarRing(c, c.Origin, 5, c.Count * 14f, 10, 40, 24, S(BulletStyle.OrbRed, 0), Seven)), Always));

    private static List<ScheduledPattern> PairSeries(float turn, BulletStyle ring, BulletStyle fan, BulletStyle petal) => L(
        P(F(0.34f, c => Pat.Ring(c, c.Origin, 3, c.Count * 22f * turn, S(ring, 135), scale: false)), 3.0f), Rest(1.6f),
        P(F(1.2f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 5, 2, 0, 17, 220, 160, S(fan, 0).Popping(220, 0.25f))), 3.6f), Rest(1.6f),
        P(F(1.0f, c => Pat.Rose(c, c.Origin, 6, 24, c.Count * 12f * turn, 0.55f, S(petal, 105).Dancing(70))), 3.4f), Rest(2.2f));
    public static List<ScheduledPattern> LeftOfPair => PairSeries(1, BulletStyle.OrbRed, BulletStyle.Fireball, BulletStyle.Wisp);
    public static List<ScheduledPattern> RightOfPair => PairSeries(-1, BulletStyle.OrbGreen, BulletStyle.VenomOrb, BulletStyle.LotusPetal);
    private static List<ScheduledPattern> SecondPairSeries(float turn, BulletStyle ring, BulletStyle bolt) => L(
        P(Zigzag(0.9f, 5, 50, bolt, 200), 3.0f), Rest(1.4f),
        P(Ring(0.9f, 16, 13 * turn, 110, new[] { ring }, dance: true), 3.4f), Rest(1.4f),
        P(F(1.3f, c => Pat.PolarRing(c, c.Origin, 5, c.Count * 8f * turn, 10, 42, 24 * turn, S(BulletStyle.OrbRed, 0), Seven)), 3.4f), Rest(2.0f));
    public static List<ScheduledPattern> LeftOfSecondPair => SecondPairSeries(1, BulletStyle.OrbRed, BulletStyle.Star);
    public static List<ScheduledPattern> RightOfSecondPair => SecondPairSeries(-1, BulletStyle.OrbGreen, BulletStyle.Wisp);
    public static List<ScheduledPattern> RainbowHeavy => L(
        P(F(0.34f, c => Pat.Ring(c, c.Origin, 3, c.Count * 18f, S(BulletStyle.OrbRed, 135), scale: false)), 3.0f), Rest(1.4f),
        P(F(1.3f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 5, 2, 0, 17, 220, 160, S(BulletStyle.Star, 0).Popping(220, 0.25f))), 3.6f), Rest(1.4f),
        P(F(2.4f, c => Pat.Star(c, c.Origin, 5, 2, 70, c.Count * 17f, 3, S(BulletStyle.BeadYellow, 1).LaunchedOut(0.6f, 140))), 4.8f), Rest(2.0f));

    // MARK: Stage 6, the river of stars

    public static List<ScheduledPattern> SilverFan => L(P(Fan(1.9f, 3, 1, 20, 195, 195, BulletStyle.KunaiBlue), Always));
    public static List<ScheduledPattern> StarRing => L(P(F(2.4f, c => Pat.Rose(c, c.Origin, 4, 18, c.Count * 9f, 0.55f, S(BulletStyle.StarBlue, 100).Dancing(70))), Always));
    public static List<ScheduledPattern> MoonBolts => L(P(Zigzag(0.9f, 3, 36, BulletStyle.VenomOrb, 195), 2.4f), Rest(2.0f));
    public static List<ScheduledPattern> MoonSpiral => L(P(F(0.34f, c => Pat.Ring(c, c.Origin, 2, c.Count * 25f, S(BulletStyle.OrbWhite, 125), scale: false)), 2.0f), Rest(2.2f));
    public static List<ScheduledPattern> MoonCrescent => L(P(F(2.6f, c => Pat.Crescent(c, c.Origin, c.Aim, 60, 6, S(BulletStyle.BeadWhite, 150, 0.85f))), Always));

    public static List<ScheduledPattern> LoneMoonHeavy => L(
        P(F(0.34f, c => Pat.Ring(c, c.Origin, 3, c.Count * 18f, S(BulletStyle.OrbWhite, 135), scale: false)), 3.0f), Rest(1.3f),
        P(F(1.3f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 5, 2, 0, 17, 220, 160, S(BulletStyle.StarBlue, 0).Popping(220, 0.25f))), 3.6f), Rest(1.3f),
        P(F(2.0f, c => { for (int k = -1; k <= 1; k++) Pat.Crescent(c, c.Origin, c.Aim + k * 30, 60, 6, S(BulletStyle.BeadWhite, 150, 0.85f)); }), 4.0f), Rest(1.8f));
    private static List<ScheduledPattern> MoonSeries(float turn, BulletStyle ring, BulletStyle arm) => L(
        P(F(0.34f, c => Pat.Ring(c, c.Origin, 3, c.Count * 22f * turn, S(ring, 135), scale: false)), 3.0f), Rest(1.3f),
        P(Ring(0.9f, 16, 12 * turn, 105, new[] { BulletStyle.OrbWhite }, dance: true), 3.4f), Rest(1.3f),
        P(F(1.3f, c => Pat.PolarRing(c, c.Origin, 5, c.Count * 9f * turn, 10, 42, 24 * turn, S(arm, 0), scale: false)), 3.4f), Rest(1.8f));
    public static List<ScheduledPattern> LeftMoon => MoonSeries(1, BulletStyle.OrbWhite, BulletStyle.BeadCyan);
    public static List<ScheduledPattern> RightMoon => MoonSeries(-1, BulletStyle.OrbViolet, BulletStyle.BeadViolet);
    public static List<ScheduledPattern> MiddleMoon => L(
        P(F(1.4f, c => Pat.Shoot(c, c.Origin, Pat.Aim.FanAimed, 7, 1, 0, 15, 215, 215, S(BulletStyle.Fireball, 0).Popping(220, 0.25f))), 3.4f), Rest(1.3f),
        P(F(1.1f, c => Pat.Rose(c, c.Origin, 6, 24, c.Count * 13f, 0.55f, S(BulletStyle.StarGold, 110).Dancing(80))), 3.4f), Rest(1.3f),
        P(F(2.4f, c => Pat.Star(c, c.Origin, 5, 2, 74, c.Count * 17f, 3, S(BulletStyle.StarGold, 1).LaunchedOut(0.6f, 140))), 4.8f), Rest(1.8f));
}
