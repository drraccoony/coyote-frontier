using System.Diagnostics.CodeAnalysis;
using Content.Shared._Coyote.RolePlayIncentiveShared;

namespace Content.Server._Coyote;

/// <summary>
/// Holds the data for an action that will modify one or more RPI paywards.
/// NOT immediate pay, thats somewhedre else.
/// </summary>
public sealed class RpiImmediatePayEvent(
    TimeSpan timeTaken,
    RpiActionType category,
    int flatPay = 0,
    string? message = null,
    bool suppressChat = false) : EntityEventArgs
{
    public TimeSpan TimeTaken = timeTaken;
    public RpiActionType Category = category;
    public int FlatPay = flatPay;
    public string? Message = message;
    public bool SuppressChat = suppressChat;

    public bool Handled = false;
}
