using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using VO2MaxMonitor.Services;

namespace VO2MaxMonitor.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    // Private fields
    private ViewModelBase _currentView; // View shown in the content area of the main window
    private MeasurementViewModel? _selectedMeasurement; // Current selected measurement
    private readonly IServiceProvider _services; // Dependency injection service provider

    // Constructor
    public MainWindowViewModel(IServiceProvider services)
    {
        _services = services;
        
        // Initialize with DI
        AddMeasurementCommand = ReactiveCommand.Create(() => 
        {
            CurrentView = new NewMeasurementViewModel(this, _services.GetRequiredService<IVO2MaxCalculator>());
        });
        
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
    private void ShowNewMeasurementView() => CurrentView = new NewMeasurementViewModel(this, new VO2MaxCalculator(1.225, 0.852, 20.93, 30000));
}