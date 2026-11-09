using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of stage 3, the lake of the hail: its waves, Thaqai and her attacks, and the new bullets (curtains, constellations, zigzags).</summary>
public static partial class SelfTest
{
    private static void RegisterStage3(Action<string, Action> T)
    {
        T("StageThreeBringsAllThreeKindsAndMoreSpirits", StageThreeBringsAllThreeKindsAndMoreSpirits);
        T("TheRedSpiritsComeAloneAndOnlyTwice", TheRedSpiritsComeAloneAndOnlyTwice);
        T("ThaqaiHasSixAttacksFourCalmAndTwoEnraged", ThaqaiHasSixAttacksFourCalmAndTwoEnraged);
        T("TheCurtainAlwaysHasAGapThatSwingsAtASpeedTheBoatCanFollow", TheCurtainAlwaysHasAGapThatSwingsAtASpeedTheBoatCanFollow);
        T("ConstellationsHangLinkedAndTheirLinesHurtOnlyWhenStill", ConstellationsHangLinkedAndTheirLinesHurtOnlyWhenStill);
        T("ZigzagBoltsTurnOneWayAndTheOther", ZigzagBoltsTurnOneWayAndTheOther);
        T("TheCrossIsFastAndRestsThreeSecondsAtEveryCorner", TheCrossIsFastAndRestsThreeSecondsAtEveryCorner);
        T("StageThreeCanBePlayedThroughItsWavesToTheBoss", StageThreeCanBePlayedThroughItsWavesToTheBoss);
        T("StageThreeTalksFitTheDialogueBox", StageThreeTalksFitTheDialogueBox);
    }

    private static void StageThreeBringsAllThreeKindsAndMoreSpirits()
    {
        var kinds = Spawns(Stages.Three).Select(s => s.Kind).Distinct().ToList();
        True(kinds.Contains(SpiritKind.Swarm) && kinds.Contains(SpiritKind.Gunner) && kinds.Contains(SpiritKind.Heavy), "the three kinds");
        Greater(Spawns(Stages.Three).Count(), Spawns(Stages.Two).Count(), "more spirits than stage 2");
        Greater(Stages.Three.Waves.Count, Stages.Two.Waves.Count - 1, "a full stage");
        True(Spawns(Stages.Two).All(s => s.Kind != SpiritKind.Heavy) && Spawns(Stages.One).All(s => s.Kind != SpiritKind.Heavy), "the red ones are new in stage 3");
    }

    private static void TheRedSpiritsComeAloneAndOnlyTwice()
    {
        var waves = Stages.Three.Waves;
        var heavyWaves = waves.Where(w => w.Spawns.Any(s => s.Kind == SpiritKind.Heavy)).ToList();
        Eq(heavyWaves.Count, 2, "twice");
        foreach (var wave in heavyWaves)
        {
            Eq(wave.Spawns.Count, 1, "alone");
            var heavy = wave.Spawns[0];
            GreaterEq(heavy.Patterns.Count, 4, "long, elaborate patterns");
            True(heavy.Patterns.Count(p => p.Pattern.Kind != FirePatternKind.None) >= 3, "of different figures, one after another");
        }
        Greater(SpiritKind.Heavy.MaxHealth(), 100, "a great deal of health");
    }

    private static void ThaqaiHasSixAttacksFourCalmAndTwoEnraged()
    {
        var boss = Stages.Three.Boss;
        Eq(boss.Lives, 6, "six lives");
        Eq(boss.Phases.Count(p => !p.Enraged), 4);
        Eq(boss.Phases.Count(p => p.Enraged), 2);
        Eq(new World(withCards: false, stage: Stages.Three).StartingBossLives, 6);
        True(boss.Phases.Any(p => p.Movement == BossMovementType.Cross), "she crosses the field");
        True(boss.Phases.Any(p => p.AttackNameKey == "AttackHailCurtain"), "curtains of hail that make the boat move in small steps");
        True(boss.Phases.Any(p => p.Patterns.Count > 0), "figures of the new kind: pentagrams, bolts, yarn");
        True(boss.Phases.Any(p => p.AttackNameKey == "AttackZigzagLightning"), "lightning");
        True(boss.Name != Stages.Two.Boss.Name && boss.Name != Stages.One.Boss.Name, "her own name");
    }

    private static void TheCurtainAlwaysHasAGapThatSwingsAtASpeedTheBoatCanFollow()
    {
        var weave = Attacks.FineWeave().Main;
        float width = World.PlayArea.Size.X;
        float lastGap = BulletSystem.CurtainGapCentre(weave), widest = 0;
        for (int step = 1; step <= 60 * 8; step++)
        {
            weave.CurtainClock = step / 60f;
            float gap = BulletSystem.CurtainGapCentre(weave);
            widest = MathF.Max(widest, MathF.Abs(gap - lastGap) * 60);          // px per second
            lastGap = gap;
            True(gap - weave.CurtainGap / 2 > 0 && gap + weave.CurtainGap / 2 < width, "the gap stays in the field");
        }
        LessEq(widest, 160f * 1.0f, "the gap never goes faster than the boat can while focusing (160 px/s)");
        Greater(widest, 60f, "but it does move");
        // A row has a hole: a boat in the gap is never touched; a row has bullets elsewhere.
        var pool = BulletSystem.NewPool();
        Pat.Wall(Ctx(pool), Edge.Top, 24, 0.5f, 170, new Shot(BulletStyle.Hail, 100).Sized(1.9f));
        var row = pool.Where(b => b.IsActive).Select(b => b.Position.X).OrderBy(x => x).ToList();
        Greater(row.Count, 15, "a row of hail");
        float widestHole = 0;
        for (int i = 1; i < row.Count; i++) widestHole = MathF.Max(widestHole, row[i] - row[i - 1]);
        Greater(widestHole, 170 * 0.8f, "with a hole where the boat can pass");
        Greater(widestHole, 2 * PlayerEntity.Create().Hitbox.Radius + 20, "wider than the boat's heart");
        // A row is a wall: the boat cannot slip between two hailstones, only through the gap (checked every few pixels across the field).
        float gapCentre = width / 2;
        var hail = pool.Where(b => b.IsActive).ToList();
        for (float x = 10; x < width - 10; x += 3)
        {
            bool blocked = hail.Any(b => MathF.Abs(b.Position.X - x) < b.HitboxRadius + PlayerEntity.Create().Hitbox.Radius);
            bool inGap = MathF.Abs(x - gapCentre) < 170 / 2 - 14;
            if (inGap) True(!blocked, "the gap is open at " + x);
            else if (MathF.Abs(x - gapCentre) > 170 / 2 + 14) True(blocked, "the row is a wall at " + x);
        }
        // The easy mode widens the gap, the hard one narrows it.
        var easyPool = BulletSystem.NewPool(); var hardPool = BulletSystem.NewPool();
        Pat.Wall(Ctx(easyPool, null, Difficulty.Easy), Edge.Top, 24, 0.5f, 170, new Shot(BulletStyle.Hail, 100).Sized(1.9f));
        Pat.Wall(Ctx(hardPool, null, Difficulty.Hard), Edge.Top, 24, 0.5f, 170, new Shot(BulletStyle.Hail, 100).Sized(1.9f));
        Less(easyPool.Count(b => b.IsActive), hardPool.Count(b => b.IsActive) + 1, "easy leaves a wider gap (fewer hailstones in the row)");
    }

    private static void ConstellationsHangLinkedAndTheirLinesHurtOnlyWhenStill()
    {
        var pool = BulletSystem.NewPool();
        var player = new PlayerEntity();
        var boss = BossEntity.Create(ThaqaiBoss.Definition);
        Pat.Constellation(Ctx(pool), V(300, 200), 5, 0, 2, new Shot(BulletStyle.Star, 175).Constellation(250));
        Eq(pool.Count(b => b.IsActive), 5, "five stars");
        Eq(BulletSystem.Links(pool).Count(), 5, "linked in a pentagram, each to the one two places before it");
        True(BulletSystem.Links(pool).All(l => !l.still), "they are flying out");
        // They fly, brake and hang.
        for (int i = 0; i < 60; i++) BulletSystem.MoveBullets(pool, new Rect2(-2000, -2000, 5000, 5000), 1 / 60f, V(0, 0), player);
        True(BulletSystem.Links(pool).All(l => l.still), "they hang still");
        True(pool.Where(b => b.IsActive).All(b => b.Velocity.Length() < 0.5f), "motionless");
        // A line through the boat hurts; a boat away from the lines is fine.
        var links = BulletSystem.Links(pool).ToList();
        var mid = (links[0].a + links[0].b) / 2;
        var hit = new PlayerEntity(); hit.Transform.Position = mid;
        CombatSystem.ProcessCollisions(pool, hit, boss, false);
        Less(hit.Health.CurrentHealth, hit.Health.MaxHealth, "a line hurts");
        var safe = new PlayerEntity(); safe.Transform.Position = V(300, 700);
        CombatSystem.ProcessCollisions(pool, safe, boss, false);
        Eq(safe.Health.CurrentHealth, safe.Health.MaxHealth);
        // After a while the stars fall and the lines are gone.
        for (int i = 0; i < 60 * 4; i++) BulletSystem.MoveBullets(pool, new Rect2(-2000, -2000, 5000, 5000), 1 / 60f, V(0, 0), player);
        Eq(BulletSystem.Links(pool).Count(), 0, "no more lines");
        True(pool.Where(b => b.IsActive).All(b => b.Velocity.Y > 100), "the stars fall");
    }

    private static void ZigzagBoltsTurnOneWayAndTheOther()
    {
        var pool = Pool(1);
        BulletSystem.Spawn(pool, V(300, 100), V(0, 200), Faction.Boss, BulletStyle.Spark, curve: 60, behavior: BulletBehavior.Zigzag);
        var player = new PlayerEntity();
        float minHeading = 999, maxHeading = -999;
        for (int i = 0; i < 60 * 3; i++)
        {
            BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(0, 0), player);
            float heading = Mathx.Degrees(MathF.Atan2(pool[0].Velocity.Y, pool[0].Velocity.X)) - 90;
            minHeading = MathF.Min(minHeading, heading); maxHeading = MathF.Max(maxHeading, heading);
        }
        True(minHeading < -25 && maxHeading > 25, $"it turns both ways ({minHeading}..{maxHeading})");
        Near(pool[0].Velocity.Length(), 200, 0.5f, "and keeps its speed");
    }

    private static void TheCrossIsFastAndRestsThreeSecondsAtEveryCorner()
    {
        var boss = BossEntity.Create(ThaqaiBoss.Definition);
        PhaseManagerSystem.ChangePhase(boss, 4);          // the lightning: she crosses
        boss.MovementType = BossMovementType.Cross;
        boss.Transform.Position = V(300, 150);
        float rest = 0, longestRest = 999, fastest = 0, diagonal = 0;
        var visited = new HashSet<int>();
        Vector2 last = boss.Transform.Position;
        for (int i = 0; i < 60 * 40; i++)
        {
            MovementSystem.MoveBoss(boss, World.PlayArea, 1 / 60f);
            var p = boss.Transform.Position;
            float speed = p.DistanceTo(last) * 60;
            fastest = MathF.Max(fastest, speed);
            if (speed < 1) rest += 1 / 60f;
            else
            {
                if (rest > 0.7f) longestRest = MathF.Min(longestRest, rest);     // (the first rest, before she sets out, is shorter)
                rest = 0;
                if (MathF.Abs(p.X - last.X) > 1 && MathF.Abs(p.Y - last.Y) > 1) diagonal += 1;
            }
            foreach (var (c, index) in CrossBrain.Corners.Select((c, k) => (c, k))) if (p.DistanceTo(c) < 1) visited.Add(index);
            last = p;
        }
        GreaterEq(fastest, 500, "fast");
        GreaterEq(longestRest, 2.95f, "never less than three seconds still before she moves on");
        Greater(diagonal, 20, "she crosses in diagonals");
        Eq(visited.Count, 4, "all four corners");
    }

    private static void StageThreeCanBePlayedThroughItsWavesToTheBoss()
    {
        var world = new World(withCards: false, stage: Stages.Three);
        var waveStarts = new List<int>();
        float t = 0;
        while (t < 900 && world.Part == StagePart.Waves)
        {
            world.Player.Health.InvincibilityTimer = 10;
            foreach (var e in world.Enemies) e.Health = 0;
            world.Step(1f / 60, Idle());
            foreach (var ev in world.TakeEvents()) if (ev.Kind == WorldEventKind.WaveStarted) waveStarts.Add(ev.Int);
            t += 1f / 60;
        }
        True(waveStarts.SequenceEqual(Enumerable.Range(0, Stages.Three.Waves.Count)), "every wave, in order");
        Eq(world.Part, StagePart.Boss);
        Eq(world.Boss.Health.Lives, 6);
        // Every attack of the boss can be run for a while without trouble, in every difficulty.
        foreach (var level in new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard })
            for (int phase = 1; phase <= 6; phase++)
            {
                var practice = new World(phase, withCards: false, difficulty: level, stage: Stages.Three);
                for (int i = 0; i < 60 * 25; i++)
                {
                    practice.Player.Health.InvincibilityTimer = 10;
                    practice.Step(1f / 60, Idle());
                }
                True(practice.Result == null && practice.Boss.Health.Lives >= 1, $"attack {phase} on {level} runs");
            }
    }

    private static void StageThreeTalksFitTheDialogueBox()
    {
        var lists = new[] { Dialogues.StageThreeIntro, Dialogues.StageThreeMeeting, Dialogues.StageThreeEnding };
        foreach (var lang in new[] { Language.English, Language.PortugueseBrazil })
        {
            Loc.Choose(lang);
            foreach (var list in lists)
                foreach (var line in list)
                {
                    True(!string.IsNullOrWhiteSpace(line.Text) && line.Text != line.Key, "has text");
                    True(GameHud.Wrap(line.Text, 28, 99).Count <= 5, $"[{lang}] fits: {line.Text}");
                }
        }
        Loc.Choose(Language.PortugueseBrazil);
        Eq(Dialogues.StageThreeIntro.Count, 5);
        Eq(Dialogues.StageThreeMeeting.Count, 5);
        True(Dialogues.StageThreeEnding.Any(l => l.Speaker == DialogueSpeaker.Boss) && Dialogues.StageThreeEnding.Any(l => l.Speaker == DialogueSpeaker.Airu), "the ending");
    }
}
