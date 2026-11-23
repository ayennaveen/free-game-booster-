using ConsoleBoost.Models;
using ConsoleBoost.Services;
using Xunit;

namespace ConsoleBoost.Tests;

public class HardwareClassificationServiceTests
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
    public void ClassifiesIntelCoreI3WithIntegratedGpu_AsEntryLevel()
    {
        var service = new HardwareClassificationService(new DummyLogger());
        var metrics = new HardwareMetrics
        {
            CpuName = "Intel Core i3-8130U CPU @ 2.20GHz",
            GpuName = "Intel UHD Graphics 620",
            RamTotalGb = 16.0,
            VramTotalGb = 1.0
        };

        var classification = service.ClassifyHardware(metrics);

        Assert.Equal(HardwareTier.EntryLevel, classification.Tier);
        Assert.True(classification.IsIntegratedGpu);
        Assert.True(classification.HardwareScore >= 30 && classification.HardwareScore < 55);
    }

    [Fact]
    public void ClassifiesHighEndHardware_AsHighEndOrEnthusiast()
    {
        var service = new HardwareClassificationService(new DummyLogger());
        var metrics = new HardwareMetrics
        {
            CpuName = "Intel Core i9-13900K",
            GpuName = "NVIDIA GeForce RTX 4080",
            RamTotalGb = 32.0,
            VramTotalGb = 16.0
        };

        var classification = service.ClassifyHardware(metrics);

        Assert.True(classification.Tier == HardwareTier.HighEnd || classification.Tier == HardwareTier.Enthusiast);
        Assert.False(classification.IsIntegratedGpu);
        Assert.True(classification.HardwareScore >= 75);
    }
}
