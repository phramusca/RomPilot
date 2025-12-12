using RomPilot.Core.Models;

namespace RomPilot.Core.Services;

/// <summary>
/// Interface for identifying games from ROM files using database checksums.
/// </summary>
public interface IGameIdentificationService
{
    /// <summary>
    /// Attempts to identify a game from a scanned file's checksums.
    /// </summary>
    /// <param name="scannedFile">The scanned file to identify</param>
    /// <param name="checksums">Dictionary of checksum types and values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Identified game entry or null if not found</returns>
    Task<GameEntry?> IdentifyGameAsync(
        ScannedFile scannedFile, 
        Dictionary<string, string> checksums, 
        CancellationToken cancellationToken = default);
}

