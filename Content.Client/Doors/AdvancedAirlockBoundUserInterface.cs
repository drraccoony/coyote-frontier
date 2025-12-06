using Content.Client.UserInterface.Controls;
using Content.Shared.Doors.Components;
using Robust.Client.UserInterface;

namespace Content.Client.Doors;

public sealed class AdvancedAirlockBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private AdvancedAirlockWindow? _window;

    public AdvancedAirlockBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<AdvancedAirlockWindow>();
        _window.Title = Loc.GetString("advanced-airlock-window-title");
        
        _window.OnClaimPressed += () => SendMessage(new AdvancedAirlockClaimMessage());
        _window.OnAddUserPressed += (name) => SendMessage(new AdvancedAirlockAddUserMessage(name));
        _window.OnRemoveUserPressed += (name) => SendMessage(new AdvancedAirlockRemoveUserMessage(name));
        _window.OnResetPressed += () => SendMessage(new AdvancedAirlockResetMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not AdvancedAirlockBuiState buiState)
            return;

        _window?.UpdateState(buiState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        
        if (disposing)
        {
            _window?.Close();
        }
    }
}
