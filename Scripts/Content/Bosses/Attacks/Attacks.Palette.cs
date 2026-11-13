/// <summary>The colours that the attacks cycle through (confetti, balloons, streamers, the rainbow), so that a pattern of many colours is written once.</summary>
public static partial class Attacks
{
    private static readonly BulletStyle[] Beads = { BulletStyle.BeadRed, BulletStyle.BeadBlue, BulletStyle.BeadYellow, BulletStyle.BeadGreen, BulletStyle.BeadViolet, BulletStyle.BeadPink };
    private static readonly BulletStyle[] Orbs = { BulletStyle.OrbRed, BulletStyle.OrbBlue, BulletStyle.OrbYellow, BulletStyle.OrbGreen, BulletStyle.OrbViolet, BulletStyle.OrbOrange };
    private static readonly BulletStyle[] Rices = { BulletStyle.RiceRed, BulletStyle.RiceBlue, BulletStyle.RiceYellow, BulletStyle.RiceGreen, BulletStyle.RiceViolet };
    /// <summary>The seven colours of the rainbow, in orbs.</summary>
    private static readonly BulletStyle[] RainbowOrbs = { BulletStyle.OrbRed, BulletStyle.OrbOrange, BulletStyle.OrbYellow, BulletStyle.OrbGreen, BulletStyle.OrbCyan, BulletStyle.OrbBlue, BulletStyle.OrbViolet };
    private static readonly BulletStyle[] RainbowBeads = { BulletStyle.BeadRed, BulletStyle.BeadYellow, BulletStyle.BeadYellow, BulletStyle.BeadGreen, BulletStyle.BeadCyan, BulletStyle.BeadBlue, BulletStyle.BeadViolet };
}

public static partial class Attacks
{
    /// <summary>A texture layer: bullets fall in from the top of the field all the time, at random places (a drizzle, a drip, a rain of colours).</summary>
    private static PatternSpec TopRain(float interval, int n, BulletStyle style, float speed, float scale = 0.8f, float spread = 8) =>
        Pat.Every(interval, c => Pat.Rain(c, Edge.Top, n, spread, new Shot(style, speed).Sized(scale)));

    /// <summary>A texture layer from the sides: bullets come in level from the left and the right in turns.</summary>
    private static PatternSpec SideRain(float interval, int n, BulletStyle style, float speed, float scale = 0.8f, float spread = 26) =>
        Pat.Every(interval, c => Pat.Rain(c, c.Count % 2 == 0 ? Edge.Left : Edge.Right, n, spread, new Shot(style, speed).Sized(scale)));

    /// <summary>A texture layer in the seven colours of the rainbow, from the top.</summary>
    private static PatternSpec RainbowRain(float interval, int n, float speed, float scale = 0.8f) =>
        Pat.Every(interval, c => Pat.Rain(c, Edge.Top, n, 10, new Shot(RainbowBeads[c.Count % 7], speed).Sized(scale)));

    /// <summary>Adds patterns to an attack without touching its pressure (the late bosses pile layers on layers).</summary>
    private static BossPhase Plus(this BossPhase phase, params PatternSpec[] more) { phase.Patterns.AddRange(more); return phase; }

    /// <summary>Makes the bullets of the attack fly `factor` times as fast as its boss's others.</summary>
    /// <summary>Gives the attack `factor` times the health the builder would give it.</summary>
    private static BossPhase Tougher(this BossPhase phase, float factor) { phase.HealthScale = factor; return phase; }
    private static BossPhase Faster(this BossPhase phase, float factor) { phase.AttackSpeed = factor; return phase; }

    /// <summary>Sets how heavy the attack is made (see <see cref="PatternContext.Pressure"/>): tuned so that the bullets on the screen rise from boss to boss.</summary>
    private static BossPhase Pressured(this BossPhase phase, float pressure) { phase.Pressure = pressure; return phase; }

    /// <summary>Adds patterns to an attack (the attacks are written with their signature figures first and their texture layers after).</summary>
    private static BossPhase With(this BossPhase phase, float pressure, params PatternSpec[] more)
    {
        phase.Patterns.AddRange(more);
        phase.Pressure = pressure;
        return phase;
    }
}
