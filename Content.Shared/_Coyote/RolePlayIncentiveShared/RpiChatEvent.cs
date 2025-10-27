using Content.Server._Coyote;
using Content.Shared.Chat;

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
