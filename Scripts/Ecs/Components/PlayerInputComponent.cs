using Godot;

/// <summary>What the player asks for this step: a direction, shooting, bombing.</summary>
public struct PlayerInputComponent
{
    public Vector2 Direction;
    public bool IsShooting;
    public bool IsBombing;
}
