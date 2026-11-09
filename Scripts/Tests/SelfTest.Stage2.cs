using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Tests of stage 2 (the river of the parties): its waves, the prototype of Baylo, the talks and what the boat carries over.</summary>
public static partial class SelfTest
{
    private static void RegisterStage2(Action<string, Action> T)
    {
        T("StageTwoUsesOnlySwarmAndGunnerSpirits", StageTwoUsesOnlySwarmAndGunnerSpirits);
        T("StageTwoFormationsDifferFromStageOne", StageTwoFormationsDifferFromStageOne);
        T("StageTwoSpiritsHaveTheirOwnAttacks", StageTwoSpiritsHaveTheirOwnAttacks);
        T("BayloIsAFiveAttackPrototype", BayloIsAFiveAttackPrototype);
        T("EveryStageHasMoreSpiritsThanTheLast", EveryStageHasMoreSpiritsThanTheLast);
        T("TheDancerRoamsInEveryDirection", TheDancerRoamsInEveryDirection);
        T("DancingBulletsSwayOnTheBeat", DancingBulletsSwayOnTheBeat);
        T("TheDolphinDivesAndLeapsOutWhereTheShadowWas", TheDolphinDivesAndLeapsOutWhereTheShadowWas);
        T("LosingALifeRefillsTheBombs", LosingALifeRefillsTheBombs);
        T("PracticeNeverCostsALife", PracticeNeverCostsALife);
        T("TheJetSweepsTheFieldAndLeavesAPlaceToStand", TheJetSweepsTheFieldAndLeavesAPlaceToStand);
        T("TheStagesFollowEachOther", TheStagesFollowEachOther);
        T("StageTwoTalksFitTheDialogueBox", StageTwoTalksFitTheDialogueBox);
        T("TheCarryPassesToTheNextStage", TheCarryPassesToTheNextStage);
    }

    private static IEnumerable<EnemySpec> Spawns(StageDefinition stage) => stage.Waves.SelectMany(w => w.Spawns);

    private static void StageTwoUsesOnlySwarmAndGunnerSpirits()
    {
        True(Stages.Two.Waves.Count >= 5, "a full stage");
        True(Spawns(Stages.Two).All(s => s.Kind == SpiritKind.Swarm || s.Kind == SpiritKind.Gunner), "only types 1 and 2");
        True(Spawns(Stages.Two).Any(s => s.Kind == SpiritKind.Gunner), "some of them shoot");
    }

    private static void StageTwoFormationsDifferFromStageOne()
    {
        // The set of (kind, x, y, path) of the first spirits of each wave: no wave of stage 2 may repeat one of stage 1.
        string Key(Wave w) => string.Join(";", w.Spawns.Select(s => $"{s.Kind}|{s.Position.X:F0}|{s.Position.Y:F0}|{s.Time:F1}|{s.Path.Kind}"));
        var one = Stages.One.Waves.Select(Key).ToHashSet();
        foreach (var wave in Stages.Two.Waves) True(!one.Contains(Key(wave)), "a wave of stage 2 repeats one of stage 1");
        True(Stages.Two.Waves.Select(Key).Distinct().Count() == Stages.Two.Waves.Count, "no wave repeats inside stage 2");
    }

    private static void StageTwoSpiritsHaveTheirOwnAttacks()
    {
        var shots = Spawns(Stages.Two).Where(s => s.Kind == SpiritKind.Gunner).SelectMany(s => s.Patterns).Select(p => p.Pattern.Kind).ToHashSet();
        True(shots.Count >= 2, "the gunners of the parties fire more than the aimed bullet");
    }

    private static void BayloIsAFiveAttackPrototype()
    {
        var boss = Stages.Two.Boss;
        Eq(boss.Lives, 5);
        Eq(boss.Phases.Count(p => !p.Enraged), 3);
        Eq(boss.Phases.Count(p => p.Enraged), 2);
        True(!boss.PhaseFor(3).Enraged && boss.PhaseFor(2).Enraged, "three calm attacks, then two enraged");
        True(boss.PhaseFor(5).Name != Stages.One.Boss.Name, "his own name");
        for (int l = 1; l <= 5; l++) True(!string.IsNullOrEmpty(boss.AttackName(l)), "every attack is named");
    }

    private static void EveryStageHasMoreSpiritsThanTheLast()
    {
        Greater(Spawns(Stages.Two).Count(), Spawns(Stages.One).Count());
        for (int i = 1; i < Stages.All.Count; i++) Greater(Spawns(Stages.All[i]).Count(), Spawns(Stages.All[i - 1]).Count());
    }

    private static BossEntity BayloAt(int lives)
    {
        var boss = BossEntity.Create(BayloBoss.Definition);
        boss.Health.Lives = lives;
        PhaseManagerSystem.ChangePhase(boss, lives);
        boss.Transform.Position = V(300, 140);
        boss.MovementType = boss.NextMovementType;
        return boss;
    }

    private static void TheDancerRoamsInEveryDirection()
    {
        var boss = BayloAt(5);
        True(boss.MovementType == BossMovementType.Roam, "the man roams");
        float minX = 999, maxX = 0, minY = 999, maxY = 0;
        for (int i = 0; i < 60 * 40; i++)
        {
            MovementSystem.MoveBoss(boss, World.PlayArea, 1 / 60f);
            var p = boss.Transform.Position;
            minX = MathF.Min(minX, p.X); maxX = MathF.Max(maxX, p.X); minY = MathF.Min(minY, p.Y); maxY = MathF.Max(maxY, p.Y);
            GreaterEq(p.X, boss.Hitbox.Radius); LessEq(p.X, World.PlayArea.End.X - boss.Hitbox.Radius);
        }
        Greater(maxX - minX, 250, "across the field");
        Greater(maxY - minY, 100, "and up and down");
    }

    private static void DancingBulletsSwayOnTheBeat()
    {
        var pool = Pool(2);
        BulletSystem.Spawn(pool, V(300, 100), V(0, 100), Faction.Boss, BulletStyle.Spark, curve: 90, behavior: BulletBehavior.Dance);
        float minHeading = 999, maxHeading = -999;
        var player = new PlayerEntity();
        for (int i = 0; i < 60 * 3; i++)
        {
            BulletSystem.MoveBullets(pool, new Godot.Rect2(-1000, -1000, 3000, 3000), 1 / 60f, V(0, 0), player);
            float heading = Mathx.Degrees(MathF.Atan2(pool[0].Velocity.Y, pool[0].Velocity.X)) - 90;
            minHeading = MathF.Min(minHeading, heading); maxHeading = MathF.Max(maxHeading, heading);
        }
        True(minHeading < -15 && maxHeading > 15, $"it sways both ways ({minHeading}..{maxHeading})");
        True(minHeading > -60 && maxHeading < 60, "and never wanders off");
        // Neighbours of a volley sway against each other.
        var boss = BayloAt(5);
        boss.EmitterMain = Attacks.PartyTwoStep().Main;
        var volley = Pool(200);
        boss.EmitterMain.TimeSinceLastFire = 10;
        BulletSystem.UpdateBossWeapons(boss, player, volley, 0.01f);
        True(volley[0].CurveRate == -volley[1].CurveRate && volley[0].CurveRate != 0, "alternate bullets curve the other way");
    }

    private static void TheDolphinDivesAndLeapsOutWhereTheShadowWas()
    {
        var boss = BayloAt(2);
        True(boss.MovementType == BossMovementType.Dive, "the dolphin dives");
        var player = new PlayerEntity();
        var pool = BulletSystem.NewPool();
        bool wasUnder = false, rose = false; var shadow = V(-1, -1);
        for (int i = 0; i < 60 * 20 && !rose; i++)
        {
            // The boat moves a little at first, then stays: the shadow follows it and then stops.
            var boat = i < 60 * (MovementSystem.DiveSurfaceSeconds + 2) ? V(420, 520) : V(100, 300);
            MovementSystem.MoveBoss(boss, World.PlayArea, 1 / 60f, boat);
            if (boss.Dive.Submerged)
            {
                wasUnder = true;
                shadow = V(boss.Dive.ExitX, boss.Dive.ExitY);
                int before = pool.Count(b => b.IsActive);
                BulletSystem.UpdateBossWeapons(boss, player, pool, 0.2f);
                Eq(pool.Count(b => b.IsActive), before, "nothing is fired from under the water");
                True(!HitsBoss(boss), "nothing can hurt it under the water");
            }
            if (wasUnder && boss.Dive.State == DiveState.Rising) rose = true;
        }
        True(rose, "it comes out of the water");
        Near(shadow.X, 420, 0.01f, "the shadow was under the boat");
        Near(shadow.Y, 520, 0.01f, "the shadow was under the boat");
        Near(boss.Dive.ExitX, shadow.X, 0.01f, "the shadow stopped following");
        True(boss.Dive.Emerged, "with a splash");
        BulletSystem.UpdateBossWeapons(boss, player, pool, 0.01f);
        Greater(pool.Count(b => b.IsActive), 5, "a fan of spray");
        // The dive played backwards: it rises from below its place, and ends exactly there.
        Greater(boss.Transform.Position.Y, shadow.Y, "it starts below the water line");
        for (int i = 0; i < 60; i++) MovementSystem.MoveBoss(boss, World.PlayArea, 1 / 60f, V(100, 300));
        Near(boss.Transform.Position.X, shadow.X, 0.5f);
    }

    private static void TheJetSweepsTheFieldAndLeavesAPlaceToStand()
    {
        var boss = BayloAt(1);
        True(boss.MovementType == BossMovementType.Sweep, "the second attack sweeps");
        float firstEnd = -1, secondEnd = 999, restSeconds = 0, restX = -1, firstStart = 999, secondStart = -1;
        bool warned = false;
        int passes = 0; var last = SweepState.ToLeft;
        for (int i = 0; i < 60 * 40; i++)
        {
            MovementSystem.MoveBoss(boss, World.PlayArea, 1 / 60f, V(300, 600));
            warned |= boss.Sweep.BeamWarn;
            float x = boss.Transform.Position.X;
            if (boss.Sweep.State == SweepState.Pass && boss.Sweep.BeamOn) { firstEnd = Math.Max(firstEnd, x); firstStart = Math.Min(firstStart, x); }
            if (boss.Sweep.State == SweepState.Pass2 && boss.Sweep.BeamOn) { secondEnd = Math.Min(secondEnd, x); secondStart = Math.Max(secondStart, x); }
            if (boss.Sweep.State == SweepState.Rest) { restSeconds += 1 / 60f; restX = x; }
            if (boss.Sweep.State != last && (boss.Sweep.State == SweepState.Pass || boss.Sweep.State == SweepState.Pass2)) passes++;
            last = boss.Sweep.State;
            if (boss.Sweep.State != SweepState.Pass && boss.Sweep.State != SweepState.Pass2) True(!boss.Sweep.BeamOn, "the jet is only on while sweeping");
        }
        True(warned, "a warning before each pass");
        Greater(passes, 2, "it goes on and on");
        Greater(SweepBrain.BeamHalfWidth, 60, "a great jet");
        float edge = World.PlayArea.End.X;
        // The first pass goes from the left to the right, stopping short of the right edge.
        Less(firstStart, 120, "it begins at the left");
        Greater(edge - (firstEnd + SweepBrain.BeamHalfWidth), 60, "the first pass leaves room to stand at the right");
        // The second goes back, from the right to the left, stopping short of the left edge.
        Greater(secondStart, edge - 120, "the second pass begins at the right");
        Greater(secondEnd - SweepBrain.BeamHalfWidth, 40, "the second pass leaves room to stand at the left");
        Less(secondEnd, secondStart - 200, "and it does cross the field");
        Near(restX, 300, 1, "it rests in the middle");
        Greater(restSeconds, 3, "a few seconds to hit it");
        // A boat under the jet is hurt, and one beside it is not.
        boss.Sweep.BeamOn = true; boss.Transform.Position = V(300, 150);
        var hurt = new PlayerEntity(); hurt.Transform.Position = V(340, 500);
        CombatSystem.ProcessCollisions(BulletSystem.NewPool(), hurt, boss);
        Less(hurt.Health.CurrentHealth, hurt.Health.MaxHealth, "the jet hurts");
        var safe = new PlayerEntity(); safe.Transform.Position = V(420, 500);
        CombatSystem.ProcessCollisions(BulletSystem.NewPool(), safe, boss);
        Eq(safe.Health.CurrentHealth, safe.Health.MaxHealth);
    }

    private static bool HitsBoss(BossEntity boss)
    {
        var player = new PlayerEntity();
        var pool = BulletSystem.NewPool();
        BulletSystem.Spawn(pool, boss.Transform.Position, V(0, 0), Faction.Player, BulletStyle.LightBolt);
        int hits = CombatSystem.ProcessCollisions(pool, player, boss, true);
        return hits > 0;
    }

    private static void LosingALifeRefillsTheBombs()
    {
        var world = new World(withCards: false, seed: 3);
        world.Player.Bomb.BombsLeft = 0;
        world.Player.Health.CurrentHealth = 0;
        world.Step(1 / 60f, new PlayerInputComponent());
        Eq(world.Player.Health.Lives, World.MaxLives - 1);
        Eq(world.Player.Bomb.BombsLeft, World.MaxBombs);
    }

    private static void PracticeNeverCostsALife()
    {
        foreach (var world in new[] { new World(1, withCards: false, seed: 4), new World(withCards: false, seed: 5, stagePractice: true) })
        {
            world.Player.Bomb.BombsLeft = 0;
            for (int i = 0; i < 5; i++)
            {
                world.Player.Health.CurrentHealth = 0;
                world.Step(1 / 60f, new PlayerInputComponent());
            }
            Eq(world.Player.Health.Lives, World.MaxLives);
            Eq(world.Player.Bomb.BombsLeft, World.MaxBombs);
            True(world.Result == null, "practice goes on");
        }
    }

    private static void TheStagesFollowEachOther()
    {
        True(Stages.After(Stages.One) == Stages.Two, "2 follows 1");
        True(Stages.After(Stages.Two) == Stages.Three, "3 follows 2");
        True(Stages.After(Stages.Three) == Stages.Four, "4 follows 3");
        True(Stages.After(Stages.Four) == Stages.Five, "5 follows 4");
        True(Stages.After(Stages.Five) == Stages.Six, "6 follows 5");
        True(Stages.After(Stages.Six) == null, "nothing after the last");
        True(Stages.Two.Intro != null && Stages.Two.Ending != null && Stages.Two.BossEncounter != null, "stage 2 has its talks");
    }

    private static void StageTwoTalksFitTheDialogueBox()
    {
        var lists = new[] { Dialogues.StageTwoIntro, Dialogues.StageTwoMeeting, Dialogues.StageTwoEnding };
        foreach (var lang in new[] { Language.English, Language.PortugueseBrazil })
        {
            Loc.Choose(lang);
            foreach (var list in lists)
                foreach (var line in list)
                {
                    True(!string.IsNullOrWhiteSpace(line.Text), "has text");
                    True(GameHud.Wrap(line.Text, 28, 99).Count <= 5, $"[{lang}] fits: {line.Text}");
                }
        }
        Loc.Choose(Language.PortugueseBrazil);
    }

    private static void TheCarryPassesToTheNextStage()
    {
        var first = new World(withCards: false, seed: 1);
        first.Player.Health.Lives = 2;
        first.Player.Bomb.BombsLeft = 1;
        var carry = RunCarry.From(first);
        var second = new World(withCards: false, seed: 2, stage: Stages.Two, carry: carry);
        Eq(second.Player.Health.Lives, 2);
        Eq(second.Player.Bomb.BombsLeft, 1);
        True(second.Definition == Stages.Two, "the second stage");
        // Restarting the stage keeps the carry.
        second.Player.Health.Lives = 1;
        second.Reset();
        Eq(second.Player.Health.Lives, 2);
    }
}
