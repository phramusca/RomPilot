using CommunityToolkit.Mvvm.ComponentModel;
using RomPilot.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace RomPilot.UI.ViewModels;

/// <summary>
/// View model for the library view (future functionality).
/// </summary>
public partial class LibraryViewModel : ViewModelBase
{
    [ObservableProperty]
    private List<RomFile> _romFiles = new List<RomFile>();

    [ObservableProperty]
    private RomFile? _selectedRomFile;

    [ObservableProperty]
    private List<FileProcessingResult> _processedFiles = new();

    [ObservableProperty]
    private List<FileProcessingResult> _failedFiles = new();

    public void SetRomFiles(IEnumerable<RomFile> romFiles)
    {
        RomFiles = romFiles.ToList();
    }

    public void SetProcessingResults(IEnumerable<FileProcessingResult> processedFiles, IEnumerable<FileProcessingResult> failedFiles)
    {
        ProcessedFiles = processedFiles.ToList();
        FailedFiles = failedFiles.ToList();
    }
}

