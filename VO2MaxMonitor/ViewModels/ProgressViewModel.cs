using System.Linq;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace VO2MaxMonitor.ViewModels;

// TODO: Add documentation
public class ProgressViewModel(ProfileViewModel profile) : ViewModelBase
{
    public ISeries[] Series { get; set; } = [
                                                new LineSeries<double>()
                                                {
                                                    Values = profile.Model.Measurements.Select(m => m.VO2Max).ToArray(),
                                                    Name = "V̇O₂ max",
                                                    ScalesYAt = 0,
                                                    Stroke = new SolidColorPaint(SKColors.Cyan, 2),
                                                    GeometrySize = 6
                                                },
                                                new LineSeries<double>
                                                {
                                                    Values       = profile.Model.Measurements.Select(m => m.WeightKg).ToArray(),
                                                    Name         = "Weight (kg)",
                                                    ScalesYAt    = 1,
                                                    Stroke       = new SolidColorPaint(SKColors.Magenta, 2),
                                                    GeometrySize = 6
                                                }
                                            ];

    public Axis[] XAxes { get; set; } =
        [
            new()
            {
                Labels         = profile.Model.Measurements.Select(m => m.ExerciseDate.ToString("g")).ToArray(),
                LabelsRotation = 15,
                Name           = "Date",
                TextSize       = 12
            }
        ];

    public Axis[] YAxes { get; set; } =
        [
            new() { Name = "V̇O₂ max", TextSize    = 12 },
            new() { Name = "Weight (kg)", TextSize = 12, Position = LiveChartsCore.Measure.AxisPosition.End }
        ];
}