using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
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
        var isSalf = args.Examiner == args.Examined;
        var targetIdent = ("target", Identity.Entity(args.Examined, EntityManager));
        var examinerIdent = ("examiner", Identity.Entity(args.Examiner, EntityManager));
        TryComp<HornyExamineQuirksComponent>(args.Examiner, out var examinerQuirks);
        List<string> showlocs = new();
        foreach (var showable in component.HornyShowables)
        {
            if (!_prototypeManager.TryIndex(showable, out var hornyProto))
                continue;

            if (isSalf)
            {
                if (hornyProto.SelfExamine)
                {
                    goto ShowIt;
                }
                continue;
            }
            if (string.IsNullOrEmpty(hornyProto.NeededTag))
            {
                goto ShowIt;
            }

            // check if examiner has the needed tag
            if (examinerQuirks is not null
                && !examinerQuirks.HasTagToShow(hornyProto.NeededTag))
            {
                continue;
            }
            ShowIt:
            showlocs.Add(hornyProto.TextToShow);
        }

        if (showlocs.Count <= 0)
            return;
        foreach (var loc in showlocs)
        {
            var translated = Loc.GetString(
                loc,
                targetIdent,
                examinerIdent);
            args.PushMarkup(translated);
        }
    }
}
