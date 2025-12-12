using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using RomPilot.Core.Models;
using RomPilot.Core.Preferences;
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
    private readonly IUserPreferencesService? _preferencesService;

    [ObservableProperty]
    private string _selectedDirectory = string.Empty;

    [ObservableProperty]
    private List<ScannedFile> _scannedFiles = new();

    [ObservableProperty]
    private bool _isScanning;
    
    [ObservableProperty]
    private bool _hasResults;

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

    [ObservableProperty]
    private List<FileProcessingResult> _processedFiles = new();

    [ObservableProperty]
    private List<FileProcessingResult> _failedFiles = new();

    private IScanProgressReporter? _progressReporter;
    private Window? _parentWindow;

    public ScanViewModel(
        IScanService scanService,
        IServiceProvider serviceProvider)
    {
        _scanService = scanService;
        _serviceProvider = serviceProvider;
        
        // Try to get preferences service (may not be available in all contexts)
        try
        {
            _preferencesService = serviceProvider.GetService<IUserPreferencesService>();
        }
        catch
        {
            _preferencesService = null;
        }
        
        // Load last scanned directory
        _ = LoadLastScannedDirectoryAsync();
    }

    private async Task LoadLastScannedDirectoryAsync()
    {
        if (_preferencesService != null)
        {
            try
            {
                var lastDirectory = await _preferencesService.GetPreferenceAsync("last_scanned_directory");
                if (!string.IsNullOrEmpty(lastDirectory) && Directory.Exists(lastDirectory))
                {
                    SelectedDirectory = lastDirectory;
                    System.Console.WriteLine($"[ScanViewModel] Loaded last scanned directory: {lastDirectory}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"[ScanViewModel] Error loading last directory: {ex.Message}");
            }
        }
    }

    public void SetParentWindow(Window window)
    {
        _parentWindow = window;
    }

    [RelayCommand]
    private async Task SelectDirectoryAsync()
    {
        if (_parentWindow == null)
        {
            StatusMessage = "Parent window not available. Please enter path manually.";
            System.Console.WriteLine("[ScanViewModel] Parent window is null, cannot open folder picker");
            return;
        }

        try
        {
            System.Console.WriteLine("[ScanViewModel] Opening folder picker...");
            var folder = await _parentWindow.StorageProvider.OpenFolderPickerAsync(
                new Avalonia.Platform.Storage.FolderPickerOpenOptions
                {
                    Title = "Select Directory to Scan",
                    AllowMultiple = false
                });

            System.Console.WriteLine($"[ScanViewModel] Folder picker returned {folder.Count} folder(s)");

            if (folder.Count > 0)
            {
                var selectedFolder = folder[0];
                System.Console.WriteLine($"[ScanViewModel] Selected folder: {selectedFolder.Name}");
                
                // Get the local path from the storage folder
                // In Avalonia 11, we need to check if it's a file system path
                var path = selectedFolder.Path;
                System.Console.WriteLine($"[ScanViewModel] Folder path: {path}");
                
                if (path != null && path.IsAbsoluteUri && path.Scheme == "file")
                {
                    SelectedDirectory = path.LocalPath;
                    StatusMessage = $"Selected: {path.LocalPath}";
                    System.Console.WriteLine($"[ScanViewModel] Selected directory set to: {path.LocalPath}");
                }
                else
                {
                    // Fallback: try to get path from name or URI
                    var name = selectedFolder.Name;
                    System.Console.WriteLine($"[ScanViewModel] Could not extract file path, using name: {name}");
                    StatusMessage = $"Selected folder: {name} (path: {path}). Please enter path manually.";
                }
            }
            else
            {
                System.Console.WriteLine("[ScanViewModel] No folder selected by user");
                StatusMessage = "No folder selected. Please enter path manually or try again.";
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"[ScanViewModel] ERROR in SelectDirectoryAsync: {ex.Message}");
            System.Console.WriteLine($"[ScanViewModel] Stack trace: {ex.StackTrace}");
            StatusMessage = $"Error selecting directory: {ex.Message}. Please enter path manually.";
        }
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
        ProcessedFiles.Clear();
        FailedFiles.Clear();
        ScannedFiles.Clear(); // Clear previous results
        HasResults = false;
        ProgressCurrent = 0;
        ProgressTotal = 0;
        ProgressPercentage = 0;

        System.Console.WriteLine($"[ScanViewModel] Starting scan of: {SelectedDirectory}");
        System.Console.WriteLine($"[ScanViewModel] Directory exists: {Directory.Exists(SelectedDirectory)}");

        try
        {
            _progressReporter = _serviceProvider.GetRequiredService<IScanProgressReporter>();
            ScanProgressReporter? reporter = null;
            if (_progressReporter is ScanProgressReporter scanReporter)
            {
                reporter = scanReporter;
                reporter.Reset();
                System.Console.WriteLine("[ScanViewModel] Progress reporter initialized");
            }
            else
            {
                System.Console.WriteLine("[ScanViewModel] WARNING: Progress reporter is not ScanProgressReporter");
            }

            var directories = new[] { SelectedDirectory };
            System.Console.WriteLine($"[ScanViewModel] Calling ScanDirectoriesAsync with {directories.Length} directory(ies)");
            var startTime = DateTime.Now;
            
            // Start a task to periodically update progress from reporter
            var progressUpdateTask = Task.Run(async () =>
            {
                if (reporter == null) return;
                
                while (IsScanning)
                {
                    try
                    {
                        // Update progress from reporter on UI thread
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (reporter == null) return;
                            
                            var messages = reporter.Messages.ToList();
                            if (messages.Count != ProgressMessages.Count || 
                                reporter.Current != ProgressCurrent || 
                                reporter.Total != ProgressTotal)
                            {
                                ProgressMessages.Clear();
                                ProgressMessages.AddRange(messages);
                                ProgressCurrent = reporter.Current;
                                ProgressTotal = reporter.Total;
                                
                                // Parse messages to separate processed and failed files
                                ProcessedFiles.Clear();
                                FailedFiles.Clear();
                                foreach (var msg in messages)
                                {
                                    if (msg.StartsWith("✓"))
                                    {
                                        // Success message format: "✓ filename: status (details)"
                                        var parts = msg.Substring(1).Split(new[] { ':' }, 2);
                                        if (parts.Length == 2)
                                        {
                                            var fileName = parts[0].Trim();
                                            var statusParts = parts[1].Trim().Split(new[] { '(' }, 2);
                                            var status = statusParts[0].Trim();
                                            var details = statusParts.Length > 1 ? statusParts[1].TrimEnd(')') : null;
                                            ProcessedFiles.Add(new FileProcessingResult
                                            {
                                                FilePath = fileName,
                                                Status = status,
                                                Details = details
                                            });
                                        }
                                    }
                                    else if (msg.StartsWith("✗"))
                                    {
                                        // Failure message format: "✗ filename: FAILED - reason"
                                        var parts = msg.Substring(1).Split(new[] { ':' }, 2);
                                        if (parts.Length == 2)
                                        {
                                            var fileName = parts[0].Trim();
                                            var reason = parts[1].Replace("FAILED -", "").Trim();
                                            FailedFiles.Add(new FileProcessingResult
                                            {
                                                FilePath = fileName,
                                                Status = "failed",
                                                FailureReason = reason
                                            });
                                        }
                                    }
                                }
                                
                                // Update status message
                                if (!string.IsNullOrEmpty(reporter.CurrentMessage))
                                {
                                    StatusMessage = reporter.CurrentMessage;
                                }
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[ScanViewModel] Error updating progress: {ex.Message}");
                    }
                    
                    await Task.Delay(100); // Update every 100ms
                }
            });
            
            var romFiles = await _scanService.ScanDirectoriesAsync(
                directories,
                _progressReporter,
                CancellationToken.None);
            
            var duration = DateTime.Now - startTime;
            System.Console.WriteLine($"[ScanViewModel] Scan completed in {duration.TotalSeconds:F2} seconds");
            System.Console.WriteLine($"[ScanViewModel] Found {romFiles.Count()} ROM files");

            // Final update of progress messages from reporter on UI thread
            var romFilesList = romFiles.ToList();
            System.Console.WriteLine($"[ScanViewModel] romFiles.Count() = {romFilesList.Count}");
            
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (reporter != null)
                {
                    var messageCount = reporter.Messages.Count();
                    System.Console.WriteLine($"[ScanViewModel] Progress reporter has {messageCount} messages");
                    ProgressMessages.Clear();
                    ProgressMessages.AddRange(reporter.Messages);
                    ProgressCurrent = reporter.Current;
                    ProgressTotal = reporter.Total;
                    System.Console.WriteLine($"[ScanViewModel] Updated UI: {ProgressMessages.Count} messages, {ProgressCurrent}/{ProgressTotal} progress");
                }
                else
                {
                    System.Console.WriteLine("[ScanViewModel] WARNING: No progress reporter available");
                }

                StatusMessage = $"Scan complete! Found {romFilesList.Count} ROM files.";
                
                // Store scanned files in this view model (results shown in same view)
                ScannedFiles = romFilesList;
                HasResults = romFilesList.Count > 0;
                System.Console.WriteLine($"[ScanViewModel] Stored {romFilesList.Count} ROM files in view model, HasResults={HasResults}");
                
                // Save last scanned directory
                if (_preferencesService != null && !string.IsNullOrEmpty(SelectedDirectory))
                {
                    try
                    {
                        await _preferencesService.SetPreferenceAsync("last_scanned_directory", SelectedDirectory);
                        System.Console.WriteLine($"[ScanViewModel] Saved last scanned directory: {SelectedDirectory}");
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"[ScanViewModel] Error saving last directory: {ex.Message}");
                    }
                }
            });
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

/// <summary>
/// Represents the result of processing a file during scan.
/// </summary>
public class FileProcessingResult
{
    public string FilePath { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Details { get; set; }
    public string? FailureReason { get; set; }
}

