using System.Collections.ObjectModel;
using System.Windows.Threading;
using ConsoleBoost.Core;
using ConsoleBoost.Hardware;
using ConsoleBoost.Models;
using ConsoleBoost.Services;

namespace ConsoleBoost.UI.ViewModels;

public class AdvisorViewModel : ViewModelBase
{
    private readonly IAiAdvisorService _advisorService;
    private readonly IHardwareService _hardwareService;
    private readonly IPowerPlanService _powerPlanService;
    private readonly ILoggingService _logger;
    private readonly Dispatcher _dispatcher;

    private string _statusMessage = "AI Advisor Engine Monitoring System Telemetry...";
    private int _criticalCount;
    private int _warningCount;
    private int _infoCount;

    public ObservableCollection<PerformanceRecommendation> Recommendations { get; } = new();

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public int CriticalCount
    {
        get => _criticalCount;
        set => SetProperty(ref _criticalCount, value);
    }

    public int WarningCount
    {
        get => _warningCount;
        set => SetProperty(ref _warningCount, value);
    }

    public int InfoCount
    {
        get => _infoCount;
        set => SetProperty(ref _infoCount, value);
    }

    public RelayCommand RefreshCommand { get; }
    public RelayCommand ApplyRecommendationCommand { get; }

    public AdvisorViewModel(
        IAiAdvisorService advisorService,
        IHardwareService hardwareService,
        IPowerPlanService powerPlanService,
        ILoggingService logger)
    {
        _advisorService = advisorService;
        _hardwareService = hardwareService;
        _powerPlanService = powerPlanService;
        _logger = logger;
        _dispatcher = Dispatcher.CurrentDispatcher;

        RefreshCommand = new RelayCommand(OnRefresh);
        ApplyRecommendationCommand = new RelayCommand(param => OnApplyRecommendation(param as PerformanceRecommendation));

        _hardwareService.MetricsUpdated += OnMetricsUpdated;
        _hardwareService.StartMonitoring();

        // Initial evaluation
        EvaluateRecommendations(_hardwareService.CurrentMetrics);
    }

    private void OnMetricsUpdated(HardwareMetrics metrics)
    {
        _dispatcher.InvokeAsync(() =>
        {
            EvaluateRecommendations(metrics);
        });
    }

    private void EvaluateRecommendations(HardwareMetrics metrics)
    {
        var powerPlan = _powerPlanService.GetActivePowerPlan();
        var results = _advisorService.AnalyzeTelemetry(metrics, powerPlan).ToList();

        Recommendations.Clear();
        int crit = 0, warn = 0, info = 0;

        foreach (var rec in results)
        {
            Recommendations.Add(rec);
            switch (rec.Severity)
            {
                case RecommendationSeverity.Critical: crit++; break;
                case RecommendationSeverity.Warning: warn++; break;
                case RecommendationSeverity.Info: info++; break;
            }
        }

        CriticalCount = crit;
        WarningCount = warn;
        InfoCount = info;

        StatusMessage = $"Analysis Updated - {results.Count} Recommendation(s) Active";
    }

    private void OnRefresh()
    {
        EvaluateRecommendations(_hardwareService.CurrentMetrics);
        _logger.LogInfo("AI Advisor recommendations refreshed manually.");
    }

    private void OnApplyRecommendation(PerformanceRecommendation? recommendation)
    {
        if (recommendation == null) return;

        if (recommendation.Category == RecommendationCategory.PowerPlan)
        {
            _powerPlanService.SetHighPerformance();
            StatusMessage = "Applied Action: Switched to High Performance Scheme";
        }
        else if (recommendation.Category == RecommendationCategory.Ram)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            StatusMessage = "Applied Action: Executed Working-Set RAM Trim";
        }
        else
        {
            StatusMessage = $"Suggested Action Noted: {recommendation.SuggestedAction}";
        }

        _logger.LogInfo($"Applied AI recommendation action for: {recommendation.Problem}");
        EvaluateRecommendations(_hardwareService.CurrentMetrics);
    }
}
