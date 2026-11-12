/// <summary>The bomb: how many are left, whether it is lit, for how long, and how far its light reaches.</summary>
public struct BombComponent
{
    public int BombsLeft;
    /// <summary>The bomb key was down at the last step: a bomb is lit by a new press, not by a key that is held (or all the bombs would go in one press).</summary>
    public bool WasBombing;
    public bool IsActive;
    public float Timer;
    public float Duration;
    public float Radius;
    public BombComponent(int bombsLeft, bool isActive, float timer, float duration, float radius)
    { BombsLeft = bombsLeft; IsActive = isActive; Timer = timer; Duration = duration; Radius = radius; }
}
