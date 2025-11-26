using Content.Shared.Examine;
using Robust.Shared.Prototypes;

namespace Content.Shared._Coyote.HornyQuirks;

/// <summary>
/// This handles my balls
/// </summary>
public sealed class HornyExamineQuirksSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<HornyExamineQuirksComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, HornyExamineQuirksComponent component, ExaminedEvent args)
    {
        if (args.Examiner == args.Examined)
            return;
        if (!TryComp<HornyExamineQuirksComponent>(args.Examiner, out var examinerQuirks))
            return;
        List<string> showlocs = new();
        foreach (var showable in component.HornyShowables)
        {
            if (!_prototypeManager.TryIndex(showable, out var hornyProto))
                continue;

            // check if examiner has the needed tag
            if (examinerQuirks.HasTagToShow(hornyProto.NeededTag))
            {
                showlocs.Add(hornyProto.TextToShow);
            }
        }

        if (showlocs.Count <= 0)
            return;
        foreach (var loc in showlocs)
        {
            var translated = Loc.GetString(
                loc,
                ("target", args.Examined),
                ("examiner", args.Examiner));
            args.PushMarkup(translated);
        }
    }
}
