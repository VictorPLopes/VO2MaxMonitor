using System.Collections.Generic;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

public interface IVO2MaxCalculator
{
    double Calculate(List<Reading> readings, double weightKg);
}