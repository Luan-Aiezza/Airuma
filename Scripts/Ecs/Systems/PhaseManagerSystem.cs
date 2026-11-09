using System;
using Godot;

/// <summary>Starts the attack a boss has lives left for: reads it from the boss's definition and puts it on the boss.</summary>
public static class PhaseManagerSystem
{
    /// <summary>Seconds of breathing room between attacks, during which the boss glides to where the next one begins.</summary>
    public const float BreatherSeconds = 2.0f;

    /// <summary>Seconds the boss takes to come down from above the screen before she starts to attack.</summary>
    public const float EntranceSeconds = 3.4f;

    /// <summary>The boss glides in slowly from above the screen; the first attack starts only when she has arrived.</summary>
    public static void BeginEntrance(BossEntity boss)
    {
        if (boss.MovementType != BossMovementType.Transition) return;
        boss.Transform.Position = new Vector2(boss.TargetPosition.X, -170);
        boss.TransitionTimer = EntranceSeconds;
        boss.TransitionSpeed = (boss.TargetPosition.Y + 170) / (EntranceSeconds - 0.4f);
    }

    /// <summary>The second body of a boss that is two: it glides to its place for the attack that the first one begins (it has no attacks of its own: the first one fires for both).</summary>
    public static void ChangeTwinPhase(BossEntity twin, BossEntity boss)
    {
        if (boss.Health.Lives <= 0)
        {
            twin.MovementType = BossMovementType.Stationary;
            twin.Transform.Position = new Vector2(-9999, -9999);
            return;
        }
        var phase = boss.Definition.PhaseFor(boss.Health.Lives);
        twin.Hitbox.Radius = phase.HitboxRadius;
        // once fused they stay one for the rest of the fight: no un-fusing in the breather between attacks
        if (twin.Merged && phase.Fused) { twin.MovementType = BossMovementType.Stationary; return; }
        twin.MovementType = BossMovementType.Transition;
        twin.TransitionTimer = BreatherSeconds;
        twin.TransitionSpeed = 0;
        twin.Merged = false;
        // in an attack where they are one, the second glides to the first (where it will be) and fuses with it
        twin.NextMovementType = phase.Fused ? BossMovementType.Stationary : phase.TwinMovement;
        twin.TargetPosition = phase.Fused ? phase.Target : phase.TwinTarget;
        twin.Patrol.Reset(); twin.Roam.Reset(); twin.Dive.Reset(); twin.Dash.Reset(twin.TargetPosition.X); twin.Cross.Reset();
        twin.Sweep.Reset(Difficulty.Easy);
    }

    public static void ChangePhase(BossEntity boss, int currentLives, Difficulty difficulty = Difficulty.Easy)
    {
        // No lives left: the boss is defeated.
        if (currentLives <= 0)
        {
            boss.MovementType = BossMovementType.Stationary;
            boss.Transform.Position = new Vector2(-9999, -9999);
            boss.EmitterMain.IsActive = false;
            boss.EmitterSub.IsActive = false;
            boss.Patterns = System.Array.Empty<PatternRun>();
            return;
        }

        var phase = boss.Definition.PhaseFor(currentLives);
        boss.Hitbox.Radius = phase.HitboxRadius;
        boss.Health.MaxHealth = phase.MaxHealth;
        boss.Health.CurrentHealth = phase.MaxHealth;
        boss.EmitterMain = phase.Main;
        boss.EmitterSub = phase.Sub;
        boss.Patterns = BuildPatterns(phase, difficulty);
        boss.PatternPressure = phase.Pressure; boss.PatternSpeed = phase.SpeedFactor;
        boss.Specials = SpecialsSystem.Begin(phase, difficulty);

        boss.MovementType = BossMovementType.Transition;
        boss.TransitionTimer = BreatherSeconds;
        boss.NextMovementType = phase.Movement;
        boss.TargetPosition = phase.Target;
        boss.Patrol.Reset();
        boss.Roam.Reset();
        boss.Dive.Reset();
        boss.Sweep.Reset(difficulty);
        boss.Dash.Reset(phase.Target.X);
        boss.Cross.Reset();
        if (phase.StartsAtTarget) boss.Transform.Position = phase.Target;

        ApplyDifficulty(boss, phase, difficulty);
    }

    /// <summary>The patterns that apply on this difficulty, each with its clock at the start.</summary>
    private static PatternRun[] BuildPatterns(BossPhase phase, Difficulty difficulty)
    {
        var runs = new System.Collections.Generic.List<PatternRun>();
        foreach (var spec in phase.Patterns)
            if (spec.Applies(difficulty)) runs.Add(new PatternRun { Spec = spec, NextAt = 0 });
        return runs.ToArray();
    }

    /// <summary>Each level has more bullets than the one before, and from normal on a second, slower attack is woven into some of the patterns.</summary>
    private static void ApplyDifficulty(BossEntity boss, BossPhase phase, Difficulty difficulty)
    {
        // The embers come one step down the difficulties: easy's are fewer than the pattern's, normal's are the pattern's, hard's are as many as normal's others.
        float embers = phase.EmbersOneStepDown ? difficulty.Pick(0.6f, 1f, 1.35f) : difficulty.BulletFactor();
        float gap = phase.EmbersOneStepDown ? difficulty.Pick(1.35f, 1f, 0.85f) : difficulty.Pick(1f, 0.85f, 0.75f);
        ScaleEmitter(ref boss.EmitterMain, embers, gap);
        ScaleEmitter(ref boss.EmitterSub, embers, gap);
        if (difficulty != Difficulty.Easy && !boss.EmitterSub.IsActive && phase.HardExtra.IsActive) boss.EmitterSub = phase.HardExtra;
    }

    /// <summary>The bullets of a volley times `bulletFactor` and the gap of a curtain times `gapFactor`. A constellation keeps its shape.</summary>
    private static void ScaleEmitter(ref BulletEmitterComponent emitter, float bulletFactor, float gapFactor)
    {
        if (!emitter.IsActive) return;
        if (emitter.Curtain) emitter.CurtainGap *= gapFactor;
        else if (!emitter.Link) emitter.BulletsPerVolley = Math.Max(1, (int)MathF.Round(emitter.BulletsPerVolley * bulletFactor, MidpointRounding.AwayFromZero));
    }
}
