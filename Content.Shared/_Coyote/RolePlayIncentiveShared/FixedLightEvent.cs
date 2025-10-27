namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// This is the event raised when some other kind of action is taken.
/// Will be recorded as a multiplier to the next payday or so
/// </summary>
public sealed class FixedLightEvent(
    TimeSpan timeSpentBroken,
    EntityUid source,
    EntityUid lightEntity
) : EntityEventArgs
{
    /// <summary>
    /// How long the light was broken for.
    /// </summary>
    public TimeSpan TimeSpentBroken = timeSpentBroken;

    /// <summary>
    /// The source entity that caused the light to be fixed.
    /// </summary>
    public EntityUid Source = source;

    /// <summary>
    /// The light entity that was fixed.
    /// </summary>
    public EntityUid LightEntity = lightEntity;
}
