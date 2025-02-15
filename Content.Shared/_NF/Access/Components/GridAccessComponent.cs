using Content.Shared.Access.Systems;

namespace Content.Shared._NF.Access.Components;

[RegisterComponent]
[Access(typeof(SharedAccessOverriderSystem))]
public sealed partial class GridAccessComponent : Component
{
    /// <summary>
    /// The access tags that this grid requires a scoped access configurator to have.
    /// Any access configurator without tags will have access to all grids.
    /// </summary>
    [DataField]
    public List<string> Tags = new();
}
