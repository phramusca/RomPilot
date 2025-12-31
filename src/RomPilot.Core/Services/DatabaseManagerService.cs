using System.Net.Http;
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
    private readonly IHttpClientFactory _httpClientFactory;
    private const int MaxRetryAttempts = 3;
    private const int RetryDelayMs = 1000;

    // TODO: Injecter les providers réels (NoIntro, Redump, GoodSet)
    // private readonly INoIntroProvider _noIntroProvider;
    // private readonly IRedumpProvider _redumpProvider;
    // private readonly IGoodSetProvider _goodSetProvider;

    public DatabaseManagerService(
        IReferenceDatabaseRepository databaseRepository,
        IGameRepository gameRepository,
        IHttpClientFactory httpClientFactory)
    {
        _databaseRepository = databaseRepository;
        _gameRepository = gameRepository;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IEnumerable<string>> GetAvailableProvidersAsync()
    {
        // T062: Retourner les providers disponibles (NoIntro, Redump, GoodSet)
        await Task.CompletedTask;
        return new[] { "NoIntro", "Redump", "GoodSet" };
    }

    public async Task<IEnumerable<ReferenceDatabase>> GetAvailableVersionsAsync(string provider, string console)
    {
        // T063: Lister les versions disponibles pour un provider et une console
        // TODO: Implémenter la récupération réelle depuis les APIs des providers
        // Pour l'instant, retourner une liste vide (les tests vérifieront la structure)
        await Task.CompletedTask;
        return Enumerable.Empty<ReferenceDatabase>();
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

        // T064: Téléchargement réel avec HttpClient
        // T065: Progress reporting
        // T068: Retry logic pour erreurs réseau
        var downloadPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RomPilot",
            "Databases",
            provider,
            console,
            $"{version}.dat");

        // Créer le répertoire si nécessaire
        Directory.CreateDirectory(Path.GetDirectoryName(downloadPath)!);

        try
        {
            progressCallback?.Report(0);

            // T064: Télécharger le fichier avec HttpClient
            // TODO: Implémenter les URLs réelles des providers (NoIntro, Redump, GoodSet)
            // Pour l'instant, utiliser une URL placeholder qui sera remplacée par les vraies URLs
            var downloadUrl = GetDownloadUrl(provider, console, version);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromMinutes(10); // Timeout long pour gros fichiers

            // T068: Retry logic avec exponential backoff
            Exception? lastException = null;
            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    // T064: Télécharger avec HttpClient et progress reporting
                    using var response = await httpClient.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes > 0 && progressCallback != null;

                    using var contentStream = await response.Content.ReadAsStreamAsync();
                    using var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                    var buffer = new byte[8192];
                    long totalBytesRead = 0;
                    int bytesRead;

                    while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;

                        // T065: Progress reporting
                        if (canReportProgress)
                        {
                            var progress = (int)((double)totalBytesRead / totalBytes * 100);
                            progressCallback?.Report(progress);
                        }
                    }

                    // Si pas de Content-Length, reporter 100% à la fin
                    if (!canReportProgress)
                    {
                        progressCallback?.Report(100);
                    }

                    break; // Succès, sortir de la boucle de retry
                }
                catch (HttpRequestException ex) when (attempt < MaxRetryAttempts)
                {
                    lastException = ex;
                    // Exponential backoff: attendre avant de réessayer
                    var delay = RetryDelayMs * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delay);
                    continue;
                }
                catch (TaskCanceledException ex) when (attempt < MaxRetryAttempts && ex.InnerException is TimeoutException)
                {
                    lastException = ex;
                    // Timeout, réessayer avec exponential backoff
                    var delay = RetryDelayMs * (int)Math.Pow(2, attempt - 1);
                    await Task.Delay(delay);
                    continue;
                }
            }

            if (lastException != null && !File.Exists(downloadPath))
            {
                throw lastException;
            }

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

            // Nettoyer le fichier partiellement téléchargé
            if (File.Exists(downloadPath))
            {
                try
                {
                    File.Delete(downloadPath);
                }
                catch
                {
                    // Ignorer erreurs de suppression
                }
            }

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

    /// <summary>
    /// T064: Construit l'URL de téléchargement pour un provider, console et version donnés.
    /// TODO: Implémenter les URLs réelles des providers (NoIntro, Redump, GoodSet).
    /// Pour l'instant, retourne une URL placeholder qui sera remplacée par les vraies URLs.
    /// </summary>
    private string GetDownloadUrl(string provider, string console, string version)
    {
        // TODO: Implémenter les URLs réelles selon le provider
        // Exemples de structure attendue:
        // - NoIntro: https://www.no-intro.org/datfile/...
        // - Redump: https://redump.org/datfile/...
        // - GoodSet: https://goodset.datfiles.com/...

        // Pour l'instant, utiliser une URL qui échouera de manière contrôlée
        // afin que les tests puissent valider la structure sans nécessiter de connexion réelle
        throw new NotImplementedException(
            $"Le téléchargement depuis {provider} n'est pas encore implémenté. " +
            $"Les URLs des datfiles doivent être configurées pour {provider}/{console}/{version}");
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

