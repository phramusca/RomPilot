using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RomPilot.Core.Models;
using RomPilot.Core.Services;

namespace RomPilot.UI.ViewModels;

/// <summary>
/// View model pour la gestion des bases de données de référence (NoIntro, Redump, GoodSet).
/// </summary>
public partial class DatabaseManagerViewModel : ViewModelBase
{
    private readonly IDatabaseManagerService _databaseManagerService;

    [ObservableProperty]
    private ObservableCollection<string> _providers = new();

    [ObservableProperty]
    private string _selectedProvider = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _consoles = new();

    [ObservableProperty]
    private string _selectedConsole = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ReferenceDatabase> _availableVersions = new();

    [ObservableProperty]
    private ReferenceDatabase? _selectedVersion;

    [ObservableProperty]
    private ObservableCollection<ReferenceDatabase> _downloadedDatabases = new();

    [ObservableProperty]
    private string _statusMessage = "Prêt";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isDownloading;

    [ObservableProperty]
    private int _downloadProgress;

    [ObservableProperty]
    private string _downloadingDatabaseName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ReferenceDatabase> _availableUpdates = new();

    [ObservableProperty]
    private ReferenceDatabase? _selectedDownloadedDatabase;

    public DatabaseManagerViewModel(IDatabaseManagerService databaseManagerService)
    {
        _databaseManagerService = databaseManagerService;
        _ = LoadProvidersAsync();
        _ = LoadDownloadedDatabasesAsync();
    }

    private async Task LoadProvidersAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Chargement des providers...";
            var providers = await _databaseManagerService.GetAvailableProvidersAsync();
            Providers.Clear();
            foreach (var provider in providers)
            {
                Providers.Add(provider);
            }
            if (Providers.Count > 0)
            {
                SelectedProvider = Providers[0];
            }
            StatusMessage = "Providers chargés";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadConsolesForProviderAsync()
    {
        if (string.IsNullOrEmpty(SelectedProvider))
            return;

        try
        {
            IsLoading = true;
            StatusMessage = $"Chargement des consoles pour {SelectedProvider}...";
            
            // Charger les bases téléchargées pour ce provider pour obtenir la liste des consoles
            var databases = await _databaseManagerService.GetAvailableDatabasesAsync(SelectedProvider);
            var consoleList = databases.Select(d => d.Console).Distinct().OrderBy(c => c).ToList();
            
            Consoles.Clear();
            foreach (var console in consoleList)
            {
                Consoles.Add(console);
            }

            if (Consoles.Count > 0)
            {
                SelectedConsole = Consoles[0];
                await LoadVersionsForConsoleAsync();
            }
            else
            {
                AvailableVersions.Clear();
                StatusMessage = $"Aucune console trouvée pour {SelectedProvider}";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task LoadVersionsForConsoleAsync()
    {
        if (string.IsNullOrEmpty(SelectedProvider) || string.IsNullOrEmpty(SelectedConsole))
            return;

        try
        {
            IsLoading = true;
            StatusMessage = $"Chargement des versions pour {SelectedProvider}/{SelectedConsole}...";
            var versions = await _databaseManagerService.GetAvailableVersionsAsync(SelectedProvider, SelectedConsole);
            AvailableVersions.Clear();
            foreach (var version in versions)
            {
                AvailableVersions.Add(version);
            }
            StatusMessage = $"{AvailableVersions.Count} version(s) disponible(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DownloadSelectedVersionAsync()
    {
        if (SelectedVersion == null)
            return;

        try
        {
            IsDownloading = true;
            DownloadingDatabaseName = $"{SelectedVersion.Provider}/{SelectedVersion.Console}/{SelectedVersion.Version}";
            StatusMessage = $"Téléchargement de {DownloadingDatabaseName}...";
            DownloadProgress = 0;

            var progress = new Progress<int>(p =>
            {
                Dispatcher.UIThread.Post(() => DownloadProgress = p);
            });

            var database = await _databaseManagerService.DownloadDatabaseAsync(
                SelectedVersion.Provider,
                SelectedVersion.Console,
                SelectedVersion.Version,
                progress);

            StatusMessage = $"Téléchargement terminé: {DownloadingDatabaseName}";
            DownloadProgress = 100;

            // Recharger les bases téléchargées
            await LoadDownloadedDatabasesAsync();
        }
        catch (NotImplementedException)
        {
            StatusMessage = "⚠️ Le téléchargement n'est pas encore implémenté (URLs des providers à configurer)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur lors du téléchargement: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
            DownloadProgress = 0;
            DownloadingDatabaseName = string.Empty;
        }
    }

    private async Task LoadDownloadedDatabasesAsync()
    {
        try
        {
            IsLoading = true;
            var allProviders = await _databaseManagerService.GetAvailableProvidersAsync();
            var allDownloaded = new List<ReferenceDatabase>();

            foreach (var provider in allProviders)
            {
                var databases = await _databaseManagerService.GetAvailableDatabasesAsync(provider);
                var downloaded = databases.Where(d => d.DownloadStatus == "Downloaded");
                allDownloaded.AddRange(downloaded);
            }

            DownloadedDatabases.Clear();
            foreach (var db in allDownloaded.OrderBy(d => d.Provider).ThenBy(d => d.Console).ThenByDescending(d => d.Version))
            {
                DownloadedDatabases.Add(db);
            }

            StatusMessage = $"{DownloadedDatabases.Count} base(s) téléchargée(s)";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SetAsDefaultAsync(ReferenceDatabase? database = null)
    {
        database ??= SelectedDownloadedDatabase;
        if (database == null || database.Id == 0)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = $"Définition de {database.Provider}/{database.Console}/{database.Version} comme version par défaut...";
            await _databaseManagerService.SetDefaultDatabaseAsync(database.Id);
            StatusMessage = "Version par défaut mise à jour";
            await LoadDownloadedDatabasesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteDatabaseAsync(ReferenceDatabase? database = null)
    {
        database ??= SelectedDownloadedDatabase;
        if (database == null || database.Id == 0)
            return;

        try
        {
            IsLoading = true;
            StatusMessage = $"Suppression de {database.Provider}/{database.Console}/{database.Version}...";
            await _databaseManagerService.DeleteDatabaseAsync(database.Id);
            StatusMessage = "Base de données supprimée";
            await LoadDownloadedDatabasesAsync();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task CheckForUpdatesAsync()
    {
        try
        {
            IsLoading = true;
            StatusMessage = "Vérification des mises à jour...";
            var updates = await _databaseManagerService.CheckForUpdatesAsync();
            AvailableUpdates.Clear();
            foreach (var (current, update) in updates)
            {
                if (update != null)
                {
                    AvailableUpdates.Add(update);
                }
            }

            if (AvailableUpdates.Count > 0)
            {
                StatusMessage = $"⚠️ {AvailableUpdates.Count} mise(s) à jour disponible(s)";
            }
            else
            {
                StatusMessage = "Toutes les bases sont à jour";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Erreur: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task RefreshDownloadedDatabasesAsync()
    {
        await LoadDownloadedDatabasesAsync();
    }

    partial void OnSelectedProviderChanged(string value)
    {
        _ = LoadConsolesForProviderAsync();
    }

    partial void OnSelectedConsoleChanged(string value)
    {
        _ = LoadVersionsForConsoleAsync();
    }
}

