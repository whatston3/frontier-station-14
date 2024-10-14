using Content.Shared.GameTicking;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._NF.LateJoin.Interfaces;

public abstract class BasePickerControl : PanelContainer
{
    public abstract void UpdateUi(IReadOnlyDictionary<NetEntity, StationJobInformation> obj);
}
