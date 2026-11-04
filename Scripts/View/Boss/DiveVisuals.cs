using System;
using Godot;
using static ViewKit;

/// <summary>
/// What is seen of a boss that dives: it shrinks as it sinks, with a ring closing in on it, is gone while under the water, a shadow under the boat
/// grows and darkens as the boss comes out, and it comes up at the shadow, growing back, with a ring opening round it (the sinking played backwards).
/// </summary>
public partial class DiveVisuals : Node2D
{
    private readonly Sprite2D _shadow = new() { Visible = false };
    private readonly Sprite2D _ring = new() { Visible = false };
    private float _clock;

    public DiveVisuals()
    {
        _shadow.Texture = MakeShadowTexture(96);
        _shadow.ZIndex = Z(1.7f);       // on the water, under the boss and the boat
        _ring.Texture = MakeRingTexture(96);
        _ring.ZIndex = Z(1.75f);
        AddChild(_shadow);
        AddChild(_ring);
    }

    /// <summary>A soft red disc, `size` px across, that marks where the boss will come out of the water.</summary>
    private static ImageTexture MakeShadowTexture(int size)
    {
        var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f - size / 2f) / (size / 2f), dy = (y + 0.5f - size / 2f) / (size / 2f);
                float d = MathF.Sqrt(dx * dx + dy * dy);
                img.SetPixel(x, y, new Color(0.92f, 0.08f, 0.1f, d >= 1 ? 0 : 0.7f * Mathf.Clamp((1 - d) * 3, 0, 1) * (0.55f + 0.45f * d)));
            }
        return ImageTexture.CreateFromImage(img);
    }

    /// <summary>A thin pale ring, `size` px across, for the ripple where the boss goes into the water and comes out.</summary>
    private static ImageTexture MakeRingTexture(int size)
    {
        var img = Image.CreateEmpty(size, size, false, Image.Format.Rgba8);
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = MathF.Sqrt(MathF.Pow(x + 0.5f - size / 2f, 2) + MathF.Pow(y + 0.5f - size / 2f, 2)) / (size / 2f);
                float ring = 1 - MathF.Abs(d - 0.85f) / 0.07f;
                if (ring > 0) img.SetPixel(x, y, new Color(0.95f, 0.98f, 1f, Mathf.Clamp(ring, 0, 1)));
            }
        return ImageTexture.CreateFromImage(img);
    }

    private static float Progress(BossEntity boss) => Mathf.Clamp(boss.Dive.Clock / MovementSystem.DiveSinkSeconds, 0, 1);

    /// <summary>The size of the boss's body: it shrinks while sinking and grows while coming out.</summary>
    public float BodySize(BossEntity boss, bool diving)
    {
        if (!diving) return 1;
        float u = Progress(boss);
        return boss.Dive.State == DiveState.Sinking ? 1 - 0.6f * u : (boss.Dive.State == DiveState.Rising ? 0.4f + 0.6f * u : 1);
    }

    public void Update(float dt, BossEntity boss, bool diving)
    {
        _clock += dt;
        float u = Progress(boss);
        bool ring = diving && (boss.Dive.State == DiveState.Sinking || boss.Dive.State == DiveState.Rising);
        _ring.Visible = ring;
        if (ring)
        {
            float open = boss.Dive.State == DiveState.Rising ? u : 1 - u;          // the ring opens as he comes out, and closes as he goes in
            _ring.Position = Point(boss.Transform.Position) + new Vector2(0, boss.Hitbox.Radius * 0.8f);
            _ring.Scale = new Vector2(0.7f + 1.5f * open, (0.7f + 1.5f * open) * 0.55f);
            _ring.Modulate = new Color(1, 1, 1, boss.Dive.State == DiveState.Rising ? 1 - 0.7f * u : 0.3f + 0.7f * u);
        }

        bool shadow = diving && boss.Dive.State == DiveState.Under;
        _shadow.Visible = shadow;
        if (!shadow) return;
        // the warning: a red ring that closes on the spot again and again, quicker as the moment comes
        float tt = Mathf.Clamp(boss.Dive.Clock / MovementSystem.DiveUnderSeconds, 0, 1);
        float cycle = (_clock * (1.4f + 2.2f * tt)) % 1f;
        _ring.Visible = true;
        _ring.Position = Point(new Vector2(boss.Dive.ExitX, boss.Dive.ExitY));
        _ring.Scale = Vector2.One * (5.4f - 3.6f * cycle) * (boss.Hitbox.Radius / 37f);
        _ring.Modulate = new Color(1f, 0.15f, 0.15f, 0.25f + 0.65f * cycle);
        float t = Mathf.Clamp(boss.Dive.Clock / MovementSystem.DiveUnderSeconds, 0, 1);
        // a red mark, big and beating (the beat quickens as he is about to come out)
        float beat = 0.5f + 0.5f * MathF.Sin(_clock * (7 + 11 * t));
        float pulse = 1 + 0.22f * beat;
        _shadow.Position = Point(new Vector2(boss.Dive.ExitX, boss.Dive.ExitY));
        _shadow.Scale = Vector2.One * (1.15f + 0.6f * t) * pulse * (boss.Hitbox.Radius / 37f) * 2f;
        _shadow.Modulate = new Color(1, 1, 1, 0.5f + 0.5f * beat * (0.6f + 0.4f * t));
    }
}
