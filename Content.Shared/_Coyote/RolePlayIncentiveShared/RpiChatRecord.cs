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

    public bool ChatActionIsSpent = false;
}
