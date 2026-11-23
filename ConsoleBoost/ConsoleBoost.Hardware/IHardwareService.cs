using ConsoleBoost.Models;

namespace ConsoleBoost.Hardware;

/// <summary>
/// Service interface for fetching real-time system hardware telemetry.
/// </summary>
public interface IHardwareService : IDisposable
{
    event Action<HardwareMetrics>? MetricsUpdated;
    HardwareMetrics CurrentMetrics { get; }
    void StartMonitoring();
    void StopMonitoring();
    Task<HardwareMetrics> GetMetricsAsync();
}
