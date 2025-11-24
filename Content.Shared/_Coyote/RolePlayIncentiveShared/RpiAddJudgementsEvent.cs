using Content.Shared._Coyote.RolePlayIncentiveShared;
using Robust.Shared.Prototypes;

namespace Content.Server._Coyote;

public sealed class RpiAddJudgementsEvent(
    EntityUid targetEntity,
    List<ProtoId<RpiChatJudgementModifierPrototype>> stuffToAdd)
    : EntityEventArgs
{
    public EntityUid TargetEntity = targetEntity;
    public List<ProtoId<RpiChatJudgementModifierPrototype>> StuffToAdd = stuffToAdd;
}
