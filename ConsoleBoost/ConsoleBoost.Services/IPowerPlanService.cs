using ConsoleBoost.Models;

namespace ConsoleBoost.Services;

/// <summary>
/// Service interface for querying and switching Windows Power Plans.
/// </summary>
public interface IPowerPlanService
{
    PowerPlanInfo GetActivePowerPlan();
    IEnumerable<PowerPlanInfo> GetAvailablePowerPlans();
    bool SetActivePowerPlan(Guid schemeGuid);
    bool SetHighPerformance();
    bool RestorePowerPlan(Guid schemeGuid);
}
