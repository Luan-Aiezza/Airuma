using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 5, the waterfall of the seven colours. The red spirits (type 3) come in PAIRS, one at each side, with figures that mirror each other, and now the pairs come ACCOMPANIED by the spirits
/// of types 1 and 2; a lone red spirit comes now and then among the others, as in stage 3. The shots are small figures in the colours of the rainbow. Everything comes from the top of the field or from its top corners.
/// </summary>
public static class Stage5Waves
{
    /// <summary>How long a red spirit stays, at most, once it has settled.</summary>
    public const float HeavyHold = 24;
    private static List<ScheduledPattern> RainbowFan => SpiritFire.RainbowFan;
    private static List<ScheduledPattern> BlueRing => SpiritFire.BlueRing;
    private static List<ScheduledPattern> VioletBolts => SpiritFire.VioletBolts;
    private static List<ScheduledPattern> GreenSpiral => SpiritFire.GreenSpiral;

    private static List<EnemySpec> Pair(float start, List<ScheduledPattern> left, List<ScheduledPattern> right) =>
        Cat(Heavy(V(170, -30), V(170, 180), start, HeavyHold, left), Heavy(V(430, -30), V(430, 180), start, HeavyHold, right));

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. Conga lines, and gunners that throw the colours.
        new Wave(Cat(Column(Swarm, 9, 110, 0, 0.38f, amplitude: 120, frequency: 1.9f), Column(Swarm, 9, 490, 1.4f, 0.38f, amplitude: 120, frequency: 1.9f, phase: Pi),
                     Row(Gunner, 3, 150, 450, 5, speed: 100, shots: RainbowFan),
                     Wall(Swarm, 10, 40, 560, new[] { 3, 6 }, 9.5f), Cross(Swarm, 7, 12, 0.38f),
                     SideColumn(Swarm, 6, true, 120, 3, 0.4f, amplitude: 40),
                     SideColumn(Swarm, 6, false, 150, 4.5f, 0.4f, amplitude: 40, phase: Pi))),
        // 2. Diamonds, and crossing streams of gunners.
        new Wave(Cat(Diamond(Swarm, 3, 190, 0, spacing: 36), Diamond(Swarm, 3, 410, 2.4f, spacing: 36),
                     Stream(Gunner, 3, V(-30, 70), V(150, 90), 3, 1.0f, shots: RainbowFan), Stream(Gunner, 3, V(630, 70), V(-150, 90), 5, 1.0f, shots: VioletBolts),
                     Cross(Swarm, 7, 9, 0.38f), Wall(Swarm, 10, 40, 560, new[] { 1, 2 }, 13),
                     Sentries(Gunner, V(-30, 60), new[] { V(100, 150) }, 5f, 0.8f, 7, SpiritFire.Rainbow),
                     Sentries(Gunner, V(630, 60), new[] { V(500, 150) }, 5.4f, 0.8f, 7, SpiritFire.Rainbow))),
        // 3. THE FIRST PAIR of red spirits, with the gunners and a few of the little ones.
        new Wave(Cat(Pair(0, SpiritFire.LeftOfPair, SpiritFire.RightOfPair),
                     Column(Gunner, 2, 300, 5, 1.5f, amplitude: 70, shots: BlueRing), Column(Swarm, 6, 80, 8, 0.45f, amplitude: 40), Column(Swarm, 6, 520, 8.6f, 0.45f, amplitude: 40, phase: Pi),
                     Stream(Gunner, 2, V(-30, 80), V(145, 85), 13, 1.1f, shots: RainbowFan), Stream(Gunner, 2, V(630, 80), V(-145, 85), 13, 1.1f, shots: VioletBolts))),
        // 4. Rings of spirits that come down together, a V, and gunners with spirals.
        new Wave(Cat(Ring(Swarm, 10, V(180, -90), 70, 0), Ring(Swarm, 10, V(420, -90), 70, 2.6f), Ring(Swarm, 12, V(300, -100), 80, 5.4f), Vee(Swarm, 7, 300, 8.5f, spacing: 40),
                     Column(Gunner, 2, 120, 3, 1.6f, amplitude: 30, shots: GreenSpiral), Column(Gunner, 2, 480, 3.7f, 1.6f, amplitude: 30, shots: GreenSpiral),
                     Cross(Swarm, 7, 11, 0.38f), Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 14),
                     Sentries(Gunner, V(300, -30), new[] { V(160, 170), V(440, 170) }, 6f, 0.8f, 7, SpiritFire.BlueRing))),
        // 5. A red spirit alone among the crowd.
        new Wave(Cat(Heavy(V(300, -30), V(300, 175), 0, HeavyHold, SpiritFire.RainbowHeavy),
                     DancingPairs(Swarm, 7, 300, 260, 3, 0.45f, amplitude: 120), Cross(Swarm, 7, 9, 0.38f),
                     Stream(Gunner, 3, V(-30, 80), V(145, 80), 4, 1.0f, shots: VioletBolts), Stream(Gunner, 3, V(630, 80), V(-145, 80), 4, 1.0f, shots: RainbowFan),
                     Wall(Swarm, 12, 30, 570, new[] { 2, 3 }, 16), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 20), Diamond(Swarm, 3, 300, 12, spacing: 36))),
        // 6. The crowd: everything, from the top.
        new Wave(Cat(DancingPairs(Swarm, 8, 300, 260, 0, 0.42f, amplitude: 120), Cross(Swarm, 8, 1.5f, 0.38f),
                     Row(Gunner, 3, 150, 450, 3, speed: 100, shots: VioletBolts),
                     Stream(Gunner, 3, V(-30, 90), V(145, 80), 6, 1.0f, shots: RainbowFan), Stream(Gunner, 3, V(630, 90), V(-145, 80), 6, 1.0f, shots: VioletBolts),
                     Column(Gunner, 3, 300, 9, 1.3f, amplitude: 90, shots: BlueRing),
                     Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 12), Diamond(Swarm, 3, 160, 14, spacing: 36), Diamond(Swarm, 3, 440, 14, spacing: 36),
                     Sentries(Gunner, V(-30, 60), new[] { V(90, 150), V(190, 200) }, 6f, 0.8f, 8, SpiritFire.RainbowFan),
                     Sentries(Gunner, V(630, 60), new[] { V(510, 150), V(410, 200) }, 6.3f, 0.8f, 8, SpiritFire.Rainbow))),
        // 7. THE SECOND PAIR, quicker, with the gunners.
        new Wave(Cat(Pair(0, SpiritFire.LeftOfSecondPair, SpiritFire.RightOfSecondPair),
                     Column(Gunner, 3, 300, 4, 1.4f, amplitude: 80, shots: BlueRing), Row(Gunner, 3, 150, 450, 8, speed: 100, shots: RainbowFan),
                     Column(Swarm, 7, 110, 11, 0.4f, amplitude: 40), Column(Swarm, 7, 490, 11.6f, 0.4f, amplitude: 40, phase: Pi),
                     Stream(Gunner, 3, V(-30, 80), V(145, 85), 15, 1.0f, shots: VioletBolts), Stream(Gunner, 3, V(630, 80), V(-145, 85), 15, 1.0f, shots: RainbowFan))),
        // 8. A breath: spirits of the first kind in long lines and walls, and a few gunners.
        new Wave(Cat(DancingPairs(Swarm, 8, 300, 240, 0, 0.42f, amplitude: 120), Cross(Swarm, 8, 1.5f, 0.38f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 6), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 8.5f),
                     Column(Gunner, 3, 300, 9, 1.3f, amplitude: 150, frequency: 1.6f, shots: RainbowFan),
                     Diamond(Swarm, 3, 160, 12, spacing: 36), Diamond(Swarm, 3, 440, 12, spacing: 36),
                     Sentries(Gunner, V(-30, 60), new[] { V(100, 150) }, 4f, 0.8f, 8, SpiritFire.Rainbow),
                     Sentries(Gunner, V(630, 60), new[] { V(500, 150) }, 4.3f, 0.8f, 8, SpiritFire.Rainbow),
                     SideColumn(Swarm, 7, true, 130, 11, 0.4f, amplitude: 50))),
        // 9. THE LAST PAIR, with a crowd of the little spirits and gunners round them.
        new Wave(Cat(Pair(0, SpiritFire.LeftOfSecondPair, SpiritFire.RightOfSecondPair),
                     DancingPairs(Swarm, 8, 300, 250, 3, 0.4f, amplitude: 130), Cross(Swarm, 8, 5, 0.38f), Cross(Swarm, 8, 14, 0.38f),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 9), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 12.5f),
                     Ring(Swarm, 10, V(180, -90), 70, 7), Ring(Swarm, 10, V(420, -90), 70, 10), Vee(Swarm, 7, 300, 15, spacing: 40),
                     Stream(Gunner, 2, V(-30, 80), V(145, 85), 6, 1.2f, shots: VioletBolts), Stream(Gunner, 2, V(630, 80), V(-145, 85), 6, 1.2f, shots: RainbowFan),
                     Column(Swarm, 8, 90, 18, 0.38f, amplitude: 60), Column(Swarm, 8, 510, 18.6f, 0.38f, amplitude: 60, phase: Pi))),
    };
}
