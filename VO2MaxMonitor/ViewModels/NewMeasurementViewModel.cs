using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using VO2MaxMonitor.Models;
using VO2MaxMonitor.Services;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for creating new VO2Max measurements.
/// </summary>
public class NewMeasurementViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;
    private readonly IVO2MaxCalculator   _vo2Calculator;
    private          string              _exerciseType = string.Empty;
    private          string              _filePath     = string.Empty;
    private          string              _weightKg;


    /// <summary>
    ///     Initializes a new instance of the <see cref="NewMeasurementViewModel" /> class.
    /// </summary>
    public NewMeasurementViewModel(
        MainWindowViewModel mainVm,
        IVO2MaxCalculator   vo2Calculator)
    {
        Title = "New Measurement";

        _mainVm        = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        _weightKg      = (mainVm.SelectedProfile?.WeightKg ?? 0).ToString(CultureInfo.InvariantCulture);
        _vo2Calculator = vo2Calculator ?? throw new ArgumentNullException(nameof(vo2Calculator));

        var canCompute = this.WhenAnyValue(
                                           x => x.WeightKg,
                                           x => x.FilePath,
                                           x => x.ExerciseType,
                                           (weight, filePath, exerciseType) =>
                                               !string.IsNullOrWhiteSpace(filePath) &&
                                               !string.IsNullOrWhiteSpace(exerciseType) &&
                                               IsValidWeight(weight));

        SelectCsvCommand = ReactiveCommand.CreateFromTask(SelectCsvFileAsync);
        ComputeCommand   = ReactiveCommand.Create(ComputeVO2Max, canCompute);
        CancelCommand    = ReactiveCommand.Create(Cancel);
    }

    /// <summary>
    ///     Gets or sets the exercise type.
    /// </summary>
    public string ExerciseType
    {
        get => _exerciseType;
        set => this.RaiseAndSetIfChanged(ref _exerciseType, value);
    }

    /// <summary>
    ///     Gets or sets the subject's weight in kilograms.
    /// </summary>
    public string WeightKg
    {
        get => _weightKg;
        set => this.RaiseAndSetIfChanged(ref _weightKg, value);
    }

    /// <summary>
    ///     Gets or sets the path to the CSV file containing sensor data.
    /// </summary>
    public string FilePath
    {
        get => _filePath;
        private set => this.RaiseAndSetIfChanged(ref _filePath, value);
    }

    /// <summary>
    ///     Gets the command for selecting a CSV file.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SelectCsvCommand { get; }

    /// <summary>
    ///     Gets the command for computing VO2Max from the selected file.
    /// </summary>
    public ReactiveCommand<Unit, Unit> ComputeCommand { get; }

    /// <summary>
    ///     Command to cancel the action.
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    private async Task SelectCsvFileAsync()
    {
        try
        {
            var filesService = App.Current?.Services?.GetRequiredService<IFilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.OpenFileAsync();
            if (file is null) return;

            // Get the file path
            FilePath = file.Path.AbsolutePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error selecting file: {ex.Message}");
        }
    }

    // ReSharper disable once InconsistentNaming
    private void ComputeVO2Max()
    {
        var parsedWeight = double.Parse(WeightKg, CultureInfo.InvariantCulture);

        // Update the weight in the selected profile if it exists
        if (_mainVm.SelectedProfile is not null)
            _mainVm.SelectedProfile.WeightKg = parsedWeight;

        try
        {
            List<Reading> readings;
            using (var reader = new StreamReader(FilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                    readings = csv.GetRecords<Reading>().ToList();

            var vo2Max        = _vo2Calculator.Calculate(readings, parsedWeight);
            var measurement   = new Measurement(vo2Max, parsedWeight, _exerciseType);
            var measurementVm = new MeasurementViewModel(measurement);

            _mainVm.SelectedProfile?.AddMeasurement(measurementVm);
            _mainVm.SelectedMeasurement = measurementVm;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error computing VO2Max: {ex.Message}");
        }
    }

    private void Cancel() => _mainVm.CurrentView = new WelcomeViewModel();

    // Validate the weight input
    private static bool IsValidWeight(string weight) =>
        double.TryParse(weight, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) &&
        result is > 0 and < 300;
}