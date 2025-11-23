using Avalonia.Controls;
using RomPilot.UI.ViewModels;

namespace RomPilot.UI.Views;

public partial class ScanView : UserControl
{
    public ScanView()
    {
        InitializeComponent();
        this.Loaded += ScanView_Loaded;
    }

    private void ScanView_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Set parent window reference when view is loaded
        if (DataContext is ScanViewModel viewModel)
        {
            var window = TopLevel.GetTopLevel(this) as Window;
            if (window != null)
            {
                viewModel.SetParentWindow(window);
            }
        }
    }
}

