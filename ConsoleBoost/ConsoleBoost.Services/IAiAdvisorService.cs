using ConsoleBoost.Models;

namespace ConsoleBoost.Services;

/// <summary>
/// Service interface for analyzing hardware telemetry and process footprint to produce AI recommendations.
/// </summary>
public interface IAiAdvisorService
{
    IEnumerable<PerformanceRecommendation> AnalyzeTelemetry(HardwareMetrics metrics, PowerPlanInfo powerPlan);
}
