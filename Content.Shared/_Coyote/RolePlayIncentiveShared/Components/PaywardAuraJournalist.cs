using Content.Shared._Coyote.RolePlayIncentiveShared;

namespace Content.Shared._Coyote;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PaywardAuraJournalistComponent : Component
{
    /// <summary>
    /// The range at which the aura applies.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Range = 10.0f;

    /// <summary>
    /// The almighty multiplier.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Multiplier = 1.75f;
}

/// <summary>
/// And the 'system' to go with it.
/// </summary>
public sealed partial class PaywardAuraJournalistSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PaywardAuraJournalistComponent, RpiCheckAurasEvent>(OnGetAuraData);
    }

    // copypaste this into all your aura systems
    private void OnGetAuraData(EntityUid uid, PaywardAuraJournalistComponent c, ref RpiCheckAurasEvent args)
    {
        args.AddAura(
            c.GetType().Name,
            uid,
            c.Range,
            c.Multiplier);
    }
}

// boy I wish I was smart enough to come up with a more elegant way to do this
// oh well!
