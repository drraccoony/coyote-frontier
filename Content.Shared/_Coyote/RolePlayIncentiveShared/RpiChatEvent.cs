using Content.Shared.Chat;
using Content.Shared.Radio;

namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// This is the event raised when a chat action is taken.
/// </summary>
public sealed class RpiChatEvent(
    EntityUid source,
    ChatChannel channel,
    string message,
    int peoplePresent = 0
    ) : EntityEventArgs
{
    public readonly EntityUid Source = source;
    public readonly ChatChannel Channel = channel;
    public readonly string Message = message;
    public readonly int PeoplePresent = peoplePresent;
}

/// <summary>
/// This is the event raised when some other kind of action is taken.
/// </summary>
public sealed class RpiActionEvent(
    EntityUid source,
    RpiActionType action,
    bool immediate = false,
    int flatPay = 0,
    float multiplier = 1f,
    bool checkPeoplePresent = true,
    string? message = null
    ) : EntityEventArgs
{
    public readonly EntityUid Source = source;
    public readonly RpiActionType Action = action;

    /// <summary>
    /// If true, the action should be processed immediately rather than waiting for the next payward.
    /// </summary>
    public readonly bool Immediate = immediate;

    /// <summary>
    /// A flat pay bonus to apply to the action.
    /// </summary>
    public readonly int FlatPay = flatPay;

    /// <summary>
    /// A multiplier to apply to the action.
    /// </summary>
    public readonly float Multiplier = multiplier;

    /// <summary>
    /// If true, the number of people present should be checked when calculating the final modifier.
    /// </summary>
    public readonly bool CheckPeoplePresent = checkPeoplePresent;

    /// <summary>
    /// An optional message to display when the action is processed.
    /// </summary>
    public readonly string? Message = message;

}



//// <summary>
/// Enum for different types of roleplay incentives
/// to help determine which components should to be check for
/// when calculating the final modifier.
/// </summary>
public enum RpiActionType : byte
{
    None,
    Mining,
    Salvage,
    Cooking,
    Bartending,
    Medical,
    Janitorial,
    Engineering,
    Atmos,
    Pilot,
    Librarian,
    Chaplain,
    Horny,
    StationPrincess,
}


