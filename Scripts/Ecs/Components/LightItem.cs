using System.Collections.Generic;
using System.Linq;
using Godot;

public enum LightKind { Energy, Bomb, Life }

/// <summary>A little light that falls from a defeated spirit (or from the player, who dropped it), or, when the score reaches a certain height, a spare lantern (a bomb) or a spare boat (a life).</summary>
public struct LightItem
{
    public LightKind Kind;
    public bool IsActive;
    public Vector2 Position;
    public Vector2 Velocity;
    /// <summary>Energy it gives.</summary>
    public float Value;
    public float Age;
    public LightItem() { Kind = LightKind.Energy; IsActive = false; Position = Vector2.Zero; Velocity = Vector2.Zero; Value = 1; Age = 0; }
}
