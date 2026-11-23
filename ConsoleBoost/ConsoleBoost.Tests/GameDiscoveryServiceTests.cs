using ConsoleBoost.Launcher;
using ConsoleBoost.Models;
using ConsoleBoost.Services;
using Xunit;

namespace ConsoleBoost.Tests;

public class GameDiscoveryServiceTests
{
    private class DummyLogger : ILoggingService
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception? exception = null) { }
        public void LogOptimization(string details) { }
        public void LogGameLaunch(string gameName, string path) { }
    }

    [Fact]
    public async Task ScanInstalledGamesAsync_ReturnsNonNullList()
    {
        var service = new GameDiscoveryService(new DummyLogger());
        var games = await service.ScanInstalledGamesAsync();

        Assert.NotNull(games);
    }

    [Fact]
    public void AddGameAndToggleFavourite_UpdatesLibraryState()
    {
        var discovery = new GameDiscoveryService(new DummyLogger());
        var manager = new GameLibraryManagerService(discovery, new DummyLogger());

        var game = new GameInfo
        {
            Name = "Test Cyberpunk Game",
            ExecutablePath = @"C:\Games\Cyberpunk\cyberpunk.exe",
            Category = "Action",
            LauncherPlatform = "Steam"
        };

        manager.AddGame(game);
        Assert.Contains(manager.Games, g => g.Name == "Test Cyberpunk Game");

        manager.ToggleFavourite(game.Id);
        Assert.True(game.IsFavourite);

        manager.RemoveGame(game.Id);
        Assert.DoesNotContain(manager.Games, g => g.Id == game.Id);
    }
}
