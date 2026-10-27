using System;

/// <summary>
/// A small height map used to give pixel art volume: shapes are stamped in as domes, then lit from the upper left
/// and quantised into a palette. It wraps vertically, so anything built on it tiles in Y.
/// (A class: use <see cref="Clone"/> to copy it.)
/// </summary>
public sealed class Heightfield
{
    public readonly int Width;
    public readonly int Height;
    public readonly float[] Values;
    /// <summary>An optional label per pixel (the depth of what was stamped there, for example), set by whatever raised it.</summary>
    public readonly float[] Tags;

    public Heightfield(int width, int height)
    {
        Width = width;
        Height = height;
        Values = new float[width * height];
        Tags = new float[width * height];
    }

    public Heightfield Clone()
    {
        var h = new Heightfield(Width, Height);
        Array.Copy(Values, h.Values, Values.Length);
        Array.Copy(Tags, h.Tags, Tags.Length);
        return h;
    }

    private int WrapY(int y) => ((y % Height) + Height) % Height;

    /// <summary>Height at a pixel; zero outside the canvas horizontally, wrapped vertically.</summary>
    public float Value(int x, int y)
    {
        if (!(x >= 0 && x < Width)) return 0;
        return Values[WrapY(y) * Width + x];
    }

    /// <summary>Sets a height directly (for shapes defined by a formula rather than stamped from domes).</summary>
    public void Set(int x, int y, float v, float tag = 0)
    {
        if (!(x >= 0 && x < Width && y >= 0 && y < Height)) return;
        Values[y * Width + x] = v;
        Tags[y * Width + x] = tag;
    }

    public void Raise(int x, int y, float to, float tag = 0)
    {
        if (!(x >= 0 && x < Width)) return;
        int i = WrapY(y) * Width + x;
        if (to > Values[i])
        {
            Values[i] = to;
            Tags[i] = tag;
        }
    }

    public float Tag(int x, int y)
    {
        if (!(x >= 0 && x < Width)) return 0;
        return Tags[WrapY(y) * Width + x];
    }

    /// <summary>Adds a round dome centred on a continuous position (pixel centres are at +0.5).</summary>
    public void Stamp(float cx, float cy, float radius, float peak, float tag = 0)
    {
        int r = (int)MathF.Ceiling(radius) + 1;
        for (int dy = -r; dy <= r; dy++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int x = (int)MathF.Floor(cx) + dx, y = (int)MathF.Floor(cy) + dy;
                float ex = x + 0.5f - cx, ey = y + 0.5f - cy;
                float d = MathF.Sqrt(ex * ex + ey * ey);
                if (!(d <= radius)) continue;
                Raise(x, y, peak * MathF.Sqrt(1 - (d / radius) * (d / radius)), tag);
            }
        }
    }

    public Heightfield MirroredX()
    {
        var m = new Heightfield(Width, Height);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                m.Values[y * Width + x] = Values[y * Width + (Width - 1 - x)];
                m.Tags[y * Width + x] = Tags[y * Width + (Width - 1 - x)];
            }
        }
        return m;
    }

    public struct Sample
    {
        public int X, Y;
        public float H;
        /// <summary>The label stored by whatever raised this pixel (see <see cref="Heightfield.Tags"/>).</summary>
        public float Tag;
        /// <summary>Light on this pixel, roughly -0.5 (shadow) ... 1.2 (facing the light).</summary>
        public float Lit;
        /// <summary>On the silhouette: some 4-neighbour is empty.</summary>
        public bool Edge;
        /// <summary>The silhouette pixel faces the light (the empty neighbour is above or to the left).</summary>
        public bool Rim;
        /// <summary>The empty neighbour is directly above: the pixel's upper edge catches the moonlight.</summary>
        public bool Top;
        /// <summary>Distance in pixels to the nearest empty pixel to the right/left, up to 3 (for wet edges).</summary>
        public int GapSide;
    }

    public const float Threshold = 0.03f;

    public static readonly (float x, float y, float z) DefaultLight = (-0.55f, -0.6f, 0.58f);

    /// <summary>Paints every raised pixel with <paramref name="color"/>, which may return null to leave it empty.</summary>
    public PixelCanvas Shade(Func<Sample, PixelColor?> color, float strength = 2.4f, (float x, float y, float z)? light = null)
    {
        var L = light ?? DefaultLight;
        var canvas = new PixelCanvas(Width, Height);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float h = Value(x, y);
                if (!(h > Threshold)) continue;
                float gx = (Value(x + 1, y) - Value(x - 1, y)) * strength;
                float gy = (Value(x, y + 1) - Value(x, y - 1)) * strength;
                float length = MathF.Sqrt(gx * gx + gy * gy + 1);
                float lit = (gx * -L.x + gy * -L.y + L.z) / length;
                bool emptyRight = Value(x + 1, y) <= Threshold;
                bool emptyLeft = Value(x - 1, y) <= Threshold;
                bool emptyUp = Value(x, y - 1) <= Threshold;
                bool emptyDown = Value(x, y + 1) <= Threshold;
                int gap = 3;
                for (int d = 1; d <= 3; d++)
                {
                    if (Value(x + d, y) <= Threshold || Value(x - d, y) <= Threshold) { gap = d - 1; break; }
                }
                var sample = new Sample
                {
                    X = x, Y = y, H = h, Tag = Tag(x, y), Lit = lit,
                    Edge = emptyRight || emptyLeft || emptyUp || emptyDown,
                    Rim = emptyLeft || emptyUp, Top = emptyUp, GapSide = gap,
                };
                var c = color(sample);
                if (c.HasValue) canvas[x, y] = c.Value;
            }
        }
        return canvas;
    }

    /// <summary>Picks from a dark-to-light ramp for a light value, with ordered dithering between steps.</summary>
    public static PixelColor Pick(PixelColor[] ramp, float lit, int x, int y)
    {
        float t = MathF.Max(0, MathF.Min(0.999f, (lit + 0.15f) / 1.1f));
        float f = t * ramp.Length + (Bayer.Threshold(x, y) - 0.5f) * 0.7f;
        return ramp[Math.Max(0, Math.Min(ramp.Length - 1, (int)MathF.Floor(f)))];
    }
}
