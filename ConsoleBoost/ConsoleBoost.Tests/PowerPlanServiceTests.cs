using ConsoleBoost.Services;
using ConsoleBoost.Models;
using Xunit;

namespace ConsoleBoost.Tests;

public class PowerPlanServiceTests
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
    public void GetActivePowerPlan_ReturnsNonNullPlan()
    {
        var service = new PowerPlanService(new DummyLogger());
        var plan = service.GetActivePowerPlan();

        Assert.NotNull(plan);
        Assert.False(string.IsNullOrWhiteSpace(plan.Name));
    }

    [Fact]
    public void GetAvailablePowerPlans_ReturnsStandardPlans()
    {
        var service = new PowerPlanService(new DummyLogger());
        var plans = service.GetAvailablePowerPlans();

        Assert.NotEmpty(plans);
    }
}
