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
    /// <inheritdoc />
    public async Task SaveToFileAsync(IEnumerable<Measurement> itemsToSave, Guid id)
    {
        ArgumentNullException.ThrowIfNull(itemsToSave);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GetPath(id))!);
            await using var fs = File.Create(GetPath(id));
            await JsonSerializer.SerializeAsync(fs, itemsToSave);
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to save measurements to file", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Measurement>?> LoadFromFileAsync(Guid id)
    {
        try
        {
            await using var fs = File.OpenRead(GetPath(id));
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

    /// <summary>
    ///     Generates the file path for the measurements JSON file based on the provided ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    private static string GetPath(Guid id) =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VO2MaxMonitor", "profiles",
                     $"{id}.json");
}