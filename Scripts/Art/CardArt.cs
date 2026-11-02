using System;
using System.Collections.Generic;

/// <summary>Pixel art for the cards: a 14x14 icon for each, and the card itself (a wooden frame with a rarity band).</summary>
public static class CardArt
{
    public const int IconSize = 14;
    /// <summary>A card is drawn on this grid; one pixel is <see cref="PixelScale"/> points.</summary>
    public const int Width = 50, Height = 90;
    public const float PixelScale = 4;

    private static readonly Dictionary<char, PixelColor> Legend = new Dictionary<char, PixelColor>
    {
        ['i'] = PixelColor.Hex(0x1B1620), ['w'] = PixelColor.Hex(0xFFF3B0), ['y'] = PixelColor.Hex(0xFFD23F),
        ['o'] = PixelColor.Hex(0xFF9A1F), ['r'] = PixelColor.Hex(0xC42A12), ['b'] = PixelColor.Hex(0x2A6FDB),
        ['c'] = PixelColor.Hex(0xDDF8FF), ['a'] = PixelColor.Hex(0x4FC3E8), ['g'] = PixelColor.Hex(0x55B35F),
        ['d'] = PixelColor.Hex(0x2F8A4E), ['k'] = PixelColor.Hex(0x8A5A35), ['p'] = PixelColor.Hex(0x9B5DE5),
        ['s'] = PixelColor.Hex(0xB9C4D0), ['m'] = PixelColor.Hex(0x6B7886),
    };

    private static string[] Rows(Card card)
    {
        switch (card)
        {
            case Card.HomingWisp:
                return new[]
                {
                    "..............",
                    "..........ii..",
                    ".........iwwi.",
                    "........iwwyoi",
                    "........iwyyoi",
                    ".......iiyooi.",
                    "......ioiioi..",
                    ".....ioi.ii...",
                    "...ioi........",
                    "..ioi.........",
                    ".ioi..........",
                    ".ii...........",
                    "..............",
                    "..............",
                };
            case Card.TwinFan:
                return new[]
                {
                    "a............a",
                    "aa....c.....aa",
                    ".aa...c....aa.",
                    "..aa..w...aa..",
                    "...aa.w..aa...",
                    "....aaw.aa....",
                    ".....awa.a....",
                    "......wa......",
                    "......w.......",
                    "......w.......",
                    ".....yoy......",
                    "....yoooy.....",
                    ".....yoy......",
                    "..............",
                };
            case Card.PiercingLight:
                return new[]
                {
                    ".......c......",
                    "......cwc.....",
                    ".....cawac....",
                    "......awa.....",
                    "..aaaaawaaaa..",
                    ".a....awa...a.",
                    "a.....awa....a",
                    "a.....awa....a",
                    ".a....awa...a.",
                    "..aaaaawaaaa..",
                    "......awa.....",
                    "......awa.....",
                    ".......a......",
                    "..............",
                };
            case Card.Fervor:
                return new[]
                {
                    "......o.......",
                    ".....oo.......",
                    "....ooyo......",
                    "....ooyoo..o..",
                    "...ooyyyoo.oo.",
                    "..ooyywyyooo..",
                    "..ooywwwyoo...",
                    "..ooywwwyoo...",
                    "..oyywwwyyo...",
                    "...oyywyyo....",
                    "...ooyyyoo....",
                    "....oooor.....",
                    ".....rrr......",
                    "..............",
                };
            case Card.QuickWick:
                return new[]
                {
                    "..y...y...y...",
                    "...y..w..y....",
                    ".....yow......",
                    "......o.......",
                    "......k.......",
                    ".....iwwi.....",
                    ".....iwsi.....",
                    ".....iwsi.....",
                    ".....iwsi.....",
                    ".....iwsi.....",
                    ".....iwsi.....",
                    "....iikkkii...",
                    "....iiiiiii...",
                    "..............",
                };
            case Card.BurstingBolts:
                return new[]
                {
                    "......y.......",
                    ".y....o....y..",
                    "..o...w...o...",
                    "...o.ywy.o....",
                    "....yywyy.....",
                    "yooowwwwwooooy",
                    "....yywyy.....",
                    "...o.ywy.o....",
                    "..o...w...o...",
                    ".y....o....y..",
                    "......y.......",
                    "..............",
                    "..............",
                    "..............",
                };
            case Card.SpiritWard:
                return new[]
                {
                    "..............",
                    "..iiiiiiiiii..",
                    ".iaaccccccaai.",
                    ".iacccwwcccai.",
                    ".iaccwwwwccai.",
                    ".iaccwwwwccai.",
                    ".iaaccwwccaai.",
                    "..iaaccccaai..",
                    "..iaaaccaaai..",
                    "...iaaaaaai...",
                    "....iaaaai....",
                    ".....iaai.....",
                    "......ii......",
                    "..............",
                };
            case Card.MendingWard:
                return new[]
                {
                    "..............",
                    "..iiiiiiiiii..",
                    ".iggccccccggi.",
                    ".igcccwwcccgi.",
                    ".igccwwwwccgi.",
                    ".igcwwwwwwcgi.",
                    ".iggccwwccggi.",
                    "..iggccccggi..",
                    "..idggccggdi..",
                    "...iddggddi...",
                    "....iddddi....",
                    ".....iddi.....",
                    "......ii......",
                    "..............",
                };
            case Card.SlimWake:
                return new[]
                {
                    "..............",
                    "......aa......",
                    ".....a..a.....",
                    "....a....a....",
                    "...a..ww..a...",
                    "...a.wwww.a...",
                    "...a.wwww.a...",
                    "...a..ww..a...",
                    "....a....a....",
                    ".....a..a.....",
                    "......aa......",
                    "..aa......aa..",
                    "aa..aa..aa..aa",
                    "..............",
                };
            case Card.CalmSails:
                return new[]
                {
                    "......k.......",
                    "......kcc.....",
                    "......kccc....",
                    "......kcccc...",
                    "......kccccc..",
                    "......kcccccc.",
                    "......kccccccc",
                    "......k.......",
                    "..kkkkkkkkkk..",
                    "...kkkkkkkk...",
                    "aaaa......aaaa",
                    "..aaa..aaaa...",
                    "....aaaa......",
                    "..............",
                };
            case Card.BrighterLight:
                return new[]
                {
                    "y.....y.....y.",
                    ".y....w....y..",
                    "..y...k...y...",
                    "...y.iwwi.y...",
                    "....iwwyyi....",
                    "yy..iwyyoi..yy",
                    "....iwyyoi....",
                    "....iwyooi....",
                    "...y.iiii.y...",
                    "..y..ikki..y..",
                    ".y....kk....y.",
                    "y............y",
                    "..............",
                    "..............",
                };
            case Card.LongBurn:
                return new[]
                {
                    "..iiiiiiiiii..",
                    "..ikkkkkkkki..",
                    "...icccccci...",
                    "....icccci....",
                    ".....icci.....",
                    "......ii......",
                    "......yy......",
                    ".....iyyi.....",
                    "....iyyyyi....",
                    "...iyyyyyyi...",
                    "..iyyyyyyyyi..",
                    "..ikkkkkkkki..",
                    "..iiiiiiiiii..",
                    "..............",
                };
            case Card.SearingLight:
                return new[]
                {
                    "......r.......",
                    ".....ro.......",
                    ".....rorr.....",
                    "....rooyr.....",
                    "....roywor....",
                    "...roywwyor...",
                    "...roywwyor...",
                    "..iiiiiiiiii..",
                    "..iwwyyyyooi..",
                    "..iwyyoooorii.",
                    "..iiiiiiiiii..",
                    "....ikkkkki...",
                    "....iiiiiii...",
                    "..............",
                };
            case Card.SpareLantern:
                return new[]
                {
                    ".....iiii.....",
                    "....i.kk.i....",
                    "...iikkkkii...",
                    "..iwwwyyyooi..",
                    "..iwwyyyoooi..",
                    "..iwyyyyoooi..",
                    "..iwyyyyoooi..",
                    "..iwyyyooori..",
                    "..iiwyyooiii..",
                    "...iikkkkii...",
                    "....iiiiii....",
                    "..............",
                    "..............",
                    "..............",
                };
            case Card.HungryLantern:
                return new[]
                {
                    "y.....y.....y.",
                    "..y...y...y...",
                    "....y.....y...",
                    "......iii.....",
                    ".....ikkki....",
                    "....iwyyyoi...",
                    "....iwyyooi...",
                    "....iwyoori...",
                    "....iiiiiii...",
                    "....ikkkkki...",
                    ".....iiiii....",
                    "..............",
                    "..............",
                    "..............",
                };
            case Card.WideNet:
                return new[]
                {
                    "..............",
                    "..rr......ss..",
                    "..rrr....sss..",
                    "..rrr....sss..",
                    "...rrr..sss...",
                    "...rrr..sss...",
                    "....rrrsss....",
                    "....irrssi....",
                    ".....iiii.....",
                    "..y.......y...",
                    "....y...y.....",
                    "......y.......",
                    "..............",
                    "..............",
                };
            case Card.GoldenSpirits:
                return new[]
                {
                    "......yy......",
                    ".....yooy.....",
                    "....yowwoy....",
                    "...yowyyowy...",
                    "..yowyooywoy..",
                    "..yowyooywoy..",
                    "..yowyooywoy..",
                    "...yowyyowy...",
                    "....yowwoy....",
                    ".....yooy.....",
                    "......yy......",
                    "..............",
                    "..............",
                    "..............",
                };
            case Card.SerpentFlame:
                return new[]
                {
                    "......r.......",
                    ".....ror......",
                    "....roor..r...",
                    "....rooor.ro..",
                    "...roooyor.o..",
                    "..roooyyyor...",
                    "..rooyywyor...",
                    "..rooywwyor...",
                    "..rooyywyor...",
                    "..roooyyyor...",
                    "...rooyyor....",
                    "....roooor....",
                    ".....rrrr.....",
                    "..............",
                };
            case Card.PartyFever:
                return new[]
                {
                    "...y......p...",
                    ".....r...b....",
                    "....ror.......",
                    "....rwr...y...",
                    "...rwrwr......",
                    "...rrwrr..g...",
                    "..rwrrrwr.....",
                    "..rrwrwrr.....",
                    ".rwrrrrrwr.p..",
                    "iiiiiiiiiii...",
                    "..y.....b...r.",
                    "....g....y....",
                    "...........b..",
                    "..............",
                };
            case Card.StormBolt:
                return new[]
                {
                    "...........i..",
                    "..........iyi.",
                    "........iyyi..",
                    "......iyyyi...",
                    ".....iyyyi....",
                    "....iyyyyyi...",
                    "......iyyyi...",
                    ".....iyyyi....",
                    "....iyyi......",
                    "...iyyi.......",
                    "...iyi........",
                    "...ii.........",
                    "..............",
                    "..............",
                };
            case Card.BearCloak:
                return new[]
                {
                    "..............",
                    ".kk........kk.",
                    "kook......kook",
                    "kokk......kkok",
                    ".kkkkkkkkkkkk.",
                    "..kkkkkkkkkk..",
                    ".kkkiwkkwikkk.",
                    ".kkkiikkiikkk.",
                    ".kkkkkssskkkk.",
                    ".kkkkssiisskk.",
                    ".kkkkkkiikkkk.",
                    "..kkkkrrkkkk..",
                    "...kkkkkkkk...",
                    "..............",
                };
            default:
                return new string[0];
        }
    }

    public static PixelCanvas Icon(Card card)
    {
        var c = new PixelCanvas(IconSize, IconSize);
        var rows = Rows(card);
        for (int y = 0; y < Math.Min(IconSize, rows.Length); y++)
        {
            var row = rows[y];
            for (int x = 0; x < Math.Min(IconSize, row.Length); x++)
                if (Legend.TryGetValue(row[x], out var color)) c[x, y] = color;
        }
        return c;
    }

    /// <summary>The colour of a card's band and frame: its rarity's, or, for a boss's special card, one of the boss's theme (fire, party, storm).</summary>
    public static PixelColor AccentColor(Card card) => card switch
    {
        Card.SerpentFlame => PixelColor.Hex(0xFF6A1A),
        Card.PartyFever => PixelColor.Hex(0xE84AA8),
        Card.StormBolt => PixelColor.Hex(0x6FD0FF),
        Card.BearCloak => PixelColor.Hex(0xC8905A),
        _ => RarityColor(CardInfo.Rarity(card)),
    };

    public static PixelColor RarityColor(CardRarity rarity)
    {
        switch (rarity)
        {
            case CardRarity.Common: return PixelColor.Hex(0xB9C4D0);
            case CardRarity.Uncommon: return PixelColor.Hex(0x4FC3E8);
            case CardRarity.Special: return PixelColor.Hex(0xE8503A);
            default: return PixelColor.Hex(0xF2C14E);
        }
    }

    /// <summary>The card's face, without text: frame, rarity band, a round window for the icon, and the icon drawn double size.</summary>
    public static PixelCanvas Face(Card card)
    {
        var c = HudArt.Box(Width, Height);
        var band = AccentColor(card);
        bool special = CardInfo.Rarity(card) == CardRarity.Special;
        for (int x = 5; x < Width - 5; x++)
        {
            c[x, 5] = band;
            c[x, 6] = band.Scaled(0.7f);
        }
        // A dark window for the icon, with the icon at twice its size.
        int windowTop = 10, windowSize = 32, left = (Width - windowSize) / 2;
        for (int y = windowTop; y < windowTop + windowSize; y++)
        {
            for (int x = left; x < left + windowSize; x++)
            {
                bool edge = x == left || x == left + windowSize - 1 || y == windowTop || y == windowTop + windowSize - 1;
                c[x, y] = edge ? HudArt.Colors.Ink : HudArt.Colors.DeepInk.Mixed(band, special ? 0.30f + 0.18f * ((x + y) % 6 < 2 ? 1 : 0) : 0.12f);
            }
        }
        if (special)
        {
            // A double band, and sparks at the corners of the window: the card of a boss is not an ordinary one.
            for (int x = 5; x < Width - 5; x++) { c[x, 8] = band.Scaled(0.5f); c[x, Height - 8] = band; c[x, Height - 7] = band.Scaled(0.7f); }
            foreach (var (sx, sy) in new[] { (left - 3, windowTop - 3), (left + windowSize + 2, windowTop - 3), (left - 3, windowTop + windowSize + 2), (left + windowSize + 2, windowTop + windowSize + 2) })
                foreach (var (dx, dy) in new[] { (0, 0), (-1, 0), (1, 0), (0, -1), (0, 1) }) if (c.Contains(sx + dx, sy + dy)) c[sx + dx, sy + dy] = band;
        }
        var art = Icon(card);
        for (int y = 0; y < IconSize; y++)
        {
            for (int x = 0; x < IconSize; x++)
            {
                if (art[x, y].A == 0) continue;
                for (int dy = 0; dy < 2; dy++)
                {
                    for (int dx = 0; dx < 2; dx++)
                    {
                        int px = left + 2 + x * 2 + dx, py = windowTop + 2 + y * 2 + dy;
                        if (c.Contains(px, py)) c[px, py] = art[x, y];
                    }
                }
            }
        }
        return c;
    }

    /// <summary>A small icon for the list of owned cards.</summary>
    public static PixelCanvas Chip(Card card)
    {
        var c = new PixelCanvas(IconSize + 2, IconSize + 2, HudArt.Colors.DeepInk);
        var art = Icon(card);
        for (int y = 0; y < IconSize; y++)
            for (int x = 0; x < IconSize; x++)
                if (art[x, y].A > 0) c[x + 1, y + 1] = art[x, y];
        return c;
    }

    // MARK: The ward around the boat

    public const int WardSize = 28;

    /// <summary>A thin ring of light, with a few brighter beads that creep round it from one frame to the next.</summary>
    public static PixelCanvas WardRing(int frame)
    {
        var c = new PixelCanvas(WardSize, WardSize);
        float centre = WardSize / 2f;
        var rim = PixelColor.Hex(0x4FC3E8); var bead = PixelColor.Hex(0xFFFFFF);
        for (int y = 0; y < WardSize; y++)
        {
            for (int x = 0; x < WardSize; x++)
            {
                float dx = x + 0.5f - centre, dy = y + 0.5f - centre;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                if (!(d > centre - 1.6f && d <= centre - 0.4f)) continue;
                float angle = MathF.Atan2(dy, dx) + (frame == 0 ? 0 : MathF.PI / 8);
                bool lit = (int)MathF.Floor((angle + MathF.PI) / (2 * MathF.PI) * 8) % 2 == 0
                           && (angle + MathF.PI) % (MathF.PI / 4) < 0.35f;
                c[x, y] = lit ? bead : rim.Scaled(0.85f);
            }
        }
        return c;
    }
}
