using Robust.Shared.Serialization;

namespace Content.Shared._NF.Shipyard.BUI;

[Serializable, NetSerializable]
public sealed class BluespaceDrydockConsoleInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// The name of the currently active ship (from deed), or null if no ship.
    /// </summary>
    public readonly string? ActiveShipName;

    /// <summary>
    /// The name of the stored ship, or null if no ship stored.
    /// </summary>
    public readonly string? StoredShipName;

    /// <summary>
    /// Whether there is an ID card in the console.
    /// </summary>
    public readonly bool HasIdCard;

    /// <summary>
    /// Whether the ID card has an active ship deed.
    /// </summary>
    public readonly bool HasActiveDeed;

    /// <summary>
    /// Whether the ID card has a stored ship.
    /// </summary>
    public readonly bool HasStoredShip;

    public BluespaceDrydockConsoleInterfaceState(
        string? activeShipName,
        string? storedShipName,
        bool hasIdCard,
        bool hasActiveDeed,
        bool hasStoredShip)
    {
        ActiveShipName = activeShipName;
        StoredShipName = storedShipName;
        HasIdCard = hasIdCard;
        HasActiveDeed = hasActiveDeed;
        HasStoredShip = hasStoredShip;
    }
}
