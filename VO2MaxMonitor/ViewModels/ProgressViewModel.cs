using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for displaying the progress of a user's V̇O₂ max and weight measurements through a line chart.
/// </summary>
public class ProgressViewModel : ViewModelBase
{
    private static readonly SKColor PrimaryTextPaint   = GetThemedSkColor("TextFillColorPrimaryBrush");
    private static readonly SKColor SecondaryTextPaint = GetThemedSkColor("TextFillColorSecondaryBrush");

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProgressViewModel" /> class.
    /// </summary>
    /// <param name="profile">The <see cref="ProfileViewModel" /> from which the measurements will be extracted.</param>
    public ProgressViewModel(ProfileViewModel profile)
    {
        Title = "Progress for " + profile.Model.Name;

        Series =
        [
            new LineSeries<double>
            {
                Values = profile.Model.Measurements
                                .GroupBy(m => m.ExerciseDate.Date)
                                .Select(g => g.OrderByDescending(m => m.VO2Max).FirstOrDefault())
                                .OrderBy(m => m?.ExerciseDate)
                                .Take(30)
                                .ToList()
                                .Select(m => m!.VO2Max)
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
                                .OrderBy(m => m?.ExerciseDate)
                                .Take(30)
                                .ToList()
                                .Select(m => m!.WeightKg)
                                .ToArray(), // Selects the weight for each day, for a maximum of 30 days
                Name         = "Weight (kg)",
                ScalesYAt    = 1,
                GeometrySize = 6
            }
        ];
        XAxes =
        [
            new Axis
            {
                Labels = profile.Model.Measurements
                                .GroupBy(m => m.ExerciseDate.Date)
                                .Select(g => g.OrderByDescending(m => m.VO2Max).FirstOrDefault())
                                .OrderBy(m => m?.ExerciseDate)
                                .Take(30)
                                .ToList()
                                .Select(m => m!.ExerciseDate.ToString("g"))
                                .ToArray(), // Selects the date for each day, for a maximum of 30 days
                LabelsRotation = 15,
                Name           = "Date",
                TextSize       = 12,
                LabelsPaint    = new SolidColorPaint(PrimaryTextPaint),
                NamePaint      = new SolidColorPaint(PrimaryTextPaint)
            }
        ];
    }

    /// <summary>
    ///     Gets or sets the series to be displayed in the chart.
    /// </summary>
    public ISeries[] Series { get; set; }

    /// <summary>
    ///     Gets or sets the X axes for the chart, which represent the dates of the measurements.
    /// </summary>
    public Axis[] XAxes { get; set; }

    /// <summary>
    ///     Gets or sets the Y axes for the chart, which represent the V̇O₂ max and weight measurements.
    /// </summary>
    public Axis[] YAxes { get; set; } =
    [
        new()
        {
            Name        = "V̇O₂ max (mL/min/kg)",
            TextSize    = 12,
            Position    = AxisPosition.Start,
            LabelsPaint = new SolidColorPaint(PrimaryTextPaint),
            NamePaint   = new SolidColorPaint(PrimaryTextPaint)
        },
        new()
        {
            Name        = "Weight (kg)",
            TextSize    = 12,
            Position    = AxisPosition.End,
            LabelsPaint = new SolidColorPaint(PrimaryTextPaint),
            NamePaint   = new SolidColorPaint(PrimaryTextPaint)
        }
    ];

    // Helper function
    private static SKColor GetThemedSkColor(string brushKey)
    {
        var app = Application.Current;
        if (app is null)
            return SKColor.Empty;

        var theme = app.ActualThemeVariant;

        if (app?.TryFindResource(brushKey, theme, out var brushObj) != true ||
            brushObj is not ISolidColorBrush brush) return SKColor.Empty;
        var color = brush.Color;
        return new SKColor(color.R, color.G, color.B, color.A);
    }
}