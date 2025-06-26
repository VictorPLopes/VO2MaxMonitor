using ReactiveUI;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     Base class for all view models in the V̇O₂ max monitoring application.
/// </summary>
public class ViewModelBase : ReactiveObject
{
    /// <summary>
    ///     Title of the view model passed to the view.
    /// </summary>
    public string Title { get; set; } = string.Empty;
}