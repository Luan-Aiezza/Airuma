using System;
using Godot;

/// <summary>The lantern's energy and the weapon level that follows it (see <see cref="PowerCurve"/>).</summary>
public static class PowerSystem
{
    /// <summary>
    /// Feeds the lantern. Once it is full the surplus is not stored: it is returned, to be worth points. The weapon level follows the
    /// energy, and `events` is told of a new level.
    /// </summary>
    public static float AddEnergy(PlayerEntity player, float amount, System.Collections.Generic.List<WorldEvent> events)
    {
        if (!(amount > 0)) return 0;
        int before = PowerCurve.Level(player.Energy);
        float total = player.Energy + amount;
        player.Energy = MathF.Min(total, PowerCurve.MaxEnergy);
        float spare = total - PowerCurve.MaxEnergy;
        int after = PowerCurve.Level(player.Energy);
        player.Weapon.PowerLevel = after;
        if (after > before) events.Add(WorldEvent.PowerUp(after));
        return MathF.Max(0, spare);
    }

    /// <summary>
    /// Dying costs part of the energy, which falls as lights from where the boat was lost: it can be gathered again
    /// before it drifts out of the picture.
    /// </summary>
    public static void LoseEnergy(PlayerEntity player, LightItem[] lights, Vector2 place)
    {
        float lost = MathF.Round(player.Energy * PowerCurve.LossOnDeath, MidpointRounding.AwayFromZero);
        if (!(lost >= 1)) return;
        player.Energy -= lost;
        player.Weapon.PowerLevel = PowerCurve.Level(player.Energy);
        int pieces = Math.Max(3, Math.Min(18, (int)lost));
        for (int k = 0; k < pieces; k++) LightSystem.Spawn(lights, place, lost / pieces, 1.5f);
    }
}
