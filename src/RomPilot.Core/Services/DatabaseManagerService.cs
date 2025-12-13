using RomPilot.Core.Models;
using RomPilot.Core.Repositories;

namespace RomPilot.Core.Services;

/// <summary>
/// Service pour gérer les bases de données de référence (NoIntro, Redump, GoodSet).
/// Correspond à la User Story 2 : Gestionnaire de bases de données intégré.
/// </summary>
public class DatabaseManagerService : IDatabaseManagerService
{
    private readonly IReferenceDatabaseRepository _databaseRepository;
    private readonly IGameRepository _gameRepository;

    // TODO: Injecter les providers réels (NoIntro, Redump, GoodSet)
    // private readonly INoIntroProvider _noIntroProvider;
    // private readonly IRedumpProvider _redumpProvider;
    // private readonly IGoodSetProvider _goodSetProvider;

    public DatabaseManagerService(
        IReferenceDatabaseRepository databaseRepository,
        IGameRepository gameRepository)
    {
        _databaseRepository = databaseRepository;
        _gameRepository = gameRepository;
    }

    public async Task<IEnumerable<ReferenceDatabase>> GetAvailableDatabasesAsync(string provider)
    {
        return await _databaseRepository.GetByProviderAsync(provider);
    }

    public async Task<IEnumerable<ReferenceDatabase>> GetDatabasesForConsoleAsync(string console)
    {
        return await _databaseRepository.GetByConsoleAsync(console);
    }

    public async Task<ReferenceDatabase> DownloadDatabaseAsync(
        string provider,
        string console,
        string version,
        IProgress<int>? progressCallback = null)
    {
        // Vérifier si la base existe déjà
        var existing = await _databaseRepository.GetByProviderConsoleVersionAsync(provider, console, version);
        if (existing != null && existing.DownloadStatus == "Downloaded")
        {
            throw new InvalidOperationException($"La base de données {provider}/{console}/{version} est déjà téléchargée");
        }

        // Créer ou mettre à jour l'entrée
        var database = existing ?? new ReferenceDatabase
        {
            Provider = provider,
            Console = console,
            Version = version,
            ReleaseDate = DateTime.UtcNow.ToString("yyyy-MM-dd"), // TODO: Obtenir depuis le provider
        };

        database.DownloadStatus = "Downloading";

        if (existing == null)
        {
            database = await _databaseRepository.AddAsync(database);
        }
        else
        {
            await _databaseRepository.UpdateAsync(database);
        }

        try
        {
            // TODO: Implémenter le téléchargement réel via les providers
            // Pour l'instant, simulation
            progressCallback?.Report(0);

            // Simuler le téléchargement
            await Task.Delay(100); // Placeholder pour téléchargement réel
            progressCallback?.Report(50);

            // TODO: Sauvegarder le fichier localement
            var downloadPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "RomPilot",
                "Databases",
                provider,
                console,
                $"{version}.dat");

            // Créer le répertoire si nécessaire
            Directory.CreateDirectory(Path.GetDirectoryName(downloadPath)!);

            // TODO: Télécharger et sauvegarder le fichier réel
            // Pour l'instant, créer un fichier vide comme placeholder
            await File.WriteAllTextAsync(downloadPath, "<!-- Placeholder database file -->");

            progressCallback?.Report(100);

            // Mettre à jour le statut
            database.DownloadStatus = "Downloaded";
            database.FilePath = downloadPath;
            database.DownloadedAt = DateTime.UtcNow;
            database.FileSize = new FileInfo(downloadPath).Length;

            await _databaseRepository.UpdateAsync(database);

            return database;
        }
        catch (Exception ex)
        {
            database.DownloadStatus = "Error";
            database.ErrorMessage = ex.Message;
            await _databaseRepository.UpdateAsync(database);
            throw;
        }
    }

    public async Task<IEnumerable<(ReferenceDatabase current, ReferenceDatabase? update)>> CheckForUpdatesAsync()
    {
        var downloaded = await _databaseRepository.GetDownloadedAsync();
        var updates = new List<(ReferenceDatabase current, ReferenceDatabase? update)>();

        foreach (var db in downloaded)
        {
            // TODO: Interroger le provider pour vérifier les nouvelles versions
            // Pour l'instant, retourner null (pas de mise à jour)
            updates.Add((db, null));

            // Mettre à jour le timestamp de vérification
            db.LastCheckedAt = DateTime.UtcNow;
            await _databaseRepository.UpdateAsync(db);
        }

        return updates;
    }

    public async Task SetDefaultDatabaseAsync(int databaseId)
    {
        var database = await _databaseRepository.GetByIdAsync(databaseId);
        if (database == null)
        {
            throw new ArgumentException($"Base de données {databaseId} introuvable");
        }

        if (database.DownloadStatus != "Downloaded")
        {
            throw new InvalidOperationException("Seule une base téléchargée peut être définie par défaut");
        }

        // Désactiver le flag IsDefault pour toutes les autres bases de cette console
        var otherDbs = await _databaseRepository.GetByConsoleAsync(database.Console);
        foreach (var otherDb in otherDbs.Where(d => d.Id != databaseId && d.IsDefault))
        {
            otherDb.IsDefault = false;
            await _databaseRepository.UpdateAsync(otherDb);
        }

        // Activer le flag pour cette base
        database.IsDefault = true;
        await _databaseRepository.UpdateAsync(database);
    }

    public async Task<ReferenceDatabase?> GetDefaultDatabaseForConsoleAsync(string console)
    {
        return await _databaseRepository.GetDefaultForConsoleAsync(console);
    }

    public async Task DeleteDatabaseAsync(int databaseId)
    {
        var database = await _databaseRepository.GetByIdAsync(databaseId);
        if (database == null)
        {
            throw new ArgumentException($"Base de données {databaseId} introuvable");
        }

        // Supprimer le fichier local si présent
        if (!string.IsNullOrEmpty(database.FilePath) && File.Exists(database.FilePath))
        {
            File.Delete(database.FilePath);
        }

        // TODO: Supprimer les GameEntry associées ?
        // Pour l'instant, on garde les entrées (DeleteBehavior.Restrict)

        await _databaseRepository.DeleteAsync(databaseId);
    }

    public async Task<int> LoadDatabaseEntriesAsync(int databaseId)
    {
        var database = await _databaseRepository.GetByIdAsync(databaseId);
        if (database == null)
        {
            throw new ArgumentException($"Base de données {databaseId} introuvable");
        }

        if (database.DownloadStatus != "Downloaded" || string.IsNullOrEmpty(database.FilePath))
        {
            throw new InvalidOperationException("La base de données doit être téléchargée avant de charger ses entrées");
        }

        // TODO: Parser le fichier DAT et créer les GameEntry
        // Pour l'instant, retourner 0
        await Task.CompletedTask;
        return 0;
    }

    public async Task<DatabaseStatistics> GetDatabaseStatisticsAsync(int databaseId)
    {
        var database = await _databaseRepository.GetByIdAsync(databaseId);
        if (database == null)
        {
            throw new ArgumentException($"Base de données {databaseId} introuvable");
        }

        // TODO: Calculer les statistiques réelles depuis GameEntry
        // Pour l'instant, retourner des stats vides
        await Task.CompletedTask;

        return new DatabaseStatistics
        {
            TotalEntries = 0,
            UniqueGames = 0,
            EntriesByConsole = new Dictionary<string, int>(),
            LastLoaded = database.DownloadedAt
        };
    }
}

