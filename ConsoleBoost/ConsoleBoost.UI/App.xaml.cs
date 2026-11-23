using System.Windows;
using ConsoleBoost.Core;
using ConsoleBoost.Hardware;
using ConsoleBoost.Launcher;
using ConsoleBoost.Services;
using ConsoleBoost.UI.Services;
using ConsoleBoost.UI.ViewModels;
using ConsoleBoost.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleBoost.UI;

public partial class App : Application
{
    private readonly IServiceProvider _serviceProvider;

    public App()
    {
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // Core Services
        services.AddSingleton<ILoggingService, LoggingService>();
        services.AddSingleton<IHardwareService, HardwareService>();
        services.AddSingleton<IPowerPlanService, PowerPlanService>();
        services.AddSingleton<IGamingModeService, GamingModeService>();

        // V2 Services
        services.AddSingleton<IAiAdvisorService, AiAdvisorService>();

        // V3 Services
        services.AddSingleton<IHardwareClassificationService, HardwareClassificationService>();

        // V4 Launcher & Smart Library Services
        services.AddSingleton<IGameDiscoveryService, GameDiscoveryService>();
        services.AddSingleton<IGameLibraryManagerService, GameLibraryManagerService>();
        services.AddSingleton<IGameLauncherService, GameLauncherService>();

        // Navigation Service factory
        services.AddSingleton<INavigationService>(provider =>
            new NavigationService(type => (ViewModelBase)provider.GetRequiredService(type)));

        // View Models
        services.AddSingleton<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<AdvisorViewModel>();
        services.AddTransient<ClassificationViewModel>();
        services.AddTransient<GameLibraryViewModel>();
        services.AddTransient<PerformanceViewModel>();
        services.AddTransient<BoostViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<AboutViewModel>();

        // Windows
        services.AddSingleton<MainWindow>(provider => new MainWindow
        {
            DataContext = provider.GetRequiredService<MainViewModel>()
        });
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var logger = _serviceProvider.GetRequiredService<ILoggingService>();
        logger.LogInfo("ConsoleBoost V4 Application initialized successfully.");

        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        var hardwareService = _serviceProvider.GetService<IHardwareService>();
        hardwareService?.Dispose();

        base.OnExit(e);
    }
}
