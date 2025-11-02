using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._CS.Mobs.Pouchy;

/// <summary>
/// Component for Pouchy - a kangaroo that grabs nearby players and stuffs them in its pouch
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PouchyComponent : Component
{
    /// <summary>
    /// How close a player needs to be before Pouchy grabs them
    /// </summary>
    [DataField, AutoNetworkedField]
    public float GrabRange = 2.5f;

    /// <summary>
    /// Cooldown between grab attempts (in seconds)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float GrabCooldown = 10f;

    /// <summary>
    /// Time when Pouchy can grab again
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan NextGrabTime = TimeSpan.Zero;

    /// <summary>
    /// Whether Pouchy is currently trying to grab someone
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsGrabbing = false;

    /// <summary>
    /// How long it takes to struggle free from the pouch (in seconds)
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StruggleTime = 10f;

    /// <summary>
    /// Time when the next escape attempt can happen
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan NextEscapeAttempt = TimeSpan.Zero;
}
