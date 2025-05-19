using System;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using ReactiveUI;
using VO2MaxMonitor.ViewModels;

namespace VO2MaxMonitor.Views;

public partial class ConfirmDialog : ReactiveWindow<ConfirmDialogViewModel>
{
    public ConfirmDialog()
    {
        InitializeComponent();

        if (Design.IsDesignMode) return;

        this.WhenActivated(disposables =>
        {
            // Handle Confirm command
            disposables(ViewModel!.ConfirmCommand.Subscribe(result => Close(result)));

            // Handle Cancel command
            disposables(ViewModel!.CancelCommand.Subscribe(result => Close(result)));
        });
    }
}