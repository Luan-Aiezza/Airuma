using System;
using Godot;

/// <summary>
/// The great jet of water that the whale fires down the field: first a warning (a cone of pink dashes that narrows onto the jet's column, marks on the water and a glow at the mouth), then the jet itself:
/// a column of water with wavy edges, a bright core, bands of light that run down it, streaks of foam, drops thrown out of its edges in arcs, ripples on the water along its length, and, where it lands,
/// a burst with rings that spread and drops that fall back. Drawn in blocks of four screen pixels, like the rest of the art. Call <see cref="Update"/> every frame.
/// </summary>
public partial class WaterJet : Node2D
{
    private bool _warn, _on;
    private Vector2 _from;
    private float _bottom, _halfWidth, _clock, _onTime;

    public void Update(float dt, bool warn, bool on, Vector2 from, float bottom, float halfWidth)
    {
        _clock += dt;
        _onTime = on ? _onTime + dt : 0;
        _warn = warn; _on = on; _from = from; _bottom = bottom; _halfWidth = halfWidth;
        Visible = warn || on;
        if (Visible) QueueRedraw();
    }

    private static float Snap(float v) => MathF.Round(v / 4) * 4;

    /// <summary>A number between 0 and 1 that is always the same for the same `seed` (a stable scatter for the drops and the foam).</summary>
    private static float Hash(int seed) { unchecked { uint h = (uint)seed * 2654435761u; h ^= h >> 15; h *= 2246822519u; h ^= h >> 13; return (h & 0xFFFF) / 65535f; } }

    public override void _Draw()
    {
        float x = Snap(_from.X), top = Snap(_from.Y), length = _bottom - top;
        if (length <= 0) return;
        float w = _halfWidth;
        if (_warn && !_on) { DrawWarning(x, top, length, w); return; }
        DrawJet(x, top, length, w);
    }

    private void DrawWarning(float x, float top, float length, float w)
    {
        float pulse = 0.5f + 0.5f * MathF.Sin(_clock * 16);
        // the column that will be hit: its two edges as dashes that run down, and the dashes of the middle
        for (float y = top; y < _bottom; y += 24)
        {
            float run = (y + _clock * 120) % 24;
            DrawRect(new Rect2(x - 2, y, 4, 12), new Color(1, 0.72f, 0.88f, 0.35f + 0.4f * pulse));
            float spread = w * MathF.Min(1, (y - top) / 220f);
            DrawRect(new Rect2(Snap(x - spread - 2), y + run * 0.0f, 4, 10), new Color(1, 0.45f, 0.6f, 0.25f + 0.4f * pulse));
            DrawRect(new Rect2(Snap(x + spread - 2), y, 4, 10), new Color(1, 0.45f, 0.6f, 0.25f + 0.4f * pulse));
        }
        // the water gathers at the mouth: a glow that swells, with drops drawn into it
        DrawCircle(new Vector2(x, top), 14 + 8 * pulse, new Color(1, 0.8f, 0.92f, 0.35f));
        DrawCircle(new Vector2(x, top), 7 + 4 * pulse, new Color(1, 1, 1, 0.6f));
        for (int k = 0; k < 12; k++)
        {
            float a = k * MathF.Tau / 12 + _clock * 3;
            float r = 46 - ((_clock * 60 + k * 11) % 40);
            DrawRect(new Rect2(Snap(x + MathF.Cos(a) * r), Snap(top + MathF.Sin(a) * r * 0.6f), 4, 4), new Color(1, 0.9f, 0.96f, 0.7f));
        }
        // where it will land: marks on the water, brighter as it comes
        for (int k = 0; k < 3; k++) DrawArc(new Vector2(x, _bottom - 14), w * (0.7f + 0.25f * k) , 0, MathF.Tau, 24, new Color(1, 0.6f, 0.75f, 0.2f + 0.25f * pulse), 2);
    }

    private void DrawJet(float x, float top, float length, float w)
    {
        float grow = MathF.Min(1, _onTime / 0.18f);          // the jet shoots out: its width opens in a moment
        float ww = w * grow;

        // a faint glow round the whole jet
        DrawRect(new Rect2(x - ww * 1.55f, top, ww * 3.1f, length), new Color(0.5f, 0.8f, 1f, 0.12f));
        DrawRect(new Rect2(x - ww * 1.25f, top, ww * 2.5f, length), new Color(0.55f, 0.85f, 1f, 0.16f));

        // the body, as a column with wavy edges
        const int step = 16;
        int rows = (int)MathF.Ceiling(length / step);
        var left = new Vector2[rows + 1]; var right = new Vector2[rows + 1];
        for (int i = 0; i <= rows; i++)
        {
            float y = top + i * step;
            float wave = MathF.Sin(y * 0.045f - _clock * 11) * 5 + MathF.Sin(y * 0.11f + _clock * 17) * 2.5f;
            float widen = 1 + 0.06f * MathF.Sin(y * 0.02f - _clock * 5);
            left[i] = new Vector2(x - ww * widen + wave, y); right[i] = new Vector2(x + ww * widen + wave, y);
        }
        for (int i = 0; i < rows; i++)
        {
            DrawColoredPolygon(new[] { left[i], right[i], right[i + 1], left[i + 1] }, new Color(0.55f, 0.82f, 1f, 0.55f));
            var l2 = left[i].Lerp(right[i], 0.2f); var r2 = left[i].Lerp(right[i], 0.8f); var l3 = left[i + 1].Lerp(right[i + 1], 0.2f); var r3 = left[i + 1].Lerp(right[i + 1], 0.8f);
            DrawColoredPolygon(new[] { l2, r2, r3, l3 }, new Color(0.82f, 0.95f, 1f, 0.75f));
            var l4 = left[i].Lerp(right[i], 0.38f); var r4 = left[i].Lerp(right[i], 0.62f); var l5 = left[i + 1].Lerp(right[i + 1], 0.38f); var r5 = left[i + 1].Lerp(right[i + 1], 0.62f);
            DrawColoredPolygon(new[] { l4, r4, r5, l5 }, new Color(1, 1, 1, 0.95f));
        }
        // the bright edges
        for (int i = 0; i < rows; i++)
        {
            DrawLine(left[i], left[i + 1], new Color(0.9f, 0.98f, 1f, 0.9f), 3);
            DrawLine(right[i], right[i + 1], new Color(0.9f, 0.98f, 1f, 0.9f), 3);
        }

        // bands of light that run down the jet, one after the other
        for (int k = 0; k < 6; k++)
        {
            float y = top + ((_clock * 640 + k * length / 6) % length);
            float half = ww * (0.95f - 0.1f * ((k * 7) % 3));
            DrawRect(new Rect2(Snap(x - half), Snap(y), half * 2, 8), new Color(1, 1, 1, 0.4f));
        }

        // streaks of foam running down the jet
        for (int k = 0; k < 22; k++)
        {
            float lane = (Hash(k * 3 + 1) * 2 - 1) * (ww - 8);
            float speed = 520 + Hash(k * 5 + 2) * 340;
            float y = top + ((_clock * speed + Hash(k * 7 + 3) * length) % (length + 60)) - 30;
            DrawRect(new Rect2(Snap(x + lane) - 2, Snap(y), 4, 16 + (k % 3) * 10), new Color(1, 1, 1, 0.8f));
        }

        // drops thrown out of the edges in arcs, and falling back
        for (int k = 0; k < 40; k++)
        {
            int side = k % 2 == 0 ? -1 : 1;
            float life = (_clock * (0.9f + Hash(k * 11) * 0.8f) + Hash(k * 13)) % 1f;
            float y0 = top + Hash(k * 17 + 5) * length;
            float outward = life * (30 + Hash(k * 19) * 46);
            float rise = -MathF.Sin(life * MathF.PI) * 18 + life * life * 34;
            DrawRect(new Rect2(Snap(x + side * (ww + 4 + outward)), Snap(y0 + rise), 4, 4), new Color(0.9f, 0.97f, 1f, (1 - life) * 0.9f));
        }

        // ripples on the water along the jet: flat rings that open and fade, one after the other
        for (int k = 0; k < 7; k++)
        {
            float life = (_clock * 0.9f + k / 7f) % 1f;
            float y = top + 90 + k * ((length - 120) / 7f);
            DrawArc(new Vector2(x, y), ww * (0.9f + 0.9f * life), 0, MathF.Tau, 28, new Color(0.85f, 0.97f, 1f, (1 - life) * 0.28f), 2);
        }

        // the mouth: a burst with rays
        DrawCircle(new Vector2(x, top), ww * 0.95f, new Color(1, 1, 1, 0.6f));
        DrawCircle(new Vector2(x, top), ww * 0.6f, new Color(0.8f, 0.95f, 1f, 0.7f));
        for (int k = 0; k < 14; k++)
        {
            float a = k * MathF.Tau / 14 + _clock * 2;
            float r = ww * (1.05f + 0.35f * MathF.Sin(_clock * 18 + k * 2));
            DrawLine(new Vector2(x, top), new Vector2(x + MathF.Cos(a) * r, top + MathF.Sin(a) * r * 0.7f), new Color(1, 1, 1, 0.55f), 3);
        }

        // where it lands: rings that spread over the water, a crown of foam and drops that fall back
        for (int k = 0; k < 3; k++)
        {
            float life = (_clock * 1.3f + k / 3f) % 1f;
            DrawArc(new Vector2(x, _bottom - 10), ww * (0.8f + 1.3f * life), 0, MathF.Tau, 32, new Color(1, 1, 1, (1 - life) * 0.55f), 3);
        }
        DrawRect(new Rect2(x - ww * 1.1f, _bottom - 12, ww * 2.2f, 12), new Color(1, 1, 1, 0.65f));
        for (int k = 0; k < 26; k++)
        {
            float life = (_clock * 1.6f + Hash(k * 23)) % 1f;
            float a = (Hash(k * 29) * 2 - 1) * 1.15f;
            float px = x + MathF.Sin(a) * (ww + 8 + life * 60);
            float py = _bottom - 8 - MathF.Sin(life * MathF.PI) * (28 + Hash(k * 31) * 50);
            DrawRect(new Rect2(Snap(px), Snap(py), 4, 4), new Color(1, 1, 1, 0.85f * (1 - life * 0.6f)));
        }
    }
}
