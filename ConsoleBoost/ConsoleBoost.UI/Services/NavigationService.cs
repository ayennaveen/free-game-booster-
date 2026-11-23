using ConsoleBoost.Core;

namespace ConsoleBoost.UI.Services;

public class NavigationService : INavigationService
{
    private readonly Func<Type, ViewModelBase> _viewModelFactory;
    private ViewModelBase? _currentView;

    public ViewModelBase? CurrentView => _currentView;
    public event Action<ViewModelBase>? CurrentViewChanged;

    public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
    {
        NavigateTo(typeof(TViewModel));
    }

    public void NavigateTo(Type viewModelType)
    {
        var viewModel = _viewModelFactory(viewModelType);
        _currentView = viewModel;
        CurrentViewChanged?.Invoke(viewModel);
    }
}
