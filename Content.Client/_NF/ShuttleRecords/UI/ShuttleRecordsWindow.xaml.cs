﻿using System.Linq;
using Content.Shared._NF.ShuttleRecords;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._NF.ShuttleRecords.UI;

[GenerateTypedNameReferences]
public sealed partial class ShuttleRecordsWindow : DefaultWindow
{
    [Dependency] private readonly ILocalizationManager _loc = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public Action<ShuttleRecord>? OnCopyDeed;

    public ShuttleRecordsWindow()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
    }

    public void UpdateState(ShuttleRecordsConsoleInterfaceState state)
    {
        ShuttleRecordList.RemoveAllChildren();
        var viewStateList = BuildShuttleRecordListItemViewStateList(state.Records);
        foreach (var pair in viewStateList)
        {
            var listItem = new ShuttleRecordListItem(pair.ViewState);
            listItem.OnPressed += _ => OnShuttleRecordListItemPressed(pair.ShuttleRecord);
            ShuttleRecordList.AddChild(listItem);
        }
    }

    public record ShuttleRecordViewStatePair(
        ShuttleRecord ShuttleRecord,
        ShuttleRecordListItem.ViewState ViewState
    );

    private List<ShuttleRecordViewStatePair> BuildShuttleRecordListItemViewStateList(
        List<ShuttleRecord> shuttleRecords)
    {
        return shuttleRecords.Select(shuttleRecord =>
                new ShuttleRecordViewStatePair(
                    shuttleRecord,
                    new ShuttleRecordListItem.ViewState(
                        shuttleRecord.Name + shuttleRecord.Suffix,
                        disabled: false,
                        toolTip: ""
                    )
                ))
            .ToList();
    }

    private void OnShuttleRecordListItemPressed(ShuttleRecord shuttleRecord)
    {
        ShuttleRecordDetailsContainer.RemoveAllChildren();
        var shuttleStatus = ShuttleExists(netEntity: shuttleRecord.EntityUid)
            ? _loc.GetString(messageId: "shuttle-records-shuttle-status-active")
            : _loc.GetString(messageId: "shuttle-records-shuttle-status-inactive");
        var viewState = new ShuttleRecordDetailsControl.ViewState(
            shuttleName: _loc.GetString(messageId: "shuttle-records-shuttle-name-label",
                arg: ("name", shuttleRecord.Name + shuttleRecord.Suffix)),
            shuttleOwnerName: _loc.GetString(messageId: "shuttle-records-shuttle-owner-label",
                arg: ("owner", shuttleRecord.OwnerName)),
            activity: _loc.GetString(messageId: "shuttle-records-shuttle-status", arg: ("status", shuttleStatus)),
            disabled: false,
            toolTip: ""
        );
        var control = new ShuttleRecordDetailsControl(state: viewState);
        control.CopyDeedButton.OnPressed += _ => OnCopyDeed?.Invoke(shuttleRecord);
        ShuttleRecordDetailsContainer.AddChild(child: control);
    }

    private bool ShuttleExists(NetEntity netEntity)
    {
        return _entityManager.TryGetEntity(netEntity, out _);
    }
}