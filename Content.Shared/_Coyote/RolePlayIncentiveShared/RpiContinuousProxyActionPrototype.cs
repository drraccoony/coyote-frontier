using Robust.Shared.Prototypes;

namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// This is a prototype for...
/// </summary>
[Prototype("rpiContinuousProxyAction")]
public sealed partial class RpiContinuousProxyActionPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// Category of thing, cus I dont know how components work in yml
    /// </summary>
    [DataField("proxyTarget", required: true)]
    public RpiProximityMode ProxyTarget = RpiProximityMode.None;

    /// <summary>
    /// How long does it take to reach max RPI bonus?
    /// </summary>
    [DataField("minutesToMaxBonus", required: true)]
    public float MinutesToMaxBonus = 10f;

    /// <summary>
    /// The max multiplier you can get from this action.
    /// </summary>
    [DataField("maxMultiplier", required: true)]
    public float MaxMultiplier = 2f;

    /// <summary>
    /// Distance you need to be to the target entity for this to work.
    /// </summary>
    [DataField("maxDistance", required: true)]
    public float MaxDistance = 10f;

    /// <summary>
    /// Distance where the bonus gets a bonus (so you get more bonus for being closer)
    /// </summary>
    [DataField("optimalDistance")]
    public float OptimalDistance = 5f;

    /// <summary>
    /// The bonus bonus multiplier you get for being within optimal distance.
    /// </summary>
    [DataField("optimalDistanceBonusMultiplier")]
    public float OptimalDistanceBonusMultiplier = 2f;

    /// <summary>
    /// Stub localization key for the readouts in the examine thingy
    /// </summary>
    [DataField("examineTextKey")]
    public string ExamineTextKey = string.Empty;
}

/// <summary>
/// enum of things that can be proxied to
/// </summary>
public enum RpiProximityMode
{
    None,

    /// <summary>
    /// Likes to be near pirates, while not being a pirate.
    /// For capturebait bottoms who love to be hostages uwu~
    /// </summary>
    BeNearPirate,

    /// <summary>
    /// Likes to be near non-pirates, while being a pirate.
    /// For pirates who love to be surrounded by their prey~
    /// </summary>
    BeNearNonPirates,
}
