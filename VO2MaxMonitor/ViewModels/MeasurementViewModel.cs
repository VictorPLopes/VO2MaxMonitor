using ReactiveUI;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.ViewModels;

public class MeasurementViewModel(Measurement model) : ViewModelBase
{
    // Private fields
    private bool _isSelected;

    // Model access
    public Measurement Model => model;

    // Expose model properties with formatting
    public string FormattedDate     => model.ExerciseDate.ToString("g"); // Short date and time
    public string FormattedVO2Max   => $"{model.VO2Max:F2} ml/min/kg";
    public string ExerciseType      => model.ExerciseType ?? string.Empty;
    public string FormattedWeightKg => $"{model.WeightKg} kg";

    // Selection state
    public bool IsSelected
    {
        get => _isSelected;
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }
}