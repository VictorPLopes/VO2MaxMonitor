using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using VO2MaxMonitor.Services;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for the main application window, managing the overall application state.
/// </summary>
public class MainWindowViewModel : ViewModelBase
{
    private readonly IServiceProvider      _services;
    private          ViewModelBase         _currentView;
    private          MeasurementViewModel? _selectedMeasurement;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
    /// </summary>
    /// <param name="services">The service provider for dependency injection.</param>
    public MainWindowViewModel(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        // Initialize commands
        AddMeasurementCommand = ReactiveCommand.Create(() =>
        {
            CurrentView = new NewMeasurementViewModel(
                                                      this,
                                                      _services.GetRequiredService<IVO2MaxCalculator>()
                                                     );
        });

        AddMeasurementCommand = ReactiveCommand.Create(ShowNewMeasurementView);
        CurrentView           = new WelcomeViewModel();
    }

    /// <summary>
    ///     Gets the collection of measurements.
    /// </summary>
    public ObservableCollection<MeasurementViewModel> Measurements { get; } = [];

    /// <summary>
    ///     Gets or sets the currently displayed view.
    /// </summary>
    public ViewModelBase CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    /// <summary>
    ///     Gets or sets the currently selected measurement.
    /// </summary>
    public MeasurementViewModel? SelectedMeasurement
    {
        get => _selectedMeasurement;
        set
        {
            if (_selectedMeasurement != null)
                _selectedMeasurement.IsSelected = false;

            this.RaiseAndSetIfChanged(ref _selectedMeasurement, value);

            if (value != null)
            {
                value.IsSelected = true;
                CurrentView      = new MeasurementDetailViewModel(value, this);
            }
            else
            {
                CurrentView = new WelcomeViewModel();
            }
        }
    }

    /// <summary>
    ///     Gets the command for adding a new measurement.
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddMeasurementCommand { get; }


    private void ShowNewMeasurementView() =>
        CurrentView = new NewMeasurementViewModel(this, new VO2MaxCalculator(1.225, 0.852, 20.93, 30000));
}