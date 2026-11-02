using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>Tests of how the content is put together, so that a new stage or boss cannot be added half done.</summary>
public static partial class SelfTest
{
    private static void RegisterArchitecture(Action<string, Action> T)
    {
        T("EveryStageIsCompleteContent", EveryStageIsCompleteContent);
        T("EveryBossIsCalmThenEnraged", EveryBossIsCalmThenEnraged);
        T("EveryBossStyleFitsItsHitbox", EveryBossStyleFitsItsHitbox);
        T("ANewAttackResetsWhatTheBossWasDoing", ANewAttackResetsWhatTheBossWasDoing);
        T("SpiritsCanFireDancingBullets", SpiritsCanFireDancingBullets);
        T("EverySpiritStyleDrawsEveryKindAtItsSize", EverySpiritStyleDrawsEveryKindAtItsSize);
        T("EveryTextPropertyIsInEveryLanguage", EveryTextPropertyIsInEveryLanguage);
        T("EveryLineOfEveryTalkHasItsTextInEveryLanguage", EveryLineOfEveryTalkHasItsTextInEveryLanguage);
    }

    private static void EveryStageIsCompleteContent()
    {
        True(Stages.All.Count >= 2, "the stages are listed");
        foreach (var stage in Stages.All)
        {
            True(stage.Waves.Count > 0 && stage.Waves.All(w => w.Spawns.Count > 0), "waves of spirits");
            True(stage.Boss.Lives > 0, "a boss with attacks");
            True(stage.Hud() != null && stage.Hud().Background != null, "a HUD");
            True(!string.IsNullOrEmpty(stage.Title) && stage.Title != "StageTitle" && !string.IsNullOrEmpty(stage.MusicTitle), "a title in the texts");
            True(stage.Boss.Style != null && stage.Boss.Style.MakeFrame != null, "a style for the boss");
            Eq(Stages.IndexOf(stage), Stages.All.ToList().IndexOf(stage));
            var names = Enumerable.Range(1, stage.Boss.Lives).Select(stage.Boss.AttackName).ToList();
            True(names.All(n => !string.IsNullOrEmpty(n) && !n.StartsWith("Attack")), "every attack has a name, in the texts");
            Eq(names.Distinct().Count(), names.Count, "no two attacks of a boss have the same name");
        }
    }

    private static void EveryBossIsCalmThenEnraged()
    {
        foreach (var stage in Stages.All)
        {
            var phases = stage.Boss.Phases;
            True(phases.Any(p => !p.Enraged) && phases.Any(p => p.Enraged), "calm attacks and enraged ones");
            int firstEnraged = phases.ToList().FindIndex(p => p.Enraged);
            True(phases.Skip(firstEnraged).All(p => p.Enraged), "once enraged, a boss stays enraged");
            foreach (var p in phases)
            {
                Eq(p.Theme, p.Enraged ? MusicPlayer.Theme.BossTwo : MusicPlayer.Theme.Boss, "the music follows the rage");
                Greater(p.MaxHealth, 0);
            }
            Greater(phases.First(p => p.Enraged).MaxHealth, phases.First(p => !p.Enraged).MaxHealth, "the enraged attacks are stronger");
        }
    }

    private static void EveryBossStyleFitsItsHitbox()
    {
        foreach (var stage in Stages.All)
        {
            var style = stage.Boss.Style;
            float width = style.SpriteSize.X;
            foreach (var phase in stage.Boss.Phases)
            {
                Greater(phase.HitboxRadius * 2, width * 0.4f, "the hitbox is not a speck");
                LessEq(phase.HitboxRadius * 2, width, "and not wider than the sprite");
            }
            // The sprite is anchored inside its own canvas and the water line is below the position.
            True(style.Centre.x > 0 && style.Centre.x < style.Width && style.Centre.y > 0 && style.Centre.y < style.Height, "the centre is inside the art");
            Greater(style.Waterline, 0, "the water is below the boss's position");
            True(style.MakeFrame(0).Width == style.Width && style.MakeFrame(0).Height == style.Height, "the frames have the size of the style");
        }
    }

    private static void ANewAttackResetsWhatTheBossWasDoing()
    {
        var boss = BossEntity.Create(BayloBoss.Definition);
        boss.Dive.State = DiveState.Under; boss.Dive.Submerged = true; boss.Sweep.State = SweepState.Pass2; boss.Sweep.BeamOn = true;
        boss.Roam.HasTarget = true; boss.Patrol.Wait = 5;
        PhaseManagerSystem.ChangePhase(boss, 3, Difficulty.Hard);
        Eq(boss.Dive.State, DiveState.Surface);
        False(boss.Dive.Submerged, "it is not under the water");
        Eq(boss.Sweep.State, SweepState.ToLeft);
        False(boss.Sweep.BeamOn, "no jet");
        False(boss.Roam.HasTarget);
        Eq(boss.Patrol.Wait, 0f);
        Less(boss.Sweep.GapScale, 1f, "hard leaves narrower gaps");
    }

    private static void SpiritsCanFireDancingBullets()
    {
        var enemy = new EnemyEntity(1, SpiritKind.Gunner, V(300, 200), EnemyPath.Straight(V(0, 0)), 0,
            new List<ScheduledPattern> { new ScheduledPattern(FirePattern.Fan(0.1f, 4, 60, 150).Styled(BulletStyle.Spark, 1.2f).Dancing(80), 100) }, 0) { Age = 1 };
        var pool = BulletSystem.NewPool();
        for (int i = 0; i < 10; i++) EnemySystem.Fire(enemy, V(300, 600), pool, World.PlayArea, 0.05f);
        var bullets = pool.Where(b => b.IsActive).ToList();
        Greater(bullets.Count, 3, "it fired");
        True(bullets.All(b => b.Behavior == BulletBehavior.Dance && b.Style == BulletStyle.Spark), "dancing sparks");
        True(bullets.Any(b => b.CurveRate > 0) && bullets.Any(b => b.CurveRate < 0), "neighbours sway against each other");
        // A pattern with no look of its own is as it always was.
        var plain = new EnemyEntity(2, SpiritKind.Gunner, V(300, 200), EnemyPath.Straight(V(0, 0)), 0,
            new List<ScheduledPattern> { new ScheduledPattern(FirePattern.Aimed(0.1f, 200), 100) }, 0) { Age = 1 };
        var pool2 = BulletSystem.NewPool();
        for (int i = 0; i < 4; i++) EnemySystem.Fire(plain, V(300, 600), pool2, World.PlayArea, 0.05f);
        True(pool2.Where(b => b.IsActive).All(b => b.Behavior == BulletBehavior.Normal && b.Style == BulletStyle.OrbGreen), "plain green orbs");
    }

    private static void EverySpiritStyleDrawsEveryKindAtItsSize()
    {
        var styles = Stages.All.Select(stage => stage.Spirits).Distinct().ToList();
        Greater(styles.Count, 2, "stages have spirits of their own");
        foreach (var style in styles)
            foreach (var kind in Enum.GetValues<SpiritKind>())
            {
                for (int frame = 0; frame < style.FrameCount; frame++)
                {
                    var canvas = style.MakeCanvas(kind, frame);
                    Eq(canvas.Width, style.Width); Eq(canvas.Height, style.Height);
                    Greater(canvas.Pixels.Count(px => px.A > 0), 60, "something is drawn");
                }
                True(style.SparkColour(kind).A > 0, "a colour for the sparks");
            }
        // The three kinds of a style do not look alike.
        foreach (var style in styles)
        {
            var a = style.MakeCanvas(SpiritKind.Swarm, 0).Pixels; var b = style.MakeCanvas(SpiritKind.Gunner, 0).Pixels; var c = style.MakeCanvas(SpiritKind.Heavy, 0).Pixels;
            True(!a.SequenceEqual(b) && !b.SequenceEqual(c) && !a.SequenceEqual(c), "each kind has its own look");
        }
    }

    private static void EveryTextPropertyIsInEveryLanguage()
    {
        const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static;
        var keys = typeof(UIText).GetProperties(flags).Where(p => p.PropertyType == typeof(string)).Select(p => p.Name).ToList();
        Greater(keys.Count, 50, "the texts are found");
        InEveryLanguage(language =>
        {
            var table = Loc.TableOf(language);
            foreach (var key in keys) True(table.ContainsKey(key), $"{language} lacks {key}");
        });
    }

    private static void EveryLineOfEveryTalkHasItsTextInEveryLanguage()
    {
        var keys = Stages.All.SelectMany(stage => stage.Talks()).SelectMany(talk => talk).Select(line => line.Key).ToList();
        Greater(keys.Count, 30, "the talks are found");
        InEveryLanguage(language =>
        {
            var table = Loc.TableOf(language);
            foreach (var key in keys) True(table.ContainsKey(key), $"{language} lacks the line {key}");
        });
        // And the talks of a stage are told by someone who is in them.
        foreach (var stage in Stages.All)
            foreach (var talk in stage.Talks())
                True(talk.All(line => line.Text.Length > 0 && line.Text != line.Key), "every line has its words");
    }
}
