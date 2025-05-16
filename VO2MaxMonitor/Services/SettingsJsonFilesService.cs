using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

/// <summary>
///     JSON-based implementation of <see cref="ISettingsJsonFilesService" />.
/// </summary>
/// <remarks>
///     Stores settings in the application data folder.
/// </remarks>
public class SettingsJsonFilesService : ISettingsJsonFilesService
{
    private readonly string _jsonFilePath = Path.Combine(
                                                         Environment.GetFolderPath(Environment.SpecialFolder
                                                                  .ApplicationData),
                                                         "VO2MaxMonitor",
                                                         "settings.json"
                                                        );

    /// <inheritdoc />
    public async Task SaveToFileAsync(Settings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_jsonFilePath)!);
            await using var fs = File.Create(_jsonFilePath);
            await JsonSerializer.SerializeAsync(fs, settings);
        }
        catch (Exception ex)
        {
            throw new IOException("Failed to save user settings to file", ex);
        }
    }

    /// <inheritdoc />
    public async Task<Settings?> LoadFromFileAsync()
    {
        try
        {
            await using var fs = File.OpenRead(_jsonFilePath);
            return await JsonSerializer.DeserializeAsync<Settings>(fs);
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
            throw new IOException("Failed to load settings from file", ex);
        }
    }
}