using CommunityToolkit.Mvvm.ComponentModel;
using RomPilot.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace RomPilot.UI.ViewModels;

/// <summary>
/// View model for displaying scan results.
/// </summary>
public partial class ScanResultsViewModel : ViewModelBase
{
    [ObservableProperty]
    private List<RomFile> _romFiles = new List<RomFile>();

    [ObservableProperty]
    private RomFile? _selectedRomFile;

    public void SetRomFiles(IEnumerable<RomFile> romFiles)
    {
        RomFiles = romFiles.ToList();
    }
}

