/*
 * New Frontiers - This file is licensed under AGPLv3
 * Copyright (c) 2024 New Frontiers
 * See AGPLv3.txt for details.
 */
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Content._NF.Shared.GameRule;
using Content.Server.Procedural;
using Content.Shared.Bank.Components;
using Robust.Server.GameObjects;
using Robust.Server.Maps;
using Robust.Shared.Console;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Map.Components;
using Content.Shared.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Cargo.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Maps;
using Content.Server.Station.Systems;
using Content.Shared.CCVar;
using Content.Shared.NF14.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server._NF.GameRule;

/// <summary>
/// This handles the dungeon and trading post spawning, as well as round end capitalism summary
/// </summary>
public sealed class NfAdventureRuleSystem : GameRuleSystem<AdventureRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly MapLoaderSystem _map = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly DungeonSystem _dunGen = default!;
    [Dependency] private readonly IConsoleHost _console = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;

    private readonly HttpClient _httpClient = new();
    private ISawmill _discordSawmill = default!;
    private ISawmill _miningSawmill = default!;

    // State for world generation, shared between Startup functions, not otherwise persistent
    private sealed class NfAdventureWorldGenState
    {
        // Generation parameters
        public float DistanceOffset = 1f;

        // POI prototypes
        public List<PointOfInterestPrototype> DepotProtos = new();
        public List<PointOfInterestPrototype> MarketProtos = new();
        public List<PointOfInterestPrototype> RequiredProtos = new();
        public List<PointOfInterestPrototype> OptionalProtos = new();
        public Dictionary<string, List<PointOfInterestPrototype>> UniqueProtosByGroup = new();

        // Dungeon prototypes
        public List<SpaceDungeonPrototype> DungeonProtos = new();

        // Generation state
        public List<Vector2> StationCoords = new();
        public MapId MapId = default!;
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawningEvent);
        _discordSawmill = Logger.GetSawmill("discord");
        _miningSawmill = Logger.GetSawmill("mining");
    }

    protected override void AppendRoundEndText(EntityUid uid, AdventureRuleComponent component, GameRuleComponent gameRule, ref RoundEndTextAppendEvent ev)
    {
        var profitText = Loc.GetString($"adventure-mode-profit-text");
        var lossText = Loc.GetString($"adventure-mode-loss-text");
        ev.AddLine(Loc.GetString("adventure-list-start"));
        var allScore = new List<Tuple<string, int>>();

        foreach (var player in component.NFPlayerMinds)
        {
            if (!TryComp<BankAccountComponent>(player.Item1, out var bank) || !TryComp<MetaDataComponent>(player.Item1, out var meta))
                continue;

            var profit = bank.Balance - player.Item2;
            ev.AddLine($"- {meta.EntityName} {profitText} {profit} Spesos");
            allScore.Add(new Tuple<string, int>(meta.EntityName, profit));
        }

        if (!(allScore.Count >= 1))
            return;

        var relayText = Loc.GetString("adventure-list-high");
        relayText += '\n';
        var highScore = allScore.OrderByDescending(h => h.Item2).ToList();

        for (var i = 0; i < 10 && i < highScore.Count; i++)
        {
            relayText += $"{highScore.First().Item1} {profitText} {highScore.First().Item2} Spesos";
            relayText += '\n';
            highScore.Remove(highScore.First());
        }
        relayText += Loc.GetString("adventure-list-low");
        relayText += '\n';
        highScore.Reverse();
        for (var i = 0; i < 10 && i < highScore.Count; i++)
        {
            relayText += $"{highScore.First().Item1} {lossText} {highScore.First().Item2} Spesos";
            relayText += '\n';
            highScore.Remove(highScore.First());
        }
#pragma warning disable CS4014 // Throw out a request, don't wait for it to return
        ReportRound(relayText);
#pragma warning restore CS4014 // Throw out a request, don't wait for it to return
    }

    private void OnPlayerSpawningEvent(PlayerSpawnCompleteEvent ev)
    {
        var adventureQuery = QueryActiveRules();
        if (ev.Player.AttachedEntity is { Valid: true } mobUid)
        {
            while (adventureQuery.MoveNext(out var _, out var _, out var adventureComp, out var _))
                adventureComp.NFPlayerMinds.Add((mobUid, ev.Profile.BankBalance));
            EnsureComp<CargoSellBlacklistComponent>(mobUid);
        }
    }

    protected override void Started(EntityUid uid, AdventureRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        NfAdventureWorldGenState state = new NfAdventureWorldGenState
        {
            MapId = GameTicker.DefaultMap,
            DistanceOffset = _configurationManager.GetCVar(NF14CVars.POIDistanceModifier),
            StationCoords = new List<Vector2>()
        };

        //First, we need to grab the list and sort it into its respective spawning logics

        foreach (var location in _prototypeManager.EnumeratePrototypes<PointOfInterestPrototype>())
        {
            if (location.SpawnGroup == "CargoDepot")
                state.DepotProtos.Add(location);
            else if (location.SpawnGroup == "MarketStation")
                state.MarketProtos.Add(location);
            else if (location.AlwaysSpawn == true)
                state.RequiredProtos.Add(location);
            else if (location.SpawnGroup == "Optional")
                state.OptionalProtos.Add(location);
            else // the remainder are done on a per-poi-per-group basis
            {
                if (!state.UniqueProtosByGroup.ContainsKey(location.SpawnGroup))
                    state.UniqueProtosByGroup[location.SpawnGroup] = new();
                state.UniqueProtosByGroup[location.SpawnGroup].Add(location);
            }
        }
        foreach (var location in _prototypeManager.EnumeratePrototypes<SpaceDungeonPrototype>())
        {
            state.DungeonProtos.Add(location);
        }

        GenerateDepots(state, out component.CargoDepots);
        GenerateMarkets(state, out component.MarketStations);
        GenerateRequireds(state, out component.RequiredPois);
        GenerateOptionals(state, out component.OptionalPois);
        GenerateUniques(state, out component.UniquePois);

        base.Started(uid, component, gameRule, args);

        GenerateSpaceDungeons(state);
    }

    private void GenerateDepots(NfAdventureWorldGenState state, out List<EntityUid> depotStations)
    {
        //For depots, we want them to fill a circular type dystance formula to try to keep them as far apart as possible
        //Therefore, we will be taking our range properties and treating them as magnitudes of a direction vector divided
        //by the number of depots set in our corresponding cvar

        depotStations = new List<EntityUid>();
        var depotCount = _configurationManager.GetCVar(NF14CVars.CargoDepots);
        var rotation = 2 * Math.PI / depotCount;
        var rotationOffset = _random.NextAngle() / depotCount;

        for (int i = 0; i < depotCount && state.DepotProtos.Count > 0; i++)
        {
            var proto = _random.Pick(state.DepotProtos);
            Vector2i offset = new Vector2i((int) (_random.Next(proto.RangeMin, proto.RangeMax) * state.DistanceOffset), 0);
            offset = offset.Rotate(rotationOffset);
            rotationOffset += rotation;
            string depotName = $"{proto.Name} {(char) ('A' + i)}";

            if (TrySpawnPoiGrid(state, proto, offset, _random.NextAngle(), out var depotUid, overrideName: depotName) && depotUid is { Valid: true } depot)
            {
                depotStations.Add(depot);
                AddStationCoordsToSet(state, offset); // adjust list of actual station coords
            }
        }
    }

    private void GenerateMarkets(NfAdventureWorldGenState state, out List<EntityUid> marketStations)
    {
        //For market stations, we are going to allow for a bit of randomness and a different offset configuration. We dont
        //want copies of this one, since these can be more themed and duplicate names, for instance, can make for a less
        //ideal world

        marketStations = new List<EntityUid>();
        var marketCount = _configurationManager.GetCVar(NF14CVars.MarketStations);
        _random.Shuffle(state.MarketProtos);
        int marketsAdded = 0;
        foreach (var proto in state.MarketProtos)
        {
            if (marketsAdded >= marketCount)
                break;

            var offset = GetRandomPOICoord(state, proto.RangeMin, proto.RangeMax, true);

            if (TrySpawnPoiGrid(state, proto, offset, _random.NextAngle(), out var marketUid) && marketUid is { Valid: true } market)
            {
                marketStations.Add(market);
                marketsAdded++;
                AddStationCoordsToSet(state, offset);
            }
        }
    }

    private void GenerateOptionals(NfAdventureWorldGenState state, out List<EntityUid> optionalStations)
    {
        //Stations that do not have a defined grouping in their prototype get a default of "Optional" and get put into the
        //generic random rotation of POIs. This should include traditional places like Tinnia's rest, the Science Lab, The Pit,
        //and most RP places. This will essentially put them all into a pool to pull from, and still does not use the RNG function.

        optionalStations = new List<EntityUid>();
        var optionalCount = _configurationManager.GetCVar(NF14CVars.OptionalStations);
        _random.Shuffle(state.OptionalProtos);
        int optionalsAdded = 0;
        foreach (var proto in state.OptionalProtos)
        {
            if (optionalsAdded >= optionalCount)
                break;

            var offset = GetRandomPOICoord(state, proto.RangeMin, proto.RangeMax, true);

            if (TrySpawnPoiGrid(state, proto, offset, _random.NextAngle(), out var optionalUid) && optionalUid is { Valid: true } uid)
            {
                optionalStations.Add(uid);
                AddStationCoordsToSet(state, offset);
            }
        }
    }

    private void GenerateRequireds(NfAdventureWorldGenState state, out List<EntityUid> requiredStations)
    {
        //Stations are required are ones that are vital to function but otherwise still follow a generic random spawn logic
        //Traditionally these would be stations like Expedition Lodge, NFSD station, Prison/Courthouse POI, etc.
        //There are no limit to these, and any prototype marked alwaysSpawn = true will get pulled out of any list that isnt Markets/Depots
        //And will always appear every time, and also will not be included in other optional/dynamic lists

        requiredStations = new List<EntityUid>();
        foreach (var proto in state.RequiredProtos)
        {
            var offset = GetRandomPOICoord(state, proto.RangeMin, proto.RangeMax, true);

            if (TrySpawnPoiGrid(state, proto, offset, _random.NextAngle(), out var requiredUid) && requiredUid is { Valid: true } uid)
            {
                requiredStations.Add(uid);
                AddStationCoordsToSet(state, offset);
            }
        }
    }

    private void GenerateUniques(NfAdventureWorldGenState state, out List<EntityUid> uniqueStations)
    {
        //Unique locations are semi-dynamic groupings of POIs that rely each independantly on the SpawnChance per POI prototype
        //Since these are the remainder, and logically must have custom-designated groupings, we can then know to subdivide
        //our random pool into these found groups.
        //To do this with an equal distribution on a per-POI, per-round percentage basis, we are going to ensure a random
        //pick order of which we analyze our weighted chances to spawn, and if successful, remove every entry of that group
        //entirely.

        uniqueStations = new List<EntityUid>();
        foreach (var prototypeList in state.UniqueProtosByGroup.Values)
        {
            // Try to spawn 
            _random.Shuffle(prototypeList);
            foreach (var proto in prototypeList)
            {
                var chance = _random.NextFloat(0, 1);
                if (chance <= proto.SpawnChance)
                {
                    var offset = GetRandomPOICoord(state, proto.RangeMin, proto.RangeMax, true);

                    if (TrySpawnPoiGrid(state, proto, offset, _random.NextAngle(), out var optionalUid) && optionalUid is { Valid: true } uid)
                    {
                        uniqueStations.Add(uid);
                        AddStationCoordsToSet(state, offset);
                        break;
                    }
                }
            }
        }
    }

    private void GenerateSpaceDungeons(NfAdventureWorldGenState state)
    {
        foreach (var dungeon in state.DungeonProtos)
        {
            if (!_prototypeManager.TryIndex(dungeon.DungeonConfig, out var dungeonConfig))
                continue;

            var seed = _random.Next();
            var offset = GetRandomPOICoord(state, dungeon.RangeMin, dungeon.RangeMax, true);
            if (!_map.TryLoad(state.MapId, dungeon.GridPath.ToString(), out var grids,
                    new MapLoadOptions
                    {
                        Offset = offset
                    }))
            {
                continue;
            }

            foreach (var grid in grids)
            {
                var meta = EnsureComp<MetaDataComponent>(grid);
                _meta.SetEntityName(grid, dungeon.Name, meta);
                _shuttle.SetIFFColor(grid, dungeon.IffColor);
                _shuttle.AddIFFFlag(grid, IFFFlags.HideLabel);
            }
            _console.WriteLine(null, $"dungeon spawned at {offset}");

            //pls fit the grid I beg, this is so hacky
            //its better now but i think i need to do a normalization pass on the dungeon configs
            //because they are all offset. confirmed good size grid, just need to fix all the offsets.
            var mapGrid = EnsureComp<MapGridComponent>(grids[0]);
            _dunGen.GenerateDungeon(dungeonConfig, grids[0], mapGrid, new Vector2i(0, 0), seed);
            AddStationCoordsToSet(state, offset);
        }
    }

    private bool TrySpawnPoiGrid(NfAdventureWorldGenState state, PointOfInterestPrototype proto, Vector2 offset, Angle rotation, out EntityUid? gridUid, string? overrideName = null)
    {
        gridUid = null;
        if (_map.TryLoad(state.MapId, proto.GridPath.ToString(), out var mapUids,
                new MapLoadOptions
                {
                    Offset = offset,
                    Rotation = rotation
                }))
        {
            string entityName = overrideName ?? proto.Name;
            if (_prototypeManager.TryIndex<GameMapPrototype>(proto.ID, out var stationProto))
            {
                _station.InitializeNewStation(stationProto.Stations[proto.ID], mapUids, entityName);
            }

            foreach (var grid in mapUids)
            {
                var meta = EnsureComp<MetaDataComponent>(grid);
                _meta.SetEntityName(grid, entityName, meta);
                _shuttle.SetIFFColor(grid, proto.IffColor);
                if (proto.IsHidden)
                {
                    _shuttle.AddIFFFlag(grid, IFFFlags.HideLabel);
                }
            }
            gridUid = mapUids[0];
            return true;
        }

        return false;
    }

    private Vector2 GetRandomPOICoord(NfAdventureWorldGenState state, float unscaledMinRange, float unscaledMaxRange, bool scaleRange)
    {
        int numRetries = int.Max(_configurationManager.GetCVar(NF14CVars.POIPlacementRetries), 0);
        float minDistance = float.Max(_configurationManager.GetCVar(NF14CVars.MinPOIDistance), 0); // Constant at the end to avoid NaN weirdness

        Vector2 coords = _random.NextVector2(unscaledMinRange, unscaledMaxRange);
        if (scaleRange)
            coords *= state.DistanceOffset;
        for (int i = 0; i < numRetries; i++)
        {
            bool positionIsValid = true;
            foreach (var station in state.StationCoords)
            {
                if (Vector2.Distance(station, coords) < minDistance)
                {
                    positionIsValid = false;
                    break;
                }
            }

            // We have a valid position
            if (positionIsValid)
                break;

            // No vector yet, get next value.
            coords = _random.NextVector2(unscaledMinRange, unscaledMaxRange);
            if (scaleRange)
                coords *= state.DistanceOffset;
        }

        return coords;
    }

    private void AddStationCoordsToSet(NfAdventureWorldGenState state, Vector2 coords)
    {
        state.StationCoords.Add(coords);
    }

    private async Task ReportRound(String message, int color = 0x77DDE7)
    {
        _discordSawmill.Info(message);
        String webhookUrl = _configurationManager.GetCVar(CCVars.DiscordLeaderboardWebhook);
        if (webhookUrl == string.Empty)
            return;

        var payload = new WebhookPayload
        {
            Embeds = new List<Embed>
            {
                new()
                {
                    Title = Loc.GetString("adventure-list-start"),
                    Description = message,
                    Color = color,
                },
            },
        };

        var ser_payload = JsonSerializer.Serialize(payload);
        var content = new StringContent(ser_payload, Encoding.UTF8, "application/json");
        var request = await _httpClient.PostAsync($"{webhookUrl}?wait=true", content);
        var reply = await request.Content.ReadAsStringAsync();
        if (!request.IsSuccessStatusCode)
        {
            _miningSawmill.Error($"Discord returned bad status code when posting message: {request.StatusCode}\nResponse: {reply}");
        }
    }

    // https://discord.com/developers/docs/resources/channel#message-object-message-structure
    private struct WebhookPayload
    {
        [JsonPropertyName("username")] public string? Username { get; set; } = null;

        [JsonPropertyName("avatar_url")] public string? AvatarUrl { get; set; } = null;

        [JsonPropertyName("content")] public string Message { get; set; } = "";

        [JsonPropertyName("embeds")] public List<Embed>? Embeds { get; set; } = null;

        [JsonPropertyName("allowed_mentions")]
        public Dictionary<string, string[]> AllowedMentions { get; set; } =
            new()
            {
                { "parse", Array.Empty<string>() },
            };

        public WebhookPayload()
        {
        }
    }

    // https://discord.com/developers/docs/resources/channel#embed-object-embed-structure
    private struct Embed
    {
        [JsonPropertyName("title")] public string Title { get; set; } = "";

        [JsonPropertyName("description")] public string Description { get; set; } = "";

        [JsonPropertyName("color")] public int Color { get; set; } = 0;

        [JsonPropertyName("footer")] public EmbedFooter? Footer { get; set; } = null;

        public Embed()
        {
        }
    }

    // https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure
    private struct EmbedFooter
    {
        [JsonPropertyName("text")] public string Text { get; set; } = "";

        [JsonPropertyName("icon_url")] public string? IconUrl { get; set; }

        public EmbedFooter()
        {
        }
    }
}
