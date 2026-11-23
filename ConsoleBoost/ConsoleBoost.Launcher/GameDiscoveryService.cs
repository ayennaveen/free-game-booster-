using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Win32;
using ConsoleBoost.Models;
using ConsoleBoost.Services;

namespace ConsoleBoost.Launcher;

public class GameDiscoveryService : IGameDiscoveryService
{
    private readonly ILoggingService _logger;
    private readonly string _iconsDirectory;

    public event Action<GameInfo>? NewGameDetected;

    public GameDiscoveryService(ILoggingService logger)
    {
        _logger = logger;
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _iconsDirectory = Path.Combine(appData, "ConsoleBoost", "Icons");
        Directory.CreateDirectory(_iconsDirectory);
    }

    public Task<IEnumerable<GameInfo>> ScanInstalledGamesAsync()
    {
        return Task.Run(() =>
        {
            var discoveredGames = new List<GameInfo>();

            try
            {
                ScanSteamGames(discoveredGames);
                ScanEpicGames(discoveredGames);
                ScanGogGames(discoveredGames);
                ScanUbisoftGames(discoveredGames);
                ScanEaGames(discoveredGames);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error during launcher game discovery scan", ex);
            }

            _logger.LogInfo($"Game discovery completed. Total launcher games found: {discoveredGames.Count}");
            return (IEnumerable<GameInfo>)discoveredGames;
        });
    }

    public Task<GameInfo?> DetectGameFromExecutableAsync(string exePath)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(exePath) || !File.Exists(exePath))
                return null;

            string title = Path.GetFileNameWithoutExtension(exePath);
            string iconPath = ExtractExecutableIcon(exePath);

            var game = new GameInfo
            {
                Name = title,
                ExecutablePath = exePath,
                InstallLocation = Path.GetDirectoryName(exePath) ?? string.Empty,
                LauncherPlatform = "Custom Exe",
                IconPath = iconPath,
                IsCustomExecutable = true,
                Category = "Custom"
            };

            NewGameDetected?.Invoke(game);
            return game;
        });
    }

    public string ExtractExecutableIcon(string exePath)
    {
        try
        {
            if (!File.Exists(exePath)) return string.Empty;

            string fileName = Path.GetFileNameWithoutExtension(exePath) + ".png";
            string targetPath = Path.Combine(_iconsDirectory, fileName);

            if (File.Exists(targetPath)) return targetPath;

            using Icon? sysIcon = Icon.ExtractAssociatedIcon(exePath);
            if (sysIcon != null)
            {
                using Bitmap bmp = sysIcon.ToBitmap();
                bmp.Save(targetPath, ImageFormat.Png);
                return targetPath;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Could not extract icon from {exePath}: {ex.Message}");
        }
        return string.Empty;
    }

    private void ScanSteamGames(List<GameInfo> games)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
            string? steamPath = key?.GetValue("SteamPath")?.ToString();
            if (string.IsNullOrWhiteSpace(steamPath) || !Directory.Exists(steamPath)) return;

            string appsPath = Path.Combine(steamPath, "steamapps");
            if (!Directory.Exists(appsPath)) return;

            foreach (var manifestFile in Directory.GetFiles(appsPath, "appmanifest_*.acf"))
            {
                string content = File.ReadAllText(manifestFile);
                string name = ParseAcfValue(content, "name");
                string installDir = ParseAcfValue(content, "installdir");
                string appId = ParseAcfValue(content, "appid");

                if (!string.IsNullOrWhiteSpace(name) && name != "Steamworks Common Redistributables")
                {
                    string fullInstallPath = Path.Combine(appsPath, "common", installDir);
                    string exePath = FindPrimaryExeInDirectory(fullInstallPath);

                    games.Add(new GameInfo
                    {
                        Name = name,
                        InstallLocation = fullInstallPath,
                        ExecutablePath = exePath,
                        LauncherPlatform = "Steam",
                        IconPath = ExtractExecutableIcon(exePath),
                        Category = "Steam"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Steam discovery scan encountered issue: {ex.Message}");
        }
    }

    private void ScanEpicGames(List<GameInfo> games)
    {
        try
        {
            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            string epicManifestsDir = Path.Combine(programData, "Epic", "EpicGamesLauncher", "Data", "Manifests");

            if (!Directory.Exists(epicManifestsDir)) return;

            foreach (var itemFile in Directory.GetFiles(epicManifestsDir, "*.item"))
            {
                string content = File.ReadAllText(itemFile);
                string title = ParseJsonString(content, "DisplayName");
                string installLocation = ParseJsonString(content, "InstallLocation");
                string mainExe = ParseJsonString(content, "LaunchExecutable");

                if (!string.IsNullOrWhiteSpace(title) && !string.IsNullOrWhiteSpace(installLocation))
                {
                    string exePath = Path.Combine(installLocation, mainExe);

                    games.Add(new GameInfo
                    {
                        Name = title,
                        InstallLocation = installLocation,
                        ExecutablePath = File.Exists(exePath) ? exePath : FindPrimaryExeInDirectory(installLocation),
                        LauncherPlatform = "Epic Games",
                        IconPath = ExtractExecutableIcon(exePath),
                        Category = "Epic"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"Epic discovery scan encountered issue: {ex.Message}");
        }
    }

    private void ScanGogGames(List<GameInfo> games)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\GOG.com\Games");
            if (key == null) return;

            foreach (string subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                string? name = subKey?.GetValue("gameName")?.ToString();
                string? path = subKey?.GetValue("path")?.ToString();
                string? exe = subKey?.GetValue("exe")?.ToString();

                if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    string exePath = !string.IsNullOrWhiteSpace(exe) ? Path.Combine(path, exe) : FindPrimaryExeInDirectory(path);

                    games.Add(new GameInfo
                    {
                        Name = name,
                        InstallLocation = path,
                        ExecutablePath = exePath,
                        LauncherPlatform = "GOG Galaxy",
                        IconPath = ExtractExecutableIcon(exePath),
                        Category = "GOG"
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"GOG discovery scan encountered issue: {ex.Message}");
        }
    }

    private void ScanUbisoftGames(List<GameInfo> games)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Ubisoft\Launcher\Installs");
            if (key == null) return;

            foreach (string subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                string? path = subKey?.GetValue("InstallDir")?.ToString();
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    string name = Path.GetFileName(path.TrimEnd('\\', '/'));
                    string exePath = FindPrimaryExeInDirectory(path);

                    games.Add(new GameInfo
                    {
                        Name = name,
                        InstallLocation = path,
                        ExecutablePath = exePath,
                        LauncherPlatform = "Ubisoft Connect",
                        IconPath = ExtractExecutableIcon(exePath),
                        Category = "Ubisoft"
                    });
                }
            }
        }
        catch { }
    }

    private void ScanEaGames(List<GameInfo> games)
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\EA Games");
            if (key == null) return;

            foreach (string subKeyName in key.GetSubKeyNames())
            {
                using var subKey = key.OpenSubKey(subKeyName);
                string? path = subKey?.GetValue("Install Dir")?.ToString();
                if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    string name = subKeyName;
                    string exePath = FindPrimaryExeInDirectory(path);

                    games.Add(new GameInfo
                    {
                        Name = name,
                        InstallLocation = path,
                        ExecutablePath = exePath,
                        LauncherPlatform = "EA App",
                        IconPath = ExtractExecutableIcon(exePath),
                        Category = "EA"
                    });
                }
            }
        }
        catch { }
    }

    private string FindPrimaryExeInDirectory(string dirPath)
    {
        if (!Directory.Exists(dirPath)) return string.Empty;
        try
        {
            var files = Directory.GetFiles(dirPath, "*.exe", SearchOption.TopDirectoryOnly);
            foreach (var f in files)
            {
                string fn = Path.GetFileName(f).ToLowerInvariant();
                if (!fn.Contains("unins") && !fn.Contains("crash") && !fn.Contains("setup") && !fn.Contains("update"))
                    return f;
            }
            if (files.Length > 0) return files[0];
        }
        catch { }
        return string.Empty;
    }

    private string ParseAcfValue(string content, string keyName)
    {
        string search = $"\"{keyName}\"";
        int idx = content.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        if (idx == -1) return string.Empty;

        int firstQuote = content.IndexOf('"', idx + search.Length);
        if (firstQuote == -1) return string.Empty;

        int secondQuote = content.IndexOf('"', firstQuote + 1);
        if (secondQuote == -1) return string.Empty;

        return content.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
    }

    private string ParseJsonString(string content, string keyName)
    {
        string search = $"\"{keyName}\"";
        int idx = content.IndexOf(search, StringComparison.OrdinalIgnoreCase);
        if (idx == -1) return string.Empty;

        int colon = content.IndexOf(':', idx);
        if (colon == -1) return string.Empty;

        int firstQuote = content.IndexOf('"', colon);
        if (firstQuote == -1) return string.Empty;

        int secondQuote = content.IndexOf('"', firstQuote + 1);
        if (secondQuote == -1) return string.Empty;

        return content.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
    }
}
