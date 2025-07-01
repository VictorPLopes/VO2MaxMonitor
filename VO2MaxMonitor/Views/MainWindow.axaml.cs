using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.ReactiveUI;
using Avalonia.Styling;
using ReactiveUI;
using VO2MaxMonitor.ViewModels;

namespace VO2MaxMonitor.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();

        SetBackgroundBasedOnTheme();

        this.WhenActivated(disposables =>
        {
            disposables.Add(ViewModel!.ShowConfirmDialog.RegisterHandler(DoShowConfirmDialogAsync));

            // Set the flyout reference by getting it from the button
            var button = this.FindControl<Button>("ProfileButton");
            ViewModel.SetFlyout(button!.Flyout!);
        });
    }

    private async Task DoShowConfirmDialogAsync(IInteractionContext<ConfirmDialogViewModel, bool> interaction)
    {
        var dialog = new ConfirmDialog
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<bool>(this);
        interaction.SetOutput(result);
    }

    // Workaround to set padding for maximized windows on Windows OS.
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // If the OS is not Windows, we don't need to apply this workaround.
        if (!OperatingSystem.IsWindows()) return;

        var style = new Style
        {
            Selector = Selectors.Is<Window>(null)
                                .PropertyEquals(WindowStateProperty, WindowState.Maximized)
        };

        // On Windows, when a window is maximized, add padding to the window
        var setter = new Setter
        {
            Property = PaddingProperty,
            Value    = Thickness.Parse("8")
        };
        style.Add(setter);
        Styles.Add(style);
    }

    private void SetBackgroundBasedOnTheme()
    {
        // Check the actual transparency level that was applied
        var appliedTransparency = ActualTransparencyLevel;

        if (appliedTransparency == WindowTransparencyLevel.Mica) return;
        // Fallback color based on theme
        var isDark = ActualThemeVariant == ThemeVariant.Dark ||
                     Application.Current?.ActualThemeVariant == ThemeVariant.Dark ||
                     RequestedThemeVariant == ThemeVariant.Dark;

        Background = new SolidColorBrush(Color.Parse(isDark ? "#FA202020" : "#FAF2F2F2"));
    }
}