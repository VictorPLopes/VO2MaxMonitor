using System.Reactive;
using ReactiveUI;

namespace VO2MaxMonitor.ViewModels;

public class MeasurementDetailViewModel : ViewModelBase
{
    // Private fields
    private readonly MainWindowViewModel _mainVm;

    // Constructor
    public MeasurementDetailViewModel(MeasurementViewModel measurementVm, MainWindowViewModel mainVm)
    {
        MeasurementVm = measurementVm;
        _mainVm       = mainVm;

        // Initialize commands
        DeleteCommand = ReactiveCommand.Create(DeleteMeasurement);
    }

    // Public properties
    public MeasurementViewModel MeasurementVm { get; }

    // Commands
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    // Command methods
    private void DeleteMeasurement()
    {
        // Remove the measurement from the main view model
        _mainVm.Measurements.Remove(MeasurementVm);

        // Clear the selected measurement
        _mainVm.SelectedMeasurement = null;
    }
}