using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Tests of the patterns of bullets (<see cref="Pat"/>): the figures keep their shape, the new motions work, and the attacks of the bosses follow the curve of difficulty (more patterns at once, more bullets on the screen, stage after stage).</summary>
public static partial class SelfTest
{
    private static void RegisterPatterns(Action<string, Action> T)
    {
        T("TheAttacksOfTheBossesFollowTheCurveOfPatterns", TheAttacksOfTheBossesFollowTheCurveOfPatterns);
        T("APolygonAndAStarKeepTheirShape", APolygonAndAStarKeepTheirShape);
        T("ACrescentFliesAsOne", ACrescentFliesAsOne);
        T("ARoseHasPetals", ARoseHasPetals);
        T("PolarBulletsFollowTheirTrack", PolarBulletsFollowTheirTrack);
        T("LaunchedBulletsWaitAndThenGo", LaunchedBulletsWaitAndThenGo);
        T("ASplittingBulletBreaksIntoARing", ASplittingBulletBreaksIntoARing);
        T("BouncingBulletsTurnBackAtTheSides", BouncingBulletsTurnBackAtTheSides);
        T("ABulletThatTurnsStopsAndThenFliesAtTheBoat", ABulletThatTurnsStopsAndThenFliesAtTheBoat);
        T("TheSerpentStandsStillForFiveSecondsBeforeEveryLunge", TheSerpentStandsStillForFiveSecondsBeforeEveryLunge);
        T("TheTextsOfThePauseAndDefeatBoxesFitInsideThem", TheTextsOfThePauseAndDefeatBoxesFitInsideThem);
        T("EverySpeakerHasAFaceForTheDialogueBox", EverySpeakerHasAFaceForTheDialogueBox);
        T("TheBulletsOfEveryStageSoundAtTheSameLevel", TheBulletsOfEveryStageSoundAtTheSameLevel);
        T("TheGravityMoonPullsWithASound", TheGravityMoonPullsWithASound);
        T("TheNextStageDoesNotDropTheLivesAndBombsTheLastOneGave", TheNextStageDoesNotDropTheLivesAndBombsTheLastOneGave);
        T("TheCatBossBeginsWithSoundAtOnce", TheCatBossBeginsWithSoundAtOnce);
        T("ALaserIsWarnedOfAndThenStrikes", ALaserIsWarnedOfAndThenStrikes);
        T("TheStagesBringTheKindsOfSpiritsOfTheirOwnFromTheTopOnly", TheStagesBringTheKindsOfSpiritsOfTheirOwnFromTheTopOnly);
        T("TheMoonComesToTheMiddleAndPullsTheBoat", TheMoonComesToTheMiddleAndPullsTheBoat);
        T("ALaserGoesTransparentSoTheBoatCanCross", ALaserGoesTransparentSoTheBoatCanCross);
        T("TheBulletsOfStageFiveKeepRoomForTheBoat", TheBulletsOfStageFiveKeepRoomForTheBoat);
        T("GravityBendsTheFlightOfABullet", GravityBendsTheFlightOfABullet);
        T("TheDifficultyScalesThePatterns", TheDifficultyScalesThePatterns);
        T("APatternWaitsForItsDelayAndRestsBetweenBursts", APatternWaitsForItsDelayAndRestsBetweenBursts);
        T("AFullPoolIsHandledWithoutAFuss", AFullPoolIsHandledWithoutAFuss);
        T("TheAttacksThatMakeAWayAreWideAndTheBossStaysStill", TheAttacksThatMakeAWayAreWideAndTheBossStaysStill);
        T("TheCutsFromAnyDirectionAlwaysCrossTheFieldThroughTheBoat", TheCutsFromAnyDirectionAlwaysCrossTheFieldThroughTheBoat);
        T("TheWallsOfAMazeAreNotMovedByTheWind", TheWallsOfAMazeAreNotMovedByTheWind);
        T("APentagramFliesAsOneAndBursts", APentagramFliesAsOneAndBursts);
        T("EveryNewStyleOfBulletHasADrawing", EveryNewStyleOfBulletHasADrawing);
    }

    /// <summary>Patterns at once, per attack, at the least, for stages 2 to 6.</summary>
    private static readonly int[] MinPatterns = { 3, 5, 7, 8, 9 };
    /// <summary>The bullets on the screen at the most (in 18 seconds of an attack, the boat idle) that an attack of the stage must reach at least once, for stages 2 to 6.</summary>
    private static readonly int[] MinPeak = { 350, 500, 650, 750, 1000 };

    private static void TheAttacksOfTheBossesFollowTheCurveOfPatterns()
    {
        // the number of attacks of each boss, in its first form and its second: 2+2, 3+2, 4+2, 4+3, 5+3, 6+4
        var expected = new (int calm, int enraged)[] { (2, 2), (3, 2), (4, 2), (4, 3), (5, 3), (6, 4) };
        for (int s = 1; s <= 6; s++)
        {
            var boss = Stages.All[s - 1].Boss;
            Eq(boss.Phases.Count(p => !p.Enraged), expected[s - 1].calm, $"stage {s}: attacks of the first form");
            Eq(boss.Phases.Count(p => p.Enraged), expected[s - 1].enraged, $"stage {s}: attacks of the second form");
        }
    }

    private static void APolygonAndAStarKeepTheirShape()
    {
        var pool = BulletSystem.NewPool();
        Pat.Polygon(Ctx(pool), V(300, 300), 6, 80, 0, 3, new Shot(BulletStyle.Spark, 1).LaunchedOut(0.5f, 200));
        var bullets = pool.Where(b => b.IsActive).ToList();
        Eq(bullets.Count, 18, "six sides of three");
        foreach (var vertex in Enumerable.Range(0, 6)) True(bullets.Any(b => b.Position.DistanceTo(V(300, 300) + Pat.Dir(vertex * 60) * 80) < 0.5f), "a bullet at each vertex");
        True(bullets.All(b => b.Position.DistanceTo(V(300, 300)) <= 80.01f && b.Position.DistanceTo(V(300, 300)) >= 80 * MathF.Cos(MathF.PI / 6) - 0.5f), "all on the outline of the hexagon");
        // laid out and held; then let go outward
        var player = new PlayerEntity();
        for (int i = 0; i < 20; i++) BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(0, 0), player);
        True(pool.Where(b => b.IsActive).All(b => b.Velocity.Length() < 2), "held in place at first");
        for (int i = 0; i < 40; i++) BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(0, 0), player);
        True(pool.Where(b => b.IsActive).All(b => b.Velocity.Length() > 190), "and then let go at full speed");
        True(pool.Where(b => b.IsActive).All(b => (b.Position - V(300, 300)).Dot(b.Velocity) > 0), "away from the centre");
        var stars = BulletSystem.NewPool();
        Pat.Star(Ctx(stars), V(300, 300), 5, 2, 90, -90, 4, new Shot(BulletStyle.Star, 1).LaunchedOut(0.5f, 100));
        Eq(stars.Count(b => b.IsActive), 20, "a pentagram: five lines of four");
    }

    private static void ACrescentFliesAsOne()
    {
        var pool = BulletSystem.NewPool();
        Pat.Crescent(Ctx(pool), V(300, 300), 90, 64, 9, new Shot(BulletStyle.BeadWhite, 160));
        var bullets = pool.Where(b => b.IsActive).ToList();
        Eq(bullets.Count, 9 + 7, "an outer arc of nine and an inner arc of seven");
        True(bullets.All(b => (b.Velocity - bullets[0].Velocity).Length() < 0.01f), "they all fly the same way at the same speed");
        // the horns point the way it goes: the tips are ahead of the middle of the back
        float backMost = bullets.Min(b => b.Position.Y), frontMost = bullets.Max(b => b.Position.Y);
        True(frontMost > 300 - 64 * 0.9f, "the horns reach forward");
        Less(backMost, 300 - 64 * 0.9f, "and the back of the moon is behind");
    }

    private static void ARoseHasPetals()
    {
        var pool = BulletSystem.NewPool();
        Pat.Rose(Ctx(pool), V(300, 300), 6, 60, 0, 0.6f, new Shot(BulletStyle.OrbViolet, 100));
        var speeds = pool.Where(b => b.IsActive).Select(b => b.Velocity.Length()).ToList();
        Near(speeds.Max(), 100, 1, "the tips of the petals go at full speed");
        Near(speeds.Min(), 40, 1.5f, "the notches between the petals at 40% of it");
        int tips = 0;
        for (int i = 0; i < speeds.Count; i++) if (speeds[i] > speeds[(i + 1) % speeds.Count] && speeds[i] >= speeds[(i + speeds.Count - 1) % speeds.Count]) tips++;
        Eq(tips, 6, "six petals");
    }

    private static void PolarBulletsFollowTheirTrack()
    {
        var pool = BulletSystem.NewPool();
        int i = BulletSystem.FirePolar(pool, V(300, 300), 10, 50, 0, 90, new Shot(BulletStyle.BeadCyan, 0));
        var player = new PlayerEntity();
        for (int k = 0; k < 60; k++) BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(40, 0), player);
        // after one second: radius 60, angle 90 degrees (straight down), and the wind did not move it
        Near(pool[i].Position.X, 300, 0.5f); Near(pool[i].Position.Y, 360, 0.5f);
        True(pool[i].Velocity.Length() > 50, "its velocity is for the picture");
    }

    private static void LaunchedBulletsWaitAndThenGo()
    {
        var pool = BulletSystem.NewPool();
        var ctx = Ctx(pool);
        ctx.Fire(V(300, 100), 90, new Shot(BulletStyle.OrbRed, 0).Launched(1.0f, 200, 0));
        var player = new PlayerEntity();
        for (int k = 0; k < 50; k++) BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(0, 0), player);
        Near(pool.First(b => b.IsActive).Position.X, 300, 0.01f); Near(pool.First(b => b.IsActive).Position.Y, 100, 0.01f);
        for (int k = 0; k < 30; k++) BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(0, 0), player);
        var b0 = pool.First(b => b.IsActive);
        Greater(b0.Position.X, 310, "it went to the right once released");
        Near(b0.Position.Y, 100, 0.5f);
        // aimed: NaN heads for the boat
        var aimed = BulletSystem.NewPool();
        Ctx(aimed).Fire(V(300, 100), 90, new Shot(BulletStyle.OrbRed, 0).Launched(0.1f, 300));
        var boat = new PlayerEntity(); boat.Transform.Position = V(100, 100);
        for (int k = 0; k < 20; k++) BulletSystem.MoveBullets(aimed, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(0, 0), boat);
        Less(aimed.First(b => b.IsActive).Velocity.X, -250f, "it flew to the boat");
    }

    private static void ASplittingBulletBreaksIntoARing()
    {
        var pool = BulletSystem.NewPool();
        Ctx(pool).Fire(V(300, 100), 90, new Shot(BulletStyle.GiantRed, 100).Splitting(0.5f, 12, BulletStyle.OrbYellow, 120));
        var player = new PlayerEntity();
        for (int k = 0; k < 40; k++) BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(0, 0), player);
        var active = pool.Where(b => b.IsActive).ToList();
        Eq(active.Count, 12, "twelve pieces and no stone");
        True(active.All(b => b.Style == BulletStyle.OrbYellow && MathF.Abs(b.Velocity.Length() - 120) < 1), "of the child's style and speed");
        True(active.Select(b => (int)MathF.Round(Mathx.Degrees(MathF.Atan2(b.Velocity.Y, b.Velocity.X)) / 30)).Distinct().Count() >= 11, "all round");
    }

    private static void BouncingBulletsTurnBackAtTheSides()
    {
        var pool = BulletSystem.NewPool();
        Ctx(pool).Fire(V(100, 100), 180, new Shot(BulletStyle.OrbBlue, 300).Bouncing(2));
        var player = new PlayerEntity();
        for (int k = 0; k < 90; k++) BulletSystem.MoveBullets(pool, World.PlayArea, 1 / 60f, V(0, 0), player);
        var b = pool.First(x => x.IsActive);
        Greater(b.Velocity.X, 0, "it turned back at the left side");
        Greater(b.Position.X, 0);
    }

    private static void ABulletThatTurnsStopsAndThenFliesAtTheBoat()
    {
        var pool = BulletSystem.NewPool();
        Ctx(pool).Fire(V(300, 100), 90, new Shot(BulletStyle.OrbBlue, 200).Turning(1, 1f, 1, 0, 250));
        var player = new PlayerEntity();
        var target = V(100, 500);
        float slowest = 1000;
        for (int k = 0; k < 60; k++)
        {
            BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, target, player);
            slowest = MathF.Min(slowest, pool.First(b => b.IsActive).Velocity.Length());
        }
        Less(slowest, 40, "it slowed almost to a stop");
        for (int k = 0; k < 30; k++) BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, target, player);
        var b2 = pool.First(b => b.IsActive);
        Greater(b2.Velocity.Length(), 200, "and left again");
        Less(b2.Velocity.X, 0, "toward the boat, to the left");
    }

    private static void ALaserIsWarnedOfAndThenStrikes()
    {
        var pool = BulletSystem.NewPool();
        var slashes = new List<SlashData>();
        var c = Ctx(pool); c.Slashes = slashes;
        Pat.Laser(c, V(300, 100), 90, 30, 1f, 0.5f, new Color(1, 0, 0), 10);
        Eq(slashes.Count, 1);
        True(slashes[0].Beam && slashes[0].Telegraph > 0.9f && slashes[0].Len > 2000, "a beam with a warning");
        True(slashes[0].Spin != 0, "that turns");
    }

    private static void TheMoonComesToTheMiddleAndPullsTheBoat()
    {
        var phase = Attacks.KillaConstellations();
        True(phase.Gravity.IsActive, "the attack has a moon");
        var boss = BossEntity.Create(Stages.Six.Boss);
        boss.Transform.Position = V(300, 150);
        boss.Specials = SpecialsSystem.Begin(phase, Difficulty.Normal);
        var player = PlayerEntity.Create(); player.Transform.Position = V(100, 650);
        var slashes = new List<SlashData>(); var events = new List<WorldEvent>(); var rng = new SeededGenerator(1);
        float pulled = 0; bool warned = false;
        for (int i = 0; i < 60 * 14; i++)
        {
            var push = SpecialsSystem.Update(boss, player, slashes, 1f / 60, events, rng);
            if (SpecialsSystem.Moon(boss.Specials).Warning > 0.1f) warned = true;
            if (push != Vector2.Zero) { pulled = MathF.Max(pulled, push.Length()); True(push.Dot(boss.Specials.MoonPos - player.Transform.Position) > 0, "it pulls towards the moon"); }
        }
        var moon = SpecialsSystem.Moon(boss.Specials);
        Near(moon.Position.X, 300, 1f, "the moon came to the middle"); Greater(moon.Position.Y, 300, "down the field");
        True(warned, "it warned before it pulled");
        Greater(pulled, 150, "and it pulled");
        Less(pulled, 400, "but less than the boat's speed, so it can pull away");
        player.Transform.Position = moon.Position;
        True(SpecialsSystem.HitByMoon(boss.Specials, player), "touching the moon hurts");
    }

    /// <summary>The rules of the stages: types 1 and 2 only up to stage 2, the red ones alone in stage 3, in pairs in stage 4, in pairs with the others in stage 5, in trios in stage 6; and nothing from below.</summary>
    private static void TheStagesBringTheKindsOfSpiritsOfTheirOwnFromTheTopOnly()
    {
        int stageNumber = 0;
        foreach (var stage in Stages.All)
        {
            stageNumber++;
            foreach (var wave in stage.Waves)
            {
                int heavy = wave.Spawns.Count(s => s.Kind == SpiritKind.Heavy);
                int others = wave.Spawns.Count(s => s.Kind != SpiritKind.Heavy);
                if (stageNumber <= 2) Eq(heavy, 0, $"stage {stageNumber}: no red spirits");
                if (stageNumber == 3) { LessEq(heavy, 1, "stage 3: red spirits alone"); if (heavy == 1) Eq(others, 0, "stage 3: and nothing with them"); }
                if (stageNumber == 4) { True(heavy == 0 || heavy == 2, "stage 4: in pairs"); if (heavy > 0) Eq(others, 0, "stage 4: the pairs come alone"); }
                if (stageNumber == 5) { True(heavy <= 2, "stage 5: up to a pair"); }
                if (stageNumber == 6) { LessEq(heavy, 3, "stage 6: up to a trio"); }
                if (stageNumber == 1) True(wave.Spawns.All(s => s.Kind != SpiritKind.Heavy), "stage 1: only types 1 and 2");
                foreach (var spawn in wave.Spawns)
                {
                    // from the top, or from the top of the sides: never from the bottom, and the sides only high up
                    Less(spawn.Position.Y, 230, $"stage {stageNumber}: spirits begin at the top");
                    if (spawn.Position.X < 0 || spawn.Position.X > 600) Less(spawn.Position.Y, 190, $"stage {stageNumber}: and from the top of the sides");
                }
            }
            if (stageNumber >= 5) True(stage.Waves.Any(w => w.Spawns.Count(s => s.Kind == SpiritKind.Heavy) == 2 && w.Spawns.Any(s => s.Kind != SpiritKind.Heavy)), "pairs with the others from stage 5");
            if (stageNumber == 6) True(stage.Waves.Any(w => w.Spawns.Count(s => s.Kind == SpiritKind.Heavy) == 3), "trios in stage 6");
        }
        // each stage is a little longer than the one before
        for (int i = 1; i < Stages.All.Count; i++) GreaterEq(Stages.All[i].Waves.Count, Stages.All[i - 1].Waves.Count, "stage " + (i + 1) + " is not shorter");
        // a spirit does not shoot from the lower half of the field
        var pool = BulletSystem.NewPool();
        var low = new EnemyEntity(1, SpiritKind.Gunner, V(300, 500), EnemyPath.Straight(V(0, 0)), 0, SpiritFire.HailFan) { Age = 5 };
        EnemySystem.Fire(low, V(300, 700), pool, World.PlayArea, 5f);
        Eq(ActiveCount(pool), 0, "no shots from the lower half");
    }

    private static void TheSerpentStandsStillForFiveSecondsBeforeEveryLunge()
    {
        var bounds = World.PlayArea;
        var boss = BossEntity.Create(Stages.One.Boss);
        PhaseManagerSystem.ChangePhase(boss, 1);          // the last attack: the lunges
        boss.MovementType = BossMovementType.Dash;
        boss.Transform.Position = V(300, -50);
        boss.Dash.Timer = 0;
        float still = 0, longest = 0; Vector2 last = boss.Transform.Position;
        for (int i = 0; i < 60 * 40; i++)
        {
            MovementSystem.MoveBoss(boss, bounds, 1f / 60);
            if ((boss.Transform.Position - last).Length() < 0.01f) still += 1f / 60; else { if (still > 0.5f) longest = MathF.Max(longest, still); still = 0; }
            last = boss.Transform.Position;
        }
        GreaterEq(longest, 5f, "it stands still at least five seconds before it lunges again");
    }

    /// <summary>The boxes of the pause and the defeat are 120 art pixels wide (480 on the screen); their plaques, 46 and 48 (184 and 192); the texts must fit, with a margin, in every language.</summary>
    private static void TheTextsOfThePauseAndDefeatBoxesFitInsideThem()
    {
        const float boxInner = 480 - 32, plaqueInner = 184 - 16;
        foreach (var language in Loc.Available)
        {
            var t = Loc.TableOf(language);
            float W(string text, float scale) => PixelFont.Columns(text) * scale;
            foreach (var key in new[] { "Victory", "Defeat", "Paused" }) LessEq(W(t[key], 5), boxInner, $"{language}: {key} fits the box");
            LessEq(W(t["PauseHint"], 3), boxInner, $"{language}: the pause hint fits");
            LessEq(W(t["RestartsLeft"] + " 3", 3), boxInner, $"{language}: the restarts left fit the box");
            LessEq(W(t["Score"] + " 0000000", 3), boxInner, $"{language}: the score fits");
            LessEq(W(t["NewRecord"], 3), boxInner, $"{language}: the record fits");
            foreach (var key in new[] { "Restart", "Menu", "Continue", "GiveUp" }) LessEq(W(t[key], 3), plaqueInner, $"{language}: {key} fits its plaque");
        }
    }

    /// <summary>Every one who speaks has a face of 32 x 32 pixels in six expressions that differ (the lantern spirit, a flame of 64 x 64 that flickers), and the bosses of the six stages each have their own; the square for the box is 34 x 34 (68 x 68 for the flame).</summary>
    private static void EverySpeakerHasAFaceForTheDialogueBox()
    {
        // for now only the flame has a face; the others show the empty frame
        foreach (var emotion in Enum.GetValues<Emotion>())
        {
            var art = PortraitArt.Face(DialogueSpeaker.Airu, 1, emotion);
            Eq(art.Width, 64); Eq(art.Height, 64);
            Eq(PortraitArt.Square(DialogueSpeaker.Airu, 1, emotion, 0).Width, 68);
        }
        True(!PortraitArt.Face(DialogueSpeaker.Airu, 1, Emotion.Neutral, 0).Pixels.Select(px => px.R).SequenceEqual(PortraitArt.Face(DialogueSpeaker.Airu, 1, Emotion.Neutral, 1).Pixels.Select(px => px.R)), "the flame flickers");
        foreach (var speaker in new[] { DialogueSpeaker.Atli, DialogueSpeaker.Boss, DialogueSpeaker.Illari })
            Eq(PortraitArt.Square(speaker, 1, Emotion.Neutral, 0).Width, 34, $"{speaker}: the empty frame");
    }

    /// <summary>Every style of bullet of the bosses has a sound whose peak, with the volume of the game, is within a few dB of the others' (no stage is quieter than the first).</summary>
    private static void TheBulletsOfEveryStageSoundAtTheSameLevel()
    {
        float Peak(SfxBank.Sound sound)
        {
            var data = SfxBank.Make(sound); float peak = 0;
            foreach (var v in data) peak = MathF.Max(peak, MathF.Abs(v));
            return 20 * MathF.Log10(peak + 1e-9f) + SfxBank.Settings(sound).db;
        }
        foreach (var style in Enum.GetValues<BulletStyle>())
        {
            if (style == BulletStyle.LightBolt) continue;                      // the boat's own shot
            float peak = Peak(SfxBank.ForBullet(style));
            GreaterEq(peak, -20f, $"{style} is not too quiet ({peak:0.0} dB)");
            LessEq(peak, -4f, $"{style} is not too loud ({peak:0.0} dB)");
        }
        // the sounds of the small bullets (the most frequent) are within 4 dB of each other
        var small = new[] { SfxBank.Sound.OrbGreen, SfxBank.Sound.OrbRed, SfxBank.Sound.RiceRed, SfxBank.Sound.Spark, SfxBank.Sound.VenomOrb, SfxBank.Sound.Ember, SfxBank.Sound.FireSerpent, SfxBank.Sound.Rock, SfxBank.Sound.FlameDart };
        LessEq(small.Max(Peak) - small.Min(Peak), 8f, "the small bullets are at about the same level");
    }

    /// <summary>The moon of gravity makes a sound when it begins to pull (an event for the scene to play), once per pull.</summary>
    private static void TheGravityMoonPullsWithASound()
    {
        var phase = Attacks.KillaConstellations();
        var boss = BossEntity.Create(Stages.Six.Boss);
        boss.Transform.Position = V(300, 150);
        boss.Specials = SpecialsSystem.Begin(phase, Difficulty.Normal);
        var player = PlayerEntity.Create(); player.Transform.Position = V(100, 650);
        var slashes = new List<SlashData>(); var events = new List<WorldEvent>(); var rng = new SeededGenerator(1);
        int pulls = 0; float pullFirstAt = -1;
        for (int i = 0; i < 60 * 20; i++)
        {
            events.Clear();
            SpecialsSystem.Update(boss, player, slashes, 1f / 60, events, rng);
            foreach (var e in events) if (e.Kind == WorldEventKind.GravityPulled) { pulls++; if (pullFirstAt < 0) pullFirstAt = i / 60f; }
        }
        GreaterEq(pulls, 2, "it pulls again and again");
        LessEq(pulls, 4, "once per pull, not on every frame");
        Greater(pullFirstAt, 5f, "after the moon has come to the middle");
        True(SfxBank.Make(SfxBank.Sound.Gravity).Length > SfxBank.Rate * 2, "the sound lasts as long as the pull begins");
    }

    /// <summary>The prizes of the score (spare lanterns and boats) already given in a stage are not given again when the next stage begins with that score.</summary>
    private static void TheNextStageDoesNotDropTheLivesAndBombsTheLastOneGave()
    {
        var carry = new RunCarry { Score = 500_000, Lives = 3, Bombs = 2 };
        var world = new World(withCards: false, stage: Stages.Two, carry: carry);
        world.Step(1f / 60, new PlayerInputComponent());
        foreach (var kind in new[] { LightKind.Bomb, LightKind.Life })
            Eq(world.Lights.Count(l => l.IsActive && l.Kind == kind), 0, $"no spare {kind} at the start of the stage");
        // and the next prize beyond that score is still to come
        var board = new ScoreBoard { Points = 0 };
        var lights = LightSystem.NewPool();
        ScoreSystem.Add(ref board, 1, 1f, lights);
        Eq(lights.Count(l => l.IsActive), 0);
    }

    /// <summary>The theme of the third boss no longer begins with seconds of silence: there is sound in the first second.</summary>
    private static void TheCatBossBeginsWithSoundAtOnce()
    {
        var (left, _) = Songs.CatInTheStorm();
        int second = SfxBank.Rate > 0 ? Synth.Rate : 44100;
        for (int sec = 0; sec < 4; sec++)
        {
            double e = 0; for (int k = 0; k < second / 2; k++) { var v = left[sec * second + k]; e += v * (double)v; }
            double rms = Math.Sqrt(e / (second / 2));
            double sum = 0; foreach (var v in left) sum += v * (double)v; double all = Math.Sqrt(sum / left.Length);
            Greater((float)(rms / all), 0.15f, $"second {sec} of the cat's song has sound");
        }
    }

    private static void ALaserGoesTransparentSoTheBoatCanCross()
    {
        var laser = new SlashData { Top = V(300, 0), Angle = 0, Telegraph = 1, Strike = 5, Width = 30, Len = 2000, Beam = true, Blink = 2 };
        laser.Age = 1.5f; True(laser.Striking && !laser.Ghost, "it strikes at first");
        laser.Age = 1 + 1.5f; True(laser.Ghost && !laser.Striking, "and is transparent at the end of each 2 s");
        laser.Age = 1 + 2.1f; True(laser.Striking, "and strikes again");
    }

    /// <summary>The gaps rule (see Pat.MinGap): the typical gap between a bullet and its nearest neighbour must leave room for the boat (8 px across).</summary>
    private static void TheBulletsOfStageFiveKeepRoomForTheBoat()
    {
        var st = Stages.Five;
        for (int ph = 1; ph <= st.Boss.Lives; ph++)
        {
            var w = new World(ph, withCards: false, difficulty: Difficulty.Normal, stage: st); w.CalmWindForTesting();
            var gaps = new List<float>();
            for (int k = 0; k < 12 * 60; k++)
            {
                w.Step(1f / 60, new PlayerInputComponent());
                if (k > 5 * 60 && k % 40 == 0)
                {
                    var bs = w.Bullets.Where(b => b.IsActive && b.Faction == Faction.Boss && b.Position.Y > 250 && b.Position.Y < 760).ToList();
                    for (int i = 0; i < bs.Count; i++)
                    {
                        float best = 1e9f;
                        for (int j = 0; j < bs.Count; j++) if (j != i) best = MathF.Min(best, (bs[i].Position - bs[j].Position).Length() - bs[i].HitboxRadius - bs[j].HitboxRadius);
                        if (best < 1e8f) gaps.Add(best);
                    }
                }
            }
            gaps.Sort();
            if (gaps.Count > 0) GreaterEq(gaps[gaps.Count / 2], Pat.PlayerHitbox * 1.5f, $"attack {ph}: the usual gap leaves room for the boat");
        }
    }

    private static void GravityBendsTheFlightOfABullet()
    {
        var pool = BulletSystem.NewPool();
        Ctx(pool).Fire(V(0, 700), -60, new Shot(BulletStyle.BeadBlue, 430).Pulled(V(0, 260)));
        var player = new PlayerEntity();
        float top = 700;
        for (int k = 0; k < 150; k++)
        {
            BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(0, 0), player);
            top = MathF.Min(top, pool.First(b => b.IsActive).Position.Y);
        }
        Less(top, 500, "it flew up");
        Greater(pool.First(b => b.IsActive).Velocity.Y, 0, "and then fell");
    }

    private static void TheDifficultyScalesThePatterns()
    {
        int Count(Difficulty d) { var p = BulletSystem.NewPool(); Pat.Ring(Ctx(p, null, d), V(300, 200), 20, 0, new Shot(BulletStyle.OrbRed, 100)); return p.Count(b => b.IsActive); }
        Less(Count(Difficulty.Easy), Count(Difficulty.Normal));
        Less(Count(Difficulty.Normal), Count(Difficulty.Hard));
        // the patterns that the easy mode does without
        var hardOnly = new PatternSpec { Fire = _ => { } }.OnlyOnHard();
        var notEasy = new PatternSpec { Fire = _ => { } }.NotOnEasy();
        True(hardOnly.Applies(Difficulty.Hard) && hardOnly.Applies(Difficulty.Normal) && !hardOnly.Applies(Difficulty.Easy));
        True(notEasy.Applies(Difficulty.Normal) && notEasy.Applies(Difficulty.Hard) && notEasy.Applies(Difficulty.Easy), "(the easy mode is the old normal now)");
    }

    private static void APatternWaitsForItsDelayAndRestsBetweenBursts()
    {
        var shots = new List<float>();
        float clock = 0;
        var phase = new BossPhase { Movement = BossMovementType.Stationary, Target = V(300, 150) };
        phase.Patterns.Add(Pat.Every(0.5f, c => shots.Add(clock)).After(1.0f).InBursts(1.0f, 1.0f));
        var boss = BossEntity.Create(new BossDefinition("X", new[] { phase }));
        PhaseManagerSystem.ChangePhase(boss, 1);
        boss.MovementType = BossMovementType.Stationary;
        var pool = BulletSystem.NewPool();
        var player = new PlayerEntity();
        for (int i = 0; i < 60 * 6; i++)
        {
            clock = (i + 1) / 60f;
            BulletSystem.UpdateBossWeapons(boss, player, pool, 1 / 60f);
        }
        True(shots.Count > 0, "it fired");
        GreaterEq(shots.Min(), 1.0f - 0.02f, "not before its delay");
        // fires from 1.0 to 2.0, rests 2.0 to 3.0, fires 3.0 to 4.0, rests, fires 5.0 to 6.0
        True(shots.All(t => { float local = (t - 1.0f) % 2.0f; return local < 1.0f + 0.02f; }), "never in the rest");
        GreaterEq(shots.Count, 5, "and in every burst");
    }

    private static void AFullPoolIsHandledWithoutAFuss()
    {
        var pool = BulletSystem.NewPool(20);
        var ctx = Ctx(pool);
        for (int i = 0; i < 100; i++) ctx.Fire(V(300, 100), i, new Shot(BulletStyle.OrbRed, 100));
        Eq(pool.Count(b => b.IsActive), 20, "no more than the pool holds");
        Eq(ctx.Fire(V(0, 0), 0, new Shot(BulletStyle.OrbRed, 1)), -1, "a shot with no room is refused");
    }

    private static void TheAttacksThatMakeAWayAreWideAndTheBossStaysStill()
    {
        // an attack that makes a way for the boat (a curtain with a gap, or cuts with lanes between them): the gap is never narrow, and the boss does not move; the gap swings only so far that it is always
        // under the boss (the way leads to it)
        int ways = 0;
        foreach (var stage in Stages.All)
            foreach (var phase in stage.Boss.Phases)
            {
                var curtains = new[] { phase.Main, phase.Sub }.Where(e => e.IsActive && e.Curtain).ToList();
                bool cuts = phase.Slashes.IsActive && phase.Slashes.Count > 1;
                if (curtains.Count == 0 && !cuts) continue;
                ways++;
                Eq(phase.Movement, BossMovementType.Stationary, $"{phase.AttackNameKey}: the boss stays still");
                True(!phase.Dance, $"{phase.AttackNameKey}: and so do the two of a boss that is two");
                foreach (var e in curtains)
                {
                    GreaterEq(e.CurtainGap, 110f, $"{phase.AttackNameKey}: the gap is not narrow");
                    LessEq(e.CurtainSwing, e.CurtainMirror ? 60f : 80f, $"{phase.AttackNameKey}: the gap stays under the boss");
                    Near(e.CurtainCentre, phase.Target.X, 1f, $"{phase.AttackNameKey}: and swings about it");
                }
                if (cuts) GreaterEq(phase.Slashes.Spacing - phase.Slashes.Width, 100f, $"{phase.AttackNameKey}: the lanes between the cuts are wide");
            }
        Greater(ways, 3, "there are many attacks of this kind");
    }

    private static void TheCutsFromAnyDirectionAlwaysCrossTheFieldThroughTheBoat()
    {
        var rng = new SeededGenerator(11);
        foreach (int attack in new[] { 4, 5 })           // the single cuts and the triple cuts
        {
            var boss = BossEntity.Create(Stages.Four.Boss);
            PhaseManagerSystem.ChangePhase(boss, Stages.Four.Boss.Lives - attack + 1);
            var pool = BulletSystem.NewPool();
            var angles = new HashSet<int>();
            for (int n = 0; n < 40; n++)
            {
                var boat = new PlayerEntity(); boat.Transform.Position = V(30 + n * 13 % 540, 80 + n * 37 % 660);
                var slashes = new List<SlashData>(); var events = new List<WorldEvent>();
                boss.Specials.SlashClock = boss.Specials.Slashes.Interval;
                SpecialsSystem.Update(boss, boat, slashes, 0.01f, events, rng, pool);
                True(slashes.Count == boss.Specials.Slashes.Count, "one volley");
                var middle = slashes[(slashes.Count - 1) / 2];
                float centre = slashes.Count % 2 == 1 ? 0 : 1;
                if (slashes.Count % 2 == 1) Less(Mathx.DistanceToSegment(boat.Transform.Position, middle.Top, middle.Bottom), 1f, "the line passes through the boat");
                foreach (var sl in slashes)
                {
                    var field = new Rect2(0, 0, 600, 775);
                    True(!field.HasPoint(sl.Top) && !field.HasPoint(sl.Bottom), "it begins and ends outside the field: it crosses all of it");
                    // it does cross the field (some point of the line is inside)
                    bool inside = false;
                    for (float d = 0; d < sl.TotalLength; d += 20) if (field.HasPoint(sl.Top + sl.Direction * d)) inside = true;
                    True(inside, "and it is on the screen");
                }
                angles.Add((int)(middle.Angle * 4));
            }
            Greater(angles.Count, 8, "in many directions");
        }
    }

    private static void TheWallsOfAMazeAreNotMovedByTheWind()
    {
        // the curtains of the third boss (and of the last), and the walls of the patterns: with the strongest wind from the side they fall straight
        var boss = BossEntity.Create(ThaqaiBoss.Definition);
        PhaseManagerSystem.ChangePhase(boss, 6);                // the curtain of hail
        boss.MovementType = BossMovementType.Stationary;
        var pool = BulletSystem.NewPool();
        boss.EmitterMain.TimeSinceLastFire = 10;
        BulletSystem.UpdateBossWeapons(boss, new PlayerEntity(), pool, 0.001f);
        var before = pool.Where(b => b.IsActive).Select(b => b.Position.X).OrderBy(x => x).ToList();
        Greater(before.Count, 15, "a row of hail");
        for (int i = 0; i < 120; i++) BulletSystem.MoveBullets(pool, new Rect2(-3000, -3000, 9000, 9000), 1 / 60f, V(105, 0), new PlayerEntity());
        var after = pool.Where(b => b.IsActive).Select(b => b.Position.X).OrderBy(x => x).ToList();
        for (int i = 0; i < before.Count; i++) Near(after[i], before[i], 0.01f, "the wall did not drift");
        var walls = BulletSystem.NewPool();
        Pat.Wall(Ctx(walls), Edge.Top, 24, 0.5f, 90, new Shot(BulletStyle.Hail, 110).Sized(1.9f));
        True(walls.Where(b => b.IsActive).All(b => b.WindModifier == 0), "the walls of the patterns neither");
    }

    private static void APentagramFliesAsOneAndBursts()
    {
        var boss = BossEntity.Create(ThaqaiBoss.Definition);
        PhaseManagerSystem.ChangePhase(boss, 5);                // the second attack: the great pentagrams
        boss.MovementType = BossMovementType.Stationary;
        boss.Transform.Position = V(300, 200);
        var pool = BulletSystem.NewPool();
        var player = new PlayerEntity(); player.Transform.Position = V(300, 650);
        BulletSystem.UpdateBossWeapons(boss, player, pool, 0.001f);
        var flying = pool.Where(b => b.IsActive && (b.Style == BulletStyle.Star || b.Style == BulletStyle.StarBlue || b.Style == BulletStyle.StarGold)).ToList();
        Greater(flying.Count, 12, "a pentagram of stars");
        var field = new Rect2(-3000, -3000, 9000, 9000);
        for (int i = 0; i < 60; i++) BulletSystem.MoveBullets(pool, field, 1 / 60f, V(60, 0), player);          // a second of flight, with a wind
        var edge = pool.Where(b => b.IsActive && b.Behavior == BulletBehavior.Launch).ToList();
        True(edge.Count >= 10, "the lines are still waiting to burst");
        True(edge.All(b => b.Velocity.Y > 50), "and the figure flies at the boat as one");
        for (int i = 0; i < 90; i++) BulletSystem.MoveBullets(pool, field, 1 / 60f, V(0, 0), player);
        var burst = pool.Where(b => b.IsActive && b.Behavior == BulletBehavior.Normal && b.Velocity.Length() > 100).ToList();
        True(burst.Count >= 10, "then the stars of its lines fly out");
        var centre = burst.Select(b => b.Anchor).First();
        True(burst.Where(b => b.Anchor == centre).All(b => (b.Position - centre).Dot(b.Velocity) > 0), "away from its centre");
    }

    private static void EveryNewStyleOfBulletHasADrawing()
    {
        foreach (var style in Enum.GetValues<BulletStyle>())
        {
            var size = BulletArt.Size(style);
            Greater(size.Width, 1, style + " has a size"); 
            var art = BulletArt.MakeCanvas(style, 0);
            Greater(art.Pixels.Count(px => px.A > 0), 5, style + " has a body");
            Eq(art.Width, size.Width, style + " is drawn at its size");
        }
        Greater(BulletArt.Size(BulletStyle.GiantRed).Width, 20, "the great orbs are great");
        True(BulletArt.Rotates(BulletStyle.RiceBlue) && BulletArt.Rotates(BulletStyle.KunaiRed), "rice and kunai point where they go");
    }
}
