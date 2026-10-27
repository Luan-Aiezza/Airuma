using Godot;

/// <summary>Run the simulation tests: Godot --headless --path . res://Scenes/selftest.tscn</summary>
public partial class SelfTestRunner : Node
{
    public override void _Ready()
    {
        int failures = SelfTest.Run();
        GetTree().Quit(failures);
    }
}
