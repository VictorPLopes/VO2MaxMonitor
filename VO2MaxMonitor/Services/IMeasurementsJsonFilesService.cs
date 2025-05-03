using System.Collections.Generic;
using System.Threading.Tasks;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

/// <summary>
///     Provides persistence operations for measurement data in JSON format.
/// </summary>
public interface IMeasurementsJsonFilesService
{
    /// <summary>
    ///     Saves a collection of measurements to persistent storage.
    /// </summary>
    /// <param name="itemsToSave">The measurements to persist.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SaveToFileAsync(IEnumerable<Measurement> itemsToSave);

    /// <summary>
    ///     Loads measurements from persistent storage.
    /// </summary>
    /// <returns>
    ///     The loaded measurements or null if no data exists.
    /// </returns>
    Task<IEnumerable<Measurement>?> LoadFromFileAsync();
}