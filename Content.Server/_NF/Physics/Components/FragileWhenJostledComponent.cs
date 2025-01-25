using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared.Conveyor;

/// <summary>
/// Indicates this entity takes damage when colliding with other entities
/// </summary>
[RegisterComponent]
public sealed partial class FragileWhenJostledComponent : Component
{
    [ViewVariables]
    public List<EntityUid> Jostling = new();

    [ViewVariables]
    public EntityCoordinates LastPosition;

    [ViewVariables]
    public MapCoordinates LastWorldPosition;
}
