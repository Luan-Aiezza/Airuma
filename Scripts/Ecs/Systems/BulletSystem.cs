using System;
using System.Collections.Generic;
using Godot;

/// <summary>Every bullet: the pool, the player's weapon and its cards, the boss's emitters, and how bullets move (acceleration, curves, wind, homing).</summary>
public static class BulletSystem
{
    public const int PoolSize = 1000;

    /// <summary>A pool of inactive bullets with their defaults filled in.</summary>
    public static BulletData[] NewPool(int count = PoolSize)
    {
        var pool = new BulletData[count];
        for (int i = 0; i < count; i++) pool[i] = new BulletData();
        return pool;
    }

    /// <summary>The beat of the dancing bullets: half a second, like a march or a samba at 120 beats a minute.</summary>
    public const float DanceBeat = 0.5f;

    /// <summary>Puts a bullet in the pool; returns where (-1 if the pool is full).</summary>
    public static int Spawn(
        BulletData[] pool,
        Vector2 position,
        Vector2 velocity,
        Faction faction,
        BulletStyle style,
        float scale = 1,
        float acceleration = 0,
        float curve = 0,
        BulletBehavior behavior = BulletBehavior.Normal,
        float power = 1,
        int pierce = 0,
        float burstRadius = 0,
        float burstScale = 1)
    {
        int i = -1;
        for (int k = 0; k < pool.Length; k++) if (!pool[k].IsActive) { i = k; break; }
        if (i < 0) return -1;
        pool[i] = new BulletData
        {
            IsActive = true,
            Fresh = true,
            Position = position,
            Velocity = velocity,
            Faction = faction,
            HitboxRadius = 5 * scale, // the hitbox grows with the scale
            Damage = 1,
            WindModifier = faction == Faction.Player ? 1.0f : 0.5f,
            Acceleration = acceleration,
            CurveRate = curve,
            Scale = scale,
            Behavior = behavior,
            Timer = 0,
            Style = style,
            Power = power,
            Pierce = pierce,
            BurstRadius = burstRadius,
            BurstScale = burstScale,
        };
        return i;
    }

    /// <summary>Seconds between volleys of homing wisps.</summary>
    public const float WispInterval = 0.4f;
    public const float WispSpeed = 420;
    /// <summary>How fast a wisp can turn, in degrees per second.</summary>
    public const float WispTurnRate = 260;
    public const float WispPower = 1.4f;
    /// <summary>Angle between neighbouring bolts of a fan, in degrees.</summary>
    public const float FanAngle = 7;
    /// <summary>Strength of each bolt of a fan, against the 1 of a bolt of the main stream. The fan does not grow with the weapon's level.</summary>
    public const float FanPower = 0.4f;
    /// <summary>The scale of the player's bolts (their hitbox grows with it): wide enough not to slip between the spirits of a formation.</summary>
    public const float BoltScale = 1.7f;
    /// <summary>Focusing: the bolts crowd together (this far apart, instead of 15), each hits harder, and the fans close up to a narrow angle.</summary>
    public const float FocusSpacing = 7, FocusPower = 1.4f, FocusFanAngle = 2.5f;

    public static void UpdatePlayerWeapons(PlayerEntity player, BulletData[] pool, float delta, CardEffects? effectsOrNull = null)
    {
        CardEffects effects = effectsOrNull ?? new CardEffects();
        player.Weapon.TimeSinceLastFire += delta;
        player.Weapon.TimeSinceLastWisp += delta;
        if (effects.HomingWisps > 0 && player.Input.IsShooting && player.Weapon.TimeSinceLastWisp >= WispInterval)
        {
            player.Weapon.TimeSinceLastWisp = 0;
            for (int k = 0; k < effects.HomingWisps; k++)
            {
                // They leave the lantern, one to each side in turn.
                float side = k % 2 == 0 ? -1 : 1;
                Spawn(pool,
                      player.Transform.Position + new Vector2(side * 12, -4),
                      new Vector2(side * 120, -WispSpeed),
                      Faction.Player, BulletStyle.Wisp, behavior: BulletBehavior.Homing, power: WispPower);
            }
        }

        if (!(player.Input.IsShooting && player.Weapon.TimeSinceLastFire >= player.Weapon.FireRate)) return;

        player.Weapon.TimeSinceLastFire = 0;
        bool focus = player.Input.IsFocused;
        float spacing = focus ? FocusSpacing : 15;
        float focusPower = focus ? FocusPower : 1;

        Vector2[] offsets;
        switch (player.Weapon.PowerLevel)
        {
            case 1: offsets = new[] { Vector2.Zero }; break;
            case 2: offsets = new[] { new Vector2(-spacing, 0), new Vector2(spacing, 0) }; break;
            case 3: offsets = new[] { Vector2.Zero, new Vector2(-spacing, 0), new Vector2(spacing, 0) }; break;
            default: offsets = new[] { Vector2.Zero }; break;
        }

        // The main stream: one, two or three bolts, by the weapon's level. The cards' effects are the same whatever the level:
        // the bolts of a volley share one burst between them.
        foreach (Vector2 offset in offsets)
        {
            Spawn(pool,
                  player.Transform.Position + offset,
                  new Vector2(0, -player.Weapon.BulletSpeed),
                  Faction.Player,
                  BulletStyle.LightBolt,
                  scale: BoltScale,
                  power: effects.DamageMultiplier * focusPower,
                  pierce: effects.Pierce,
                  burstRadius: effects.BurstRadius,
                  burstScale: 1f / offsets.Length);
        }

        // The fans: a pair of lighter bolts for each, from the lantern itself, one volley after another, however strong the weapon.
        for (int k = 1; k <= effects.FanPairs; k++)
        {
            foreach (float side in new[] { -1f, 1f })
            {
                Spawn(pool,
                      player.Transform.Position,
                      new Vector2(0, -player.Weapon.BulletSpeed).Rotated(Mathx.Radians(side * k * (focus ? FocusFanAngle : FanAngle))),
                      Faction.Player,
                      BulletStyle.LightBolt,
                      scale: BoltScale,
                      power: FanPower * effects.DamageMultiplier * focusPower,
                      pierce: effects.Pierce,
                      burstRadius: effects.BurstRadius,
                      burstScale: 1f);
            }
        }
    }

    public static void UpdateBossWeapons(BossEntity boss, PlayerEntity player, BulletData[] pool, float delta, Difficulty difficulty = Difficulty.Normal, Rect2? field = null, BossEntity twin = null)
    {
        // No shooting during the breather between phases.
        if (boss.MovementType == BossMovementType.Transition) return;

        // Out of the water the dolphin throws up a fan of spray; it fires nothing while it sinks, is under, or leaps.
        if (boss.Dive.Emerged) { boss.Dive.Emerged = false; SpawnSplash(boss, pool, difficulty); }
        if (boss.Dive.Submerged || (boss.MovementType == BossMovementType.Dive && boss.Dive.State != DiveState.Surface)) return;

        if (boss.EmitterMain.IsActive)
            ProcessEmitter(boss, player, ref boss.EmitterMain, pool, delta, field ?? World.PlayArea, twin);
        if (boss.EmitterSub.IsActive)
            ProcessEmitter(boss, player, ref boss.EmitterSub, pool, delta, field ?? World.PlayArea, twin);
    }

    /// <summary>The spray where the dolphin leaps out: a fan of bullets going up and out from the water.</summary>
    private static void SpawnSplash(BossEntity boss, BulletData[] pool, Difficulty difficulty)
    {
        int count = difficulty.Scale(18, 6);
        Vector2 at = boss.Transform.Position + new Vector2(0, -boss.Hitbox.Radius);
        for (int i = 0; i < count; i++)
        {
            float angle = Mathx.Radians(200 + 140 * (count > 1 ? (float)i / (count - 1) : 0.5f));
            Spawn(pool, at, new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * 190, Faction.Boss, BulletStyle.Spark, scale: 0.9f);
        }
    }

    private static void ProcessEmitter(BossEntity boss, PlayerEntity player, ref BulletEmitterComponent emitter, BulletData[] pool, float delta, Rect2 field, BossEntity twin = null)
    {
        // Where the bullets come from: the boss, or, for a boss that is two, the second body.
        var source = emitter.FromTwin && twin != null ? twin : boss;
        if (emitter.QuakeOnly && boss.Specials.QuakeLeft <= 0) return;
        emitter.CurtainClock += delta;
        emitter.TimeSinceLastFire += delta;
        if (!(emitter.TimeSinceLastFire >= emitter.FireRate)) return;
        emitter.TimeSinceLastFire = 0;

        // Continuous spin for spirals.
        emitter.BaseAngle += emitter.SpinRate;

        // Aimed attacks ignore the spin and point at the player.
        if (emitter.IsAimed)
        {
            Vector2 toPlayer = player.Transform.Position - source.Transform.Position;
            emitter.BaseAngle = Mathx.Degrees(MathF.Atan2(toPlayer.Y, toPlayer.X));
            emitter.BaseAngle -= emitter.SpreadAngle / 2; // centre the cone
        }

        if (emitter.Curtain) { FireCurtain(emitter, pool, field); return; }
        if (emitter.Ceiling) { FireCeiling(emitter, pool, field); return; }

        int bullets = emitter.BulletsPerVolley;
        float angleStep = bullets > 1 ? emitter.SpreadAngle / bullets : 0;
        Vector2 spawnPos = source.Transform.Position + emitter.SpawnOffset;

        var slots = emitter.Link ? new int[bullets] : null;
        for (int i = 0; i < bullets; i++)
        {
            int slot = FireBullet(pool, spawnPos, emitter.BaseAngle + i * angleStep, emitter.BulletSpeed, emitter.Rainbow ? RainbowStyles[i % RainbowStyles.Length] : emitter.BulletStyle, emitter.BulletScale,
                                  emitter.Acceleration, emitter.AlternateCurve && i % 2 == 1 ? -emitter.CurveRate : emitter.CurveRate, emitter.BulletBehavior);
            if (slots != null) slots[i] = slot;
        }
        if (slots != null) LinkStars(pool, slots, Math.Max(1, emitter.LinkSkip));
    }

    private static int _nextChain = 1;

    /// <summary>Links the stars of a volley into a constellation: each to the one `skip` places before it, round the volley.</summary>
    private static void LinkStars(BulletData[] pool, int[] slots, int skip)
    {
        int chain = _nextChain++;
        int n = slots.Length;
        for (int i = 0; i < n; i++)
        {
            if (slots[i] < 0) continue;
            pool[slots[i]].Chain = chain;
            int prev = slots[((i - skip) % n + n) % n];
            pool[slots[i]].Prev = prev == slots[i] ? -1 : prev;
        }
    }

    /// <summary>The x of the gap (or of the first gap) of a curtain at a moment.</summary>
    public static float CurtainGapCentre(in BulletEmitterComponent e) =>
        e.CurtainCentre + e.CurtainSwing * MathF.Sin(MathF.Tau * e.CurtainClock / MathF.Max(0.1f, e.CurtainPeriod));

    /// <summary>
    /// A curtain: a row of bullets across the whole field, falling, with a gap (or two) that swings from side to side. A wider field (in the boss fight)
    /// has more bullets in the row, as far apart as before, so that there is no place at the sides to hide from it.
    /// </summary>
    private static void FireCurtain(in BulletEmitterComponent e, BulletData[] pool, Rect2 field)
    {
        float width = field.Size.X, centre = field.Position.X + width / 2;
        int slots = (int)MathF.Round(e.CurtainSlots * width / World.PlayArea.Size.X);
        float gapX = CurtainGapCentre(e);
        for (int i = 0; i < slots; i++)
        {
            float x = field.Position.X + width * (i + 0.5f) / slots;
            bool inGap = MathF.Abs(x - gapX) < e.CurtainGap / 2 || (e.CurtainMirror && MathF.Abs(x - (2 * centre - gapX)) < e.CurtainGap / 2);
            if (inGap) continue;
            FireBullet(pool, new Vector2(x, -12), 90, e.BulletSpeed, e.BulletStyle, e.BulletScale, e.Acceleration, e.CurveRate, e.BulletBehavior);
        }
    }

    /// <summary>The ceiling falls: bullets come in from the top at random places across the field, a little off straight down and not all at the same speed.</summary>
    private static void FireCeiling(in BulletEmitterComponent e, BulletData[] pool, Rect2 field)
    {
        for (int i = 0; i < Math.Max(1, e.BulletsPerVolley); i++)
        {
            float x = field.Position.X + 16 + (float)Random.Shared.NextDouble() * (field.Size.X - 32);
            float angle = 90 + ((float)Random.Shared.NextDouble() - 0.5f) * 10;
            float speed = e.BulletSpeed * (0.8f + 0.4f * (float)Random.Shared.NextDouble());
            FireBullet(pool, new Vector2(x, -14), angle, speed, e.BulletStyle, e.BulletScale, e.Acceleration, e.CurveRate, e.BulletBehavior);
        }
    }

    /// <summary>The thin lines of the constellations that hang: where they are, and whether the stars are still (a still line is dangerous).</summary>
    public static IEnumerable<(Vector2 a, Vector2 b, bool still)> Links(BulletData[] pool)
    {
        for (int i = 0; i < pool.Length; i++)
        {
            ref BulletData b = ref pool[i];
            if (!b.IsActive || b.Behavior != BulletBehavior.Constellation || b.Chain == 0 || b.Prev < 0) continue;
            ref BulletData p = ref pool[b.Prev];
            if (!p.IsActive || p.Chain != b.Chain || p.Behavior != BulletBehavior.Constellation) continue;
            yield return (p.Position, b.Position, b.Timer >= StarFlySeconds && p.Timer >= StarFlySeconds);
        }
    }

    /// <summary>How long a star flies out before it stops, how long it hangs, and how fast it falls afterwards.</summary>
    public const float StarFlySeconds = 0.75f, StarHangSeconds = 3.0f, StarFallSpeed = 150;
    /// <summary>The seconds between the turns of a zigzag, and half the width of a line of a constellation.</summary>
    /// <summary>The seven colours of the rainbow, as the bullets that come closest to them: red, orange, yellow, green, blue, indigo and violet.</summary>
    public static readonly BulletStyle[] RainbowStyles = { BulletStyle.OrbRed, BulletStyle.Fireball, BulletStyle.Star, BulletStyle.OrbGreen, BulletStyle.Wisp, BulletStyle.VenomOrb, BulletStyle.LotusPetal };
    public const float ZigzagInterval = 0.3f, LineHalfWidth = 3;
    /// <summary>Seconds a great stone flies before it breaks, how many small stones it breaks into, and their speed.</summary>
    public const float BoulderFuse = 1.3f, BoulderFragmentSpeed = 150;
    public const int BoulderFragments = 14;

    /// <summary>
    /// A bullet of the enemy side (a boss's or a spirit's), going off at `angle` degrees. The one way bullets are fired: the emitters of the bosses
    /// and the patterns of the spirits both end here.
    /// </summary>
    public static int FireBullet(BulletData[] pool, Vector2 origin, float angle, float speed, BulletStyle style, float scale = 1,
                                 float acceleration = 0, float curve = 0, BulletBehavior behavior = BulletBehavior.Normal)
    {
        float a = Mathx.Radians(angle);
        return Spawn(pool, origin, new Vector2(MathF.Cos(a), MathF.Sin(a)) * speed, Faction.Boss, style, scale: scale, acceleration: acceleration, curve: curve, behavior: behavior);
    }

    public static void MoveBullets(BulletData[] pool, Rect2 screenBounds, float delta, Vector2 wind, PlayerEntity player, IList<Vector2> targets = null)
    {
        // Slightly larger kill margin so curving bullets can leave the screen and come back.
        Rect2 safeBounds = screenBounds.Grown(100);

        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].IsActive) continue;
            ref BulletData b = ref pool[i];

            if (b.Behavior == BulletBehavior.StopAndAim)
            {
                b.Timer += delta;

                // At 1.5s the bullet has braked: re-aim at the player and fire off fast.
                if (b.Timer >= 1.5f && b.Timer < 5.0f)
                {
                    Vector2 dir = Mathx.Norm(player.Transform.Position - b.Position);
                    b.Velocity = dir * 350;
                    b.Acceleration = 0;
                    b.CurveRate = 0;
                    b.Timer = 10; // lock the route
                }
            }

            if (b.Behavior == BulletBehavior.Homing && targets != null && targets.Count > 0)
            {
                Vector2 target = targets[0];
                float best = target.DistanceTo(b.Position);
                for (int t = 1; t < targets.Count; t++)
                {
                    float d = targets[t].DistanceTo(b.Position);
                    if (d < best) { best = d; target = targets[t]; }
                }
                // Turn towards the nearest foe, but only so fast: the wisp swings round rather than snapping.
                Vector2 velocity = b.Velocity;
                float wanted = MathF.Atan2(target.Y - b.Position.Y, target.X - b.Position.X);
                float turn = wanted - MathF.Atan2(velocity.Y, velocity.X);
                while (turn > MathF.PI) turn -= 2 * MathF.PI;
                while (turn < -MathF.PI) turn += 2 * MathF.PI;
                float limit = Mathx.Radians(WispTurnRate) * delta;
                b.Velocity = velocity.Rotated(MathF.Max(-limit, MathF.Min(limit, turn)));
                // And it speeds up to its cruising speed.
                float speed = MathF.Min(WispSpeed, velocity.Length() + 600 * delta);
                b.Velocity = Mathx.Norm(b.Velocity) * speed;
            }

            if (b.Behavior == BulletBehavior.Burst)
            {
                // A great stone: it flies, and after a moment it breaks into a ring of small stones, turned a little at random.
                b.Timer += delta;
                if (b.Timer >= BoulderFuse)
                {
                    float start = (float)Random.Shared.NextDouble() * 360;
                    for (int k = 0; k < BoulderFragments; k++)
                        FireBullet(pool, b.Position, start + k * 360f / BoulderFragments, BoulderFragmentSpeed, BulletStyle.Rock, 0.8f);
                    b.IsActive = false;
                    continue;
                }
            }

            if (b.Behavior == BulletBehavior.Zigzag)
            {
                // Turns by the bullet's curve (degrees) every interval, one way and then the other; the first turn comes after half an interval.
                float before = b.Timer;
                b.Timer += delta;
                if ((int)MathF.Floor((b.Timer + ZigzagInterval / 2) / ZigzagInterval) > (int)MathF.Floor((before + ZigzagInterval / 2) / ZigzagInterval))
                {
                    // The first turn is half as wide, so that the bolt zigzags evenly about the line it was fired along.
                    b.Velocity = b.Velocity.Rotated(Mathx.Radians(b.Timer < ZigzagInterval ? b.CurveRate / 2 : b.CurveRate));
                    b.CurveRate = -b.CurveRate;
                }
            }

            if (b.Behavior == BulletBehavior.Constellation)
            {
                b.Timer += delta;
                if (b.Timer < StarFlySeconds) { }                                   // flying out (braking, by its acceleration)
                else if (b.Timer < StarFlySeconds + StarHangSeconds) { b.Velocity = Vector2.Zero; b.Acceleration = 0; }   // hanging
                else
                {
                    // It lets go: it falls, and the lines are gone.
                    b.Behavior = BulletBehavior.Normal; b.Velocity = new Vector2(0, StarFallSpeed); b.Chain = 0; b.Prev = -1;
                }
            }

            if (b.Behavior == BulletBehavior.Dance)
            {
                // Sways to one side and the other on the beat: half a leg to start with, so that it sways evenly about its heading.
                b.Timer += delta;
                float wave = (int)MathF.Floor((b.Timer + DanceBeat / 2) / DanceBeat) % 2 == 0 ? 1 : -1;
                b.Velocity = b.Velocity.Rotated(Mathx.Radians(b.CurveRate * wave * delta));
            }

            if (b.Acceleration != 0)
            {
                float speed = b.Velocity.Length();
                Vector2 direction = Mathx.Norm(b.Velocity);
                speed += b.Acceleration * delta;
                // Don't let the bullet reverse forever: it stops near 0 and moves on.
                if (speed < 0) speed = 0.1f;
                b.Velocity = direction * speed;
            }

            if (b.CurveRate != 0 && b.Behavior != BulletBehavior.Dance && b.Behavior != BulletBehavior.Zigzag)
                b.Velocity = b.Velocity.Rotated(Mathx.Radians(b.CurveRate * delta));

            b.Position += (b.Velocity + wind * b.WindModifier) * delta;

            if (!safeBounds.HasPoint(b.Position))
                b.IsActive = false;
        }
    }

    /// <summary>
    /// Tells the scene what kinds of bullets were fired since the last call, once for each kind (it makes the sound of each).
    /// </summary>
    public static void ReportFresh(BulletData[] pool, List<WorldEvent> events)
    {
        Dictionary<BulletStyle, int> fired = null;
        for (int i = 0; i < pool.Length; i++)
        {
            if (!pool[i].Fresh) continue;
            pool[i].Fresh = false;
            fired ??= new Dictionary<BulletStyle, int>();
            fired[pool[i].Style] = fired.GetValueOrDefault(pool[i].Style) + 1;
        }
        if (fired == null) return;
        foreach (var pair in fired) events.Add(WorldEvent.BulletsFired(pair.Key, pair.Value));
    }

    /// <summary>What the homing wisps may turn towards: the spirits, and the boss once she is on the screen. None if no card gives wisps.</summary>
    public static List<Vector2> HomingTargets(CardEffects effects, IEnumerable<EnemyEntity> enemies, BossEntity boss, bool bossActive, BossEntity twin = null)
    {
        var targets = new List<Vector2>();
        if (effects.HomingWisps <= 0) return targets;
        foreach (var e in enemies) if (e.Health > 0) targets.Add(e.Position);
        if (bossActive && boss.Health.CurrentHealth > 0 && boss.Transform.Position.Y > 0) targets.Add(boss.Transform.Position);
        if (bossActive && twin != null && boss.Health.CurrentHealth > 0 && twin.Transform.Position.Y > 0) targets.Add(twin.Transform.Position);
        return targets;
    }

    /// <summary>Takes every bullet off the field, whoever fired it (the boss's arrival freezes the picture: nothing may be left hanging in it).</summary>
    public static void ClearAll(BulletData[] pool)
    {
        for (int i = 0; i < pool.Length; i++) pool[i].IsActive = false;
    }

    public static void ClearBossBullets(BulletData[] pool)
    {
        for (int i = 0; i < pool.Length; i++)
            if (pool[i].IsActive && pool[i].Faction == Faction.Boss)
                pool[i].IsActive = false;
    }
}
