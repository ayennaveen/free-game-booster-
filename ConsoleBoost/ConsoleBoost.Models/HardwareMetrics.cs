namespace ConsoleBoost.Models;

/// <summary>
/// Holds system telemetry data collected from hardware sensors and OS APIs.
/// </summary>
public class HardwareMetrics
{
    // CPU
    public double CpuUsagePercent { get; set; }
    public double CpuTemperatureC { get; set; }
    public double CpuClockMhz { get; set; }
    public double CpuPowerWatts { get; set; }
    public string CpuName { get; set; } = "Generic CPU";

    // GPU
    public double GpuUsagePercent { get; set; }
    public double GpuTemperatureC { get; set; }
    public double GpuClockMhz { get; set; }
    public double GpuPowerWatts { get; set; }
    public double GpuFanSpeedRpm { get; set; }
    public string GpuName { get; set; } = "Generic GPU";

    // RAM
    public double RamUsedGb { get; set; }
    public double RamTotalGb { get; set; }
    public double RamUsagePercent => RamTotalGb > 0 ? (RamUsedGb / RamTotalGb) * 100.0 : 0.0;

    // VRAM
    public double VramUsedGb { get; set; }
    public double VramTotalGb { get; set; }
    public double VramUsagePercent => VramTotalGb > 0 ? (VramUsedGb / VramTotalGb) * 100.0 : 0.0;

    // Disk
    public double DiskReadSpeedMbPerSec { get; set; }
    public double DiskWriteSpeedMbPerSec { get; set; }
    public double DiskUsagePercent { get; set; }

    // Network
    public double NetworkDownloadMbps { get; set; }
    public double NetworkUploadMbps { get; set; }

    // Timestamp
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
