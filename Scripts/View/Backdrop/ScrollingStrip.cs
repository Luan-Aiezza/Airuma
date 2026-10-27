using System;
using Godot;

/// <summary>
/// A vertically tiling texture that scrolls downwards in whole art pixels, drawn from at most two sub-rectangles
/// of the tile (two Sprite2D with RegionRect).
///
/// API: new ScrollingStrip(variants, tileRows, visibleRows, pixel, speed, origin)  (origin = top-left corner, screen space);
/// Advance(dt); Reset(); RowsPerSecond (art rows per second); Variant (index into variants, applied on the next render).
/// </summary>
public partial class ScrollingStrip : Node2D
{
    private readonly Texture2D[] _variants;
    private readonly int _tileRows;
    private readonly int _visibleRows;
    private readonly float _pixel;
    private readonly Sprite2D _top = new Sprite2D();
    private readonly Sprite2D _bottom = new Sprite2D();

    private double _travelled;
    private int _shownOffset = -1;
    private int _shownVariant = -1;

    /// <summary>Art rows per second.</summary>
    public double RowsPerSecond;
    public int Variant;

    public ScrollingStrip(Texture2D[] variants, int tileRows, int visibleRows, float pixel, double speed, Vector2 origin)
    {
        _variants = variants;
        _tileRows = tileRows;
        _visibleRows = visibleRows;
        _pixel = pixel;
        RowsPerSecond = speed;
        Position = origin;
        foreach (var sprite in new[] { _top, _bottom })
        {
            sprite.Centered = false;
            sprite.RegionEnabled = true;
            sprite.Scale = new Vector2(pixel, pixel);
            AddChild(sprite);
        }
        Render();
    }

    public void Advance(double dt)
    {
        _travelled = (_travelled + dt * RowsPerSecond) % _tileRows;
        Render();
    }

    public void Reset()
    {
        _travelled = 0;
        Render();
    }

    private void Render()
    {
        int offset = (int)_travelled;
        if (offset == _shownOffset && Variant == _shownVariant) return;
        _shownOffset = offset;
        _shownVariant = Variant;

        var source = _variants[Variant % _variants.Length];
        float width = source.GetWidth();

        // Screen row 0 shows tile row `start`; going down it runs to the end of the tile, then wraps to row 0.
        int start = (_tileRows - offset) % _tileRows;
        int firstRows = Math.Min(_visibleRows, _tileRows - start);
        int secondRows = _visibleRows - firstRows;

        Piece(_top, source, width, start, firstRows, 0);
        Piece(_bottom, source, width, 0, secondRows, firstRows * _pixel);
    }

    private static void Piece(Sprite2D sprite, Texture2D source, float width, int row, int rows, float y)
    {
        if (rows <= 0) { sprite.Visible = false; return; }
        sprite.Texture = source;
        sprite.RegionRect = new Rect2(0, row, width, rows);
        sprite.Position = new Vector2(0, y);
        sprite.Visible = true;
    }
}
