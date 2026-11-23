using ConsoleBoost.Models;

namespace ConsoleBoost.Launcher;

/// <summary>
/// Interface for discovering installed games across Steam, Epic, Xbox, GOG, Ubisoft, and EA app launchers.
/// </summary>
public interface IGameDiscoveryService
{
    event Action<GameInfo>? NewGameDetected;

    Task<IEnumerable<GameInfo>> ScanInstalledGamesAsync();
    Task<GameInfo?> DetectGameFromExecutableAsync(string exePath);
    string ExtractExecutableIcon(string exePath);
}
