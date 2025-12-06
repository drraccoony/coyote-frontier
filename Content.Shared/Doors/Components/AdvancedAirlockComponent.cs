using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Doors.Components;

/// <summary>
/// Component for advanced airlocks that support ownership and authorization management.
/// The first user to interact with the airlock with an ID card becomes the owner.
/// The owner can then add or remove authorized users via a UI interface.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AdvancedAirlockComponent : Component
{
    /// <summary>
    /// The full name from the ID card of the owner.
    /// Null if no owner has claimed the airlock yet.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? OwnerName;

    /// <summary>
    /// The job title from the ID card of the owner.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public string? OwnerJobTitle;

    /// <summary>
    /// List of authorized user names (full names from their ID cards).
    /// These users can access the door in addition to the owner.
    /// </summary>
    [DataField, AutoNetworkedField]
    [ViewVariables(VVAccess.ReadWrite)]
    public HashSet<string> AuthorizedUsers = new();

    /// <summary>
    /// Whether the airlock has been claimed by an owner.
    /// </summary>
    [ViewVariables]
    public bool IsClaimed => OwnerName != null;
}

[Serializable, NetSerializable]
public enum AdvancedAirlockUiKey : byte
{
    Key
}

/// <summary>
/// State sent to clients to update the UI.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdvancedAirlockBuiState : BoundUserInterfaceState
{
    public string? OwnerName;
    public string? OwnerJobTitle;
    public HashSet<string> AuthorizedUsers;
    public bool IsClaimed;
    public bool IsOwner;

    public AdvancedAirlockBuiState(
        string? ownerName,
        string? ownerJobTitle,
        HashSet<string> authorizedUsers,
        bool isClaimed,
        bool isOwner)
    {
        OwnerName = ownerName;
        OwnerJobTitle = ownerJobTitle;
        AuthorizedUsers = authorizedUsers;
        IsClaimed = isClaimed;
        IsOwner = isOwner;
    }
}

/// <summary>
/// Message sent when trying to claim ownership of the airlock.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdvancedAirlockClaimMessage : BoundUserInterfaceMessage
{
}

/// <summary>
/// Message sent to add an authorized user.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdvancedAirlockAddUserMessage : BoundUserInterfaceMessage
{
    public string UserName;

    public AdvancedAirlockAddUserMessage(string userName)
    {
        UserName = userName;
    }
}

/// <summary>
/// Message sent to remove an authorized user.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdvancedAirlockRemoveUserMessage : BoundUserInterfaceMessage
{
    public string UserName;

    public AdvancedAirlockRemoveUserMessage(string userName)
    {
        UserName = userName;
    }
}

/// <summary>
/// Message sent to clear ownership and all authorized users.
/// </summary>
[Serializable, NetSerializable]
public sealed class AdvancedAirlockResetMessage : BoundUserInterfaceMessage
{
}
