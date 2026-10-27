using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// The menu's backdrop, in pixel art: the view from the boat, looking down a river into a moonlit jungle.
///
/// The picture is built from layers on a 278x194 grid (4 points per pixel), back to front: the sky with its moon,
/// two lines of distant trees in the mist, the water (a mirror of the layers above it, rippled by a shader),
/// the near banks (giant trunks, roots, palms, hanging vines, all lit from behind by the moon so that only their
/// rims catch the light), and the bow of the boat with its lantern.
/// </summary>
public static class MenuArt
{
    public const int Width = 278;
    public const int Height = 194;
    public const float Pixel = 4;

    /// <summary>Height maps wrap vertically; shapes that run off the bottom are given spare rows, which are cropped away.</summary>
    private const int Spare = 28;

    private static PixelCanvas Cropped(PixelCanvas canvas)
    {
        var o = new PixelCanvas(Width, Height);
        for (int y = 0; y < Height; y++)
            for (int x = 0; x < Width; x++)
                o[x, y] = canvas[x, y];
        return o;
    }

    /// <summary>Row of the horizon.</summary>
    public const int Horizon = 84;
    /// <summary>The moon, in grid pixels.</summary>
    public static readonly (int x, int y) Moon = (182, 36);
    public const float MoonRadius = 11;

    // MARK: Geometry of the river

    /// <summary>Column of the middle of the river on row <paramref name="y"/> (below the horizon): it winds towards the horizon on the right.</summary>
    public static float RiverCentre(int y)
    {
        float t = (float)Math.Max(0, y - Horizon) / (Height - Horizon);
        return 158 - 20 * t + 5 * MathF.Sin(t * 3.4f);
    }

    /// <summary>Half the river's width on row <paramref name="y"/>; it widens as it comes towards us (perspective).</summary>
    public static float RiverHalfWidth(int y) => 7 + 0.63f * Math.Max(0, y - Horizon);

    // MARK: Palettes

    public static readonly PixelColor HazeColor = PixelColor.Hex(0x2A6483);
    private static readonly PixelColor[] JungleRamp =
    {
        PixelColor.Hex(0x04090D), PixelColor.Hex(0x08141A), PixelColor.Hex(0x0E2227),
        PixelColor.Hex(0x173836), PixelColor.Hex(0x2A5A4E), PixelColor.Hex(0x4E8C78), PixelColor.Hex(0x9CD0C0),
    };
    private static readonly PixelColor[] TrunkRamp =
    {
        PixelColor.Hex(0x080506), PixelColor.Hex(0x120C0C), PixelColor.Hex(0x241814),
        PixelColor.Hex(0x3A281E), PixelColor.Hex(0x5A4030), PixelColor.Hex(0x8C6A52), PixelColor.Hex(0xC8B49C),
    };
    /// <summary>The moon is up and behind the scene: light comes from the upper right and a little from the back.</summary>
    private static readonly (float x, float y, float z) MoonLight = (0.62f, -0.5f, 0.28f);

    private static float Hypot(float a, float b) => MathF.Sqrt(a * a + b * b);

    // MARK: Sky

    /// <summary>
    /// The sky down to the horizon: a deep gradient with a glow around the moon, streaky clouds, a Milky Way, stars
    /// and the moon with its craters.
    /// </summary>
    public static PixelCanvas MakeSky()
    {
        var rng = new SeededGenerator(0x5EED);
        var clouds = new PeriodicNoise(7, 4, rng);
        var c = new PixelCanvas(Width, Horizon);
        var top = PixelColor.Hex(0x03070F); var middle = PixelColor.Hex(0x0A1D38); var low = PixelColor.Hex(0x2B6E8C);
        var halo = PixelColor.Hex(0x4F93B0);

        for (int y = 0; y < Horizon; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float t = (float)y / Horizon;
                var color = t < 0.6f ? top.Mixed(middle, t / 0.6f) : middle.Mixed(low, MathF.Pow((t - 0.6f) / 0.4f, 1.4f));
                float d = Hypot(x - Moon.x, y - Moon.y);
                float glow = MathF.Pow(MathF.Max(0, 1 - d / 62), 1.7f);
                float band = MathF.Floor(glow * 6 + (Bayer.Threshold(x, y) - 0.5f) * 0.8f) / 6;
                if (band > 0) color = color.Mixed(halo, MathF.Min(1, band * 0.9f));
                float n = clouds.Sample((float)x / Width, (float)y / Horizon);
                if (n > 0.58f)
                {
                    float level = MathF.Floor((n - 0.58f) * 6 * 3 + (Bayer.Threshold(x, y) - 0.5f) * 0.7f) / 3;
                    if (level > 0) color = color.Mixed(PixelColor.Hex(0x6AA3BE), MathF.Min(0.55f, level * (0.25f + glow * 0.4f)));
                }
                c[x, y] = color;
            }
        }
        // The Milky Way, a diagonal smudge of faint stars.
        for (int i = 0; i < 700; i++)
        {
            float u = rng.Range(0f, 1f);
            float across = (rng.Range(-1f, 1f) + rng.Range(-1f, 1f)) * 12;
            int x = (int)(Width * (0.05f + 0.5f * u) + across * 0.6f), y = (int)(Horizon * (0.85f - 0.8f * u) + across * 0.9f);
            if (!(c.Contains(x, y) && Hypot(x - Moon.x, y - Moon.y) > 24)) continue;
            c[x, y] = c[x, y].Mixed(PixelColor.Hex(0xA6C0DC), rng.Range(0.25f, 0.6f));
        }
        var starColors = new[] { PixelColor.Hex(0xFFFFFF), PixelColor.Hex(0xB4CCFF), PixelColor.Hex(0xFFE9B8) };
        for (int i = 0; i < 170; i++)
        {
            int x = rng.Range(1, Width - 2), y = rng.Range(1, Horizon - 15);
            if (!(Hypot(x - Moon.x, y - Moon.y) > 22)) continue;
            var color = starColors[rng.Range(0, starColors.Length - 1)];
            c[x, y] = color;
            if (i % 9 == 0)
                foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    c[x + dx, y + dy] = color.Mixed(c[x + dx, y + dy], 0.55f);
        }
        DrawMoon(c);
        return c;
    }

    private static void DrawMoon(PixelCanvas c)
    {
        var cream = PixelColor.Hex(0xF8F4D6); var shade = PixelColor.Hex(0xCDC9A4); var crater = PixelColor.Hex(0xB2AE8C);
        var craters = new (float, float, float)[] { (-3.5f, -3, 2.4f), (3, 1.5f, 3), (-1.5f, 5, 1.8f), (5, -4.5f, 1.6f), (-6.5f, 1.5f, 1.5f), (0.5f, -7, 1.4f) };
        float r = MoonRadius;
        for (int dy = -13; dy <= 13; dy++)
        {
            for (int dx = -13; dx <= 13; dx++)
            {
                float fx = dx + 0.5f, fy = dy + 0.5f;
                float d = Hypot(fx, fy);
                if (!(d <= r)) continue;
                var color = cream;
                if ((fx + fy) / (2 * r) > 0.3f + Bayer.Threshold(dx + 30, dy + 30) * 0.14f) color = shade;
                if (d > r - 1.2f) color = color.Mixed(shade, 0.45f);
                foreach (var (cx, cy, cr) in craters)
                    if (Hypot(fx - cx, fy - cy) < cr) color = crater;
                if (c.Contains(Moon.x + dx, Moon.y + dy)) c[Moon.x + dx, Moon.y + dy] = color;
            }
        }
    }

    // MARK: Distant trees

    /// <summary>A line of trees along the horizon, seen through mist. <paramref name="rise"/> is how tall the crowns get, the crown range their radius.</summary>
    public static PixelCanvas MakeTreeline(ulong seed, float rise, float crownMin, float crownMax, float haze, bool dark)
    {
        var rng = new SeededGenerator(seed);
        var field = new Heightfield(Width, Horizon + 12);
        float baseY = Horizon + 2;
        float x = -4;
        while (x < Width + 4)
        {
            float r = rng.Range(crownMin, crownMax);
            // Taller trees towards the sides, where the forest is closer; the odd giant standing above the rest.
            float side = MathF.Abs(x - 139) / 139;
            float h = rise * (0.55f + 0.6f * side) + rng.Range(-rise * 0.25f, rise * 0.35f);
            if (rng.Range(0f, 1f) < 0.07f) h += rise * 0.7f;
            field.Stamp(x, baseY - h, r, 0.9f + rng.Range(0f, 0.2f), 1);
            // Fill down to the water so the line has no gaps.
            float y = baseY - h;
            while (y < baseY) { field.Stamp(x, y, r * 0.9f, 0.85f, 1); y += r * 0.8f; }
            x += r * rng.Range(0.7f, 1.3f);
        }
        var ramp = dark
            ? new[] { PixelColor.Hex(0x0A1A26), PixelColor.Hex(0x102A3A), PixelColor.Hex(0x1A4254), PixelColor.Hex(0x2F6578) }
            : new[] { PixelColor.Hex(0x1B4560), PixelColor.Hex(0x245772), PixelColor.Hex(0x33708C), PixelColor.Hex(0x5290AA) };
        var canvas = field.Shade(s => Heightfield.Pick(ramp, s.Lit, s.X, s.Y).Mixed(HazeColor, haze), 2.0f, MoonLight);
        // The line ends at the horizon: the water starts below it.
        for (int y = Horizon; y < canvas.Height; y++)
            for (int xx = 0; xx < Width; xx++)
                canvas[xx, y] = PixelColor.Clear;
        var trimmed = new PixelCanvas(Width, Horizon);
        for (int y = 0; y < Horizon; y++)
            for (int xx = 0; xx < Width; xx++)
                trimmed[xx, y] = canvas[xx, y];
        return trimmed;
    }

    /// <summary>Mist lying along the horizon, in front of the far trees: a few flat bands, thickest at the bottom.</summary>
    public static PixelCanvas MakeHorizonMist()
    {
        var c = new PixelCanvas(Width, Horizon);
        var mist = PixelColor.Hex(0x7FB4C8);
        const int rows = 16;
        for (int y = Horizon - rows; y < Horizon; y++)
        {
            float t = (float)(y - (Horizon - rows)) / rows;
            for (int x = 0; x < Width; x++)
            {
                float level = MathF.Floor(t * 4 + (Bayer.Threshold(x, y) - 0.5f) * 0.7f) / 4;
                if (!(level > 0)) continue;
                c[x, y] = mist.WithAlpha((byte)Math.Min(255f, 150 * level));
            }
        }
        return c;
    }

    // MARK: Water

    /// <summary>
    /// What the water mirrors, compressed a little and fading with distance: the sky, the moon and the far trees.
    /// The shader ripples it.
    /// </summary>
    public static PixelCanvas MakeWaterReflection(PixelCanvas sky, IList<PixelCanvas> treelines, PixelCanvas mist)
    {
        var upper = new PixelCanvas(Width, Horizon);
        var layers = new List<PixelCanvas> { sky };
        layers.AddRange(treelines);
        layers.Add(mist);
        foreach (var layer in layers)
        {
            for (int y = 0; y < Horizon; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (layer[x, y].A == 0) continue;
                    upper[x, y] = layer[x, y].A == 255 ? layer[x, y] : upper[x, y].Mixed(layer[x, y], layer[x, y].A / 255f);
                    upper[x, y] = upper[x, y].WithAlpha(255);
                }
            }
        }
        int rows = Height - Horizon;
        var water = new PixelCanvas(Width, rows);
        var deep = PixelColor.Hex(0x050E16);
        for (int k = 0; k < rows; k++)
        {
            int source = Horizon - 1 - (int)(k * 0.78f);
            for (int x = 0; x < Width; x++)
            {
                var color = deep;
                if (source >= 0)
                {
                    // Fades to dark water near the boat, and is a little darker than the real thing.
                    float fade = MathF.Max(0, 1 - (float)k / rows * 0.8f);
                    color = deep.Mixed(upper[x, source], 0.9f * fade);
                }
                water[x, k] = color;
            }
        }
        return water;
    }

    // MARK: Near banks

    private static float WallTop(float x) => Horizon - 8 - 0.34f * MathF.Abs(x - 150);

    private sealed class Spot
    {
        public float X, Y, R, Peak;
        public Spot(float x, float y, float r, float peak) { X = x; Y = y; R = r; Peak = peak; }
    }

    private static bool CoinFlip(SeededGenerator rng) => (rng.NextULong() & 1) == 0;

    /// <summary>
    /// The jungle on both banks, close to us: layered foliage that thickens and darkens towards the foreground,
    /// giant trunks with roots reaching into the river, palms leaning over the water and hanging vines.
    /// </summary>
    public static PixelCanvas MakeBanks()
    {
        var rng = new SeededGenerator(0xB4A1);
        var field = new Heightfield(Width, Height + Spare);
        float h = Height;

        // Foliage: domes of leaves, nearer ones bigger and higher so they hide what is behind.
        var spots = new List<Spot>();
        for (int i = 0; i < 3400; i++)
        {
            bool left = CoinFlip(rng);
            float y = rng.Range(30f, h - 2);
            int yi = (int)y;
            int row = Math.Max(yi, Horizon);
            float inner = left ? RiverCentre(row) - RiverHalfWidth(row) : RiverCentre(row) + RiverHalfWidth(row);
            float x = left ? rng.Range(-6f, inner + 2) : rng.Range(inner - 2, Width + 6f);
            float r = 2.4f + (y / h) * 6.5f + rng.Range(0f, 1.6f);
            if (!(y > WallTop(x) - r * 0.4f)) continue;
            spots.Add(new Spot(x, y, r, 0.45f + (y / h) * 0.9f + rng.Range(0f, 0.25f)));
        }
        // Canopy hanging over the top corners, framing the sky.
        for (int i = 0; i < 420; i++)
        {
            bool left = CoinFlip(rng);
            float x = left ? rng.Range(-6f, 110f) : rng.Range(222f, Width + 6f);
            float reach = 34 - 0.36f * (left ? 110 - x : x - 222); // shorter towards the middle
            float y = rng.Range(-8f, MathF.Max(2, reach));
            spots.Add(new Spot(x, y, rng.Range(5f, 10f), 1.0f + rng.Range(0f, 0.3f)));
        }
        foreach (var spot in spots.OrderBy(s => s.Y))
            field.Stamp(spot.X, spot.Y, spot.R, spot.Peak, spot.Y < 40 ? 150 : spot.Y);

        // Trunks of the trees standing on the banks, with crowns; nearer foliage hides their feet.
        void Tree(float x0, float lean, float radius, float top, float bottom, float depth)
        {
            float peak = 0.55f + depth / h * 0.9f + 0.35f;
            float y = bottom;
            while (y >= top)
            {
                field.Stamp(x0 + lean * (bottom - y), y, radius, peak, 1000 + depth);
                y -= 1.2f;
            }
            float cx = x0 + lean * (bottom - top);
            for (int i = 0; i < 14; i++)
            {
                field.Stamp(cx + rng.Range(-10f, 10f), top + rng.Range(-6f, 6f),
                            rng.Range(4f, 7f), peak + 0.1f, depth);
            }
        }
        foreach (var (x, r, depth, top) in new (float, float, float, float)[]
                 { (58, 3.6f, 112, 30), (80, 2.8f, 96, 42), (44, 4.2f, 128, 24), (244, 3.8f, 114, 28), (224, 2.8f, 98, 40), (260, 4.4f, 130, 24) })
        {
            Tree(x, rng.Range(-0.05f, 0.05f), r, top, h - 2, depth);
        }
        // The two giants framing the picture, flaring at their feet into roots that dive into the river.
        void Giant(float x0, float lean, float radius, float toward)
        {
            float y = h;
            while (y > -6)
            {
                // Wider and wider towards the ground: buttresses.
                float flare = MathF.Max(0, y - 120) / (h - 120);
                field.Stamp(x0 + lean * (h - y) + toward * flare * flare * 14, y, radius + flare * flare * 12, 2.2f, 1194);
                y -= 1.0f;
            }
        }
        Giant(14, 0.03f, 11, 1);
        Giant(264, -0.025f, 10.5f, -1);

        // Palms leaning over the water.
        void Palm((float, float) baseP, (float, float) crown, int fronds, float span)
        {
            const int steps = 70;
            for (int i = 0; i <= steps; i++)
            {
                float t = (float)i / steps;
                float bow = MathF.Sin(t * MathF.PI) * 6;
                field.Stamp(baseP.Item1 + (crown.Item1 - baseP.Item1) * t + bow * (crown.Item1 > baseP.Item1 ? -1 : 1),
                            baseP.Item2 + (crown.Item2 - baseP.Item2) * t,
                            2.2f - 0.6f * t, 1.6f, 1000 + 150);
            }
            for (int f = 0; f < fronds; f++)
            {
                float angle = -MathF.PI / 2 + ((float)f / (fronds - 1) - 0.5f) * span + rng.Range(-0.12f, 0.12f);
                float len = rng.Range(20f, 30f);
                float t = 0;
                while (t <= 1)
                {
                    float x = crown.Item1 + MathF.Cos(angle) * len * t, y = crown.Item2 + MathF.Sin(angle) * len * t + 22 * t * t;
                    field.Stamp(x, y, 1.2f, 1.5f, 150);
                    // Leaflets on both sides, drooping.
                    foreach (float side in new[] { -1f, 1f })
                    {
                        if ((int)(t * 24) % 2 != 0) continue;
                        field.Stamp(x + side * 2.2f * (1 - t * 0.5f), y + 1.6f + 1.2f * t, 0.9f, 1.45f, 150);
                    }
                    t += 0.035f;
                }
            }
        }
        Palm((236, h - 36), (212, 70), 11, 3.0f);
        Palm((62, h - 44), (96, 74), 9, 2.8f);

        // Hanging vines.
        for (int i = 0; i < 26; i++)
        {
            bool left = CoinFlip(rng);
            float x = left ? rng.Range(28f, 100f) : rng.Range(190f, 252f);
            float top = MathF.Max(WallTop(x), 12) + rng.Range(0f, 20f);
            float length = rng.Range(18f, 60f);
            float d = 0;
            while (d < length)
            {
                field.Stamp(x + MathF.Sin(d * 0.2f + x) * 0.9f, top + d, 0.85f, 1.4f, 160);
                if ((int)d % 6 == 5) field.Stamp(x + ((int)d % 2 == 0 ? 1.8f : -1.8f), top + d, 1.5f, 1.4f, 160);
                d += 0.8f;
            }
        }

        // The water stays clear (with a slightly ragged shore), so the reflection shows.
        for (int y = Horizon; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                float ragged = ((x * 31 + y * 17) % 5) * 0.7f;
                if (MathF.Abs(x + 0.5f - RiverCentre(y)) < RiverHalfWidth(y) - 2 - ragged) field.Set(x, y, 0);
            }
        }

        return Cropped(field.Shade(s =>
        {
            bool trunk = s.Tag >= 1000;
            float depth = trunk ? s.Tag - 1000 : s.Tag;
            // Backlit: faces turned away from the moon are nearly black; only rims catch the light.
            float lit = s.Lit - (trunk ? 0.38f : 0.22f) - (s.Edge && !s.Rim ? 0.3f : 0);
            float haze = MathF.Max(0, MathF.Min(1, (200 - depth) / 170)) * 0.5f;
            var color = Heightfield.Pick(trunk ? TrunkRamp : JungleRamp, lit, s.X, s.Y);
            if (s.Top && s.Lit > 0.5f && (s.X + s.Y * 2) % 3 == 0) color = trunk ? TrunkRamp[5] : JungleRamp[6]; // moonlit rim
            return color.Mixed(HazeColor, haze);
        }, 2.2f, MoonLight));
    }

    // MARK: Foreground

    /// <summary>Where the middle of the lantern is, in grid pixels. It hangs from a crook on the left gunwale.</summary>
    public static readonly (int x, int y) Lantern = (100, 136);

    private static readonly PixelColor[] WoodRamp =
    {
        PixelColor.Hex(0x0E0806), PixelColor.Hex(0x24150E), PixelColor.Hex(0x42281A),
        PixelColor.Hex(0x6C4328), PixelColor.Hex(0x9A6638), PixelColor.Hex(0xC8924E), PixelColor.Hex(0xF0C77C),
    };

    /// <summary>
    /// The bow of our boat, seen from the stern: two gunwales converge towards a curled prow, with the dark floor
    /// planks between them and a seat across. The lantern's warm light falls on the wood from the left.
    /// </summary>
    public static PixelCanvas MakeBow()
    {
        var field = new Heightfield(Width, Height + Spare);
        float tipX = 140, tipY = 158;
        void Gunwale(float side)
        {
            float t = 0;
            while (t <= 1)
            {
                // From the corner of the screen, curving in to the prow.
                float x = tipX + side * (62 * MathF.Pow(1 - t, 1.25f) + 1.5f);
                float y = tipY + (Height - tipY + 10) * MathF.Pow(1 - t, 1.05f);
                field.Stamp(x, y, 3.4f + 2.2f * (1 - t), 1.7f, 1);
                t += 0.004f;
            }
        }
        Gunwale(-1);
        Gunwale(1);
        // The floor, sunk below the rails, with planks running towards the prow.
        for (int y = (int)(tipY + 2); y < Height; y++)
        {
            float t = (float)(y - (int)tipY) / (Height - (int)tipY);
            float half = 4 + 56 * t;
            for (int x = (int)(tipX - half); x <= (int)(tipX + half); x++)
            {
                bool plank = (int)((x - (int)tipX) / MathF.Max(2, half) * 5 + 40) % 2 == 0;
                field.Raise(x, y, plank ? 0.62f : 0.5f, 0);
            }
        }
        // A seat (thwart) across the boat.
        for (int y = 176; y <= 179; y++)
        {
            float t = (float)(y - (int)tipY) / (Height - (int)tipY);
            float half = 4 + 56 * t;
            float x = tipX - half;
            while (x <= tipX + half)
            {
                field.Stamp(x, y + 0.5f, 2.2f, 1.45f, 1);
                x += 1;
            }
        }
        // The prow: a post that rises and curls, with a small carved head.
        for (int i = 0; i <= 26; i++)
        {
            float t = i / 26f;
            field.Stamp(tipX + MathF.Sin(t * 2.6f) * 3.2f, tipY - 2 - t * 13, 2.4f - t * 0.8f, 1.8f, 1);
        }
        field.Stamp(tipX + 3.4f, tipY - 17, 2.4f, 1.9f, 1);
        // The lantern's post: a crook of wood that rises from the left gunwale, clamped to it by an iron band, and bends over
        // to hold the lantern by a chain.
        for (int i = 0; i <= 90; i++)
        {
            float t = i / 90f;
            var (px, py) = Crook(t);
            field.Stamp(px, py, 2.3f - 0.9f * t, 2.0f, 2);
        }
        for (int i = 0; i <= 8; i++) field.Stamp(Crook(0.05f).x + (i % 2) * 0.01f, Crook(0.05f).y - 3 + i * 0.75f, 3.1f, 2.3f, 3); // the iron band
        float lx = Lantern.x, ly = Lantern.y;
        var bow = Cropped(field.Shade(s =>
        {
            if (s.Edge && !s.Rim) return WoodRamp[0];
            float d = Hypot(s.X - lx, s.Y - ly);
            float glow = MathF.Pow(MathF.Max(0, 1 - d / 95), 1.5f);
            bool floor = s.Tag == 0, pole = s.Tag == 2, iron = s.Tag == 3;
            if (iron) return Heightfield.Pick(SidePillarArt.Stone, s.Lit * 0.8f - 0.1f + glow * 0.5f, s.X, s.Y);
            var color = Heightfield.Pick(WoodRamp, s.Lit * 0.75f + glow * (pole ? 0.7f : 1.1f) - (floor ? 0.55f : (pole ? 0.4f : 0.28f)), s.X, s.Y);
            if (s.Top && !pole && glow > 0.2f && (s.X + s.Y * 2) % 3 == 0) color = WoodRamp[6]; // lantern-lit edges
            return color;
        }, 2.4f, (-0.4f, -0.5f, 0.7f)));
        DrawChain(bow);
        DrawPostVines(bow);
        return bow;
    }

    /// <summary>A point of the lantern's post: a cubic curve from the gunwale (t = 0) up and over to the hook (t = 1).</summary>
    private static (float x, float y) Crook(float t)
    {
        (float x, float y) p0 = (92, 190), p1 = (80, 126), p2 = (90, 96), p3 = (Lantern.x, 102);
        float u = 1 - t;
        return (u * u * u * p0.x + 3 * u * u * t * p1.x + 3 * u * t * t * p2.x + t * t * t * p3.x,
                u * u * u * p0.y + 3 * u * u * t * p1.y + 3 * u * t * t * p2.y + t * t * t * p3.y);
    }

    /// <summary>The chain from the end of the crook to the ring of the lantern: links, dark and pale by turns.</summary>
    private static void DrawChain(PixelCanvas c)
    {
        var (hx, hy) = Crook(1);
        int top = (int)hy + 1, bottom = Lantern.y - 16;
        for (int y = top; y <= bottom; y++)
        {
            int x = (int)MathF.Round(hx);
            var link = (y - top) % 3 == 2 ? SidePillarArt.Stone[3] : SidePillarArt.Stone[1];
            if (c.Contains(x, y)) c[x, y] = link;
            if (c.Contains(x + 1, y)) c[x + 1, y] = (y - top) % 3 == 1 ? SidePillarArt.Stone[2] : SidePillarArt.Out;
            if (c.Contains(x - 1, y) && (y - top) % 3 != 1) c[x - 1, y] = SidePillarArt.Out;
        }
    }

    /// <summary>The same vines as on the pillars wind round the post, with their leaves and little orange flowers.</summary>
    private static void DrawPostVines(PixelCanvas c)
    {
        var leaf = SidePillarArt.Leaf; var flower = SidePillarArt.Flower;
        int n = 0;
        for (float t = 0.08f; t <= 0.96f; t += 0.006f, n++)
        {
            var (x, y) = Crook(t);
            var (x2, y2) = Crook(t + 0.01f);
            float dx = x2 - x, dy = y2 - y; float len = MathF.Max(0.001f, MathF.Sqrt(dx * dx + dy * dy));
            float nx = -dy / len, ny = dx / len;
            float off = MathF.Sin(t * 38) * 2.6f;
            bool front = MathF.Cos(t * 38) > 0;
            int vx = (int)MathF.Round(x + nx * off), vy = (int)MathF.Round(y + ny * off);
            if (front && c.Contains(vx, vy)) c[vx, vy] = n % 3 == 0 ? leaf[3] : leaf[2];
            if (front && c.Contains(vx + 1, vy)) c[vx + 1, vy] = leaf[1];
            if (n % 9 == 4 && c.Contains(vx, vy))
            {
                int dir = n % 2 == 0 ? -1 : 1;
                foreach (var (ox, oy, k) in new[] { (dir, 0, 2), (2 * dir, -1, 3), (3 * dir, -1, 4), (2 * dir, 1, 1) })
                    if (c.Contains(vx + ox, vy + oy)) c[vx + ox, vy + oy] = leaf[k];
            }
            if (n % 29 == 14 && c.Contains(vx, vy))
            {
                foreach (var (ox, oy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }) if (c.Contains(vx + ox, vy + oy)) c[vx + ox, vy + oy] = flower[1];
                c[vx, vy] = flower[3];
            }
        }
    }

    public const int LanternWidth = 21, LanternHeight = 32;

    /// <summary>
    /// The lantern (21 x 32): a ring, a roof of stepped stone like the pillars' capitals, a frame of carved stone round a glass full of
    /// flame, and a stepped foot with a chakana cut in it; a little green creeps over the roof. Two frames, so the flame flickers.
    /// </summary>
    public static PixelCanvas MakeLantern(int frame)
    {
        var c = new PixelCanvas(LanternWidth, LanternHeight);
        var stone = SidePillarArt.Stone; var leaf = SidePillarArt.Leaf; var flower = SidePillarArt.Flower;
        var ink = SidePillarArt.Out;
        var warm = PixelColor.Hex(0xFF9A1F);

        void Block(int x0, int x1, int y0, int y1)
        {
            for (int y = y0; y <= y1; y++)
                for (int x = x0; x <= x1; x++)
                {
                    int t = 2;
                    if (y == y0 || x == x0) t = 3;
                    if (y == y1 || x == x1) t = 1;
                    if (y == y0 && x == x0) t = 4;
                    c[x, y] = stone[t];
                }
        }

        // The ring it hangs by.
        foreach (var (x, y) in new[] { (9, 0), (10, 0), (11, 0), (8, 1), (12, 1), (8, 2), (12, 2), (9, 3), (11, 3) }) c[x, y] = stone[3];
        c[10, 3] = stone[1];
        // The roof: five steps, each wider than the last.
        int[] widths = { 5, 9, 13, 17, 21 };
        for (int k = 0; k < 5; k++)
        {
            int half = widths[k] / 2;
            Block(10 - half, 10 + half, 4 + 2 * k, 5 + 2 * k);
        }
        // The frame: a post at each side, a beam above and below the glass.
        Block(1, 3, 14, 27); Block(17, 19, 14, 27);
        Block(1, 19, 14, 15); Block(1, 19, 26, 27);
        // The glass: dark at the edges and warm towards the flame.
        for (int y = 16; y <= 25; y++)
            for (int x = 4; x <= 16; x++)
            {
                float dx = (x - 10) / 6.5f, dy = (y - 22) / 6.5f;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                c[x, y] = d < 0.45f ? PixelColor.Hex(0x7A3A14) : PixelColor.Hex(0x4A2410);
            }
        // The flame: a teardrop, wider at the foot, that sways between the frames.
        int lift = frame == 0 ? 0 : 1, sway = frame == 0 ? 0 : 1;
        for (int y = 17 - lift; y <= 25; y++)
        {
            float v = (y - (17 - lift)) / (25f - (17 - lift));          // 0 at the tip ... 1 at the foot
            float half = 1 + 4.2f * MathF.Sin(MathF.Min(1, v * 1.15f) * MathF.PI / 2) * (v < 0.9f ? 1 : 0.8f);
            float centre = 10 + sway * (1 - v) * 1.2f;
            for (int x = 4; x <= 16; x++)
            {
                float d = MathF.Abs(x + 0.5f - centre - 0.5f) / half;
                if (d >= 1) continue;
                c[x, y] = d > 0.75f ? warm : (d > 0.45f || v < 0.3f ? PixelColor.Hex(0xFFD66B) : (d > 0.2f ? PixelColor.Hex(0xFFF1B0) : PixelColor.Hex(0xFFFFE8)));
            }
        }
        // The inner sides of the posts catch the flame's light.
        for (int y = 16; y <= 25; y++) { c[3, y] = PixelColor.Hex(0xB8743A); c[17, y] = PixelColor.Hex(0x9A5A2E); }
        // The foot: two steps, with a chakana cut in the first.
        Block(2, 18, 28, 29); Block(5, 15, 30, 31);
        foreach (var (x, y) in new[] { (10, 28), (9, 29), (10, 29), (11, 29), (10, 30) }) c[x, y] = stone[0];
        // An outline round everything.
        PixelPaint.Outline(c, ink);
        // Green creeping over the roof and the left post, and a flower.
        foreach (var (x, y, k) in new[] { (3, 13, 2), (4, 13, 3), (5, 12, 2), (2, 12, 1), (1, 13, 2), (0, 13, 1), (3, 14, 3), (2, 15, 2), (1, 16, 1), (2, 17, 2), (3, 18, 1), (6, 11, 2), (7, 10, 3) })
            if (c.Contains(x, y)) c[x, y] = leaf[k];
        foreach (var (x, y) in new[] { (17, 12), (18, 12) }) c[x, y] = leaf[2];
        foreach (var (ox, oy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) }) c[18 + ox, 11 + oy] = flower[1];
        c[18, 11] = flower[3];
        return c;
    }

    /// <summary>A tiny glowing speck for fireflies, in two brightnesses.</summary>
    public static PixelCanvas MakeFirefly(bool bright)
    {
        var c = new PixelCanvas(5, 5);
        var core = PixelColor.Hex(bright ? 0xFFFFC0u : 0xE6F59Au);
        c[2, 2] = core;
        foreach (var (x, y) in new[] { (1, 2), (3, 2), (2, 1), (2, 3) })
            c[x, y] = PixelColor.Hex(0xB6E24A).WithAlpha((byte)(bright ? 150 : 80));
        return c;
    }
}
