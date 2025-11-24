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

    // Keep references to ViewModels to preserve their state
    private ScanViewModel? _scanViewModel;
    private ScanResultsViewModel? _scanResultsViewModel;

    public MainWindowViewModel()
    {
        // Initialize ViewModels once and keep references
        _scanViewModel = App.Services?.GetService<ScanViewModel>();
        _scanResultsViewModel = App.Services?.GetService<ScanResultsViewModel>();
        
        // Start with scan view
        NavigateToScan();
    }

    /// <summary>
    /// Gets the ScanResultsViewModel instance (for setting ROM files after scan).
    /// </summary>
    public ScanResultsViewModel? GetScanResultsViewModel()
    {
        if (_scanResultsViewModel == null)
        {
            _scanResultsViewModel = App.Services?.GetService<ScanResultsViewModel>();
        }
        return _scanResultsViewModel;
    }

    [RelayCommand]
    private void NavigateToScan()
    {
        // Reuse existing instance or create new one if null
        if (_scanViewModel == null)
        {
            _scanViewModel = App.Services?.GetService<ScanViewModel>();
        }
        CurrentViewModel = _scanViewModel;
    }

    [RelayCommand]
    private void NavigateToResults()
    {
        // Reuse existing instance or create new one if null
        if (_scanResultsViewModel == null)
        {
            _scanResultsViewModel = App.Services?.GetService<ScanResultsViewModel>();
        }
        CurrentViewModel = _scanResultsViewModel;
    }
}
