using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 4, the river of the cave. The red spirits (type 3) now come in PAIRS, one at each side, with figures that mirror each other, alone in their wave (the others of types 1 and 2 come
/// in the waves between). The shots are small figures of cave ice: shards, rings that dance and bolts that zigzag. Everything comes from the top of the field or from its top corners.
/// </summary>
public static class Stage4Waves
{
    /// <summary>How long a red spirit stays, at most, once it has settled.</summary>
    public const float HeavyHold = 24;
    private static List<ScheduledPattern> ShardFan => SpiritFire.ShardFan;
    private static List<ScheduledPattern> ShardRing => SpiritFire.ShardRing;
    private static List<ScheduledPattern> ShardBolts => SpiritFire.ShardBolts;
    private static List<ScheduledPattern> ShardSpiral => SpiritFire.ShardSpiral;

    private static List<EnemySpec> Pair(float start, List<ScheduledPattern> left, List<ScheduledPattern> right) =>
        Cat(Heavy(V(170, -30), V(170, 180), start, HeavyHold, left), Heavy(V(430, -30), V(430, 180), start, HeavyHold, right));

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. Conga lines in the dark, and gunners that throw shards.
        new Wave(Cat(Column(Swarm, 8, 110, 0, 0.4f, amplitude: 120, frequency: 1.9f),
                     Column(Swarm, 8, 490, 1.4f, 0.4f, amplitude: 120, frequency: 1.9f, phase: Pi),
                     Row(Gunner, 3, 150, 450, 5, speed: 100, shots: ShardFan),
                     Wall(Swarm, 10, 40, 560, new[] { 3, 6 }, 9.5f), Cross(Swarm, 6, 12, 0.4f),
                     SideColumn(Swarm, 6, true, 120, 3, 0.4f, amplitude: 40),
                     SideColumn(Swarm, 6, false, 150, 4.5f, 0.4f, amplitude: 40, phase: Pi))),
        // 2. Diamonds and crossing streams of gunners.
        new Wave(Cat(Diamond(Swarm, 3, 190, 0, spacing: 36), Diamond(Swarm, 3, 410, 2.4f, spacing: 36),
                     Stream(Gunner, 3, V(-30, 70), V(150, 90), 3, 1.0f, shots: ShardFan),
                     Stream(Gunner, 3, V(630, 70), V(-150, 90), 5, 1.0f, shots: ShardBolts),
                     Cross(Swarm, 6, 9, 0.4f), Wall(Swarm, 10, 40, 560, new[] { 1, 2 }, 13),
                     Sentries(Gunner, V(-30, 60), new[] { V(100, 150) }, 5f, 0.8f, 7, SpiritFire.CaveClaws),
                     Sentries(Gunner, V(630, 60), new[] { V(500, 150) }, 5.4f, 0.8f, 7, SpiritFire.CaveClaws))),
        // 3. THE FIRST PAIR of red spirits, alone.
        new Wave(Pair(0, SpiritFire.IcePairLeft, SpiritFire.IcePairRight)),
        // 4. Rings of spirits that come down together, a V, and gunners with spirals of shards.
        new Wave(Cat(Ring(Swarm, 10, V(180, -90), 70, 0), Ring(Swarm, 10, V(420, -90), 70, 2.6f), Ring(Swarm, 12, V(300, -100), 80, 5.4f),
                     Vee(Swarm, 7, 300, 8.5f, spacing: 40),
                     Column(Gunner, 2, 120, 3, 1.6f, amplitude: 30, shots: ShardSpiral), Column(Gunner, 2, 480, 3.7f, 1.6f, amplitude: 30, shots: ShardSpiral),
                     Cross(Swarm, 6, 11, 0.4f), Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 14),
                     Sentries(Gunner, V(300, -30), new[] { V(160, 170), V(440, 170) }, 6f, 0.8f, 7, SpiritFire.ShardRing))),
        // 5. The crowd: gunners crossing, pairs that weave, walls.
        new Wave(Cat(DancingPairs(Swarm, 7, 300, 240, 0, 0.45f, amplitude: 110),
                     Stream(Gunner, 3, V(-30, 80), V(145, 80), 3, 1.0f, shots: ShardBolts), Stream(Gunner, 3, V(630, 80), V(-145, 80), 3, 1.0f, shots: ShardFan),
                     Column(Gunner, 3, 300, 8, 1.3f, amplitude: 90, shots: ShardRing),
                     Wall(Swarm, 12, 30, 570, new[] { 2, 3 }, 11), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 14), Diamond(Swarm, 3, 300, 15, spacing: 36))),
        // 6. THE SECOND PAIR, alone.
        new Wave(Pair(0, SpiritFire.StonePairLeft, SpiritFire.StonePairRight)),
        // 7. A breath: long lines of the little ones, crosses, and gunners from the corners.
        new Wave(Cat(Column(Swarm, 8, 90, 0, 0.4f, amplitude: 70), Column(Swarm, 8, 510, 0.6f, 0.4f, amplitude: 70, phase: Pi),
                     Cross(Swarm, 7, 5, 0.4f), Cross(Swarm, 7, 12, 0.4f),
                     Row(Gunner, 3, 150, 450, 9, speed: 100, shots: ShardFan),
                     Stream(Gunner, 3, V(-30, 80), V(145, 85), 14, 1.0f, shots: ShardRing), Stream(Gunner, 3, V(630, 80), V(-145, 85), 14, 1.0f, shots: ShardRing),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 16))),
        // 8. The last crowd, before the way opens to the bear.
        new Wave(Cat(DancingPairs(Swarm, 8, 300, 240, 0, 0.42f, amplitude: 120), Cross(Swarm, 7, 1.5f, 0.4f),
                     Row(Gunner, 3, 150, 450, 3, speed: 100, shots: ShardBolts),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 6), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 8.5f),
                     Column(Gunner, 3, 300, 9, 1.3f, amplitude: 150, frequency: 1.6f, shots: ShardFan),
                     Diamond(Swarm, 3, 160, 12, spacing: 36), Diamond(Swarm, 3, 440, 12, spacing: 36),
                     Sentries(Gunner, V(-30, 60), new[] { V(90, 150), V(190, 200) }, 4f, 0.8f, 8, SpiritFire.ShardFan),
                     Sentries(Gunner, V(630, 60), new[] { V(510, 150), V(410, 200) }, 4.3f, 0.8f, 8, SpiritFire.ShardFan),
                     SideColumn(Swarm, 7, true, 130, 11, 0.4f, amplitude: 50))),
    };
}
