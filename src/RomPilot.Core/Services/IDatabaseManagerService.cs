using RomPilot.Core.Models;

namespace RomPilot.Core.Services;

/// <summary>
/// Service pour gérer les bases de données de référence (NoIntro, Redump, GoodSet).
/// Correspond à la User Story 2 : Gestionnaire de bases de données intégré.
/// </summary>
public interface IDatabaseManagerService
{
    /// <summary>
    /// Liste toutes les bases de données disponibles pour un provider donné.
    /// </summary>
    Task<IEnumerable<ReferenceDatabase>> GetAvailableDatabasesAsync(string provider);
    
    /// <summary>
    /// Liste toutes les bases de données pour une console donnée.
    /// </summary>
    Task<IEnumerable<ReferenceDatabase>> GetDatabasesForConsoleAsync(string console);
    
    /// <summary>
    /// Télécharge une base de données spécifique.
    /// </summary>
    /// <param name="provider">Provider (NoIntro, Redump, GoodSet)</param>
    /// <param name="console">Console/plateforme</param>
    /// <param name="version">Version du datfile</param>
    /// <param name="progressCallback">Callback optionnel pour progression (pourcentage 0-100)</param>
    Task<ReferenceDatabase> DownloadDatabaseAsync(
        string provider, 
        string console, 
        string version,
        IProgress<int>? progressCallback = null);
    
    /// <summary>
    /// Vérifie les mises à jour disponibles pour les bases téléchargées.
    /// </summary>
    Task<IEnumerable<(ReferenceDatabase current, ReferenceDatabase? update)>> CheckForUpdatesAsync();
    
    /// <summary>
    /// Définit une base de données comme version par défaut pour une console.
    /// </summary>
    Task SetDefaultDatabaseAsync(int databaseId);
    
    /// <summary>
    /// Obtient la base de données par défaut pour une console.
    /// </summary>
    Task<ReferenceDatabase?> GetDefaultDatabaseForConsoleAsync(string console);
    
    /// <summary>
    /// Supprime une base de données téléchargée.
    /// </summary>
    Task DeleteDatabaseAsync(int databaseId);
    
    /// <summary>
    /// Charge les entrées d'une base de données dans la table GameEntries.
    /// </summary>
    Task<int> LoadDatabaseEntriesAsync(int databaseId);
    
    /// <summary>
    /// Obtient les statistiques d'une base de données (nombre d'entrées, consoles, etc.).
    /// </summary>
    Task<DatabaseStatistics> GetDatabaseStatisticsAsync(int databaseId);
}

/// <summary>
/// Statistiques d'une base de données de référence.
/// </summary>
public class DatabaseStatistics
{
    public int TotalEntries { get; set; }
    public int UniqueGames { get; set; }
    public Dictionary<string, int> EntriesByConsole { get; set; } = new();
    public DateTime? LastLoaded { get; set; }
}

