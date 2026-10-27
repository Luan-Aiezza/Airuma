using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Small rasterisers that paint shapes straight into a <see cref="PixelCanvas"/> (no anti-aliasing: hard pixel edges), with
/// alpha blending, for the larger painterly pixel-art pieces such as the dialogue portraits.
/// </summary>
public static class PixelPaint
{
    public static void Plot(PixelCanvas c, int x, int y, PixelColor color)
    {
        if (!c.Contains(x, y) || color.A == 0) return;
        if (color.A == 255) { c[x, y] = color; return; }
        var dst = c[x, y];
        float a = color.A / 255f;
        float outA = a + dst.A / 255f * (1 - a);
        if (outA <= 0) return;
        byte Mix(byte s, byte d) => (byte)Math.Clamp((s * a + d * (dst.A / 255f) * (1 - a)) / outA, 0, 255);
        c[x, y] = new PixelColor(Mix(color.R, dst.R), Mix(color.G, dst.G), Mix(color.B, dst.B), (byte)Math.Clamp(outA * 255, 0, 255));
    }

    public static void Rect(PixelCanvas c, int x0, int y0, int x1, int y1, PixelColor color)
    {
        for (int y = y0; y <= y1; y++) for (int x = x0; x <= x1; x++) Plot(c, x, y, color);
    }

    /// <summary>Scanline fill (even-odd) of a polygon whose points are in canvas pixels.</summary>
    public static void Poly(PixelCanvas c, IList<Vector2> pts, PixelColor color)
    {
        if (pts.Count < 3) return;
        float minY = float.MaxValue, maxY = float.MinValue;
        foreach (var p in pts) { minY = Math.Min(minY, p.Y); maxY = Math.Max(maxY, p.Y); }
        for (int y = (int)Math.Floor(minY); y <= (int)Math.Ceiling(maxY); y++)
        {
            float sy = y + 0.5f;
            var xs = new List<float>();
            for (int i = 0; i < pts.Count; i++)
            {
                var a = pts[i]; var b = pts[(i + 1) % pts.Count];
                if ((a.Y <= sy && b.Y > sy) || (b.Y <= sy && a.Y > sy))
                    xs.Add(a.X + (sy - a.Y) / (b.Y - a.Y) * (b.X - a.X));
            }
            xs.Sort();
            for (int i = 0; i + 1 < xs.Count; i += 2)
                for (int x = (int)Math.Round(xs[i]); x < (int)Math.Round(xs[i + 1]); x++) Plot(c, x, y, color);
        }
    }

    public static void Ellipse(PixelCanvas c, float cx, float cy, float rx, float ry, PixelColor color)
    {
        for (int y = (int)Math.Floor(cy - ry); y <= (int)Math.Ceiling(cy + ry); y++)
            for (int x = (int)Math.Floor(cx - rx); x <= (int)Math.Ceiling(cx + rx); x++)
            {
                float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry;
                if (dx * dx + dy * dy <= 1) Plot(c, x, y, color);
            }
    }

    /// <summary>Dark outline round everything solid in <paramref name="layer"/>, drawn into the same layer on empty pixels.</summary>
    public static void Outline(PixelCanvas layer, PixelColor color)
    {
        var copy = layer.Clone();
        (int, int)[] n = { (1, 0), (-1, 0), (0, 1), (0, -1) };
        for (int y = 0; y < layer.Height; y++)
            for (int x = 0; x < layer.Width; x++)
            {
                if (copy[x, y].A > 40) continue;
                foreach (var (dx, dy) in n)
                    if (copy.Contains(x + dx, y + dy) && copy[x + dx, y + dy].A > 200) { layer[x, y] = color; break; }
            }
    }

    public static void Composite(PixelCanvas dst, PixelCanvas src)
    {
        for (int y = 0; y < src.Height; y++) for (int x = 0; x < src.Width; x++) if (src[x, y].A > 0) Plot(dst, x, y, src[x, y]);
    }
}
