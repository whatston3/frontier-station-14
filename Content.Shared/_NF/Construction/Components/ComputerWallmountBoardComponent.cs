using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._NF.Construction.Components;

/// <summary>
/// Used for construction graphs in building wallmount computers.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ComputerWallmountBoardComponent : Component
{
    [DataField]
    public EntProtoId? Prototype;
}
