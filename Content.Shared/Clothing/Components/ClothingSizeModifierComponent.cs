using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// When worn, this clothing modifies the wearer's size.
/// Examples: high heels (make taller), compression clothing (make smaller), etc.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClothingSizeModifierComponent : Component
{
    /// <summary>
    /// The scale multiplier this clothing applies.
    /// 1.0 = no change, 1.1 = 10% larger, 0.9 = 10% smaller
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ScaleModifier = 1.0f;

    /// <summary>
    /// Priority for this modifier. Higher values are applied last.
    /// Default is 5 for clothing (applied before size gun at 10)
    /// </summary>
    [DataField]
    public int Priority = 5;
}
