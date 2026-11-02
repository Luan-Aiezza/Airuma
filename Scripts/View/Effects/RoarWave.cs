using System;
using Godot;

/// <summary>The roar of the boss, as a wave: rings that spread from her mouth, one after another, thin and pale, and fade. It frees itself when it is over.</summary>
public partial class RoarWave : Node2D
{
    private float _age;
    private const float Life = 1.0f;

    public override void _Process(double delta)
    {
        _age += (float)delta;
        if (_age >= Life) { QueueFree(); return; }
        QueueRedraw();
    }

    public override void _Draw()
    {
        for (int ring = 0; ring < 3; ring++)
        {
            float t = (_age - ring * 0.12f) / (Life - 0.3f);
            if (t <= 0 || t >= 1) continue;
            float radius = 30 + 520 * (1 - (1 - t) * (1 - t));
            DrawArc(Vector2.Zero, radius, 0, MathF.Tau, 72, new Color(1f, 0.92f, 0.78f, 0.55f * (1 - t)), 5 * (1 - t) + 1.5f);
        }
    }
}
