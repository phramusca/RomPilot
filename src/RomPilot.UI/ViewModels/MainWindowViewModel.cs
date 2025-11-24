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
    private LibraryViewModel? _libraryViewModel;

    public MainWindowViewModel()
    {
        // Initialize ViewModels once and keep references
        _scanViewModel = App.Services?.GetService<ScanViewModel>();
        _libraryViewModel = App.Services?.GetService<LibraryViewModel>();
        
        // Start with scan view
        NavigateToScan();
    }

    /// <summary>
    /// Gets the LibraryViewModel instance (for future functionality).
    /// </summary>
    public LibraryViewModel? GetLibraryViewModel()
    {
        if (_libraryViewModel == null)
        {
            _libraryViewModel = App.Services?.GetService<LibraryViewModel>();
        }
        return _libraryViewModel;
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
    private void NavigateToLibrary()
    {
        // Reuse existing instance or create new one if null
        if (_libraryViewModel == null)
        {
            _libraryViewModel = App.Services?.GetService<LibraryViewModel>();
        }
        CurrentViewModel = _libraryViewModel;
    }
}
