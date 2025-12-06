using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Shared.Doors.Systems;

/// <summary>
/// Handles the shared logic for advanced airlocks with ownership and authorization.
/// </summary>
public abstract class SharedAdvancedAirlockSystem : EntitySystem
{
    [Dependency] protected readonly SharedIdCardSystem IdCardSystem = default!;
    [Dependency] protected readonly SharedPopupSystem PopupSystem = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem UiSystem = default!;
    [Dependency] protected readonly AccessReaderSystem AccessReaderSystem = default!;
    [Dependency] protected readonly INetManager NetManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AdvancedAirlockComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(Entity<AdvancedAirlockComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        // Try to find an ID card on the user
        if (!IdCardSystem.TryFindIdCard(args.User, out var idCard))
            return;

        if (!TryComp<IdCardComponent>(idCard, out var idCardComp))
            return;

        var user = args.User;

        // If not claimed, show a verb to claim it
        if (!ent.Comp.IsClaimed)
        {
            var claimVerb = new AlternativeVerb
            {
                Text = Loc.GetString("advanced-airlock-claim-verb"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/lock.svg.192dpi.png")),
                Act = () =>
                {
                    if (NetManager.IsServer)
                    {
                        ClaimOwnership(ent, idCardComp.FullName, idCardComp.LocalizedJobTitle);
                        PopupSystem.PopupEntity(Loc.GetString("advanced-airlock-claimed"), ent, user);
                    }
                },
                Priority = 1
            };
            args.Verbs.Add(claimVerb);
            return;
        }

        // If already claimed and user is the owner, show management verb
        if (IsOwner(ent, idCardComp.FullName))
        {
            var manageVerb = new AlternativeVerb
            {
                Text = Loc.GetString("advanced-airlock-manage-access"),
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
                Act = () =>
                {
                    if (NetManager.IsClient)
                        UiSystem.TryToggleUi(ent.Owner, AdvancedAirlockUiKey.Key, user);
                },
                Priority = 1
            };
            args.Verbs.Add(manageVerb);
        }
    }

    /// <summary>
    /// Custom access check for advanced airlocks.
    /// Integrates with AccessReaderSystem by overriding the access check.
    /// </summary>
    public bool CheckAdvancedAirlockAccess(Entity<AdvancedAirlockComponent> ent, EntityUid user)
    {
        // If unclaimed, allow normal access
        if (!ent.Comp.IsClaimed)
            return true;

        // Try to find an ID card on the user
        if (!IdCardSystem.TryFindIdCard(user, out var idCard))
            return false;

        if (!TryComp<IdCardComponent>(idCard, out var idCardComp) || idCardComp.FullName == null)
            return false;

        // Check authorization
        return IsAuthorized(ent, idCardComp.FullName);
    }

    /// <summary>
    /// Claims ownership of the airlock.
    /// </summary>
    protected void ClaimOwnership(Entity<AdvancedAirlockComponent> ent, string? ownerName, string? ownerJobTitle)
    {
        if (ent.Comp.IsClaimed || ownerName == null)
            return;

        ent.Comp.OwnerName = ownerName;
        ent.Comp.OwnerJobTitle = ownerJobTitle;
        Dirty(ent);
    }

    /// <summary>
    /// Checks if the given name is the owner of the airlock.
    /// </summary>
    public bool IsOwner(Entity<AdvancedAirlockComponent> ent, string? userName)
    {
        if (userName == null || !ent.Comp.IsClaimed)
            return false;

        return ent.Comp.OwnerName == userName;
    }

    /// <summary>
    /// Checks if the given name is authorized to use the airlock (owner or in authorized list).
    /// </summary>
    public bool IsAuthorized(Entity<AdvancedAirlockComponent> ent, string? userName)
    {
        if (userName == null || !ent.Comp.IsClaimed)
            return false;

        return IsOwner(ent, userName) || ent.Comp.AuthorizedUsers.Contains(userName);
    }

    /// <summary>
    /// Adds a user to the authorized list.
    /// </summary>
    protected void AddAuthorizedUser(Entity<AdvancedAirlockComponent> ent, string userName)
    {
        if (string.IsNullOrWhiteSpace(userName) || ent.Comp.AuthorizedUsers.Contains(userName))
            return;

        ent.Comp.AuthorizedUsers.Add(userName);
        Dirty(ent);
    }

    /// <summary>
    /// Removes a user from the authorized list.
    /// </summary>
    protected void RemoveAuthorizedUser(Entity<AdvancedAirlockComponent> ent, string userName)
    {
        if (!ent.Comp.AuthorizedUsers.Remove(userName))
            return;

        Dirty(ent);
    }

    /// <summary>
    /// Resets the airlock, removing ownership and all authorized users.
    /// </summary>
    protected void ResetAirlock(Entity<AdvancedAirlockComponent> ent)
    {
        ent.Comp.OwnerName = null;
        ent.Comp.OwnerJobTitle = null;
        ent.Comp.AuthorizedUsers.Clear();
        Dirty(ent);
    }
}
