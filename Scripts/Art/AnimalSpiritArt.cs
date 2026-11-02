using P = PixelColor;

/// <summary>
/// The common enemies of stage 3, the lake in the black forest: small spirits of animals, in the shape of the spirits of the forest (a round,
/// pale head on a tail of mist), each with the features of its animal and eyes that glow in the dark:
///   Swarm  the rat: round ears, red bead eyes, a pink nose with whiskers, a thin tail that wriggles;
///   Gunner the owl: ear tufts, a facial disc with two great yellow eyes, a beak and folded wings;
///   Heavy  the rabbit: long ears (pink inside) that twitch, angry red eyes, a pink nose and fluffy cheeks.
/// </summary>
public static class AnimalSpiritArt
{
    private static readonly P Eye = P.Hex(0x1C1228), Glint = P.Hex(0xFFFFFF);

    public static SpiritArt.Palette PaletteFor(SpiritKind kind) => kind switch
    {
        SpiritKind.Swarm => new SpiritArt.Palette(P.Hex(0xD8D4E4), P.Hex(0xF0EEF8), P.Hex(0x8C84A8), P.Hex(0xA8C8FF), P.Hex(0xE89AB4), P.Hex(0xA8607A)),
        SpiritKind.Gunner => new SpiritArt.Palette(P.Hex(0xEFE8D8), P.Hex(0xFFFAEA), P.Hex(0xA89C84), P.Hex(0xFFE08A), P.Hex(0x8A6A48), P.Hex(0x5A4430)),
        _ => new SpiritArt.Palette(P.Hex(0xF4EEF4), P.Hex(0xFFFFFF), P.Hex(0xB0A0BC), P.Hex(0xFF8A9A), P.Hex(0xF8A8C0), P.Hex(0xC87090)),
    };

    public static PixelColor SparkColour(SpiritKind kind) => PaletteFor(kind).Sprout;

    public static PixelCanvas MakeCanvas(SpiritKind kind, int frame) => SpiritArt.Build(PaletteFor(kind), frame, kind switch
    {
        SpiritKind.Swarm => Rat,
        SpiritKind.Gunner => Owl,
        _ => Rabbit,
    }, null);

    private static void Rat(SpiritArt.SpiritBrush b, SpiritArt.Palette p, int frame)
    {
        // Round ears at the sides of the head, pink inside.
        foreach (int y in new[] { 2, 3 }) { b.Sym(4, y, p.Light); b.Sym(5, y, p.Body); }
        b.Sym(3, 4, p.Body); b.Sym(4, 4, p.Light); b.Sym(5, 4, p.Body);
        b.Sym(4, 3, p.Tunic); b.Sym(4, 2, p.Tunic.Mixed(p.Body, 0.4f));
        // Red bead eyes, a pink nose, whiskers and a little mouth.
        b.Sym(2, 8, P.Hex(0xFF5A4A)); b.Sym(3, 8, Eye); b.Sym(2, 7, Eye.Mixed(p.Body, 0.3f));
        b.Put(6, 10, p.Tunic); b.Put(7, 10, p.Tunic); b.Put(6, 11, Eye.Mixed(p.Body, 0.4f)); b.Put(7, 11, Eye.Mixed(p.Body, 0.4f));
        b.Sym(4, 10, p.Light); b.Sym(5, 9, p.Light); b.Sym(5, 11, p.Light);
        // A thin tail that wriggles at the side.
        int wriggle = frame % 3 == 1 ? 1 : (frame % 3 == 2 ? -1 : 0);
        b.Put(11, 13, p.TunicDark); b.Put(12, 14 + wriggle, p.TunicDark); b.Put(13, 13 + wriggle, p.TunicDark); b.Put(13, 12 + wriggle, p.TunicDark);
    }

    private static void Owl(SpiritArt.SpiritBrush b, SpiritArt.Palette p, int frame)
    {
        // Ear tufts.
        b.Sym(3, 3, p.Dark); b.Sym(3, 2, p.Dark); b.Sym(4, 3, p.TunicDark); b.Sym(2, 3, p.Dark.Mixed(p.Body, 0.4f));
        // The facial disc, and two great yellow eyes with dark pupils.
        for (int y = 6; y <= 10; y++) for (int d = 1; d <= 4; d++) b.Sym(d, y, y == 6 || y == 10 || d == 4 ? p.Tunic.Mixed(p.Body, 0.5f) : p.Tunic);
        foreach (int y in new[] { 7, 8, 9 }) for (int d = 2; d <= 3; d++) b.Sym(d, y, P.Hex(0xFFD24A));
        b.Sym(2, 8, Eye); b.Sym(3, 7, Glint);
        // The beak, between the eyes.
        b.Put(6, 9, P.Hex(0xE89A2A)); b.Put(7, 9, P.Hex(0xE89A2A)); b.Put(6, 10, P.Hex(0xB86A1A)); b.Put(7, 10, P.Hex(0xB86A1A));
        // Folded wings along the body, with feathers; they flutter a little.
        int flutter = frame % 3 == 1 ? 1 : 0;
        b.Sym(5, 11, p.TunicDark); b.Sym(5, 12, p.TunicDark); b.Sym(4, 13 + flutter, p.TunicDark); b.Sym(5, 13, p.Tunic);
        b.Sym(1, 12, p.Dark); b.Sym(0, 12, p.Dark.Mixed(p.Body, 0.5f));
    }

    private static void Rabbit(SpiritArt.SpiritBrush b, SpiritArt.Palette p, int frame)
    {
        // Long ears, pink inside, that twitch.
        int lean = frame % 3 == 1 ? 1 : (frame % 3 == 2 ? -1 : 0);
        for (int y = 0; y <= 3; y++)
        {
            b.Put(9 + lean * (3 - y) / 3, y, p.Light); b.Put(10 + lean * (3 - y) / 3, y, p.Body);
            b.Put(4 - lean * (3 - y) / 3, y, p.Light); b.Put(3 - lean * (3 - y) / 3, y, p.Body);
            if (y >= 1) { b.Put(9 + lean * (3 - y) / 3, y, p.Tunic); b.Put(4 - lean * (3 - y) / 3, y, p.Tunic); }
        }
        // Angry red eyes under heavy brows, a pink nose, a small mouth, fluffy cheeks.
        b.Sym(2, 8, P.Hex(0xFF3A4A)); b.Sym(3, 8, P.Hex(0xC8283A)); b.Sym(2, 7, Eye); b.Sym(3, 7, Eye.Mixed(p.Body, 0.3f)); b.Sym(2, 9, p.Dark);
        b.Sym(2, 8, P.Hex(0xFF3A4A));
        b.Put(6, 10, p.Tunic); b.Put(7, 10, p.Tunic); b.Put(6, 11, Eye); b.Put(7, 11, Eye); b.Put(5, 12, Eye.Mixed(p.Body, 0.5f)); b.Put(8, 12, Eye.Mixed(p.Body, 0.5f));
        b.Sym(5, 10, p.Light); b.Sym(5, 11, p.Light); b.Sym(4, 11, p.Body);
    }
}
