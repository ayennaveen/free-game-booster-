using ConsoleBoost.Core;
using ConsoleBoost.Hardware;
using ConsoleBoost.Models;
using ConsoleBoost.Services;

namespace ConsoleBoost.UI.ViewModels;

public class ClassificationViewModel : ViewModelBase
{
    private readonly IHardwareClassificationService _classificationService;
    private readonly IHardwareService _hardwareService;
    private readonly ILoggingService _logger;

    private HardwareClassification _classification = new();
    private string _statusMessage = "Hardware Classification Active";

    public HardwareClassification Classification
    {
        get => _classification;
        set => SetProperty(ref _classification, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public RelayCommand ReevaluateCommand { get; }

    public ClassificationViewModel(
        IHardwareClassificationService classificationService,
        IHardwareService hardwareService,
        ILoggingService logger)
    {
        _classificationService = classificationService;
        _hardwareService = hardwareService;
        _logger = logger;

        ReevaluateCommand = new RelayCommand(OnReevaluate);

        EvaluateTier();
    }

    private void EvaluateTier()
    {
        var metrics = _hardwareService.CurrentMetrics;
        Classification = _classificationService.ClassifyHardware(metrics);
        StatusMessage = $"Detected Tier: {Classification.TierName} (Score: {Classification.HardwareScore}/100)";
    }

    private void OnReevaluate()
    {
        EvaluateTier();
        _logger.LogInfo("Hardware classification re-evaluated manually.");
    }
}
