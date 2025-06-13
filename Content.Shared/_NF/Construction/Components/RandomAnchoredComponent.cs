using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._NF.Construction.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class RandomAnchoredComponent : Component
{
    [DataField]
    public List<ProtoId<ToolQualityPrototype>> PossibleToolUses = new() { "Anchoring", "Welding", "Prying", "Cutting" };

    [DataField, AutoNetworkedField]
    public ProtoId<ToolQualityPrototype> SelectedToolUse = "Anchoring";

    /// <summary>
    /// Probability the item starts anchored
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float AnchoredProb = 0.3f;

    /// <summary>
    /// Base minimum anchorable delay.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float MinDelay = 1f;

    /// <summary>
    /// Base maximum anchorable delay.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float MaxDelay = 3f;

    /// <summary>
    /// Frontier: actual delay to use for anchoring.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CurrentDelay = 1f;
}
