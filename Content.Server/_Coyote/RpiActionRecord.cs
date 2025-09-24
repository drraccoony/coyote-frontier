using System.Diagnostics.CodeAnalysis;
using Content.Shared._Coyote.RolePlayIncentiveShared;

namespace Content.Server._Coyote;

public sealed class RpiActionRecord(
    TimeSpan timeTaken,
    RpiActionType category,
    RpiFunction function,
    float? peoplePresentModifier,
    int flatPay = 0,
    float paywardMultiplier = 1f,
    string? message = null)
{
    public TimeSpan TimeTaken = timeTaken;
    public RpiActionType Category = category;
    public RpiFunction Function = function;
    public float? PeoplePresentModifier = peoplePresentModifier;
    public int? FlatPay = flatPay;
    public float? PaywardMultiplier = paywardMultiplier;
    public string? Message = message;

    public bool Handled = false;
}
