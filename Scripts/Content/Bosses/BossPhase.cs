using Godot;

/// <summary>
/// One attack of a boss, and everything that goes with it: how it looks and is named while it lasts, where it moves, and the bullets it
/// fires. A boss is a list of these (see <see cref="BossDefinition"/>); the systems never know which boss they are running.
/// </summary>
public sealed class BossPhase
{
    /// <summary>Shown like the name of a Touhou spell card when the attack begins. These three are keys of <see cref="Loc"/> (a text that is
    /// not in a table is shown as it is).</summary>
    public string AttackNameKey;

    /// <summary>The boss's name, and the music, with its title, that plays for this attack.</summary>
    public string NameKey;
    public MusicPlayer.Theme Theme;
    public string MusicTitleKey;

    public string AttackName => Loc.Get(AttackNameKey);
    public string Name => Loc.Get(NameKey);
    public string MusicTitle => Loc.Get(MusicTitleKey);
    /// <summary>The health of the boss for this attack: each attack begins with the bar full. The second form of a boss is the stronger one.</summary>
    public int MaxHealth;
    /// <summary>The radius of the hitbox: about a third of the width of the sprite, as the hitbox is about two thirds of it.</summary>
    public float HitboxRadius;

    /// <summary>What she does once the breather after the last attack is over: how she moves, and where she settles.</summary>
    public BossMovementType Movement;
    public Vector2 Target;
    /// <summary>The attack begins with her already at the target (the dash enters from above the screen on its own).</summary>
    public bool StartsAtTarget;

    /// <summary>Where the second body goes and how it moves, for a boss that is two (see <see cref="BossDefinition.HasTwin"/>).</summary>
    /// <summary>In this attack the two bodies of a boss that is two are ONE: the second glides to the first and they fuse (the fused style is shown, and the twin is gone).</summary>
    public bool Fused;
    /// <summary>The two bodies take turns to move anywhere on the screen while the other stays parked at the top (see <see cref="TwinDance"/>); each goes back to its place (Target, TwinTarget) before the other leaves.</summary>
    public bool Dance;
    /// <summary>The boss never goes higher than this (its y; null: no limit), for a boss that stands on the horizon and may go to the sides and down, not above it.</summary>
    public float? MinY;
    public BossMovementType TwinMovement = BossMovementType.Stationary;
    public Vector2 TwinTarget;

    /// <summary>The bullets: the main emitter, and a second one for the attacks that weave two patterns together.</summary>
    public BulletEmitterComponent Main;
    public BulletEmitterComponent Sub;
    /// <summary>On hard difficulty, a second pattern woven into an attack that has no <see cref="Sub"/>; unused if not active.</summary>
    public BulletEmitterComponent HardExtra;

    /// <summary>What the attack does besides firing bullets: slashes of claws, a quake of the screen, a roar that pushes the boat (see <see cref="BossSpecials"/>).</summary>
    public SlashSpec Slashes;
    public QuakeSpec Quake;
    public RoarSpec Roar;

    /// <summary>The attack is a lunge that only drops embers in its wake: easy has none, normal has what easy would have, hard what normal would.</summary>
    public bool EmbersOneStepDown;

    /// <summary>The boss is enraged in this attack (the last ones of a boss): its music changes, it gets extra effects and the scenery turns redder.</summary>
    public bool Enraged;
}
