using System;
using System.Collections.Generic;
using System.Linq;
using VO2MaxMonitor.Models;

namespace VO2MaxMonitor.Services;

public class VO2MaxCalculator(double o2Density, double airDryness, double ambientO2, uint vO2ComputationInterval) : IVO2MaxCalculator
{
    // Constants
    private readonly double _o2Density              = o2Density; // kg/m³
    private readonly double _airDryness             = airDryness; // Correction factor for dry air
    private readonly double _ambientO2              = ambientO2; // % of O2 in ambient air
    private readonly uint   _vO2ComputationInterval = vO2ComputationInterval; // ms between VO2 calculations
    
    // Timer struct
    private struct Timer
    {
        public ulong vo2;
        public ulong flow;
    }
    
    public double Calculate(IEnumerable<Reading> readings, double weightKg)
    {
        // Validate inputs
        if (readings == null || !readings.Any())
            throw new ArgumentException("No sensor readings provided");
        
        if (weightKg <= 0)
            throw new ArgumentException("Weight must be positive");
        
        // Start timer
        var startTime = readings.First().TimeStamp;
        var timer = new Timer
        {
            vo2  = startTime,
            flow = startTime
        };
        
        // Initialize variables
        var totalVolume = 0.0;
        var vO2Max      = 0.0;

        // Start calculations
        foreach (var reading in readings)
        {
            // 1. COMPUTE AIRFLOW (when there's breathing)
            if (reading.DifferentialPressure > 1.0)
            {
                var deltaTime = (reading.TimeStamp - timer.flow) / 1000.0; // in seconds
                var flowRate = ComputeAirFlow(reading.DifferentialPressure, reading.VenturiAreaRegular, reading.VenturiAreaConstricted);
                totalVolume += flowRate * deltaTime; // integrates to volume (m³)
                timer.flow = reading.TimeStamp;
            }
            
            // 2. PERIODIC VO2 CALCULATION
            if (reading.TimeStamp - timer.vo2 <= _vO2ComputationInterval) continue;
            vO2Max = Math.Max(ComputeVO2(totalVolume, reading.O2, weightKg), vO2Max);
                
            // Reset total volume for next interval
            totalVolume = 0.0;
            timer.vo2   = reading.TimeStamp;
        }
        return vO2Max;
    }

    private double ComputeAirFlow(double differentialPressure, double venturiAreaRegular, double venturiAreaConstricted)
    {
        var numerator   = Math.Abs(differentialPressure) * 2.0 * _o2Density;
        var denominator = (1 / Math.Pow(venturiAreaConstricted, 2)) - (1 / Math.Pow(venturiAreaRegular, 2));
        return Math.Sqrt(numerator / denominator) / _o2Density;
    }

    private double ComputeVO2(double volume, double o2, double weightKg)
    {
        // Gaseous concentration calculations
        var co2 = _ambientO2 - o2; // Simplified estimate
        var n2  = 100.0 - o2 - co2; // % of N2 in exhaled air
        
        // Volume corrected for a minute (L/min)
        var minuteVolume = volume * (60000.0 / _vO2ComputationInterval) * _airDryness;
        
        // Haldane Transformation
        var o2Consumption = minuteVolume * ((n2 * 0.00265) - (o2 / 100.0));
        
        // Conversion to ml/min/kg
        var vo2 = (o2Consumption * 1000.0) / weightKg;
        
        // Validate plausible values
        if (minuteVolume < 0.1 || o2 > 21.0 || o2 < 10.0)
            return -1.0; // Error: unexpected values (forces the last valid VO2Max to be used)

        return vo2;
    }
}