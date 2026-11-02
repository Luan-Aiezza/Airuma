using System;
using System.IO;
using System.Text;
using Godot;

/// <summary>
/// The tools of the developer, run from the command line instead of the game: they render the generated art, sounds and music to files, to look at
/// and to listen to outside the game. `Run` does the one that was asked for (see <see cref="LaunchOptions"/>) and says whether there was one.
/// </summary>
public static class DevTools
{
    /// <summary>Runs the export asked for in the options, if any; true if one was run (the game then quits).</summary>
    public static bool Run(LaunchOptions options)
    {
        if (options.Value("--export-sfx") is string sfx) ExportSfx(sfx);
        else if (options.Value("--export-music") is string music) ExportMusic(music);
        else if (options.Value("--export-icon") is string icon) IconArt.MakeImage().SavePng(icon);
        else if (options.Value("--export-pillar") is string pillar) ExportPillar(pillar);
        else if (options.Value("--export-baylo") is string baylo) ExportBoss(baylo, BayloArt.MakeCanvas, BayloArt.Width, BayloArt.Height, 6);
        else if (options.Value("--export-thaqai") is string thaqai) ExportBoss(thaqai, ThaqaiArt.MakeCanvas, ThaqaiArt.Width, ThaqaiArt.Height, 6);
        else if (options.Value("--export-ukhumo") is string ukhumo) ExportBoss(ukhumo, UkhumoArt.MakeCanvas, UkhumoArt.Width, UkhumoArt.Height, 6);
        else if (options.Value("--export-kuychu") is string kuychu) ExportKuychu(kuychu);
        else if (options.Value("--export-iryssh") is string iryssh) ExportBoss(iryssh, IrysshWomanArt.MakeCanvas, IrysshWomanArt.Width, IrysshWomanArt.Height, 6);
        else if (options.Value("--export-retired") is string retired) ExportRetired(retired);
        else if (options.Value("--export-cards") is string cards) ExportCards(cards);
        else if (options.Value("--export-spirits") is string spirits) ExportSpirits(spirits);
        else return false;
        return true;
    }

    /// <summary>The four frames of a boss side by side, `k` times larger, on dark water.</summary>
    private static void ExportBoss(string file, Func<int, PixelCanvas> frame, int width, int height, int k)
    {
        var sheet = Image.CreateEmpty(width * k * BossStyle.FrameCount + 50, height * k + 20, false, Image.Format.Rgba8);
        sheet.Fill(new Color(0.05f, 0.06f, 0.16f));
        for (int f = 0; f < BossStyle.FrameCount; f++) Blit(sheet, frame(f), 10 + f * (width * k + 10), 10, k);
        sheet.SavePng(file);
    }

    private static void Blit(Image sheet, PixelCanvas canvas, int x, int y, int k)
    {
        var img = canvas.ToImage();
        img.Resize(img.GetWidth() * k, img.GetHeight() * k, Image.Interpolation.Nearest);
        sheet.BlendRect(img, new Rect2I(0, 0, img.GetWidth(), img.GetHeight()), new Vector2I(x, y));
    }

    /// <summary>The faces of an ordinary card (Fervor) and of the three special ones, 2x, side by side.</summary>
    private static void ExportCards(string file)
    {
        var shown = new[] { Card.Fervor, Card.SerpentFlame, Card.PartyFever, Card.StormBolt };
        const int k = 2, gap = 12;
        var sheet = Image.CreateEmpty((CardArt.Width * k + gap) * shown.Length + gap, CardArt.Height * k + 2 * gap, false, Image.Format.Rgba8);
        sheet.Fill(new Color(0.05f, 0.06f, 0.16f));
        for (int i = 0; i < shown.Length; i++) Blit(sheet, CardArt.Face(shown[i]), gap + i * (CardArt.Width * k + gap), gap, k);
        sheet.SavePng(file);
    }

    /// <summary>The spirits of every stage: a row for each style, each kind's three frames side by side, 8 times larger.</summary>
    /// <summary>The two persons of Kuychu, the four frames of each, one row for each.</summary>
    private static void ExportKuychu(string file)
    {
        const int k = 4;
        var sheet = Image.CreateEmpty(KuychuArt.Width * k * BossStyle.FrameCount + 50, KuychuArt.Height * k * 3 + 40, false, Image.Format.Rgba8);
        sheet.Fill(new Color(0.05f, 0.06f, 0.16f));
        for (int f = 0; f < BossStyle.FrameCount; f++)
        {
            Blit(sheet, KuychuArt.MakeLeft(f), 10 + f * (KuychuArt.Width * k + 10), 10, k);
            Blit(sheet, KuychuArt.MakeRight(f), 10 + f * (KuychuArt.Width * k + 10), 20 + KuychuArt.Height * k, k);
            Blit(sheet, KuychuArt.MakeFused(f), 10 + f * (KuychuArt.Width * k + 10), 30 + KuychuArt.Height * k * 2, k);
        }
        sheet.SavePng(file);
    }

    private static void ExportSpirits(string file)
    {
        var styles = new[] { SpiritStyle.Forest, SpiritStyle.Party, SpiritStyle.Animals, SpiritStyle.Cave, SpiritStyle.Andes };
        var kinds = Enum.GetValues<SpiritKind>();
        const int k = 8, gap = 12;
        int cell = 24 * k + gap, rowHeight = 28 * k + gap;
        var sheet = Image.CreateEmpty(cell * 3 * kinds.Length + gap, rowHeight * styles.Length + gap, false, Image.Format.Rgba8);
        sheet.Fill(new Color(0.05f, 0.06f, 0.16f));
        for (int row = 0; row < styles.Length; row++)
            for (int i = 0; i < kinds.Length; i++)
                for (int f = 0; f < styles[row].FrameCount; f++)
                    Blit(sheet, styles[row].MakeCanvas(kinds[i], f), gap + (i * 3 + f) * cell, gap + row * rowHeight, k);
        sheet.SavePng(file);
    }

    /// <summary>The art kept from the transformations that are not used for now: the serpent (head, charging head, body) and the dolphin.</summary>
    private static void ExportRetired(string folder)
    {
        Directory.CreateDirectory(folder);
        const int k = 4;
        var sheet = Image.CreateEmpty(SerpentBossArt.Width * k / 2 * 5 + 40, SerpentBossArt.Height * k / 2 + SerpentBossArt.BodyLength * k / 4 + 30, false, Image.Format.Rgba8);
        sheet.Fill(new Color(0.05f, 0.18f, 0.22f));
        void Place(PixelCanvas c, int x, int y, bool half)
        {
            var img = c.ToImage();
            img.Resize(c.Width * k / (half ? 4 : 2), c.Height * k / (half ? 4 : 2), Image.Interpolation.Nearest);
            sheet.BlendRect(img, new Rect2I(0, 0, img.GetWidth(), img.GetHeight()), new Vector2I(x, y));
        }
        for (int f = 0; f < 4; f++) Place(SerpentBossArt.MakeHead(f, false), 5 + f * (SerpentBossArt.Width * k / 2 + 5), 5, false);
        Place(SerpentBossArt.MakeHead(0, true), 5 + 4 * (SerpentBossArt.Width * k / 2 + 5), 5, false);
        Place(SerpentBossArt.MakeBody(), 5, SerpentBossArt.Height * k / 2 + 10, true);
        sheet.SavePng(Path.Combine(folder, "serpent.png"));
        ExportBoss(Path.Combine(folder, "dolphin.png"), WhaleArt.MakeCanvas, WhaleArt.Width, WhaleArt.Height, 3);
    }

    /// <summary>The side pillar on its wall, three times side by side, at 4x.</summary>
    private static void ExportPillar(string file)
    {
        var pillar = SidePillarArt.MakePillar(); var wall = SidePillarArt.MakeWall();
        var sheet = new PixelCanvas(wall.Width * 3 + pillar.Width + 4, SidePillarArt.Height);
        for (int ox = 0; ox < wall.Width * 3; ox += wall.Width) for (int y = 0; y < wall.Height; y++) for (int x = 0; x < wall.Width; x++) sheet[ox + x, y] = wall[x, y];
        PixelPaint.Composite(sheet, Shift(pillar, wall.Width * 3 - 16, sheet.Width, sheet.Height));
        var img = sheet.ToImage(); img.Resize(img.GetWidth() * 4, img.GetHeight() * 4, Image.Interpolation.Nearest);
        img.SavePng(file);
    }

    private static PixelCanvas Shift(PixelCanvas c, int dx, int w, int h)
    {
        var r = new PixelCanvas(w, h);
        for (int y = 0; y < c.Height; y++) for (int x = 0; x < c.Width; x++) if (c.Contains(x, y) && x + dx >= 0 && x + dx < w) r[x + dx, y] = c[x, y];
        return r;
    }

    /// <summary>Renders every sound effect to a WAV file in a folder.</summary>
    private static void ExportSfx(string folder)
    {
        Directory.CreateDirectory(folder);
        foreach (SfxBank.Sound sound in Enum.GetValues<SfxBank.Sound>())
        {
            var data = SfxBank.Make(sound);
            var pcm = new byte[data.Length * 2];
            for (int i = 0; i < data.Length; i++)
            {
                short v = (short)Math.Clamp(Math.Tanh(data[i]) * 32000, -32768, 32767);
                pcm[i * 2] = (byte)(v & 0xFF); pcm[i * 2 + 1] = (byte)((v >> 8) & 0xFF);
            }
            WriteWav(Path.Combine(folder, sound + ".wav"), pcm, channels: 1, rate: SfxBank.Rate);
        }
    }

    /// <summary>Renders the songs to WAV files in a folder.</summary>
    private static void ExportMusic(string folder)
    {
        Directory.CreateDirectory(folder);
        foreach (var (name, song) in new[] { ("menu_quiet_water", Songs.QuietWater()), ("stage_ember_serpent", Songs.EmberSerpent()), ("boss_inferno_serpent", Songs.InfernoSerpent()), ("boss_serpent_ablaze", Songs.SerpentAblaze()) })
        {
            WriteWav(Path.Combine(folder, name + ".wav"), Synth.ToPcm16(song.Item1, song.Item2), channels: 2, rate: Synth.Rate);
            GD.Print($"{name}: {song.Item1.Length / (double)Synth.Rate:F1} s");
        }
    }

    private static void WriteWav(string file, byte[] pcm, int channels, int rate)
    {
        using var f = File.Create(file);
        using var w = new BinaryWriter(f);
        w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + pcm.Length); w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        w.Write(16); w.Write((short)1); w.Write((short)channels); w.Write(rate); w.Write(rate * 2 * channels); w.Write((short)(2 * channels)); w.Write((short)16);
        w.Write(Encoding.ASCII.GetBytes("data")); w.Write(pcm.Length); w.Write(pcm);
    }
}
