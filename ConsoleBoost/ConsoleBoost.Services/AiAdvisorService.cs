using ConsoleBoost.Models;

namespace ConsoleBoost.Services;

public class AiAdvisorService : IAiAdvisorService
{
    private readonly ILoggingService _logger;

    public AiAdvisorService(ILoggingService logger)
    {
        _logger = logger;
    }

    public IEnumerable<PerformanceRecommendation> AnalyzeTelemetry(HardwareMetrics metrics, PowerPlanInfo powerPlan)
    {
        var recommendations = new List<PerformanceRecommendation>();

        try
        {
            // 1. GPU Bottleneck Check
            if (metrics.GpuUsagePercent > 94.0 && metrics.CpuUsagePercent < 70.0)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Problem = "GPU Bottleneck Detected",
                    Explanation = $"GPU usage is at {metrics.GpuUsagePercent:F1}% while CPU usage is only {metrics.CpuUsagePercent:F1}%. Graphics hardware is limiting frame rates.",
                    EstimatedImpact = "High Impact",
                    SuggestedAction = "Lower GPU-heavy settings such as Shadows, Ray Tracing, and DLSS/FSR Quality Mode.",
                    ExpectedImprovement = "+15-30% Frame Rate Increase",
                    Severity = RecommendationSeverity.Warning,
                    Category = RecommendationCategory.Gpu
                });
            }

            // 2. CPU Bottleneck Check
            if (metrics.CpuUsagePercent > 85.0 && metrics.GpuUsagePercent < 65.0)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Problem = "CPU Bottleneck Detected",
                    Explanation = $"CPU usage is saturated at {metrics.CpuUsagePercent:F1}% while GPU usage is at {metrics.GpuUsagePercent:F1}%. Game physics or background processes are bottlenecking frame pacing.",
                    EstimatedImpact = "High Impact",
                    SuggestedAction = "Trim non-essential background applications and lower CPU-bound game settings (Crowd Density, Draw Distance).",
                    ExpectedImprovement = "+20% 1% Low FPS Consistency",
                    Severity = RecommendationSeverity.Warning,
                    Category = RecommendationCategory.Cpu
                });
            }

            // 3. RAM Pressure Check
            if (metrics.RamUsagePercent > 85.0)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Problem = "RAM Pressure Detected",
                    Explanation = $"System memory usage is at {metrics.RamUsagePercent:F1}% ({metrics.RamUsedGb:F1} / {metrics.RamTotalGb:F1} GB). High RAM pressure forces Windows paging.",
                    EstimatedImpact = "High Impact",
                    SuggestedAction = "Execute Quick Working-Set Trim to release transient memory back to Windows.",
                    ExpectedImprovement = "Eliminates Micro-stutters and Pagefile Paging",
                    Severity = RecommendationSeverity.Critical,
                    Category = RecommendationCategory.Ram
                });
            }

            // 4. VRAM Saturation Check
            if (metrics.VramUsagePercent > 90.0)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Problem = "VRAM Saturation Warning",
                    Explanation = $"VRAM usage is at {metrics.VramUsagePercent:F1}% ({metrics.VramUsedGb:F1} / {metrics.VramTotalGb:F1} GB). Running out of VRAM causes texture swapping.",
                    EstimatedImpact = "Medium Impact",
                    SuggestedAction = "Lower Texture Quality settings by one tier in the active game.",
                    ExpectedImprovement = "Prevents Sudden Texture Pop-in and FPS Drops",
                    Severity = RecommendationSeverity.Warning,
                    Category = RecommendationCategory.Vram
                });
            }

            // 5. Thermal Throttling Risk (CPU)
            if (metrics.CpuTemperatureC > 85.0)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Problem = "CPU Thermal Throttling Risk",
                    Explanation = $"CPU temperature is reaching {metrics.CpuTemperatureC:F0}°C. Thermal limits will force clock downthrottling.",
                    EstimatedImpact = "Critical Impact",
                    SuggestedAction = "Ensure laptop ventilation is unblocked or increase fan profile curves.",
                    ExpectedImprovement = "Prevents Thermal Down-clocking",
                    Severity = RecommendationSeverity.Critical,
                    Category = RecommendationCategory.Thermal
                });
            }

            // 6. Thermal Throttling Risk (GPU)
            if (metrics.GpuTemperatureC > 83.0)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Problem = "GPU Thermal Throttling Risk",
                    Explanation = $"GPU core temperature is at {metrics.GpuTemperatureC:F0}°C. GPU Boost clocks decrease as temperature exceeds 83°C.",
                    EstimatedImpact = "High Impact",
                    SuggestedAction = "Increase GPU fan speed or cap frame rates to match display refresh rate.",
                    ExpectedImprovement = "Maintains Peak GPU Boost Clocks",
                    Severity = RecommendationSeverity.Critical,
                    Category = RecommendationCategory.Thermal
                });
            }

            // 7. Power Plan Efficiency Check
            if (powerPlan.Type != PowerPlanType.HighPerformance && powerPlan.Type != PowerPlanType.UltimatePerformance)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Problem = "Sub-optimal Power Scheme Active",
                    Explanation = $"Current power scheme is set to '{powerPlan.Name}'. CPU core parking and energy saving modes are active.",
                    EstimatedImpact = "Medium Impact",
                    SuggestedAction = "Switch to High Performance or Ultimate Performance power scheme.",
                    ExpectedImprovement = "Unlocks Full CPU Turbo Frequencies",
                    Severity = RecommendationSeverity.Info,
                    Category = RecommendationCategory.PowerPlan
                });
            }

            // 8. Storage Disk Saturation
            if (metrics.DiskUsagePercent > 88.0)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Problem = "High Disk Activity Detected",
                    Explanation = $"Disk active time is at {metrics.DiskUsagePercent:F1}%. High disk read/write contention degrades streaming asset loading.",
                    EstimatedImpact = "Medium Impact",
                    SuggestedAction = "Pause background downloads or disk indexing operations while gaming.",
                    ExpectedImprovement = "Faster Open-World Asset Streaming",
                    Severity = RecommendationSeverity.Warning,
                    Category = RecommendationCategory.Storage
                });
            }

            // Default nominal recommendation if system operating optimal
            if (recommendations.Count == 0)
            {
                recommendations.Add(new PerformanceRecommendation
                {
                    Problem = "System Operating Optimally",
                    Explanation = "All CPU, GPU, RAM, VRAM, and thermal metrics are operating within ideal performance parameters.",
                    EstimatedImpact = "Optimal",
                    SuggestedAction = "No action required. Ready for max settings gaming.",
                    ExpectedImprovement = "Maximum Framerate & Stability",
                    Severity = RecommendationSeverity.Info,
                    Category = RecommendationCategory.Cpu
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error generating AI advisor recommendations", ex);
        }

        return recommendations;
    }
}
