namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for the welcome screen (default screen when no measurement is selected).
/// </summary>
public class WelcomeViewModel : ViewModelBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="WelcomeViewModel" /> class.
    /// </summary>
    public WelcomeViewModel() => Title = "Welcome to V̇O₂ Max Monitor";
}