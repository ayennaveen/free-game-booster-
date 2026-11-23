namespace ConsoleBoost.Core;

/// <summary>
/// Navigation service interface for switching active view models.
/// </summary>
public interface INavigationService
{
    ViewModelBase? CurrentView { get; }
    event Action<ViewModelBase>? CurrentViewChanged;

    void NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
    void NavigateTo(Type viewModelType);
}
