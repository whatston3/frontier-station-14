/*
 * New Frontiers - This file is licensed under AGPLv3
 * Copyright (c) 2024 New Frontiers
 * See AGPLv3.txt for details.
 */
namespace Content.Server._NF.GameRule;

[RegisterComponent, Access(typeof(NfAdventureRuleSystem))]
public sealed partial class AdventureRuleComponent : Component
{
    // Player list
    [ViewVariables]
    public List<(EntityUid user, int balance)> NFPlayerMinds = new();

    // POI stations
    [ViewVariables]
    public List<EntityUid> CargoDepots = new();
    [ViewVariables]
    public List<EntityUid> MarketStations = new();
    [ViewVariables]
    public List<EntityUid> RequiredPois = new();
    [ViewVariables]
    public List<EntityUid> OptionalPois = new();
    [ViewVariables]
    public List<EntityUid> UniquePois = new();

    // Dungeon grids
    [ViewVariables]
    public List<EntityUid> SpaceDungeons = new();
}
