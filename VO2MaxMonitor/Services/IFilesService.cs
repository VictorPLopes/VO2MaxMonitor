using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace VO2MaxMonitor.Services;

/// <summary>
///     Provides file system operations for the application.
/// </summary>
public interface IFilesService
{
    /// <summary>
    ///     Opens a file picker dialog to select a single file.
    /// </summary>
    /// <returns>
    ///     The selected file or null if no file was selected.
    /// </returns>
    Task<IStorageFile?> OpenFileAsync();

    /// <summary>
    ///     Opens a save file dialog to save a file.
    /// </summary>
    /// <returns>
    ///     The selected file or null if no file was selected.
    /// </returns>
    Task<IStorageFile?> SaveFileAsync();
}