using System.Collections.Generic;
using Godot;

/// <summary>
/// The life of the cave over the still water: the dim shadows of a few fish that swim about slowly (under the surface, so they are only shadows), and fireflies
/// that drift and blink, mostly near the rock walls, drawn with light (additively). Purely for the eyes. <see cref="Update"/> is called every frame; as the cave
/// opens (the walls slide away) the fireflies spread out over the water.
/// </summary>
public partial class CaveLife : Node2D
{
    private sealed class Fish
    {
        public Sprite2D Sprite; public Vector2 Position, Velocity; public float Wander, Phase;
    }

    private sealed class Firefly
    {
        public Sprite2D Sprite; public Vector2 Home; public float Radius, Speed, Phase, Blink;
    }

    private readonly Vector2 _size;
    private readonly List<Fish> _fish = new();
    private readonly List<Firefly> _flies = new();
    private readonly Texture2D[] _fishFrames;
    private readonly SeededGenerator _rng = new(0xF15D);
    private double _time;

    private readonly bool _upward;
    private readonly float _fishSpeed;

    /// <summary>`fireflies`: whether fireflies drift over the water. `fishSpeed`: how fast the fish swim (1: slowly, as in the cave); `upward`: they swim up the picture, with the water (the waterfall).</summary>
    public CaveLife(Vector2 size, bool fireflies = true, float fishSpeed = 1, bool upward = false)
    {
        _size = size;
        _upward = upward;
        _fishSpeed = fishSpeed;
        _fishFrames = new[] { CaveArt.MakeFish(0).ToTexture(), CaveArt.MakeFish(1).ToTexture() };
        for (int i = 0; i < 7; i++)
        {
            float scale = i % 3 == 0 ? 6 : (i % 3 == 1 ? 4 : 3);
            var sprite = new Sprite2D { Texture = _fishFrames[0], Scale = new Vector2(scale, scale), ZIndex = -98, Modulate = new Color(1, 1, 1, 0.34f), TextureFilter = TextureFilterEnum.Nearest };
            AddChild(sprite);
            float angle = upward ? -Mathf.Pi / 2 + _rng.Range(-0.25f, 0.25f) : _rng.Range(0f, Mathf.Tau);
            _fish.Add(new Fish
            {
                Sprite = sprite, Position = new Vector2(_rng.Range(60f, size.X - 60), _rng.Range(40f, size.Y - 40)),
                Velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _rng.Range(22f, 40f) * fishSpeed / (scale / 4f), Wander = _rng.Range(0.3f, 0.8f), Phase = _rng.Range(0f, 10f),
            });
        }
        if (!fireflies) return;
        var tex = CaveArt.MakeFirefly().ToTexture();
        var add = new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };
        for (int i = 0; i < 30; i++)
        {
            bool nearWall = i % 10 < 7;
            float x = nearWall ? (i % 2 == 0 ? _rng.Range(15f, 130f) : size.X - _rng.Range(15f, 130f)) : _rng.Range(80f, size.X - 80);
            var sprite = new Sprite2D { Texture = tex, Scale = new Vector2(3, 3), ZIndex = -45, Material = add, TextureFilter = TextureFilterEnum.Nearest };
            AddChild(sprite);
            _flies.Add(new Firefly { Sprite = sprite, Home = new Vector2(x, _rng.Range(20f, size.Y - 20)), Radius = _rng.Range(14f, 46f), Speed = _rng.Range(0.3f, 0.9f), Phase = _rng.Range(0f, 20f), Blink = _rng.Range(0.6f, 1.6f) });
        }
    }

    public void Update(float dt, float open)
    {
        _time += dt;
        float t = (float)_time;
        foreach (var f in _fish)
        {
            // a fish swims on, turning a little now and then; when it leaves the water it comes back on the other side
            float turn = Mathf.Sin(t * f.Wander + f.Phase) * (_upward ? 0.25f : 0.6f) * dt;
            f.Velocity = f.Velocity.Rotated(turn);
            // a fish in the waterfall is swept up quickly, and slows as the water calms
            f.Position += f.Velocity * dt * (_upward ? 1 - 0.85f * open : 1);
            if (f.Position.X < -40) f.Position.X = _size.X + 30; else if (f.Position.X > _size.X + 40) f.Position.X = -30;
            if (f.Position.Y < -40) f.Position.Y = _size.Y + 30; else if (f.Position.Y > _size.Y + 40) f.Position.Y = -30;
            f.Sprite.Position = f.Position;
            f.Sprite.Rotation = f.Velocity.Angle();
            f.Sprite.Texture = _fishFrames[(int)(t * 3 + f.Phase) % 2];
        }
        foreach (var f in _flies)
        {
            float a = t * f.Speed + f.Phase;
            var spread = new Vector2((f.Home.X < _size.X / 2 ? -1 : 1) * -open * 120, 0);           // as the walls go, they spread over the water
            f.Sprite.Position = f.Home + spread + new Vector2(Mathf.Sin(a) * f.Radius, Mathf.Cos(a * 1.3f) * f.Radius * 0.7f);
            float blink = 0.5f + 0.5f * Mathf.Sin(t * f.Blink * 2.2f + f.Phase * 3);
            f.Sprite.Modulate = new Color(1, 1, 1, 0.15f + 0.65f * blink * blink);
        }
    }
}
