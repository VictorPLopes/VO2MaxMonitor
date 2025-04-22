using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

public class MeasurementsFileService : IMeasurementsFileService
{
    // TODO: Implement different files for different users.
    // For now, we will use a single file (and a single user) with a fixed name.
    private readonly string _jsonFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VO2MaxMonitor", "measurements.json");
    
    
    // Saves the measurements to a JSON file.
    public async Task SaveToFileAsync(IEnumerable<Measurement> itemsToSave)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_jsonFilePath)!);
            await using var fs = File.Create(_jsonFilePath);
            await JsonSerializer.SerializeAsync(fs, itemsToSave);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    // Loads the measurements from a JSON file.
    public async Task<IEnumerable<Measurement>?> LoadFromFileAsync()
    {
        try
        {
            await using var fs = File.OpenRead(_jsonFilePath);
            return await JsonSerializer.DeserializeAsync<IEnumerable<Measurement>>(fs);
        }
        catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
        {
            return null;
        }
    }
}