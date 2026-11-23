namespace ConsoleBoost.Services;

/// <summary>
/// Service interface for application logging storing logs under Documents\ConsoleBoost\Logs.
/// </summary>
public interface ILoggingService
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception? exception = null);
    void LogOptimization(string details);
    void LogGameLaunch(string gameName, string path);
}
