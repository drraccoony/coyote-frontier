using Content.Server.UserInterface;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Doors;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.Doors.Systems;

/// <summary>
/// Server-side system for advanced airlocks with ownership and authorization.
/// Handles BUI interactions and validates user permissions.
/// </summary>
public sealed class AdvancedAirlockSystem : SharedAdvancedAirlockSystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedIdCardSystem _idCardSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedDoorSystem _doorSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private readonly Dictionary<EntityUid, TimeSpan> _lastDenyTime = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AdvancedAirlockComponent, BoundUIOpenedEvent>(OnUIOpen);
        SubscribeLocalEvent<AdvancedAirlockComponent, AdvancedAirlockClaimMessage>(OnClaimMessage);
        SubscribeLocalEvent<AdvancedAirlockComponent, AdvancedAirlockAddUserMessage>(OnAddUserMessage);
        SubscribeLocalEvent<AdvancedAirlockComponent, AdvancedAirlockRemoveUserMessage>(OnRemoveUserMessage);
        SubscribeLocalEvent<AdvancedAirlockComponent, AdvancedAirlockResetMessage>(OnResetMessage);
        
        // Override BeforeDoorOpenedEvent to inject our custom access check
        SubscribeLocalEvent<AdvancedAirlockComponent, BeforeDoorOpenedEvent>(OnBeforeDoorOpened, before: new[] { typeof(SharedDoorSystem) });
    }

    private void OnBeforeDoorOpened(Entity<AdvancedAirlockComponent> ent, ref BeforeDoorOpenedEvent args)
    {
        // If unclaimed, use normal door access
        if (!ent.Comp.IsClaimed)
            return;

        // If no user, allow (e.g., buttons, remotes)
        if (args.User == null)
            return;

        // Check our custom access
        if (!CheckAdvancedAirlockAccess(ent, args.User.Value))
        {
            args.Cancel();
            
            // Show deny animation and popup (rate-limited to prevent spam)
            var now = _gameTiming.CurTime;
            if (!_lastDenyTime.TryGetValue(args.User.Value, out var lastTime) || (now - lastTime).TotalSeconds >= 0.5)
            {
                _lastDenyTime[args.User.Value] = now;
                
                if (TryComp<DoorComponent>(ent, out var door))
                    _doorSystem.Deny(ent, door, args.User.Value);
                    
                _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-access-denied"), ent, args.User.Value);
            }
        }
    }

    private void OnUIOpen(Entity<AdvancedAirlockComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUI(ent, args.Actor);
    }

    private void OnClaimMessage(Entity<AdvancedAirlockComponent> ent, ref AdvancedAirlockClaimMessage args)
    {
        if (!_uiSystem.IsUiOpen(ent.Owner, AdvancedAirlockUiKey.Key, args.Actor))
            return;

        // Already claimed
        if (ent.Comp.IsClaimed)
        {
            _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-already-claimed"), ent, args.Actor);
            return;
        }

        // Try to find an ID card on the user
        if (!_idCardSystem.TryFindIdCard(args.Actor, out var idCard))
        {
            _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-no-id"), ent, args.Actor);
            return;
        }

        if (!TryComp<IdCardComponent>(idCard, out var idCardComp) || idCardComp.FullName == null)
        {
            _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-invalid-id"), ent, args.Actor);
            return;
        }

        ClaimOwnership(ent, idCardComp.FullName, idCardComp.LocalizedJobTitle);
        _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-claimed"), ent, args.Actor);
        UpdateUI(ent, args.Actor);
    }

    private void OnAddUserMessage(Entity<AdvancedAirlockComponent> ent, ref AdvancedAirlockAddUserMessage args)
    {
        if (!_uiSystem.IsUiOpen(ent.Owner, AdvancedAirlockUiKey.Key, args.Actor))
            return;

        // Verify the user is the owner
        if (!_idCardSystem.TryFindIdCard(args.Actor, out var idCard))
            return;

        if (!TryComp<IdCardComponent>(idCard, out var idCardComp))
            return;

        if (!IsOwner(ent, idCardComp.FullName))
        {
            _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-not-owner"), ent, args.Actor);
            return;
        }

        // Try to find the target user's ID card (they need to be near the airlock)
        var xformQuery = GetEntityQuery<TransformComponent>();
        var xform = xformQuery.GetComponent(ent);
        var idCardQuery = AllEntityQuery<IdCardComponent, TransformComponent>();
        
        IdCardComponent? targetIdCard = null;
        while (idCardQuery.MoveNext(out var targetId, out var targetComp, out var targetXform))
        {
            if (targetComp.FullName == args.UserName && 
                targetXform.Coordinates.InRange(EntityManager, xform.Coordinates, 2f))
            {
                targetIdCard = targetComp;
                break;
            }
        }

        if (targetIdCard == null)
        {
            _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-user-not-found"), ent, args.Actor);
            return;
        }

        AddAuthorizedUser(ent, args.UserName);
        _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-user-added", ("name", args.UserName)), ent, args.Actor);
        UpdateUI(ent, args.Actor);
    }

    private void OnRemoveUserMessage(Entity<AdvancedAirlockComponent> ent, ref AdvancedAirlockRemoveUserMessage args)
    {
        if (!_uiSystem.IsUiOpen(ent.Owner, AdvancedAirlockUiKey.Key, args.Actor))
            return;

        // Verify the user is the owner
        if (!_idCardSystem.TryFindIdCard(args.Actor, out var idCard))
            return;

        if (!TryComp<IdCardComponent>(idCard, out var idCardComp))
            return;

        if (!IsOwner(ent, idCardComp.FullName))
        {
            _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-not-owner"), ent, args.Actor);
            return;
        }

        RemoveAuthorizedUser(ent, args.UserName);
        _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-user-removed", ("name", args.UserName)), ent, args.Actor);
        UpdateUI(ent, args.Actor);
    }

    private void OnResetMessage(Entity<AdvancedAirlockComponent> ent, ref AdvancedAirlockResetMessage args)
    {
        if (!_uiSystem.IsUiOpen(ent.Owner, AdvancedAirlockUiKey.Key, args.Actor))
            return;

        // Verify the user is the owner
        if (!_idCardSystem.TryFindIdCard(args.Actor, out var idCard))
            return;

        if (!TryComp<IdCardComponent>(idCard, out var idCardComp))
            return;

        if (!IsOwner(ent, idCardComp.FullName))
        {
            _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-not-owner"), ent, args.Actor);
            return;
        }

        ResetAirlock(ent);
        _popupSystem.PopupEntity(Loc.GetString("advanced-airlock-reset"), ent, args.Actor);
        UpdateUI(ent, args.Actor);
    }

    private void UpdateUI(Entity<AdvancedAirlockComponent> ent, EntityUid actor)
    {
        // Check if the actor is the owner
        var isOwner = false;
        if (_idCardSystem.TryFindIdCard(actor, out var idCard) && 
            TryComp<IdCardComponent>(idCard, out var idCardComp))
        {
            isOwner = IsOwner(ent, idCardComp.FullName);
        }

        var state = new AdvancedAirlockBuiState(
            ent.Comp.OwnerName,
            ent.Comp.OwnerJobTitle,
            new HashSet<string>(ent.Comp.AuthorizedUsers),
            ent.Comp.IsClaimed,
            isOwner
        );

        _uiSystem.SetUiState(ent.Owner, AdvancedAirlockUiKey.Key, state);
    }
}
