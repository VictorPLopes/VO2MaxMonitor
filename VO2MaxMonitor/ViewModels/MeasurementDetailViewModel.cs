using System;
using System.Reactive;
using ReactiveUI;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for displaying detailed information about a measurement.
/// </summary>
public class MeasurementDetailViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MeasurementDetailViewModel" /> class.
    /// </summary>
    /// <param name="measurementVm">The measurement to display.</param>
    /// <param name="mainVm">The main window ViewModel.</param>
    public MeasurementDetailViewModel(MeasurementViewModel measurementVm, MainWindowViewModel mainVm)
    {
        MeasurementVm = measurementVm ?? throw new ArgumentNullException(nameof(measurementVm));
        _mainVm       = mainVm ?? throw new ArgumentNullException(nameof(mainVm));

        DeleteCommand = ReactiveCommand.Create(DeleteMeasurement);
    }

    /// <summary>
    ///     Gets the measurement being displayed.
    /// </summary>
    public MeasurementViewModel MeasurementVm { get; }

    /// <summary>
    ///     Gets the command for deleting this measurement.
    /// </summary>
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }

    private void DeleteMeasurement()
    {
        _mainVm.SelectedProfile?.RemoveMeasurement(MeasurementVm);
        _mainVm.SelectedMeasurement = null;
    }
}