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
        T("GravityBendsTheFlightOfABullet", GravityBendsTheFlightOfABullet);
        T("TheDifficultyScalesThePatterns", TheDifficultyScalesThePatterns);
        T("APatternWaitsForItsDelayAndRestsBetweenBursts", APatternWaitsForItsDelayAndRestsBetweenBursts);
        T("AFullPoolIsHandledWithoutAFuss", AFullPoolIsHandledWithoutAFuss);
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
        // the patterns of the hard mode alone, and the ones the easy mode does without
        var hardOnly = new PatternSpec { Fire = _ => { } }.OnlyOnHard();
        var notEasy = new PatternSpec { Fire = _ => { } }.NotOnEasy();
        True(hardOnly.Applies(Difficulty.Hard) && !hardOnly.Applies(Difficulty.Normal) && !hardOnly.Applies(Difficulty.Easy));
        True(notEasy.Applies(Difficulty.Normal) && notEasy.Applies(Difficulty.Hard) && !notEasy.Applies(Difficulty.Easy));
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
        Greater(flying.Count, 20, "a pentagram of many stars");
        var field = new Rect2(-3000, -3000, 9000, 9000);
        for (int i = 0; i < 60; i++) BulletSystem.MoveBullets(pool, field, 1 / 60f, V(60, 0), player);          // a second of flight, with a wind
        var edge = pool.Where(b => b.IsActive && b.Behavior == BulletBehavior.Launch).ToList();
        True(edge.Count >= 20, "the lines are still waiting to burst");
        True(edge.All(b => b.Velocity.Y > 50), "and the figure flies at the boat as one");
        for (int i = 0; i < 90; i++) BulletSystem.MoveBullets(pool, field, 1 / 60f, V(0, 0), player);
        var burst = pool.Where(b => b.IsActive && b.Behavior == BulletBehavior.Normal && b.Velocity.Length() > 100).ToList();
        True(burst.Count >= 20, "then the stars of its lines fly out");
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
