using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using VO2MaxMonitor.Models;
using VO2MaxMonitor.Services;
using VO2MaxMonitor.ViewModels;
using VO2MaxMonitor.Views;

namespace VO2MaxMonitor;

/// <summary>
///     The main application class responsible for initialization and service configuration.
/// </summary>
public class App : Application
{
    private bool _canClose;

    /// <summary>
    ///     Gets the current application instance.
    /// </summary>
    public new static App? Current => Application.Current as App;

    /// <summary>
    ///     Gets the application's service provider.
    /// </summary>
    public IServiceProvider? Services { get; private set; }

    /// <inheritdoc />
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc />
    public override async void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // Register services and view models that do not depend on MainWindow
        services.AddSingleton<IMeasurementsJsonFilesService, MeasurementsJsonFilesService>();
        services.AddSingleton<IProfilesJsonFilesService, ProfilesJsonFilesService>();
        services.AddSingleton<ISettingsJsonFilesService, SettingsJsonFilesService>();
        services.AddSingleton<IVO2MaxCalculator, VO2MaxCalculator>();
        services.AddSingleton<MainWindowViewModel>();

        // Build a temporary provider to create the view model
        Services = services.BuildServiceProvider();
        var mainVm = Services.GetRequiredService<MainWindowViewModel>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainVm
            };

            // Now that MainWindow exists, register CsvFilesService
            services.AddSingleton<IFilesService>(x => new CsvFilesService(desktop.MainWindow));

            // Rebuild ServiceProvider with the new service
            Services = services.BuildServiceProvider();

            // Hook shutdown logic
            desktop.ShutdownRequested += (s, e) => DesktopOnShutdownRequested(s, e, mainVm);
        }

        // Init the MainViewModel
        await InitMainViewModelAsync(mainVm);
        base.OnFrameworkInitializationCompleted();
    }

    // Load data from disc
    private async Task InitMainViewModelAsync(MainWindowViewModel vm)
    {
        // Load settings
        var settingsFileService = Services!.GetRequiredService<ISettingsJsonFilesService>();
        var settings            = await settingsFileService.LoadFromFileAsync();

        // Load profiles
        var profilesFileService = Services!.GetRequiredService<IProfilesJsonFilesService>();
        // Get the items to load
        var profilesLoaded = await profilesFileService.LoadFromFileAsync();
        var profiles       = profilesLoaded?.ToList();

        // If there are no profiles, create a default one
        if (profiles is null || profiles.Count == 0)
        {
            profiles = [new Profile("Default", 70.0)];
            // Set the new profile as the selected one
            vm.SelectedProfile = new ProfileViewModel(profiles[0]);
            // Add the new profile to the ViewModel
            vm.Profiles.Add(vm.SelectedProfile);
        }
        else // If there are profiles
        {
            // Add the profiles to the ViewModel, load its measurements and set the selected one
            foreach (var profile in profiles)
            {
                var profileVm = new ProfileViewModel(profile);

                // Load the measurements for this profile
                var measurementsFileService = Services!.GetRequiredService<IMeasurementsJsonFilesService>();
                var measurementsLoaded      = await measurementsFileService.LoadFromFileAsync(profileVm.Model.Id);

                // Add the measurements to the ViewModel
                if (measurementsLoaded is null) continue;
                foreach (var measurement in measurementsLoaded)
                    profileVm.AddMeasurement(new MeasurementViewModel(measurement));

                // Add the profile to the ViewModel
                vm.Profiles.Add(profileVm);
                // If the profile is the last one used, set it as selected
                if (profile.Id == settings?.LastProfileId)
                    vm.SelectedProfile = profileVm;
            }

            // If no profile was selected, select the first one
            vm.SelectedProfile ??= vm.Profiles[0];
        }
    }

    // We want to save our data before we actually shutdown the App. As File I/O is async, we need to wait until file is closed
    // before we can actually close this window
    private async void DesktopOnShutdownRequested(object? sender, ShutdownRequestedEventArgs e, MainWindowViewModel vm)
    {
        e.Cancel = !_canClose; // cancel closing event first time
        if (_canClose) return;

        // Save settings
        var settingsFileService = Services!.GetRequiredService<ISettingsJsonFilesService>();
        var settings = new Settings
        {
            LastProfileId = vm.SelectedProfile?.Model.Id ?? Guid.Empty
        };
        await settingsFileService.SaveToFileAsync(settings);

        // To save the items, we map them to the Profile model which is better suited for I/O operations
        var fileService = Services!.GetRequiredService<IProfilesJsonFilesService>();
        var itemsToSave = vm.Profiles.Select(item => item.Model);
        await fileService.SaveToFileAsync(itemsToSave);

        // Save measurements for each profile
        foreach (var profile in vm.Profiles)
        {
            var measurementsFileService = Services!.GetRequiredService<IMeasurementsJsonFilesService>();
            var measurementsToSave      = profile.Measurements.Select(item => item.Model);
            await measurementsFileService.SaveToFileAsync(measurementsToSave, profile.Model.Id);
        }

        // Set _canClose to true and Close this Window again
        _canClose = true;
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) desktop.Shutdown();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
    }
}