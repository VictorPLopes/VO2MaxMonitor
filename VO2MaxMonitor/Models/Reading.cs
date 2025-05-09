namespace VO2MaxMonitor.Models;

/// <summary>
///     Represents a single reading from the device.
/// </summary>
/// <param name="VenturiAreaRegular">Cross-sectional area of venturi tube (m²) before constriction.</param>
/// <param name="VenturiAreaConstricted">Cross-sectional area at venturi constriction (m²).</param>
/// <param name="O2">Measured oxygen concentration in percent.</param>
/// <param name="DifferentialPressure">Pressure difference across venturi in Pascals (Pa).</param>
/// <param name="TimeStamp">Relative time of the reading (ms).</param>
public readonly record struct Reading(
    double VenturiAreaRegular,
    double VenturiAreaConstricted,
    double O2,
    double DifferentialPressure,
    ulong  TimeStamp
);