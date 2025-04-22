using System;

namespace VO2MaxMonitor.Models;

public class Measurement(double vO2Max, double weightKg, string exerciseType)
{
    public DateTime ExerciseDate { get; set; } = DateTime.Now;
    public double   VO2Max       { get; set; } = vO2Max;
    public double   WeightKg     { get; set; } = weightKg;
    public string?  ExerciseType { get; set; } = exerciseType;
}