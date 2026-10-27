using Godot;
using System;
using System.Linq;

/// <summary>
/// The little lights that spirits drop: warm specks in the colour of the boat's lantern, with a soft halo.
/// Two frames, so they twinkle. Drawn additively.
/// </summary>
public static class LightArt
{
    public const float Scale = 4;
    public const int Size = 5;
    public const int FrameCount = 2;

    public static PixelCanvas MakeCanvas(int frame)
    {
        var c = new PixelCanvas(Size, Size);
        var core = PixelColor.Hex(0xFFF6C0);
        var mid = PixelColor.Hex(0xFFD66B);
        var halo = PixelColor.Hex(0xFF9A1F, (byte)(frame == 0 ? 110 : 60));
        c[2, 2] = core;
        foreach (var (x, y) in new[] { (1, 2), (3, 2), (2, 1), (2, 3) }) c[x, y] = mid;
        foreach (var (x, y) in new[] { (1, 1), (3, 1), (1, 3), (3, 3) }) c[x, y] = halo;
        // A twinkle: the second frame stretches into a little cross.
        if (frame == 1) foreach (var (x, y) in new[] { (2, 0), (2, 4), (0, 2), (4, 2) }) c[x, y] = mid;
        return c;
    }

    /// <summary>The spare lantern that now and then drops: the lantern of the HUD in a pale blue-white glow that pulses (13 x 13 pixels).</summary>
    public const int BombSize = 13;

    public static PixelCanvas MakeBombCanvas(int frame)
    {
        var c = new PixelCanvas(BombSize, BombSize);
        float radius = frame == 0 ? 6.4f : 5.6f;
        for (int y = 0; y < BombSize; y++)
            for (int x = 0; x < BombSize; x++)
            {
                float d = MathF.Sqrt((x - 6f) * (x - 6f) + (y - 6f) * (y - 6f));
                if (d > radius) continue;
                float t = 1 - d / radius;
                // soft rings of a pale glow
                byte alpha = (byte)(t > 0.66f ? 120 : (t > 0.33f ? 80 : 40));
                if ((x + y) % 2 == 0 && t < 0.33f) alpha = 0;
                if (alpha > 0) c[x, y] = PixelColor.Hex(0xBFE8FF, alpha);
            }
        var icon = HudArt.LanternIcon(true);
        for (int y = 0; y < icon.Height; y++)
            for (int x = 0; x < icon.Width; x++)
                if (icon[x, y].A > 0) c[x + 3, y + 3] = icon[x, y];
        return c;
    }

    /// <summary>The spare boat that is given for a high score: the boat of the HUD in a warm red-gold glow that pulses (13 x 13 pixels).</summary>
    public static PixelCanvas MakeLifeCanvas(int frame)
    {
        var c = new PixelCanvas(BombSize, BombSize);
        float radius = frame == 0 ? 6.4f : 5.6f;
        for (int y = 0; y < BombSize; y++)
            for (int x = 0; x < BombSize; x++)
            {
                float d = MathF.Sqrt((x - 6f) * (x - 6f) + (y - 6f) * (y - 6f));
                if (d > radius) continue;
                float t = 1 - d / radius;
                byte alpha = (byte)(t > 0.66f ? 130 : (t > 0.33f ? 90 : 45));
                if ((x + y) % 2 == 0 && t < 0.33f) alpha = 0;
                if (alpha > 0) c[x, y] = PixelColor.Hex(0xFF8A5A, alpha);
            }
        var icon = HudArt.BoatIcon(true);
        int ox = (BombSize - icon.Width) / 2, oy = (BombSize - icon.Height) / 2;
        for (int y = 0; y < icon.Height; y++)
            for (int x = 0; x < icon.Width; x++)
                if (icon[x, y].A > 0) c[x + ox, y + oy] = icon[x, y];
        return c;
    }

    public static Texture2D[] MakeLifeFrames() =>
        Enumerable.Range(0, FrameCount).Select(f => (Texture2D)MakeLifeCanvas(f).ToTexture()).ToArray();

    public static Texture2D[] MakeBombFrames() =>
        Enumerable.Range(0, FrameCount).Select(f => (Texture2D)MakeBombCanvas(f).ToTexture()).ToArray();

    public static Texture2D[] MakeFrames() =>
        Enumerable.Range(0, FrameCount).Select(f => (Texture2D)MakeCanvas(f).ToTexture()).ToArray();
}
