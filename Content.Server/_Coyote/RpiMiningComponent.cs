namespace Content.Server._Coyote;

/// <summary>
/// When this entity gets mined by a mining tool, it gives RPI to the miner.
/// BUT only uif they are a miner!
/// </summary>
[RegisterComponent]
public sealed partial class RpiMiningComponent : Component
{
    /// <summary>
    /// RPI multiplier.
    /// </summary>
    [DataField("mult")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float Multiplier = 1f;

    /// <summary>
    /// RPI additive.
    /// </summary>
    [DataField("add")]
    [ViewVariables(VVAccess.ReadWrite)]
    public int Additive = 0;

    /// <summary>
    /// payout the Add immediately on mine? instead of the next payward?
}
