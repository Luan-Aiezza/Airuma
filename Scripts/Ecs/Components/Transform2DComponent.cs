using Godot;

// Plain data of the simulation. Structs with non-zero defaults declare a parameterless constructor, so always create them
// with `new T()` (arrays of structs need to be filled explicitly, see BulletSystem.NewPool).

/// <summary>Where something is and how it is scaled.</summary>
public struct Transform2DComponent
{
    public Vector2 Position;
    public Vector2 Scale;
    public Transform2DComponent() { Position = Vector2.Zero; Scale = new Vector2(1, 1); }
    public Transform2DComponent(Vector2 position, Vector2 scale) { Position = position; Scale = scale; }
}
