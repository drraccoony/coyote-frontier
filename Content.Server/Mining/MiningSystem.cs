using Content.Server._Coyote;
using Content.Server._Coyote.CoolIncentives;
using Content.Shared._Coyote.RolePlayIncentiveShared;
using Content.Shared.Destructible;
using Content.Shared.Mining;
using Content.Shared.Mining.Components;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Mining;

/// <summary>
/// This handles creating ores when the entity is destroyed.
/// </summary>
public sealed class MiningSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly RoleplayIncentiveSystem _RpiSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OreVeinComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<OreVeinComponent, DestructionEventArgs>(OnDestruction);
    }

    private void OnDestruction(EntityUid uid, OreVeinComponent component, DestructionEventArgs args)
    {
        if (component.CurrentOre == null)
            return;

        // Frontier
        if (component.PreventSpawning)
            return;
        // End Frontier

        var proto = _proto.Index<OrePrototype>(component.CurrentOre);

        if (args.Destroyer != null
            && TryComp<RoleplayIncentiveComponent>(args.Destroyer, out var rpi))
        {
            PayoutLoser(
                args.Destroyer.Value,
                proto.Payout,
                rpi);
        }

        if (proto.OreEntity == null)
            return;

        var coords = Transform(uid).Coordinates;
        var toSpawn = _random.Next(proto.MinOreYield, proto.MaxOreYield+1);
        for (var i = 0; i < toSpawn; i++)
        {
            Spawn(proto.OreEntity, coords.Offset(_random.NextVector2(0.2f)));
        }
    }

    private void OnMapInit(EntityUid uid, OreVeinComponent component, MapInitEvent args)
    {
        if (component.CurrentOre != null || component.OreRarityPrototypeId == null || !_random.Prob(component.OreChance))
            return;

        component.CurrentOre = _proto.Index<WeightedRandomOrePrototype>(component.OreRarityPrototypeId).Pick(_random);
    }

    private void PayoutLoser(EntityUid toPay, int payout, RoleplayIncentiveComponent rpi)
    {
        if (payout <= 0)
            return;

        _RpiSystem.GetTaxBracketDataForEntity(toPay, rpi, out var taxData);
        var finalPayout = (int) (payout * taxData.MiningMultiplier);
        if (finalPayout <= 0)
            return;
        // the paypig message
        var msg = Loc.GetString(
            "coyote-rpi-plus-fund-yellow",
            ("amount", finalPayout));
        var ev = new RpiActionEvent(
            toPay,
            RpiActionType.Mining,
            RpiFunction.Immediate,
            flatPay: finalPayout,
            checkPeoplePresent: true,
            peoplePresentModifier: 1.5f,
            message: msg);
        RaiseLocalEvent(toPay, ev);
    }
}
