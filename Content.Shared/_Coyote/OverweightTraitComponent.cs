namespace Content.Shared._Coyote;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class OverweightTraitComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextCreak = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan CreakDelay = TimeSpan.FromSeconds(10);
}
