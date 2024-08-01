using Content.Shared.Guidebook;
using Content.Shared.Procedural;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Utility;

namespace Content._NF.Shared.GameRule;

/// <summary>
///     Describes information for a space dungeon type to be spawned in the world.
/// </summary>
[Prototype("spaceDungeon")]
[Serializable, NetSerializable]
public sealed partial class SpaceDungeonPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <inheritdoc/>
    [DataField("name")]
    public string Name { get; private set; } = default!;

    /// <summary>
    ///     Minimum range to spawn this dungeon at
    /// </summary>
    [DataField("rangeMin")]
    public int RangeMin { get; private set; } = 5000;

    /// <summary>
    ///     Maximum range to spawn this dungeon at
    /// </summary>
    [DataField("rangeMax")]
    public int RangeMax { get; private set; } = 10000;

    /// <summary>
    ///     The color to display the grid and name tag as in the radar screen
    /// </summary>
    [DataField("iffColor")]
    public Color IffColor { get; private set; } = (100, 100, 100, 100);

    /// <summary>
    ///     The path to the grid base (e.g. a lattice, an asteroid) that the dungeon spawns in.
    /// </summary>
    [DataField("gridPath", required: true)]
    public ResPath GridPath { get; private set; } = default!;

    /// <summary>
    ///     The path to the grid base (e.g. a lattice, an asteroid) that the dungeon spawns in.
    /// </summary>
    [DataField("dungeonConfig", required: true)]
    public ProtoId<DungeonConfigPrototype> DungeonConfig { get; private set; } = default!;
}
