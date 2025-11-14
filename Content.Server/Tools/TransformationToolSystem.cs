using System.Linq;
using Content.Server.Consent;
using Content.Server.Polymorph.Systems;
using Content.Shared.Consent;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Polymorph;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Tools;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Tools;

public sealed class TransformationToolSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly ConsentSystem _consent = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private static readonly ProtoId<ConsentTogglePrototype> TransformationConsent = "Transformation";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TransformationToolComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<TransformationToolComponent, AfterActivatableUIOpenEvent>(OnUIOpened);

        SubscribeLocalEvent<TransformationToolComponent, TransformationToolClearScanMessage>(OnClearScan);
        SubscribeLocalEvent<TransformationToolComponent, TransformationToolRevertMessage>(OnRevert);
        SubscribeLocalEvent<TransformationToolComponent, TransformationToolRevertAllMessage>(OnRevertAll);
        SubscribeLocalEvent<TransformationToolComponent, TransformationToolSetDurationMessage>(OnSetDuration);
    }

    private void OnAfterInteract(EntityUid uid, TransformationToolComponent component, AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null)
            return;

        var target = args.Target.Value;

        // If we have no scanned entity, scan the target
        if (component.ScannedEntity == null)
        {
            ScanEntity(uid, component, target, args.User);
            args.Handled = true;
            return;
        }

        // If we have a scanned entity and target is a mob with a mind, transform them
        if (HasComp<Shared.Mind.Components.MindContainerComponent>(target))
        {
            TransformEntity(uid, component, target, args.User);
            args.Handled = true;
        }
    }

    private void ScanEntity(EntityUid tool, TransformationToolComponent component, EntityUid target, EntityUid user)
    {
        // Don't scan the tool itself
        if (target == tool)
            return;

        // Check blacklist tags
        if (component.BlacklistTags.Count > 0)
        {
            foreach (var tag in component.BlacklistTags)
            {
                if (_tag.HasTag(target, tag))
                {
                    _popup.PopupEntity("This entity cannot be scanned!", tool, user, PopupType.Medium);
                    return;
                }
            }
        }

        // Check whitelist tags (if any are specified)
        if (component.WhitelistTags.Count > 0)
        {
            var hasWhitelistTag = false;
            foreach (var tag in component.WhitelistTags)
            {
                if (_tag.HasTag(target, tag))
                {
                    hasWhitelistTag = true;
                    break;
                }
            }

            if (!hasWhitelistTag)
            {
                _popup.PopupEntity("This entity cannot be scanned!", tool, user, PopupType.Medium);
                return;
            }
        }

        var metaData = MetaData(target);

        component.ScannedEntity = target;
        component.ScannedPrototype = metaData.EntityPrototype?.ID;
        component.ScannedName = metaData.EntityName;

        if (component.ScanSound != null)
            _audio.PlayPvs(component.ScanSound, tool);

        _popup.PopupEntity($"Scanned: {component.ScannedName}", tool, user);

        Dirty(tool, component);
        UpdateUI(tool, component);
    }

    private void TransformEntity(EntityUid tool, TransformationToolComponent component, EntityUid target, EntityUid user)
    {
        if (component.ScannedPrototype == null)
        {
            _popup.PopupEntity("No entity scanned!", tool, user, PopupType.Medium);
            return;
        }

        // Prevent self-transformation
        if (target == user)
        {
            _popup.PopupEntity("You cannot transform yourself!", tool, user, PopupType.Medium);
            return;
        }

        // Check if target has consented to transformations
        if (!_consent.HasConsent(target, TransformationConsent))
        {
            _popup.PopupEntity($"{MetaData(target).EntityName} has not consented to transformations!", tool, user, PopupType.Medium);
            return;
        }

        // Check if already transformed
        if (component.ActiveTransformations.ContainsKey(target))
        {
            _popup.PopupEntity($"{MetaData(target).EntityName} is already transformed!", tool, user, PopupType.Medium);
            return;
        }

        // Convert minutes to seconds for polymorph system
        var durationSeconds = component.DefaultDurationMinutes * 60f;

        // Create a temporary polymorph prototype
        var polymorphConfig = new PolymorphConfiguration
        {
            Entity = component.ScannedPrototype,
            Duration = durationSeconds > 0 ? (int)durationSeconds : null,
            Forced = false,
            TransferDamage = true,
            TransferName = true,
            TransferHumanoidAppearance = false,
            Inventory = PolymorphInventoryChange.Transfer,
            RevertOnCrit = false,
            RevertOnDeath = false,
            RevertOnEat = false,
            AllowRepeatedMorphs = false,
        };

        var transformed = _polymorph.PolymorphEntity(target, polymorphConfig);

        if (transformed != null)
        {
            component.ActiveTransformations[transformed.Value] = target;

            if (component.TransformSound != null)
                _audio.PlayPvs(component.TransformSound, tool);

            _popup.PopupEntity($"Transformed {MetaData(target).EntityName} into {component.ScannedName}!", tool, user);

            Dirty(tool, component);
            UpdateUI(tool, component);
        }
    }

    private void OnUIOpened(EntityUid uid, TransformationToolComponent component, AfterActivatableUIOpenEvent args)
    {
        UpdateUI(uid, component);
    }

    private void OnClearScan(EntityUid uid, TransformationToolComponent component, TransformationToolClearScanMessage args)
    {
        component.ScannedEntity = null;
        component.ScannedPrototype = null;
        component.ScannedName = null;

        Dirty(uid, component);
        UpdateUI(uid, component);
    }

    private void OnRevert(EntityUid uid, TransformationToolComponent component, TransformationToolRevertMessage args)
    {
        var target = GetEntity(args.Target);
        if (component.ActiveTransformations.TryGetValue(target, out var original))
        {
            _polymorph.Revert(target);
            component.ActiveTransformations.Remove(target);

            Dirty(uid, component);
            UpdateUI(uid, component);
        }
    }

    private void OnRevertAll(EntityUid uid, TransformationToolComponent component, TransformationToolRevertAllMessage args)
    {
        foreach (var (transformed, original) in component.ActiveTransformations.ToList())
        {
            if (Exists(transformed))
                _polymorph.Revert(transformed);
        }

        component.ActiveTransformations.Clear();
        Dirty(uid, component);
        UpdateUI(uid, component);
    }

    private void OnSetDuration(EntityUid uid, TransformationToolComponent component, TransformationToolSetDurationMessage args)
    {
        component.DefaultDurationMinutes = Math.Clamp(args.DurationMinutes, 0, 60); // Max 60 minutes (1 hour)
        Dirty(uid, component);
        UpdateUI(uid, component);
    }

    private void UpdateUI(EntityUid uid, TransformationToolComponent component, EntityUid? user = null)
    {
        if (!_ui.HasUi(uid, TransformationToolUiKey.Key))
            return;

        var netTransformations = new Dictionary<NetEntity, NetEntity>();
        foreach (var (transformed, original) in component.ActiveTransformations)
        {
            netTransformations[GetNetEntity(transformed)] = GetNetEntity(original);
        }

        var state = new TransformationToolBoundUserInterfaceState(
            component.ScannedName,
            component.ScannedPrototype,
            netTransformations,
            component.DefaultDurationMinutes
        );

        _ui.SetUiState(uid, TransformationToolUiKey.Key, state);
    }
}
