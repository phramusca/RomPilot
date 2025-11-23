using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RomPilot.Core.Models;
using RomPilot.Core.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RomPilot.UI.ViewModels;

/// <summary>
/// View model for the scan view, handles directory selection and scan execution.
/// </summary>
public partial class ScanViewModel : ViewModelBase
{
    private readonly IScanService _scanService;
    private readonly IServiceProvider _serviceProvider;

    [ObservableProperty]
    private string _selectedDirectory = string.Empty;

    [ObservableProperty]
    private bool _isScanning;

    [ObservableProperty]
    private string _statusMessage = "Ready to scan";

    [ObservableProperty]
    private int _progressCurrent;

    [ObservableProperty]
    private int _progressTotal;

    [ObservableProperty]
    private double _progressPercentage;

    [ObservableProperty]
    private List<string> _progressMessages = new();

    private IScanProgressReporter? _progressReporter;

    public ScanViewModel(
        IScanService scanService,
        IServiceProvider serviceProvider)
    {
        _scanService = scanService;
        _serviceProvider = serviceProvider;
    }

    [RelayCommand]
    private Task SelectDirectoryAsync()
    {
        // TODO: Implement directory picker dialog
        // For now, we'll use a simple approach
        StatusMessage = "Directory selection not yet implemented. Please enter path manually.";
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StartScanAsync()
    {
        if (string.IsNullOrWhiteSpace(SelectedDirectory) || !Directory.Exists(SelectedDirectory))
        {
            StatusMessage = "Please select a valid directory";
            return;
        }

        IsScanning = true;
        StatusMessage = "Scanning...";
        ProgressMessages.Clear();
        ProgressCurrent = 0;
        ProgressTotal = 0;
        ProgressPercentage = 0;

        try
        {
            _progressReporter = _serviceProvider.GetRequiredService<IScanProgressReporter>();
            if (_progressReporter is ScanProgressReporter reporter)
            {
                reporter.Reset();
            }

            var directories = new[] { SelectedDirectory };
            var romFiles = await _scanService.ScanDirectoriesAsync(
                directories,
                _progressReporter,
                CancellationToken.None);

            StatusMessage = $"Scan complete! Found {romFiles.Count()} ROM files.";
            
            // Navigate to results view
            var mainViewModel = App.Services?.GetService<MainWindowViewModel>();
            if (mainViewModel != null)
            {
                var resultsViewModel = App.Services?.GetService<ScanResultsViewModel>();
                if (resultsViewModel != null)
                {
                    resultsViewModel.SetRomFiles(romFiles.ToList());
                    mainViewModel.CurrentViewModel = resultsViewModel;
                }
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during scan: {ex.Message}";
            ProgressMessages.Add($"Error: {ex.Message}");
        }
        finally
        {
            IsScanning = false;
        }
    }

    [RelayCommand]
    private void CancelScan()
    {
        // TODO: Implement cancellation
        IsScanning = false;
        StatusMessage = "Scan cancelled";
    }

    partial void OnProgressCurrentChanged(int value)
    {
        UpdateProgressPercentage();
    }

    partial void OnProgressTotalChanged(int value)
    {
        UpdateProgressPercentage();
    }

    private void UpdateProgressPercentage()
    {
        if (ProgressTotal > 0)
        {
            ProgressPercentage = (double)ProgressCurrent / ProgressTotal * 100;
        }
        else
        {
            ProgressPercentage = 0;
        }
    }
}

