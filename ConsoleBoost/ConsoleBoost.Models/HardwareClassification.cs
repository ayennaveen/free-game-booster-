namespace ConsoleBoost.Models;

public enum HardwareTier
{
    UltraLowEnd,
    EntryLevel,
    MidRange,
    HighEnd,
    Enthusiast
}

/// <summary>
/// Data model representing the classification of system hardware.
/// </summary>
public class HardwareClassification
{
    public HardwareTier Tier { get; set; } = HardwareTier.EntryLevel;
    public string TierName { get; set; } = "Entry-Level Portable";
    public int HardwareScore { get; set; } = 45; // 1 - 100

    public string CpuName { get; set; } = "Intel Core i3-8130U";
    public int CpuCores { get; set; } = 2;
    public int CpuThreads { get; set; } = 4;
    public double CpuClockGhz { get; set; } = 2.20;

    public double RamTotalGb { get; set; } = 16.0;

    public string GpuName { get; set; } = "Intel UHD Graphics 620";
    public double VramTotalGb { get; set; } = 1.0;
    public bool IsIntegratedGpu { get; set; } = true;

    public bool IsLaptop { get; set; } = true;
    public bool IsOnBattery { get; set; }

    public string Description { get; set; } = "Suitable for light esports, casual gaming, and 720p/1080p low settings with optimized background services.";
    public DateTime EvaluatedAt { get; set; } = DateTime.Now;
}
