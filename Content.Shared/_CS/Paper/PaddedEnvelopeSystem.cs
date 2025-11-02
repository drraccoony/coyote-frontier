using System.Linq;
using Content.Shared.DoAfter;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Content.Shared.Examine;
using Robust.Shared.Containers;

namespace Content.Shared._CS.Paper;

/// <summary>
/// System for handling padded envelopes that can hold multiple small items
/// </summary>
public sealed class PaddedEnvelopeSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly SharedStorageSystem _storageSystem = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaddedEnvelopeComponent, ContainerIsInsertingAttemptEvent>(OnInsertAttempt);
        SubscribeLocalEvent<PaddedEnvelopeComponent, ContainerGettingRemovedAttemptEvent>(OnRemoveAttempt);
        SubscribeLocalEvent<PaddedEnvelopeComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<PaddedEnvelopeComponent, PaddedEnvelopeDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<PaddedEnvelopeComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<PaddedEnvelopeComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Sealed)
        {
            args.PushMarkup(Loc.GetString("padded-envelope-sealed-examine", ("envelope", ent.Owner)));
        }
        else if (ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Torn)
        {
            args.PushMarkup(Loc.GetString("padded-envelope-torn-examine", ("envelope", ent.Owner)));
        }
    }

    private void OnGetAltVerbs(Entity<PaddedEnvelopeComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Torn)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString(ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Open ? "padded-envelope-verb-seal" : "padded-envelope-verb-tear"),
            IconEntity = GetNetEntity(ent.Owner),
            Act = () =>
            {
                TryStartDoAfter(ent, user, ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Open ? ent.Comp.SealDelay : ent.Comp.TearDelay);
            },
        });
    }

    private void OnInsertAttempt(Entity<PaddedEnvelopeComponent> ent, ref ContainerIsInsertingAttemptEvent args)
    {
        // Only allow inserting when the envelope is open
        if (ent.Comp.State != PaddedEnvelopeComponent.PaddedEnvelopeState.Open)
        {
            args.Cancel();
        }
    }

    private void OnRemoveAttempt(Entity<PaddedEnvelopeComponent> ent, ref ContainerGettingRemovedAttemptEvent args)
    {
        // Prevent removing items when sealed
        if (ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Sealed)
        {
            args.Cancel();
        }
    }

    private void TryStartDoAfter(Entity<PaddedEnvelopeComponent> ent, EntityUid user, TimeSpan delay)
    {
        if (ent.Comp.EnvelopeDoAfter.HasValue)
            return;

        var doAfterEventArgs = new DoAfterArgs(EntityManager, user, delay, new PaddedEnvelopeDoAfterEvent(), ent.Owner, ent.Owner)
        {
            BreakOnDamage = true,
            NeedHand = true,
            BreakOnHandChange = true,
            MovementThreshold = 0.01f,
            DistanceThreshold = 1.0f,
        };

        if (_doAfterSystem.TryStartDoAfter(doAfterEventArgs, out var doAfterId))
            ent.Comp.EnvelopeDoAfter = doAfterId;
    }

    private void OnDoAfter(Entity<PaddedEnvelopeComponent> ent, ref PaddedEnvelopeDoAfterEvent args)
    {
        ent.Comp.EnvelopeDoAfter = null;

        if (args.Cancelled)
            return;

        if (ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Open)
        {
            _audioSystem.PlayPredicted(ent.Comp.SealSound, ent.Owner, args.User);
            ent.Comp.State = PaddedEnvelopeComponent.PaddedEnvelopeState.Sealed;
            Dirty(ent.Owner, ent.Comp);
        }
        else if (ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Sealed)
        {
            _audioSystem.PlayPredicted(ent.Comp.TearSound, ent.Owner, args.User);
            ent.Comp.State = PaddedEnvelopeComponent.PaddedEnvelopeState.Torn;
            Dirty(ent.Owner, ent.Comp);

            // Eject all contents to the user when torn open
            if (_containerSystem.TryGetContainer(ent.Owner, "storagebase", out var container))
            {
                var containedEntities = container.ContainedEntities.ToArray();
                foreach (var item in containedEntities)
                {
                    _containerSystem.RemoveEntity(ent.Owner, item);
                    // Try to put items in user's hands, otherwise drop them
                    if (!_containerSystem.TryGetContainingContainer((args.User, null), out var userContainer))
                    {
                        Transform(item).Coordinates = Transform(args.User).Coordinates;
                    }
                }
            }
        }
    }
}
