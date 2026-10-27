/// <summary>1 = fully affected by the wind, 0 = immune.</summary>
public struct WindModifierComponent
{
    public float Value;
    public WindModifierComponent() { Value = 1; }
    public WindModifierComponent(float value) { Value = value; }
}
