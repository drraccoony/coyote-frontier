// ReSharper disable InconsistentNaming

using Content.Shared._Coyote.RolePlayIncentiveShared;

namespace Content.Server._Coyote.CoolIncentives;

/// <summary>
/// Handles modifies role-playing (RP) incentives to the for people who have em,
/// and other things who are there, okay?
/// </summary>
public sealed class RpiJobModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RpiJobModifierComponent, GetRpiModifier>(OnGetRpiModifier);
    }

    private void OnGetRpiModifier(EntityUid uid, RpiJobModifierComponent component, ref GetRpiModifier args)
    {
        args.Modify(component.Multiplier, component.Additive);
    }
}

