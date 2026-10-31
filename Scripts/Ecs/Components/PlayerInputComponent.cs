using Godot;

/// <summary>What the player asks for this step: a direction, shooting, bombing, and whether to focus (slower, a narrower shot, the hitbox shown).</summary>
public struct PlayerInputComponent
{
    public Vector2 Direction;
    public bool IsShooting;
    public bool IsBombing;
    public bool IsFocused;
}
