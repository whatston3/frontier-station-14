﻿using System.Linq;
using Content.Client.GameTicking.Managers;
using Content.Client.Lobby;
using Content.Client.Players.PlayTimeTracking;
using Content.Client.UserInterface.Controls;
using Content.Shared.Roles;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._NF.Latejoin;

[GenerateTypedNameReferences]
public sealed partial class PickerWindow : FancyWindow
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConsoleHost _consoleHost = default!;
    [Dependency] private readonly JobRequirementsManager _jobReqs = default!;
    [Dependency] private readonly IClientPreferencesManager _preferencesManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystem = default!;
    private ClientGameTicker _gameTicker;
    private ISawmill _latejoinSawmill;

    public enum PickerType
    {
        Crew,
        Station,
    }

    private PickerType _pickerType;
    private CrewPickerControl _crewPickerControl = new CrewPickerControl();
    private StationPickerControl _stationPickerControl = new StationPickerControl();

    private Dictionary<NetEntity, Dictionary<ProtoId<JobPrototype>, int?>> _lobbyJobs = new();
    private Dictionary<NetEntity, string> _stationNames = new();
    private Dictionary<NetEntity, ResPath> _stationIcons = new();
    private Dictionary<NetEntity, LocId> _stationSubtexts = new();

    private NetEntity _lastSelection;

    public PickerWindow() : this(PickerType.Station) { }

    public PickerWindow(PickerType pickerType)
    {
        _pickerType = pickerType;
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        _gameTicker = _entitySystem.GetEntitySystem<ClientGameTicker>();
        _gameTicker.LobbyJobsAvailableUpdated += UpdateLobbyJobs;
        _latejoinSawmill = Logger.GetSawmill("latejoin");

        CrewTabButton.OnPressed += _ =>
        {
            _pickerType = PickerType.Crew;
            UpdateUi();
        };

        StationTabButton.OnPressed += _ =>
        {
            _pickerType = PickerType.Station;
            UpdateUi();
        };

        UpdateLobbyJobs(_gameTicker.JobsAvailable);
    }

    protected override void ExitedTree()
    {
        base.ExitedTree();
        _gameTicker.LobbyJobsAvailableUpdated -= UpdateLobbyJobs;
    }

    private void UpdateLobbyJobs(IReadOnlyDictionary<NetEntity, Dictionary<ProtoId<JobPrototype>, int?>> obj)
    {
        _lobbyJobs = obj.Where(kvp => kvp.Value.Values.Count != 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        _stationNames = _gameTicker.StationNames
            .Where(kvp => _gameTicker.JobsAvailable[kvp.Key].Values.Count != 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        _stationIcons = _gameTicker.StationIcons
            .Where(kvp => _gameTicker.JobsAvailable[kvp.Key].Values.Count != 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        _stationSubtexts = _gameTicker.StationSubtexts
            .Where(kvp => _gameTicker.JobsAvailable[kvp.Key].Values.Count != 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        UpdateUi();
    }

    private void UpdateUi()
    {
        ContentContainer.RemoveAllChildren();
        switch (_pickerType)
        {
            case PickerType.Crew:
                ContentContainer.AddChild(_crewPickerControl);
                break;
            case PickerType.Station:
                ContentContainer.AddChild(_stationPickerControl);
                _stationPickerControl.UpdateUi(_lobbyJobs, _stationNames, _stationSubtexts, _stationIcons);
                break;
        }
    }
}