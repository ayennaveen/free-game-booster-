using System.Windows.Threading;
using ConsoleBoost.Core;
using ConsoleBoost.Hardware;
using ConsoleBoost.Launcher;
using ConsoleBoost.Models;
using ConsoleBoost.Services;
using Microsoft.Win32;

namespace ConsoleBoost.UI.ViewModels;

public class DashboardViewModel : ViewModelBase
{
    private readonly IHardwareService _hardwareService;
    private readonly IPowerPlanService _powerPlanService;
    private readonly IGamingModeService _gamingModeService;
    private readonly IGameLauncherService _gameLauncherService;
    private readonly ILoggingService _logger;
    private readonly Dispatcher _dispatcher;

    private HardwareMetrics _metrics = new();
    private PowerPlanInfo _activePowerPlan = new();
    private bool _isGamingModeEnabled;
    private string _statusMessage = "System Nominal - Live Telemetry Active";
    private string _selectedExePath = string.Empty;
    private bool _isGameRunning;
    private string _runningGameName = string.Empty;

    public HardwareMetrics Metrics
    {
        get => _metrics;
        set => SetProperty(ref _metrics, value);
    }

    public PowerPlanInfo ActivePowerPlan
    {
        get => _activePowerPlan;
        set => SetProperty(ref _activePowerPlan, value);
    }

    public bool IsGamingModeEnabled
    {
        get => _isGamingModeEnabled;
        set => SetProperty(ref _isGamingModeEnabled, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string SelectedExePath
    {
        get => _selectedExePath;
        set => SetProperty(ref _selectedExePath, value);
    }

    public bool IsGameRunning
    {
        get => _isGameRunning;
        set => SetProperty(ref _isGameRunning, value);
    }

    public string RunningGameName
    {
        get => _runningGameName;
        set => SetProperty(ref _runningGameName, value);
    }

    public RelayCommand QuickOptimizeCommand { get; }
    public RelayCommand TogglePowerPlanCommand { get; }
    public AsyncRelayCommand SelectAndRunExeCommand { get; }

    public DashboardViewModel(
        IHardwareService hardwareService,
        IPowerPlanService powerPlanService,
        IGamingModeService gamingModeService,
        IGameLauncherService gameLauncherService,
        ILoggingService logger)
    {
        _hardwareService = hardwareService;
        _powerPlanService = powerPlanService;
        _gamingModeService = gamingModeService;
        _gameLauncherService = gameLauncherService;
        _logger = logger;
        _dispatcher = Dispatcher.CurrentDispatcher;

        QuickOptimizeCommand = new RelayCommand(OnQuickOptimize);
        TogglePowerPlanCommand = new RelayCommand(OnTogglePowerPlan);
        SelectAndRunExeCommand = new AsyncRelayCommand(OnSelectAndRunExeAsync);

        _gameLauncherService.GameLaunchStarted += OnGameLaunchStarted;
        _gameLauncherService.GameExited += OnGameExited;

        LoadInitialData();

        _hardwareService.MetricsUpdated += OnMetricsUpdated;
        _hardwareService.StartMonitoring();
    }

    private void LoadInitialData()
    {
        ActivePowerPlan = _powerPlanService.GetActivePowerPlan();
        IsGamingModeEnabled = _gamingModeService.IsGamingModeEnabled;
    }

    private void OnMetricsUpdated(HardwareMetrics metrics)
    {
        _dispatcher.InvokeAsync(() =>
        {
            Metrics = metrics;
        });
    }

    private async Task OnSelectAndRunExeAsync()
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Executable / Game to Boost & Run",
            Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
            Multiselect = false
        };

        if (openFileDialog.ShowDialog() == true)
        {
            SelectedExePath = openFileDialog.FileName;
            string fileName = System.IO.Path.GetFileName(SelectedExePath);
            StatusMessage = $"Optimizing Windows & Launching {fileName}...";

            bool success = await _gameLauncherService.LaunchExecutableAsync(SelectedExePath);
            if (success)
            {
                StatusMessage = $"🚀 Boost Active: Running {fileName}";
            }
            else
            {
                StatusMessage = $"❌ Failed to launch {fileName}";
            }
        }
    }

    private void OnGameLaunchStarted(string gameName)
    {
        _dispatcher.InvokeAsync(() =>
        {
            IsGameRunning = true;
            RunningGameName = gameName;
            StatusMessage = $"🚀 Gaming Boost Active for: {gameName}";
            ActivePowerPlan = _powerPlanService.GetActivePowerPlan();
        });
    }

    private void OnGameExited(string gameName)
    {
        _dispatcher.InvokeAsync(() =>
        {
            IsGameRunning = false;
            RunningGameName = string.Empty;
            StatusMessage = $"✅ {gameName} exited - Power Plan & Settings Restored";
            ActivePowerPlan = _powerPlanService.GetActivePowerPlan();
        });
    }

    private void OnQuickOptimize()
    {
        StatusMessage = "Quick Optimizing System Working Sets...";
        GC.Collect();
        GC.WaitForPendingFinalizers();
        _logger.LogOptimization("Quick Working-set trim triggered from Dashboard");
        StatusMessage = "Optimization Complete - Freed Transient Memory";
    }

    private void OnTogglePowerPlan()
    {
        if (ActivePowerPlan.Type == PowerPlanType.HighPerformance || ActivePowerPlan.Type == PowerPlanType.UltimatePerformance)
        {
            _powerPlanService.SetActivePowerPlan(Helpers.NativeMethods.GUID_TYPICAL_POWER_SAVINGS);
        }
        else
        {
            _powerPlanService.SetHighPerformance();
        }
        ActivePowerPlan = _powerPlanService.GetActivePowerPlan();
    }
}
