using System.Collections.Generic;
using System.Threading.Tasks;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

/// <summary>
///     Provides persistence operations for user profiles in JSON format.
/// </summary>
public interface IProfilesJsonFilesService
{
    /// <summary>
    ///     Saves a collection of profiles to persistent storage.
    /// </summary>
    /// <param name="itemsToSave">The measurements to persist.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveToFileAsync(IEnumerable<Profile> itemsToSave);

    /// <summary>
    ///     Loads profiles from persistent storage.
    /// </summary>
    /// <returns>
    ///     The loaded profiles or null if no data exists.
    /// </returns>
    Task<IEnumerable<Profile>?> LoadFromFileAsync();
}