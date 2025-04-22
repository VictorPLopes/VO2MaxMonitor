using System.Collections.Generic;
using System.Threading.Tasks;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

public interface IMeasurementsFileService
{
    Task                            SaveToFileAsync(IEnumerable<Measurement> itemsToSave);
    Task<IEnumerable<Measurement>?> LoadFromFileAsync();
}