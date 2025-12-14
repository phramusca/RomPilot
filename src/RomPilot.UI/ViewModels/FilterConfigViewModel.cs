using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomPilot.Core.Models;
using RomPilot.Core.Services;

namespace RomPilot.UI.ViewModels;

/// <summary>
/// ViewModel pour la configuration des filtres d'exclusion.
/// </summary>
public partial class FilterConfigViewModel : ViewModelBase
{
    private readonly IFilterService _filterService;

    [ObservableProperty]
    private ObservableCollection<ExclusionFilterViewModel> _filters = new();

    [ObservableProperty]
    private string _statusMessage = "Chargement des filtres...";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _newFilterValue = string.Empty;

    public FilterConfigViewModel(IFilterService filterService)
    {
        _filterService = filterService;
        LoadFiltersCommand = new AsyncRelayCommand(LoadFiltersAsync);
        ToggleFilterCommand = new AsyncRelayCommand<ExclusionFilterViewModel>(ToggleFilterAsync);
        AddCustomFilterCommand = new AsyncRelayCommand(AddCustomFilterAsync);
        
        // Charger les filtres au démarrage
        Task.Run(LoadFiltersAsync);
    }

    public ICommand LoadFiltersCommand { get; }
    public ICommand ToggleFilterCommand { get; }
    public ICommand AddCustomFilterCommand { get; }

    private async Task AddCustomFilterAsync()
    {
        if (string.IsNullOrWhiteSpace(NewFilterValue))
        {
            StatusMessage = "Veuillez saisir une extension (ex: .mp3)";
            return;
        }

        try
        {
            // Always use "Extension" type
            await _filterService.AddCustomFilterAsync("Extension", NewFilterValue);
            StatusMessage = $"Filtre d'extension '{NewFilterValue}' ajouté";
            NewFilterValue = string.Empty;
            await LoadFiltersAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur : {ex.Message}";
        }
    }

    private async Task LoadFiltersAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Chargement des filtres d'exclusion...";

            var filters = await _filterService.GetAllFiltersAsync();
            var allFilters = filters.ToList();
            
            Filters.Clear();
            foreach (var filter in allFilters.OrderBy(f => f.FilterType).ThenBy(f => f.FilterValue))
            {
                var filterVm = new ExclusionFilterViewModel(filter);
                
                // Listen to IsActive changes to save to database
                filterVm.PropertyChanged += async (sender, e) =>
                {
                    if (e.PropertyName == nameof(ExclusionFilterViewModel.IsActive) && sender is ExclusionFilterViewModel vm)
                    {
                        System.Console.WriteLine($"[FilterConfigViewModel] IsActive changed for filter {vm.Id} to {vm.IsActive}");
                        try
                        {
                            await _filterService.SetFilterActiveAsync(vm.Id, vm.IsActive);
                            StatusMessage = vm.IsActive 
                                ? $"Filtre '{vm.FilterValue}' activé" 
                                : $"Filtre '{vm.FilterValue}' désactivé";
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine($"[FilterConfigViewModel] Error updating filter: {ex.Message}");
                            StatusMessage = $"Erreur : {ex.Message}";
                            // Revert the change in UI
                            vm.IsActive = !vm.IsActive;
                        }
                    }
                };
                
                Filters.Add(filterVm);
            }

            StatusMessage = $"{Filters.Count} filtres d'exclusion";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors du chargement des filtres : {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task ToggleFilterAsync(ExclusionFilterViewModel? filterViewModel)
    {
        if (filterViewModel == null) return;

        try
        {
            var newStatus = !filterViewModel.IsActive;
            await _filterService.SetFilterActiveAsync(filterViewModel.Id, newStatus);
            filterViewModel.IsActive = newStatus;
            
            StatusMessage = newStatus 
                ? $"Filtre '{filterViewModel.FilterValue}' activé" 
                : $"Filtre '{filterViewModel.FilterValue}' désactivé";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur : {ex.Message}";
        }
    }
}

/// <summary>
/// ViewModel pour un filtre d'exclusion individuel.
/// </summary>
public partial class ExclusionFilterViewModel : ObservableObject
{
    public int Id { get; }
    public string FilterType { get; }
    public string FilterValue { get; }
    public bool IsDefault { get; }
    public string? Description { get; }

    [ObservableProperty]
    private bool _isActive;

    public ExclusionFilterViewModel(ExclusionFilter filter)
    {
        Id = filter.Id;
        FilterType = filter.FilterType;
        FilterValue = filter.FilterValue;
        IsDefault = filter.IsDefault;
        IsActive = filter.IsActive;
        Description = filter.Description;
    }

    public string DisplayText => IsDefault 
        ? $"{FilterValue} (par défaut)" 
        : FilterValue;
}

