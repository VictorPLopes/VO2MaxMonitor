using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.ViewModels;

public class NewMeasurementViewModel : ViewModelBase
{
    // Main window view model reference
    private readonly MainWindowViewModel _mainVm;

    // Form properties
    private string _exerciseType = "Treadmill"; // Default value
    private string _filePath     = string.Empty;
    private double _weightKg;

    public NewMeasurementViewModel(MainWindowViewModel mainVm)
    {
        _mainVm = mainVm;
        //_weightKg = mainVm.CurrentProfile?.WeightKg ?? 70.0; // TODO: Default to profile weight or 70kg
        _weightKg = 70.0; // Default weight for new measurement

        // Initialize validation
        var canCompute = this.WhenAnyValue(
                                           x => x.WeightKg, x => x.FilePath, x => x.ExerciseType,
                                           (weight, filePath, exerciseType) =>
                                               !string.IsNullOrWhiteSpace(filePath)     &&
                                               !string.IsNullOrWhiteSpace(exerciseType) &&
                                               weight > 0);

        // Initialize commands
        SelectCsvCommand = ReactiveCommand.CreateFromTask(SelectCsvFileAsync);
        ComputeCommand   = ReactiveCommand.Create(ComputeVo2Max, canCompute);
    }

    // Form bindable properties
    public string ExerciseType
    {
        get => _exerciseType;
        set => this.RaiseAndSetIfChanged(ref _exerciseType, value);
    }

    public double WeightKg
    {
        get => _weightKg;
        set => this.RaiseAndSetIfChanged(ref _weightKg, value);
        // TODO: Update profile weight if changed
        // if (_mainVm.CurrentProfile != null)
        //    _mainVm.CurrentProfile.WeightKg = value;
    }

    public string FilePath
    {
        get => _filePath;
        private set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }
    
    // Commands
    public ReactiveCommand<Unit, Unit> SelectCsvCommand { get; }
    public ReactiveCommand<Unit, Unit> ComputeCommand   { get; }

    // Command methods
    private async Task SelectCsvFileAsync() =>
        // TODO: Implement file selection dialog
        FilePath = "PLACEHOLDER_CSV_PATH"; // Replace with actual file path

    //
    private void ComputeVo2Max()
    {
        // TODO: Implement VO2Max computation logic
        // For now, just create a new measurement and add it to the main view model
        var rand = new Random(); // Placeholder for actual VO2Max computation
        var measurement =
            new Measurement(20 + rand.NextDouble() * 40.0, _weightKg, _exerciseType); // Placeholder VO2Max value
        
        // Add the new measurement to the main view model
        var measurementVm = new MeasurementViewModel(measurement);

        _mainVm.Measurements.Add(measurementVm);
        _mainVm.SelectedMeasurement = measurementVm;
    }
}