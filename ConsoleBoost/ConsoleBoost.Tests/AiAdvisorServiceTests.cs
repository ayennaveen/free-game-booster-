using ConsoleBoost.Models;
using ConsoleBoost.Services;
using Xunit;

namespace ConsoleBoost.Tests;

public class AiAdvisorServiceTests
{
    private class DummyLogger : ILoggingService
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? exception = null) { }
        public void LogOptimization(string details) { }
        public void LogGameLaunch(string gameName, string path) { }
    }

    [Fact]
    public void DetectsGpuBottleneck_WhenGpuHighAndCpuLow()
    {
        var service = new AiAdvisorService(new DummyLogger());
        var metrics = new HardwareMetrics
        {
            GpuUsagePercent = 98.0,
            CpuUsagePercent = 45.0,
            RamUsedGb = 8.0,
            RamTotalGb = 16.0
        };
        var powerPlan = new PowerPlanInfo { Type = PowerPlanType.HighPerformance, Name = "High Performance" };

        var recommendations = service.AnalyzeTelemetry(metrics, powerPlan).ToList();

        Assert.Contains(recommendations, r => r.Problem.Contains("GPU Bottleneck"));
    }

    [Fact]
    public void DetectsCpuBottleneck_WhenCpuHighAndGpuLow()
    {
        var service = new AiAdvisorService(new DummyLogger());
        var metrics = new HardwareMetrics
        {
            GpuUsagePercent = 40.0,
            CpuUsagePercent = 92.0,
            RamUsedGb = 8.0,
            RamTotalGb = 16.0
        };
        var powerPlan = new PowerPlanInfo { Type = PowerPlanType.HighPerformance, Name = "High Performance" };

        var recommendations = service.AnalyzeTelemetry(metrics, powerPlan).ToList();

        Assert.Contains(recommendations, r => r.Problem.Contains("CPU Bottleneck"));
    }

    [Fact]
    public void DetectsRamPressure_WhenRamAbove85Percent()
    {
        var service = new AiAdvisorService(new DummyLogger());
        var metrics = new HardwareMetrics
        {
            RamUsedGb = 14.5,
            RamTotalGb = 16.0
        };
        var powerPlan = new PowerPlanInfo { Type = PowerPlanType.HighPerformance, Name = "High Performance" };

        var recommendations = service.AnalyzeTelemetry(metrics, powerPlan).ToList();

        Assert.Contains(recommendations, r => r.Problem.Contains("RAM Pressure"));
    }

    [Fact]
    public void DetectsThermalWarning_WhenCpuAbove85C()
    {
        var service = new AiAdvisorService(new DummyLogger());
        var metrics = new HardwareMetrics
        {
            CpuTemperatureC = 89.0,
            RamUsedGb = 8.0,
            RamTotalGb = 16.0
        };
        var powerPlan = new PowerPlanInfo { Type = PowerPlanType.HighPerformance, Name = "High Performance" };

        var recommendations = service.AnalyzeTelemetry(metrics, powerPlan).ToList();

        Assert.Contains(recommendations, r => r.Problem.Contains("CPU Thermal Throttling"));
    }
}
