namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// This is the event raised when some other kind of action is taken.
/// Will be recorded as a multiplier to the next payday or so
/// </summary>
public sealed class RpiActionMultEvent(
    EntityUid source,
    RpiActionType action,
    float multiplier = 1f,
    float peoplePresentModifier = 0f,
    int paywards = 1
) : EntityEventArgs
{
    public EntityUid Source = source;
    public RpiActionType Action = action;

    /// <summary>
    /// A multiplier to apply to the action.
    /// </summary>
    public float Multiplier = multiplier;

    /// <summary>
    /// If >0, how much should people present modify the action.
    /// </summary>
    public float PeoplePresentModifier = peoplePresentModifier;

    /// <summary>
    /// Number of paywards this action should apply to.
    /// </summary>
    public int Paywards = paywards;

    /// <summary>
    /// has this action been handled?
    /// </summary>
    public bool Handled = false;

    public bool CheckForPeoplePresent() => PeoplePresentModifier > 0f;

    public float GetPeoplePresentModifier()
    {
        if (CheckForPeoplePresent())
            return PeoplePresentModifier;
        return 1f;
    }
}
