namespace Content.Shared._Coyote.Needs;

/// <summary>
/// The threshold of a need, all need to be filled out
/// </summary>
public enum NeedThreshold : byte
{
    /// <summary>
    /// The need is in the best possible state
    /// </summary>
    ExtraSatisfied = 0,

    /// <summary>
    /// The need is satisfied
    /// </summary>
    Satisfied = 1,

    /// <summary>
    /// The need is low
    /// </summary>
    Low = 2,

    /// <summary>
    /// The need is critical, threshold will be set to the minimum value automagestically
    /// </summary>
    Critical = 3,
}
