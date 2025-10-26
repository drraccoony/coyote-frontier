namespace Content.Shared.Power.Generator;

/// <summary>
/// This handles small, portable generators that run off a material fuel.
/// </summary>
/// <seealso cref="FuelGeneratorComponent"/>
public abstract class SharedGeneratorSystem : EntitySystem
{
    /// <summary>
    /// Calculates the fuel->joule efficiency based on the target power level.
    /// Expressed as a linear curve clamped at 0.2x and 2.0x efficiency.
    /// </summary>
    /// <param name="targetPower">Target power level</param>
    /// <param name="optimalPower">Optimal power level</param>
    /// <param name="component"></param>
    /// <returns>Expected fuel efficiency as a percentage</returns>
    public static float CalcFuelEfficiency(float targetPower, float optimalPower, FuelGeneratorComponent component)
    {
        return Math.Clamp(((optimalPower - targetPower) / (10.0f * component.ClockstepConstant)) + 1.0f,0.2f,2.0f);
    }
}
