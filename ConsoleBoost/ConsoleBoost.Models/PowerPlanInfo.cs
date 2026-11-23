namespace ConsoleBoost.Models;

/// <summary>
/// Represents a Windows Power Scheme plan.
/// </summary>
public class PowerPlanInfo
{
    public Guid SchemeGuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public PowerPlanType Type { get; set; }
}

public enum PowerPlanType
{
    Balanced,
    HighPerformance,
    UltimatePerformance,
    PowerSaver,
    Custom
}
