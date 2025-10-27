using Content.Shared.FixedPoint;

namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// Complicated thing for thing for thing for thing
/// </summary>
[Serializable]
public sealed class RpiAuraData(
    EntityUid source,
    string rpiUid,
    string auraId,
    float multiplier,
    float maxDistance,
    TimeSpan timeTillFullEffect,
    TimeSpan decayDelay,
    TimeSpan decayToZeroTime)
{
    // the static aura data, stuff that is dependant on THEM

    [ViewVariables(VVAccess.ReadWrite)]
    public string RpiUid = rpiUid;

    [ViewVariables(VVAccess.ReadWrite)]
    public string AuraId = auraId; // basically the proto id, but shh

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid Source = source; // generally unneeded, as we'll be using the RPI comp's mob. just to make garbage collection harder

    [ViewVariables(VVAccess.ReadWrite)]
    public float Multiplier = multiplier;

    [ViewVariables(VVAccess.ReadWrite)]
    public float MaxDistance = maxDistance;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan TimeTillFullEffect = timeTillFullEffect;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan DecayDelay = decayDelay;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan DecayToZeroTime = decayToZeroTime;

    [ViewVariables(VVAccess.ReadWrite)]
    public double DecayCoefficient = 0f; // to falctualate

    // dynamic aura data, stuff that is dependant on US
    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan TimeSpentInAura = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan TimeOutsideAura = TimeSpan.Zero;

    public string GetUid()
    {
        return $"{RpiUid}:{AuraId}";
    }

    public void TickInRange(TimeSpan delta)
    {
        TimeSpentInAura += delta;
        // cap it at the max time
        if (TimeSpentInAura > TimeTillFullEffect)
        {
            TimeSpentInAura = TimeTillFullEffect;
        }
        TimeOutsideAura = TimeSpan.Zero;
    }

    public void TickOutOfRange(TimeSpan delta)
    {
        TimeOutsideAura += delta;
        if (TimeOutsideAura > DecayDelay)
        {
            if (DecayCoefficient == 0f)
            {
                // calculate decay coefficient
                var totalDecayTime = DecayToZeroTime.TotalSeconds + DecayDelay.TotalSeconds;
                if (totalDecayTime <= 0)
                {
                    DecayCoefficient = 1f; // linear decay
                }
                else
                {
                    DecayCoefficient = TimeTillFullEffect.TotalSeconds / totalDecayTime;
                }
            }
            delta *= DecayCoefficient;
            TimeSpentInAura -= delta;
            TimeOutsideAura = DecayDelay;
            if (TimeSpentInAura < TimeSpan.Zero)
            {
                TimeSpentInAura = TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Multiplier is actually the Max multiplier at full effect.
    /// So, this will return a float that is a standin for a percentage of how much to reward the player.
    /// 1.0 is actually no change, less than 1.0 is a penalty, more than 1.0 is a bonus.
    /// This means that if the multiplier is 1.0, return 0f.
    /// if its 2.0, return 1.0f.
    /// if its 0.5, return -0.5f.
    /// Cus these will be added up by the RPI thingy
    /// </summary>
    /// <returns>some kind of percentage float thing</returns>
    public float GetCurrentMultiplier()
    {
        if (TimeSpentInAura >= TimeTillFullEffect)
        {
            return Multiplier - 1.0f;
        }
        if (TimeSpentInAura <= TimeSpan.Zero)
        {
            return 0f;
        }
        // linear scale between 0 and full effect
        var effectPercent = (float)(TimeSpentInAura.TotalSeconds / TimeTillFullEffect.TotalSeconds);
        var currentMultiplier = 1.0f + (Multiplier - 1.0f) * effectPercent;
        // round to nearest tenth
        currentMultiplier = (float)Math.Round(currentMultiplier * 10f) / 10f;
        return currentMultiplier - 1.0f;
    }

    public bool IsFullyDecayed()
    {
        return TimeSpentInAura <= TimeSpan.Zero;
    }
}

/// <summary>
/// Event raised when Auras are requested to be checked.
/// </summary>
public sealed class RpiCheckAurasEvent : EntityEventArgs
{
    public List<RpiAuraData> DetectedAuras = new();

    public void AddAura(
        EntityUid source,
        string rpiUid,
        string auraId,
        float multiplier,
        float maxDistance,
        TimeSpan timeTillFullEffect,
        TimeSpan decayDelay,
        TimeSpan decayToZeroTime)
    {
        DetectedAuras.Add(
            new RpiAuraData(
                source,
                rpiUid,
                auraId,
                multiplier,
                maxDistance,
                timeTillFullEffect,
                decayDelay,
                decayToZeroTime));
    }

}

// when I say aura, I mean like diablo 2 paladin auras that do cool stuff
// not like aura farming, what even is that


