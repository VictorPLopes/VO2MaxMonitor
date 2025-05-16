using System.Threading.Tasks;
using Avalonia.ReactiveUI;
using ReactiveUI;
using VO2MaxMonitor.ViewModels;

namespace VO2MaxMonitor.Views;

public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
{
    public MainWindow()
    {
        InitializeComponent();
        this.WhenActivated(action =>
                               action(ViewModel!.ShowDialog.RegisterHandler(DoShowDialogAsync)));
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
}