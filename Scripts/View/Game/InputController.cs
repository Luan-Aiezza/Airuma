using System.Collections.Generic;
using Godot;

/// <summary>
/// Merges hardware keyboard (arrows or WASD, Z, X, Shift) and on-screen touch controls into one <see cref="PlayerInputComponent"/>.
/// Touches are identified by an int (the touch index; -1 for the mouse). All points are screen px (Y down).
/// </summary>
public class InputController
{
    /// <summary>Virtual joystick: how far (in screen px) a drag must go to reach full speed.</summary>
    public const float JoystickRadius = 50;

    private int? _movementTouch;
    private Vector2 _movementOrigin;
    private Vector2 _movementCurrent;
    public bool TouchShooting { get; private set; }
    public bool TouchBombing { get; private set; }

    private readonly HashSet<int> _shootTouches = new();
    private readonly HashSet<int> _bombTouches = new();

    /// <summary>Test hook: when set, replaces the keyboard (used by the autoplay).</summary>
    public System.Func<PlayerInputComponent> Override;

    public Vector2? JoystickOrigin => _movementTouch == null ? null : _movementOrigin;
    public Vector2 JoystickKnob => _movementCurrent;

    // MARK: Touch tracking

    public void BeginMovement(int touch, Vector2 point)
    {
        if (_movementTouch != null) return;
        _movementTouch = touch;
        _movementOrigin = point;
        _movementCurrent = point;
    }

    public void MoveTouch(int touch, Vector2 point)
    {
        if (touch != _movementTouch) return;
        var d = point - _movementOrigin;
        float dist = d.Length();
        _movementCurrent = dist > JoystickRadius ? _movementOrigin + d / dist * JoystickRadius : point;
    }

    public void End(int touch)
    {
        if (touch == _movementTouch) _movementTouch = null;
        _shootTouches.Remove(touch);
        _bombTouches.Remove(touch);
        TouchShooting = _shootTouches.Count > 0;
        TouchBombing = _bombTouches.Count > 0;
    }

    public void EndAll()
    {
        _movementTouch = null;
        _shootTouches.Clear();
        _bombTouches.Clear();
        TouchShooting = false;
        TouchBombing = false;
    }

    // MARK: Sampling

    private static bool Down(Key key) => Godot.Input.IsKeyPressed(key);

    public PlayerInputComponent Sample()
    {
        if (Override != null) return Override();
        var input = new PlayerInputComponent();
        var dir = Vector2.Zero;

        if (Down(Key.Left) || Down(Key.A)) dir.X -= 1;
        if (Down(Key.Right) || Down(Key.D)) dir.X += 1;
        if (Down(Key.Up) || Down(Key.W)) dir.Y -= 1;
        if (Down(Key.Down) || Down(Key.S)) dir.Y += 1;
        input.IsShooting = Down(Key.Z);
        input.IsBombing = Down(Key.X);
        input.IsFocused = Down(Key.Shift);

        if (_movementTouch != null)
        {
            // Screen and game space both have Y down.
            var touchDir = (_movementCurrent - _movementOrigin) / JoystickRadius;
            if (touchDir.Length() > 0.15f) dir += touchDir;
        }

        // Keyboard is digital (normalised, like the original); touch keeps analogue magnitude up to 1.
        input.Direction = dir.Length() > 1 ? Mathx.Norm(dir) : dir;
        input.IsShooting = input.IsShooting || TouchShooting;
        input.IsBombing = input.IsBombing || TouchBombing;
        return input;
    }
}
