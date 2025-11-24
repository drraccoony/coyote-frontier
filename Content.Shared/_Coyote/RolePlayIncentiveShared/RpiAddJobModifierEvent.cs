using Content.Shared._Coyote.RolePlayIncentiveShared;
using Robust.Shared.Prototypes;

namespace Content.Server._Coyote;

public sealed class RpiAddJobModifierEvent(
    EntityUid targetEntity,
    List<ProtoId<RpiJobModifierPrototype>> stuffToAdd)
    : EntityEventArgs
{
    public EntityUid TargetEntity = targetEntity;
    public List<ProtoId<RpiJobModifierPrototype>> StuffToAdd = stuffToAdd;
}
