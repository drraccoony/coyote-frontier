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
    RpiFunction function,
    int flatPay = 0,
    float multiplier = 1f,
    float peoplePresentModifier = 0f,
    string? message = null,
    int paywards = 1
    ) : EntityEventArgs
{
    public EntityUid Source = source;
    public RpiActionType Action = action;

    /// <summary>
    /// How this action should be processed.
    /// </summary>
    public RpiFunction Function = function;

    /// <summary>
    /// A flat pay bonus to apply to the action.
    /// </summary>
    public int FlatPay = flatPay;

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
    /// An optional message to display when the action is processed.
    /// </summary>
    public string? Message = message;

    /// <summary>
    /// has this action been handled?
    /// </summary>
    public bool Handled = false;
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


/// <summary>
/// A message queued up to be sent to the player, regarding their roleplay incentive actions.
/// </summary>
public sealed class RpiMessageQueue(
    string message,
    TimeSpan timeToShow)
{
    public string Message = message;
    public TimeSpan TimeToShow = timeToShow;
}
