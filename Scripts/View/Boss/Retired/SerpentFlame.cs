using Godot;

/// <summary>
/// The red flames round the serpent: a sprite that is a child of the serpent's sprite (so it follows, scales and flips with
/// it), showing the same art on a larger canvas through <c>flame.gdshader</c>, which burns flames out of the silhouette.
/// Give it the padded texture of the current frame (<see cref="SerpentBossArt.Padded"/>) and the parent's offset.
/// </summary>
public partial class SerpentFlame : Sprite2D
{
    private static Shader _shader;

    public SerpentFlame()
    {
        Centered = false;
        _shader ??= GD.Load<Shader>("res://Shaders/flame.gdshader");
        Material = new ShaderMaterial { Shader = _shader };
        TextureFilter = TextureFilterEnum.Nearest;
    }

    /// <summary>How far the flames reach (art pixels), how fierce they are, and how deep inside the body the rim of flame goes.</summary>
    public void Tune(float reach, float fury, float rim)
    {
        var m = (ShaderMaterial)Material;
        m.SetShaderParameter("reach", reach);
        m.SetShaderParameter("fury", fury);
        m.SetShaderParameter("rim", rim);
    }

    /// <summary>Lines the padded canvas up with the parent's art, which is drawn at <paramref name="parentOffset"/>.</summary>
    public void Align(Vector2 parentOffset) => Position = parentOffset - new Vector2(SerpentBossArt.FlamePad, SerpentBossArt.FlamePad);
}
