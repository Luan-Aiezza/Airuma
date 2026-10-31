using Godot;

/// <summary>
/// The library of boss attacks, each as a prototype of a <see cref="BossPhase"/> (the name, how the boss moves, the bullets). A boss puts the ones it
/// wants in order and lets <see cref="BossBuilder"/> dress them (its name, hitbox, health, music). The library is split by theme into files, one for
/// the attacks of each stage's boss: <c>Attacks.Embers.cs</c> (the Iryssh, and the spare ones kept for later bosses), <c>Attacks.Party.cs</c> (Baylo).
/// A new boss adds a file; an attack that a boss reuses from another should change something about it (see the dancing bullets of stage 2).
/// </summary>
public static partial class Attacks
{
    private static readonly Vector2 Centre = new Vector2(300, Layout.BossY);

    private static BossPhase Attack(string nameKey, BossMovementType movement, Vector2 target, BulletEmitterComponent main, BulletEmitterComponent sub = default,
                                    BulletEmitterComponent hardExtra = default, bool startsAtTarget = false, bool embersOneStepDown = false) =>
        new BossPhase { AttackNameKey = nameKey, Movement = movement, Target = target, StartsAtTarget = startsAtTarget, Main = main, Sub = sub, HardExtra = hardExtra, EmbersOneStepDown = embersOneStepDown };
}
