using Content.Shared._Coyote.RolePlayIncentiveShared;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Coyote.RolePlayIncentiveServer.JobStuff;

public sealed partial class RpiChatJudgementModifierSpecial : JobSpecial
{
    [DataField(required: true)]
    public List<ProtoId<RpiChatJudgementModifierPrototype>> ModsToAdd = new();

    public override void AfterEquip(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        entMan.EnsureComponent<RoleplayIncentiveComponent>(mob, out var rpiComp);
        var ev = new RpiAddJudgementsEvent(mob, ModsToAdd);
        entMan.EventBus.RaiseLocalEvent(mob, ev, true);
    }
}


