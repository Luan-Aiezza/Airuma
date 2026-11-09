using System;
using Godot;
using P = PixelColor;
using static PortraitKit;

/// <summary>The face of the lantern spirit, Naru: only the flame of the lantern, the same as the one on the panel of the game, big, with its glow and its sparks; it flickers between two frames.</summary>
public static class FlamePortrait
{
    public static PixelCanvas Make(Emotion emotion, int frame)
    {
        var c = NewCanvas();
        // the glow: warm light that fades into the night, in dithered rings
        var night = P.Hex(0x1A1830); var glow = P.Hex(0x8A5424);
        for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                float d = MathF.Sqrt((x - 32f) * (x - 32f) + (y - 38f) * (y - 38f));
                float i = MathF.Max(0, 1 - d / 34);
                c[x, y] = night.Mixed(glow, i * 1.3f > Bayer.Threshold(x, y) ? 0.9f : 0.1f);
            }
        // the flame of the panel, enlarged three times
        var small = new PixelCanvas(MenuArt.FlameWidth + 2, MenuArt.FlameHeight + 2);
        MenuArt.DrawFlame(small, frame, 1, 1);
        const int k = 3;
        int ox = 32 - small.Width * k / 2, oy = 36 - small.Height * k / 2;
        for (int y = 0; y < small.Height; y++)
            for (int x = 0; x < small.Width; x++)
            {
                var p = small[x, y];
                if (p.A == 0) continue;
                for (int j = 0; j < k; j++) for (int i = 0; i < k; i++) Put(c, ox + x * k + i, oy + y * k + j, p);
            }
        // sparks
        var spark = P.Hex(0xFFFFD8);
        foreach (var (sx, sy) in frame == 0 ? new[] { (12, 12), (50, 18), (9, 40), (54, 44) } : new[] { (52, 11), (13, 19), (55, 38), (10, 46) }) { Put(c, sx, sy, spark); Put(c, sx - 1, sy, spark.Mixed(night, 0.5f)); Put(c, sx + 1, sy, spark.Mixed(night, 0.5f)); Put(c, sx, sy - 1, spark.Mixed(night, 0.5f)); Put(c, sx, sy + 1, spark.Mixed(night, 0.5f)); }
        return c;
    }
}
