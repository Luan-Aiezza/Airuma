/// <summary>The boat's weapon: its level, how often it fires and how fast its bolts fly.</summary>
public struct PlayerWeaponComponent
{
    public int PowerLevel;
    public float FireRate;
    public float TimeSinceLastFire;
    public float BulletSpeed;
    /// <summary>Counts up to the next volley of homing wisps (a card).</summary>
    public float TimeSinceLastWisp;
    public PlayerWeaponComponent() { PowerLevel = 1; FireRate = 0.1f; TimeSinceLastFire = 0; BulletSpeed = 800; TimeSinceLastWisp = 0; }
}
