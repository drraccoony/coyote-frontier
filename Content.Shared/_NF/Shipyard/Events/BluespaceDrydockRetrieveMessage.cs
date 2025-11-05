using Robust.Shared.Serialization;

namespace Content.Shared._NF.Shipyard.Events;

/// <summary>
/// Message sent when player wants to retrieve their stored ship.
/// </summary>
[Serializable, NetSerializable]
public sealed class BluespaceDrydockRetrieveMessage : BoundUserInterfaceMessage
{
}
