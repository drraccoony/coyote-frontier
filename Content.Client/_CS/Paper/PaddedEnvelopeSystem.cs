using Content.Shared._CS.Paper;
using Robust.Client.GameObjects;

namespace Content.Client._CS.Paper;

/// <summary>
/// Handles visual updates for padded envelopes based on their state
/// </summary>
public sealed class PaddedEnvelopeSystem : VisualizerSystem<PaddedEnvelopeComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PaddedEnvelopeComponent, AfterAutoHandleStateEvent>(OnAfterAutoHandleState);
    }

    private void OnAfterAutoHandleState(Entity<PaddedEnvelopeComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateAppearance(ent);
    }

    private void UpdateAppearance(Entity<PaddedEnvelopeComponent> ent, SpriteComponent? sprite = null)
    {
        if (!Resolve(ent.Owner, ref sprite))
            return;

        sprite.LayerSetVisible(PaddedEnvelopeVisualLayers.Open, ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Open);
        sprite.LayerSetVisible(PaddedEnvelopeVisualLayers.Sealed, ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Sealed);
        sprite.LayerSetVisible(PaddedEnvelopeVisualLayers.Torn, ent.Comp.State == PaddedEnvelopeComponent.PaddedEnvelopeState.Torn);
    }

    public enum PaddedEnvelopeVisualLayers : byte
    {
        Open,
        Sealed,
        Torn
    }
}
