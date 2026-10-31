using System;
using Godot;

/// <summary>
/// What goes with the dancer's spin: pink afterimages that trail behind him and fade, and a ring of confetti that whirls round his waist
/// (in front of him on the near side, behind him on the far side). Call <see cref="Update"/> every frame with whether he is spinning and
/// where; the afterimages copy the sprite as it is (squeezed or flipped by the spin).
/// </summary>
public partial class SpinEffect : Node2D
{
    private const int Ghosts = 6, Pieces = 14;
    private const float GhostEvery = 0.04f, GhostLife = 0.32f;

    private readonly Sprite2D[] _ghosts = new Sprite2D[Ghosts];
    private readonly float[] _ghostAge = new float[Ghosts];
    private readonly Sprite2D[] _pieces = new Sprite2D[Pieces];
    private float _nextGhost, _strength, _angle;
    private int _ghostIndex;

    private static readonly uint[] Colours = { 0xFFE27A, 0xFF6A5A, 0x7AE0FF, 0xFF8AD0, 0x9AF08A };

    public SpinEffect()
    {
        var material = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        for (int i = 0; i < Ghosts; i++)
        {
            _ghosts[i] = new Sprite2D { Centered = false, Visible = false, Material = material, ZIndex = 19 };
            AddChild(_ghosts[i]);
            _ghostAge[i] = GhostLife;
        }
        for (int i = 0; i < Pieces; i++)
        {
            var canvas = new PixelCanvas(3, 3, PixelColor.Hex(Colours[i % Colours.Length]));
            canvas[0, 0] = PixelColor.Clear; canvas[2, 2] = PixelColor.Clear;
            _pieces[i] = new Sprite2D { Texture = canvas.ToTexture(), Scale = new Vector2(3, 3), Visible = false };
            AddChild(_pieces[i]);
        }
    }

    /// <summary>`sprite` is the boss's sprite (its texture, scale and offset are copied to the afterimages); `at` is where the boss is; `ring` the height of the confetti above `at`.</summary>
    public void Update(float dt, bool spinning, Vector2 at, Sprite2D sprite, float radius)
    {
        _strength = Mathf.MoveToward(_strength, spinning ? 1 : 0, dt * (spinning ? 6 : 3));
        _angle += dt * 9;

        // Afterimages.
        if (spinning)
        {
            _nextGhost -= dt;
            if (_nextGhost <= 0)
            {
                _nextGhost = GhostEvery;
                var ghost = _ghosts[_ghostIndex];
                _ghostIndex = (_ghostIndex + 1) % Ghosts;
                ghost.Texture = sprite.Texture;
                ghost.Scale = sprite.Scale;
                ghost.Offset = sprite.Offset;
                ghost.Position = at;
                _ghostAge[Array.IndexOf(_ghosts, ghost)] = 0;
            }
        }
        for (int i = 0; i < Ghosts; i++)
        {
            _ghostAge[i] += dt;
            float t = _ghostAge[i] / GhostLife;
            _ghosts[i].Visible = t < 1;
            _ghosts[i].Modulate = new Color(1f, 0.55f, 0.85f, 0.45f * (1 - t));
        }

        // The ring of confetti, tilted like a hoop seen from a little above.
        for (int i = 0; i < Pieces; i++)
        {
            float a = _angle + i * MathF.Tau / Pieces;
            float depth = MathF.Sin(a);                                // > 0: the near side
            var piece = _pieces[i];
            piece.Visible = _strength > 0.01f;
            piece.Position = at + new Vector2(MathF.Cos(a) * radius * 1.25f, depth * radius * 0.3f + radius * 0.35f);
            piece.ZIndex = depth > 0 ? 21 : 19;
            piece.Rotation = a * 2;
            piece.Modulate = new Color(1, 1, 1, _strength * (0.55f + 0.45f * depth));
        }
    }
}
