using Godot;
using System;

/// <summary>Pixel art for wake foam: a puff that breaks up into bubbles and then specks as it ages.</summary>
public static class FoamArt
{
    public const float Scale = 4;
    public const int StageCount = 4;

    /// <summary>Small puffs are 3x3 art pixels; large ones (the churn right behind the stern) are 5x5.</summary>
    public static Texture2D[] MakeStages(bool large)
    {
        var result = new Texture2D[StageCount];
        for (int i = 0; i < StageCount; i++) result[i] = MakeCanvas(i, large).ToTexture();
        return result;
    }

    public static Vector2 Size(bool large)
    {
        float n = (large ? 5 : 3) * Scale;
        return new Vector2(n, n);
    }

    private static readonly PixelColor Bright = PixelColor.Hex(0xF2FBF4);
    private static readonly PixelColor Light = PixelColor.Hex(0xD6EBDD);
    private static readonly PixelColor Shade = PixelColor.Hex(0x9CC9B4);

    public static PixelCanvas MakeCanvas(int stage, bool large)
    {
        int n = large ? 5 : 3;
        var c = new PixelCanvas(n, n);
        float centre = (float)n / 2;
        float radius = (float)n / 2 + 0.15f;

        for (int y = 0; y < n; y++)
        {
            for (int x = 0; x < n; x++)
            {
                float dx = x + 0.5f - centre, dy = y + 0.5f - centre;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                if (!(d <= radius)) continue;
                float t = d / radius;
                switch (stage)
                {
                    case 0: // fresh puff, brightest in the middle; big ones are a little ragged
                        if (large && t > 0.85f && (x + y) % 2 == 0) continue;
                        c[x, y] = t < 0.45f ? Bright : Light;
                        break;
                    case 1: // thinning: the lit side stays, the rest is lighter foam
                        c[x, y] = t < 0.5f ? Light : (dx + dy < 0 ? Light : Shade);
                        break;
                    case 2: // bubbles: only a ring is left
                        if (t > 0.5f) c[x, y] = Shade;
                        break;
                    default: // specks: a sparse scatter of dots
                        if ((x + y * 2) % 3 == 0) c[x, y] = Shade;
                        break;
                }
            }
        }
        return c;
    }
}
