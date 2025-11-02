using Content.Shared.Consent;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.EffectConditions;

/// <summary>
///     Requires the target entity to be above or below a certain temperature.
///     Used for things like cryoxadone and pyroxadone.
/// </summary>
public sealed partial class Consent : EntityEffectCondition
{

    [DataField(required: true)]
    public ProtoId<ConsentTogglePrototype> ConsentId = "None";

    [DataField(required: true)]
    public string Desc = "Doing the Nasty";

    public override bool Condition(EntityEffectBaseArgs args)
    {
        var consentManager = args.EntityManager.System<SharedConsentSystem>();
        return consentManager.HasConsent(args.TargetEntity, ConsentId);
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString(
            "reagent-effect-condition-guidebook-consent",
            ("toggle", Desc)); // localization is for winners
    }
}
