using System;

/// <summary>
/// How the river of a stage looks, as data: the sky that the water reflects, the banks (what is reflected of them, the ground drawn over the boat, the
/// things that hang over the water, the ripples where they touch it) and what happens when the boss comes. A new stage with a new look is a new
/// scenery here (see <see cref="FestivalArt"/> for the model of the art); <see cref="RiverBackground"/> builds the river from whichever it is given.
/// </summary>
public sealed class Scenery
{
    public Func<PixelCanvas> MakeSky;
    /// <summary>What the water reflects of the banks.</summary>
    public Func<BankArt.Side, PixelCanvas> MakeReflection;
    /// <summary>The ground of the banks, drawn over the boat and the boss.</summary>
    public Func<BankArt.Side, PixelCanvas> MakeGround;
    /// <summary>What hangs over the water, near the camera, drawn over the boat and the boss.</summary>
    public Func<BankArt.Side, PixelCanvas> MakeOverhead;
    /// <summary>The foam where the banks touch the water, in two sizes that the scene alternates between.</summary>
    public Func<BankArt.Side, PixelCanvas[]> MakeRipples;
    /// <summary>When the boss comes, the river opens into the sea (the horizon, with the moon); otherwise it stays as it is.</summary>
    public bool OpensToSea;
    /// <summary>A few lanterns float on the water during the boss fight.</summary>
    public bool FloatingLanterns;
    /// <summary>The wind carries lanterns instead of leaves.</summary>
    public bool LanternWind;

    /// <summary>The jungle: a night sky with its moon and stars, trees on the banks, leaves in the wind.</summary>
    public static readonly Scenery Jungle = new Scenery
    {
        MakeSky = SkyArt.MakeCanvas, MakeReflection = BankArt.MakeCrowns, MakeGround = BankArt.MakeRoots, MakeOverhead = BankArt.MakeBranches,
        MakeRipples = BankArt.MakeRipples, OpensToSea = true,
    };

    /// <summary>The festival: a dark sky, a party on the banks, strings of lights over the water, lanterns in the wind.</summary>
    public static readonly Scenery Festival = new Scenery
    {
        MakeSky = FestivalArt.MakeSky, MakeReflection = FestivalArt.MakeReflection, MakeGround = FestivalArt.MakeGround,
        MakeOverhead = FestivalArt.MakeStrings, MakeRipples = FestivalArt.MakeRipples, FloatingLanterns = true, LanternWind = true,
    };
}
