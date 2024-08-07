using Content.Client.UserInterface.Controls;
using Content.Shared._NF.Pirate;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._NF.Pirate.UI;

[GenerateTypedNameReferences]
public sealed partial class PirateBountyRedemptionMenu : FancyWindow
{
    public Action? SellRequested;

    public PirateBountyRedemptionMenu()
    {
        RobustXamlLoader.Load(this);
        SellButton.OnPressed += OnSellPressed;
    }

    private void OnSellPressed(BaseButton.ButtonEventArgs obj)
    {
        SellRequested?.Invoke();
    }
}
