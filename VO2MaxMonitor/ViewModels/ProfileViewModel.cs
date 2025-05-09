using System;
using System.Collections.ObjectModel;
using System.Linq;
using ReactiveUI;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel wrapper for <see cref="Profile" /> model with presentation logic.
/// </summary>
/// <param name="model">The profile model to wrap.</param>
public class ProfileViewModel(Profile model) : ViewModelBase
{
    private bool _isSelected;
    
    private Profile Model { get; } = model ?? throw new ArgumentNullException(nameof(model));
    
    /// <summary>
    ///     Gets the profile's name.
    /// </summary>
    public string Name => Model.Name;
    
    /// <summary>
    ///     Gets the profile's weight in kilograms.
    /// </summary>
    public double WeightKg => Model.WeightKg;
    
    /// <summary>
    ///     Gets the collection of measurements associated with this profile.
    /// </summary>
    public ObservableCollection<MeasurementViewModel> Measurements => new(model.Measurements.Select(m => new MeasurementViewModel(m)));
    
    /// <summary>
    ///     Gets or sets whether this measurement is currently selected.
    /// </summary>
    public bool IsSelected
    {
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }
    
    /// <summary>
    ///    Adds a new measurement to the profile.
    /// </summary>
    public void AddMeasurement(Measurement measurement)
    {
        ArgumentNullException.ThrowIfNull(measurement);
        Model.Measurements.Add(measurement);
        Measurements.Add(new MeasurementViewModel(measurement));
    }
    
    /// <summary>
    ///     Removes a measurement from the profile.
    /// </summary>
    public void RemoveMeasurement(MeasurementViewModel measurementVm)
    {
        ArgumentNullException.ThrowIfNull(measurementVm);
        Model.Measurements.Remove(measurementVm.Model);
        Measurements.Remove(measurementVm);
    }
}