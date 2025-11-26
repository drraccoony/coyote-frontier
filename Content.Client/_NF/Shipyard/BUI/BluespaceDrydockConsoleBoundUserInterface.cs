using Content.Client.UserInterface.Controls;
using Content.Client._NF.Shipyard.UI;
using Content.Shared._NF.Shipyard.BUI;
using Content.Shared._NF.Shipyard.Events;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._NF.Shipyard.BUI;

[UsedImplicitly]
public sealed class BluespaceDrydockConsoleBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private BluespaceDrydockConsoleMenu? _menu;

    public BluespaceDrydockConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<BluespaceDrydockConsoleMenu>();
        _menu.OnStorePressed += OnStorePressed;
        _menu.OnRetrievePressed += OnRetrievePressed;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not BluespaceDrydockConsoleInterfaceState cast)
            return;

        _menu?.UpdateState(cast);
    }

    private void OnStorePressed()
    {
        SendMessage(new BluespaceDrydockStoreMessage());
    }

    private void OnRetrievePressed()
    {
        SendMessage(new BluespaceDrydockRetrieveMessage());
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _menu?.Dispose();
        }
    }
}
