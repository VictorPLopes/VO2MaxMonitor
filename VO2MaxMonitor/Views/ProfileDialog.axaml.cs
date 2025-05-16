using System;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using VO2MaxMonitor.ViewModels;

namespace VO2MaxMonitor.Views;

public partial class ProfileDialog : ReactiveWindow<ProfileDialogViewModel>
{
    public ProfileDialog()
    {
        InitializeComponent();

        if (Design.IsDesignMode) return;

        this.WhenActivated(disposables =>
        {
            // Handle Save command
            disposables(ViewModel!.SaveCommand.Subscribe(Close));

            // Handle Cancel command
            disposables(ViewModel!.CancelCommand.Subscribe(_ => { Close(null); }));
        });
    }
}