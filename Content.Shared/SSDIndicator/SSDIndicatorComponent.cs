using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.SSDIndicator;

/// <summary>
///     Shows status icon when player in SSD
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class SSDIndicatorComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool IsSSD = true;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    [AutoNetworkedField] // Frontier: update client when icon changes
    public ProtoId<SsdIconPrototype> Icon = "SSDIcon";

    /// <summary>
    ///     When the entity should fall asleep
    /// </summary>
    [DataField, AutoPausedField, Access(typeof(SSDIndicatorSystem))]
    public TimeSpan FallAsleepTime = TimeSpan.Zero;

    /// <summary>
    ///     Required to don't remove forced sleep from other sources
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public bool ForcedSleepAdded = false;

    // Frontier: skip sleeping
    /// <summary>
    ///     Required to don't remove forced sleep from other sources
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public bool PreventSleep = false;
    // End Frontier

    /// <summary>
    /// They went SSD at this time.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public TimeSpan WentBraindeadAt = TimeSpan.Zero;

    /// <summary>
    /// The job that was opened when they went SSD.
    /// Prevents reopening the job if they go SSD again within a certain time frame.
    /// </summary>
    public bool JobOpened = false;

    /// <summary>
    /// When they started being braindead on nash.
    /// People dont like seeing a bunch of soulless husks sitting around the bar
    /// so when it gets to idk like 3 hours, we find a cryopod and dump their dumb pu55y in it.
    /// </summary>
    [DataField]
    [AutoNetworkedField]
    public TimeSpan BraindeadNashTime = TimeSpan.Zero;

    /// <summary>
    /// if its been this long since they went SSD, we cryopod them.
    /// </summary>
    [DataField]
    public TimeSpan CryoBraindeadTimeLimit = TimeSpan.FromHours(3); // HEY DAN REMEMBER TO CHANGE THIS BACK TO 3 HOURS AFTER TESTING

}
