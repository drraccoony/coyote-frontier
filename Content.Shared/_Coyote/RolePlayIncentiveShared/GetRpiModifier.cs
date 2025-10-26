using Content.Shared.Chat;
using Content.Shared.Radio;

namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// This is the event raised when a roleplay incentive action is taken.
/// </summary>
public sealed class GetRpiModifier(EntityUid src, float baseMultiplier = 1f)
    : EntityEventArgs
{
    public EntityUid Source = src;
    public float Multiplier = baseMultiplier;

    /// <summary>
    /// This is used to modify the roleplay incentive multiplier and additive values.
    /// </summary>
    public void Modify(float multiplier)
    {
        // multipliers are additive
        Multiplier += (multiplier - 1f);
    }

    public void Modify(float multiplier, float additive)
    {
        Modify(multiplier); // lol
    }
}
