using System.Linq;
using Robust.Shared.Prototypes;

namespace Content.Shared._Coyote.HornyQuirks;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class HornyExamineQuirksComponent : Component
{
    /// <summary>
    /// Showables based on quirktags
    /// </summary>
    [DataField("hornyShowables")]
    public List<ProtoId<HornyExaminePrototype>> HornyShowables = new();

    public void AddHornyExamineTrait(HornyExaminePrototype hornyProto, IPrototypeManager prototypeManager)
    {
        // does this proto already exist in my showables?
        if (HornyShowables.Any(showable => showable == hornyProto.ID))
        {
            return;
        }
        // Check if any of the HornyShowables suppress this new proto
        foreach (var showable in HornyShowables)
        {
            if (!prototypeManager.TryIndex(showable, out var existingProto))
                continue;
            if (existingProto.SuppressTags.Contains(hornyProto.NeededTag))
            {
                // don't add this proto, it's suppressed
                return;
            }
        }
        // Remove any existing showables that are suppressed by this new proto
        foreach (var showable in HornyShowables.ToList())
        {
            if (!prototypeManager.TryIndex(showable, out var existingProto))
                continue;
            if (hornyProto.SuppressTags.Contains(existingProto.NeededTag))
            {
                HornyShowables.Remove(showable);
            }
        }
        // Finally, add the new proto
        HornyShowables.Add(hornyProto.ID);
    }

    public bool HasTagToShow(ProtoId<HornyExaminePrototype> tagToShow)
    {
        if (string.IsNullOrEmpty(tagToShow))
            return true;
        return HornyShowables.Contains(tagToShow);
    }
}

