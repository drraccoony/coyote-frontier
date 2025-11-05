using Robust.Shared.Serialization;

namespace Content.Shared._NF.Shipyard.Events;

/// <summary>
/// Message sent when player wants to store their active ship.
/// </summary>
[Serializable, NetSerializable]
public sealed class BluespaceDrydockStoreMessage : BoundUserInterfaceMessage
{
}
