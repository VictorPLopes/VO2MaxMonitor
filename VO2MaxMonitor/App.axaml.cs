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
    private IServiceProvider? Services { get; set; }

    /// <inheritdoc />
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    /// <inheritdoc />
    public override async void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        RegisterServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            InitializeMainWindow(desktop, services);
        }

        await InitializeViewModelAsync();
        base.OnFrameworkInitializationCompleted();
    }

    private void RegisterServices(IServiceCollection services)
    {
        services.AddSingleton<IMeasurementsJsonFilesService, MeasurementsJsonFilesService>();
        services.AddSingleton<IVO2MaxCalculator>(_ =>
                                                     new VO2MaxCalculator(1.225, 0.852, 20.93, 30000));
        services.AddSingleton<MainWindowViewModel>();
    }

    private void InitializeMainWindow(IClassicDesktopStyleApplicationLifetime desktop, IServiceCollection services)
    {
        var mainVm = Services!.GetRequiredService<MainWindowViewModel>();
        desktop.MainWindow = new MainWindow { DataContext = mainVm };

        services.AddSingleton<IFilesService>(_ => new CsvFilesService(desktop.MainWindow));
        Services = services.BuildServiceProvider();

        desktop.ShutdownRequested += OnShutdownRequested;
    }

    private async Task InitializeViewModelAsync()
    {
        var mainVm      = Services!.GetRequiredService<MainWindowViewModel>();
        var fileService = Services.GetRequiredService<IMeasurementsJsonFilesService>();

        var itemsLoaded = await fileService.LoadFromFileAsync();
        if (itemsLoaded != null)
            foreach (var item in itemsLoaded)
                mainVm.Measurements.Add(new MeasurementViewModel(item));
    }

    private async void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
    {
        e.Cancel = !_canClose;
        if (_canClose) return;

        var mainVm      = Services!.GetRequiredService<MainWindowViewModel>();
        var fileService = Services.GetRequiredService<IMeasurementsJsonFilesService>();

        await fileService.SaveToFileAsync(mainVm.Measurements.Select(m => m.Model));
        _canClose = true;

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) desktop.Shutdown();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var pluginsToRemove = BindingPlugins.DataValidators
                                            .OfType<DataAnnotationsValidationPlugin>()
                                            .ToArray();

        foreach (var plugin in pluginsToRemove) BindingPlugins.DataValidators.Remove(plugin);
    }
}