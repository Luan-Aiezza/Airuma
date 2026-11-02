using Godot;

/// <summary>A boss's source of bullets: how often it fires, how many, in which directions and with what behaviour.</summary>
public struct BulletEmitterComponent
{
    public bool IsActive;
    public float FireRate;
    public float TimeSinceLastFire;
    public int BulletsPerVolley;
    public float SpreadAngle;
    public float BaseAngle;
    public float BulletSpeed;
    public float SpinRate;
    public bool IsAimed;
    /// <summary>Every other bullet of a volley curves the opposite way (for the dancing bullets: neighbours sway against each other).</summary>
    public bool AlternateCurve;

    // A constellation: the bullets of a volley are stars linked by thin lines, each to the one `LinkSkip` places before it (1: a polygon; 2: a pentagram).
    public bool Link;
    public int LinkSkip;

    // A curtain: instead of a volley from the boss, a row of bullets across the field that falls, with a gap that swings from side to side
    // (the boat has to follow it, in small moves). `CurtainSlots` bullets across; the gap is `CurtainGap` px wide, swings `CurtainSwing` px either
    // side of `CurtainCentre` and takes `CurtainPeriod` seconds to go and come back; `CurtainMirror` makes a second gap, mirrored.
    public bool Curtain;
    public int CurtainSlots;
    public float CurtainGap, CurtainSwing, CurtainCentre, CurtainPeriod, CurtainClock;
    public bool CurtainMirror;

    /// <summary>Instead of a volley from the boss, the bullets come in from the top of the field at random places (the ceiling falling).</summary>
    public bool Ceiling;
    /// <summary>The emitter only fires while the boss's quake is on (see <see cref="QuakeSpec"/>).</summary>
    public bool QuakeOnly;
    /// <summary>The bullets of a volley take the seven colours of the rainbow in turn (the style is then only a default).</summary>
    public bool Rainbow;
    /// <summary>The bullets come from the second body of the boss (the twin) instead of the first.</summary>
    public bool FromTwin;

    public float Acceleration;
    public float CurveRate;
    public float BulletScale;
    public Vector2 SpawnOffset;

    public BulletBehavior BulletBehavior;
    public BulletStyle BulletStyle;

    public BulletEmitterComponent()
    {
        IsActive = false; FireRate = 0; TimeSinceLastFire = 0; BulletsPerVolley = 0; SpreadAngle = 0; BaseAngle = 0;
        BulletSpeed = 0; SpinRate = 0; IsAimed = false; Acceleration = 0; CurveRate = 0; BulletScale = 1;
        SpawnOffset = Vector2.Zero; BulletBehavior = BulletBehavior.Normal; BulletStyle = BulletStyle.Fireball;
    }
}
