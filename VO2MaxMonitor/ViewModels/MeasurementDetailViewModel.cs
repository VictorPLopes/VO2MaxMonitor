namespace VO2MaxMonitor.ViewModels;

public class MeasurementDetailViewModel(MeasurementViewModel measurementVm) : ViewModelBase
{
    public MeasurementViewModel MeasurementVm { get; } = measurementVm;
}