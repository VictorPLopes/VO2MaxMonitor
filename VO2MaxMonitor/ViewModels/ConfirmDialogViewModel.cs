using System.Reactive;
using ReactiveUI;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for the confirmation dialog.
/// </summary>
public class ConfirmDialogViewModel : ViewModelBase
{
    /// <summary>
    ///     Gets or sets the title of the dialog.
    /// </summary>
    public string Title   { get; set; } = "Confirm";
    
    /// <summary>
    ///     Gets or sets the message to display in the dialog.
    /// </summary>
    public string Message { get; set; } = "Are you sure?";

    /// <summary>
    ///     Command to confirm the action.
    /// </summary>
    public ReactiveCommand<Unit, bool> ConfirmCommand { get; } = ReactiveCommand.Create(() => true);
    
    /// <summary>
    ///     Command to cancel the action.
    /// </summary>
    public ReactiveCommand<Unit, bool> CancelCommand  { get; } = ReactiveCommand.Create(() => false);
}