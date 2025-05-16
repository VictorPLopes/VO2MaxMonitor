using System.Threading.Tasks;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

/// <summary>
///     Provides persistence operations for user settings in JSON format.
/// </summary>
public interface ISettingsJsonFilesService
{
    /// <summary>
    ///     Saves user settings to persistent storage.
    /// </summary>
    /// <param name="settings">The settings to persist.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveToFileAsync(Settings settings);

    /// <summary>
    ///     Loads settings from persistent storage.
    /// </summary>
    /// <returns>
    ///     The loaded settings or null if no data exists.
    /// </returns>
    Task<Settings?> LoadFromFileAsync();
}