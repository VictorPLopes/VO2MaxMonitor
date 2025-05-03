using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

/// <summary>
///     JSON-based implementation of <see cref="IMeasurementsJsonFilesService" />.
/// </summary>
/// <remarks>
///     Stores measurements in the application data folder.
/// </remarks>
public class MeasurementsJsonFilesService : IMeasurementsJsonFilesService
{
    // TODO: Implement different files for different users.
    // For now, we will use a single file (and a single user) with a fixed name.
    private readonly string _jsonFilePath = Path.Combine(
                                                         Environment.GetFolderPath(Environment.SpecialFolder
                                                             .ApplicationData),
                                                         "VO2MaxMonitor",
                                                         "measurements.json"
                                                        );

    /// <inheritdoc />
    public async Task SaveToFileAsync(IEnumerable<Measurement> itemsToSave)
    {
        ArgumentNullException.ThrowIfNull(itemsToSave);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_jsonFilePath)!);
            await using var fs = File.Create(_jsonFilePath);
            await JsonSerializer.SerializeAsync(fs, itemsToSave);
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to save measurements to file", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Measurement>?> LoadFromFileAsync()
    {
        try
        {
            await using var fs = File.OpenRead(_jsonFilePath);
            return await JsonSerializer.DeserializeAsync<IEnumerable<Measurement>>(fs);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (DirectoryNotFoundException)
        {
            return null;
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to load measurements from file", ex);
        }
    }
}