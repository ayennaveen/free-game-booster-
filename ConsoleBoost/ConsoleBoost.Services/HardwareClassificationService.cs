using System.Management;
using ConsoleBoost.Models;

namespace ConsoleBoost.Services;

public class HardwareClassificationService : IHardwareClassificationService
{
    private readonly ILoggingService _logger;

    public HardwareClassificationService(ILoggingService logger)
    {
        _logger = logger;
    }

    public HardwareClassification ClassifyHardware(HardwareMetrics currentMetrics)
    {
        var result = new HardwareClassification
        {
            EvaluatedAt = DateTime.Now
        };

        try
        {
            // Populate CPU specs
            result.CpuName = string.IsNullOrWhiteSpace(currentMetrics.CpuName) || currentMetrics.CpuName.Contains("Generic")
                ? GetWmiProperty("Win32_Processor", "Name", "Intel Core i3-8130U CPU @ 2.20GHz")
                : currentMetrics.CpuName;

            result.CpuCores = Environment.ProcessorCount / 2 > 0 ? Environment.ProcessorCount / 2 : 2;
            result.CpuThreads = Environment.ProcessorCount;

            // Populate RAM specs
            result.RamTotalGb = currentMetrics.RamTotalGb > 0 ? currentMetrics.RamTotalGb : 16.0;

            // Populate GPU specs
            result.GpuName = string.IsNullOrWhiteSpace(currentMetrics.GpuName) || currentMetrics.GpuName.Contains("Generic")
                ? GetWmiProperty("Win32_VideoController", "Name", "Intel UHD Graphics 620")
                : currentMetrics.GpuName;

            result.IsIntegratedGpu = CheckIsIntegratedGpu(result.GpuName);
            result.VramTotalGb = currentMetrics.VramTotalGb > 0 ? currentMetrics.VramTotalGb : (result.IsIntegratedGpu ? 1.0 : 4.0);

            // Compute composite score (1 - 100)
            int score = CalculateCompositeScore(result);
            result.HardwareScore = score;

            // Determine Tier
            if (score < 30)
            {
                result.Tier = HardwareTier.UltraLowEnd;
                result.TierName = "Ultra Low-End";
                result.Description = "Designed for basic tasks. Aggressive background service pausing and lightweight UI rendering required.";
            }
            else if (score < 55)
            {
                result.Tier = HardwareTier.EntryLevel;
                result.TierName = "Entry-Level Portable";
                result.Description = "Optimal for 720p/1080p light esports & casual gaming with balanced background process trimming.";
            }
            else if (score < 75)
            {
                result.Tier = HardwareTier.MidRange;
                result.TierName = "Mid-Range Gaming System";
                result.Description = "Solid 1080p high settings capability. Moderate optimization profile recommended.";
            }
            else if (score < 90)
            {
                result.Tier = HardwareTier.HighEnd;
                result.TierName = "High-End Gaming Rig";
                result.Description = "Capable of 1440p / 4K gaming with ray tracing. Light background tuning recommended.";
            }
            else
            {
                result.Tier = HardwareTier.Enthusiast;
                result.TierName = "Enthusiast Workstation";
                result.Description = "Top-tier hardware. Maximum polling precision and full visual effects enabled.";
            }

            _logger.LogInfo($"Hardware Classified: {result.TierName} (Score: {result.HardwareScore}/100) - CPU: {result.CpuName}, GPU: {result.GpuName}");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error classifying hardware tier", ex);
        }

        return result;
    }

    private int CalculateCompositeScore(HardwareClassification h)
    {
        int score = 0;

        // CPU scoring (max 35 points)
        if (h.CpuThreads >= 16) score += 35;
        else if (h.CpuThreads >= 8) score += 28;
        else if (h.CpuThreads >= 4) score += 18;
        else score += 10;

        // RAM scoring (max 25 points)
        if (h.RamTotalGb >= 32.0) score += 25;
        else if (h.RamTotalGb >= 16.0) score += 20;
        else if (h.RamTotalGb >= 8.0) score += 12;
        else score += 5;

        // GPU scoring (max 40 points)
        if (!h.IsIntegratedGpu)
        {
            if (h.VramTotalGb >= 12.0) score += 40;
            else if (h.VramTotalGb >= 8.0) score += 35;
            else if (h.VramTotalGb >= 4.0) score += 25;
            else score += 18;
        }
        else
        {
            // Integrated Graphics (e.g. Intel UHD / Iris)
            score += 10;
        }

        return Math.Clamp(score, 1, 100);
    }

    private bool CheckIsIntegratedGpu(string gpuName)
    {
        string name = gpuName.ToLowerInvariant();
        return name.Contains("intel") || name.Contains("uhd") || name.Contains("hd graphics") || name.Contains("iris") || name.Contains("vega 3") || name.Contains("integrated");
    }

    private string GetWmiProperty(string wmiClass, string propertyName, string fallback)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher($"SELECT {propertyName} FROM {wmiClass}");
            foreach (ManagementObject obj in searcher.Get().Cast<ManagementObject>())
            {
                string? val = obj[propertyName]?.ToString();
                if (!string.IsNullOrWhiteSpace(val))
                    return val.Trim();
            }
        }
        catch { }
        return fallback;
    }
}
