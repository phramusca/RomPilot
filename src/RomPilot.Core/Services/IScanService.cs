using RomPilot.Core.Models;

namespace RomPilot.Core.Services;

/// <summary>
/// Interface for scanning directories and identifying ROM files.
/// </summary>
public interface IScanService
{
    /// <summary>
    /// Scans one or more directories for ROM files, calculates checksums, and identifies games.
    /// </summary>
    /// <param name="directoryPaths">Paths to directories to scan</param>
    /// <param name="progressReporter">Optional progress reporter for UI updates</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of identified ROM files</returns>
    Task<IEnumerable<RomFile>> ScanDirectoriesAsync(
        IEnumerable<string> directoryPaths,
        IScanProgressReporter? progressReporter = null,
        CancellationToken cancellationToken = default);
}

