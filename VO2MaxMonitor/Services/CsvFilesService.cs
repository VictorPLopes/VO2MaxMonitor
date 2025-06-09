using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace VO2MaxMonitor.Services;

/// <summary>
///     Implementation of <see cref="IFilesService" /> for CSV file operations.
/// </summary>
/// <param name="target">The parent window for file dialogs.</param>
public class CsvFilesService(Window target) : IFilesService
{
    private readonly Window _target = target ?? throw new ArgumentNullException(nameof(target));

    /// <inheritdoc />
    public async Task<IStorageFile?> OpenFileAsync()
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title         = "Open CSV File",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("CSV files") { Patterns = ["*.csv"] },
                new FilePickerFileType("All files") { Patterns = ["*"] }
            ]
        });

        return files.Count >= 1 ? files[0] : null;
    }

    /// <inheritdoc />
    public async Task<IStorageFile?> SaveFileAsync() =>
        await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save CSV File",
            FileTypeChoices =
            [
                new FilePickerFileType("CSV files") { Patterns = ["*.csv"] },
                new FilePickerFileType("All files") { Patterns = ["*"] }
            ]
        });
}