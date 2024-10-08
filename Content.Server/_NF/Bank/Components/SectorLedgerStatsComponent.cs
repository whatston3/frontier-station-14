using Content.Shared.Cargo;
using Content.Shared._NF.CartridgeLoader.Cartridges;

namespace Content.Server._NF.Bank.Components;

/// <summary>
/// Tracks all station income statistics in the sector.
/// </summary>
[RegisterComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class SectorLedgerStatsComponent : Component
{
    [DataField]
    public LedgerStats Stats { get; set; }
}
