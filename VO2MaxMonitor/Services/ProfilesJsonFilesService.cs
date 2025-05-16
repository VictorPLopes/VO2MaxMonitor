using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

/// <summary>
///     JSON-based implementation of <see cref="IProfilesJsonFilesService" />.
/// </summary>
/// <remarks>
///     Stores profiles in the application data folder.
/// </remarks>
public class ProfilesJsonFilesService : IProfilesJsonFilesService
{
    private readonly string _jsonFilePath = Path.Combine(
                                                         Environment.GetFolderPath(Environment.SpecialFolder
                                                                  .ApplicationData),
                                                         "VO2MaxMonitor",
                                                         "profiles.json"
                                                        );

    /// <inheritdoc />
    public async Task SaveToFileAsync(IEnumerable<Profile> itemsToSave)
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
            throw new IOException("Failed to save profiles to file", ex);
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Profile>?> LoadFromFileAsync()
    {
        try
        {
            await using var fs = File.OpenRead(_jsonFilePath);
            return await JsonSerializer.DeserializeAsync<IEnumerable<Profile>>(fs);
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
            throw new IOException("Failed to load profiles from file", ex);
        }
    }
}