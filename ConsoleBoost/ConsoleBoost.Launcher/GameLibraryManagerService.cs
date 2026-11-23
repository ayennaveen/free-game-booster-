using System.Collections.ObjectModel;
using System.Text.Json;
using ConsoleBoost.Models;
using ConsoleBoost.Services;

namespace ConsoleBoost.Launcher;

public class GameLibraryManagerService : IGameLibraryManagerService
{
    private readonly IGameDiscoveryService _discoveryService;
    private readonly ILoggingService _logger;
    private readonly string _libraryFilePath;
    private readonly object _lockObj = new();

    public ObservableCollection<GameInfo> Games { get; } = new();
    public event Action? LibraryUpdated;

    public GameLibraryManagerService(IGameDiscoveryService discoveryService, ILoggingService logger)
    {
        _discoveryService = discoveryService;
        _logger = logger;

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string folder = Path.Combine(appData, "ConsoleBoost");
        Directory.CreateDirectory(folder);
        _libraryFilePath = Path.Combine(folder, "library.json");

        _discoveryService.NewGameDetected += OnNewGameDetected;
    }

    public async Task LoadLibraryAsync()
    {
        try
        {
            if (File.Exists(_libraryFilePath))
            {
                string json = await File.ReadAllTextAsync(_libraryFilePath);
                var items = JsonSerializer.Deserialize<List<GameInfo>>(json);
                if (items != null)
                {
                    Games.Clear();
                    foreach (var item in items)
                    {
                        Games.Add(item);
                    }
                    _logger.LogInfo($"Library loaded from file. Total items: {Games.Count}");
                    LibraryUpdated?.Invoke();
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error loading library.json", ex);
        }

        // If library file doesn't exist or failed, run initial discovery scan
        await AutoDiscoverAndPopulateAsync();
    }

    public async Task SaveLibraryAsync()
    {
        try
        {
            List<GameInfo> list;
            lock (_lockObj)
            {
                list = Games.ToList();
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(list, options);
            await File.WriteAllTextAsync(_libraryFilePath, json);
            _logger.LogInfo("Library saved to library.json");
            LibraryUpdated?.Invoke();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error saving library.json", ex);
        }
    }

    public void AddGame(GameInfo game)
    {
        if (Games.Any(g => g.ExecutablePath.Equals(game.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
            return;

        Games.Add(game);
        _logger.LogInfo($"Added game to library: {game.Name}");
        _ = SaveLibraryAsync();
    }

    public void RemoveGame(string gameId)
    {
        var existing = Games.FirstOrDefault(g => g.Id == gameId);
        if (existing != null)
        {
            Games.Remove(existing);
            _logger.LogInfo($"Removed game from library: {existing.Name}");
            _ = SaveLibraryAsync();
        }
    }

    public void ToggleFavourite(string gameId)
    {
        var existing = Games.FirstOrDefault(g => g.Id == gameId);
        if (existing != null)
        {
            existing.IsFavourite = !existing.IsFavourite;
            _logger.LogInfo($"Toggled favourite for: {existing.Name} ({existing.IsFavourite})");
            _ = SaveLibraryAsync();
        }
    }

    public void UpdateGame(GameInfo updatedGame)
    {
        var existing = Games.FirstOrDefault(g => g.Id == updatedGame.Id);
        if (existing != null)
        {
            existing.Name = updatedGame.Name;
            existing.Category = updatedGame.Category;
            existing.LaunchArguments = updatedGame.LaunchArguments;
            existing.ImagePath = updatedGame.ImagePath;
            existing.IsFavourite = updatedGame.IsFavourite;
            existing.IsHidden = updatedGame.IsHidden;
            _logger.LogInfo($"Updated game details for: {existing.Name}");
            _ = SaveLibraryAsync();
        }
    }

    private async Task AutoDiscoverAndPopulateAsync()
    {
        var discovered = await _discoveryService.ScanInstalledGamesAsync();
        foreach (var g in discovered)
        {
            if (!Games.Any(existing => existing.ExecutablePath.Equals(g.ExecutablePath, StringComparison.OrdinalIgnoreCase)))
            {
                Games.Add(g);
            }
        }
        await SaveLibraryAsync();
    }

    private void OnNewGameDetected(GameInfo newGame)
    {
        AddGame(newGame);
    }
}
