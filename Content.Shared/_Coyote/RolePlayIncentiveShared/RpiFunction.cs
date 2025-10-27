namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// Enum for different functions thuis event can have.
/// </summary>
public enum RpiFunction : byte
{
    /// <summary>
    /// Provide a multiplier to the next payday.
    /// </summary>
    PaydayModifier,

    /// <summary>
    /// Just record the action for logging purposes.
    /// </summary>
    RecordOnly,
}