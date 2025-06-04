using System;
using System.Collections.Generic;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

/// <summary>
///     Interface for calculating maximum oxygen consumption (V̇O₂ max) based on sensor readings.
/// </summary>
/// <remarks>
///     Implementations of this interface typically use Bernoulli's equation for airflow calculations
///     and the Haldane transformation for oxygen consumption computations.
/// </remarks>
public interface IVO2MaxCalculator
{
    /// <summary>
    ///     Calculates the maximum oxygen consumption (V̇O₂ max) from a series of sensor readings.
    /// </summary>
    /// <param name="readings">Collection of sensor readings. Must contain at least one reading.</param>
    /// <param name="weightKg">Body mass of the subject in kilograms (must be positive).</param>
    /// <returns>The maximum V̇O₂ value computed over the dataset (mL/min/kg).</returns>
    /// <exception cref="ArgumentException">
    ///     Thrown when:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>readings is null or empty</description>
    ///         </item>
    ///         <item>
    ///             <description>weightKg is not positive</description>
    ///         </item>
    ///     </list>
    /// </exception>
    double Calculate(List<Reading> readings, double weightKg);
}