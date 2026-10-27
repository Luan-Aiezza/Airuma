using System;

/// <summary>Radial light, dithered into a few opacity steps so it stays pixel art. Meant to be drawn additively.</summary>
public static class GlowArt
{
    public struct Tiers
    {
        public byte Strong, Medium, Faint;
        public Tiers(byte strong, byte medium, byte faint) { Strong = strong; Medium = medium; Faint = faint; }
    }

    public static PixelCanvas MakeCanvas(int size, float radius, PixelColor core, PixelColor rim, Tiers tiers)
    {
        var canvas = new PixelCanvas(size, size);
        float centre = size / 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - centre, dy = y - centre;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                float intensity = MathF.Pow(MathF.Max(0, 1 - d / radius), 1.7f);
                float q = intensity + (Bayer.Threshold(x, y) - 0.5f) * 0.1f;
                byte alpha;
                if (q >= 0.45f) alpha = tiers.Strong;
                else if (q >= 0.22f) alpha = tiers.Medium;
                else if (q >= 0.08f) alpha = tiers.Faint;
                else continue;
                canvas[x, y] = rim.Mixed(core, MathF.Min(1, intensity * 1.6f)).WithAlpha(alpha);
            }
        }
        return canvas;
    }
}
