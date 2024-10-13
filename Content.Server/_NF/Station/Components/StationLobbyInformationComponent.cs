using Robust.Shared.Utility;

namespace Content.Server._NF.Station.Components;

[RegisterComponent]
public sealed partial class StationLobbyInformationComponent : Component
{
    /// <summary>
    /// An optional icon path.
    /// </summary>
    [DataField]
    public ResPath? IconPath;

    /// <summary>
    /// A short description of this station/ship.
    /// </summary>
    [DataField]
    public LocId? StationSubtext;

    /// <summary>
    /// Long form text describing what this station does.
    /// </summary>
    [DataField]
    public LocId? StationDescription;

    /// <summary>
    /// The order in which this station should be displayed in the lobby.  Lower numbers display first, 0 is unordered and will display at the bottom.
    /// </summary>
    /// <remarks>
    /// Higher values are displayed later.
    /// These are currently the set values, and as an example, this is how they are sorted:
    /// 1 -- Frontier Outpost
    /// 2 -- NFSD Outpost
    /// 3 -- Expedition Lodge
    /// 0 -- Will be listed below sorted stations, no order guaranteed. (default)
    /// </remarks>
    [DataField]
    public int LobbySortOrder;

    /// <summary>
    /// If true, this station is categorized as a latejoin station (and not a ship) in the latejoin menu.
    /// </summary>
    [DataField]
    public bool IsLateJoinStation = true;
}
