using System;

namespace VO2MaxMonitor.Models;

/// <summary>
///     Represents a measurement of V̇O₂ max.
/// </summary>
/// <param name="vO2Max">The maximum V̇O₂ value computed over a time interval (mL/min/kg).</param>
/// <param name="weightKg">Body mass of the subject in kilograms.</param>
/// <param name="exerciseType">A user-defined tag to identify what kind of exercise performed when measuring V̇O₂ max.</param>
public class Measurement(double vO2Max, double weightKg, string exerciseType)
{
    /// <summary>
    ///     Gets or sets the date and time when the V̇O₂ value was computed.
    /// </summary>
    public DateTime ExerciseDate { get; set; } = DateTime.Now;

    /// <summary>
    ///     Gets the maximum V̇O₂ value (mL/min/kg) computed over the measurement interval.
    /// </summary>
    public double VO2Max { get; } = vO2Max;

    /// <summary>
    ///     Gets the body mass of the subject in kilograms.
    /// </summary>
    public double WeightKg { get; } = weightKg;

    /// <summary>
    ///     Gets a user-defined tag used to identify the type of exercise performed when measuring V̇O₂ max.
    /// </summary>
    public string? ExerciseType { get; } = exerciseType;
}