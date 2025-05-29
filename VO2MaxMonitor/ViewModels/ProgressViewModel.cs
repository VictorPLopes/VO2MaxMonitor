using System.Collections.Generic;
using System.Linq;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for displaying the progress of a user's V̇O₂ max and weight measurements through a line chart.
/// </summary>
/// <param name="profile">The <see cref="ProfileViewModel" /> from which the measurements will be extracted.</param>
public class ProgressViewModel(ProfileViewModel profile) : ViewModelBase
{
    /// <summary>
    ///     Gets or sets the series to be displayed in the chart.
    /// </summary>
    public ISeries[] Series { get; set; } =
        [
            new LineSeries<double>
            {
                Values       = profile.Model.Measurements
                                      .GroupBy(m => m.ExerciseDate.Date)
                                      .Select(g => g.OrderByDescending(m => m.VO2Max).FirstOrDefault())
                                      .OrderBy(m => m.ExerciseDate)
                                      .Take(30)
                                      .ToList()
                                      .Select(m => m.VO2Max)
                                      .ToArray(), // Selects the maximum V̇O₂ for each day, for a maximum of 30 days
                Name         = "V̇O₂ max (mL/min/kg)",
                ScalesYAt    = 0,
                GeometrySize = 6
            },
            new LineSeries<double>
            {
                Values = profile.Model.Measurements
                                .GroupBy(m => m.ExerciseDate.Date)
                                .Select(g => g.OrderByDescending(m => m.VO2Max).FirstOrDefault())
                                .OrderBy(m => m.ExerciseDate)
                                .Take(30)
                                .ToList()
                                .Select(m => m.WeightKg)
                                .ToArray(), // Selects the weight for each day, for a maximum of 30 days
                Name         = "Weight (kg)",
                ScalesYAt    = 1,
                GeometrySize = 6
            }
        ];

    /// <summary>
    ///     Gets or sets the X axes for the chart, which represent the dates of the measurements.
    /// </summary>
    public Axis[] XAxes { get; set; } =
        [
            new()
            {
                Labels         = profile.Model.Measurements
                                        .GroupBy(m => m.ExerciseDate.Date)
                                        .Select(g => g.OrderByDescending(m => m.VO2Max).FirstOrDefault())
                                        .OrderBy(m => m.ExerciseDate)
                                        .Take(30)
                                        .ToList()
                                        .Select(m => m.ExerciseDate.ToString("g"))
                                        .ToArray(), // Selects the date for each day, for a maximum of 30 days
                LabelsRotation = 15,
                Name           = "Date",
                TextSize       = 12
            }
        ];

    /// <summary>
    ///     Gets or sets the Y axes for the chart, which represent the V̇O₂ max and weight measurements.
    /// </summary>
    public Axis[] YAxes { get; set; } =
        [
            new() { Name = "V̇O₂ max (mL/min/kg)", TextSize    = 12 },
            new() { Name = "Weight (kg)", TextSize = 12, Position = AxisPosition.End }
        ];
}