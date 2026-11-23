using ConsoleBoost.Models;

namespace ConsoleBoost.Services;

/// <summary>
/// Service interface for classifying system hardware into performance tiers.
/// </summary>
public interface IHardwareClassificationService
{
    HardwareClassification ClassifyHardware(HardwareMetrics currentMetrics);
}
