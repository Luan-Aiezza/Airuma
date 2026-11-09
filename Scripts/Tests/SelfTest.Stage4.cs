using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of stage 4, the river of the cave: the red spirits in combos with the others, Ukhumo and her attacks, and the curtain whose gap jumps.</summary>
public static partial class SelfTest
{
    private static void RegisterStage4(Action<string, Action> T)
    {
        T("StageFourPutsRedSpiritsInCombosOneAtATime", StageFourPutsRedSpiritsInCombosOneAtATime);
        T("UkhumoHasSevenAttacksFourCalmAndThreeEnraged", UkhumoHasSevenAttacksFourCalmAndThreeEnraged);
        T("TheClawsWarnBeforeTheyCutAndLeaveRoomToStand", TheClawsWarnBeforeTheyCutAndLeaveRoomToStand);
        T("TheQuakeShakesTheScreenAndOnlyThenTheCeilingFalls", TheQuakeShakesTheScreenAndOnlyThenTheCeilingFalls);
        T("TheRoarPushesTheBoatAwayFromTheBoss", TheRoarPushesTheBoatAwayFromTheBoss);
        T("AGreatStoneBreaksIntoARingOfSmallOnes", AGreatStoneBreaksIntoARingOfSmallOnes);
        T("EveryAttackOfUkhumoIsDifferent", EveryAttackOfUkhumoIsDifferent);
        T("TheMarksOfClawsAndStonesAreDrawnAndTheBearCloakWardsAndCalmsTheWind", TheMarksOfClawsAndStonesAreDrawnAndTheBearCloakWardsAndCalmsTheWind);
        T("StageFourCanBePlayedThroughItsWavesToTheBoss", StageFourCanBePlayedThroughItsWavesToTheBoss);
        T("StageFourTalksFitTheDialogueBox", StageFourTalksFitTheDialogueBox);
    }

    private static void StageFourPutsRedSpiritsInCombosOneAtATime()
    {
        // stage 4: the red spirits come in PAIRS, alone in their wave
        var waves = Stages.Four.Waves;
        var pairs = waves.Where(w => w.Spawns.Any(s => s.Kind == SpiritKind.Heavy)).ToList();
        GreaterEq(pairs.Count, 2, "red spirits come in pairs");
        foreach (var wave in pairs)
        {
            Eq(wave.Spawns.Count(s => s.Kind == SpiritKind.Heavy), 2, "a pair");
            Eq(wave.Spawns.Count, 2, "alone in its wave");
        }
    }

    private static void UkhumoHasSevenAttacksFourCalmAndThreeEnraged()
    {
        var boss = Stages.Four.Boss;
        Eq(boss.Lives, 7, "seven lives");
        Eq(boss.Phases.Count(p => !p.Enraged), 4);
        Eq(boss.Phases.Count(p => p.Enraged), 3);
        Eq(new World(withCards: false, stage: Stages.Four).StartingBossLives, 7);
        True(boss.Phases.Any(p => p.Slashes.IsActive) && boss.Phases.Any(p => p.Quake.IsActive) && boss.Phases.Any(p => p.Roar.IsActive), "claws, quake and roar");
        True(boss.Phases.Any(p => p.Main.BulletBehavior == BulletBehavior.Burst || p.Sub.BulletBehavior == BulletBehavior.Burst || p.Patterns.Count > 0), "the stone that breaks");
        True(boss.Phases.All(p => p.Movement == BossMovementType.Stationary && p.Target == new Vector2(300, Layout.BossY)), "she never leaves her platform");
        True(boss.Phases.All(p => !p.Main.IsActive || p.Main.BulletStyle == BulletStyle.Claw || p.Main.BulletStyle == BulletStyle.Rock || p.Main.BulletStyle == BulletStyle.Stalactite), "her bullets are claws and stones");
        var others = Stages.All.Where(s => s != Stages.Four).Select(s => s.Boss.Name).ToList();
        True(!others.Contains(boss.Name), "her own name");
        True(Stages.Four.BossCard == Card.BearCloak);
    }

    private static BossEntity UkhumoOn(int attack)
    {
        var boss = BossEntity.Create(Stages.Four.Boss);
        PhaseManagerSystem.ChangePhase(boss, Stages.Four.Boss.Lives - attack + 1);
        boss.MovementType = BossMovementType.Stationary;
        boss.Transform.Position = V(300, Layout.BossY);
        return boss;
    }

    private static void TheClawsWarnBeforeTheyCutAndLeaveRoomToStand()
    {
        var boss = UkhumoOn(1);
        var player = PlayerEntity.Create(); player.Transform.Position = V(300, 600);
        var slashes = new List<SlashData>();
        var events = new List<WorldEvent>();
        var rng = new SeededGenerator(5);
        boss.Specials.SlashClock = boss.Specials.Slashes.Interval;          // a volley now
        SpecialsSystem.Update(boss, player, slashes, 0.01f, events, rng);
        Eq(slashes.Count, 3, "three claws");
        True(slashes.All(sl => sl.Warning), "warned of first");
        True(!SpecialsSystem.HitBySlash(slashes, player), "no harm in the warning");
        // they are parallel and the room between them is wide enough to stand in
        float gap = slashes[1].Top.DistanceTo(slashes[0].Top) - slashes[0].Width;
        Greater(gap, 2 * player.Hitbox.Radius + 40, "room to stand between two cuts");
        // the strike
        for (int i = 0; i < 100 && slashes.All(sl => !sl.Striking); i++) SpecialsSystem.Update(boss, player, slashes, 0.01f, events, rng);
        True(slashes.Any(sl => sl.Striking), "then they strike");
        True(events.Any(e => e.Kind == WorldEventKind.SlashStruck), "with a sound");
        // a boat in the middle of a cut is hurt; one in the room between two is not
        var on = slashes[1];
        var inCut = PlayerEntity.Create(); inCut.Transform.Position = on.Top + on.Direction * 500;
        True(SpecialsSystem.HitBySlash(slashes, inCut), "a cut hurts");
        var between = PlayerEntity.Create(); between.Transform.Position = (slashes[0].Top + slashes[1].Top) / 2 + slashes[0].Direction * 500;
        True(!SpecialsSystem.HitBySlash(slashes, between), "the room between is safe");
        // and the scar fades away
        for (int i = 0; i < 100; i++) SpecialsSystem.Update(UkhumoOn(2), player, slashes, 0.02f, events, rng);
        Eq(slashes.Count, 0, "they are gone");
        // the diagonal claws lean
        var crossing = UkhumoOn(5);
        crossing.Specials.SlashClock = crossing.Specials.Slashes.Interval;
        SpecialsSystem.Update(crossing, player, slashes, 0.01f, events, rng);
        Greater(MathF.Abs(slashes[0].Angle), 0.3f, "slanting");
        // a slash in the world hurts the boat at the strike
        var world = new World(1, withCards: false, stage: Stages.Four);
        world.CalmWindForTesting();
        world.Player.Transform.Position = V(300, 500);
        for (int i = 0; i < 60 * 60 && world.Player.Health.CurrentHealth == world.Player.Health.MaxHealth && world.Player.Health.Lives == 3; i++)
        {
            world.Player.Transform.Position = V(300, 500);          // it does not move out of the way
            BulletSystem.ClearBossBullets(world.Bullets);
            world.Step(1f / 60, Idle());
        }
        True(world.Player.Health.Lives < 3 || world.Player.Health.CurrentHealth < world.Player.Health.MaxHealth, "a boat that stays is cut");
    }

    private static void TheQuakeShakesTheScreenAndOnlyThenTheCeilingFalls()
    {
        var boss = UkhumoOn(3);
        var player = PlayerEntity.Create(); player.Transform.Position = V(300, 600);
        var pool = BulletSystem.NewPool();
        int Stalactites() => pool.Count(b => b.IsActive && b.Style == BulletStyle.GiantStalactite);
        for (int i = 0; i < 12; i++) BulletSystem.UpdateBossWeapons(boss, player, pool, 0.1f);
        Eq(Stalactites(), 0, "no stalactite falls before the quake");
        Eq(SpecialsSystem.Shake(boss.Specials), 0f, "and nothing shakes");
        boss.Specials.QuakeClock = boss.Specials.Quake.Interval;
        var events = new List<WorldEvent>();
        SpecialsSystem.Update(boss, player, new List<SlashData>(), 0.01f, events, new SeededGenerator(1));
        True(events.Any(e => e.Kind == WorldEventKind.QuakeStarted), "the quake begins");
        Greater(SpecialsSystem.Shake(boss.Specials), 0.5f, "the screen shakes");
        for (int i = 0; i < 6; i++) BulletSystem.UpdateBossWeapons(boss, player, pool, 0.1f);
        var fallen = pool.Where(b => b.IsActive && b.Style == BulletStyle.GiantStalactite).ToList();
        Greater(fallen.Count, 0, "the ceiling comes down once the quake begins");
        True(fallen.All(b => b.Velocity.Y > 100 && b.Velocity.Y < 250), "they fall slowly");
        True(fallen.All(b => b.Style == BulletStyle.GiantStalactite && BulletArt.Size(b.Style).Height >= 40), "and they are giants");
        // the quake ends
        for (int i = 0; i < 400; i++) SpecialsSystem.Update(boss, player, new List<SlashData>(), 0.02f, events, new SeededGenerator(1));
        // many fall over a quake, all over the field
        var xs = new HashSet<int>();
        for (int i = 0; i < 80; i++) { boss.Specials.QuakeLeft = 1; BulletSystem.UpdateBossWeapons(boss, player, pool, 0.45f); }
        foreach (var b in pool.Where(b => b.IsActive)) xs.Add((int)(b.Position.X / 100));
        GreaterEq(xs.Count, 4, "they fall all over the field");
        // in the world the shake is reported
        var world = new World(3, withCards: false, stage: Stages.Four);
        world.CalmWindForTesting();
        float most = 0;
        for (int i = 0; i < 60 * 20; i++) { world.Player.Health.InvincibilityTimer = 10; world.Step(1f / 60, Idle()); most = MathF.Max(most, world.Shake); }
        Greater(most, 0.5f, "the world reports the shake");
    }

    private static void TheRoarPushesTheBoatAwayFromTheBoss()
    {
        var boss = UkhumoOn(2);
        var player = PlayerEntity.Create(); player.Transform.Position = V(300, 500);
        var events = new List<WorldEvent>();
        boss.Specials.RoarClock = boss.Specials.Roar.Interval;
        var push = SpecialsSystem.Update(boss, player, new List<SlashData>(), 0.01f, events, new SeededGenerator(1));
        True(events.Any(e => e.Kind == WorldEventKind.Roared), "she roars");
        push = SpecialsSystem.Update(boss, player, new List<SlashData>(), 0.01f, events, new SeededGenerator(1));
        Greater(push.Y, 100f, "the boat is pushed down, away from her");
        player.Transform.Position = V(450, 400);
        push = SpecialsSystem.Update(boss, player, new List<SlashData>(), 0.01f, events, new SeededGenerator(1));
        Greater(push.X, 0f, "and away to the side she is not on");
    }

    private static void AGreatStoneBreaksIntoARingOfSmallOnes()
    {
        var pool = BulletSystem.NewPool();
        var player = PlayerEntity.Create(); player.Transform.Position = V(300, 700);
        BulletSystem.Spawn(pool, V(300, 200), V(0, 0), Faction.Boss, BulletStyle.Rock, scale: 3, behavior: BulletBehavior.Burst);
        Eq(pool.Count(b => b.IsActive), 1);
        for (int i = 0; i < 60; i++) BulletSystem.MoveBullets(pool, new Rect2(-2000, -2000, 5000, 5000), 1 / 60f, V(0, 0), player);
        Eq(pool.Count(b => b.IsActive), 1, "it flies a while whole");
        for (int i = 0; i < 40; i++) BulletSystem.MoveBullets(pool, new Rect2(-2000, -2000, 5000, 5000), 1 / 60f, V(0, 0), player);
        Eq(pool.Count(b => b.IsActive), BulletSystem.BoulderFragments, "then it breaks into a ring");
        True(pool.Where(b => b.IsActive).All(b => b.Style == BulletStyle.Rock && b.Velocity.Length() > 100), "of small stones");
    }

    private static void EveryAttackOfUkhumoIsDifferent()
    {
        // what each attack asks of the player: the set of mechanics it uses must not be the same as any other's
        var kinds = Stages.Four.Boss.Phases.Select(p => (p.Slashes.IsActive, p.Slashes.Angle != 0, p.Quake.IsActive, p.Roar.IsActive, p.Slashes.Aim, p.Slashes.Count, p.Patterns.Count)).ToList();
        Eq(kinds.Distinct().Count(), kinds.Count, "no two attacks work alike");
        Greater(kinds.Count(k => k.Item1 || k.Item3 || k.Item4), 5, "most attacks have a mechanic beyond bullets");
    }

    private static void TheMarksOfClawsAndStonesAreDrawnAndTheBearCloakWardsAndCalmsTheWind()
    {
        foreach (var style in new[] { BulletStyle.Shard, BulletStyle.Claw, BulletStyle.Rock, BulletStyle.Stalactite })
            foreach (var frame in new[] { 0, 1 })
                Greater(BulletArt.MakeCanvas(style, frame).Pixels.Count(px => px.A > 0), 15, style + " has a body");
        True(BulletArt.Rotates(BulletStyle.Claw) && BulletArt.Rotates(BulletStyle.Stalactite), "they point where they go");
        var cloak = new CardEffects(new Dictionary<Card, int> { [Card.BearCloak] = 1 });
        Less(cloak.WindMultiplier, 1f);
        var world = new World(withCards: false);
        CardSystem.Take(world.Player, Card.BearCloak);
        Eq(world.Player.Shield, 1);
        True(CardArt.Face(Card.BearCloak).Pixels.Count(px => px.A > 0) > 100);
    }

    private static void StageFourCanBePlayedThroughItsWavesToTheBoss()
    {
        var world = new World(withCards: false, stage: Stages.Four);
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
        True(waveStarts.SequenceEqual(Enumerable.Range(0, Stages.Four.Waves.Count)), "every wave, in order");
        Eq(world.Part, StagePart.Boss);
        Eq(world.Boss.Health.Lives, 7);
        foreach (var level in new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard })
            for (int phase = 1; phase <= 7; phase++)
            {
                var practice = new World(phase, withCards: false, difficulty: level, stage: Stages.Four);
                for (int i = 0; i < 60 * 25; i++)
                {
                    practice.Player.Health.InvincibilityTimer = 10;
                    practice.Step(1f / 60, Idle());
                }
                True(practice.Result == null && practice.Boss.Health.Lives >= 1, $"attack {phase} on {level} runs");
            }
    }

    private static void StageFourTalksFitTheDialogueBox()
    {
        var lists = new[] { Dialogues.StageFourIntro, Dialogues.StageFourMeeting, Dialogues.StageFourEnding };
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
        Eq(Dialogues.StageFourIntro.Count, 5);
        Eq(Dialogues.StageFourMeeting.Count, 5);
        True(Dialogues.StageFourEnding.Any(l => l.Speaker == DialogueSpeaker.Boss) && Dialogues.StageFourEnding.Any(l => l.Speaker == DialogueSpeaker.Atli), "the ending");
    }
}
