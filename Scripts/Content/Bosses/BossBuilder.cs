using System;

/// <summary>
/// Puts a boss together from attacks. Every boss goes the same way: its first attacks are calm and its last ones enraged (stronger, with other
/// music, and the scene adds its effects); all of them carry the boss's name, hitbox and health. A boss file lists its attacks in order.
/// </summary>
public sealed class BossBuilder
{
    private readonly string _nameKey;
    private readonly float _hitbox;
    private readonly int _health, _enragedHealth;
    private readonly BossStyle _style;
    private readonly MusicPlayer.Theme _calmTheme, _enragedTheme;
    private readonly string _calmTitleKey, _enragedTitleKey;
    /// <summary>How much faster than written the bullets of all this boss's patterns fly: each boss is a little quicker than the one before.</summary>
    private readonly float _speed;

    /// <summary>`hitbox` is a third of the width of the sprite; the enraged attacks have `enragedBonus` more health than the calm ones (0.10: a tenth more; each boss has a little more than the one before).</summary>
    public BossBuilder(string nameKey, float hitbox, BossStyle style, int health = 300, float enragedBonus = 0.10f, MusicPlayer.Theme? theme = null, string themeTitleKey = null, float speed = 1)
    {
        _speed = speed;
        _nameKey = nameKey; _hitbox = hitbox; _style = style; _health = health; _enragedHealth = (int)Math.Round(health * (1 + enragedBonus));
        // a boss with a song of its own plays it through the whole fight (calm and enraged); the others have the calm song and the enraged one
        _calmTheme = theme ?? MusicPlayer.Theme.Boss; _enragedTheme = theme ?? MusicPlayer.Theme.Boss;
        _calmTitleKey = themeTitleKey ?? nameof(UIText.BossMusic); _enragedTitleKey = themeTitleKey ?? nameof(UIText.BossMusic);
    }

    /// <summary>A calm attack.</summary>
    public BossPhase Calm(BossPhase attack) => Dress(attack, _calmTheme, _calmTitleKey, _health, false);

    /// <summary>An enraged attack: other music and a little more health.</summary>
    public BossPhase Enraged(BossPhase attack) => Dress(attack, _enragedTheme, _enragedTitleKey, _enragedHealth, true);

    /// <summary>Every boss attack has this share of the health written for it (both forms), to set the length of the fights.</summary>
    public const float HealthFactor = 0.48f;

    /// <summary>An attack in which the boss stays still (it is easier to hit) has this many times the health of one in which it moves.</summary>
    public const int StillHealthMultiplier = 2;

    /// <summary>An attack in which the boss sweeps across the field, out of reach for most of it (it rests in the middle only a few seconds), has this share of the health of one in which it moves about.</summary>
    public const float SweepHealthShare = 0.7f;

    /// <summary>How much the health written for the boss is multiplied for an attack, by how the boss moves in it.</summary>
    public static float HealthMultiplier(BossMovementType movement) => movement switch
    {
        BossMovementType.Stationary => StillHealthMultiplier,
        BossMovementType.Sweep => SweepHealthShare,
        _ => 1f,
    };

    /// <summary>The health of an attack, for the health written for the boss's form: multiplied by how the boss moves (see <see cref="HealthMultiplier"/>), then scaled by <see cref="HealthFactor"/>.</summary>
    public static int HealthOf(int written, BossMovementType movement) => Math.Max(10, (int)(written * HealthMultiplier(movement) * HealthFactor));

    /// <summary>The bases of the time bonus, one per boss (the later the boss the richer).</summary>
    public static readonly int[] AttackBases = { 7_000, 7_500, 9_000, 10_500, 11_500, 13_000 };

    /// <summary>Least and most seconds of an attack (stream weapon), and the share of the kill time of the ideal player it allows.</summary>
    public const float MinTimeLimit = 30, MaxTimeLimit = 65, TimeLimitSlack = 3;

    /// <summary>Seconds added to every limit.</summary>
    public const float ExtraTimeSeconds = 5;

    /// <summary>The time limit of an attack of this health (stream weapon): about three times what a good player needs, in steps of 5 s.</summary>
    public static float TimeLimitOf(int health, BossMovementType movement)
    {
        float kill = health / (movement == BossMovementType.Stationary ? 29f : 20f);
        return Math.Clamp(5 * MathF.Ceiling(kill * TimeLimitSlack / 5), MinTimeLimit, MaxTimeLimit) + ExtraTimeSeconds;
    }

    private int AttackBase => AttackBases[Math.Clamp((int)Math.Round((_health - 300) / 30.0), 0, AttackBases.Length - 1)];

    private BossPhase Dress(BossPhase attack, MusicPlayer.Theme theme, string musicTitleKey, int health, bool enraged)
    {
        attack.NameKey = _nameKey; attack.Theme = theme; attack.MusicTitleKey = musicTitleKey;
        attack.SpeedFactor = _speed * attack.AttackSpeed; attack.HitboxRadius = _hitbox; attack.MaxHealth = (int)(HealthOf(health, attack.Movement) * attack.HealthScale); attack.Enraged = enraged;
        attack.TimeLimit = TimeLimitOf(attack.MaxHealth, attack.Movement); attack.AttackBase = AttackBase;
        return attack;
    }

    /// <summary>The boss: the attacks in the order they come, one life each.</summary>
    public BossDefinition Build(params BossPhase[] attacks) => new BossDefinition(_nameKey, attacks) { Style = _style };
}
