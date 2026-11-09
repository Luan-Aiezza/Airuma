using System.Collections.Generic;
using Godot;

/// <summary>
/// What the spirits fire, stage after stage: the same figures as the bosses (<see cref="Pat"/>), in the colours of each stage and a little more elaborate with every one. The gunners (type 2) throw a
/// fan, a ring or a rose that goes on for as long as they are there; the red ones (type 3) work through a series of figures, with a rest between: spokes, roses, galaxies, constellations, polygons.
/// </summary>
public static class SpiritFire
{
    private static List<ScheduledPattern> L(params ScheduledPattern[] p) => new List<ScheduledPattern>(p);
    private static ScheduledPattern P(FirePattern f, float duration) => new ScheduledPattern(f, duration);
    private static FirePattern F(float interval, System.Action<PatternContext> figure) => FirePattern.Custom(interval, figure);
    private static Shot S(BulletStyle style, float speed, float scale = 0.9f) => new Shot(style, speed).Sized(scale);
    private static readonly ScheduledPattern Rest14 = P(FirePattern.None, 1.4f);
    private static ScheduledPattern Rest(float seconds) => P(FirePattern.None, seconds);

    // MARK: Stage 2, the party

    public static List<ScheduledPattern> Confetti => L(P(F(1.5f, c => { Pat.Aimed(c, c.Origin, 5, 44, S(BulletStyle.RiceYellow, 190)); Pat.Ring(c, c.Origin, 6, c.Count * 20f, S(BulletStyle.BeadPink, 85, 0.8f)); }), 1000));
    public static List<ScheduledPattern> Balloons => L(P(F(2.0f, c => Pat.Ring(c, c.Origin, 12, c.Count * 11f, S(BulletStyle.OrbBlue, 100).Dancing(60), alternate: true)), 1000));
    public static List<ScheduledPattern> Streamers => L(P(F(0.22f, c => Pat.Ring(c, c.Origin, 3, c.Count * 17f, S(BulletStyle.BeadYellow, 130, 0.8f), scale: false)), 2.2f), Rest(2));
    public static List<ScheduledPattern> Pinwheel => L(P(F(1.3f, c => Pat.Rose(c, c.Origin, 5, 20, c.Count * 8f, 0.5f, S(BulletStyle.OrbViolet, 105).Dancing(50))), 1000));

    // MARK: Stage 3, the storm

    public static List<ScheduledPattern> HailFan => L(P(F(1.4f, c => { Pat.Aimed(c, c.Origin, 5, 40, S(BulletStyle.Hail, 185)); Pat.Ring(c, c.Origin, 8, c.Count * 17f, S(BulletStyle.BeadWhite, 80, 0.8f)); }), 1000));
    public static List<ScheduledPattern> HailRing => L(P(F(1.8f, c => Pat.Rose(c, c.Origin, 6, 20, c.Count * 9f, 0.55f, S(BulletStyle.Hail, 105).Dancing(70))), 1000));
    public static List<ScheduledPattern> Bolts => L(P(F(0.8f, c => Pat.Aimed(c, c.Origin, 5, 44, S(BulletStyle.Spark, 210).Zigzagging(60))), 2.7f), Rest(1.8f));
    public static List<ScheduledPattern> HailSpiral => L(P(F(0.2f, c => Pat.Ring(c, c.Origin, 3, c.Count * 15f, S(BulletStyle.Hail, 135), scale: false)), 2.4f), Rest(1.8f));
    public static List<ScheduledPattern> StormStars => L(P(F(2.6f, c => Pat.Constellation(c, c.Origin, 5, c.Count * 23f, 2, new Shot(BulletStyle.Star, 150).Constellation(210))), 1000));

    public static List<ScheduledPattern> FirstStormHeavy => L(
        P(F(0.5f, c => Pat.Rose(c, c.Origin, 6, 24, c.Count * 11f, 0.55f, S(BulletStyle.Hail, 120).Dancing(70))), 3.6f),
        P(F(0.7f, c => Pat.Aimed(c, c.Origin, 9, 74, S(BulletStyle.Hail, 230))), 3.2f),
        P(F(0.1f, c => Pat.Ring(c, c.Origin, 3, c.Count * 21f, S(BulletStyle.Hail, 150), scale: false)), 3.2f),
        P(F(1.4f, c => Pat.Constellation(c, c.Origin, 5, c.Count * 31f, 2, new Shot(BulletStyle.Star, 160).Constellation(230))), 3.0f), Rest14);
    public static List<ScheduledPattern> SecondStormHeavy => L(
        P(F(0.09f, c => Pat.Ring(c, c.Origin, 4, c.Count * 17f, S(BulletStyle.Hail, 160), scale: false)), 3.4f),
        P(F(0.5f, c => Pat.Aimed(c, c.Origin, 7, 56, S(BulletStyle.Spark, 220).Zigzagging(60))), 3.0f),
        P(F(2.0f, c => Pat.Polygon(c, c.Origin, 6, 60, c.Count * 17f, 3, S(BulletStyle.Spark, 1).LaunchedOut(0.5f, 200))), 4.0f),
        P(F(0.5f, c => Pat.Ring(c, c.Origin, 24, c.Count * -13f, S(BulletStyle.Hail, 125).Dancing(80), alternate: true)), 3.4f), Rest(1.2f));

    // MARK: Stage 4, the cave

    public static List<ScheduledPattern> ShardFan => L(P(F(1.4f, c => { Pat.Aimed(c, c.Origin, 5, 44, S(BulletStyle.Shard, 190)); Pat.Ring(c, c.Origin, 8, c.Count * 15f, S(BulletStyle.BeadCyan, 85, 0.8f)); }), 1000));
    public static List<ScheduledPattern> ShardRing => L(P(F(1.8f, c => Pat.Rose(c, c.Origin, 7, 21, c.Count * 8f, 0.55f, S(BulletStyle.Hail, 100).Dancing(60))), 1000));
    public static List<ScheduledPattern> ShardBolts => L(P(F(0.8f, c => Pat.Aimed(c, c.Origin, 5, 40, S(BulletStyle.Spark, 215).Zigzagging(60))), 2.7f), Rest(1.6f));
    public static List<ScheduledPattern> ShardSpiral => L(P(F(0.2f, c => Pat.Ring(c, c.Origin, 3, c.Count * 17f, S(BulletStyle.Shard, 140), scale: false)), 2.4f), Rest(1.6f));
    public static List<ScheduledPattern> CaveClaws => L(P(F(1.7f, c => Pat.Comb(c, c.Origin + new Vector2(0, 10), c.Aim, 5, 28, new Shot(BulletStyle.Claw, 200))), 1000));

    public static List<ScheduledPattern> IceHeavy => L(
        P(F(0.5f, c => Pat.Ring(c, c.Origin, 26, c.Count * 12f, S(BulletStyle.Hail, 115).Dancing(70), alternate: true)), 3.4f),
        P(F(0.6f, c => Pat.Aimed(c, c.Origin, 9, 74, S(BulletStyle.Shard, 225))), 3.0f),
        P(F(0.1f, c => Pat.Ring(c, c.Origin, 3, c.Count * 22f, S(BulletStyle.Shard, 145), scale: false)), 3.0f),
        P(F(1.1f, c => Pat.Spokes(c, c.Origin, 6, 4, c.Count * 13f, 22, S(BulletStyle.BeadCyan, 90))), 3.3f), Rest(1.6f));
    public static List<ScheduledPattern> StoneHeavy => L(
        P(F(0.09f, c => Pat.Ring(c, c.Origin, 4, c.Count * 18f, S(BulletStyle.Shard, 155), scale: false)), 3.2f),
        P(F(0.5f, c => Pat.Aimed(c, c.Origin, 7, 56, S(BulletStyle.Spark, 220).Zigzagging(60))), 3.0f),
        P(F(1.2f, c => Pat.Comb(c, c.Origin, c.Aim, 9, 30, new Shot(BulletStyle.Claw, 210))), 3.0f),
        P(F(0.5f, c => Pat.Rose(c, c.Origin, 8, 32, c.Count * -13f, 0.55f, S(BulletStyle.Hail, 120).Dancing(80))), 3.4f), Rest(1.4f));
    public static List<ScheduledPattern> DeepHeavy => L(
        P(F(0.45f, c => Pat.Ring(c, c.Origin, 30, c.Count * 9f, S(BulletStyle.Hail, 125).Dancing(80), alternate: true)), 3.0f),
        P(F(0.08f, c => Pat.Ring(c, c.Origin, 5, c.Count * 15f, S(BulletStyle.Shard, 160), scale: false)), 3.0f),
        P(F(0.45f, c => Pat.Aimed(c, c.Origin, 9, 70, S(BulletStyle.Spark, 240).Zigzagging(60))), 3.0f),
        P(F(2.0f, c => Pat.Polygon(c, c.Origin, 6, 66, c.Count * 15f, 3, S(BulletStyle.BeadCyan, 1).LaunchedOut(0.5f, 190))), 4.0f),
        P(F(0.6f, c => Pat.Aimed(c, c.Origin, 13, 100, S(BulletStyle.Shard, 235))), 2.6f), Rest(1.0f));

    // MARK: Stage 5, the waterfall of the seven colours

    private static readonly BulletStyle[] Seven = { BulletStyle.OrbRed, BulletStyle.OrbOrange, BulletStyle.OrbYellow, BulletStyle.OrbGreen, BulletStyle.OrbCyan, BulletStyle.OrbBlue, BulletStyle.OrbViolet };

    public static List<ScheduledPattern> RainbowFan => L(P(F(1.4f, c => { Pat.Arc(c, c.Origin, 7, c.Aim, 54, S(BulletStyle.OrbRed, 195), Seven); Pat.Ring(c, c.Origin, 7, c.Count * 20f, S(BulletStyle.BeadWhite, 85, 0.8f)); }), 1000));
    public static List<ScheduledPattern> BlueRing => L(P(F(1.8f, c => Pat.Ring(c, c.Origin, 14, c.Count * 13f, S(BulletStyle.Wisp, 105).Dancing(70), alternate: true)), 1000));
    public static List<ScheduledPattern> VioletBolts => L(P(F(0.8f, c => Pat.Aimed(c, c.Origin, 5, 44, S(BulletStyle.VenomOrb, 215).Zigzagging(60))), 2.7f), Rest(1.6f));
    public static List<ScheduledPattern> GreenSpiral => L(P(F(0.2f, c => Pat.Ring(c, c.Origin, 3, c.Count * 19f, S(BulletStyle.OrbGreen, 145), scale: false)), 2.4f), Rest(1.6f));
    public static List<ScheduledPattern> Rainbow => L(P(F(0.3f, c => Pat.PolarRing(c, c.Origin, 7, c.Count * 7f, 10, 70, 40, S(BulletStyle.OrbRed, 0), Seven)), 1000));

    public static List<ScheduledPattern> LeftOfPair => L(
        P(F(0.1f, c => Pat.Ring(c, c.Origin, 3, c.Count * 22f, S(BulletStyle.OrbRed, 150), scale: false)), 3.2f),
        P(F(0.6f, c => Pat.Arc(c, c.Origin, 9, c.Aim, 74, S(BulletStyle.Fireball, 225), Seven)), 3.0f),
        P(F(0.5f, c => Pat.Rose(c, c.Origin, 7, 28, c.Count * 12f, 0.55f, S(BulletStyle.Wisp, 115).Dancing(70))), 3.2f), Rest(1.6f));
    public static List<ScheduledPattern> RightOfPair => L(
        P(F(0.1f, c => Pat.Ring(c, c.Origin, 3, c.Count * -22f, S(BulletStyle.OrbGreen, 150), scale: false)), 3.2f),
        P(F(0.6f, c => Pat.Arc(c, c.Origin, 9, c.Aim, 74, S(BulletStyle.VenomOrb, 225), Seven)), 3.0f),
        P(F(0.5f, c => Pat.Rose(c, c.Origin, 7, 28, c.Count * -12f, 0.55f, S(BulletStyle.LotusPetal, 115).Dancing(70))), 3.2f), Rest(1.6f));
    public static List<ScheduledPattern> LeftOfSecondPair => L(
        P(F(0.5f, c => Pat.Aimed(c, c.Origin, 7, 56, S(BulletStyle.Star, 225).Zigzagging(60))), 3.0f),
        P(F(0.45f, c => Pat.Ring(c, c.Origin, 26, c.Count * 13f, S(BulletStyle.OrbRed, 120).Dancing(80), alternate: true)), 3.2f),
        P(F(0.09f, c => Pat.Ring(c, c.Origin, 4, c.Count * 17f, S(BulletStyle.Fireball, 160), scale: false)), 3.0f),
        P(F(0.32f, c => Pat.PolarRing(c, c.Origin, 7, c.Count * 8f, 10, 70, 44, S(BulletStyle.OrbRed, 0), Seven)), 3.0f), Rest(1.2f));
    public static List<ScheduledPattern> RightOfSecondPair => L(
        P(F(0.5f, c => Pat.Aimed(c, c.Origin, 7, 56, S(BulletStyle.Wisp, 225).Zigzagging(60))), 3.0f),
        P(F(0.45f, c => Pat.Ring(c, c.Origin, 26, c.Count * -13f, S(BulletStyle.OrbGreen, 120).Dancing(80), alternate: true)), 3.2f),
        P(F(0.09f, c => Pat.Ring(c, c.Origin, 4, c.Count * -17f, S(BulletStyle.VenomOrb, 160), scale: false)), 3.0f),
        P(F(0.32f, c => Pat.PolarRing(c, c.Origin, 7, c.Count * -8f, 10, 70, -44, S(BulletStyle.OrbRed, 0), Seven)), 3.0f), Rest(1.2f));
    public static List<ScheduledPattern> RainbowHeavy => L(
        P(F(0.09f, c => Pat.Ring(c, c.Origin, 4, c.Count * 18f, S(BulletStyle.OrbRed, 155), scale: false)), 3.2f),
        P(F(0.5f, c => Pat.Arc(c, c.Origin, 9, c.Aim, 74, S(BulletStyle.Star, 220), Seven)), 3.0f),
        P(F(0.5f, c => Pat.Ring(c, c.Origin, 26, c.Count * -13f, S(BulletStyle.Wisp, 120).Dancing(80), alternate: true)), 3.4f),
        P(F(2.2f, c => Pat.Star(c, c.Origin, 5, 2, 70, c.Count * 17f, 4, S(BulletStyle.BeadYellow, 1).LaunchedOut(0.6f, 150))), 4.4f), Rest(1.4f));

    // MARK: Stage 6, the river of stars

    public static List<ScheduledPattern> SilverFan => L(P(F(1.3f, c => { Pat.Aimed(c, c.Origin, 5, 44, S(BulletStyle.KunaiBlue, 205)); Pat.Ring(c, c.Origin, 8, c.Count * 17f, S(BulletStyle.BeadWhite, 85, 0.8f)); }), 1000));
    public static List<ScheduledPattern> StarRing => L(P(F(1.7f, c => Pat.Rose(c, c.Origin, 6, 22, c.Count * 9f, 0.55f, S(BulletStyle.StarBlue, 105).Dancing(70))), 1000));
    public static List<ScheduledPattern> MoonBolts => L(P(F(0.8f, c => Pat.Aimed(c, c.Origin, 5, 44, S(BulletStyle.VenomOrb, 215).Zigzagging(60))), 2.7f), Rest(1.5f));
    public static List<ScheduledPattern> MoonSpiral => L(P(F(0.2f, c => Pat.Ring(c, c.Origin, 3, c.Count * 19f, S(BulletStyle.OrbGreen, 145), scale: false)), 2.4f), Rest(1.5f));
    public static List<ScheduledPattern> MoonCrescent => L(P(F(2.2f, c => Pat.Crescent(c, c.Origin, c.Aim, 40, 7, S(BulletStyle.BeadWhite, 170))), 1000));

    public static List<ScheduledPattern> LoneMoonHeavy => L(
        P(F(0.09f, c => Pat.Ring(c, c.Origin, 4, c.Count * 18f, S(BulletStyle.OrbRed, 155), scale: false)), 3.2f),
        P(F(0.5f, c => Pat.Aimed(c, c.Origin, 9, 70, S(BulletStyle.Star, 220).Zigzagging(60))), 3.0f),
        P(F(0.5f, c => Pat.Ring(c, c.Origin, 26, c.Count * -13f, S(BulletStyle.Wisp, 120).Dancing(80), alternate: true)), 3.4f),
        P(F(1.6f, c => { for (int k = -1; k <= 1; k++) Pat.Crescent(c, c.Origin, c.Aim + k * 28, 46, 7, S(BulletStyle.BeadWhite, 170)); }), 3.2f), Rest(1.4f));
    public static List<ScheduledPattern> LeftMoon => L(
        P(F(0.1f, c => Pat.Ring(c, c.Origin, 3, c.Count * 22f, S(BulletStyle.OrbRed, 150), scale: false)), 3.2f),
        P(F(0.5f, c => Pat.Ring(c, c.Origin, 26, c.Count * 12f, S(BulletStyle.Wisp, 115).Dancing(70), alternate: true)), 3.2f),
        P(F(0.3f, c => Pat.PolarRing(c, c.Origin, 4, c.Count * 9f, 10, 75, 40, S(BulletStyle.BeadCyan, 0), scale: false)), 3.0f), Rest(1.4f));
    public static List<ScheduledPattern> RightMoon => L(
        P(F(0.1f, c => Pat.Ring(c, c.Origin, 3, c.Count * -22f, S(BulletStyle.OrbGreen, 150), scale: false)), 3.2f),
        P(F(0.5f, c => Pat.Ring(c, c.Origin, 26, c.Count * -12f, S(BulletStyle.LotusPetal, 115).Dancing(70), alternate: true)), 3.2f),
        P(F(0.3f, c => Pat.PolarRing(c, c.Origin, 4, c.Count * -9f, 10, 75, -40, S(BulletStyle.BeadViolet, 0), scale: false)), 3.0f), Rest(1.4f));
    public static List<ScheduledPattern> MiddleMoon => L(
        P(F(0.7f, c => Pat.Aimed(c, c.Origin, 13, 90, S(BulletStyle.Fireball, 230))), 3.0f),
        P(F(0.09f, c => Pat.Ring(c, c.Origin, 4, c.Count * 17f, S(BulletStyle.OrbRed, 160), scale: false)), 3.2f),
        P(F(0.5f, c => Pat.Rose(c, c.Origin, 8, 36, c.Count * 13f, 0.55f, S(BulletStyle.Star, 120).Dancing(80))), 3.2f),
        P(F(2.0f, c => Pat.Star(c, c.Origin, 5, 2, 76, c.Count * 17f, 4, S(BulletStyle.StarGold, 1).LaunchedOut(0.6f, 150))), 4.0f), Rest(1.2f));
}
