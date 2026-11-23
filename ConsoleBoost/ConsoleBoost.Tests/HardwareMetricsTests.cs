using ConsoleBoost.Models;
using Xunit;

namespace ConsoleBoost.Tests;

public class HardwareMetricsTests
{
    [Fact]
    public void RamUsagePercent_CalculatesCorrectly()
    {
        var metrics = new HardwareMetrics
        {
            RamUsedGb = 8.0,
            RamTotalGb = 16.0
        };

        Assert.Equal(50.0, metrics.RamUsagePercent);
    }

    [Fact]
    public void RamUsagePercent_ReturnsZeroWhenTotalIsZero()
    {
        var metrics = new HardwareMetrics
        {
            RamUsedGb = 0,
            RamTotalGb = 0
        };

        Assert.Equal(0.0, metrics.RamUsagePercent);
    }
}
