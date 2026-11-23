namespace ConsoleBoost.Models;

public enum RecommendationSeverity
{
    Info,
    Warning,
    Critical
}

public enum RecommendationCategory
{
    Cpu,
    Gpu,
    Ram,
    Vram,
    Thermal,
    Storage,
    Battery,
    PowerPlan,
    BackgroundApp
}

/// <summary>
/// Model representing an AI Performance Advisor recommendation item.
/// </summary>
public class PerformanceRecommendation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Problem { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string EstimatedImpact { get; set; } = "Medium Impact"; // High, Medium, Low
    public string SuggestedAction { get; set; } = string.Empty;
    public string ExpectedImprovement { get; set; } = string.Empty;
    public RecommendationSeverity Severity { get; set; } = RecommendationSeverity.Warning;
    public RecommendationCategory Category { get; set; } = RecommendationCategory.Cpu;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}
