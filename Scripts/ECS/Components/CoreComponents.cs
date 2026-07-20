using Godot;
using System;

public struct Transform2DComponent { public Vector2 Position; }
public struct VelocityComponent { public Vector2 Velocity; public float Speed; }
public struct PlayerInputComponent { public Vector2 InputDirection; }
public struct RenderComponent { public Sprite2D SpriteNode; }