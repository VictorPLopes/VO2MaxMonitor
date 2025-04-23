using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace VO2MaxMonitor.Services;

public interface IFilesService
{
    public Task<IStorageFile?> OpenFileAsync();
}