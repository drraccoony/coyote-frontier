using Content.Shared._Coyote.RolePlayIncentiveShared;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Coyote.RolePlayIncentiveServer.JobStuff;

public sealed partial class AddRpiAuraSpecial : JobSpecial
{
    [DataField(required: true)]
    public List<ProtoId<RpiAuraPrototype>> AurasToAdd = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        entMan.EnsureComponent<RoleplayIncentiveComponent>(mob, out var rpiComp);
        var ev = new RpiAddAurasEvent(mob, AurasToAdd);
        entMan.EventBus.RaiseLocalEvent(mob, ev, true);
    }
}


