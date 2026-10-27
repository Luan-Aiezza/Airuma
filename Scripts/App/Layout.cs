/// <summary>Sizes of the picture. Game space is 600x775 (Y down); the river is the field plus half a bank of trees each side.</summary>
public static class Layout
{
    public const float Height = 775;

    /// <summary>Width of the game field: where actors' centres (and the boss's hitbox) can be.</summary>
    public const float PlayWidth = 600;

    /// <summary>Width of each bank's tree art.</summary>
    public const float BankWidth = 160;

    /// <summary>Actors may go into the trees up to their middle, so the field is inset by half a bank.</summary>
    public const float FieldInset = BankWidth / 2;

    /// <summary>The river as drawn: the field plus the inset on both sides.</summary>
    public const float RiverWidth = PlayWidth + 2 * FieldInset;

    /// <summary>The HUD panel starts a hair inside the river, so its border overlaps the edge.</summary>
    public const float PanelX = RiverWidth - 2;
    public const float PanelWidth = 352;

    public static Godot.Vector2 SceneSize => new Godot.Vector2(PanelX + PanelWidth, Height);

    /// <summary>Scene x for a game x.</summary>
    public static float SceneX(float gameX) => gameX + FieldInset;

    /// <summary>Scene (screen) position of a game point; Godot's Y axis points down like the game's.</summary>
    public static Godot.Vector2 ScenePoint(Godot.Vector2 p) => new Godot.Vector2(p.X + FieldInset, p.Y);

    /// <summary>
    /// In the boss fight the river opens into the sea: the horizon is this many pixels from the top, and the woman-boss floats with her
    /// toes exactly on it, so her position is the horizon less the distance from her chest to her feet.
    /// </summary>
    public const float Horizon = 196;
    public const float BossFeetBelowCentre = 52;
    public const float BossY = Horizon - BossFeetBelowCentre;
}
