using Robust.Client.UserInterface;
using Content.Client.UserInterface.Fragments;
using Content.Shared._NF.CartridgeLoader.Cartridges;

namespace Content.Client._NF.CartridgeLoader.Cartridges;

public sealed partial class LedgerStatsUi : UIFragment
{
    private LedgerStatsUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new LedgerStatsUiFragment();
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is LedgerStatsUiState cast)
        {
            _fragment?.UpdateState(cast);
        }
    }
}
