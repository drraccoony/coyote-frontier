using Content.Shared._Coyote;

namespace Content.Server._Coyote;

/// <summary>
/// Structure to hold the action and the time it was taken.
/// </summary>
public sealed class RpiChatRecord(
    RpiChatActionCategory action,
    TimeSpan timeTaken,
    string? message = null,
    int peoplePresent = 0
    )
{
    /// <summary>
    /// The action that was taken.
    /// </summary>
    public RpiChatActionCategory Action = action;

    /// <summary>
    /// The time the action was taken.
    /// </summary>
    public TimeSpan TimeTaken = timeTaken;

    /// <summary>
    /// The message of the action, if applicable.
    /// </summary>
    public string? Message = message;

    /// <summary>
    /// The number of people who were present when the action was taken.
    /// Not counting the person who did the action.
    /// </summary>
    public int PeoplePresent = peoplePresent;

    /// <summary>
    /// A modifier on the payward for this action
    /// Counts toward length uwu
    /// </summary>
    public float Multiplier = 1.0f;

    public bool ChatActionIsSpent = false;

    /// <summary>
    /// Additively modifies the multiplier for this action.
    /// Assumes the mod is percent form, so 1.25 adds 25% to the multiplier.
    /// </summary>
    public void ModifyMultiplier(float mod)
    {
        Multiplier += (mod - 1.0f);
    }
}
