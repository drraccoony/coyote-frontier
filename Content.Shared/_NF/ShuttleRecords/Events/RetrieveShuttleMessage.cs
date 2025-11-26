using Robust.Shared.Serialization;

namespace Content.Shared._NF.ShuttleRecords.Events;

/// <summary>
/// Message that is sent from the client to the server when a shuttle needs to be retrieved.
/// </summary>
[Serializable, NetSerializable]
public sealed class RetrieveShuttleMessage(NetEntity shuttleNetEntity) : BoundUserInterfaceMessage
{
    public NetEntity ShuttleNetEntity { get; set; } = shuttleNetEntity;
}
