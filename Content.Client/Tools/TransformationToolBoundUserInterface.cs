using Content.Shared.Tools;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client.Tools;

public sealed class TransformationToolBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private TransformationToolWindow? _window;

    public TransformationToolBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<TransformationToolWindow>();
        _window.OnClearScan += () => SendMessage(new TransformationToolClearScanMessage());
        _window.OnRevert += (target) => SendMessage(new TransformationToolRevertMessage(target));
        _window.OnRevertAll += () => SendMessage(new TransformationToolRevertAllMessage());
        _window.OnSetDuration += (duration) => SendMessage(new TransformationToolSetDurationMessage(duration));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not TransformationToolBoundUserInterfaceState cast)
            return;

        _window.UpdateState(cast);
    }
}
