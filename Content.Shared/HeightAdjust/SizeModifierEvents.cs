namespace Content.Shared.HeightAdjust;

/// <summary>
/// Raised on an entity to request that its size be recalculated.
/// All systems that modify size should listen for this event and respond
/// by raising GetSizeModifierEvent to contribute their modifiers.
/// </summary>
[ByRefEvent]
public record struct RequestSizeRecalcEvent;

/// <summary>
/// Raised on an entity to collect all active size modifiers.
/// Systems that want to modify size (clothing, buffs, size gun, etc.)
/// should listen for this event and add their modifier to the collection.
/// The HeightAdjustSystem will then apply all modifiers together.
/// </summary>
[ByRefEvent]
public record struct GetSizeModifierEvent
{
    /// <summary>
    /// The entity whose size modifiers are being collected.
    /// </summary>
    public EntityUid Target { get; init; }

    /// <summary>
    /// Collection of all size modifiers that will be applied.
    /// Each system should add its modifier here.
    /// </summary>
    public List<SizeModifier> Modifiers { get; init; }

    public GetSizeModifierEvent(EntityUid target)
    {
        Target = target;
        Modifiers = new List<SizeModifier>();
    }
}

/// <summary>
/// Represents a single source of size modification.
/// </summary>
public sealed class SizeModifier
{
    /// <summary>
    /// Unique identifier for this modifier source (e.g., "SizeGun", "ClothingBoots", "GeneticBuff")
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// The scale multiplier to apply. 1.0 = normal size, 2.0 = double size, 0.5 = half size
    /// </summary>
    public float Scale { get; init; } = 1.0f;

    /// <summary>
    /// Priority for this modifier. Higher priority modifiers are applied last.
    /// Use this to ensure certain effects override others when needed.
    /// </summary>
    public int Priority { get; init; } = 0;
}
