using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using RomPilot.Core.Models;
using RomPilot.Core.Repositories;

namespace RomPilot.UI.ViewModels;

/// <summary>
/// View model for the library view - displays all ROM files from the database.
/// </summary>
public partial class LibraryViewModel : ViewModelBase
{
    private readonly IScannedFileRepository _scannedFileRepository;

    [ObservableProperty]
    private List<ScannedFile> _scannedFiles = new List<ScannedFile>();

    [ObservableProperty]
    private ScannedFile? _selectedScannedFile;

    [ObservableProperty]
    private string _statusMessage = "Loading library...";

    public LibraryViewModel(IScannedFileRepository scannedFileRepository)
    {
        _scannedFileRepository = scannedFileRepository;
        _ = LoadLibraryAsync();
    }

    private async Task LoadLibraryAsync()
    {
        try
        {
            StatusMessage = "Loading scanned files from database...";
            var scannedFiles = await _scannedFileRepository.GetAllAsync();
            var scannedFilesList = scannedFiles.ToList();
            ScannedFiles = scannedFilesList;
            StatusMessage = $"Loaded {ScannedFiles.Count} scanned files";
            System.Console.WriteLine($"[LibraryViewModel] Loaded {ScannedFiles.Count} scanned files from database");
            if (ScannedFiles.Count > 0)
            {
                var firstFile = ScannedFiles[0];
                System.Console.WriteLine($"[LibraryViewModel] Sample file: Console={(firstFile.Console?.Name ?? "null")}, Checksums={firstFile.Checksums?.Count ?? 0}");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading library: {ex.Message}";
            System.Console.WriteLine($"[LibraryViewModel] Error loading library: {ex.Message}");
        }
    }

    public void SetScannedFiles(IEnumerable<ScannedFile> scannedFiles)
    {
        ScannedFiles = scannedFiles.ToList();
    }

    public void SetProcessingResults(IEnumerable<FileProcessingResult> processedFiles, IEnumerable<FileProcessingResult> failedFiles)
    {
        // Not used in library view
    }
}

