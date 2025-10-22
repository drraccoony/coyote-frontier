using System.Linq;
using Content.Shared._Coyote.RolePlayIncentiveShared;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

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
    /// The sources of auras that have been detected.
    /// Dictionary of (aura comp ID, multiplier)
    /// </summary>
    [DataField]
    public Dictionary<string, RpiAuraData> DetectedAuraSources = new();
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

    /// <summary>
    /// Command to update the Auras on a player.
    /// </summary>
    public void UpdateAuras(RpiCheckAurasEvent coolEvent)
    {
        DetectedAuraSources = coolEvent.DetectedAuras.ToDictionary(x => x.Key, x => x.Value);
    }
}


/// <summary>
/// A RPI Continuous Action Proxy Datum.
/// This is used to track continuous actions that should give paywards over time.
/// </summary>
[Serializable]
public sealed class RpiContinuousActionProxyDatum(ProtoId<RpiContinuousProxyActionPrototype> proto)
{
    public ProtoId<RpiContinuousProxyActionPrototype> Proto = proto;
    public TimeSpan TotalAccumulated = TimeSpan.Zero;
    public TimeSpan LastAccumulated = TimeSpan.Zero;
    public bool IsActive = true;
    public FixedPoint2 CurrentMultiplier = 1.0f;

    private IGameTiming _gameTiming = IoCManager.Resolve<IGameTiming>();
    private IPrototypeManager _prototypeManager = IoCManager.Resolve<IPrototypeManager>();

    /// <summary>
    /// Call this every tick to accumulate time.
    /// </summary>
    private void Accumulate(float mult = 1.0f)
    {
        if (!IsActive)
        {
            return;
        }

        var delta = _gameTiming.CurTime - LastAccumulated;
        if (delta < TimeSpan.Zero)
            delta = TimeSpan.Zero;
        delta *= mult;
        TotalAccumulated += delta;
        LastAccumulated = _gameTiming.CurTime;
    }

    /// <summary>
    /// Call this every tick the player is in range of the proxy target.
    /// Handles all the fancy toggling and accumulation and stuff.
    /// </summary>
    public void TickInRange(float mult = 1.0f)
    {
        if (!IsActive)
        {
            SetActive();
            return;
        }
        Accumulate(mult);
    }

    /// <summary>
    /// Call this every tick the player is out of range of the proxy target.
    /// Handles all the fancy toggling and accumulation and stuff.
    /// </summary>
    public void TickOutOfRange()
    {
        SetInactive();
    }

    public void Reset()
    {
        TotalAccumulated = TimeSpan.Zero;
        LastAccumulated = TimeSpan.Zero;
        CurrentMultiplier = 1.0f;
        IsActive = false;
    }

    private void SetActive()
    {
        if (IsActive)
            return;
        IsActive = true;
        LastAccumulated = _gameTiming.CurTime;
    }

    private void SetInactive()
    {
        if (!IsActive)
            return;
        IsActive = false;
        LastAccumulated = TimeSpan.Zero;
    }

    public FixedPoint2 GetCurrentMultiplier()
    {
        if (!_prototypeManager.TryIndex(Proto, out var proto))
        {
            return FixedPoint2.New(1.0f);
        }
        var maxTime = TimeSpan.FromMinutes(proto.MinutesToMaxBonus);
        var curTime = TotalAccumulated;
        if (curTime > maxTime)
            return FixedPoint2.New(proto.MaxMultiplier);
        var mult = curTime.TotalSeconds / maxTime.TotalSeconds;
        mult = Math.Clamp(mult * proto.MaxMultiplier, 1.0f, proto.MaxMultiplier);
        CurrentMultiplier = FixedPoint2.New(mult);
        return CurrentMultiplier;
    }

}

