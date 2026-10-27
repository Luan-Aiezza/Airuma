using Godot;

/// <summary>
/// One attack of a boss, and everything that goes with it: how it looks and is named while it lasts, where it moves, and the bullets it
/// fires. A boss is a list of these (see <see cref="BossDefinition"/>); the systems never know which boss they are running.
/// </summary>
public sealed class BossPhase
{
    /// <summary>Shown like the name of a Touhou spell card when the attack begins.</summary>
    public string AttackName;

    /// <summary>The shape the boss has in this attack (the art and the size of the hitbox depend on it).</summary>
    public BossForm Form;
    /// <summary>The boss's name while she has this shape, and the music, with its title, that plays for it.</summary>
    public string Name;
    public MusicPlayer.Theme Theme;
    public string MusicTitle;
    /// <summary>The health of the boss for this attack: each attack begins with the bar full. The second form of a boss is the stronger one.</summary>
    public int MaxHealth;
    /// <summary>The radius of the hitbox: about a third of the width of the sprite, as the hitbox is about two thirds of it.</summary>
    public float HitboxRadius;

    /// <summary>What she does once the breather after the last attack is over: how she moves, and where she settles.</summary>
    public BossMovementType Movement;
    public Vector2 Target;
    /// <summary>The attack begins with her already at the target (the dash enters from above the screen on its own).</summary>
    public bool StartsAtTarget;

    /// <summary>The bullets: the main emitter, and a second one for the attacks that weave two patterns together.</summary>
    public BulletEmitterComponent Main;
    public BulletEmitterComponent Sub;
    /// <summary>On hard difficulty, a second pattern woven into an attack that has no <see cref="Sub"/>; unused if not active.</summary>
    public BulletEmitterComponent HardExtra;

    /// <summary>When she takes this shape from another one: what she says before the change (null if she says nothing).</summary>
    public System.Collections.Generic.IReadOnlyList<DialogueLine> TransformationDialogue;
}
