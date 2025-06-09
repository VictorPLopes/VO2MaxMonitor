using System;
using System.Collections.Generic;
using System.Linq;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

/// <summary>
///     Service for calculating maximum oxygen consumption (V̇O₂ max) based on sensor readings.
///     Implements the <see cref="IVO2MaxCalculator" /> interface.
/// </summary>
/// <remarks>
///     This class uses Bernoulli's equation for airflow calculations and the Haldane transformation
///     for oxygen consumption computations.
/// </remarks>
/// <param name="airDensity">Air density in kg/m³ (must be positive).</param>
/// <param name="airDryness">Correction factor for dry air.</param>
/// <param name="ambientO2">Percentage of oxygen in ambient air (typically ~20.95).</param>
/// <param name="vO2ComputationInterval">Time interval between V̇O₂ calculations in milliseconds.</param>
public class VO2MaxCalculator(double airDensity, double airDryness, double ambientO2, uint vO2ComputationInterval)
    : IVO2MaxCalculator
{
    /// <inheritdoc />
    public double Calculate(List<Reading> readings, double weightKg)
    {
        // Validate inputs
        if (readings == null || readings.Count == 0)
            throw new ArgumentException("No sensor readings provided");

        if (weightKg <= 0)
            throw new ArgumentException("Weight must be positive");

        // Start the timer
        var startTime = readings.First().TimeStamp;
        var timer = new Timer
        {
            VO2  = startTime,
            Flow = startTime
        };

        // Initialize variables.
        var totalVolume = 0.0; // Accumulated volume in liters
        var vO2Max      = 0.0; // Maximum recorded V̇O₂ (mL/min/kg)

        // Process each reading.
        foreach (var reading in readings)
        {
            // 1. Compute airflow if differential pressure indicates breathing
            if (reading.DifferentialPressure > 1.0)
            {
                var flowRate = ComputeAirflow(reading.DifferentialPressure,
                                              reading.VenturiAreaRegular,
                                              reading.VenturiAreaConstricted);
                totalVolume += flowRate * (reading.TimeStamp - timer.Flow) * 1000.0; // Integrate to get volume (m³)
            }

            timer.Flow = reading.TimeStamp;

            // 2. Periodic V̇O₂ max computation
            if (reading.TimeStamp - timer.VO2 <= vO2ComputationInterval) continue;
            vO2Max = Math.Max(ComputeVO2(totalVolume, reading.O2, weightKg),
                              vO2Max); // If there's an error, the method will return -1, which will always be smaller than the previous value (and will be ignored)

            // Reset for the next iteration
            totalVolume = 0.0;
            timer.VO2   = reading.TimeStamp;
        }

        return vO2Max;
    }

    /// <summary>
    ///     Computes the airflow rate using Bernoulli's equation for venturi tube flow measurement.
    /// </summary>
    /// <param name="differentialPressure">Pressure difference across venturi in Pascals (Pa).</param>
    /// <param name="venturiAreaRegular">Cross-sectional area of venturi tube (m²) before constriction.</param>
    /// <param name="venturiAreaConstricted">Cross-sectional area at venturi constriction (m²).</param>
    /// <returns>Airflow rate in cubic meters per second (m³/s).</returns>
    private double ComputeAirflow(double differentialPressure, double venturiAreaRegular, double venturiAreaConstricted)
    {
        var massFlow = Math.Sqrt(Math.Abs(differentialPressure) * (60000.0 / vO2ComputationInterval) * airDensity /
                                 (1 / Math.Pow(venturiAreaConstricted, 2) - 1 / Math.Pow(venturiAreaRegular, 2)));
        return massFlow / airDensity; // Convert mass flow to volume flow
    }

    /// <summary>
    ///     Computes the oxygen consumption (V̇O₂) using the Haldane transformation.
    /// </summary>
    /// <param name="volume">Total air volume in cubic meters (m³) for the computation interval.</param>
    /// <param name="o2">Measured oxygen concentration in percent.</param>
    /// <param name="weightKg">Body mass of the subject in kilograms.</param>
    /// <returns>
    ///     Oxygen consumption rate in milliliters per minute per kilogram (mL/min/kg).
    ///     Returns -1.0 if input values are implausible.
    /// </returns>
    private double ComputeVO2(double volume, double o2, double weightKg)
    {
        var co2 = ambientO2 - o2;   // Estimate CO₂ as the difference from ambient O₂
        var n2  = 100.0 - o2 - co2; // Remaining percentage assumed to be N₂

        // Volume corrected for a minute (mL/min)
        var minuteVolume = volume * (60000.0 / vO2ComputationInterval) * airDryness;

        // Haldane Transformation
        var o2Consumption = minuteVolume * (n2 / 100.0 * 0.265 - o2 / 100.0);

        // Conversion to mL/min/kg
        var vo2 = o2Consumption / weightKg;

        // Validate plausible physiological values
        if (minuteVolume < 0.1 || o2 > 21.0 || o2 < 10.0)
            return -1.0; // Error indicator (invalid measurement)

        return vo2;
    }

    /// <summary>
    ///     Helper structure for tracking timestamps of the last V̇O₂ and airflow updates.
    /// </summary>
    private struct Timer
    {
        /// <summary>
        ///     Timestamp of the last V̇O₂ computation in milliseconds.
        /// </summary>
        public ulong VO2;

        /// <summary>
        ///     Timestamp of the last airflow computation in milliseconds.
        /// </summary>
        public ulong Flow;
    }
}