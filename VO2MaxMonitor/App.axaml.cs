using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using VO2MaxMonitor.Services;
using VO2MaxMonitor.ViewModels;
using VO2MaxMonitor.Views;

namespace VO2MaxMonitor;

public class App : Application
{
    private           bool              _canClose;
    public new static App?              Current  => Application.Current as App;
    public            IServiceProvider? Services { get; private set; }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override async void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // Register services and view models that do not depend on MainWindow
        services.AddSingleton<IMeasurementsJsonFilesService, MeasurementsJsonFilesService>();
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
        var fileService = Services!.GetRequiredService<IMeasurementsJsonFilesService>();
        // get the items to load
        var itemsLoaded = await fileService.LoadFromFileAsync();

        if (itemsLoaded is not null)
            foreach (var item in itemsLoaded)
                vm.Measurements.Add(new MeasurementViewModel(item));
    }

    // We want to save our measurements before we actually shutdown the App. As File I/O is async, we need to wait until file is closed
    // before we can actually close this window
    private async void DesktopOnShutdownRequested(object? sender, ShutdownRequestedEventArgs e, MainWindowViewModel vm)
    {
        e.Cancel = !_canClose; // cancel closing event first time

        if (_canClose) return;
        // To save the items, we map them to the ToDoItem-Model which is better suited for I/O operations
        var fileService = Services!.GetRequiredService<IMeasurementsJsonFilesService>();
        var itemsToSave = vm.Measurements.Select(item => item.Model);
        await fileService.SaveToFileAsync(itemsToSave);

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