using Microsoft.Win32;

namespace ConsoleBoost.Services;

public class GamingModeService : IGamingModeService
{
    private readonly ILoggingService _logger;
    private const string GameBarRegistryKey = @"Software\Microsoft\GameBar";
    private const string GameModeValueName = "AllowAutoGameMode";

    public bool IsGamingModeEnabled => CheckGamingModeRegistry();

    public GamingModeService(ILoggingService logger)
    {
        _logger = logger;
    }

    public Task<bool> EnableGamingModeAsync()
    {
        return Task.Run(() =>
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey(GameBarRegistryKey);
                key?.SetValue(GameModeValueName, 1, RegistryValueKind.DWord);
                _logger.LogInfo("Windows Gaming Mode enabled via Registry.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to enable Windows Gaming Mode", ex);
                return false;
            }
        });
    }

    public Task<bool> RestoreGamingModeAsync()
    {
        return EnableGamingModeAsync();
    }

    private bool CheckGamingModeRegistry()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(GameBarRegistryKey);
            if (key != null)
            {
                object? val = key.GetValue(GameModeValueName);
                if (val is int intVal)
                {
                    return intVal == 1;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to read Gaming Mode registry state", ex);
        }
        return true; // Windows 10/11 defaults to Game Mode ON
    }
}
