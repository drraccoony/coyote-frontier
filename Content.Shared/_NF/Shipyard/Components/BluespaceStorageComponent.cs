using Robust.Shared.GameStates;

namespace Content.Shared._NF.Shipyard.Components;

/// <summary>
/// Attached to an ID card when a ship is stored in bluespace.
/// Stores the serialized grid data to be retrieved later.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedShipyardSystem), Other = AccessPermissions.ReadWriteExecute)]
public sealed partial class BluespaceStorageComponent : Component
{
    /// <summary>
    /// The serialized YAML data of the stored grid.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? StoredGridData = null;

    /// <summary>
    /// The name of the stored ship.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? StoredShipName = null;

    /// <summary>
    /// The full name with suffix of the stored ship.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? StoredShipFullName = null;

    /// <summary>
    /// When the ship was stored.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan StoredTime;
}
