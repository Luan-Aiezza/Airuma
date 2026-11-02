using System;
using Godot;

/// <summary>The actions whose keys the player may choose (see <see cref="Settings.KeyFor"/>).</summary>
public enum ControlAction { Up, Down, Left, Right, Shoot, Bomb, Focus }

/// <summary>
/// What the player chose in the Config of the menu: the lives and the bombs a run begins with, the volume of the music and of the sounds (0...10), the keys, and the window
/// (that one is kept by <see cref="Main.SetFullscreen"/>). They are kept in the settings file, next to the language. Until <see cref="Load"/> is called (it is not, in the self test)
/// they are the defaults, which are what the game was designed for.
/// </summary>
public static class Settings
{
    private const string Path = "user://settings.cfg";

    public const int DefaultLives = 3, MinLives = 1, MaxLivesChoice = 5, DefaultBombs = 3, MinBombs = 0, MaxBombsChoice = 3, MaxVolume = 10;

    /// <summary>Lives at the start of a run (1...5); the boat can hold at least three.</summary>
    public static int Lives { get; private set; } = DefaultLives;
    /// <summary>Bombs at the start of a run (0...3), and the ones it gets back after losing a life.</summary>
    public static int Bombs { get; private set; } = DefaultBombs;
    public static int Music { get; private set; } = MaxVolume;
    public static int Sfx { get; private set; } = MaxVolume;

    private static readonly Key[] DefaultKeys = { Key.Up, Key.Down, Key.Left, Key.Right, Key.Z, Key.X, Key.Shift };
    private static readonly Key[] Keys = (Key[])DefaultKeys.Clone();

    public static Key KeyFor(ControlAction action) => Keys[(int)action];
    public static Key DefaultKeyFor(ControlAction action) => DefaultKeys[(int)action];

    /// <summary>The letter that also moves the boat in a direction (W, A, S, D), as long as the arrow of that direction has not been given another key and no action has taken the letter.</summary>
    public static Key? Alias(ControlAction action)
    {
        Key alias = action switch { ControlAction.Up => Key.W, ControlAction.Down => Key.S, ControlAction.Left => Key.A, ControlAction.Right => Key.D, _ => Key.None };
        if (alias == Key.None || Keys[(int)action] != DefaultKeys[(int)action]) return null;
        foreach (var k in Keys) if (k == alias) return null;
        return alias;
    }

    /// <summary>How loud a level is, in dB: the level is a share of the full volume; 0 is silence.</summary>
    public static float ToDb(int level) => level <= 0 ? -80f : 20f * MathF.Log10(Math.Min(level, MaxVolume) / (float)MaxVolume);

    // MARK: Changing

    public static void SetLives(int value) { Lives = Math.Clamp(value, MinLives, MaxLivesChoice); Save(); }
    public static void SetBombs(int value) { Bombs = Math.Clamp(value, MinBombs, MaxBombsChoice); Save(); }
    public static void SetMusic(int value) { Music = Math.Clamp(value, 0, MaxVolume); MusicPlayer.Instance?.ApplyVolume(); Save(); }
    public static void SetSfx(int value) { Sfx = Math.Clamp(value, 0, MaxVolume); Save(); }

    /// <summary>Gives `key` to an action; if another action had it, the two swap keys, so no key is left with two uses.</summary>
    public static void SetKey(ControlAction action, Key key)
    {
        int index = (int)action;
        for (int i = 0; i < Keys.Length; i++) if (i != index && Keys[i] == key) Keys[i] = Keys[index];
        Keys[index] = key;
        Save();
    }

    public static void RestoreKeys() { Array.Copy(DefaultKeys, Keys, Keys.Length); Save(); }

    /// <summary>Everything but the keys goes back to the defaults (the window too).</summary>
    public static void RestoreDefaults()
    {
        Lives = DefaultLives; Bombs = DefaultBombs; Music = MaxVolume; Sfx = MaxVolume;
        MusicPlayer.Instance?.ApplyVolume();
        if (Main.FullscreenChosen) Main.SetFullscreen(false);
        Save();
    }

    // MARK: File

    public static void Load()
    {
        var cfg = new ConfigFile();
        if (cfg.Load(Path) != Error.Ok) return;
        Lives = Math.Clamp((int)cfg.GetValue("game", "lives", DefaultLives), MinLives, MaxLivesChoice);
        Bombs = Math.Clamp((int)cfg.GetValue("game", "bombs", DefaultBombs), MinBombs, MaxBombsChoice);
        Music = Math.Clamp((int)cfg.GetValue("audio", "music", MaxVolume), 0, MaxVolume);
        Sfx = Math.Clamp((int)cfg.GetValue("audio", "sfx", MaxVolume), 0, MaxVolume);
        for (int i = 0; i < Keys.Length; i++)
        {
            var key = (Key)(int)cfg.GetValue("controls", ((ControlAction)i).ToString(), (int)DefaultKeys[i]);
            Keys[i] = key == Key.None ? DefaultKeys[i] : key;
        }
        // a file edited by hand may give one key two uses: then all go back to the defaults
        for (int i = 0; i < Keys.Length; i++) for (int j = i + 1; j < Keys.Length; j++) if (Keys[i] == Keys[j]) { Array.Copy(DefaultKeys, Keys, Keys.Length); return; }
    }

    private static void Save()
    {
        var cfg = new ConfigFile();
        cfg.Load(Path);      // the other things in the file (window, language) stay
        cfg.SetValue("game", "lives", Lives);
        cfg.SetValue("game", "bombs", Bombs);
        cfg.SetValue("audio", "music", Music);
        cfg.SetValue("audio", "sfx", Sfx);
        for (int i = 0; i < Keys.Length; i++) cfg.SetValue("controls", ((ControlAction)i).ToString(), (int)Keys[i]);
        cfg.Save(Path);
    }
}
