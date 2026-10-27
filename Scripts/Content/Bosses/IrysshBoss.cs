using Godot;

/// <summary>
/// The boss of the first stage. She rises from the river as a woman and, half way through, throws that shape off to become a giant
/// serpent of fire. Six attacks, one life each.
/// </summary>
public static class IrysshBoss
{
    public const string Name = "Iryssh";

    // Hitboxes: a third of the width of the sprite (96 and 224 points).
    private const float WomanHitbox = 32, SerpentHitbox = 75;

    private static BossPhase Woman(string attack, BossMovementType movement, Vector2 target, BulletEmitterComponent main, BulletEmitterComponent sub = default,
                                   BulletEmitterComponent hardExtra = default) =>
        new BossPhase
        {
            AttackName = attack, Form = BossForm.Human, Name = Name, Theme = MusicPlayer.Theme.Boss, MusicTitle = UIText.BossMusic, HitboxRadius = WomanHitbox,
            Movement = movement, Target = target, Main = main, Sub = sub, HardExtra = hardExtra,
        };

    private static BossPhase Serpent(string attack, BossMovementType movement, Vector2 target, BulletEmitterComponent main, BulletEmitterComponent sub = default,
                                     BulletEmitterComponent hardExtra = default, bool startsAtTarget = false, bool firstOfTheForm = false) =>
        new BossPhase
        {
            AttackName = attack, Form = BossForm.Serpent, Name = UIText.SerpentName, Theme = MusicPlayer.Theme.BossTwo, MusicTitle = UIText.BossTwoMusic, HitboxRadius = SerpentHitbox,
            Movement = movement, Target = target, StartsAtTarget = startsAtTarget, Main = main, Sub = sub, HardExtra = hardExtra,
            TransformationDialogue = firstOfTheForm ? Dialogues.Transformation : null,
        };

    private static readonly Vector2 Centre = new Vector2(300, Layout.BossY);

    public static readonly BossDefinition Definition = new BossDefinition(Name, new[]
    {
        // 1. A classic ring of fire, while she patrols the top. Hard adds a slower inner ring that turns the other way.
        Woman("Fire: Ring of Embers", BossMovementType.Patrol, Centre,
            main: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 1.0f, BulletsPerVolley = 36,
                SpreadAngle = 360, BulletSpeed = 150, SpinRate = 5, BulletScale = 1.0f, BulletStyle = BulletStyle.Fireball,
            },
            hardExtra: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 1.0f, BulletsPerVolley = 16, SpreadAngle = 360, BulletSpeed = 95,
                SpinRate = -4, BulletScale = 0.8f, BulletStyle = BulletStyle.Fireball,
            }),

        // 2. An aimed shotgun. Hard adds a fast dart straight at the boat between the volleys.
        Woman("Flame: Piercing Tongue", BossMovementType.Stationary, Centre,
            main: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 0.8f, BulletsPerVolley = 20,
                SpreadAngle = 120, BulletSpeed = 250, IsAimed = true, BulletScale = 1.2f, BulletStyle = BulletStyle.FlameDart,
            },
            hardExtra: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 0.4f, BulletsPerVolley = 1, SpreadAngle = 0, BulletSpeed = 420,
                IsAimed = true, BulletScale = 1.0f, BulletStyle = BulletStyle.FlameDart,
            }),

        // 3. Two spinners, one at each side.
        Woman("Blaze: Twin Whirlwinds", BossMovementType.Stationary, Centre,
            main: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 0.08f, BulletsPerVolley = 2,
                SpreadAngle = 180, BaseAngle = 0, BulletSpeed = 180, SpinRate = 12,
                BulletScale = 0.8f, SpawnOffset = new Vector2(-150, 0), BulletStyle = BulletStyle.Spark,
            },
            sub: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 0.08f, BulletsPerVolley = 2,
                SpreadAngle = 180, BaseAngle = 0, BulletSpeed = 180, SpinRate = 12,
                BulletScale = 0.8f, SpawnOffset = new Vector2(150, 0), BulletStyle = BulletStyle.VenomOrb,
            }),

        // 4. A lotus whose petals brake, curve and then dart at the boat. Hard adds a ring of slow embers.
        Woman("Lotus: Blazing Bloom", BossMovementType.Patrol, Centre,
            main: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 1.5f, BulletsPerVolley = 20,
                SpreadAngle = 360, BulletSpeed = 300, SpinRate = 10,
                Acceleration = -250, CurveRate = 45, BulletScale = 1.2f,
                BulletBehavior = BulletBehavior.StopAndAim, BulletStyle = BulletStyle.LotusPetal,
            },
            hardExtra: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 1.2f, BulletsPerVolley = 10, SpreadAngle = 360, BulletSpeed = 140,
                SpinRate = 8, BulletScale = 0.9f, BulletStyle = BulletStyle.Fireball,
            }),

        // 5. The serpent: big magma orbs at the boat, and a thin spiral of serpents of fire.
        Serpent("Serpent: Fury of the River", BossMovementType.Stationary, new Vector2(300, 150), firstOfTheForm: true,
            main: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 1.5f, BulletsPerVolley = 5,
                SpreadAngle = 60, BulletSpeed = 350, IsAimed = true, BulletScale = 4.0f, BulletStyle = BulletStyle.MagmaOrb,
            },
            sub: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 0.05f, BulletsPerVolley = 2,
                SpreadAngle = 180, BulletSpeed = 150, SpinRate = 35, BulletScale = 0.5f, BulletStyle = BulletStyle.FireSerpent,
            }),

        // 6. The serpent crosses the screen, leaving a trail of embers. Hard adds aimed embers.
        Serpent("Serpent: Tide of Embers", BossMovementType.Dash, new Vector2(300, -50), startsAtTarget: true,
            main: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 0.12f, BulletsPerVolley = 8,
                SpreadAngle = 360, BulletSpeed = 110, SpinRate = 15, BulletScale = 0.5f, BulletStyle = BulletStyle.Ember,
            },
            hardExtra: new BulletEmitterComponent
            {
                IsActive = true, FireRate = 0.6f, BulletsPerVolley = 3, SpreadAngle = 30, BulletSpeed = 260,
                IsAimed = true, BulletScale = 0.7f, BulletStyle = BulletStyle.Ember,
            }),
    });
}
