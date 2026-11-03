/// <summary>How many times the player has restarted a stage in this game: three at most. It starts again at zero when a new game is begun from the menu.</summary>
public static class RunRestarts
{
    public const int Max = 3;
    public static int Used { get; set; }
    public static int Left => System.Math.Max(0, Max - Used);
}
