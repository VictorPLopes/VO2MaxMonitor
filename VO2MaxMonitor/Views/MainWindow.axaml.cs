using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using VO2MaxMonitor.ViewModels;

namespace VO2MaxMonitor.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(disposables =>
        {
            disposables.Add(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync));
            disposables.Add(ViewModel!.ShowConfirmDialog.RegisterHandler(DoShowConfirmDialogAsync));

            // Set the flyout reference by getting it from the button
            var button = this.FindControl<Button>("ProfileButton");
            ViewModel.SetFlyout(button.Flyout);
        });
    }

    private async Task DoShowDialogAsync(IInteractionContext<ProfileDialogViewModel, ProfileViewModel?> interaction)
    {
        var dialog = new ProfileDialog
        {
            DataContext = interaction.Input
        };

        var result = await dialog.ShowDialog<ProfileViewModel?>(this);
        interaction.SetOutput(result);
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
}