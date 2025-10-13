using Content.Shared.FixedPoint;

namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// Simple struct to hold data about an aura source.
/// </summary>
[Serializable]
public sealed class RpiAuraData(EntityUid source, float mult, float dist)
{
    public EntityUid Source = source;
    public float Multiplier = mult;
    public float MaxDistance = dist;
}

/// <summary>
/// Event raised when Auras are requested to be checked.
/// </summary>
public sealed class RpiCheckAurasEvent : EntityEventArgs
{
    public Dictionary<string, RpiAuraData> DetectedAuras = new();

    public void AddAura(string id, EntityUid source, float mult, float dist)
    {
        if (DetectedAuras.ContainsKey(id))
        {
            DetectedAuras[id] = new RpiAuraData(
                source,
                DetectedAuras[id].Multiplier + mult,
                Math.Max(DetectedAuras[id].MaxDistance, dist));
        }
        else
        {
            DetectedAuras.Add(id, new RpiAuraData(
                source,
                mult,
                dist));
        }
    }
}

// when I say aura, I mean like diablo 2 paladin auras that do cool stuff
// not like aura farming, what even is that


