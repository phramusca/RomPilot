using CommunityToolkit.Mvvm.ComponentModel;
using RomPilot.Core.Models;
using RomPilot.Core.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RomPilot.UI.ViewModels;

/// <summary>
/// View model for the library view - displays all ROM files from the database.
/// </summary>
public partial class LibraryViewModel : ViewModelBase
{
    private readonly IRomFileRepository _romFileRepository;

    [ObservableProperty]
    private List<RomFile> _romFiles = new List<RomFile>();

    [ObservableProperty]
    private RomFile? _selectedRomFile;

    [ObservableProperty]
    private string _statusMessage = "Loading library...";

    public LibraryViewModel(IRomFileRepository romFileRepository)
    {
        _romFileRepository = romFileRepository;
        _ = LoadLibraryAsync();
    }

    private async Task LoadLibraryAsync()
    {
        try
        {
            StatusMessage = "Loading ROM files from database...";
            var romFiles = await _romFileRepository.GetAllAsync();
            var romFilesList = romFiles.ToList();
            RomFiles = romFilesList;
            StatusMessage = $"Loaded {RomFiles.Count} ROM files";
            System.Console.WriteLine($"[LibraryViewModel] Loaded {RomFiles.Count} ROM files from database");
            if (RomFiles.Count > 0)
            {
                var firstRom = RomFiles[0];
                System.Console.WriteLine($"[LibraryViewModel] Sample ROM: Console={(firstRom.Console?.Name ?? "null")}, Checksums={firstRom.Checksums?.Count ?? 0}");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading library: {ex.Message}";
            System.Console.WriteLine($"[LibraryViewModel] Error loading library: {ex.Message}");
        }
    }

    public void SetRomFiles(IEnumerable<RomFile> romFiles)
    {
        RomFiles = romFiles.ToList();
    }

    public void SetProcessingResults(IEnumerable<FileProcessingResult> processedFiles, IEnumerable<FileProcessingResult> failedFiles)
    {
        // Not used in library view
    }
}

