/// <summary>The bomb: how many are left, whether it is lit, for how long, and how far its light reaches.</summary>
public struct BombComponent
{
    public int BombsLeft;
    public bool IsActive;
    public float Timer;
    public float Duration;
    public float Radius;
    public BombComponent(int bombsLeft, bool isActive, float timer, float duration, float radius)
    { BombsLeft = bombsLeft; IsActive = isActive; Timer = timer; Duration = duration; Radius = radius; }
}
