namespace VO2MaxMonitor.Models;

public readonly record struct Reading(
    double VenturiAreaRegular,
    double VenturiAreaConstricted,
    double O2,
    double DifferentialPressure,
    ulong  TimeStamp
);