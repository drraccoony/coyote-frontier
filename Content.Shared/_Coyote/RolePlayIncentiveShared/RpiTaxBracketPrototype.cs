using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Coyote.RolePlayIncentiveShared;

/// <summary>
/// This is a prototype for defining cool RPI stuff for like, tax brackets and such.
/// </summary>
[Prototype("rpiTaxBracket")]
public sealed partial class RpiTaxBracketPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    /// This bracket applies to you if you have less than this amount of $$$
    /// </summary>
    [DataField("cashThreshold", required: true)]
    public int CashThreshold = 0;

    /// <summary>
    /// The payout you get, per Judgement Point.
    /// </summary>
    [DataField("judgementPointPayout", required: true)]
    public int JudgementPointPayout = 0;

    /// <summary>
    /// How much you get penalized for dying. This is percent of your total cash!
    /// </summary>
    [DataField("deathPenalty", required: true)]
    public float DeathPenalty = 0f;

    /// <summary>
    /// How much you get penalized for being deep-fried. This is percent of your total cash!
    /// </summary>
    [DataField("deepFriedPenalty", required: true)]
    public float DeepFriedPenalty = 0f;

    /// <summary>
    /// Journalism
    /// </summary>
    [DataField("journalismDat", required: true)]
    public RpiJournalismData JournalismDat = new();

    /// <summary>
    /// Whats the multiplier for getting paid for mining?
    /// </summary>
    [DataField("actionMultipliers", required: true)]
    public Dictionary<RpiActionType, float> ActionMultipliers = new();
}

/// <summary>
/// Data specific to journalism paywards.
/// </summary>
[Serializable, DataDefinition]
public sealed partial class RpiJournalismData
{
    /// <summary>
    /// baseFlatPayout?
    /// </summary>
    [DataField("baseFlatPayout", required: true)]
    public int BaseFlatPayout = 0;

    /// <summary>
    /// For every this many characters in the article, you get a bonus payout.
    /// </summary>
    [DataField("charCountDivisor", required: true)]
    public int CharCountDivisor = 100;

    /// <summary>
    /// The bonus per divisor.
    /// </summary>
    [DataField("multPerDivisor", required: true)]
    public float MultPerDivisor = 1.0f;

    /// <summary>
    /// The maximum pay you can get from a single article.
    /// </summary>
    [DataField("maxPay", required: true)]
    public float MaxPay = 5.0f;

    /// <summary>
    /// So you dont just spam the damn articles, apply a cooldown penalty.
    /// This only applies if the cooldown is less than 1 half complete
    /// otherwise it scales up with the time remaining, using a complicated
    /// exponential bell curve derivation that I dont feel like explaining here.
    /// </summary>
    [DataField("cooldownPenalty", required: true)]
    public float CooldownPenalty = 0.90f;

    /// <summary>
    /// Cooldown time in minutes.
    /// </summary>
    [DataField("cooldownTimeMinutes", required: true)]
    public int CooldownTimeMinutes = 20;

    // now some helpers
    public RpiJournalismPayResult GetPaypig(bool isJournal, int charCount, TimeSpan lastArticleTime)
    {
        var pay = (double) BaseFlatPayout;
        var basePay = (int) pay;
        if (isJournal)
        {
            pay += ((float)charCount / CharCountDivisor) * MultPerDivisor;
            if (pay > MaxPay)
                pay = MaxPay;
        }
        var cooldownMult = CooldownMult(lastArticleTime);
        pay *= cooldownMult;
        var totalPay = (int) Math.Floor(pay);
        var minsTillCooled = GetTimeWhenFullyCooledDown(lastArticleTime).TotalMinutes switch
        {
            <= 0 => 0,
            var x => (int) Math.Ceiling(x),
        };
        return new RpiJournalismPayResult(
            isJournal,
            basePay,
            totalPay,
            cooldownMult,
            minsTillCooled);
    }

    private float CooldownMult(TimeSpan lastArticleTime)
    {
        double mulTout = 1.0f;
        var timeSys = IoCManager.Resolve<GameTiming>();
        var timeSinceLastArticle = timeSys.CurTime - lastArticleTime;
        var cooldownTime = TimeSpan.FromMinutes(CooldownTimeMinutes);
        if (timeSinceLastArticle < cooldownTime)
        {
            var fraction = timeSinceLastArticle.TotalSeconds / cooldownTime.TotalSeconds;
            if (fraction < 0.5f)
            {
                mulTout = CooldownPenalty;
            }
            else
                // more than half way done, apply a complicated exponential bell curve derivation
            {
                var adjFraction = (fraction - 0.5f) * 2;
                // setup the curve equation
                // y = -4(x-1)^2 + 1
                var curveValue = -4 * Math.Pow(adjFraction - 1, 2) + 1;
                mulTout = curveValue;
            }
        }
        return (float) mulTout;
    }

    private TimeSpan GetTimeWhenFullyCooledDown(TimeSpan lastArticleTime)
    {
        var timeSys = IoCManager.Resolve<GameTiming>();
        var timeSinceLastArticle = timeSys.CurTime - lastArticleTime;
        var cooldownTime = TimeSpan.FromMinutes(CooldownTimeMinutes);
        if (timeSinceLastArticle >= cooldownTime)
            return TimeSpan.Zero;
        return cooldownTime - timeSinceLastArticle;
    }

}

/// <summary>
/// The return thing for when you the journalism treutnrs
/// </summary>
public struct RpiJournalismPayResult(
    bool isJournalist,
    int basePay,
    int totalPay,
    float cooldownMultiplier,
    int minsTilCooled)
{
    public bool IsJournalist = isJournalist;
    public int BasePay = basePay;
    public int TotalPay = totalPay;
    public float CooldownMultiplier = cooldownMultiplier;
    public int MinsTillCooled = minsTilCooled;
}
