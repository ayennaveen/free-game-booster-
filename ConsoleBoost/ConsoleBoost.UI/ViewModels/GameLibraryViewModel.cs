using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Threading;
using ConsoleBoost.Core;
using ConsoleBoost.Launcher;
using ConsoleBoost.Models;
using ConsoleBoost.Services;
using Microsoft.Win32;

namespace ConsoleBoost.UI.ViewModels;

public class GameLibraryViewModel : ViewModelBase
{
    private readonly IGameLibraryManagerService _libraryManager;
    private readonly IGameDiscoveryService _discoveryService;
    private readonly IGameLauncherService _launcherService;
    private readonly ILoggingService _logger;
    private readonly Dispatcher _dispatcher;

    private string _searchQuery = string.Empty;
    private string _selectedCategory = "All";
    private GameInfo? _selectedGame;
    private string _statusMessage = "Library Loaded";

    public ObservableCollection<GameInfo> FilteredGames { get; } = new();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
                ApplyFilters();
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetProperty(ref _selectedCategory, value))
                ApplyFilters();
        }
    }

    public GameInfo? SelectedGame
    {
        get => _selectedGame;
        set => SetProperty(ref _selectedGame, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public AsyncRelayCommand ScanGamesCommand { get; }
    public AsyncRelayCommand AddCustomExeCommand { get; }
    public AsyncRelayCommand LaunchGameCommand { get; }
    public RelayCommand ToggleFavouriteCommand { get; }
    public RelayCommand RemoveGameCommand { get; }
    public RelayCommand SelectCategoryCommand { get; }

    public GameLibraryViewModel(
        IGameLibraryManagerService libraryManager,
        IGameDiscoveryService discoveryService,
        IGameLauncherService launcherService,
        ILoggingService logger)
    {
        _libraryManager = libraryManager;
        _discoveryService = discoveryService;
        _launcherService = launcherService;
        _logger = logger;
        _dispatcher = Dispatcher.CurrentDispatcher;

        ScanGamesCommand = new AsyncRelayCommand(OnScanGamesAsync);
        AddCustomExeCommand = new AsyncRelayCommand(OnAddCustomExeAsync);
        LaunchGameCommand = new AsyncRelayCommand(OnLaunchGameAsync);
        ToggleFavouriteCommand = new RelayCommand(param => OnToggleFavourite(param as GameInfo));
        RemoveGameCommand = new RelayCommand(param => OnRemoveGame(param as GameInfo));
        SelectCategoryCommand = new RelayCommand(param => SelectedCategory = param?.ToString() ?? "All");

        _libraryManager.LibraryUpdated += OnLibraryUpdated;
        _ = InitializeLibraryAsync();
    }

    private async Task InitializeLibraryAsync()
    {
        await _libraryManager.LoadLibraryAsync();
        ApplyFilters();
    }

    private void OnLibraryUpdated()
    {
        _dispatcher.InvokeAsync(() =>
        {
            ApplyFilters();
            StatusMessage = $"Library updated - {FilteredGames.Count} game(s) available";
        });
    }

    public void ApplyFilters()
    {
        FilteredGames.Clear();
        var query = _libraryManager.Games.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            query = query.Where(g => g.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                                     g.LauncherPlatform.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedCategory == "Favourites")
        {
            query = query.Where(g => g.IsFavourite);
        }
        else if (SelectedCategory != "All")
        {
            query = query.Where(g => g.Category.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase) ||
                                     g.LauncherPlatform.Equals(SelectedCategory, StringComparison.OrdinalIgnoreCase));
        }

        foreach (var game in query)
        {
            FilteredGames.Add(game);
        }

        if (SelectedGame == null && FilteredGames.Count > 0)
        {
            SelectedGame = FilteredGames[0];
        }
    }

    public async Task ImportExecutableFileAsync(string filePath)
    {
        if (File.Exists(filePath) && filePath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
        {
            var game = await _discoveryService.DetectGameFromExecutableAsync(filePath);
            if (game != null)
            {
                _libraryManager.AddGame(game);
                StatusMessage = $"Imported {game.Name} into library.";
            }
        }
    }

    private async Task OnScanGamesAsync()
    {
        StatusMessage = "Scanning Steam, Epic, Xbox, GOG, Ubisoft & EA App launchers...";
        var discovered = await _discoveryService.ScanInstalledGamesAsync();
        int count = 0;
        foreach (var g in discovered)
        {
            _libraryManager.AddGame(g);
            count++;
        }
        StatusMessage = $"Scan Complete - Discovered & Updated {count} games.";
    }

    private async Task OnAddCustomExeAsync()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Game / Executable to Add",
            Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() == true)
        {
            await ImportExecutableFileAsync(openFileDialog.FileName);
        }
    }

    private async Task OnLaunchGameAsync()
    {
        if (SelectedGame == null) return;
        StatusMessage = $"Boosting System & Launching {SelectedGame.Name}...";
        bool success = await _launcherService.LaunchGameAsync(SelectedGame);
        if (!success)
        {
            StatusMessage = $"❌ Failed to launch {SelectedGame.Name}";
        }
    }

    private void OnToggleFavourite(GameInfo? game)
    {
        if (game == null) return;
        _libraryManager.ToggleFavourite(game.Id);
    }

    private void OnRemoveGame(GameInfo? game)
    {
        if (game == null) return;
        _libraryManager.RemoveGame(game.Id);
        StatusMessage = $"Removed {game.Name} from library.";
    }
}
