using Content.Shared._Coyote.RolePlayIncentiveShared;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Coyote.RolePlayIncentiveServer.JobStuff;

public sealed partial class AddRpiJobModifierSpecial : JobSpecial
{
    [DataField(required: true)]
    public List<ProtoId<RpiJobModifierPrototype>> ModifiersToAdd = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        entMan.EnsureComponent<RoleplayIncentiveComponent>(mob, out var rpiComp);
        var ev = new RpiAddJobModifierEvent(mob, ModifiersToAdd);
        entMan.EventBus.RaiseLocalEvent(mob, ev, true);
    }
}


