using System.Linq;
using Content.Shared._Coyote.RolePlayIncentiveShared;
using Robust.Shared.Prototypes;

namespace Content.Server._Coyote;

/// <summary>
/// Hi! This is the RP incentive component.
/// This will track the actions a player does, and adjust some paywards
/// for them once if they do those things, sometimes!
/// </summary>
[RegisterComponent]
public sealed partial class RoleplayIncentiveComponent : Component
{
    /// <summary>
    /// My unique RPI ID.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string RpiId = string.Empty;

    /// <summary>
    /// The actions that have taken place.
    /// </summary>
    [DataField]
    public List<RpiChatRecord> ChatActionsTaken = new();

    [DataField]
    public List<RpiActionRecord> MiscActionsTaken = new();

    [DataField]
    public List<RpiMessageQueue> MessagesToShow = new();

    /// <summary>
    /// The last time the system checked for actions, for paywards.
    /// </summary>
    [DataField]
    public DateTime LastCheck = DateTime.MinValue;

    /// <summary>
    /// The next time the system will check for actions, for paywards.
    /// </summary>
    [DataField]
    public TimeSpan NextPayward = TimeSpan.Zero;

    /// <summary>
    /// Interval between paywards.
    /// </summary>
    [DataField]
    public TimeSpan PaywardInterval = TimeSpan.FromMinutes(20);

    #region Continuous Action Proxies
    /// <summary>
    /// Continuous proxy datums
    /// </summary>
    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<ProtoId<RpiContinuousProxyActionPrototype>, RpiContinuousActionProxyDatum> Proxies = new();

    [DataField]
    [ViewVariables(VVAccess.ReadWrite)]
    public List<ProtoId<RpiContinuousProxyActionPrototype>> AllowedProxies = new()
    {
        "rpiContinuousProxyActionLikesPirates",
        "rpiContinuousProxyActionLikesNonPiratesWhilePirate",
        "rpiContinuousProxyActionLikesShuttleConsoles",
    };

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextProxyCheck = TimeSpan.Zero;
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ProxyCheckInterval = TimeSpan.FromSeconds(10);
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextProxySync = TimeSpan.Zero;
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan ProxySyncInterval = TimeSpan.FromSeconds(5);
    #endregion

    #region Aura Farming
    /// <summary>
    /// All the auras I am currently generating
    /// </summary>
    [DataField]
    public List<RpiAuraData> AuraHolder = new();

    /// <summary>
    /// All the auras I have been affected by and are tracking
    /// </summary>
    [DataField]
    public List<RpiAuraData> AuraTracker = new();

    /// <summary>
    /// Next time to check auras
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextAuraCheck = TimeSpan.Zero;

    /// <summary>
    /// Interval between aura checks
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan AuraCheckInterval = TimeSpan.FromSeconds(10);

    [ViewVariables(VVAccess.ReadWrite)]
    public bool DebugIgnoreState = false;
    #endregion

    #region Journalism
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastArticleTime = TimeSpan.Zero;
    #endregion

    #region Janitation
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int LightFixPay = 200;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int LightFixOnNashBonus = 100;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int LightFixByJanitorBonus = 200;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int LightFixOnOtherShuttlesBonus = 50;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int LightFixOnOtherStationsBonus = 150;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LightFixTimeBrokenBonusThreshold = TimeSpan.FromHours(1);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LightFixTimeBrokenMaxBonusThreshold = TimeSpan.FromHours(5);

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int LightFixCashPerHourBroken = 100;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public List<TimeSpan> LightSpree = new();

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float LightSpreeBonusPerLight = 0.1f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan MaxLightSpreeTime = TimeSpan.FromMinutes(10);
    #endregion

    #region Death and Deep Fryer Punishments
    /// <summary>
    /// The last time they were PUNISHED for DYING like a noob.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastDeathPunishment = TimeSpan.Zero;

    /// <summary>
    /// The last time they were PUNISHED for hopping in the fukcing deep fryer, you LRP frick.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastDeepFryerPunishment = TimeSpan.Zero;

    /// <summary>
    /// Punish dying?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool PunishDeath = false;

    /// <summary>
    /// Punish deep frying?
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool PunishDeepFryer = false;

    [ViewVariables(VVAccess.ReadWrite)]
    public int TaxBracketPayoutOverride = -1; // -1 means no override, use the default payouts
    [ViewVariables(VVAccess.ReadWrite)]
    public int TaxBracketDeathPenaltyOverride = -1; // -1 means no override, use the default payouts
    [ViewVariables(VVAccess.ReadWrite)]
    public int TaxBracketDeepFryerPenaltyOverride = -1; // -1 means no override, use the default payouts

    [ViewVariables(VVAccess.ReadWrite)]
    public float DebugMultiplier = 1.0f;
    #endregion
}
