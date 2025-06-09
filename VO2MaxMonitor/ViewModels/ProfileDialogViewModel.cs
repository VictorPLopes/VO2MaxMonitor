using System.Reactive;
using ReactiveUI;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for the Profile dialog.
/// </summary>
public class ProfileDialogViewModel : ViewModelBase
{
    private string _name;
    private double _weightKg;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ProfileDialogViewModel" /> class.
    /// </summary>
    /// <param name="profile">The profile to edit, or null for a new profile.</param>
    public ProfileDialogViewModel(ProfileViewModel? profile = null)
    {
        // Initialize properties from the profile or with defaults
        _name     = profile?.Name ?? string.Empty;
        _weightKg = profile?.WeightKg ?? 70.0;

        SaveCommand = ReactiveCommand.Create(() => new ProfileViewModel(new Profile(Name, WeightKg)));

        CancelCommand = ReactiveCommand.Create(() => { });
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
    public double WeightKg
    {
        get => _weightKg;
        set => this.RaiseAndSetIfChanged(ref _weightKg, value);
    }

    /// <summary>
    ///     Command to save the profile.
    /// </summary>
    public ReactiveCommand<Unit, ProfileViewModel> SaveCommand { get; }

    /// <summary>
    ///     Command to cancel the dialog.
    /// </summary>
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }
}