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

    public static void ChangePhase(BossEntity boss, int currentLives, Difficulty difficulty = Difficulty.Normal)
    {
        // No lives left: the boss is defeated.
        if (currentLives <= 0)
        {
            boss.MovementType = BossMovementType.Stationary;
            boss.Transform.Position = new Vector2(-9999, -9999);
            boss.EmitterMain.IsActive = false;
            boss.EmitterSub.IsActive = false;
            return;
        }

        var phase = boss.Definition.PhaseFor(currentLives);
        boss.Hitbox.Radius = phase.HitboxRadius;
        boss.Health.MaxHealth = phase.MaxHealth;
        boss.Health.CurrentHealth = phase.MaxHealth;
        boss.EmitterMain = phase.Main;
        boss.EmitterSub = phase.Sub;

        boss.MovementType = BossMovementType.Transition;
        boss.TransitionTimer = BreatherSeconds;
        boss.NextMovementType = phase.Movement;
        boss.TargetPosition = phase.Target;
        if (phase.Movement == BossMovementType.Dash) boss.DashTimer = 0;
        if (phase.StartsAtTarget) boss.Transform.Position = phase.Target;

        ApplyDifficulty(boss, phase, difficulty);
    }

    /// <summary>Easy: fewer bullets in every volley. Hard: more bullets, and a second, slower attack woven into some of the patterns.</summary>
    private static void ApplyDifficulty(BossEntity boss, BossPhase phase, Difficulty difficulty)
    {
        if (difficulty == Difficulty.Normal) return;
        if (boss.EmitterMain.IsActive) boss.EmitterMain.BulletsPerVolley = difficulty.Scale(boss.EmitterMain.BulletsPerVolley);
        if (boss.EmitterSub.IsActive) boss.EmitterSub.BulletsPerVolley = difficulty.Scale(boss.EmitterSub.BulletsPerVolley);
        if (difficulty == Difficulty.Hard && !boss.EmitterSub.IsActive && phase.HardExtra.IsActive) boss.EmitterSub = phase.HardExtra;
    }
}
