using System;
using System.Globalization;
using System.Reactive;
using ReactiveUI;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for the Profile dialog.
/// </summary>
public class EditProfileViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainVm;
    private          string              _name;
    private          ProfileViewModel?   _profile;
    private          string              _weightKg;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EditProfileViewModel" /> class.
    /// </summary>
    /// <param name="profile">The profile to edit, or null for a new profile.</param>
    public EditProfileViewModel(MainWindowViewModel mainVm, ProfileViewModel? profile = null)
    {
        if (profile is not null && !string.IsNullOrWhiteSpace(profile.Name))
            Title = "Editing \"" + profile.Name + "\"";
        else
            Title = "New Profile";

        // Initialize properties from the profile or with defaults
        _name     = profile?.Name ?? string.Empty;
        _weightKg = (profile?.WeightKg ?? 70.0).ToString(CultureInfo.InvariantCulture);
        _mainVm   = mainVm ?? throw new ArgumentNullException(nameof(mainVm));
        _profile  = profile;

        var canSave = this.WhenAnyValue(
                                        x => x.Name,
                                        x => x.WeightKg,
                                        (name, weight) => !string.IsNullOrWhiteSpace(name) && IsValidWeight(weight));

        SaveCommand = ReactiveCommand.Create(SaveProfile, canSave);

        CancelCommand = ReactiveCommand.Create(Cancel);
    }

    /// <summary>
    ///     Gets or sets the name of the profile.
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    ///     Gets or sets the body mass of the subject in kilograms.
    /// </summary>
    public string WeightKg
    {
        get => _weightKg;
        set => this.RaiseAndSetIfChanged(ref _weightKg, value);
    }

    /// <summary>
    ///     Command to save the profile.
    /// </summary>
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }

    /// <summary>
    ///     Command to cancel the dialog.
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    private void SaveProfile()
    {
        var parsedWeight = double.Parse(WeightKg, CultureInfo.InvariantCulture);

        // If profile is null, create a new one
        if (_profile is null)
        {
            _profile = new ProfileViewModel(new Profile(Name, parsedWeight));
            _mainVm.Profiles.Add(_profile);
            // Set the newly created profile as the selected one
            _mainVm.SelectedProfile = _profile;
        }
        else
        {
            // Update existing profile
            _profile.Name     = Name;
            _profile.WeightKg = parsedWeight;

            // Force UI update
            _mainVm.RaisePropertyChanged(nameof(_mainVm.SelectedProfile));
        }

        // Navigate to the welcome view
        _mainVm.CurrentView = new WelcomeViewModel();
    }

    private void Cancel() => _mainVm.CurrentView = new WelcomeViewModel();

    // Validate the weight input
    private static bool IsValidWeight(string weight) =>
        double.TryParse(weight, NumberStyles.Float, CultureInfo.InvariantCulture, out var result) &&
        result is > 0 and < 300;
}