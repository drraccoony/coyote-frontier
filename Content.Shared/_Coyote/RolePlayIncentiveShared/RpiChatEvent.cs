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


/// <summary>
/// Event to modify the event that is being recorded.
/// </summary>
public sealed class RpiModifyChatRecordEvent(RpiChatRecord record) : EntityEventArgs
{
    public RpiChatRecord Record = record;

    public bool IsAction(RpiChatActionCategory action)
    {
        return Record.Action == action;
    }

    // screw you im, dumb as hell
    public void AddMultIfAction(RpiChatActionCategory action, float mod)
    {
        if (IsAction(action))
        {
            AddMultiplier(mod);
        }
    }

    public void AddMultIfActions(List<RpiChatActionCategory> actions, float mod)
    {
        foreach (var action in actions)
        {
            if (IsAction(action))
            {
                AddMultiplier(mod);
                return;
            }
        }
    }

    public void AddMultiplier(float mod)
    {
        Record.ModifyMultiplier(mod);
    }
}
