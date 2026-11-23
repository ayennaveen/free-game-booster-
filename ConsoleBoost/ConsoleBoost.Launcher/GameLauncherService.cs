using System.Diagnostics;
using ConsoleBoost.Models;
using ConsoleBoost.Services;

namespace ConsoleBoost.Launcher;

public class GameLauncherService : IGameLauncherService
{
    private readonly IPowerPlanService _powerPlanService;
    private readonly IGamingModeService _gamingModeService;
    private readonly ILoggingService _logger;

    private Guid? _previousPowerPlanGuid;
    private GameInfo? _currentRunningGame;

    public event Action<string>? GameLaunchStarted;
    public event Action<string>? GameExited;

    public bool IsGameRunning => _currentRunningGame != null;
    public GameInfo? CurrentRunningGame => _currentRunningGame;

    public GameLauncherService(
        IPowerPlanService powerPlanService,
        IGamingModeService gamingModeService,
        ILoggingService logger)
    {
        _powerPlanService = powerPlanService;
        _gamingModeService = gamingModeService;
        _logger = logger;
    }

    public async Task<bool> LaunchGameAsync(GameInfo game)
    {
        if (string.IsNullOrWhiteSpace(game.ExecutablePath) || !File.Exists(game.ExecutablePath))
        {
            _logger.LogError($"Cannot launch game. Executable path not found: {game.ExecutablePath}");
            return false;
        }

        return await LaunchExecutableInternalAsync(game.ExecutablePath, game.LaunchArguments, game.Name, game);
    }

    public async Task<bool> LaunchExecutableAsync(string exePath, string? arguments = null)
    {
        if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
        {
            _logger.LogError($"Executable path does not exist: {exePath}");
            return false;
        }

        string gameName = Path.GetFileNameWithoutExtension(exePath);
        var gameInfo = new GameInfo
        {
            Name = gameName,
            ExecutablePath = exePath,
            InstallLocation = Path.GetDirectoryName(exePath) ?? string.Empty,
            IsCustomExecutable = true,
            LauncherPlatform = "Custom Exe"
        };

        return await LaunchExecutableInternalAsync(exePath, arguments, gameName, gameInfo);
    }

    private async Task<bool> LaunchExecutableInternalAsync(string exePath, string? arguments, string gameName, GameInfo gameInfo)
    {
        try
        {
            _logger.LogGameLaunch(gameName, exePath);

            // Step 1: Save current Windows Power Plan
            var currentPlan = _powerPlanService.GetActivePowerPlan();
            _previousPowerPlanGuid = currentPlan.SchemeGuid;

            // Step 2: Switch to High Performance power plan
            _powerPlanService.SetHighPerformance();

            // Step 3: Enable Gaming Mode
            await _gamingModeService.EnableGamingModeAsync();

            // Step 4: Quick working-set memory trim
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // Step 5: Launch Process
            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments ?? string.Empty,
                WorkingDirectory = Path.GetDirectoryName(exePath) ?? string.Empty,
                UseShellExecute = true
            };

            var process = Process.Start(startInfo);
            if (process == null)
            {
                _logger.LogError($"Failed to start process for: {exePath}");
                RestoreSettings();
                return false;
            }

            _currentRunningGame = gameInfo;
            gameInfo.LastPlayed = DateTime.Now;
            GameLaunchStarted?.Invoke(gameName);

            // Step 6: Monitor Process Exit
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) =>
            {
                OnGameProcessExited(gameName, process);
            };

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception launching executable: {exePath}", ex);
            RestoreSettings();
            return false;
        }
    }

    private void OnGameProcessExited(string gameName, Process process)
    {
        try
        {
            _logger.LogInfo($"Game process exited: {gameName} (PID: {process.Id})");
            RestoreSettings();
            _currentRunningGame = null;
            GameExited?.Invoke(gameName);
            process.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during post-game exit cleanup for {gameName}", ex);
        }
    }

    private void RestoreSettings()
    {
        if (_previousPowerPlanGuid.HasValue)
        {
            _powerPlanService.RestorePowerPlan(_previousPowerPlanGuid.Value);
            _logger.LogInfo($"Power plan restored to: {_previousPowerPlanGuid.Value}");
            _previousPowerPlanGuid = null;
        }
    }
}
