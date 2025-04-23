using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace VO2MaxMonitor.Services;

public class CsvFilesService(Window target) : IFilesService
{
    public async Task<IStorageFile?> OpenFileAsync()
    {
        var files = await target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title         = "Open CSV File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("CSV files")
                {
                Patterns = ["*.csv"]
                },
                new FilePickerFileType("All files")
                {
                Patterns = ["*"]
                }
            ]
        });

        return files.Count >= 1 ? files[0] : null;
    }
}