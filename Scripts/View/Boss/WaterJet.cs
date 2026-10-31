using System;
using Godot;

/// <summary>
/// The great jet of water that the whale fires down the field: first a thin pink warning line from its mouth that pulses, then the jet itself,
/// a column of pale blue water with a bright core, streaks of foam running down it, spray at its edges and a burst where it lands.
/// Drawn in blocks of four screen pixels, like the rest of the art. Call <see cref="Update"/> every frame.
/// </summary>
public partial class WaterJet : Node2D
{
    private bool _warn, _on;
    private Vector2 _from;
    private float _bottom, _halfWidth, _clock;

    public void Update(float dt, bool warn, bool on, Vector2 from, float bottom, float halfWidth)
    {
        _clock += dt;
        _warn = warn; _on = on; _from = from; _bottom = bottom; _halfWidth = halfWidth;
        Visible = warn || on;
        if (Visible) QueueRedraw();
    }

    private static float Snap(float v) => MathF.Round(v / 4) * 4;

    public override void _Draw()
    {
        float x = Snap(_from.X), top = Snap(_from.Y), length = _bottom - top;
        if (length <= 0) return;
        if (_warn && !_on)
        {
            // A dashed warning line, and a little glow that swells at the mouth.
            float pulse = 0.5f + 0.5f * MathF.Sin(_clock * 16);
            for (float y = top; y < _bottom; y += 24)
                DrawRect(new Rect2(x - 2, y, 4, 12), new Color(1, 0.72f, 0.88f, 0.35f + 0.4f * pulse));
            DrawCircle(new Vector2(x, top), 10 + 6 * pulse, new Color(1, 0.8f, 0.92f, 0.4f));
            return;
        }
        float w = _halfWidth;
        DrawRect(new Rect2(x - w, top, 2 * w, length), new Color(0.55f, 0.82f, 1f, 0.5f));
        DrawRect(new Rect2(x - w * 0.6f, top, w * 1.2f, length), new Color(0.82f, 0.95f, 1f, 0.75f));
        DrawRect(new Rect2(x - w * 0.25f, top, w * 0.5f, length), new Color(1, 1, 1, 0.95f));
        // Streaks of foam running down the jet.
        for (int k = 0; k < 14; k++)
        {
            float lane = ((k * 37) % 11 - 5) / 5f * (w - 6);
            float y = top + ((_clock * (520 + k * 23) + k * 83) % (length + 40)) - 20;
            DrawRect(new Rect2(Snap(x + lane) - 2, Snap(y), 4, 16 + (k % 3) * 8), new Color(1, 1, 1, 0.8f));
        }
        // Spray at the edges.
        for (int k = 0; k < 18; k++)
        {
            int side = k % 2 == 0 ? -1 : 1;
            float y = top + ((_clock * 300 + k * 71) % length);
            float out_ = w + 4 + ((k * 13) % 9) * 2;
            DrawRect(new Rect2(Snap(x + side * out_), Snap(y), 4, 4), new Color(0.85f, 0.95f, 1f, 0.7f));
        }
        // The mouth, and where the jet lands.
        DrawCircle(new Vector2(x, top), w * 0.8f, new Color(1, 1, 1, 0.55f));
        for (int k = 0; k < 10; k++)
        {
            float a = _clock * 6 + k * MathF.Tau / 10;
            DrawRect(new Rect2(Snap(x + MathF.Cos(a) * (w + 10)), Snap(_bottom - 6 - MathF.Abs(MathF.Sin(a * 1.7f)) * 26), 4, 4), new Color(1, 1, 1, 0.8f));
        }
    }
}
