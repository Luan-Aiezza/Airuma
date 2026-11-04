using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of stage 5, the waterfall: the pairs of red spirits, Kuychu and his attacks (the heads in turns, the rainbow, the level walls of water), and the talks.</summary>
public static partial class SelfTest
{
    private static void RegisterStage5(Action<string, Action> T)
    {
        T("StageFiveBringsTheRedSpiritsInPairsAndTheLastPairWithTheLittleOnes", StageFiveBringsTheRedSpiritsInPairsAndTheLastPairWithTheLittleOnes);
        T("KuychuHasEightAttacksFiveCalmAndThreeEnraged", KuychuHasEightAttacksFiveCalmAndThreeEnraged);
        T("TheTwoHeadsFireInTurnsAndTheRainbowHasSevenColours", TheTwoHeadsFireInTurnsAndTheRainbowHasSevenColours);
        T("ALevelWallOfWaterCrossesTheFieldAndLeavesRoomAboveAndBelow", ALevelWallOfWaterCrossesTheFieldAndLeavesRoomAboveAndBelow);
        T("TheTwoFuseIntoOneForTheSecondHalfOfTheFight", TheTwoFuseIntoOneForTheSecondHalfOfTheFight);
        T("OneOfTheTwoCanAlwaysBeHitAndTheyShareOneLife", OneOfTheTwoCanAlwaysBeHitAndTheyShareOneLife);
        T("StageFiveCanBePlayedThroughItsWavesToTheBoss", StageFiveCanBePlayedThroughItsWavesToTheBoss);
        T("StageFiveTalksFitTheDialogueBox", StageFiveTalksFitTheDialogueBox);
    }

    private static void StageFiveBringsTheRedSpiritsInPairsAndTheLastPairWithTheLittleOnes()
    {
        var waves = Stages.Five.Waves;
        Greater(Spawns(Stages.Five).Count(), Spawns(Stages.Four).Count(), "harder than stage 4");
        var heavyWaves = waves.Where(w => w.Spawns.Any(s => s.Kind == SpiritKind.Heavy)).ToList();
        var pairs = heavyWaves.Where(w => w.Spawns.Count(s => s.Kind == SpiritKind.Heavy) == 2).ToList();
        GreaterEq(pairs.Count, 3, "red spirits in pairs");
        True(heavyWaves.All(w => w.Spawns.Count(s => s.Kind == SpiritKind.Heavy) <= 2), "never more than two together");
        foreach (var wave in pairs)
        {
            var heavies = wave.Spawns.Where(s => s.Kind == SpiritKind.Heavy).ToList();
            Eq(heavies[0].Time, heavies[1].Time, "the two arrive together");
            True(heavies[0].Position.X < 300 && heavies[1].Position.X > 300, "one on each side");
            True(heavies[0].Patterns.Select(p => p.Pattern.Spin).SequenceEqual(heavies[1].Patterns.Select(p => -p.Pattern.Spin)) || heavies[0].Patterns.Count > 0, "with patterns of their own");
        }
        // the first pairs come with the gunners; the last, with the little spirits (type 1)
        True(pairs[0].Spawns.Any(s => s.Kind == SpiritKind.Gunner), "the first pair comes with the gunners");
        var last = pairs[pairs.Count - 1];
        GreaterEq(last.Spawns.Count(s => s.Kind == SpiritKind.Swarm), 40, "the last pair comes with a crowd of type 1");
        var all = waves.ToList();
        True(all.IndexOf(last) > all.IndexOf(pairs[0]) && last == heavyWaves[heavyWaves.Count - 1], "and it is the last");
        True(waves.Take(2).All(w => w.Spawns.All(s => s.Kind != SpiritKind.Heavy)), "none at the very start");
    }

    private static void KuychuHasEightAttacksFiveCalmAndThreeEnraged()
    {
        var boss = Stages.Five.Boss;
        Eq(boss.Lives, 8, "eight lives");
        Eq(boss.Phases.Count(p => !p.Enraged), 5);
        Eq(boss.Phases.Count(p => p.Enraged), 3);
        Eq(new World(withCards: false, stage: Stages.Five).StartingBossLives, 8);
        True(!Stages.All.Where(s => s != Stages.Five).Select(s => s.Boss.Name).Contains(boss.Name), "his own name");
        True(boss.Phases.Any(p => p.Quake.IsActive) && boss.Phases.Any(p => p.Slashes.IsActive), "the tremor and the water");
        True(boss.Phases.Any(p => p.Main.Rainbow), "the rainbow");
        True(boss.Phases.Any(p => p.Sub.FromTwin) && Stages.Five.Boss.HasTwin, "two persons, who fire in turns");
        // no two attacks work alike
        var kinds = boss.Phases.Select(p => (p.Slashes.IsActive, p.Slashes.Angle, p.Quake.IsActive, p.Roar.IsActive, p.Main.Rainbow, p.Main.Ceiling, p.Main.SpinRate != 0, p.Main.CurveRate != 0, p.Sub.IsActive, p.Main.BulletStyle, p.Movement)).ToList();
        Eq(kinds.Distinct().Count(), kinds.Count, "no two attacks work alike");
        // an enraged one that roams rests as it does in the other stages
        // in the second half of the fight (the enraged attacks) the two are one body
        True(boss.Phases.Where(p => p.Enraged).All(p => p.Fused) && boss.Phases.Where(p => !p.Enraged).All(p => !p.Fused), "the two fuse into one when she is enraged");
        True(boss.Phases.Where(p => p.Fused).All(p => !p.Main.FromTwin && !p.Sub.FromTwin), "and the one body fires from itself");
        True(Stages.Five.Boss.Style.Fused != null && Stages.Five.Boss.Style.Fused.MakeFrame != null, "with a look of its own");
    }

    private static void TheTwoHeadsFireInTurnsAndTheRainbowHasSevenColours()
    {
        var boss = BossEntity.Create(Stages.Five.Boss);
        var twin = BossEntity.Create(Stages.Five.Boss);
        PhaseManagerSystem.ChangePhase(boss, Stages.Five.Boss.Lives);       // the two heads
        boss.MovementType = BossMovementType.Stationary;
        boss.Transform.Position = V(170, Layout.BossY);
        twin.Transform.Position = V(430, Layout.BossY);
        var player = PlayerEntity.Create(); player.Transform.Position = V(300, 650);
        var pool = BulletSystem.NewPool();
        var firedLeft = new List<float>(); var firedRight = new List<float>();
        for (int i = 0; i < 60 * 8; i++)
        {
            BulletSystem.UpdateBossWeapons(boss, player, pool, 1f / 60, Difficulty.Normal, null, twin);
            for (int k = 0; k < pool.Length; k++)
                if (pool[k].IsActive && pool[k].Fresh)
                {
                    (pool[k].Position.X < 300 ? firedLeft : firedRight).Add(i / 60f);
                    pool[k].Fresh = false;
                }
        }
        Greater(firedLeft.Count, 0); Greater(firedRight.Count, 0);
        // every volley of one head is followed, a good while later, by one of the other: never together
        float nearest = firedLeft.Min(l => firedRight.Min(r => MathF.Abs(l - r)));
        GreaterEq(nearest, 0.5f, "the heads take turns");
        // the rainbow: a volley has all seven colours
        var rainbow = Attacks.SevenArcs();
        var pool2 = BulletSystem.NewPool();
        var boss2 = BossEntity.Create(Stages.Five.Boss);
        PhaseManagerSystem.ChangePhase(boss2, Stages.Five.Boss.Lives - 1);
        boss2.MovementType = BossMovementType.Stationary;
        boss2.EmitterMain.TimeSinceLastFire = 10; boss2.EmitterSub.IsActive = false;
        BulletSystem.UpdateBossWeapons(boss2, player, pool2, 0.001f);
        Eq(pool2.Where(b => b.IsActive).Select(b => b.Style).Distinct().Count(), 7, "seven colours");
    }

    private static void ALevelWallOfWaterCrossesTheFieldAndLeavesRoomAboveAndBelow()
    {
        var boss = BossEntity.Create(Stages.Five.Boss);
        PhaseManagerSystem.ChangePhase(boss, Stages.Five.Boss.Lives - 3);          // the tide
        boss.MovementType = BossMovementType.Stationary;
        var player = PlayerEntity.Create(); player.Transform.Position = V(300, 520);
        var slashes = new List<SlashData>(); var events = new List<WorldEvent>(); var rng = new SeededGenerator(3);
        boss.Specials.SlashClock = boss.Specials.Slashes.Interval;
        SpecialsSystem.Update(boss, player, slashes, 0.01f, events, rng);
        Eq(slashes.Count, 2, "two walls");
        foreach (var s in slashes)
        {
            Less(MathF.Abs(s.Direction.Y), 0.2f, "level, or nearly");
            True(s.Top.X < 0 && s.Bottom.X > 600, "it crosses the whole field");
        }
        // at the moment of the strike, a boat between the two walls, and one above them, are untouched
        for (int i = 0; i < 400 && slashes.All(s => !s.Striking); i++) SpecialsSystem.Update(boss, player, slashes, 0.01f, events, rng);
        var ys = slashes.Select(s => s.Top.Y + s.Direction.Y * ((300 - s.Top.X) / s.Direction.X)).OrderBy(y => y).ToList();
        float between = (ys[0] + ys[1]) / 2;
        var atBetween = PlayerEntity.Create(); atBetween.Transform.Position = V(300, between);
        True(ys[1] - ys[0] - slashes[0].Width > 2 * atBetween.Hitbox.Radius + 60, "room to go between them");
        True(!SpecialsSystem.HitBySlash(slashes, atBetween), "between is safe");
        var onWall = PlayerEntity.Create(); onWall.Transform.Position = V(300, ys[0]);
        True(SpecialsSystem.HitBySlash(slashes, onWall), "a wall hurts");
    }

    private static void OneOfTheTwoCanAlwaysBeHitAndTheyShareOneLife()
    {
        foreach (var level in new[] { Difficulty.Easy, Difficulty.Hard })
            for (int phase = 1; phase <= 8; phase++)
            {
                var world = new World(phase, withCards: false, difficulty: level, stage: Stages.Five);
                True(world.Twin != null, "there are two");
                int bothGone = 0;
                for (int i = 0; i < 60 * 30; i++)
                {
                    world.Player.Health.InvincibilityTimer = 10;
                    world.Step(1f / 60, Idle());
                    bool Reachable(BossEntity b) => b.Transform.Position.Y > 0 && b.Transform.Position.Y < 700 && b.Transform.Position.X > -20 && b.Transform.Position.X < 620 && !b.Dive.Submerged;
                    if (i > 60 * 6 && !Reachable(world.Boss) && !(!world.Twin.Merged && Reachable(world.Twin))) bothGone++;
                }
                Eq(bothGone, 0, $"attack {phase} on {level}: one of them is always where the boat can hit it");
            }
        // either one takes the shots, and the life is one
        var w = new World(1, withCards: false, stage: Stages.Five);
        for (int i = 0; i < 60 * 6; i++) { w.Player.Health.InvincibilityTimer = 10; w.Step(1f / 60, Idle()); }
        int before = w.Boss.Health.CurrentHealth;
        BulletSystem.Spawn(w.Bullets, w.Twin.Transform.Position, V(0, 0), Faction.Player, BulletStyle.LightBolt);
        w.Step(1f / 60, Idle());
        Less(w.Boss.Health.CurrentHealth, before, "a shot on the twin hurts the one boss");
        Eq(w.Twin.Health.CurrentHealth, w.Boss.Health.CurrentHealth, "and they share it");
    }

    private static void TheTwoFuseIntoOneForTheSecondHalfOfTheFight()
    {
        // the calm attacks: two bodies, each hurt by the shots
        var calm = new World(2, withCards: false, stage: Stages.Five);
        for (int i = 0; i < 60 * 8; i++) { calm.Player.Health.InvincibilityTimer = 10; calm.Step(1f / 60, Idle()); }
        True(!calm.Twin.Merged, "two bodies at first");
        Greater(calm.Twin.Transform.Position.DistanceTo(calm.Boss.Transform.Position), 80f, "far apart");
        // the enraged ones: the second glides to the first and they are one
        var fused = new World(7, withCards: false, stage: Stages.Five);
        float apart = float.MaxValue;
        for (int i = 0; i < 60 * 8; i++) { fused.Player.Health.InvincibilityTimer = 10; fused.Step(1f / 60, Idle()); if (fused.Twin.Merged) apart = MathF.Min(apart, fused.Twin.Transform.Position.DistanceTo(fused.Boss.Transform.Position)); }
        True(fused.Twin.Merged, "one body");
        Less(apart, 1f, "in the same place");
        // a shot where the twin was is nothing now, and the bullets come from the one body
        var pool = BulletSystem.NewPool();
        var player = PlayerEntity.Create(); player.Transform.Position = V(300, 650);
        fused.Boss.EmitterMain.TimeSinceLastFire = 10; fused.Boss.EmitterSub.IsActive = false;
        BulletSystem.UpdateBossWeapons(fused.Boss, player, pool, 0.001f, Difficulty.Normal, null, null);
        True(pool.Where(b => b.IsActive).All(b => MathF.Abs(b.Position.X - fused.Boss.Transform.Position.X) < 60), "from the one body");
    }

    private static void StageFiveCanBePlayedThroughItsWavesToTheBoss()
    {
        var world = new World(withCards: false, stage: Stages.Five);
        var waveStarts = new List<int>();
        float t = 0;
        while (t < 1200 && world.Part == StagePart.Waves)
        {
            world.Player.Health.InvincibilityTimer = 10;
            foreach (var e in world.Enemies) e.Health = 0;
            world.Step(1f / 60, Idle());
            foreach (var ev in world.TakeEvents()) if (ev.Kind == WorldEventKind.WaveStarted) waveStarts.Add(ev.Int);
            t += 1f / 60;
        }
        True(waveStarts.SequenceEqual(Enumerable.Range(0, Stages.Five.Waves.Count)), "every wave, in order");
        Eq(world.Part, StagePart.Boss);
        Eq(world.Boss.Health.Lives, 8);
        foreach (var level in new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard })
            for (int phase = 1; phase <= 8; phase++)
            {
                var practice = new World(phase, withCards: false, difficulty: level, stage: Stages.Five);
                for (int i = 0; i < 60 * 25; i++)
                {
                    practice.Player.Health.InvincibilityTimer = 10;
                    practice.Step(1f / 60, Idle());
                }
                True(practice.Result == null && practice.Boss.Health.Lives >= 1, $"attack {phase} on {level} runs");
            }
    }

    private static void StageFiveTalksFitTheDialogueBox()
    {
        var lists = new[] { Dialogues.StageFiveIntro, Dialogues.StageFiveMeeting, Dialogues.StageFiveEnding };
        foreach (var lang in new[] { Language.English, Language.PortugueseBrazil })
        {
            Loc.Choose(lang);
            foreach (var list in lists)
                foreach (var line in list)
                {
                    True(!string.IsNullOrWhiteSpace(line.Text) && line.Text != line.Key, "has text");
                    True(GameHud.Wrap(line.Text, 36, 3).Count <= 3, $"[{lang}] fits: {line.Text}");
                }
        }
        Loc.Choose(Language.PortugueseBrazil);
        Eq(Dialogues.StageFiveIntro.Count, 5);
        Eq(Dialogues.StageFiveMeeting.Count, 7);
        True(Dialogues.StageFiveMeeting.Any(l => l.Speaker == DialogueSpeaker.LeftSerpent) && Dialogues.StageFiveMeeting.Any(l => l.Speaker == DialogueSpeaker.RightSerpent), "the serpents speak");
        True(Dialogues.StageFiveIntro.Any(l => l.Speaker == DialogueSpeaker.Ukhumo), "Ukhumo says goodbye");
        True(Dialogues.StageFiveEnding.All(l => l.Speaker != DialogueSpeaker.Ukhumo), "and is gone");
    }
}
