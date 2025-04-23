using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace VO2MaxMonitor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // Private fields
    // View shown in the content area of the main window
    private ViewModelBase _currentView;

    // Current selected measurement
    private MeasurementViewModel? _selectedMeasurement;


    // Constructor
    public MainWindowViewModel()
    {
        AddMeasurementCommand = ReactiveCommand.Create(ShowNewMeasurementView);
        CurrentView           = new WelcomeViewModel(); // Placeholder for empty state
    }


    // List of saved measurements
    public ObservableCollection<MeasurementViewModel> Measurements { get; } = [];


    // Form bindable properties
    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    public MeasurementViewModel? SelectedMeasurement
    {
        get => _selectedMeasurement;
        set
        {
            // Clear the previous selection
            if (_selectedMeasurement != null)
                _selectedMeasurement.IsSelected = false;

            this.RaiseAndSetIfChanged(ref _selectedMeasurement, value);

            // Set the new selection
            if (value != null)
            {
                value.IsSelected = true;
                CurrentView      = new MeasurementDetailViewModel(value);
            }
            else
            {
                CurrentView = new WelcomeViewModel(); // Placeholder for empty state
            }
        }
    }


    // Commands
    public ReactiveCommand<Unit, Unit> AddMeasurementCommand { get; }


    // Command Methods
    private void ShowNewMeasurementView() => CurrentView = new NewMeasurementViewModel(this);
}