using System.Collections.Generic;
using Godot;

/// <summary>The thin lines between the stars of a constellation: dim while the stars fly out, bright when they hang still (and the lines can hurt).</summary>
public partial class ConstellationLines : Node2D
{
    private readonly List<(Vector2 a, Vector2 b, bool still)> _lines = new();
    private float _clock;

    public void Update(float dt, IEnumerable<(Vector2 a, Vector2 b, bool still)> links)
    {
        _clock += dt;
        _lines.Clear();
        foreach (var (a, b, still) in links) _lines.Add((ViewKit.Point(a), ViewKit.Point(b), still));
        QueueRedraw();
    }

    public override void _Draw()
    {
        float twinkle = 0.75f + 0.25f * Mathf.Sin(_clock * 9);
        foreach (var (a, b, still) in _lines)
        {
            if (still)
            {
                DrawLine(a, b, new Color(0.7f, 0.85f, 1f, 0.35f * twinkle), 6);                 // a soft glow round the line
                DrawLine(a, b, new Color(0.95f, 0.98f, 1f, 0.95f * twinkle), 2);
            }
            else DrawLine(a, b, new Color(0.8f, 0.9f, 1f, 0.3f), 1);
        }
    }
}
