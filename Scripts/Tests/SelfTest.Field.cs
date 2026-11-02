using System;
using System.Linq;
using Godot;

/// <summary>Tests of the room the boat has in the boss fight, and of the point of collection (the lights fly to a boat that is high in the picture at full power).</summary>
public static partial class SelfTest
{
    private static void RegisterField(Action<string, Action> T)
    {
        T("TheBoatHasMoreRoomAtTheSidesOnceTheBanksHaveGone", TheBoatHasMoreRoomAtTheSidesOnceTheBanksHaveGone);
        T("TheCurtainOfAWiderFieldHasNoPlaceToHideAtTheSides", TheCurtainOfAWiderFieldHasNoPlaceToHideAtTheSides);
        T("AtFullPowerHighInThePictureEveryLightFliesToTheBoat", AtFullPowerHighInThePictureEveryLightFliesToTheBoat);
        T("TheMainMenuEndsWithAButtonToQuit", TheMainMenuEndsWithAButtonToQuit);
    }

    private static void TheBoatHasMoreRoomAtTheSidesOnceTheBanksHaveGone()
    {
        var world = new World(practicePhase: 1, withCards: false);
        Near(world.Field.Position.X, 0, 0.001f, "the banks are in: the play area");
        Eq(world.Field.Size.X, World.PlayArea.Size.X);
        var left = new Vector2(-1, 0);
        float Run(bool toLeft)
        {
            world.Player.Transform.Position = V(300, 600);
            for (int i = 0; i < 60 * 5; i++)
            {
                world.Player.Health.InvincibilityTimer = 10;
                world.Step(1 / 60f, new PlayerInputComponent { Direction = toLeft ? left : -left });
            }
            return world.Player.Transform.Position.X;
        }
        Near(Run(true), 0, 1f, "kept at the left edge while the banks are in");
        Near(Run(false), 600, 1f, "and at the right");
        world.FieldOpen = 0.5f;
        Near(world.Field.Position.X, -World.FieldExtraMax / 2, 0.01f);
        world.FieldOpen = 1;
        Near(world.Field.Position.X, -World.FieldExtraMax, 0.01f, "the banks are gone: more room");
        Near(world.Field.End.X, 600 + World.FieldExtraMax, 0.01f);
        Near(Run(true), -World.FieldExtraMax, 1f, "the boat goes into the room the banks took");
        Near(Run(false), 600 + World.FieldExtraMax, 1f);
        // The boss goes where the boat can go, too: the roam keeps inside the wider field.
        var boss = BossEntity.Create(BayloBoss.Definition);
        PhaseManagerSystem.ChangePhase(boss, 5);
        boss.MovementType = BossMovementType.Roam;
        float minX = 999, maxX = -999;
        for (int i = 0; i < 60 * 150; i++)
        {
            MovementSystem.MoveBoss(boss, world.Field, 1 / 60f);
            minX = MathF.Min(minX, boss.Transform.Position.X); maxX = MathF.Max(maxX, boss.Transform.Position.X);
        }
        Less(minX, 80, "it goes to the left, near the old edge or beyond"); Greater(maxX, 500, "and to the right");
    }

    private static void TheCurtainOfAWiderFieldHasNoPlaceToHideAtTheSides()
    {
        var boss = BossEntity.Create(ThaqaiBoss.Definition);
        PhaseManagerSystem.ChangePhase(boss, 3);
        boss.MovementType = BossMovementType.Stationary;
        var wide = new Rect2(-World.FieldExtraMax, 0, 600 + 2 * World.FieldExtraMax, 775);
        var pool = BulletSystem.NewPool();
        boss.EmitterMain.TimeSinceLastFire = 10;
        BulletSystem.UpdateBossWeapons(boss, new PlayerEntity(), pool, 0.001f, Difficulty.Normal, wide);
        var row = pool.Where(b => b.IsActive).Select(b => b.Position.X).OrderBy(x => x).ToList();
        Less(row.First(), -World.FieldExtraMax + 20, "the row starts at the left of the wider field");
        Greater(row.Last(), 600 + World.FieldExtraMax - 20, "and ends at the right of it");
        float widest = 0;
        for (int i = 1; i < row.Count; i++) widest = MathF.Max(widest, row[i] - row[i - 1]);
        Less(widest, boss.EmitterMain.CurtainGap + 50, "only the gap is open (the hole between two bullets of the row is the gap and a slot)");
    }

    private static void AtFullPowerHighInThePictureEveryLightFliesToTheBoat()
    {
        World MakeWorld(float y, float energy)
        {
            var world = new World(withCards: false);
            world.Player.Transform.Position = V(300, y);
            world.Player.Energy = energy;
            LightSystem.Spawn(world.Lights, V(560, 700), 1);
            LightSystem.SpawnReward(world.Lights, LightKind.Bomb, 40, 600);
            LightSystem.SpawnReward(world.Lights, LightKind.Life, 520, 500);
            for (int i = 0; i < world.Lights.Length; i++) if (world.Lights[i].IsActive) { world.Lights[i].Velocity = Vector2.Zero; world.Lights[i].Age = 1; }
            return world;
        }
        int Active(World w) => w.Lights.Count(l => l.IsActive);
        void Run(World w, float seconds)
        {
            // (the boat is kept where it was put: the wind would push it about)
            float y = w.Player.Transform.Position.Y;
            for (int i = 0; i < 60 * seconds; i++)
            {
                w.Player.Health.InvincibilityTimer = 10; w.Enemies.Clear();
                w.Player.Transform.Position = new Vector2(300, y);
                w.Step(1 / 60f, Idle());
            }
        }
        var high = MakeWorld(World.CollectLine - 60, PowerCurve.MaxEnergy);
        True(high.Collecting, "high, at full power");
        Run(high, 3);
        Eq(Active(high), 0, "every light, bomb and boat came to the boat, from the other side of the picture");
        Greater(high.Score, 0, "(and were worth something)");
        var low = MakeWorld(World.CollectLine + 200, PowerCurve.MaxEnergy);
        False(low.Collecting, "low in the picture: no");
        Run(low, 3);
        Greater(Active(low), 0, "the lights stay where they fall");
        var weak = MakeWorld(World.CollectLine - 60, PowerCurve.Level2);
        False(weak.Collecting, "high, but not at full power: no");
        Run(weak, 3);
        Greater(Active(weak), 0);
    }

    private static void TheMainMenuEndsWithAButtonToQuit()
    {
        var menu = new MenuScene();
        menu.Show(MenuScene.Screen.Main);
        var last = menu.Entries[^1];
        Eq(last.Action, MenuScene.ActionKind.Quit);
        Eq(last.Title, UIText.MenuQuit);
        True(last.Title.Length > 0 && last.Title != "MenuQuit", "a title in the texts");
        bool asked = false;
        menu.Quit += () => asked = true;
        menu.Select(menu.Entries.Count - 1);
        menu.ActivateSelection();
        True(asked, "choosing it asks to close the game");
        menu.Free();
    }
}
