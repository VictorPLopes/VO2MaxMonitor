using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VO2MaxMonitor.Models;

/// <summary>
///     Represents a user profile in the V̇O₂ max monitoring application.
///     The profile includes user information and a list of associated measurements.
/// </summary>
/// <param name="name">The name of the profile.</param>
/// <param name="weightKg">Body mass of the subject in kilograms.</param>
public class Profile(string name, double weightKg)
{
    private string _name { get; set; } = name?.Trim() ?? string.Empty;

    /// <summary>
    ///     Gets or sets the unique identifier for the profile.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    ///     Gets or sets the name of the profile.
    /// </summary>
    public string Name
    {
        get => _name;
        set => _name = value?.Trim() ?? string.Empty;
    }

    /// <summary>
    ///     Gets or sets the body mass of the subject in kilograms.
    /// </summary>
    public double WeightKg { get; set; } = weightKg;

    /// <summary>
    ///     Gets or sets the list of measurements associated with this profile.
    /// </summary>
    [JsonIgnore]
    public List<Measurement> Measurements { get; set; } = [];

    /// <summary>
    ///     Gets or sets the date and time when the profile was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}