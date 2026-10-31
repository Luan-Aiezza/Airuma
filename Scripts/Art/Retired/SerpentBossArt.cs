using System;
using P = PixelColor;

/// <summary>
/// The Iryssh's second form: a huge dark-red serpent, of which only the head shows: a smooth, heavy body (a little narrower than the head) in
/// faint oval scales, which swells without a seam into the head; a real snake's head (a hooded skull narrowing to a rounded snout, a forked
/// tongue) with three closed eyes, one in the middle of the forehead and one either side, almond lids with no pupil to be seen
/// and a slit of fire between them, from which flames pour. It has little detail on purpose: it is wrapped in red flames, which are not painted here but made
/// by a shader (see <see cref="SerpentFlame"/>) from this art's silhouette. Drawn on the finer grid (2 screen pixels per art
/// pixel) like the woman. Seen from above, facing the player.
///
/// There are two variants: in the first attack the neck rises out of the water (it fades into a ripple ring);
/// in the charge the neck runs off the top of the sprite, where the body (SerpentBody) attaches. The body is one long
/// image of the same rings, as thick as the neck.
/// </summary>
public static class SerpentBossArt
{
    public const int Width = 112;
    public const int Height = 168;
    /// <summary>Art pixel that is the boss's position: the middle of the head (and its hitbox).</summary>
    public static readonly (int x, int y) Centre = (56, 68);

    // MARK: Palettes (shadow ... highlight, each with its own dark outline)

    private readonly struct Pal
    {
        public readonly P Out, Dark, Base, Light, Hi;
        public Pal(uint o, uint d, uint b, uint l, uint h) { Out = P.Hex(o); Dark = P.Hex(d); Base = P.Hex(b); Light = P.Hex(l); Hi = P.Hex(h); }
    }

    private static readonly Pal Head = new Pal(0x1A0508, 0x3A0C14, 0x5E1420, 0x8A2228, 0xB8403A);
    private static readonly Pal Lid = new Pal(0x1A0508, 0x7A1A20, 0xB8403A, 0xE0705A, 0xF8B090);
    private static readonly Pal Tongue = new Pal(0x2A0A10, 0xC42A12, 0xF25C12, 0xFF9A1F, 0xFFD23F);
    private static readonly P Ember = P.Hex(0xFF9A1F), EmberHot = P.Hex(0xFFD23F), Ink = P.Hex(0x0A0204);

    /// <summary>Margin (art pixels) added round the silhouette for the flames to burn in.</summary>
    public const int FlamePad = 44;

    /// <summary>
    /// What the flame shader burns round: the art's silhouette on a larger transparent canvas (alpha), and in the red channel
    /// the pixels that give out flames of their own (the slits of the eyes). Where the art runs off the sprite (into the body, or
    /// under the head) it goes on in the margin, so no flames burn on the cut.
    /// </summary>
    public static PixelCanvas Padded(PixelCanvas art, bool continueTop = false, bool continueBottom = false, Func<int, int, bool> emitter = null)
    {
        var big = new PixelCanvas(art.Width + 2 * FlamePad, art.Height + 2 * FlamePad);
        for (int y = 0; y < art.Height; y++)
            for (int x = 0; x < art.Width; x++)
                if (art[x, y].A > 0) big[x + FlamePad, y + FlamePad] = emitter != null && emitter(x, y) ? new P(255, 0, 0, art[x, y].A) : new P(0, 0, 0, art[x, y].A);
        for (int i = 0; i < FlamePad; i++)
            for (int x = 0; x < art.Width; x++)
            {
                if (continueTop && art[x, 0].A > 0) big[x + FlamePad, i] = new P(0, 0, 0, art[x, 0].A);
                if (continueBottom && art[x, art.Height - 1].A > 0) big[x + FlamePad, FlamePad + art.Height + i] = new P(0, 0, 0, art[x, art.Height - 1].A);
            }
        return big;
    }

    private const float NeckHalf = 28;

    // MARK: Piece machinery

    private static bool[,] MaskOf(int w, int h, Func<float, float, bool> inside, float ox, float oy)
    {
        var m = new bool[w, h];
        for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) m[x, y] = inside(x + 0.5f - ox, y + 0.5f - oy);
        return m;
    }

    /// <summary>
    /// Paints a piece: a dark outline, then flat bands from the upper left (highlight, light) to the lower right (shadow).
    /// `openTop` treats what is above the canvas as part of the piece (no outline across the top).
    /// </summary>
    private static void Fill(PixelCanvas c, bool[,] m, Pal pal, bool openTop = false, Func<int, int, P, P> detail = null)
    {
        int w = c.Width, h = c.Height;
        var L = new int[w, h]; var R = new int[w, h]; var U = new int[w, h]; var D = new int[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!m[x, y]) continue;
                L[x, y] = x > 0 && m[x - 1, y] ? L[x - 1, y] + 1 : 1;
                U[x, y] = y > 0 ? (m[x, y - 1] ? U[x, y - 1] + 1 : 1) : (openTop ? 99 : 1);
            }
        for (int y = h - 1; y >= 0; y--)
            for (int x = w - 1; x >= 0; x--)
            {
                if (!m[x, y]) continue;
                R[x, y] = x < w - 1 && m[x + 1, y] ? R[x + 1, y] + 1 : 1;
                D[x, y] = y < h - 1 && m[x, y + 1] ? D[x, y + 1] + 1 : 1;
            }
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                if (!m[x, y]) continue;
                int l = L[x, y], r = R[x, y], u = U[x, y], d = D[x, y];
                P color;
                if (l == 1 || r == 1 || u == 1 || d == 1) color = pal.Out;
                else
                {
                    int a = Math.Min(l, u), b = Math.Min(r, d);
                    if (a < b) color = a <= 3 ? pal.Hi : (a <= 5 ? pal.Light : pal.Base);
                    else if (b < a) color = b <= 4 ? pal.Dark : pal.Base;
                    else color = pal.Base;
                    if (detail != null) color = detail(x, y, color);
                }
                c[x, y] = color;
            }
    }

    private static void Put(PixelCanvas c, float u, float v, P color) =>
        BossArt.Put(c, (int)MathF.Floor(Centre.x + u), (int)MathF.Floor(Centre.y + v), color);

    // MARK: Head

    private static readonly (float v, float hw)[] SkullEdge =
    {
        (-30, 28), (-20, 28.5f), (-10, 31), (-2, 34), (6, 34.5f), (18, 32), (30, 27), (42, 21), (52, 16), (60, 11), (66, 5), (69, 0),
    };

    private static float SkullHalf(float v)
    {
        if (v <= SkullEdge[0].v) return NeckHalf;
        if (v >= SkullEdge[^1].v) return 0;
        for (int i = 1; i < SkullEdge.Length; i++)
        {
            if (v > SkullEdge[i].v) continue;
            float t = (v - SkullEdge[i - 1].v) / (SkullEdge[i].v - SkullEdge[i - 1].v);
            return SkullEdge[i - 1].hw + (SkullEdge[i].hw - SkullEdge[i - 1].hw) * t;
        }
        return 0;
    }

    /// <summary>The three closed eyes: (centre u, centre v, half-length, half-thickness, angle of the long axis). The first is in the middle of the forehead.</summary>
    private static readonly (float u, float v, float rx, float ry, float angle)[] Eyes =
    {
        (0, 4, 11, 4.6f, MathF.PI / 2),
        (17, 31, 11, 4.4f, -0.35f),
        (-17, 31, 11, 4.4f, MathF.PI + 0.35f),
    };

    /// <summary>How deep inside an eye the point is: above 0 inside the lid (an almond), at 1 in the middle of its length.</summary>
    private static float Almond((float u, float v, float rx, float ry, float angle) e, float u, float v)
    {
        float du = u - e.u, dv = v - e.v;
        float a = du * MathF.Cos(e.angle) + dv * MathF.Sin(e.angle), b = -du * MathF.Sin(e.angle) + dv * MathF.Cos(e.angle);
        float q = a / e.rx;
        if (MathF.Abs(q) >= 1) return -1;
        return e.ry * (1 - q * q) - MathF.Abs(b);
    }

    private static bool InSlit(float u, float v)
    {
        foreach (var e in Eyes)
        {
            float du = u - e.u, dv = v - e.v;
            float a = du * MathF.Cos(e.angle) + dv * MathF.Sin(e.angle), b = -du * MathF.Sin(e.angle) + dv * MathF.Cos(e.angle);
            float q = a / e.rx;
            if (MathF.Abs(q) < 0.92f && MathF.Abs(b) < 0.8f * (1 - q * q) + 0.25f) return true;
        }
        return false;
    }

    /// <summary>Whether this pixel of the head sprite gives out flames of its own (the slits of the eyes).</summary>
    public static bool IsEmitter(int x, int y) => InSlit(x + 0.5f - Centre.x, y + 0.5f - Centre.y);

    public static PixelCanvas MakeHead(int frame, bool charging)
    {
        var canvas = new PixelCanvas(Width, Height);
        int reach = new[] { 0, 2, 6, 2 }[frame];

        if (!charging) BossArt.DrawRipples(canvas, Centre.x, 8, 50, 10, frame, false);

        // The forked tongue, flickering out of the snout (under the head).
        DrawTongue(canvas, 64, reach);

        // The neck and the head are one piece: the neck swells into a hooded skull that narrows to a rounded snout, all
        // in the same faint oval scales as the body.
        Fill(canvas, MaskOf(Width, Height, (u, v) => MathF.Abs(u) < SkullHalf(v), Centre.x, Centre.y), Head, charging,
            (x, y, col) => OvalScales(col, x + 0.5f - Centre.x, Centre.y - y));

        // The eyes: closed, almond lids with a slit of fire between them.
        foreach (var eye in Eyes)
        {
            var e = eye;
            Fill(canvas, MaskOf(Width, Height, (u, v) => Almond(e, u, v) > 0, Centre.x, Centre.y), Lid, false, (x, y, col) =>
                InSlit(x + 0.5f - Centre.x, y + 0.5f - Centre.y) ? (frame % 2 == 0 ? EmberHot : Ember) : col);
        }

        // Nostrils on the snout, each with an ember in it.
        foreach (float s in new[] { -1f, 1f })
        {
            Put(canvas, s * 5, 59, Ink); Put(canvas, s * 5 + 1, 59, Ink); Put(canvas, s * 5, 60, frame % 2 == 0 ? Ember : Ink); Put(canvas, s * 5 + 1, 60, Ink);
        }

        if (!charging)
        {
            // The neck rises out of the water: fade it in at the top, and spread the near half of the ripples.
            for (int y = 0; y < 18; y++)
            {
                float p = (18 - y) / 18f;
                for (int x = Centre.x - 40; x < Centre.x + 40; x++) if (Bayer.Threshold(x, y) < p && canvas[x, y].A == 255) canvas[x, y] = P.Clear;
            }
            BossArt.DrawRipples(canvas, Centre.x, 8, 50, 10, frame, true);
        }
        return canvas;
    }

    private static void DrawTongue(PixelCanvas c, int top, int reach)
    {
        int tip = top + 12 + reach;
        var m = MaskOf(c.Width, c.Height, (u, v) =>
        {
            if (MathF.Abs(u) < 2.2f && v >= top && v < tip) return true;
            for (int k = 0; k < 9; k++)
            {
                float fv = tip + k, fu = 1 + k * 0.9f;
                if (v >= fv && v < fv + 3 && MathF.Abs(MathF.Abs(u) - fu) < 2f) return true;
            }
            return false;
        }, Centre.x, Centre.y);
        Fill(c, m, Tongue);
    }

    // MARK: Skin

    /// <summary>
    /// The faint rows of oval scales shared by the neck, the head and the body, staggered, lighter than the skin round them.
    /// `u` is across (0 at the middle), `d` the distance in rows from the head's centre along the animal, so the pattern runs
    /// on unbroken from the head into the body.
    /// </summary>
    private static P OvalScales(P col, float u, float d)
    {
        if (col != Head.Base && col != Head.Dark) return col;
        const float W = 11, H = 9;
        int row = (int)MathF.Floor(d / H);
        float shift = row % 2 == 0 ? 0 : W / 2;
        float cu = (MathF.Floor((u + shift) / W) + 0.5f) * W - shift;
        float cd = (row + 0.5f) * H;
        float e = (u - cu) / 3.4f, f = (d - cd) / 2.2f;
        if (e * e + f * f >= 1) return col;
        return col == Head.Base ? Head.Light : Head.Base;
    }

    // MARK: Body

    /// <summary>
    /// The body is one long image of the same rings as the neck, as thick as it: a heavy tube that leaves the head and
    /// relaxes into the S-curves of a worm lying on the water, thinning towards a blunt tail. Row 0 is the tail, the last row
    /// meets the head.
    /// </summary>
    public const int BodyWidth = 96;
    public const int BodyLength = 440;

    /// <summary>Centre of the body, in columns, d rows away from the head.</summary>
    public static float BodyCentre(int d)
    {
        float relax = MathF.Min(1, d / 120f);
        return BodyWidth / 2f + 10f * relax * relax * (3 - 2 * relax) * MathF.Sin(d * 0.0375f);
    }

    /// <summary>Half-width of the body d rows away from the head; zero beyond the tail tip.</summary>
    public static float BodyHalfWidth(int d)
    {
        float t = d / (float)BodyLength;
        float hw = NeckHalf - 10f * MathF.Pow(t, 1.4f);
        int tip = BodyLength - 40;
        if (d > tip) hw *= MathF.Sqrt(MathF.Max(0, 1 - MathF.Pow((d - tip) / 40f, 2)));
        return hw;
    }

    public static PixelCanvas MakeBody()
    {
        var canvas = new PixelCanvas(BodyWidth, BodyLength);
        var m = new bool[BodyWidth, BodyLength];
        for (int y = 0; y < BodyLength; y++)
        {
            int d = BodyLength - 1 - y;
            float hw = BodyHalfWidth(d), centre = BodyCentre(d);
            for (int x = 0; x < BodyWidth; x++) m[x, y] = hw > 0 && MathF.Abs(x + 0.5f - centre) < hw;
        }
        Fill(canvas, m, Head, false, (x, y, col) => OvalScales(col, x + 0.5f - BodyCentre(BodyLength - 1 - y), BodyLength - 1 - y));
        return canvas;
    }
}
