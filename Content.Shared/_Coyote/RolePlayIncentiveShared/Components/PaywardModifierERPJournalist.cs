using Content.Server._Coyote;
using Content.Shared._Coyote.RolePlayIncentiveShared;

namespace Content.Shared._Coyote;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
    public sealed partial class PaywardModifierERPJournalistComponent : Component
{
    /// <summary>
    /// The message category this applies to.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public RpiChatActionCategory WorksOn = RpiChatActionCategory.Speaking;

    /// <summary>
    /// The almighty multiplier.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Multiplier = 2f;
}

/// <summary>
/// And the 'system' to go with it.
/// </summary>
    public sealed partial class PaywardModifierERPJournalistSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
            SubscribeLocalEvent<PaywardModifierERPJournalistComponent, RpiModifyChatRecordEvent>(OnModifyChatRecord);
    }

    // copypaste this into all your ERPM systems
    private void OnModifyChatRecord(
        EntityUid uid,
        PaywardModifierERPJournalistComponent c,
        ref RpiModifyChatRecordEvent args)
    {
        args.AddMultIfAction(c.WorksOn, c.Multiplier);
    }
}

// boy I wish I was smart enough to come up with a more elegant way to do this
// oh well!
