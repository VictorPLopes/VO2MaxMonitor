using System;
using System.Collections.ObjectModel;

namespace VO2MaxMonitor.Models;

public class Profile(string name, double weightKg)
{
    public Guid Id { get; set; } = Guid.NewGuid();

    private string _name { get; set; } = name?.Trim() ?? string.Empty;

    public string Name
    {
        get => _name;
        set => _name = value?.Trim() ?? string.Empty;
    }

    public double                            WeightKg     { get; set; } = 0.0;
    public ObservableCollection<Measurement> Measurements { get; set; } = [];

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}