using System;
using System.Collections.Generic;
using Godot;
using P = PixelColor;

/// <summary>
/// The tools to draw the faces of the dialogues (the portrait in the square at the right of the dialogue box): the hair and the clothes are drawn in a fine grid of 64 x 64 and halved to the 32 x 32 of the faces (see FaceRig32); 64 x 64 pixels, the head and a little of the
/// shoulders, big soft eyes under a heavy lash with a glint, soft shading in three or four tones, locks of hair drawn as rounded tubes with a lit side, and an outline that is not black but a dark shade of the colour it
/// surrounds. Everything is made in code, like the rest of the art of the game. <see cref="FaceRig"/> draws the features; each character gives its own hair, clothes and marks.
/// </summary>
public static class PortraitKit
{
    public const int Size = 64;

    public static PixelCanvas NewCanvas() => new PixelCanvas(Size, Size);

    /// <summary>Which expression a portrait shows: how the brows lean, how open the eyes are, where they look, the mouth, and how much the cheeks blush.</summary>
    public struct Face
    {
        public float BrowTilt;      // + : the inner ends go down (angry), - : up (worried)
        public float BrowLift;      // px the brows are raised (+) or lowered (-)
        public float EyeOpen;       // 1: normal, 0.7: lids low, 1.3: wide
        public bool EyesClosedHappy;
        public float Look;          // -1 ... 1: where the eyes look, sideways
        public Mouth MouthShape;
        public float Blush;         // 0...1
        public bool Sweat;
    }

    public enum Mouth { Flat, Smile, Grin, Frown, Open, Small, Wavy, Smirk }

    public static Face FaceOf(Emotion emotion) => emotion switch
    {
        Emotion.Annoyed => new Face { BrowTilt = 2f, BrowLift = -1, EyeOpen = 0.88f, MouthShape = Mouth.Frown, Blush = 0.1f },
        Emotion.Sheepish => new Face { BrowTilt = -1f, EyeOpen = 0.95f, Look = -0.8f, MouthShape = Mouth.Wavy, Blush = 1f, Sweat = true },
        Emotion.Surprised => new Face { BrowLift = 2.5f, EyeOpen = 1.35f, MouthShape = Mouth.Open, Blush = 0.2f },
        Emotion.Happy => new Face { EyeOpen = 1, EyesClosedHappy = true, MouthShape = Mouth.Grin, Blush = 0.7f },
        Emotion.Worried => new Face { BrowTilt = -2f, BrowLift = 1, EyeOpen = 1.1f, MouthShape = Mouth.Small, Blush = 0.1f },
        _ => new Face { EyeOpen = 1, MouthShape = Mouth.Smile, Blush = 0.35f },
    };

    // MARK: Drawing

    public static bool In(PixelCanvas c, int x, int y) => x >= 0 && y >= 0 && x < c.Width && y < c.Height;

    public static void Put(PixelCanvas c, int x, int y, P color) { if (In(c, x, y)) c[x, y] = color; }

    /// <summary>A Catmull-Rom curve through `ctrl`, as `n` points.</summary>
    public static List<Vector2> Curve(Vector2[] ctrl, int n = 48)
    {
        var points = new List<Vector2>();
        int segs = ctrl.Length - 1;
        if (segs < 1) { points.Add(ctrl[0]); return points; }
        for (int i = 0; i < n; i++)
        {
            float t = (float)i / (n - 1) * segs;
            int s = Math.Min(segs - 1, (int)t);
            float u = t - s;
            Vector2 p0 = ctrl[Math.Max(0, s - 1)], p1 = ctrl[s], p2 = ctrl[s + 1], p3 = ctrl[Math.Min(segs, s + 2)];
            points.Add(0.5f * ((2 * p1) + (-p0 + p2) * u + (2 * p0 - 5 * p1 + 4 * p2 - p3) * u * u + (-p0 + 3 * p1 - 3 * p2 + p3) * u * u * u));
        }
        return points;
    }

    /// <summary>
    /// A lock of hair, a rope, a tentacle: a tube along the curve through `ctrl`, `r0` thick at its start and `r1` at its end, shaded across (the ramp from lit to dark: `ramp[0]` is the glint, the last
    /// is the shade at the edge away from the light). `light` is where the light lies across the tube (-1: the left side, 1: the right).
    /// </summary>
    public static void Tube(PixelCanvas c, Vector2[] ctrl, float r0, float r1, P[] ramp, float light = -0.4f, int samples = 56, bool flat = false)
    {
        var pts = Curve(ctrl, samples);
        var radius = new float[pts.Count];
        var normal = new Vector2[pts.Count];
        for (int i = 0; i < pts.Count; i++)
        {
            radius[i] = r0 + (r1 - r0) * i / Math.Max(1, pts.Count - 1);
            var tangent = (pts[Math.Min(pts.Count - 1, i + 1)] - pts[Math.Max(0, i - 1)]).Normalized();
            normal[i] = new Vector2(-tangent.Y, tangent.X);
        }
        float minX = 999, minY = 999, maxX = -999, maxY = -999;
        foreach (var p in pts) { minX = Math.Min(minX, p.X); minY = Math.Min(minY, p.Y); maxX = Math.Max(maxX, p.X); maxY = Math.Max(maxY, p.Y); }
        float pad = Math.Max(r0, r1) + 1;
        for (int y = Math.Max(0, (int)(minY - pad)); y <= Math.Min(c.Height - 1, (int)(maxY + pad)); y++)
            for (int x = Math.Max(0, (int)(minX - pad)); x <= Math.Min(c.Width - 1, (int)(maxX + pad)); x++)
            {
                var p = new Vector2(x + 0.5f, y + 0.5f);
                float best = 1e9f; int bi = -1;
                for (int i = 0; i < pts.Count; i++)
                {
                    float d = p.DistanceSquaredTo(pts[i]);
                    if (d < best) { best = d; bi = i; }
                }
                float dist = MathF.Sqrt(best);
                if (dist > radius[bi]) continue;
                float lateral = (p - pts[bi]).Dot(normal[bi]) / radius[bi];
                float t = MathF.Abs(lateral - light);
                int idx = flat ? (t < 0.38f ? 0 : (t < 0.88f ? 1 : 2)) : (t < 0.22f ? 0 : (t < 0.55f ? 1 : (t < 0.95f ? 2 : 3)));
                c[x, y] = ramp[Math.Min(ramp.Length - 1, idx)];
            }
    }

    /// <summary>
    /// A clump of hair (or a fold of cloth) in the manner of the small pixel arts: a tube in three flat tones (light, middle, dark) with a dark outline of its own, so that clumps that overlap are told apart by a line.
    /// </summary>
    public static void Clump(PixelCanvas c, Vector2[] ctrl, float r0, float r1, P[] ramp3, P outline, float light = -0.4f)
    {
        Tube(c, ctrl, r0 + 1f, r1 + 0.8f, new[] { outline }, 0, 40);
        Tube(c, ctrl, r0, r1, ramp3, light, 40, flat: true);
    }

    /// <summary>A clump and its mirror image about the column `axis`.</summary>
    public static void ClumpPair(PixelCanvas c, float axis, Vector2[] ctrl, float r0, float r1, P[] ramp3, P outline, float light = -0.4f)
    {
        Clump(c, ctrl, r0, r1, ramp3, outline, light);
        var m = new Vector2[ctrl.Length];
        for (int i = 0; i < ctrl.Length; i++) m[i] = new Vector2(2 * axis - ctrl[i].X, ctrl[i].Y);
        Clump(c, m, r0, r1, ramp3, outline, -light);
    }

    /// <summary>Draws a shape with `draw` on a layer of its own and puts it on `dst` with a one-pixel outline of colour `line`.</summary>
    public static void Outlined(PixelCanvas dst, Action<PixelCanvas> draw, P line)
    {
        var layer = new PixelCanvas(dst.Width, dst.Height);
        draw(layer);
        Blit(dst, Outline(layer, _ => line));
        Blit(dst, layer);
    }

    /// <summary>A blob in three flat tones with a dark outline of its own.</summary>
    public static void BlobO(PixelCanvas c, float cx, float cy, float rx, float ry, P[] ramp3, P outline, float lightX = -0.3f, float lightY = -0.4f)
    {
        Blob(c, cx, cy, rx + 1f, ry + 1f, new[] { outline }, 0, 0);
        for (int y = (int)(cy - ry - 1); y <= cy + ry + 1; y++)
            for (int x = (int)(cx - rx - 1); x <= cx + rx + 1; x++)
            {
                float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry;
                if (dx * dx + dy * dy > 1 || !In(c, x, y)) continue;
                float t = MathF.Sqrt((dx - lightX) * (dx - lightX) + (dy - lightY) * (dy - lightY));
                c[x, y] = ramp3[t < 0.5f ? 0 : (t < 1.0f ? 1 : 2)];
            }
    }

    /// <summary>A filled ellipse with a rim: `ramp[0]` in the lit part (up and to the left), then the others towards the shade at the far edge.</summary>
    public static void Blob(PixelCanvas c, float cx, float cy, float rx, float ry, P[] ramp, float lightX = -0.35f, float lightY = -0.45f)
    {
        for (int y = (int)(cy - ry - 1); y <= cy + ry + 1; y++)
            for (int x = (int)(cx - rx - 1); x <= cx + rx + 1; x++)
            {
                float dx = (x + 0.5f - cx) / rx, dy = (y + 0.5f - cy) / ry;
                if (dx * dx + dy * dy > 1 || !In(c, x, y)) continue;
                float t = MathF.Sqrt((dx - lightX) * (dx - lightX) + (dy - lightY) * (dy - lightY));
                int idx = t < 0.45f ? 0 : (t < 0.95f ? 1 : (t < 1.3f ? 2 : 3));
                c[x, y] = ramp[Math.Min(ramp.Length - 1, idx)];
            }
    }

    /// <summary>The outline of what is drawn on `layer`: a pixel round it, in the colour that `colour` gives for the pixel it touches (a dark shade of it, not black).</summary>
    public static PixelCanvas Outline(PixelCanvas layer, Func<P, P> colour)
    {
        var o = new PixelCanvas(layer.Width, layer.Height);
        (int, int)[] dirs = { (1, 0), (-1, 0), (0, 1), (0, -1) };
        for (int y = 0; y < layer.Height; y++)
            for (int x = 0; x < layer.Width; x++)
            {
                if (layer[x, y].A != 0) continue;
                foreach (var (dx, dy) in dirs)
                    if (layer.Contains(x + dx, y + dy) && layer[x + dx, y + dy].A > 0) { o[x, y] = colour(layer[x + dx, y + dy]); break; }
            }
        return o;
    }

    /// <summary>A dark, saturated shade of a colour, for outlines: darker and a little towards `tint`.</summary>
    public static P Dark(P c, P tint, float amount = 0.62f, float toTint = 0.25f) => c.Scaled(1 - amount).Mixed(tint, toTint);

    public static void Blit(PixelCanvas dst, PixelCanvas src)
    {
        for (int y = 0; y < src.Height; y++) for (int x = 0; x < src.Width; x++) if (src[x, y].A > 0) dst[x, y] = src[x, y];
    }

    /// <summary>Puts `layer` over `dst` with its outline under it.</summary>
    public static void Stack(PixelCanvas dst, PixelCanvas layer, Func<P, P> outline)
    {
        if (outline != null) Blit(dst, Outline(layer, outline));
        Blit(dst, layer);
    }

    /// <summary>A tube and its mirror image about the column `axis`, shaded the same way on both (lit on the outer side), so that hair and ornaments are the same on both sides.</summary>
    public static void Pair(PixelCanvas c, float axis, Vector2[] ctrl, float r0, float r1, P[] ramp, float light = -0.4f)
    {
        Tube(c, ctrl, r0, r1, ramp, light);
        var m = new Vector2[ctrl.Length];
        for (int i = 0; i < ctrl.Length; i++) m[i] = new Vector2(2 * axis - ctrl[i].X, ctrl[i].Y);
        Tube(c, m, r0, r1, ramp, -light);
    }

    /// <summary>
    /// Halves a 64 x 64 drawing to 32 x 32: each pixel of the result takes the colour that most of its four pixels have (the darker when they tie), and is empty if most of them are. The hair and the clothes are drawn
    /// in the fine grid and come out as the broad, flat shapes of a pixel art of 32 x 32.
    /// </summary>
    public static PixelCanvas Halve(PixelCanvas c)
    {
        var o = new PixelCanvas(c.Width / 2, c.Height / 2);
        for (int y = 0; y < o.Height; y++)
            for (int x = 0; x < o.Width; x++)
            {
                var counts = new Dictionary<P, int>();
                int solid = 0;
                for (int j = 0; j < 2; j++)
                    for (int i = 0; i < 2; i++)
                    {
                        var p = c[x * 2 + i, y * 2 + j];
                        if (p.A == 0) continue;
                        solid++;
                        counts[p] = counts.TryGetValue(p, out var n) ? n + 1 : 1;
                    }
                if (solid < 2) continue;
                P best = default; int bestN = 0; float bestL = 9;
                foreach (var kv in counts)
                {
                    float l = kv.Key.R * 0.3f + kv.Key.G * 0.59f + kv.Key.B * 0.11f;
                    if (kv.Value > bestN || (kv.Value == bestN && l < bestL)) { best = kv.Key; bestN = kv.Value; bestL = l; }
                }
                o[x, y] = best;
            }
        return o;
    }

    public static float Hash(int x, int y) { unchecked { int h = x * 374761393 + y * 668265263; h = (h ^ (h >> 13)) * 1274126177; return ((h ^ (h >> 16)) & 0xFFFF) / 65535f; } }

    /// <summary>
    /// The square at the right of the dialogue box: the portrait over a dim backdrop of the colour of its character (a vertical glow with a dithered edge), in a small wooden frame. The portrait is 64 x 64; the whole is 68 x 68,
    /// and the box shows it at twice its size.
    /// </summary>
    public static PixelCanvas Framed(PixelCanvas portrait, P backdropTop, P backdropBottom)
    {
        int frame = portrait.Width >= 64 ? 2 : 1;
        int size = portrait.Width;
        var c = new PixelCanvas(size + 2 * frame, size + 2 * frame);
        bool flat = backdropTop == backdropBottom;                  // the faces of 32 x 32 have a flat backdrop, like the small pixel arts they follow
        for (int y = 0; y < c.Height; y++)
            for (int x = 0; x < c.Width; x++)
            {
                if (flat) { c[x, y] = backdropTop; continue; }
                float t = (float)y / (c.Height - 1);
                float d = MathF.Sqrt((x - c.Width / 2f) * (x - c.Width / 2f) + (y - c.Height * 0.55f) * (y - c.Height * 0.55f)) / (c.Width * 0.62f);
                var mix = backdropTop.Mixed(backdropBottom, t);
                c[x, y] = d < 0.55f ? mix.Mixed(P.Hex(0xFFFFFF), 0.10f) : mix;
            }
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) if (portrait[x, y].A > 0) c[x + frame, y + frame] = portrait[x, y];
        var ink = flat ? P.Hex(0x010018) : P.Hex(0x120D11); var wood = P.Hex(0x7A4A1E); var light = P.Hex(0xC8923E);
        for (int i = 0; i < c.Width; i++)
        {
            c[i, 0] = ink; c[i, c.Height - 1] = ink; c[0, i] = ink; c[c.Width - 1, i] = ink;
            if (frame > 1) { c[i, 1] = i % 2 == 0 ? light : wood; c[i, c.Height - 2] = i % 2 == 0 ? wood : light; c[1, i] = i % 2 == 0 ? light : wood; c[c.Width - 2, i] = i % 2 == 0 ? wood : light; }
        }
        return c;
    }
}
