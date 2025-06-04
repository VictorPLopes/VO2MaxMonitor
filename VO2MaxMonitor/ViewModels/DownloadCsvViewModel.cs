using VO2MaxMonitor.Services;

namespace VO2MaxMonitor.ViewModels;

/// <summary>
///     ViewModel for downloading readings from an MQTT broker and saving them as a CSV file.
/// </summary>
public class DownloadCsvViewModel : ViewModelBase
{
    private readonly IFilesService _filesService;
    private string _broker = string.Empty;
    private int _port = 1883;
    private string _topic = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string _filePath = string.Empty;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DownloadCsvViewModel" /> class.
    /// </summary>
    public DownloadCsvViewModel()
    {
        
    }
}