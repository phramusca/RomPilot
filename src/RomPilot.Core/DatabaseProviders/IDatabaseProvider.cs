using RomPilot.Core.Models;

namespace RomPilot.Core.DatabaseProviders;

/// <summary>
/// Base interface for database providers (NoIntro, Redump, GoodSet).
/// </summary>
public interface IDatabaseProvider
{
    /// <summary>
    /// Name of the database provider (e.g., "NoIntro", "Redump", "GoodSet").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Loads game entries from a datfile.
    /// </summary>
    /// <param name="datfilePath">Path to the datfile</param>
    /// <param name="databaseSourceId">ID of the database source</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of game entries parsed from the datfile</returns>
    Task<IEnumerable<GameEntry>> LoadGameEntriesAsync(
        string datfilePath,
        int databaseSourceId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that a file is a valid datfile for this provider.
    /// </summary>
    bool IsValidDatfile(string datfilePath);
}

