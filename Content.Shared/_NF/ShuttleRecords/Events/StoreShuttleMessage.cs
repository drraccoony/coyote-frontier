using Robust.Shared.Serialization;

namespace Content.Shared._NF.ShuttleRecords.Events;

/// <summary>
/// Message that is sent from the client to the server when a shuttle needs to be stored.
/// </summary>
[Serializable, NetSerializable]
public sealed class StoreShuttleMessage(NetEntity shuttleNetEntity) : BoundUserInterfaceMessage
{
    public NetEntity ShuttleNetEntity { get; set; } = shuttleNetEntity;
}
