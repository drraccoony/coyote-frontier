using Robust.Shared.Utility;

namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// This is the event raised when some other kind of action is taken.
/// Will be recorded as a multiplier to the next payday or so
/// </summary>
public sealed class RpiExamineTextEvent : EntityEventArgs
{
    /// <summary>
    /// Chonks of text to add to the examine text.
    /// </summary>
    public List<FormattedMessage> Texts = new();

    /// <summary>
    /// Adds a chunk of text to the examine text.
    /// </summary>
    public void AddText(FormattedMessage text)
    {
        Texts.Add(text);
    }
}
