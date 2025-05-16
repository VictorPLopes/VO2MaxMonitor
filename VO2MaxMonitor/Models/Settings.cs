using System;

namespace VO2MaxMonitor.Models;

/// <summary>
///     Represents the application settings.
/// </summary>
public class Settings
{
    /// <summary>
    ///     Gets or sets the last opened profile ID.
    /// </summary>
    public Guid LastProfileId { get; set; } = Guid.Empty;
}