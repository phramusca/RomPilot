using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace RomPilot.UI.ViewModels;

/// <summary>
/// Main window view model with navigation support.
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentViewModel;

    [ObservableProperty]
    private string _title = "RomPilot - ROM Manager";

    public MainWindowViewModel()
    {
        // Start with scan view
        NavigateToScan();
    }

    [RelayCommand]
    private void NavigateToScan()
    {
        CurrentViewModel = App.Services?.GetService<ScanViewModel>();
    }

    [RelayCommand]
    private void NavigateToResults()
    {
        CurrentViewModel = App.Services?.GetService<ScanResultsViewModel>();
    }
}
