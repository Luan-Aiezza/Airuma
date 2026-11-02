using System;
using System.Collections.Generic;
using Godot;
using P = PixelColor;

/// <summary>Shapes for the painterly pixel art of the characters: polygons, ellipses and limbs filled in three tones, lit from the left, and the way parts
/// are put together, each with its own dark outline.</summary>
public static class PixelShapes
{
    public static List<Vector2> Pts(params (float x, float y)[] pts)
    {
        var list = new List<Vector2>();
        foreach (var (x, y) in pts) list.Add(new Vector2(x, y));
        return list;
    }

    /// <summary>Puts every solid pixel of `src` on `dst`.</summary>
    public static void Blit(PixelCanvas dst, PixelCanvas src)
    {
        for (int y = 0; y < src.Height; y++) for (int x = 0; x < src.Width; x++) if (src[x, y].A > 0) dst[x, y] = src[x, y];
    }

    /// <summary>Shades what `layer` holds like a lit shape: lit on the left third, shaded on the right, dithered between (`minX`...`maxX` is its width).</summary>
    private static void Shade(PixelCanvas c, PixelCanvas layer, P[] ramp, float minX, float maxX)
    {
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                if (layer[x, y].A == 0) continue;
                float t = (x + 0.5f - minX) / Math.Max(1, maxX - minX);
                float f = t * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.9f;
                c[x, y] = f < 0.55f ? ramp[0] : (f < 1.9f ? ramp[1] : ramp[2]);
            }
    }

    /// <summary>A polygon filled in the three tones of `ramp` (light, base, shade).</summary>
    public static void Fill(PixelCanvas c, IList<Vector2> pts, P[] ramp)
    {
        var layer = new PixelCanvas(c.Width, c.Height);
        PixelPaint.Poly(layer, pts, ramp[1]);
        float minX = float.MaxValue, maxX = float.MinValue;
        foreach (var p in pts) { minX = Math.Min(minX, p.X); maxX = Math.Max(maxX, p.X); }
        Shade(c, layer, ramp, minX, maxX);
    }

    public static void FillEllipse(PixelCanvas c, float cx, float cy, float rx, float ry, P[] ramp)
    {
        var layer = new PixelCanvas(c.Width, c.Height);
        PixelPaint.Ellipse(layer, cx, cy, rx, ry, ramp[1]);
        Shade(c, layer, ramp, cx - rx, cx + rx);
    }

    /// <summary>A limb: discs stamped along a bent path, thinning from radius `r0` to `r1`; the left edge is lit and the right one shaded.</summary>
    public static void Limb(PixelCanvas c, (float x, float y)[] path, float r0, float r1, P[] ramp)
    {
        var layer = new PixelCanvas(c.Width, c.Height);
        int segments = path.Length - 1;
        for (int s = 0; s < segments; s++)
            for (int i = 0; i <= 12; i++)
            {
                float t = i / 12f, u = (s + t) / segments;
                float x = path[s].x + (path[s + 1].x - path[s].x) * t, y = path[s].y + (path[s + 1].y - path[s].y) * t;
                PixelPaint.Ellipse(layer, x, y, r0 + (r1 - r0) * u, r0 + (r1 - r0) * u, ramp[1]);
            }
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                if (layer[x, y].A == 0) continue;
                float f = (Bayer.Threshold(x, y) - 0.5f) * 0.9f + (x % 5 < 2 ? 0 : 1) * 0.15f;
                bool leftEdge = x == 0 || layer[x - 1, y].A == 0;
                bool rightEdge = x == c.Width - 1 || layer[x + 1, y].A == 0 || (x + 2 < c.Width && layer[x + 2, y].A == 0);
                c[x, y] = leftEdge ? ramp[0] : (rightEdge ? ramp[2] : (f > 0.3f ? ramp[0] : ramp[1]));
            }
    }

    /// <summary>Puts `part` on `figure` with a one-pixel outline round it, so that parts stand out from the ones under them.</summary>
    public static void AddOutlined(PixelCanvas figure, PixelCanvas part, P outline)
    {
        (int, int)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
        for (int y = 0; y < part.Height; y++)
            for (int x = 0; x < part.Width; x++)
            {
                if (part[x, y].A != 0) continue;
                foreach (var (dx, dy) in dirs)
                    if (part.Contains(x + dx, y + dy) && part[x + dx, y + dy].A > 0) { figure[x, y] = outline; break; }
            }
        Blit(figure, part);
    }
}
