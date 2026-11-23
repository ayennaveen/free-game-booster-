namespace ConsoleBoost.Services;

/// <summary>
/// Service for Windows Game Mode state detection and activation.
/// </summary>
public interface IGamingModeService
{
    bool IsGamingModeEnabled { get; }
    Task<bool> EnableGamingModeAsync();
    Task<bool> RestoreGamingModeAsync();
}
