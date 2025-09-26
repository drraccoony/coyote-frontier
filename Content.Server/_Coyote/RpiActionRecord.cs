using System.Diagnostics.CodeAnalysis;
using Content.Shared._Coyote.RolePlayIncentiveShared;

namespace Content.Server._Coyote;

/// <summary>
/// Holds the data for an action that will modify one or more RPI paywards.
/// NOT immediate pay, thats somewhedre else.
/// </summary>
public sealed class RpiActionRecord(
    TimeSpan timeTaken,
    RpiActionType category,
    RpiFunction function,
    float paywardMultiplier = 1f,
    float? peoplePresentModifier = -1f,
    int? flatPay = 0,
    string? message = null,
    int? paywards = 1)
{
    public TimeSpan TimeTaken = timeTaken;
    public RpiActionType Category = category;
    public RpiFunction Function = function;
    public float? PeoplePresentModifier = peoplePresentModifier;
    public int? FlatPay = flatPay;
    public float? PaywardMultiplier = paywardMultiplier;
    public string? Message = message;
    public int? Paywards = paywards;

    public bool Handled = false;

    public bool IsValid()
    {
        return Paywards.HasValue && Paywards > 0 && !Handled;
    }

    public bool Handle()
    {
        Paywards -= 1;
        if (Paywards <= 0)
        {
            Handled = true;
        }
        return Handled;
    }
}
