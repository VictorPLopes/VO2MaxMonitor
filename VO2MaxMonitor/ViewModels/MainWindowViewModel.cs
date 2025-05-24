using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Controls.Primitives;
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
    private          FlyoutBase?           _profileFlyout;
    private          MeasurementViewModel? _selectedMeasurement;
    private          ProfileViewModel?     _selectedProfile;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MainWindowViewModel" /> class.
    /// </summary>
    /// <param name="services">The service provider for dependency injection.</param>
    public MainWindowViewModel(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

        // Interaction with the profile dialog
        ShowDialog = new Interaction<ProfileDialogViewModel, ProfileViewModel?>();

        // Interaction with the delete confirmation dialog
        ShowConfirmDialog = new Interaction<ConfirmDialogViewModel, bool>();

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

        // Profile commands
        ShowProfileFlyoutCommand  = ReactiveCommand.Create(ShowProfileFlyout);
        SwitchProfileCommand      = ReactiveCommand.Create<ProfileViewModel>(SwitchProfile);
        EditCurrentProfileCommand = ReactiveCommand.Create(EditCurrentProfile);
        EditProfileCommand        = ReactiveCommand.CreateFromTask<ProfileViewModel>(EditProfile);
        DeleteProfileCommand      = ReactiveCommand.CreateFromTask<ProfileViewModel>(DeleteProfile);
        AddProfileCommand         = ReactiveCommand.Create(AddProfile);
    }

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

    /// <summary>
    ///     Command to show the profile selection flyout
    /// </summary>
    public ReactiveCommand<Unit, Unit> ShowProfileFlyoutCommand { get; }

    /// <summary>
    ///     Command to switch to a different profile
    /// </summary>
    public ReactiveCommand<ProfileViewModel, Unit> SwitchProfileCommand { get; }

    /// <summary>
    ///     Command to edit an existing profile
    /// </summary>
    public ReactiveCommand<ProfileViewModel, Unit> EditProfileCommand { get; }

    /// <summary>
    ///     Command to edit the current profile
    /// </summary>
    public ReactiveCommand<Unit, Unit> EditCurrentProfileCommand { get; }

    /// <summary>
    ///     Command to delete a profile
    /// </summary>
    public ReactiveCommand<ProfileViewModel, Unit> DeleteProfileCommand { get; }

    /// <summary>
    ///     Command to add a new profile
    /// </summary>
    public ReactiveCommand<Unit, Unit> AddProfileCommand { get; }

    /// <summary>
    ///     Interaction for showing the profile dialog
    /// </summary>
    public Interaction<ProfileDialogViewModel, ProfileViewModel?> ShowDialog { get; }

    /// <summary>
    ///     Interaction for confirmation dialogs
    /// </summary>
    public Interaction<ConfirmDialogViewModel, bool> ShowConfirmDialog { get; }

    /// <summary>
    ///     Gets the collection of user profiles
    /// </summary>
    public ObservableCollection<ProfileViewModel> Profiles { get; } = [];

    /// <summary>
    ///     Gets the profiles excluding the currently selected one
    /// </summary>
    public ObservableCollection<ProfileViewModel> OtherProfiles =>
        new(Profiles.Where(p => p != SelectedProfile));

    /// <summary>
    ///     Gets or sets the currently selected profile.
    /// </summary>
    public ProfileViewModel? SelectedProfile
    {
        get => _selectedProfile;
        set
        {
            if (_selectedProfile != null)
                _selectedProfile.IsSelected = false;

            this.RaiseAndSetIfChanged(ref _selectedProfile, value);

            SelectedMeasurement = null;

            if (value == null) return;
            value.IsSelected = true;
        }
    }

    /// <summary>
    ///     Set the flyout reference (call this from view)
    /// </summary>
    public void SetFlyout(FlyoutBase flyout) => _profileFlyout = flyout;

    private void ShowNewMeasurementView() =>
        CurrentView = new NewMeasurementViewModel(this, new VO2MaxCalculator(1.225, 0.852, 20.93, 30000));

    // This is handled automatically by the Flyout in XAML
    private static void ShowProfileFlyout()
    {
    }

    private void SwitchProfile(ProfileViewModel profile)
    {
        SelectedProfile = profile;
        CloseFlyout();
    }

    private async Task EditProfile(ProfileViewModel profile)
    {
        var dialog = new ProfileDialogViewModel
        {
            Name     = profile.Name,
            WeightKg = profile.WeightKg
        };

        var result = await ShowDialog.Handle(dialog);
        if (result == null) return;

        profile.Name     = result.Name;
        profile.WeightKg = result.WeightKg;
    }

    private async void EditCurrentProfile()
    {
        if (SelectedProfile == null) return;
        await EditProfile(SelectedProfile);

        // Force UI refresh
        this.RaisePropertyChanged(nameof(SelectedProfile));
    }

    private async void AddProfile()
    {
        var dialog = new ProfileDialogViewModel();
        var result = await ShowDialog.Handle(dialog);
        if (result == null) return;

        Profiles.Add(result);
        SelectedProfile = result;
        CloseFlyout();
    }

    private async Task DeleteProfile(ProfileViewModel profile)
    {
        var confirm = new ConfirmDialogViewModel
        {
            Title   = "Delete Profile",
            Message = $"Are you sure you want to delete profile '{profile.Name}'?"
        };

        var shouldDelete = await ShowConfirmDialog.Handle(confirm);
        if (!shouldDelete) return;

        Profiles.Remove(profile);
        CloseFlyout();
    }

    private void CloseFlyout() => _profileFlyout?.Hide();
}