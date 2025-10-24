using Content.Shared._Coyote.RolePlayIncentiveShared;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Coyote;

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
