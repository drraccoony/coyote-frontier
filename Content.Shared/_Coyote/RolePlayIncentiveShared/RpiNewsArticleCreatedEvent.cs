using System.Diagnostics.CodeAnalysis;
using Content.Shared._Coyote.RolePlayIncentiveShared;
using Content.Shared.MassMedia.Systems;

namespace Content.Server._Coyote;

/// <summary>
/// Holds the data for an action that will modify one or more RPI paywards.
/// NOT immediate pay, thats somewhedre else.
/// </summary>
public sealed class RpiNewsArticleCreatedEvent(
    NewsArticle narticle,
    EntityUid doer) : EntityEventArgs
{
    public NewsArticle NArticle = narticle;
    public EntityUid Doer = doer;

    public bool Handled = false;
}
