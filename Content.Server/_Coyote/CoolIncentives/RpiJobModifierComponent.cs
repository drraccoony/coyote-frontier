namespace Content.Server._Coyote.CoolIncentives;

/// <summary>
/// Represents a component that provides role-playing (RP) incentive multipliers
/// for food service workers, encouraging engagement in cooking activities.
/// </summary>
[RegisterComponent]
public sealed partial class RpiJobModifierComponent : Component
{
    /// <summary>
    /// This is a multiplier value (default is 1f) used to modify cooking-related calculations.
    /// </summary>
    [DataField("mult")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Multiplier = 1f;

    /// <summary>
    /// This is an additive value (default is 0) used to modify cooking-related calculations.
    /// </summary>
    [DataField("add")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Additive = 0;
}
