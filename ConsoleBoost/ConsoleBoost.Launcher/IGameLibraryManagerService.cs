using System.Collections.ObjectModel;
using ConsoleBoost.Models;

namespace ConsoleBoost.Launcher;

/// <summary>
/// Service interface for managing the persistent local game library.
/// </summary>
public interface IGameLibraryManagerService
{
    ObservableCollection<GameInfo> Games { get; }
    event Action? LibraryUpdated;

    Task LoadLibraryAsync();
    Task SaveLibraryAsync();
    void AddGame(GameInfo game);
    void RemoveGame(string gameId);
    void ToggleFavourite(string gameId);
    void UpdateGame(GameInfo updatedGame);
}
