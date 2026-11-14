/// <summary>
/// How a spirit is drawn when it is not the usual picture of its kind: the common spirits of stage 1 that fire have a figure for each way of shooting, forest spirits all (see <see cref="ForestSpiritArt"/>).
/// Default: the picture of the kind (and of the stage).
/// </summary>
public enum SpiritLook
{
    Default,
    /// <summary>The mushroom: fires a single slow shot at the boat.</summary>
    Mushroom,
    /// <summary>The moth: crosses the field dropping bullets straight down, like dust from its wings.</summary>
    Moth,
    /// <summary>The owl: fires a fan of three feathers.</summary>
    Owl,
    /// <summary>The stump: settles and works its slow ring that stops and flies at the boat.</summary>
    Stump,
    /// <summary>The flower: a slow spiral of petals.</summary>
    Bloom,
}
