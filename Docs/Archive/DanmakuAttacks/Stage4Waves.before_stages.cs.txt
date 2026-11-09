using System.Collections.Generic;
using static WaveBuilder;

/// <summary>
/// The waves of stage 4, the river of the cave. The red spirits (type 3) now fight alongside the others: three waves bring one red spirit AND a crowd of
/// swarms and gunners around it (the combos). There is still only one red spirit at a time; two together come in the stages after this one.
/// The shots are shards of cave ice, in fans, rings that dance and bolts that zigzag.
/// </summary>
public static class Stage4Waves
{
    /// <summary>How long a red spirit stays, at most, once it has settled (the combos below hold it for this long).</summary>
    public const float HeavyHold = 24;

    private static List<ScheduledPattern> ShardFan => SpiritFire.ShardFan;
    private static List<ScheduledPattern> ShardRing => SpiritFire.ShardRing;
    private static List<ScheduledPattern> ShardBolts => SpiritFire.ShardBolts;
    private static List<ScheduledPattern> ShardSpiral => SpiritFire.ShardSpiral;

    /// <summary>The first combo's red spirit: slow rings of dancing hail and a wide fan of shards, with rests.</summary>
    private static List<ScheduledPattern> IceHeavy => SpiritFire.IceHeavy;

    /// <summary>The second: spirals of shards, bolts that zigzag, and a double ring that turns the other way.</summary>
    private static List<ScheduledPattern> StoneHeavy => SpiritFire.StoneHeavy;

    /// <summary>The third: everything, quicker, with little rest.</summary>
    private static List<ScheduledPattern> DeepHeavy => SpiritFire.DeepHeavy;

    public static readonly List<Wave> Waves = new List<Wave>
    {
        // 1. Conga lines in the dark, and gunners that throw shards.
        new Wave(Cat(Column(Swarm, 12, 110, 0, 0.3f, amplitude: 120, frequency: 1.9f),
                     Column(Swarm, 12, 490, 1.4f, 0.3f, amplitude: 120, frequency: 1.9f, phase: Pi),
                     Row(Gunner, 5, 120, 480, 5, speed: 105, shots: ShardFan),
                     Wall(Swarm, 10, 40, 560, new[] { 3, 6 }, 9.5f), Cross(Swarm, 8, 12, 0.33f),
                     SideColumn(Swarm, 8, true, 220, 3, 0.35f, amplitude: 60),
                     SideColumn(Swarm, 8, false, 290, 4.5f, 0.35f, amplitude: 60, phase: Pi))),

        // 2. Diamonds and crossing streams of gunners.
        new Wave(Cat(Diamond(Swarm, 3, 190, 0, spacing: 36), Diamond(Swarm, 3, 410, 2.4f, spacing: 36),
                     Stream(Gunner, 5, V(-30, 70), V(150, 90), 3, 0.8f, shots: ShardFan),
                     Stream(Gunner, 5, V(630, 70), V(-150, 90), 5, 0.8f, shots: ShardBolts),
                     Cross(Swarm, 9, 9, 0.35f), Wall(Swarm, 10, 40, 560, new[] { 1, 2 }, 13),
                     Sentries(Gunner, V(-30, 60), new[] { V(80, 150), V(190, 210) }, 5f, 0.8f, 9, SpiritFire.CaveClaws),
                     Sentries(Gunner, V(630, 60), new[] { V(520, 150), V(410, 210) }, 5.4f, 0.8f, 9, SpiritFire.CaveClaws))),

        // 3. FIRST COMBO: a red spirit settles, and the others come round it: pairs that weave, and gunners with rings.
        new Wave(Cat(Heavy(V(300, -30), V(300, 175), 0, HeavyHold, IceHeavy),
                     DancingPairs(Swarm, 10, 300, 240, 3, 0.4f, amplitude: 110),
                     Column(Gunner, 4, 120, 6, 1.3f, amplitude: 40, shots: ShardRing), Column(Gunner, 4, 480, 7, 1.3f, amplitude: 40, shots: ShardRing),
                     Wall(Swarm, 10, 40, 560, new[] { 4, 5 }, 12), Wall(Swarm, 10, 40, 560, new[] { 7, 8 }, 16),
                     Cross(Swarm, 8, 19, 0.33f))),

        // 4. Rings of spirits that come down together, a V, and gunners with spirals of shards.
        new Wave(Cat(Ring(Swarm, 12, V(180, -90), 70, 0), Ring(Swarm, 12, V(420, -90), 70, 2.4f), Ring(Swarm, 14, V(300, -100), 80, 5),
                     Vee(Swarm, 9, 300, 8, spacing: 40),
                     Column(Gunner, 3, 120, 3, 1.4f, amplitude: 30, shots: ShardSpiral), Column(Gunner, 3, 480, 3.7f, 1.4f, amplitude: 30, shots: ShardSpiral),
                     Cross(Swarm, 9, 11, 0.35f), Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 14),
                     RisingRow(Swarm, 7, 80, 520, 12, speed: 130),
                     Sentries(Gunner, V(300, 830), new[] { V(140, 190), V(460, 190) }, 6f, 0.6f, 9, SpiritFire.ShardRing))),

        // 5. SECOND COMBO: a red spirit with bolts and spirals, and crossing streams of gunners under it.
        new Wave(Cat(Heavy(V(300, -30), V(300, 170), 0, HeavyHold, StoneHeavy),
                     Stream(Gunner, 5, V(-30, 80), V(145, 80), 4, 0.9f, shots: ShardBolts), Stream(Gunner, 5, V(630, 80), V(-145, 80), 4, 0.9f, shots: ShardFan),
                     Column(Swarm, 9, 80, 3, 0.3f, amplitude: 60), Column(Swarm, 9, 520, 3.6f, 0.3f, amplitude: 60, phase: Pi),
                     Diamond(Swarm, 3, 300, 11, spacing: 36),
                     Wall(Swarm, 12, 30, 570, new[] { 2, 3 }, 16), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 20))),

        // 6. The crowd: everything, from every side.
        new Wave(Cat(DancingPairs(Swarm, 12, 300, 260, 0, 0.35f, amplitude: 120), Cross(Swarm, 10, 1.5f, 0.32f),
                     Row(Gunner, 5, 100, 500, 3, speed: 100, shots: ShardBolts),
                     Stream(Gunner, 5, V(-30, 90), V(145, 80), 6, 0.7f, shots: ShardFan), Stream(Gunner, 5, V(630, 90), V(-145, 80), 6, 0.7f, shots: ShardBolts),
                     Column(Gunner, 4, 300, 9, 1.1f, amplitude: 90, shots: ShardRing),
                     Wall(Swarm, 12, 30, 570, new[] { 5, 6 }, 12), Diamond(Swarm, 3, 160, 14, spacing: 36), Diamond(Swarm, 3, 440, 14, spacing: 36),
                     Sentries(Gunner, V(-30, 60), new[] { V(70, 140), V(170, 200), V(260, 120) }, 6f, 0.7f, 9, SpiritFire.CaveClaws),
                     Sentries(Gunner, V(630, 60), new[] { V(530, 140), V(430, 200), V(340, 120) }, 6.3f, 0.7f, 9, SpiritFire.ShardRing),
                     RisingRow(Swarm, 8, 60, 540, 13))),

        // 7. THIRD COMBO: the last red spirit, quick, with the crowd round it.
        new Wave(Cat(Heavy(V(300, -30), V(300, 170), 0, HeavyHold, DeepHeavy),
                     DancingPairs(Swarm, 12, 300, 220, 3, 0.35f, amplitude: 120),
                     Column(Gunner, 4, 110, 5, 1.2f, amplitude: 40, shots: ShardSpiral), Column(Gunner, 4, 490, 5.6f, 1.2f, amplitude: 40, shots: ShardSpiral),
                     Cross(Swarm, 9, 9, 0.33f), Cross(Swarm, 9, 17, 0.33f),
                     Row(Gunner, 5, 100, 500, 12, speed: 100, shots: ShardFan),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 14), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 18))),

        // 8. The last crowd, before the way opens to the bear.
        new Wave(Cat(DancingPairs(Swarm, 12, 300, 240, 0, 0.35f, amplitude: 120), Cross(Swarm, 10, 1.5f, 0.32f),
                     Row(Gunner, 5, 100, 500, 3, speed: 100, shots: ShardBolts),
                     Wall(Swarm, 12, 30, 570, new[] { 3, 4 }, 6), Wall(Swarm, 12, 30, 570, new[] { 8, 9 }, 8.5f),
                     Column(Gunner, 5, 300, 9, 1.0f, amplitude: 150, frequency: 1.6f, shots: ShardFan),
                     Diamond(Swarm, 3, 160, 12, spacing: 36), Diamond(Swarm, 3, 440, 12, spacing: 36),
                     Stream(Gunner, 4, V(-30, 80), V(145, 85), 14, 0.8f, shots: ShardRing), Stream(Gunner, 4, V(630, 80), V(-145, 85), 14, 0.8f, shots: ShardRing),
                     Sentries(Gunner, V(-30, 60), new[] { V(80, 130), V(180, 190) }, 4f, 0.7f, 10, SpiritFire.ShardFan),
                     Sentries(Gunner, V(630, 60), new[] { V(520, 130), V(420, 190) }, 4.3f, 0.7f, 10, SpiritFire.ShardFan),
                     RisingRow(Swarm, 8, 60, 540, 9),
                     SideColumn(Swarm, 8, true, 330, 11, 0.3f, amplitude: 80))),
    };
}
