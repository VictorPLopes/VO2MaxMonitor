﻿using System;
using ReactiveUI;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel wrapper for <see cref="Measurement" /> model with presentation logic.
/// </summary>
/// <param name="model">The measurement model to wrap.</param>
public class MeasurementViewModel(Measurement model) : ViewModelBase
{
    private bool _isSelected;

    /// <summary>
    ///     Gets the underlying <see cref="Measurement" /> model.
    /// </summary>
    public Measurement Model { get; } = model ?? throw new ArgumentNullException(nameof(model));

    /// <summary>
    ///     Gets the formatted measurement date.
    /// </summary>
    public string FormattedDate => Model.ExerciseDate.ToString("g");

    /// <summary>
    ///     Gets the formatted V̇O₂ max value.
    /// </summary>
    public string[] FormattedVO2Max => [$"{Model.VO2Max:F2}", "mL/min/kg"];

    /// <summary>
    ///     Gets the exercise type.
    /// </summary>
    public string ExerciseType => Model.ExerciseType ?? string.Empty;

    /// <summary>
    ///     Gets the formatted weight.
    /// </summary>
    public string FormattedWeightKg => $"{Model.WeightKg} kg";

    /// <summary>
    ///     Gets or sets whether this measurement is currently selected.
    /// </summary>
    public bool IsSelected
    {
        set => this.RaiseAndSetIfChanged(ref _isSelected, value);
    }
}