using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.EffectConditions;

public sealed partial class EventResponse : EntityEffectCondition
{
    [DataField(required: true)]
    public string Message;

    [DataField(required: true)]
    public string Response;

    [DataField]
    public string GuidebookHelpthing = "NULL!!!";

    public override bool Condition(EntityEffectBaseArgs args)
    {
        // send an event to the target entity, to read back the response
        var ev = new EntityEffectConditionMessageEvent(args.TargetEntity, Message);
        args.EntityManager.EventBus.RaiseLocalEvent(
            args.TargetEntity,
            ev,
            true);
        return ev.HasResponse(Response);
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return GuidebookHelpthing; // localization is for losers
    }
}


// the event!
public sealed class EntityEffectConditionMessageEvent(
    EntityUid targetEntity,
    string message) : EntityEventArgs
{
    public EntityUid TargetEntity { get; } = targetEntity;
    public string Message { get; } = message;
    public List<string> Responses { get; } = new();

    public void AddResponse(string response)
    {
        if (HasResponse(response))
            return;
        Responses.Add(response);
    }

    public bool HasResponse(string response)
    {
        return Responses.Contains(response);
    }
}

