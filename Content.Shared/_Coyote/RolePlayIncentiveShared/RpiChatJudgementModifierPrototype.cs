using Content.Server._Coyote;
using Robust.Shared.Prototypes;

namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// This thing modifies judgement points for chat actions, by multiplying the length of the action by this modifier.
/// </summary>
[Prototype("rpiChatJudgementModifier")]
public sealed partial class RpiChatJudgementModifierPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Modifier dict!
    /// </summary>
    [DataField("modifiers", required: true)]
    public Dictionary<RpiChatActionCategory, float> Modifiers = new()
    {
        { RpiChatActionCategory.Speaking,     1f },
        { RpiChatActionCategory.Whispering,   1f },
        { RpiChatActionCategory.Emoting,      1f },
        { RpiChatActionCategory.QuickEmoting, 1f },
        { RpiChatActionCategory.Subtling,     1f },
        { RpiChatActionCategory.Radio,        1f },
    };

    public float GetMod(RpiChatActionCategory category)
    {
        return Modifiers.GetValueOrDefault(category, 1f);
    }
}

