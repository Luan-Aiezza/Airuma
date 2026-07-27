using Godot;
using System;

public struct Transform2DComponent {
    public Vector2 Position;
    public Vector2 Scale; // NOVO: Para deixar o Boitatá gigante!
}
public struct VelocityComponent { public Vector2 Velocity; public float Speed; }
public struct PlayerInputComponent {
    public Vector2 Direction;
    public bool IsShooting;
    public bool IsBombing; // NOVO: Detecta se apertou X
}
public struct RenderComponent { public Sprite2D SpriteNode; }
public struct WindModifierComponent { public float Value; } // 1.0f = afeta 100%, 0.0f = imune