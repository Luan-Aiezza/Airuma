using System.Collections.Generic;
using Godot;

/// <summary>The languages the game can be played in.</summary>
public enum Language { PortugueseBrazil, English, Spanish }

public static class LanguageExt
{
    /// <summary>The code stored in the settings file.</summary>
    public static string Code(this Language language) => language switch
    {
        Language.English => "en", Language.Spanish => "es", _ => "pt-BR",
    };

    /// <summary>The name of the language in that language itself (what a language menu shows).</summary>
    public static string NativeName(this Language language) => language switch
    {
        Language.English => "English", Language.Spanish => "Español", _ => "Português",
    };

    public static Language? FromCode(string code) => code switch
    {
        "pt-BR" or "pt" => Language.PortugueseBrazil, "en" => Language.English, "es" => Language.Spanish, _ => null,
    };
}

/// <summary>
/// The language of the game, and where its texts are looked up. Every text has a key (the name of its property in <see cref="UIText"/>,
/// or of a boss attack, and so on) and a table for each language. The game draws its text with its own pixel font, so Godot's
/// translation of labels does not apply: this does the same job (a language, a table, a fallback) for our own labels.
/// A text that a language lacks is shown in English, and one that English lacks is shown as its own key, so nothing is ever empty.
/// </summary>
public static class Loc
{
    private const string SettingsPath = "user://settings.cfg";
    private const string Section = "game", Key = "language";

    private static readonly Dictionary<Language, Dictionary<string, string>> Tables = new Dictionary<Language, Dictionary<string, string>>
    {
        [Language.English] = TextsEn.Table,
        [Language.PortugueseBrazil] = TextsPtBr.Table,
    };

    /// <summary>The language of the game. Portuguese (Brazil) until the player picks another.</summary>
    public static Language Current { get; private set; } = Language.PortugueseBrazil;

    /// <summary>The languages that have a table (the others are still to be written).</summary>
    public static IEnumerable<Language> Available => Tables.Keys;

    public static IReadOnlyDictionary<string, string> TableOf(Language language) => Tables[language];

    /// <summary>Changes the language (only to one that has a table).</summary>
    public static void Set(Language language)
    {
        if (Tables.ContainsKey(language)) Current = language;
    }

    /// <summary>Reads the language the player chose last time, if any, and keeps it in the settings from now on.</summary>
    public static void Load()
    {
        var cfg = new ConfigFile();
        if (cfg.Load(SettingsPath) == Error.Ok && cfg.HasSectionKey(Section, Key) && LanguageExt.FromCode((string)cfg.GetValue(Section, Key)) is Language saved)
            Set(saved);
    }

    /// <summary>Changes the language and remembers the choice.</summary>
    public static void Choose(Language language)
    {
        Set(language);
        var cfg = new ConfigFile();
        cfg.Load(SettingsPath);
        cfg.SetValue(Section, Key, Current.Code());
        cfg.Save(SettingsPath);
    }

    /// <summary>The text for a key in the current language; in English if it is missing there, or the key itself if it is missing everywhere.</summary>
    public static string Get(string key)
    {
        if (Tables[Current].TryGetValue(key, out var text)) return text;
        if (Tables[Language.English].TryGetValue(key, out text)) return text;
        return key;
    }
}
