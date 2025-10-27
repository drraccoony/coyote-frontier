namespace Content.Shared._Coyote.RolePlayIncentiveShared;

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