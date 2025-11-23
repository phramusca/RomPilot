using RomPilot.Core.Models;

namespace RomPilot.Core.Services;

/// <summary>
/// Interface for identifying games from ROM files using database checksums.
/// </summary>
public interface IGameIdentificationService
{
    /// <summary>
    /// Attempts to identify a game from a ROM file's checksums.
    /// </summary>
    /// <param name="romFile">The ROM file to identify</param>
    /// <param name="checksums">Dictionary of checksum types and values</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Identified game entry or null if not found</returns>
    Task<GameEntry?> IdentifyGameAsync(
        RomFile romFile, 
        Dictionary<string, string> checksums, 
        CancellationToken cancellationToken = default);
}

