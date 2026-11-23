namespace ConsoleBoost.Models;

/// <summary>
/// Model representing a game or custom executable detected or added to the library.
/// </summary>
public class GameInfo
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string InstallLocation { get; set; } = string.Empty;
    public string LauncherPlatform { get; set; } = "Custom"; // Steam, Epic, Xbox, GOG, Ubisoft, EA, Custom
    public string ImagePath { get; set; } = string.Empty;
    public string BannerPath { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public string LaunchArguments { get; set; } = string.Empty;
    public string OptimizationProfileId { get; set; } = "Default";
    public string Category { get; set; } = "Action";
    public bool IsFavourite { get; set; }
    public bool IsHidden { get; set; }
    public bool IsCustomExecutable { get; set; }
    public DateTime? LastPlayed { get; set; }
    public int PlayTimeMinutes { get; set; }
    public int LaunchCount { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.Now;
}
