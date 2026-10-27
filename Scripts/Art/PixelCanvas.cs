using Godot;
using System;

/// <summary>An RGBA colour with byte channels (art is generated in code, pixel by pixel).</summary>
public readonly struct PixelColor : IEquatable<PixelColor>
{
    public readonly byte R, G, B, A;

    public PixelColor(byte r, byte g, byte b, byte a = 255) { R = r; G = g; B = b; A = a; }

    /// <summary>0xRRGGBB</summary>
    public static PixelColor Hex(uint hex, byte alpha = 255) =>
        new PixelColor((byte)((hex >> 16) & 0xFF), (byte)((hex >> 8) & 0xFF), (byte)(hex & 0xFF), alpha);

    public static readonly PixelColor Clear = new PixelColor(0, 0, 0, 0);

    /// <summary>Linear blend towards <paramref name="other"/> by t (0..1), keeping this colour's alpha.</summary>
    public PixelColor Mixed(PixelColor other, float t)
    {
        static byte Lerp(byte a, byte b, float t) => (byte)Math.Clamp(a + (b - a) * t, 0f, 255f);
        return new PixelColor(Lerp(R, other.R, t), Lerp(G, other.G, t), Lerp(B, other.B, t), A);
    }

    public PixelColor Scaled(float factor)
    {
        static byte S(byte v, float f) => (byte)Math.Clamp(v * f, 0f, 255f);
        return new PixelColor(S(R, factor), S(G, factor), S(B, factor), A);
    }

    public PixelColor WithAlpha(byte alpha) => new PixelColor(R, G, B, alpha);

    public Color ToColor() => new Color(R / 255f, G / 255f, B / 255f, A / 255f);

    public bool Equals(PixelColor o) => R == o.R && G == o.G && B == o.B && A == o.A;
    public override bool Equals(object obj) => obj is PixelColor o && Equals(o);
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);
    public static bool operator ==(PixelColor a, PixelColor b) => a.Equals(b);
    public static bool operator !=(PixelColor a, PixelColor b) => !a.Equals(b);
}

/// <summary>
/// A small RGBA bitmap drawn pixel by pixel in code and turned into a nearest-filtered texture. Row 0 is the TOP
/// (as in a Godot Image). NOTE: this is a class: assignment shares the canvas; call <see cref="Clone"/> to copy.
/// </summary>
public sealed class PixelCanvas
{
    public readonly int Width;
    public readonly int Height;
    public readonly PixelColor[] Pixels;

    public PixelCanvas(int width, int height, PixelColor? fill = null)
    {
        Width = width;
        Height = height;
        Pixels = new PixelColor[width * height];
        if (fill.HasValue) Array.Fill(Pixels, fill.Value);
    }

    public PixelColor this[int x, int y]
    {
        get => Pixels[y * Width + x];
        set => Pixels[y * Width + x] = value;
    }

    public bool Contains(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

    public PixelCanvas Clone()
    {
        var c = new PixelCanvas(Width, Height);
        Array.Copy(Pixels, c.Pixels, Pixels.Length);
        return c;
    }

    /// <summary>Writes a pixel, wrapping vertically (so textures built with it tile in Y). X outside is ignored.</summary>
    public void SetWrappedY(int x, int y, PixelColor color)
    {
        if (x < 0 || x >= Width) return;
        this[x, ((y % Height) + Height) % Height] = color;
    }

    public Image ToImage()
    {
        var data = new byte[Pixels.Length * 4];
        for (int i = 0; i < Pixels.Length; i++)
        {
            var p = Pixels[i];
            data[i * 4] = p.R; data[i * 4 + 1] = p.G; data[i * 4 + 2] = p.B; data[i * 4 + 3] = p.A;
        }
        return Image.CreateFromData(Width, Height, false, Image.Format.Rgba8, data);
    }

    /// <summary>A texture of this canvas (the project's default texture filter is nearest).</summary>
    public ImageTexture ToTexture() => ImageTexture.CreateFromImage(ToImage());
}

/// <summary>Deterministic generator so generated art is identical on every launch.</summary>
public sealed class SeededGenerator
{
    private ulong _state;

    public SeededGenerator(ulong seed) { _state = seed; }

    public ulong NextULong()
    {
        unchecked
        {
            _state += 0x9E3779B97F4A7C15UL;
            ulong z = _state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
    }

    /// <summary>A float in [0, 1).</summary>
    public float NextFloat() => (NextULong() >> 40) / (float)(1UL << 24);

    /// <summary>A float in [lo, hi].</summary>
    public float Range(float lo, float hi) => lo + (hi - lo) * NextFloat();

    /// <summary>An int in [lo, hi] (inclusive).</summary>
    public int Range(int lo, int hi) => lo + (int)(NextULong() % (ulong)(hi - lo + 1));
}

/// <summary>
/// Value noise on a lattice that wraps in both axes, sampled with normalised 0..1 coordinates, so anything built from
/// it tiles seamlessly.
/// </summary>
public sealed class PeriodicNoise
{
    private readonly int _gridW, _gridH;
    private readonly float[] _values;

    public PeriodicNoise(int gridW, int gridH, SeededGenerator generator)
    {
        _gridW = gridW;
        _gridH = gridH;
        _values = new float[gridW * gridH];
        for (int i = 0; i < _values.Length; i++) _values[i] = generator.NextFloat();
    }

    private static int Mod(int a, int n) => ((a % n) + n) % n;
    private static float Smooth(float t) => t * t * (3 - 2 * t);

    private float V(int x, int y) => _values[Mod(y, _gridH) * _gridW + Mod(x, _gridW)];

    public float Sample(float nx, float ny)
    {
        float fx = nx * _gridW, fy = ny * _gridH;
        int x0 = (int)MathF.Floor(fx), y0 = (int)MathF.Floor(fy);
        float tx = Smooth(fx - x0), ty = Smooth(fy - y0);
        float top = V(x0, y0) + (V(x0 + 1, y0) - V(x0, y0)) * tx;
        float bottom = V(x0, y0 + 1) + (V(x0 + 1, y0 + 1) - V(x0, y0 + 1)) * tx;
        return top + (bottom - top) * ty;
    }
}

public static class Bayer
{
    private static readonly float[,] Matrix =
    {
        { 0, 8, 2, 10 },
        { 12, 4, 14, 6 },
        { 3, 11, 1, 9 },
        { 15, 7, 13, 5 },
    };

    /// <summary>Ordered-dither threshold in 0..&lt;1 for a pixel position.</summary>
    public static float Threshold(int x, int y) => (Matrix[((y % 4) + 4) % 4, ((x % 4) + 4) % 4] + 0.5f) / 16f;
}

/// <summary>Small maths helpers (the game uses Godot's Vector2 and Rect2 directly; Y grows downward in both).</summary>
public static class Mathx
{
    public static float Radians(float degrees) => degrees * MathF.PI / 180f;
    public static float Degrees(float radians) => radians * 180f / MathF.PI;

    /// <summary>Zero vectors stay zero instead of becoming NaN.</summary>
    public static Vector2 Norm(Vector2 v) { float l = v.Length(); return l > 0 ? v / l : Vector2.Zero; }

    public static Rect2 Grown(this Rect2 r, float amount) =>
        new Rect2(r.Position.X - amount, r.Position.Y - amount, r.Size.X + amount * 2, r.Size.Y + amount * 2);
}
