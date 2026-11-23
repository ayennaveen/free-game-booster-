using ConsoleBoost.Models;

namespace ConsoleBoost.Launcher;

/// <summary>
/// Service interface for launching games and custom executables with automated boost optimizations.
/// </summary>
public interface IGameLauncherService
{
    event Action<string>? GameLaunchStarted;
    event Action<string>? GameExited;
    bool IsGameRunning { get; }
    GameInfo? CurrentRunningGame { get; }

    Task<bool> LaunchGameAsync(GameInfo game);
    Task<bool> LaunchExecutableAsync(string exePath, string? arguments = null);
}
