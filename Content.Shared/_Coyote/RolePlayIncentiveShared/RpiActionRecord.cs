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
    float paywardMultiplier = 1f,
    int peoplePresent = 0,
    float peoplePresentModifier = 0f,
    int paywards = 1)
{
    public TimeSpan TimeTaken = timeTaken;
    public RpiActionType Category = category;
    private float _paywardMultiplier = paywardMultiplier;
    public int PeoplePresent = peoplePresent;
    private float _peoplePresentModifier = peoplePresentModifier;
    private int _paywards = paywards;

    public bool MiscActionIsSpent = false;

    public float GetMultiplier()
    {
        var mult = _paywardMultiplier;
        if (_peoplePresentModifier > 0.1f
            && PeoplePresent > 0)
        {
            mult *= (_peoplePresentModifier * PeoplePresent);
        }
        return mult;
    }

    public bool IsValid()
    {
        var am = _paywards > 0;
        if (!am)
            MiscActionIsSpent = true;
        return am;
    }

    public bool TryPop()
    {
        if (!IsValid())
            return false;
        _paywards--;
        return true;
    }
}
