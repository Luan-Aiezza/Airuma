using System;
using Godot;

/// <summary>Small helpers shared by the views (the scene, the boss, the effects): z-order, tinted and additive materials, sizing and reflections.</summary>
public static class ViewKit
{
    /// <summary>The ZIndex of a layer given in tenths of the original's layers (water -100 ... boat and boss 20, bullets 40, HUD 100).</summary>
    public static int Z(float z) => (int)Math.Round(z * 10);

    /// <summary>The colour a reflection is tinted with: dark and cool, as seen through water.</summary>
    public static readonly Color MirrorTint = new Color(0.12f, 0.32f, 0.42f);

    private const string TintShader = @"
shader_type canvas_item;
uniform vec3 target = vec3(1.0);
uniform float amount = 0.0;
void fragment() { COLOR.rgb = mix(COLOR.rgb, target, amount); }
";
    private static Shader _tintShader;

    /// <summary>A shader material that blends the sprite's colour towards `target` by `amount`.</summary>
    public static ShaderMaterial NewTintMaterial(Color target, float amount)
    {
        _tintShader ??= new Shader { Code = TintShader };
        var m = new ShaderMaterial { Shader = _tintShader };
        m.SetShaderParameter("target", new Vector3(target.R, target.G, target.B));
        m.SetShaderParameter("amount", amount);
        return m;
    }

    public static void SetBlend(Sprite2D s, float amount) => ((ShaderMaterial)s.Material).SetShaderParameter("amount", amount);

    public static CanvasItemMaterial AddMaterial() => new CanvasItemMaterial { BlendMode = CanvasItemMaterial.BlendModeEnum.Add };

    /// <summary>Sizes a centred sprite to `size` screen px, whatever its texture.</summary>
    public static void FitTo(Sprite2D s, Vector2 size)
    {
        if (s.Texture == null) return;
        s.Scale = size / s.Texture.GetSize();
    }

    /// <summary>Screen position of a game point.</summary>
    public static Vector2 Point(Vector2 p) => Layout.ScenePoint(p);

    /// <summary>Whether a mirrored actor may be part of the reflection (a defeated boss is parked far away).</summary>
    public static bool CanReflect(Vector2 point)
    {
        const float slack = 400;
        return point.X > -slack && point.X < Layout.RiverWidth + slack && point.Y > -slack && point.Y < Layout.Height + slack;
    }

    /// <summary>Puts a mirror in the reflection (or takes it out) as wanted.</summary>
    public static void Attach(Node2D mirror, Node parent, bool wanted)
    {
        if (wanted) { if (mirror.GetParent() == null) parent.AddChild(mirror); }
        else if (mirror.GetParent() != null) mirror.GetParent().RemoveChild(mirror);
    }
}
