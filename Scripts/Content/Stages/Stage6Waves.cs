using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 6, the river of stars: the last and the longest. Every kind of spirit of the game comes, from stage 1 on: the little ones in every formation, the gunners in
/// every stream, and the red ones (type 3), which here come in up to THREE at a time: a lone one, a pair, and at the end a trio, with the others round them. The shots are of silver and of
/// starlight. The rhythm: the crowd, a breath, a heavy, the crowd again; each red spirit is worse than the last, and the last wave has the trio and everything else.
/// </summary>
public static class Stage6Waves
{
    /// <summary>How long a red spirit stays, at most, once it has settled.</summary>
    public const float HeavyHold = 24;

    private static List<ScheduledPattern> SilverFan => new List<ScheduledPattern> { P(FirePattern.Fan(1.4f, 3, 36, 200).Styled(BulletStyle.Wisp), 1000) };
    private static List<ScheduledPattern> StarRing => new List<ScheduledPattern> { P(FirePattern.Ring(1.9f, 10, 105, 8).Styled(BulletStyle.Star).Dancing(70), 1000) };
    private static List<ScheduledPattern> VioletBolts => new List<ScheduledPattern> { P(FirePattern.Fan(0.9f, 3, 30, 215).Styled(BulletStyle.VenomOrb).Zigzag(60), 2.7f), P(FirePattern.None, 1.5f) };
    private static List<ScheduledPattern> GreenSpiral => new List<ScheduledPattern> { P(FirePattern.Spiral(0.28f, 2, 26, 145).Styled(BulletStyle.OrbGreen), 2.4f), P(FirePattern.None, 1.5f) };

    private static List<ScheduledPattern> LoneHeavy => new List<ScheduledPattern>
    {
        P(FirePattern.Spiral(0.09f, 4, 18, 155).Styled(BulletStyle.OrbRed), 3.2f),
        P(FirePattern.Fan(0.5f, 5, 50, 220).Styled(BulletStyle.Star).Zigzag(60), 3.0f),
        P(FirePattern.Ring(0.5f, 18, 120, -13).Styled(BulletStyle.Wisp).Dancing(80), 3.4f),
        P(FirePattern.None, 1.4f),
    };

    /// <summary>The ones of a pair or of a trio: the left turns one way, the right the other, the middle one fires fans.</summary>
    private static List<ScheduledPattern> Left => new List<ScheduledPattern>
    {
        P(FirePattern.Spiral(0.1f, 3, 22, 150).Styled(BulletStyle.OrbRed), 3.2f),
        P(FirePattern.Ring(0.6f, 14, 115, 12).Styled(BulletStyle.Wisp).Dancing(70), 3.2f),
        P(FirePattern.Fan(0.5f, 5, 50, 225).Styled(BulletStyle.Star).Zigzag(60), 3.0f),
        P(FirePattern.None, 1.4f),
    };

    private static List<ScheduledPattern> Right => new List<ScheduledPattern>
    {
        P(FirePattern.Spiral(0.1f, 3, -22, 150).Styled(BulletStyle.OrbGreen), 3.2f),
        P(FirePattern.Ring(0.6f, 14, 115, -12).Styled(BulletStyle.LotusPetal).Dancing(70), 3.2f),
        P(FirePattern.Fan(0.5f, 5, 50, 225).Styled(BulletStyle.VenomOrb).Zigzag(60), 3.0f),
        P(FirePattern.None, 1.4f),
    };

    private static List<ScheduledPattern> Middle => new List<ScheduledPattern>
    {
        P(FirePattern.Fan(0.7f, 9, 80, 230).Styled(BulletStyle.Fireball), 3.0f),
        P(FirePattern.Spiral(0.09f, 4, 17, 160).Styled(BulletStyle.OrbRed), 3.2f),
        P(FirePattern.Ring(0.5f, 18, 120, 13).Styled(BulletStyle.Star).Dancing(80), 3.2f),
        P(FirePattern.None, 1.2f),
    };

    private static List<EnemySpec> Pair(float start) => Cat(Heavy(V(170, -30), V(170, 180), start, HeavyHold, Left), Heavy(V(430, -30), V(430, 180), start, HeavyHold, Right));

    private static List<EnemySpec> Trio(float start) =>
        Cat(Heavy(V(110, -30), V(110, 190), start, HeavyHold, Left), Heavy(V(300, -30), V(300, 150), start + 0.6f, HeavyHold, Middle), Heavy(V(490, -30), V(490, 190), start, HeavyHold, Right));

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. The crowd opens: conga lines and gunners.
        new Wave(Cat(Column(Swarm, 12, 110, 0, 0.3f, amplitude: 120, frequency: 1.9f), Column(Swarm, 12, 490, 1.4f, 0.3f, amplitude: 120, frequency: 1.9f, phase: Pi),
                     Row(Gunner, 5, 120, 480, 5, speed: 105, shots: SilverFan), Wall(Swarm, 10, 40, 560, new[] { 3, 6 }, 9.5f), Cross(Swarm, 9, 12, 0.32f))),

        // 2. Diamonds and crossing streams.
        new Wave(Cat(Diamond(Swarm, 3, 190, 0, spacing: 36), Diamond(Swarm, 3, 410, 2.4f, spacing: 36),
                     Stream(Gunner, 5, V(-30, 70), V(150, 90), 3, 0.8f, shots: SilverFan), Stream(Gunner, 5, V(630, 70), V(-150, 90), 5, 0.8f, shots: VioletBolts),
                     Cross(Swarm, 9, 9, 0.35f), Wall(Swarm, 10, 40, 560, new[] { 1, 2 }, 13))),

        // 3. A red spirit alone among the crowd.
        new Wave(Cat(Heavy(V(300, -30), V(300, 175), 0, HeavyHold, LoneHeavy),
                     DancingPairs(Swarm, 12, 300, 260, 3, 0.35f, amplitude: 120), Cross(Swarm, 10, 9, 0.33f),
                     Stream(Gunner, 5, V(-30, 80), V(145, 80), 4, 0.9f, shots: VioletBolts), Stream(Gunner, 5, V(630, 80), V(-145, 80), 4, 0.9f, shots: SilverFan),
                     Wall(Swarm, 12, 30, 570, new[] { 2, 3 }, 16), Diamond(Swarm, 3, 300, 12, spacing: 36))),

        // 4. A breath: rings, a V and long lines of the little ones.
        new Wave(Cat(Ring(Swarm, 12, V(180, -90), 70, 0), Ring(Swarm, 12, V(420, -90), 70, 2.4f), Ring(Swarm, 14, V(300, -100), 80, 5), Vee(Swarm, 9, 300, 8, spacing: 40),
                     Column(Gunner, 3, 120, 3, 1.4f, amplitude: 30, shots: GreenSpiral), Column(Gunner, 3, 480, 3.7f, 1.4f, amplitude: 30, shots: GreenSpiral),
                     Cross(Swarm, 9, 11, 0.35f), Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 14))),

        // 5. THE FIRST PAIR of red spirits, with the gunners.
        new Wave(Cat(Pair(0),
                     Column(Gunner, 4, 300, 5, 1.3f, amplitude: 70, shots: StarRing), Column(Gunner, 3, 80, 9, 1.3f, amplitude: 30, shots: GreenSpiral), Column(Gunner, 3, 520, 9.6f, 1.3f, amplitude: 30, shots: GreenSpiral),
                     Stream(Gunner, 4, V(-30, 80), V(145, 85), 14, 0.9f, shots: SilverFan), Stream(Gunner, 4, V(630, 80), V(-145, 80), 14, 0.9f, shots: VioletBolts))),

        // 6. The crowd: everything, from every side.
        new Wave(Cat(DancingPairs(Swarm, 12, 300, 260, 0, 0.35f, amplitude: 120), Cross(Swarm, 10, 1.5f, 0.32f),
                     Row(Gunner, 5, 100, 500, 3, speed: 100, shots: VioletBolts),
                     Stream(Gunner, 5, V(-30, 90), V(145, 80), 6, 0.7f, shots: SilverFan), Stream(Gunner, 5, V(630, 90), V(-145, 80), 6, 0.7f, shots: VioletBolts),
                     Column(Gunner, 4, 300, 9, 1.1f, amplitude: 90, shots: StarRing),
                     Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 12), Diamond(Swarm, 3, 160, 14, spacing: 36), Diamond(Swarm, 3, 440, 14, spacing: 36))),

        // 7. THE FIRST TRIO of red spirits: three at once, with the gunners.
        new Wave(Cat(Trio(0),
                     Column(Gunner, 4, 300, 6, 1.3f, amplitude: 60, shots: StarRing), Row(Gunner, 5, 100, 500, 10, speed: 100, shots: SilverFan),
                     Stream(Gunner, 4, V(-30, 80), V(145, 85), 15, 0.9f, shots: VioletBolts), Stream(Gunner, 4, V(630, 80), V(-145, 85), 15, 0.9f, shots: SilverFan))),

        // 8. A breath again, with a lone red spirit that arrives late.
        new Wave(Cat(DancingPairs(Swarm, 12, 300, 240, 0, 0.35f, amplitude: 120), Cross(Swarm, 10, 1.5f, 0.32f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 6), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 8.5f),
                     Column(Gunner, 5, 300, 9, 1.0f, amplitude: 150, frequency: 1.6f, shots: SilverFan),
                     Heavy(V(300, -30), V(300, 170), 12, HeavyHold, LoneHeavy),
                     Diamond(Swarm, 3, 160, 12, spacing: 36), Diamond(Swarm, 3, 440, 12, spacing: 36))),

        // 9. THE SECOND PAIR, with a crowd of the little ones round them.
        new Wave(Cat(Pair(0),
                     DancingPairs(Swarm, 14, 300, 250, 3, 0.32f, amplitude: 130), Cross(Swarm, 10, 5, 0.3f), Cross(Swarm, 10, 14, 0.3f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 9), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 12.5f),
                     Ring(Swarm, 12, V(180, -90), 70, 7), Ring(Swarm, 12, V(420, -90), 70, 10), Vee(Swarm, 9, 300, 15, spacing: 40),
                     Column(Gunner, 4, 110, 12, 1.2f, amplitude: 40, shots: GreenSpiral), Column(Gunner, 4, 490, 12.6f, 1.2f, amplitude: 40, shots: GreenSpiral))),

        // 10. THE LAST TRIO, and everything else: every kind of spirit of the game at once.
        new Wave(Cat(Trio(0),
                     DancingPairs(Swarm, 14, 300, 250, 3, 0.32f, amplitude: 130), Cross(Swarm, 10, 5, 0.3f), Cross(Swarm, 10, 14, 0.3f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 9), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 12.5f), Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 17),
                     Stream(Gunner, 5, V(-30, 80), V(145, 85), 7, 0.8f, shots: VioletBolts), Stream(Gunner, 5, V(630, 80), V(-145, 85), 7, 0.8f, shots: SilverFan),
                     Column(Gunner, 4, 300, 14, 1.2f, amplitude: 80, shots: StarRing),
                     Column(Swarm, 10, 90, 18, 0.3f, amplitude: 60), Column(Swarm, 10, 510, 18.6f, 0.3f, amplitude: 60, phase: Pi))),
    };
}
