using ConsoleBoost.Core;

namespace ConsoleBoost.UI.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private ViewModelBase? _currentView;
    private string _activeTab = "Dashboard";

    public ViewModelBase? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public string ActiveTab
    {
        get => _activeTab;
        set => SetProperty(ref _activeTab, value);
    }

    public RelayCommand NavigateDashboardCommand { get; }
    public RelayCommand NavigateAdvisorCommand { get; }
    public RelayCommand NavigateClassificationCommand { get; }
    public RelayCommand NavigateLibraryCommand { get; }
    public RelayCommand NavigatePerformanceCommand { get; }
    public RelayCommand NavigateBoostCommand { get; }
    public RelayCommand NavigateSettingsCommand { get; }
    public RelayCommand NavigateAboutCommand { get; }

    public MainViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        _navigationService.CurrentViewChanged += OnCurrentViewChanged;

        NavigateDashboardCommand = new RelayCommand(() => NavigateTo<DashboardViewModel>("Dashboard"));
        NavigateAdvisorCommand = new RelayCommand(() => NavigateTo<AdvisorViewModel>("AI Advisor"));
        NavigateClassificationCommand = new RelayCommand(() => NavigateTo<ClassificationViewModel>("Hardware Tier"));
        NavigateLibraryCommand = new RelayCommand(() => NavigateTo<GameLibraryViewModel>("Library"));
        NavigatePerformanceCommand = new RelayCommand(() => NavigateTo<PerformanceViewModel>("Performance"));
        NavigateBoostCommand = new RelayCommand(() => NavigateTo<BoostViewModel>("Boost"));
        NavigateSettingsCommand = new RelayCommand(() => NavigateTo<SettingsViewModel>("Settings"));
        NavigateAboutCommand = new RelayCommand(() => NavigateTo<AboutViewModel>("About"));

        // Default start page
        NavigateTo<DashboardViewModel>("Dashboard");
    }

    private void NavigateTo<TViewModel>(string tabName) where TViewModel : ViewModelBase
    {
        ActiveTab = tabName;
        _navigationService.NavigateTo<TViewModel>();
    }

    private void OnCurrentViewChanged(ViewModelBase newView)
    {
        CurrentView = newView;
    }
}
