using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// Tests of the logic of the game (the simulation, the cards, the score, the audio and the texts), which run without a window.
/// Run with: Godot --headless --path . res://Scenes/selftest.tscn
/// </summary>
public static partial class SelfTest
{
    private static List<string> _problems = new List<string>();

    // Tiny assertion helpers

    private static void True(bool condition, string message = "") { if (!condition) _problems.Add("expected true. " + message); }
    private static void False(bool condition, string message = "") { if (condition) _problems.Add("expected false. " + message); }
    private static void Eq<T>(T actual, T expected, string message = "")
    {
        if (!EqualityComparer<T>.Default.Equals(actual, expected)) _problems.Add($"expected {expected} but got {actual}. {message}");
    }
    private static void Near(float actual, float expected, float accuracy, string message = "")
    {
        if (!(MathF.Abs(actual - expected) <= accuracy)) _problems.Add($"expected {expected} +/- {accuracy} but got {actual}. {message}");
    }
    private static void Greater(float a, float b, string message = "") { if (!(a > b)) _problems.Add($"expected {a} > {b}. {message}"); }
    private static void GreaterEq(float a, float b, string message = "") { if (!(a >= b)) _problems.Add($"expected {a} >= {b}. {message}"); }
    private static void Less(float a, float b, string message = "") { if (!(a < b)) _problems.Add($"expected {a} < {b}. {message}"); }
    private static void LessEq(float a, float b, string message = "") { if (!(a <= b)) _problems.Add($"expected {a} <= {b}. {message}"); }

    public static int Run()
    {
        var tests = new List<(string name, Action body)>();
        void T(string name, Action body) => tests.Add((name, body));

        RegisterBasics(T);
        RegisterSpirits(T);
        RegisterWaves(T);
        RegisterStageFlow(T);
        RegisterSpiritContact(T);
        RegisterDifficulty(T);
        RegisterPower(T);
        RegisterBomb(T);
        RegisterCards(T);

        int failed = 0;
        foreach (var (name, body) in tests)
        {
            _problems = new List<string>();
            try { body(); }
            catch (Exception e) { _problems.Add("exception: " + e); }
            if (_problems.Count == 0) GD.Print("PASS " + name);
            else
            {
                failed++;
                GD.Print("FAIL " + name);
                foreach (var p in _problems.Take(4)) GD.Print("     " + p);
            }
        }
        GD.Print($"SelfTest: {tests.Count - failed} passed, {failed} failed of {tests.Count}");
        return failed;
    }

    // Helpers

    private const float Dt = 1f / 60;
    private static PlayerInputComponent Idle() => new PlayerInputComponent();
    private static Vector2 V(float x, float y) => new Vector2(x, y);
    private static BulletData[] Pool(int n) => BulletSystem.NewPool(n);
    private static int ActiveCount(BulletData[] pool) => pool.Count(b => b.IsActive);
    private static void StepN(World w, int n, PlayerInputComponent? input = null) { for (int i = 0; i < n; i++) w.Step(Dt, input ?? Idle()); }
    private static void Run(World w, float seconds) => StepN(w, (int)(seconds * 60));
    private static bool Has(List<WorldEvent> events, WorldEventKind kind) => events.Any(e => e.Kind == kind);
    private static void Place(World w, BulletData b)
    {
        int slot = Array.FindIndex(w.Bullets, x => !x.IsActive);
        w.Bullets[slot] = b;
    }

}
