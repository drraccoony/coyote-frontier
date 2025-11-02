using Content.Server.Storage.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared._CS.Mobs.Pouchy;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Popups;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._CS.Mobs.Pouchy;

/// <summary>
/// System that makes Pouchy grab nearby players and stuff them in its pouch
/// </summary>
public sealed class PouchySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private const float UpdateInterval = 1f; // Check for nearby players every second
    private float _accumulatedTime = 0f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PouchyComponent, ContainerRelayMovementEntityEvent>(OnRelayMovement);
    }
    
    // Coyote: Override escape attempts to apply custom struggle time
    private void OnRelayMovement(EntityUid uid, PouchyComponent component, ref ContainerRelayMovementEntityEvent args)
    {
        if (!TryComp<EntityStorageComponent>(uid, out var storage))
            return;
            
        // If this is the first escape attempt, set the timer
        if (component.NextEscapeAttempt == TimeSpan.Zero)
        {
            component.NextEscapeAttempt = _timing.CurTime + TimeSpan.FromSeconds(component.StruggleTime);
            _popup.PopupEntity(Loc.GetString("pouchy-struggle"), args.Entity, args.Entity);
            _popup.PopupEntity(Loc.GetString("pouchy-struggle-observer", ("entity", args.Entity)), uid, Filter.PvsExcept(args.Entity), true);
            Dirty(uid, component);
            
            // Reset the storage's internal timer to prevent it from opening too soon
            storage.NextInternalOpenAttempt = _timing.CurTime + TimeSpan.FromSeconds(component.StruggleTime);
            Dirty(uid, storage);
            return;
        }
            
        // Check if enough time has passed for escape
        if (_timing.CurTime < component.NextEscapeAttempt)
        {
            _popup.PopupEntity(Loc.GetString("pouchy-struggle"), args.Entity, args.Entity);
            
            // Keep resetting the storage timer to prevent premature escape
            storage.NextInternalOpenAttempt = component.NextEscapeAttempt;
            Dirty(uid, storage);
            return;
        }
            
        // Time's up - allow escape and reset timer
        component.NextEscapeAttempt = TimeSpan.Zero;
        Dirty(uid, component);
        
        // Let the default storage system handle the actual opening
        // by setting its timer to allow immediate opening
        storage.NextInternalOpenAttempt = TimeSpan.Zero;
        Dirty(uid, storage);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _accumulatedTime += frameTime;
        if (_accumulatedTime < UpdateInterval)
            return;

        _accumulatedTime -= UpdateInterval;

        var query = EntityQueryEnumerator<PouchyComponent, TransformComponent, EntityStorageComponent>();
        while (query.MoveNext(out var uid, out var pouchy, out var xform, out var storage))
        {
            // Skip if on cooldown
            if (_timing.CurTime < pouchy.NextGrabTime)
                continue;

            // Skip if already has someone in pouch
            if (storage.Contents.ContainedEntities.Count > 0)
                continue;

            // Find nearby players
            var nearbyEnts = _lookup.GetEntitiesInRange(xform.Coordinates, pouchy.GrabRange);
            
            foreach (var nearbyEnt in nearbyEnts)
            {
                // Skip self
                if (nearbyEnt == uid)
                    continue;

                // Only grab mobs (players and NPCs)
                if (!HasComp<MobStateComponent>(nearbyEnt))
                    continue;

                // Try to stuff them in the pouch!
                if (_entityStorage.CanInsert(nearbyEnt, uid, storage))
                {
                    pouchy.IsGrabbing = true;
                    Dirty(uid, pouchy);

                    // Show popup to the victim and nearby players
                    _popup.PopupEntity(
                        Loc.GetString("pouchy-grab-target", ("pouchy", uid)),
                        nearbyEnt,
                        nearbyEnt,
                        PopupType.LargeCaution);

                    _popup.PopupEntity(
                        Loc.GetString("pouchy-grab-others", ("pouchy", uid), ("target", nearbyEnt)),
                        uid,
                        Filter.PvsExcept(nearbyEnt),
                        true,
                        PopupType.Medium);

                    // Insert the entity
                    if (_entityStorage.Insert(nearbyEnt, uid, storage))
                    {
                        // Set cooldown
                        pouchy.NextGrabTime = _timing.CurTime + TimeSpan.FromSeconds(pouchy.GrabCooldown);
                        // Reset escape timer so they have to struggle the full time
                        pouchy.NextEscapeAttempt = TimeSpan.Zero;
                        Dirty(uid, pouchy);
                    }

                    pouchy.IsGrabbing = false;
                    Dirty(uid, pouchy);

                    // Only grab one entity per update
                    break;
                }
            }
        }
    }
}
