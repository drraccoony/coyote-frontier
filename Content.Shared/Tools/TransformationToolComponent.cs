using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Tools;

/// <summary>
/// A digital tool that can scan entities and transform players into them.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TransformationToolComponent : Component
{
    /// <summary>
    /// The entity that was scanned to use as a transformation template.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ScannedEntity;

    /// <summary>
    /// The prototype ID of the scanned entity, for persistence across map transfers.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? ScannedPrototype;

    /// <summary>
    /// The name of the scanned entity for display purposes.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? ScannedName;

    /// <summary>
    /// Currently transformed entity and their original entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Dictionary<EntityUid, EntityUid> ActiveTransformations = new();

    /// <summary>
    /// Default transformation duration in minutes. 0 = permanent.
    /// </summary>
    [DataField]
    public float DefaultDurationMinutes = 5f; // 5 minutes

    /// <summary>
    /// Only entities with these tags can be scanned. If empty, all entities except blacklisted ones can be scanned.
    /// </summary>
    [DataField]
    public List<string> WhitelistTags = new();

    /// <summary>
    /// Entities with these tags cannot be scanned.
    /// </summary>
    [DataField]
    public List<string> BlacklistTags = new()
    {
        "HighRiskItem",
        "Document",
        "Write",
        "Book",
        "Airlock",
        "Wall",
        "AirAlarm",
        "FireAlarm",
        "Uplink"
    };

    /// <summary>
    /// Sound to play when scanning an entity.
    /// </summary>
    [DataField]
    public SoundSpecifier? ScanSound = new SoundPathSpecifier("/Audio/Items/hypospray.ogg");

    /// <summary>
    /// Sound to play when transforming an entity.
    /// </summary>
    [DataField]
    public SoundSpecifier? TransformSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}

[Serializable, NetSerializable]
public enum TransformationToolUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class TransformationToolBoundUserInterfaceState : BoundUserInterfaceState
{
    public string? ScannedName;
    public string? ScannedPrototype;
    public Dictionary<NetEntity, NetEntity> ActiveTransformations;
    public float DefaultDurationMinutes;

    public TransformationToolBoundUserInterfaceState(
        string? scannedName,
        string? scannedPrototype,
        Dictionary<NetEntity, NetEntity> activeTransformations,
        float defaultDurationMinutes)
    {
        ScannedName = scannedName;
        ScannedPrototype = scannedPrototype;
        ActiveTransformations = activeTransformations;
        DefaultDurationMinutes = defaultDurationMinutes;
    }
}

[Serializable, NetSerializable]
public sealed class TransformationToolClearScanMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class TransformationToolRevertMessage : BoundUserInterfaceMessage
{
    public NetEntity Target;

    public TransformationToolRevertMessage(NetEntity target)
    {
        Target = target;
    }
}

[Serializable, NetSerializable]
public sealed class TransformationToolRevertAllMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class TransformationToolSetDurationMessage : BoundUserInterfaceMessage
{
    public float DurationMinutes;

    public TransformationToolSetDurationMessage(float durationMinutes)
    {
        DurationMinutes = durationMinutes;
    }
}
