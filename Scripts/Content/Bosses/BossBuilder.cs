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

    /// <summary>`hitbox` is a third of the width of the sprite; the enraged attacks have `enragedHealth` (500 by default, against 300).</summary>
    public BossBuilder(string nameKey, float hitbox, BossStyle style, int health = 300, int enragedHealth = 500, MusicPlayer.Theme? theme = null, string themeTitleKey = null, float speed = 1)
    {
        _speed = speed;
        _nameKey = nameKey; _hitbox = hitbox; _style = style; _health = health; _enragedHealth = enragedHealth;
        // a boss with a song of its own plays it through the whole fight (calm and enraged); the others have the calm song and the enraged one
        _calmTheme = theme ?? MusicPlayer.Theme.Boss; _enragedTheme = theme ?? MusicPlayer.Theme.Boss;
        _calmTitleKey = themeTitleKey ?? nameof(UIText.BossMusic); _enragedTitleKey = themeTitleKey ?? nameof(UIText.BossMusic);
    }

    /// <summary>A calm attack.</summary>
    public BossPhase Calm(BossPhase attack) => Dress(attack, _calmTheme, _calmTitleKey, _health, false);

    /// <summary>An enraged attack: other music and more health.</summary>
    public BossPhase Enraged(BossPhase attack) => Dress(attack, _enragedTheme, _enragedTitleKey, _enragedHealth, true);

    /// <summary>TEMPORARY, to test the flow of the stages quickly: every boss attack has this share of its health. Put it back to 1 when the stages are done.</summary>
    public const float TestHealthFactor = 0.1f;

    private BossPhase Dress(BossPhase attack, MusicPlayer.Theme theme, string musicTitleKey, int health, bool enraged)
    {
        attack.NameKey = _nameKey; attack.Theme = theme; attack.MusicTitleKey = musicTitleKey;
        attack.SpeedFactor = _speed * attack.AttackSpeed; attack.HitboxRadius = _hitbox; attack.MaxHealth = Math.Max(10, (int)(health * TestHealthFactor)); attack.Enraged = enraged;
        return attack;
    }

    /// <summary>The boss: the attacks in the order they come, one life each.</summary>
    public BossDefinition Build(params BossPhase[] attacks) => new BossDefinition(_nameKey, attacks) { Style = _style };
}
