using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server._Coyote.CoolIncentives;
using Content.Server._NF.Bank;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.Popups;
using Content.Shared._Coyote.RolePlayIncentiveShared;
using Content.Shared._NF.Bank.Components;
using Content.Shared.Chat;
using Content.Shared.FixedPoint;
using Content.Shared.Ghost;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nutrition.Components;
using Content.Shared.Popups;
using Content.Shared.SSDIndicator;
using Content.Shared.Tag;
using Robust.Server.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using YamlDotNet.Core.Tokens;

// ReSharper disable InconsistentNaming

namespace Content.Server._Coyote;

/// <summary>
/// This handles...
/// </summary>
public sealed class RoleplayIncentiveSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly BankSystem _bank = null!;
    [Dependency] private readonly PopupSystem _popupSystem = null!;
    [Dependency] private readonly ChatSystem _chatsys = null!;
    [Dependency] private readonly IChatManager _chatManager = null!;
    [Dependency] private readonly IPlayerManager _playerManager = null!;
    [Dependency] private readonly SSDIndicatorSystem _ssdThing = null!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;

    private List<ProtoId<RpiTaxBracketPrototype>> RpiDatumPrototypes = new()
    {
        "rpiTaxBracketBroke",
        "rpiTaxBracketEstablished",
        "rpiTaxBracketWealthy",
    };
    private ProtoId<RpiTaxBracketPrototype> TaxBracketDefault = "rpiTaxBracketDefault";
    private Dictionary<RpiChatActionCategory, string> ChatActionLookup = new()
    {
        { RpiChatActionCategory.Speaking, "rpiChatActionSpeaking" },
        { RpiChatActionCategory.Whispering, "rpiChatActionWhispering" },
        { RpiChatActionCategory.Emoting, "rpiChatActionEmoting" },
        { RpiChatActionCategory.QuickEmoting, "rpiChatActionQuickEmoting" },
        { RpiChatActionCategory.Subtling, "rpiChatActionSubtling" },
        { RpiChatActionCategory.Radio, "rpiChatActionRadio" },
    };

    private TimeSpan DeathPunishmentCooldown = TimeSpan.FromMinutes(30);
    private TimeSpan DeepFryerPunishmentCooldown = TimeSpan.FromMinutes(5); // please stop deep frying tesharis

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<RoleplayIncentiveComponent, ComponentInit>          (OnComponentInit);
        SubscribeLocalEvent<RoleplayIncentiveComponent, RpiChatEvent>           (OnGotRpiChatEvent);
        SubscribeLocalEvent<RoleplayIncentiveComponent, RpiActionMultEvent>     (OnGotRpiActionEvent);
        SubscribeLocalEvent<RoleplayIncentiveComponent, RpiImmediatePayEvent>   (OnRpiImmediatePayEvent);
        SubscribeLocalEvent<RoleplayIncentiveComponent, GetRpiModifier>         (OnSelfSucc);
        SubscribeLocalEvent<RoleplayIncentiveComponent, MobStateChangedEvent>   (OnGotMobStateChanged);
        SortTaxBrackets();
    }

    /// <summary>
    /// Sorts the tax brackets by cash threshold, lowest to highest.
    /// This is done so that we can easily find the correct tax bracket for a player.
    /// </summary>
    private void SortTaxBrackets()
    {
        RpiDatumPrototypes.Sort(
            (a, b) =>
            {
                if (!_prototype.TryIndex(a, out var protoA))
                {
                    Log.Warning($"RpiTaxBracketPrototype {a} not found!");
                    return 0;
                }

                if (!_prototype.TryIndex(b, out var protoB))
                {
                    Log.Warning($"RpiTaxBracketPrototype {b} not found!");
                    return 0;
                }

                return protoA.CashThreshold.CompareTo(protoB.CashThreshold);
            });
    }

    #region Event Handlers
    private void OnComponentInit(EntityUid uid, RoleplayIncentiveComponent component, ComponentInit args)
    {
        // set the next payward time
        component.NextPayward = _timing.CurTime + component.PaywardInterval;
    }

    /// <summary>
    /// This is called when a roleplay incentive event is received.
    /// It checks if it should be done, then it does it when it happensed
    /// </summary>
    /// <param name="uid">The entity that did the thing</param>
    /// <param name="rpic">The roleplay incentive component on the entity</param>
    /// <param name="args">The roleplay incentive event that was received</param>
    /// <remarks>
    /// piss
    /// </remarks>
    private void OnGotRpiChatEvent(
        EntityUid uid,
        RoleplayIncentiveComponent rpic,
        RpiChatEvent args)
    {
        ProcessRoleplayIncentiveEvent(uid, args);
    }

    /// <summary>
    /// Adds a modifier to the next payward, based on the event.
    /// Duplicate events will take the highest modifier.
    /// Is a multiplier!!!
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="rpic"></param>
    /// <param name="args"></param>
    private void OnGotRpiActionEvent(
        EntityUid uid,
        RoleplayIncentiveComponent? rpic,
        RpiActionMultEvent args)
    {
        if (!Resolve(uid, ref rpic))
            return;
        if (args.Handled)
            return;
        args.Handled = true;
        // make the record
        var now = _timing.CurTime;
        var peoplePresent = -1; // cant really know this one yet
        if (args.CheckForPeoplePresent())
            peoplePresent = GetPeopleInRange(uid, 10f); // 10 tile radius
        var action = new RpiActionRecord(
            now,
            args.Action,
            args.Multiplier,
            peoplePresent,
            args.GetPeoplePresentModifier(),
            args.Paywards);
        // add it tothe actions taken
        rpic.MiscActionsTaken.Add(action);
    }

    /// <summary>
    /// Immediately pays the player, bypassing the normal payward system.
    /// This is used for things like mining, which should give immediate rewards.
    /// </summary>
    private void OnRpiImmediatePayEvent(
        EntityUid uid,
        RoleplayIncentiveComponent? rpic,
        RpiImmediatePayEvent args)
    {
        if (!Resolve(uid, ref rpic))
            return;
        if (args.Handled)
            return;
        args.Handled = true;
        var basePay = args.FlatPay;
        ModifyImmediatePay(
            uid,
            rpic,
            args,
            ref basePay);
        // process the payment details
        var payDetails = ProcessPaymentDetails(
            basePay,
            1f);
        // pay the player
        if (!_bank.TryBankDeposit(uid, payDetails.FinalPay))
        {
            Log.Warning($"Failed to deposit {payDetails.FinalPay} into bank account of entity {uid}!");
            return;
        }

        ShowPopup(
            uid,
            payDetails,
            "coyote-rpi-immediate-pay-message",
            args.SuppressChat);
        if (!args.SuppressChat)
        {
            ShowChatMessageSimple(
                uid,
                payDetails,
                "coyote-rpi-immediate-pay-popup");
        }
    }

    /*
     * None
     * Local -> RpiChatActionCategory.Speaking
     * Whisper -> RpiChatActionCategory.Whispering
     * Server
     * Damage
     * Radio -> RpiChatActionCategory.Radio
     * LOOC
     * OOC
     * Visual
     * Notifications
     * Emotes -> RpiChatActionCategory.Emoting OR RpiChatActionCategory.QuickEmoting
     * Dead
     * Admin
     * AdminAlert
     * AdminChat
     * Unspecified
     * Telepathic
     * Subtle -> RpiChatActionCategory.Subtling
     * rest are just null
     */

    /// <summary>
    /// Applies the self success multiplier to the payward
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="component"></param>
    /// <param name="args"></param>
    private void OnSelfSucc(
        EntityUid uid,
        RoleplayIncentiveComponent component,
        ref GetRpiModifier args)
    {
        if (TryComp<SSDIndicatorComponent>(uid, out var ssd)
            && _ssdThing.IsInNashStation(uid))
        {
            args.Modify(1.5f, 0f); // 'double' pay if youre in nash!
        }
    }

    /// <summary>
    /// If the mob dies, punish them for being awful
    /// </summary>
    private void OnGotMobStateChanged(
        EntityUid uid,
        RoleplayIncentiveComponent rpic,
        MobStateChangedEvent args)
    {
        if (!rpic.PunishDeath)
            return;
        if (args.NewMobState != MobState.Dead)
            return;
        var curTime = _timing.CurTime;
        // if they died recently, dont punish them again
        if (curTime < rpic.LastDeathPunishment + DeathPunishmentCooldown)
            return;
        PunishPlayerForDeath(uid, rpic);
    }
    #endregion

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RoleplayIncentiveComponent>();
        while (query.MoveNext(out var uid, out var rpic))
        {
            if (!_playerManager.TryGetSessionByEntity(uid, out var _))
                return; // only players pls
            if (TryComp<GhostComponent>(uid, out var ghost))
                return; // no ghosts pls
            if (_mobStateSystem.IsDead(uid))
                return; // no dead ppl pls
            if (_timing.CurTime >= rpic.NextProxyCheck)
            {
                ProcessContinuousProxies(uid, rpic);
            }
            if (_timing.CurTime >= rpic.NextPayward)
            {
                rpic.NextPayward = _timing.CurTime + rpic.PaywardInterval;
                PayoutPaywardToPlayer(uid, rpic);
            }
        }
    }

    #region Payward Action
    /// <summary>
    /// Goes through all the relevant actions taken and stored, judges them,
    /// And gives the player a payward if they did something good.
    /// It also checks for things like duplicate actions, if theres people around, etc.
    /// Basically if you do stuff, you get some pay for it!
    /// </summary>
    private void PayoutPaywardToPlayer(EntityUid uid, RoleplayIncentiveComponent rpic)
    {
        if (!_bank.TryGetBalance(uid, out var hasThisMuchMoney))
            return; // no bank account, no pramgle
        //first check if this rpic is actually on the uid
        if (!TryComp<RoleplayIncentiveComponent>(uid, out var incentive))
        {
            Log.Warning($"RoleplayIncentiveComponent not found on entity {uid}!");
            return;
        }

        var taxBracket = GetTaxBracketData(rpic, hasThisMuchMoney);

        // ChatPay gets an int as our base pay
        var chatPay = GetChatActionPay(uid, rpic, taxBracket);
        // MiscPay gets a multiplier to modify the base pay
        var miscMult = GetMiscActionPayMult(uid, rpic, taxBracket);
        // Continuous proxies are applied here too, as multipliers
        var proxyMult = GetProxiesPayMult(rpic, true);
        // other components wanteing to messing with me
        var modMult = 1f;
        var modifyEvent = new GetRpiModifier(uid, modMult);
        RaiseLocalEvent(
            uid,
            modifyEvent,
            true);
        if (Math.Abs(rpic.DebugMultiplier - 1f) > 0.001f)
        {
            Log.Info($"RPI Debug Multiplier applied: {rpic.DebugMultiplier}");
            modifyEvent.Multiplier = rpic.DebugMultiplier;
        }

        var finalMult = 1f;
        finalMult += (miscMult - 1f);
        finalMult += (proxyMult - 1f);
        finalMult += (modifyEvent.Multiplier - 1f);
        var payDetails = ProcessPaymentDetails(
            chatPay,
            finalMult);

        // pay the player
        if (!_bank.TryBankDeposit(uid, payDetails.FinalPay))
        {
            Log.Warning($"Failed to deposit {payDetails.FinalPay} into bank account of entity {uid}!");
            return;
        }
        ShowPopup(uid, payDetails);
        ShowChatMessage(uid, payDetails);
        PruneOldActions(incentive);
    }
    #endregion

    #region Continuous Proxy Actions
    /// <summary>
    /// Checks the component's continuous proxies, and processes them.
    /// Easy peasy.
    /// </summary>
    private void ProcessContinuousProxies(EntityUid uid, RoleplayIncentiveComponent rpic)
    {
        rpic.NextProxyCheck = _timing.CurTime + rpic.ProxyCheckInterval;
        if (rpic.Proxies.Count == 0) // now when I say proxy, I mean proximity
            return; // no proxies, no pramgle
        foreach (var (proxKind, proxData) in rpic.Proxies)
        {
            if (!_prototype.TryIndex(proxData.Proto, out var proxProto))
            {
                Log.Warning($"RpiProxyPrototype {proxData.Proto} not found!");
                continue;
            }

            ProtoId<TagPrototype>? otherWantTag = default!;
            ProtoId<TagPrototype>? otherExcludeTag = default!;
            ProtoId<TagPrototype>? selfWantTag = default!;
            ProtoId<TagPrototype>? selfExcludeTag = default!;
            switch (proxData.Target)
            {
                case RpiProximityMode.BeNearPirate:
                    otherWantTag = "Pirate";
                    selfExcludeTag = "Pirate";
                    break;
                case RpiProximityMode.BeNearNonPirates:
                    otherExcludeTag = "Pirate";
                    selfWantTag = "Pirate";
                    break;
                case RpiProximityMode.None:
                default:
                    continue; // we dont care about the rest yet
            }
            // first first, check if ANY want tags are set
            if (otherWantTag == null
                && otherExcludeTag == null
                && selfWantTag == null
                && selfExcludeTag == null)
            {
                continue; // no tags to check, skip
            }

            // first, check if we have any self-exclude tags
            if (selfExcludeTag != null
                && _tagSystem.HasTag(uid, selfExcludeTag.Value))
            {
                continue; // we have a tag that excludes us from this proxy
            }
            // then, check if we have any self-want tags
            if (selfWantTag != null
                && !_tagSystem.HasTag(uid, selfWantTag.Value))
            {
                continue; // we dont have a tag that includes us in this proxy
            }
            // ok, lets roll through all the connected players,
            // and poll them for tags and distance
            var ourCoords = Transform(uid).Coordinates;
            var somethingHappened = false;
            foreach (var sesh in _playerManager.Sessions)
            {
                if (sesh.AttachedEntity is not { } otherEnt)
                    continue; // no entity, no pramgle
                if (otherEnt == uid)
                    continue; // dont check ourselves
                if (!_mobStateSystem.IsAlive(otherEnt))
                    continue; // They must be alive and well to count
                // check if they have any other-exclude tags
                if (otherExcludeTag != null
                    && _tagSystem.HasTag(otherEnt, otherExcludeTag.Value))
                {
                    continue; // they have a tag that excludes them from this proxy
                }
                // check if they have any other-want tags
                if (otherWantTag != null
                    && !_tagSystem.HasTag(otherEnt, otherWantTag.Value))
                {
                    continue; // they dont have a tag that includes them in this proxy
                }
                // now check distance
                if (!ourCoords.TryDistance(
                        EntityManager,
                        Transform(otherEnt).Coordinates,
                        out var dist))
                    continue; // cant get distance, no pramgle
                if (dist > proxProto.MaxDistance)
                    continue; // too far away, no pramgle
                var isOptimal = dist <= proxProto.OptimalDistance;
                var optMult = isOptimal ? proxProto.OptimalDistanceBonusMultiplier : 1f;
                proxData.TickInRange(optMult);
                somethingHappened = true;
            }

            if (!somethingHappened)
            {
                // we didnt find anyone, so set inactive
                proxData.TickOutOfRange();
            }
        }
    }

    /// <summary>
    /// Gets the total multiplier from all active proxies.
    /// </summary>
    private float GetProxiesPayMult(
        RoleplayIncentiveComponent rpic,
        bool pop = false
        )
    {
        float totalMult = 1f;
        var now = _timing.CurTime;
        foreach (var (proxKind, proxData) in rpic.Proxies)
        {
            var mult = proxData.GetCurrentMultiplier();
            // however we will be applying this to the total multiplier additively
            totalMult += (mult.Float() - 1f);
        }
        return totalMult;
    }
    #endregion

    #region Punishment Actions

    /// <summary>
    /// Punishes the player for dying, based on their tax bracket.
    /// This will take money from their bank account, based on their tax bracket.
    /// </summary>
    private void PunishPlayerForDeath(EntityUid uid, RoleplayIncentiveComponent rpic)
    {
        if (!_bank.TryGetBalance(uid, out var hasThisMuchMoney))
            return; // no bank account, no pramgle
        var taxBracket = GetTaxBracketData(rpic, hasThisMuchMoney);
        var penalty = taxBracket.DeathPenalty;
        if (penalty > hasThisMuchMoney)
            penalty = hasThisMuchMoney; // cant take more than they have
        if (penalty <= 0)
            return; // no penalty, no punishment
        if (!_bank.TryBankWithdraw(uid, (int)penalty))
        {
            Log.Warning($"Failed to withdraw {penalty} from bank account of entity {uid}!");
            return;
        }
        rpic.LastDeathPunishment = _timing.CurTime;
        // tell them they got punished
        var message = Loc.GetString(
            "coyote-rpi-death-penalty-message",
            ("amount", (int)penalty));
        if (_playerManager.TryGetSessionByEntity(uid, out var session))
        {
            _chatManager.ChatMessageToOne(
                ChatChannel.Notifications,
                message,
                message,
                default,
                false,
                session.Channel);
        }
        // also show a popup
        _popupSystem.PopupEntity(
            message,
            uid,
            uid,
            PopupType.LargeCaution);
    }

    #endregion

    #region Helpers

    private void ModifyImmediatePay(
        EntityUid uid,
        RoleplayIncentiveComponent? rpic,
        RpiImmediatePayEvent args,
        ref int basePay)
    {
        if (!Resolve(uid, ref rpic))
            return;
        var taxBracket = GetTaxBracketData(uid);
        var multiplier = taxBracket.ActionMultipliers.GetValueOrDefault(args.Category, 1f);
        basePay = (int)(basePay * multiplier);
    }

    private float GetMiscActionPayMult(
        EntityUid uid,
        RoleplayIncentiveComponent rpic,
        TaxBracketResult taxBracket)
    {
        // go through all the actions, and compile the BEST ONES EVER
        Dictionary<RpiActionType, RpiActionRecord> bestActions = new();
        foreach (var action in rpic.MiscActionsTaken.Where(action => action.IsValid()))
        {
            if (!action.TryPop())
            {
                continue;
            }
            // slot it into the best action for that type
            if (bestActions.TryGetValue(action.Category, out var existing))
            {
                if (action.GetMultiplier() > existing.GetMultiplier())
                {
                    bestActions[action.Category] = action;
                }
            }
            else
            {
                bestActions[action.Category] = action;
            }
        }
        var multOut = 1f;
        // now, sum up the best actions
        foreach (var kvp in bestActions)
        {
            /// add mults ADDITIVFELY
            multOut += (kvp.Value.GetMultiplier() - 1f);
        }
        return multOut;
    }

    private int GetChatActionPay(
        EntityUid uid,
        RoleplayIncentiveComponent? rpic,
        TaxBracketResult taxBracket)
    {
        if (!Resolve(uid, ref rpic))
            return 0;
        var total = 0;
        // go through all the actions, and compile the BEST ONES EVER
        Dictionary<RpiChatActionCategory, float> bestActions = new();
        foreach (var action in rpic.ChatActionsTaken.Where(action => !action.ChatActionIsSpent))
        {
            var judgement = JudgeChatAction(action);
            // slot it into the best action for that type
            if (bestActions.TryGetValue(action.Action, out var existing))
            {
                if (judgement > existing)
                {
                    bestActions[action.Action] = judgement;
                }
            }
            else
            {
                bestActions[action.Action] = judgement;
            }
            action.ChatActionIsSpent = true; // mark it for deletion
        }
        // now, sum up the best actions
        foreach (var kvp in bestActions)
        {
            total += (int)MathF.Ceiling(kvp.Value);
        }
        total *= taxBracket.PayPerJudgement;
        return total;
    }


    private void PruneOldActions(RoleplayIncentiveComponent rpic)
    {
        rpic.ChatActionsTaken.RemoveAll(action => action.ChatActionIsSpent);
        rpic.MiscActionsTaken.RemoveAll(action => action.MiscActionIsSpent);
    }

    public TaxBracketResult GetTaxBracketData(EntityUid uid)
    {
        if (!TryComp<RoleplayIncentiveComponent>(uid, out var rpic))
            return new TaxBracketResult(); // default values
        if (!_bank.TryGetBalance(uid, out var hasThisMuchMoney))
            return new TaxBracketResult(); // no bank account, no pramgle
        return GetTaxBracketData(rpic, hasThisMuchMoney);
    }

    public TaxBracketResult GetTaxBracketData(
        RoleplayIncentiveComponent rpic,
        int hasThisMuchMoney)
    {
        var taxBracket = new TaxBracketResult(); // default values
        // go through the prototypes, and find the one that fits the player's money
        // if none fit, use the default
        if (!_prototype.TryIndex(TaxBracketDefault, out var defaultProto))
        {
            Log.Warning($"RpiTaxBracketPrototype {TaxBracketDefault} not found! ITS THE DEFAULT AOOOAOAOAOOA");
            return taxBracket;
        }
        RpiTaxBracketPrototype? proto = null;
        // go through the sorted list, and find the Lowest bracket that is higher than the player's money
        List<RpiTaxBracketPrototype> protosHigherThanPlayer = new();
        foreach (var protoId in RpiDatumPrototypes)
        {
            if (!_prototype.TryIndex(protoId, out var myProto))
            {
                Log.Warning($"RpiTaxBracketPrototype {protoId} not found!");
                continue;
            }
            if (hasThisMuchMoney < myProto.CashThreshold)
            {
                protosHigherThanPlayer.Add(myProto);
            }
        }
        // if we found any, use the lowest one
        if (protosHigherThanPlayer.Count > 0)
        {
            proto = protosHigherThanPlayer.OrderBy(p => p.CashThreshold).First();
        }
        // if we didnt find any, use the default
        Dictionary<RpiChatActionCategory, RpiChatActionPrototype> chatActionPrototypes = new();
        proto ??= defaultProto;
        taxBracket = new TaxBracketResult(
            proto.JudgementPointPayout,
            (int)(proto.DeathPenalty * hasThisMuchMoney),
            (int)(proto.DeepFriedPenalty * hasThisMuchMoney),
            proto.ActionMultipliers);

        // and now the overrides
        if (rpic.TaxBracketPayoutOverride != -1)
        {
            taxBracket.PayPerJudgement = rpic.TaxBracketPayoutOverride;
        }

        if (rpic.TaxBracketDeathPenaltyOverride != -1)
        {
            taxBracket.DeathPenalty = rpic.TaxBracketDeathPenaltyOverride;
        }

        if (rpic.TaxBracketDeepFryerPenaltyOverride != -1)
        {
            taxBracket.DeepFryPenalty = rpic.TaxBracketDeepFryerPenaltyOverride;
        }
        return taxBracket;
    }

    private PayoutDetails ProcessPaymentDetails(
        int basePay,
        float baseMult)
    {
        var finalPay = basePay;
        // then apply the multiplier
        finalPay = (int)(finalPay * baseMult);
        // clamp the pay amount to a minimum of 20 and a maximum of int.MaxValue
        finalPay = Math.Clamp(
            (int)(Math.Ceiling(finalPay / 10.0) * 10),
            20,
            int.MaxValue);

        // round the multiplier to 2 decimal places
        var multiplier = baseMult;
        var hasMultiplier = Math.Abs(multiplier - 1f) > 0.01f;
        return new PayoutDetails(
            basePay,
            finalPay,
            multiplier,
            baseMult,
            hasMultiplier);
    }

    private void ShowPopup(EntityUid uid, string message, PopupType popupType = PopupType.LargeCaution)
    {
        _popupSystem.PopupEntity(
            message,
            uid,
            uid,
            popupType);
    }

    private void ShowPopup(
        EntityUid uid,
        PayoutDetails payDetails,
        string locale = "coyote-rpi-payward-message",
        bool suppressChat = false)
    {
        if (payDetails.FinalPay == 0)
            return; // no pay, no popup
        var messageOverhead = Loc.GetString(
            locale,
            ("amount", payDetails.FinalPay));
        _popupSystem.PopupEntity(
            messageOverhead,
            uid,
            uid,
            PopupType.Small,
            suppressChat);
    }

    private void ShowChatMessage(EntityUid uid, PayoutDetails payDetails)
    {
        if (payDetails.FinalPay <= 0)
            return; // no pay, no popup
        var message = "Hi mom~";
        // convert the multiplier to a string with 2 decimal places, if present
        if (payDetails.HasMultiplier)
        {
            message = Loc.GetString(
                "coyote-rpi-payward-message-multiplier",
                ("amount", payDetails.FinalPay),
                ("basePay", payDetails.BasePay),
                ("multiplier", payDetails.Multiplier));
        }
        else
        {
            message = Loc.GetString(
                "coyote-rpi-payward-message",
                ("amount", payDetails.FinalPay));
        }

        // cum it to chat
        if (_playerManager.TryGetSessionByEntity(uid, out var session))
        {
            _chatManager.ChatMessageToOne(
                ChatChannel.Notifications,
                message,
                message,
                default,
                false,
                session.Channel);
        }
    }

    private void ShowChatMessageSimple(EntityUid uid, PayoutDetails payDetails, string locale)
    {
        if (payDetails.FinalPay <= 0)
            return; // no pay, no popup
        var message = Loc.GetString(
            locale,
            ("amount", payDetails.FinalPay));

        // cum it to chat
        if (_playerManager.TryGetSessionByEntity(uid, out var session))
        {
            _chatManager.ChatMessageToOne(
                ChatChannel.Notifications,
                message,
                message,
                default,
                false,
                session.Channel);
        }
    }

    private void ProcessRoleplayIncentiveEvent(EntityUid uid, RpiChatEvent args)
    {
        // first, check if the uid has the component
        if (!TryComp<RoleplayIncentiveComponent>(uid, out var incentive))
        {
            Log.Warning($"RoleplayIncentiveComponent not found on entity {uid}!");
            return;
        } // i guess?

        // then, check if the channel in the args can be translated to a RoleplayAct
        var actOut = ChatChannel2RpiChatAction(args.Channel);
        if (actOut == RpiChatActionCategory.None)
        {
            return; // lot of stuff happens and it dont
        }

        // if its EmotingOrQuickEmoting, we need to doffgerentiate thewween the tween the two
        if (actOut == RpiChatActionCategory.EmotingOrQuickEmoting)
        {
            actOut = DoffgerentiateEmotingAndQuickEmoting(
                args.Source,
                args.Message);
        }

        // make the thing
        var action = new RpiChatRecord(
            actOut,
            _timing.CurTime,
            args.Message,
            args.PeoplePresent);
        // add it to the actions taken
        incentive.ChatActionsTaken.Add(action);
        // and we're good
    }

    private bool GetChatActionLookup(
        RpiChatActionCategory action,
        [NotNullWhen(true)] out RpiChatActionPrototype? proot)
    {
        if (!ChatActionLookup.TryGetValue(action, out var myPrototype))
        {
            proot = null;
            return false;
        }
        if (!_prototype.TryIndex<RpiChatActionPrototype>(myPrototype, out var proto))
        {
            Log.Warning($"RpiChatActionPrototype {myPrototype} not found!");
            proot = null;
            return false;
        }
        proot = proto;
        return true;
    }

    private static RpiChatActionCategory ChatChannel2RpiChatAction(ChatChannel channel)
    {
        // this is a bit of a hack, but it works
        return channel switch
        {
            ChatChannel.Local => RpiChatActionCategory.Speaking,
            ChatChannel.Whisper => RpiChatActionCategory.Whispering,
            ChatChannel.Emotes => RpiChatActionCategory.EmotingOrQuickEmoting, // we dont know yet
            ChatChannel.Radio => RpiChatActionCategory.Radio,
            ChatChannel.Subtle => RpiChatActionCategory.Subtling,
            // the rest are not roleplay actions
            _ => RpiChatActionCategory.None,
        };
    }

    private RpiChatActionCategory DoffgerentiateEmotingAndQuickEmoting(
        EntityUid source,
        string message
    )
    {
        return _chatsys.TryEmoteChatInput(
            source,
            message,
            false)
            ? RpiChatActionCategory.QuickEmoting // if the message is a valid emote, then its a quick emote
            : RpiChatActionCategory.Emoting;

        // well i cant figure out how the system does it, so im just gonnasay if theres
        // no spaces, its a quick emote
        // return !message.Contains(' ')
        //     ? RpiChatActionCategory.QuickEmoting
        //     // otherwise, its a normal emote
        //     : RpiChatActionCategory.Emoting;
    }

        /// <summary>
    /// Passes judgement on the action
    /// Based on a set of criteria, it will return a judgement value
    /// It will be judged based on:
    /// - How long the text was
    /// - How many people were present
    /// - and thats it for now lol
    /// </summary>
    private int JudgeChatAction(RpiChatRecord chatRecord)
    {
        var lengthMult = GetMessageLengthMultiplier(chatRecord.Action, chatRecord.Message?.Length ?? 1);
        var listenerMult = GetListenerMultiplier(chatRecord.Action, chatRecord.PeoplePresent);
        // if the action is a quick emote, it gets no judgement
        var judgement = lengthMult + listenerMult + 1f;
        return (int)Math.Floor(judgement);
    }

    /// <summary>
    /// Gets the multiplier for the number of listeners present
    /// </summary>
    /// <param name="action">The action being performed</param>
    /// <param name="listeners">The number of listeners present</param>
    private int GetListenerMultiplier(RpiChatActionCategory action, int listeners)
    {
        // if there are no listeners, return 0
        if (listeners <= 0)
            return 1;
        var numListeners = listeners;
        if (!GetChatActionLookup(action, out var proto))
            return 1;
        if (!proto.MultiplyByPeoplePresent)
            return 1;
        // clamp the number of listeners to the max defined in the prototype
        numListeners = Math.Clamp(
            numListeners,
            0,
            proto.MaxPeoplePresent);
        return numListeners;
    }

    /// <summary>
    /// Gets the message length multiplier for the action
    /// </summary>
    /// <param name="action">The action being performed</param>
    /// <param name="messageLength">The length of the message</param>
    private int GetMessageLengthMultiplier(RpiChatActionCategory action, int messageLength)
    {
        // if the message length is 0, return 1
        if (messageLength <= 0)
            return 1;

        // get the prototype for the action
        if (!GetChatActionLookup(action, out var proto))
            return 1;

        if (proto.LengthPerPoint <= 0)
        {
            return 1; // thingy isnt using length based judgement, also dont divide by 0
        }

        var rawLengthMult = messageLength / (float)proto.LengthPerPoint;
        // floor it to the nearest whole number, with a minimum of 1 and max of who cares
        return Math.Clamp(
            (int)Math.Floor(rawLengthMult),
            1,
            100);
    }

    private int GetPeopleInRange(EntityUid origin, float range)
    {
        var recipients = 1; // you were there too
        var xforms = GetEntityQuery<TransformComponent>();

        var transformSource = xforms.GetComponent(origin);
        var sourceMapId = transformSource.MapID;
        var sourceCoords = transformSource.Coordinates;

        foreach (var player in _playerManager.Sessions)
        {
            if (player.AttachedEntity is not { Valid: true } playerEntity)
                continue;

            if (playerEntity == origin)
                continue;

            var transformEntity = xforms.GetComponent(playerEntity);

            if (transformEntity.MapID != sourceMapId)
                continue;

            // even if they are a ghost hearer, in some situations we still need the range
            if (sourceCoords.TryDistance(
                    EntityManager,
                    transformEntity.Coordinates,
                    out var distance)
                && distance <= range)
            {
                recipients++;
            }
        }
        return recipients;
    }

    #endregion

    #region Data Holbies
    public sealed class TaxBracketResult(
        int payPerJudgement,
        int deathPenalty,
        int deepFryPenalty,
        Dictionary<RpiActionType, float> actionMultipliers)
    {
        public int PayPerJudgement = payPerJudgement;
        public int DeathPenalty = deathPenalty;
        public int DeepFryPenalty = deepFryPenalty;
        public Dictionary<RpiActionType, float> ActionMultipliers = actionMultipliers;

        public TaxBracketResult() : this(
            payPerJudgement:   10,
            deathPenalty:      0,
            deepFryPenalty:    0,
            actionMultipliers: new Dictionary<RpiActionType, float>())
        {
        }
    }

    private struct PayoutDetails(
        int basePay,
        int finalPay,
        FixedPoint2 multiplier,
        FixedPoint2 rawMultiplier,
        bool hasMultiplier)
    {
        public int BasePay = basePay;
        public int FinalPay = finalPay;
        public FixedPoint2 Multiplier = multiplier;
        public FixedPoint2 RawMultiplier = rawMultiplier;
        public bool HasMultiplier = hasMultiplier;
    }
    #endregion
}
