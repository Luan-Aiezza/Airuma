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
    /// <summary>What hangs over the water, near the camera, drawn over the boat and the boss (null: nothing).</summary>
    public Func<BankArt.Side, PixelCanvas> MakeOverhead;
    /// <summary>The foam where the banks touch the water, in two sizes that the scene alternates between.</summary>
    public Func<BankArt.Side, PixelCanvas[]> MakeRipples;
    /// <summary>When the boss comes, the river opens into the sea (the horizon, with the moon); otherwise it stays as it is.</summary>
    public bool OpensToSea;
    /// <summary>A few lanterns float on the water during the boss fight.</summary>
    public bool FloatingLanterns;
    /// <summary>The wind carries lanterns instead of leaves.</summary>
    public bool LanternWind;
    /// <summary>The wind is shown by birds that pass.</summary>
    public bool BirdWind;

    // The weather and the mood (only for the eyes: none of it changes the game, not even the wind).
    /// <summary>Rain and lightning over the river.</summary>
    public bool Rains;
    /// <summary>How far the trees of the banks sway in the wind, in px at the branches (0: they stand still).</summary>
    public float BankSway;
    /// <summary>When the boss comes the rain stops and the clouds part: the moon shines in the middle of the water (and the thunder goes on, twice as often).</summary>
    public bool ClearsForBoss;
    /// <summary>How agitated the water is, normally and in the boss fight (0: calm; 1: a lot; more: a lot more).</summary>
    public float Agitation, BossAgitation;
    /// <summary>The cave: shadows of fish swim in the water, fireflies drift over it, and in the boss fight a round platform of rock rises in the middle at the top,
    /// lit by a shaft of moonlight (the boss stands on it).</summary>
    public bool CaveLife, Platform;

    // The waterfall (see WaterfallArt): the boat is pulled up a waterfall, the water rushing up the picture and the rocks going down it fast, and at the top it comes out
    // on a lake, where the picture is as in any other stage.
    /// <summary>The water rushes up: streaks race up the picture, the bed of the river scrolls up, fish are swept up, and the rocks pass quickly; all of it calms when the fight begins.</summary>
    public bool WaterfallRush;
    /// <summary>How much faster than usual the rocks pass while the water rushes.</summary>
    public float RockSpeedFactor = 1;
    /// <summary>Seconds between two pictures of the foam where the water breaks on the rocks (the more pictures, the more violent).</summary>
    public float RipplePeriod = 0.5f;
    /// <summary>The sky of the fight over the open water, if it is not the sea's, and snow falling over it.</summary>
    public Func<PixelCanvas> MakeHorizon;
    public bool Snow;
    /// <summary>The open water of the fight is a lake that mirrors the sky as it is (not stretched, as the sea does), with no path of light under the moon: a nearly perfect reflection, a little rippled.</summary>
    public bool MirrorLake;
    /// <summary>There is no moon to make a path of light on the water until the fight begins.</summary>
    public bool Moonless;
    /// <summary>How much of the water is reflection in the river and in the lake of the fight (less: more of the bed is seen).</summary>
    public float RiverReflection = 0.82f, LakeReflection = 0.82f;
    /// <summary>A colour that the banks and the water are tinted with (white: none).</summary>
    public Godot.Color BankTint = Godot.Colors.White, WaterGrade = Godot.Colors.White;

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

    /// <summary>The storm over the black forest: dark water, dry trees, nearly black and bare, bent by the wind (with eyes that glow among them), a sky of dark
    /// clouds that reflects nothing, rain and lightning, and a water that becomes very agitated when the boss comes.</summary>
    public static readonly Scenery Storm = new Scenery
    {
        MakeSky = StormArt.MakeSky, MakeReflection = BankArt.MakeCrownsDry, MakeGround = BankArt.MakeRootsDry, MakeOverhead = BankArt.MakeBranchesDry,
        MakeRipples = BankArt.MakeRipples, OpensToSea = false, Rains = true, ClearsForBoss = true, BankSway = 8, Agitation = 0.35f, BossAgitation = 1.2f,
        BankTint = new Godot.Color(0.5f, 0.55f, 0.66f), WaterGrade = new Godot.Color(0.55f, 0.63f, 0.76f),
    };

    /// <summary>The cave: grey rock walls with glowing moss and stalactites, still water with the shadows of fish, fireflies, a roof that glows here and there. Nothing hangs over the water (the stalactites are on the ceiling: only their shadows are seen). In the
    /// boss fight the walls slide away and a round platform of rock rises where the boss stands, lit by the moon. Not as dark as the storm.</summary>
    public static readonly Scenery Cave = new Scenery
    {
        MakeSky = CaveArt.MakeSky, MakeReflection = CaveArt.MakeReflection, MakeGround = CaveArt.MakeGround, MakeOverhead = null,
        MakeRipples = CaveArt.MakeRipples, OpensToSea = false,
        CaveLife = true, Platform = true, Agitation = 0f, BossAgitation = 0f,
        BankTint = new Godot.Color(0.95f, 1f, 1f), WaterGrade = new Godot.Color(0.7f, 0.92f, 0.95f),
    };

    /// <summary>
    /// The waterfall: the boat is pulled up a great fall, the water white and rushing up between walls of wet rock that it breaks against in foam; shadows of fish are swept up
    /// with it; birds show the wind. At the top (the fight) the walls slide away and the boat is on a lake of clear water, calm and with snow falling on it, with the snowy
    /// mountains of the Andes behind the boss, who stands on the horizon.
    /// </summary>
    public static readonly Scenery Waterfall = new Scenery
    {
        MakeSky = WaterfallArt.MakeSky, MakeReflection = WaterfallArt.MakeReflection, MakeGround = WaterfallArt.MakeGround, MakeOverhead = null,
        MakeRipples = WaterfallArt.MakeRipples, OpensToSea = true, MakeHorizon = AndesArt.MakeHorizon, Snow = true, MirrorLake = true, Moonless = true, BirdWind = true,
        WaterfallRush = true, RockSpeedFactor = 2.4f, RipplePeriod = 0.11f, Agitation = 1.5f, BossAgitation = 0.06f,
        RiverReflection = 0.9f, LakeReflection = 0.6f,
        BankTint = new Godot.Color(0.95f, 0.98f, 1f), WaterGrade = new Godot.Color(1f, 1.1f, 1.18f),
    };
}
