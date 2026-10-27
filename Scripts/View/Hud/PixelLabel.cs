using Godot;
using System;
using System.Collections.Generic;

/// <summary>A line of text in the game's pixel font. Re-renders only when the text changes.
/// `Position` is where the left/centre/right end of the text's cap line sits (screen space, Y down).</summary>
public partial class PixelLabel : Sprite2D
{
    public enum Align { Left, Center, Right }

    public sealed class Style
    {
        public string Name;
        public Func<int, PixelColor> Color;
        public PixelColor? Shadow;

        public Style(string name, Func<int, PixelColor> color, PixelColor? shadow) { Name = name; Color = color; Shadow = shadow; }

        private static Style Flat(string name, uint hex, uint? shadow = 0x120D11u)
        {
            var c = PixelColor.Hex(hex);
            return new Style(name, _ => c, shadow.HasValue ? PixelColor.Hex(shadow.Value) : (PixelColor?)null);
        }

        public static readonly Style Label = Flat("label", 0xE8D8B0);
        public static readonly Style Value = Flat("value", 0xFFFFFF);
        public static readonly Style Gold = Flat("gold", 0xF2C14E);
        public static readonly Style Dim = Flat("dim", 0x9A8A6A);
        public static readonly Style Speech = Flat("speech", 0xFFF3B0);
        public static readonly Style MenuDim = Flat("menuDim", 0x8FA3B8, 0x050A10);
        public static readonly Style MenuHeading = Flat("menuHeading", 0xF2C14E, 0x050A10);
        public static readonly Style Victory = Flat("victory", 0xF2C14E, 0x7E1A20);
        public static readonly Style Defeat = Flat("defeat", 0xE8483C, 0x120D11);
        /// <summary>Embers: white-hot at the top of each letter, dark red at the bottom.</summary>
        public static readonly Style Title = new Style("title", r => FirePalette.Ramp[new[] { 0, 1, 1, 2, 2, 3, 4 }[Math.Max(0, Math.Min(6, r))]],
                                                       PixelColor.Hex(0x120D11));
    }

    private struct Entry { public ImageTexture Texture; public int Width, Height, CapTop; }
    private static readonly Dictionary<(string, string), Entry> Cache = new Dictionary<(string, string), Entry>();

    private static Shader _tintShader;
    private ShaderMaterial _tintMaterial;

    public readonly float PixelScale;
    public readonly Style LabelStyle;
    public readonly Align Alignment;
    public string Text { get; private set; } = "";

    public PixelLabel(Style style, float pixelScale = 3, Align align = Align.Left)
    {
        LabelStyle = style;
        PixelScale = pixelScale;
        Alignment = align;
        Centered = false;
        Scale = new Vector2(pixelScale, pixelScale);
    }

    public void SetText(string newText)
    {
        if (newText == Text) return;
        Text = newText;
        if (string.IsNullOrEmpty(newText)) { Visible = false; return; }
        Visible = true;
        var key = (newText, LabelStyle.Name);
        if (!Cache.TryGetValue(key, out var entry))
        {
            var rendered = PixelFont.Render(newText, LabelStyle.Color, LabelStyle.Shadow);
            entry = new Entry { Texture = rendered.Canvas.ToTexture(), Width = rendered.Canvas.Width, Height = rendered.Canvas.Height, CapTop = rendered.CapTop };
            Cache[key] = entry;
        }
        Texture = entry.Texture;
        // Anchor on the cap line, so labels line up whether or not they carry accents.
        float ax = Alignment == Align.Left ? 0 : (Alignment == Align.Center ? 0.5f : 1);
        Offset = new Vector2(-ax * entry.Width, -entry.CapTop);
    }

    /// <summary>Blends the text's colours towards <paramref name="color"/>; 0 removes the tint.</summary>
    public void SetTint(Color color, float amount)
    {
        if (amount <= 0) { Material = null; return; }
        if (_tintMaterial == null)
        {
            _tintShader ??= new Shader { Code = "shader_type canvas_item;\nuniform vec4 tint : source_color = vec4(1.0);\nuniform float amount = 0.0;\nvoid fragment() { COLOR.rgb = mix(COLOR.rgb, tint.rgb, amount); }\n" };
            _tintMaterial = new ShaderMaterial { Shader = _tintShader };
        }
        _tintMaterial.SetShaderParameter("tint", color);
        _tintMaterial.SetShaderParameter("amount", amount);
        Material = _tintMaterial;
    }
}
