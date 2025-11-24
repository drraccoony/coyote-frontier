using Content.Shared.Clothing.Components;
using Content.Shared.HeightAdjust;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Shared.Clothing;

/// <summary>
/// Handles size modifications from worn clothing.
/// </summary>
public sealed class ClothingSizeModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<ClothingSizeModifierComponent, GetSizeModifierEvent>(OnGetSizeModifier);
        SubscribeLocalEvent<ClothingSizeModifierComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ClothingSizeModifierComponent, GotUnequippedEvent>(OnUnequipped);
    }

    /// <summary>
    /// Contribute this clothing's size modifier to the total
    /// </summary>
    private void OnGetSizeModifier(EntityUid uid, ClothingSizeModifierComponent component, ref GetSizeModifierEvent args)
    {
        // Only contribute if this clothing is worn by the target
        if (Transform(uid).ParentUid != args.Target)
            return;

        args.Modifiers.Add(new SizeModifier
        {
            Source = $"Clothing_{uid}",
            Scale = component.ScaleModifier,
            Priority = component.Priority
        });
    }

    /// <summary>
    /// When clothing with size modifier is equipped, recalculate wearer's size
    /// </summary>
    private void OnEquipped(EntityUid uid, ClothingSizeModifierComponent component, GotEquippedEvent args)
    {
        var recalcEvent = new RequestSizeRecalcEvent();
        RaiseLocalEvent(args.Equipee, ref recalcEvent);
    }

    /// <summary>
    /// When clothing with size modifier is unequipped, recalculate wearer's size
    /// </summary>
    private void OnUnequipped(EntityUid uid, ClothingSizeModifierComponent component, GotUnequippedEvent args)
    {
        var recalcEvent = new RequestSizeRecalcEvent();
        RaiseLocalEvent(args.Equipee, ref recalcEvent);
    }
}
